Imports ExactAudio.MotionExplorer

''' <summary>
''' Доплеровский акустический детектор
''' </summary>
Public Class ExactDoppler
    'Константы
    Private Const _windowSize = 32768 '32768
    Private Const _windowStep = 1214 '1214 = Round(32768 / (3 * 3 * 3))
    Private Const _sampleRate = 48000 '48000
    Private Const _dopplerSize = 500 '500
    Private Const _nBitsCapture = 16 '16
    Private Const _nBitsPalette = 8 '8
    Private Const _waterfallSeconds = _windowSize / _sampleRate '0.6827
    Private Const _topFreq = 23000 '23000

    'Данные
    Private _inputDeviceIdx As Integer = 0
    Private _outputDeviceIdx As Integer = 0
    Private _dopplerLog As New DopplerLog()

    'Объекты
    Private _capture As WaveInSource
    Private _generator As Generator
    Private _motionExplorer As MotionExplorer

    Public ReadOnly SyncRoot As New Object

    ''' <summary>Список аудиоустройств вывода.</summary>
    Public ReadOnly Property OutputAudioDevices As String()
        Get
            Return AudioUtils.GetAudioDeviceNamesWaveOut()
        End Get
    End Property

    ''' <summary>Список аудиоустройств ввода.</summary>
    Public ReadOnly Property InputAudioDevices As String()
        Get
            Return AudioUtils.GetAudioDeviceNamesWaveIn()
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
            Return _inputDeviceIdx
        End Get
        Set(value As Integer)
            SyncLock SyncRoot
                _inputDeviceIdx = value
                _capture = New WaveInSource(_inputDeviceIdx, _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallSeconds) With {.SampleProcessor = AddressOf SampleProcessor}
            End SyncLock
        End Set
    End Property

    ''' <summary>Индекс устройства вывода аудио.</summary>
    Public Property OutputDeviceIdx As Integer
        Get
            Return _outputDeviceIdx
        End Get
        Set(value As Integer)
            SyncLock SyncRoot
                _outputDeviceIdx = value
                _generator = New Generator(_outputDeviceIdx, _sampleRate)
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
            Return _generator.Volume
        End Get
        Set(value As Single)
            SyncLock SyncRoot
                _generator.Volume = value
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
            _config = value
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
        _generator = New Generator(_outputDeviceIdx, _sampleRate)
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
            Dim motionExplorerResult = _motionExplorer.Process(pcmSamples, pcmSamplesCount, lowFreq, highFreq, _config.BlindZone, _config.DisplayLeft,
                                                               _config.DisplayRightWithLeft, _config.DisplayCenter, _config.DisplayRight,
                                                               _config.PcmOutput, _config.ImageOutput)

            'Допплер-лог
            Dim nowTimeStamp = DateTime.Now
            Dim lowDopplerSum = motionExplorerResult.LowDoppler.Sum()
            Dim highDopplerSum = motionExplorerResult.HighDoppler.Sum()
            If lowDopplerSum <> 0 Or highDopplerSum <> 0 Then
                _dopplerLog.Add(nowTimeStamp, lowDopplerSum, highDopplerSum)
            End If

            Return motionExplorerResult
        End SyncLock
    End Function

    ''' <summary>
    ''' Включение генератора
    ''' </summary>
    Public Sub SwitchOnGen()
        SwitchOnGen(Config.CenterFreq, Config.CenterFreq, False)
    End Sub

    ''' <summary>
    ''' Включение генератора
    ''' </summary>
    ''' <param name="sineFreqL">Частота левого канала.</param>
    ''' <param name="sineFreqR">Частота правого канала.</param>
    ''' <param name="mix">Смешивать каналы?</param>
    Public Sub SwitchOnGen(sineFreqL As Integer, sineFreqR As Integer, mix As Boolean)
        SyncLock SyncRoot
            _generator.SwitchOn(sineFreqL, sineFreqR, mix)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Выключение генератора
    ''' </summary>
    Public Sub SwitchOffGen()
        SyncLock SyncRoot
            _generator.SwitchOff()
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
