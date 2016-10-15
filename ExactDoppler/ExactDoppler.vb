Imports ExactAudio.MotionExplorer

''' <summary>
''' Доплеровский акустический детектор
''' </summary>
Public Class ExactDoppler
    'Константы
    Private Const _windowSize = 32768 '32768
    Private Const _windowStep = 809 '809 = Round(32768 / (3 * 3 * 3 * 1.5))
    Private Const _sampleRate = 48000 '48000
    Private Const _dopplerSize = 500 '500
    Private Const _nBitsCapture = 16 '16
    Private Const _nBitsPalette = 8 '8
    Private Const _waterfallSeconds = _windowSize / _sampleRate '0.6827
    Private Const _topFreq = 23000 '23000

    'Данные
    Private _inputDeviceIdx As Integer = -1
    Private _outputDeviceIdx As Integer = -1
    Private _dopplerLog As New DopplerLog()

    'Объекты
    Private _capture As WaveInSource
    Private _sineGenerator As SineGenerator
    Private _motionExplorer As MotionExplorer

    Public ReadOnly SyncRoot As New Object

    ''' <summary>Список аудиоустройств вывода.</summary>
    Public ReadOnly Property OutputAudioDevices As String()
        Get
            Return AudioUtils.GetWaveOutNames()
        End Get
    End Property

    ''' <summary>Список аудиоустройств ввода.</summary>
    Public ReadOnly Property InputAudioDevices As String()
        Get
            Return AudioUtils.GetWaveInNames()
        End Get
    End Property

    ''' <summary>Частота семлирования.</summary>
    Public ReadOnly Property SampleRate As Integer
        Get
            Return _sampleRate
        End Get
    End Property

    ''' <summary>Верхняя частота области интереса.</summary>
    Public ReadOnly Property TopFreq As Integer
        Get
            Return _topFreq
        End Get
    End Property

    ''' <summary>Размер доплеровской области интереса.</summary>
    Public ReadOnly Property DopplerSize
        Get
            Return _dopplerSize
        End Get
    End Property

    ''' <summary>Индекс устройства захвата аудио.</summary>
    Public Property InputDeviceIdx As Integer
        Get
            SyncLock SyncRoot
                Return _inputDeviceIdx
            End SyncLock
        End Get
        Set(value As Integer)
            SyncLock SyncRoot
                If value >= 0 AndAlso value < Me.InputAudioDevices.Count Then
                    _inputDeviceIdx = value
                    _capture = New WaveInSource(_inputDeviceIdx, _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallSeconds) With {.SampleProcessor = AddressOf SampleProcessor}
                Else
                    _inputDeviceIdx = -1
                End If
            End SyncLock
        End Set
    End Property

    ''' <summary>Индекс устройства вывода аудио.</summary>
    Public Property OutputDeviceIdx As Integer
        Get
            SyncLock SyncRoot
                Return _outputDeviceIdx
            End SyncLock
        End Get
        Set(value As Integer)
            SyncLock SyncRoot
                If value >= 0 AndAlso value < Me.OutputAudioDevices.Count Then
                    _outputDeviceIdx = value
                    _sineGenerator = New SineGenerator(_outputDeviceIdx, _sampleRate)
                Else
                    _outputDeviceIdx = -1
                End If
            End SyncLock
        End Set
    End Property

    ''' <summary>Доплеровский лог.</summary>
    Public ReadOnly Property DopplerLog As DopplerLog
        Get
            Return _dopplerLog
        End Get
    End Property

    ''' <summary>Громкость.</summary>
    Public Property Volume As Single
        Get
            SyncLock SyncRoot
                Return _sineGenerator.Volume
            End SyncLock
        End Get
        Set(value As Single)
            SyncLock SyncRoot
                _sineGenerator.Volume = value
            End SyncLock
        End Set
    End Property

    ''' <summary>Конфигурация доплеровского анализатора.</summary>
    Public Property Config As ExactDopplerConfig
        Get
            SyncLock SyncRoot
                Return _config
            End SyncLock
        End Get
        Set(value As ExactDopplerConfig)
            SyncLock SyncRoot
                _config = value
            End SyncLock
        End Set
    End Property
    Private _config As ExactDopplerConfig

    ''' <summary>
    ''' Событие "Pcm-семплы обработаны"
    ''' </summary>
    ''' <param name="motionExplorerResult">"Результат анализа движения".</param>
    Public Event PcmSamplesProcessed(motionExplorerResult As MotionExplorerResult)

    Public Sub New()
        Me.New(New ExactDopplerConfig())
    End Sub

    ''' <summary>
    ''' Конструктор
    ''' </summary>
    ''' <param name="config">Конфигурация доплеровского анализатора.</param>
    Public Sub New(config As ExactDopplerConfig)
        If config IsNot Nothing Then
            _config = config
        End If
        _sineGenerator = New SineGenerator(_outputDeviceIdx, _sampleRate)
        _capture = New WaveInSource(_inputDeviceIdx, _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallSeconds)
        _motionExplorer = New MotionExplorer(_windowSize, _windowStep, _sampleRate, _nBitsPalette, False)
        InputDeviceIdx = _config.InputDeviceIdx
        OutputDeviceIdx = _config.OutputDeviceIdx
        Volume = _config.Volume
    End Sub

    ''' <summary>
    ''' Основной метод обработки
    ''' </summary>
    ''' <param name="pcmSamples">Pcm-семплы.</param>
    ''' <param name="pcmSamplesCount">Количество семплов (для учета режима моно/стерео).</param>
    ''' <returns>"Результат анализа движения".</returns>
    Public Function Process(pcmSamples As Single(), pcmSamplesCount As Integer) As MotionExplorerResult
        SyncLock SyncRoot
            'Параметры
            Dim lowFreq As Double = 0
            Dim highFreq As Double = 0
            Dim play As Boolean = False

            If _config.CenterFreq = 0 Then
                lowFreq = 0
                highFreq = _topFreq
            Else
                lowFreq = _config.CenterFreq - _dopplerSize
                highFreq = _config.CenterFreq + _dopplerSize
                lowFreq = If(lowFreq < 0, 0, lowFreq)
                highFreq = If(highFreq > _topFreq, _topFreq, highFreq)
            End If

            'Обработка
            Dim motionExplorerResult = _motionExplorer.Process(pcmSamples, pcmSamplesCount, lowFreq, highFreq, _config.BlindZone)

            'Доплер-лог
            Static Dim lowDopplerAvgSum As Single
            Static Dim highDopplerAvgSum As Single
            Dim nowTimeStamp = DateTime.Now

            With motionExplorerResult
                lowDopplerAvgSum += .LowDoppler.Average() 'Накопление по нижней полосе
                highDopplerAvgSum += .HighDoppler.Average() 'Накопление по верхней полосе

                Dim lowDopplerAvg As Single = 0
                Dim highDopplerAvg As Single = 0

                'Если в конце фрагмента сонограммы нет энергии - всплеск "закрыт"
                If .LowDoppler.Last.Value = 0 Then
                    lowDopplerAvg = lowDopplerAvgSum
                    lowDopplerAvgSum = 0
                End If

                'Если в конце фрагмента сонограммы нет энергии - всплеск "закрыт"
                If .HighDoppler.Last.Value = 0 Then
                    highDopplerAvg = highDopplerAvgSum
                    highDopplerAvgSum = 0
                End If

                'Несущая не должна иметь слишком низкий уровень
                Dim carrierIsOK = .CarrierLevel.Min() >= _config.CarrierWarningLevel

                'Элемент лога
                Dim logItem = New DopplerLogItem(nowTimeStamp, lowDopplerAvg, highDopplerAvg, carrierIsOK)
                motionExplorerResult.DopplerLogItem = logItem
                If lowDopplerAvg <> 0 OrElse highDopplerAvg <> 0 OrElse Not carrierIsOK Then 'Если несущей нет - это тоже событие!
                    motionExplorerResult.IsWarning = True
                    _dopplerLog.Add(logItem) 'Пишем в лог - если есть данные!
                End If
            End With

            'Проверка на нормализацию L/H
            If motionExplorerResult.DopplerLogItem.LowDoppler > 99.99 Then
                Throw New Exception("ExactDoppler: motionExplorerResult.DopplerLogItem.LowDoppler > 99.99")
            End If
            If motionExplorerResult.DopplerLogItem.HighDoppler > 99.99 Then
                Throw New Exception("ExactDoppler: motionExplorerResult.DopplerLogItem.HighDoppler > 99.99")
            End If

            Return motionExplorerResult
        End SyncLock
    End Function

    ''' <summary>
    ''' Включение генератора
    ''' </summary>
    Public Sub SwitchOnGen()
        SwitchOnGen(_config.CenterFreq)
    End Sub

    ''' <summary>
    ''' Включение генератора
    ''' </summary>
    ''' <param name="frequency">Частота синуса.</param>
    Public Sub SwitchOnGen(frequency As Single)
        SyncLock SyncRoot
            _sineGenerator.SwitchOn({frequency})
        End SyncLock
    End Sub

    ''' <summary>
    ''' Выключение генератора
    ''' </summary>
    Public Sub SwitchOffGen()
        SyncLock SyncRoot
            _sineGenerator.SwitchOff()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Запуск
    ''' </summary>
    Public Sub Start()
        SyncLock SyncRoot
            With _capture
                .SampleProcessor = AddressOf SampleProcessor
                .Start()
            End With
        End SyncLock
    End Sub

    ''' <summary>
    ''' Останов
    ''' </summary>
    Public Sub [Stop]()
        SyncLock SyncRoot
            _capture.Stop()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Обработчик Pcm-семплов
    ''' </summary>
    ''' <param name="pcmSamples">Pcm-семплы.</param>
    ''' <param name="pcmSamplesCount">Количество семплов (для учета режима моно/стерео).</param>
    Private Sub SampleProcessor(pcmSamples As Single(), pcmSamplesCount As Integer)
        Dim motionExplorerResult = Process(pcmSamples, pcmSamplesCount)
        RaiseEvent PcmSamplesProcessed(motionExplorerResult)
    End Sub
End Class
