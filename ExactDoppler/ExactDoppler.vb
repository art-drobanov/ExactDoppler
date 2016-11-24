Imports System.Drawing
Imports Bwl.Imaging
Imports ExactAudio.MotionExplorer

''' <summary>
''' Доплеровский акустический детектор
''' </summary>
Public Class ExactDoppler
    'Константы
    Private Const _windowSize = 32768 '32768
    Private Const _windowStep = 809 '809 = Round(32768 / (3 * 3 * 3 * 1.5)) -> 2/81
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
    Private _config As ExactDopplerConfig

    'Объекты
    Private _capture As WaveInSource
    Private _sineGenerator As SineGenerator
    Private _fftExplorer As FFTExplorer
    Private _motionExplorers As List(Of MotionExplorer)

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

        _fftExplorer = New FFTExplorer(_windowSize, _windowStep, _sampleRate, _nBitsPalette, False)
        MotionExplorersInit()

        InputDeviceIdx = _config.InputDeviceIdx
        OutputDeviceIdx = _config.OutputDeviceIdx
        Volume = _config.Volume
    End Sub

    ''' <summary>
    ''' Основной метод обработки
    ''' </summary>
    ''' <param name="pcmSamples">Pcm-семплы.</param>
    ''' <param name="pcmSamplesCount">Количество семплов (для учета режима моно/стерео).</param>
    ''' <param name="timestamp">Штамп даты/времени.</param>
    ''' <returns>"Результат анализа движения".</returns>
    Public Function Process(pcmSamples As Single(), pcmSamplesCount As Integer, timestamp As DateTime) As MotionExplorerResult
        Dim motionExplorerResults As New List(Of MotionExplorerResult)()

        SyncLock SyncRoot
            If _config.CenterFreqs.Length <> _motionExplorers.Count Then
                Throw New Exception("ExactDoppler: _config.CenterFreqs.Count <> _motionExplorers.Count")
            End If

            'FFT
            Dim magnitudesOnly = True
            Dim fftResultFull = _fftExplorer.Explore(pcmSamples, pcmSamplesCount, magnitudesOnly) 'БПФ без выделения диапазона

            'Цикл по всем заданным несущим
            For freqIdx = 0 To _config.CenterFreqs.Length - 1
                Dim centerFreq = _config.CenterFreqs(freqIdx)
                Dim motionExplorer = _motionExplorers(freqIdx)

                'Установка параметров частот
                Dim lowFreq As Double = 0
                Dim highFreq As Double = 0
                If centerFreq = 0 Then
                    lowFreq = 0
                    highFreq = _topFreq
                Else
                    lowFreq = centerFreq - _dopplerSize
                    highFreq = centerFreq + _dopplerSize
                    lowFreq = If(lowFreq < 0, 0, lowFreq)
                    highFreq = If(highFreq > _topFreq, _topFreq, highFreq)
                End If

                'Обработка
                Dim fftResultSubBand = _fftExplorer.SubBand(fftResultFull, lowFreq, highFreq, magnitudesOnly) 'Выделяем необходимый поддиапазон гармоник...
                Dim motionExplorerResult = motionExplorer.Process(fftResultSubBand.MagL, _config.BlindZone) '...и находим доплеровские всплески

                'Доплер-лог
                With motionExplorerResult
                    Dim lowDopplerAvg = .LowDoppler.Average()
                    Dim highDopplerAvg = .HighDoppler.Average()
                    Dim carrierIsOK = .CarrierLevel.Min() >= _config.CarrierWarningLevel 'Несущая не должна иметь слишком низкий уровень
                    Dim logItem = New DopplerLogItem(timestamp, lowDopplerAvg, highDopplerAvg, carrierIsOK) 'Элемент лога
                    motionExplorerResult.DopplerLogItem = logItem
                    If lowDopplerAvg <> 0 OrElse highDopplerAvg <> 0 OrElse Not carrierIsOK Then 'Если несущей нет - это тоже событие!
                        motionExplorerResult.IsWarning = True
                    End If
                End With

                'Проверка на нормализацию L/H
                If motionExplorerResult.DopplerLogItem.LowDoppler > 99.99 Then
                    Throw New Exception("ExactDoppler: motionExplorerResult.DopplerLogItem.LowDoppler > 99.99")
                End If
                If motionExplorerResult.DopplerLogItem.HighDoppler > 99.99 Then
                    Throw New Exception("ExactDoppler: motionExplorerResult.DopplerLogItem.HighDoppler > 99.99")
                End If

                'Результат по текущей центральной частоте
                motionExplorerResults.Add(motionExplorerResult)
            Next
        End SyncLock

        'Если в результатах анализа более одного элемента...
        If motionExplorerResults.Count > 1 Then
            '...получение объединенного результата анализа (помещаем по нулевому индексу)
            Dim resIntersection = MotionExplorerResultsIntersection(motionExplorerResults.ToArray())
            resIntersection.Timestamp = timestamp 'Штамп даты и времени (1/3)
            resIntersection.RawImage = SetTimeStampOnImage(resIntersection.RawImage, timestamp) 'Штамп даты и времени (2/3)
            resIntersection.DopplerImage = SetTimeStampOnImage(resIntersection.DopplerImage, timestamp) 'Штамп даты и времени (3/3)
            _dopplerLog.Add(resIntersection.DopplerLogItem)
            Return resIntersection
        Else
            motionExplorerResults.First.Timestamp = timestamp 'Штамп даты и времени (1/3)
            motionExplorerResults.First.RawImage = SetTimeStampOnImage(motionExplorerResults.First.RawImage, timestamp) 'Штамп даты и времени (2/3)
            motionExplorerResults.First.DopplerImage = SetTimeStampOnImage(motionExplorerResults.First.DopplerImage, timestamp) 'Штамп даты и времени (3/3)
            _dopplerLog.Add(motionExplorerResults.First.DopplerLogItem)
            Return motionExplorerResults.First
        End If
    End Function

    ''' <summary>
    ''' Включение генератора
    ''' </summary>
    Public Sub SwitchOnGen()
        SyncLock SyncRoot
            MotionExplorersInit()
            SwitchOnGen(_config.CenterFreqs.Select(Function(item) CSng(item)))
        End SyncLock
    End Sub

    ''' <summary>
    ''' Включение генератора
    ''' </summary>
    ''' <param name="frequencies">Частоты синусов.</param>
    Public Sub SwitchOnGen(frequencies As IEnumerable(Of Single))
        SyncLock SyncRoot
            _config.CenterFreqs = frequencies.Select(Function(item) CDbl(item)).ToArray()
            MotionExplorersInit()
            _sineGenerator.SwitchOn(frequencies)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Включение генератора
    ''' </summary>
    ''' <param name="frequency">Частота синуса.</param>
    Public Sub SwitchOnGen(frequency As Single)
        SyncLock SyncRoot
            _config.CenterFreqs = {frequency}
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
    ''' Установка штампа даты и времени в результате обработки
    ''' </summary>
    Private Function SetTimeStampOnImage(image As RGBMatrix, timestamp As DateTime) As RGBMatrix
        Dim strW = 208
        Dim strH = 20
        Dim resW = strW + image.Width
        Dim resH = image.Height
        Dim result = New RGBMatrix(resW, resH)
        If image.Height >= strH Then
            Dim stringImg As RGBMatrix
            Using bmp = New Bitmap(strW, strH)
                Using gr = Graphics.FromImage(bmp)
                    Dim timeStr = timestamp.ToString("yyyy-MM-dd HH:mm:ss zzz")
                    gr.DrawString(timeStr, New System.Drawing.Font("Microsoft Sans Serif", 12.0F), Brushes.LightSlateGray, New PointF(0, 0))
                End Using
                stringImg = BitmapConverter.BitmapToRGBMatrix(bmp)
            End Using
            Parallel.For(0, 3, Sub(channel As Integer)
                                   For i = 0 To stringImg.Height - 1
                                       For j = 0 To stringImg.Width - 1
                                           result.Matrix(channel)(j, i) = stringImg.Matrix(channel)(j, i)
                                       Next
                                   Next
                               End Sub)
            Parallel.For(0, 3, Sub(channel As Integer)
                                   For i = 0 To image.Height - 1
                                       For j = 0 To image.Width - 1
                                           result.Matrix(channel)(strW + j, i) = image.Matrix(channel)(j, i)
                                       Next
                                   Next
                               End Sub)
        End If

        Return result
    End Function

    ''' <summary>
    ''' Пересечение результатов доплеровского анализа на разных частотах (с выделением существенной части)
    ''' </summary>
    Private Function MotionExplorerResultsIntersection(motionExplorerResults As MotionExplorerResult()) As MotionExplorerResult
        Dim result As New MotionExplorerResult()
        result.Duration = motionExplorerResults.First.Duration
        result.Timestamp = motionExplorerResults.First.Timestamp
        result.DopplerLogItem = New DopplerLogItem(result.Timestamp,
                                                   motionExplorerResults.Select(Function(item) item.DopplerLogItem.LowDoppler).Min(),
                                                   motionExplorerResults.Select(Function(item) item.DopplerLogItem.HighDoppler).Min(),
                                                   motionExplorerResults.All(Function(item) item.DopplerLogItem.CarrierIsOK))
        result.IsWarning = motionExplorerResults.Any(Function(item) item.IsWarning)
        result.CarrierLevel = IntersectListsByMin(motionExplorerResults.Select(Function(item) item.CarrierLevel))
        result.LowDoppler = IntersectListsByMin(motionExplorerResults.Select(Function(item) item.LowDoppler))
        result.HighDoppler = IntersectListsByMin(motionExplorerResults.Select(Function(item) item.HighDoppler))
        result.RawImage = IntersectImagesByMin(motionExplorerResults.Select(Function(item) item.RawImage))
        result.DopplerImage = IntersectImagesByMin(motionExplorerResults.Select(Function(item) item.DopplerImage))
        Return result
    End Function

    ''' <summary>
    ''' Попиксельное "пересечение" набора изображений
    ''' </summary>
    Private Function IntersectImagesByMin(sources As IEnumerable(Of RGBMatrix)) As RGBMatrix
        Dim W = sources.First.Width
        Dim H = sources.First.Height
        Dim N = sources.Count
        Dim result As New RGBMatrix(W, H)
        Parallel.For(0, 3, Sub(channel As Integer)
                               For i = 0 To H - 1
                                   For j = 0 To W - 1
                                       Dim minVal = sources(0).Matrix(channel)(j, i)
                                       For k = 1 To N - 1
                                           Dim currVal = sources(k).Matrix(channel)(j, i)
                                           minVal = If(currVal < minVal, currVal, minVal)
                                       Next
                                       result.Matrix(channel)(j, i) = minVal 'Минимум - как аналог пересечения через AND
                                   Next
                               Next
                           End Sub)
        Return result
    End Function

    ''' <summary>
    ''' Поэлементное пересечение набора списков по критерию "минимальное значение"
    ''' </summary>
    Private Function IntersectListsByMin(sourceLists As IEnumerable(Of LinkedList(Of Single))) As LinkedList(Of Single)
        Dim result As New LinkedList(Of Single)
        Dim sourceListsArr As New List(Of Single())
        For Each s In sourceLists
            sourceListsArr.Add(s.ToArray())
        Next
        For j = 0 To sourceListsArr(0).Length - 1
            Dim minVal = sourceListsArr(0)(j)
            For i = 1 To sourceListsArr.Count - 1
                Dim currVal = sourceListsArr(i)(j)
                minVal = If(currVal < minVal, currVal, minVal)
            Next
            result.AddLast(minVal)
        Next
        Return result
    End Function

    ''' <summary>
    ''' Инициализация анализаторов движения (выделенно для каждой частоты)
    ''' </summary>
    Private Sub MotionExplorersInit()
        SyncLock SyncRoot
            'Для каждой центральной частоты нужен свой MotionExplorer
            _motionExplorers = New List(Of MotionExplorer)()
            For Each centerFreq In _config.CenterFreqs
                _motionExplorers.Add(New MotionExplorer(_nBitsPalette, _fftExplorer))
            Next
        End SyncLock
    End Sub

    ''' <summary>
    ''' Обработчик Pcm-семплов
    ''' </summary>
    ''' <param name="pcmSamples">Pcm-семплы.</param>
    ''' <param name="pcmSamplesCount">Количество семплов (для учета режима моно/стерео).</param>
    ''' <param name="timestamp">Штамп даты/времени.</param>
    Private Sub SampleProcessor(pcmSamples As Single(), pcmSamplesCount As Integer, timestamp As DateTime)
        Dim motionExplorerResult = Process(pcmSamples, pcmSamplesCount, timestamp)
        If motionExplorerResult IsNot Nothing Then
            RaiseEvent PcmSamplesProcessed(motionExplorerResult)
        End If
    End Sub
End Class
