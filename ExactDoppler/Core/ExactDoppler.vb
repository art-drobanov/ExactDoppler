Imports System.Drawing
Imports Bwl.Imaging
Imports NAudio.Dsp

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
    Private Const _waterfallBlockDuration = _windowSize / _sampleRate '0.6827
    Private Const _topFreq = 23000 '23000

    'Данные
    Private _inputDeviceIdx As Integer = -1
    Private _outputDeviceIdx As Integer = -1
    Private _dopplerLog As New DopplerLog()
    Private _config As ExactDopplerConfig

    'Объекты
    Private WithEvents _waveSource As IWaveSource
    Private _sineGenerator As SineGenerator
    Private _fftExplorer As FFTExplorer
    Private _lowpassFilter As BiQuadFilter
    Private _motionExplorers As List(Of MotionExplorer)
    Private _pcmBlocksCounter As Long = 0

    Private _syncRoot As New Object()

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
    Public ReadOnly Property DopplerSize As Integer
        Get
            Return _dopplerSize
        End Get
    End Property

    ''' <summary>Длительность блока на "водопаде".</summary>
    Public ReadOnly Property WaterfallBlockDuration As Double
        Get
            Return _waterfallBlockDuration
        End Get
    End Property

    ''' <summary>Количество обработанных блоков PCM.</summary>
    Public ReadOnly Property PcmBlocksCounter As Long
        Get
            SyncLock _syncRoot
                Return _pcmBlocksCounter
            End SyncLock
        End Get
    End Property

    ''' <summary>Индекс устройства захвата аудио.</summary>
    Public Property InputDeviceIdx As Integer
        Get
            SyncLock _syncRoot
                Return _inputDeviceIdx
            End SyncLock
        End Get
        Set(value As Integer)
            SyncLock _syncRoot
                If value >= 0 AndAlso value < Me.InputAudioDevices.Count Then
                    _inputDeviceIdx = value
                    _waveSource = New WaveInSource(_inputDeviceIdx, _inputDeviceIdx.ToString(), _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallBlockDuration) With {.SampleProcessor = AddressOf SampleProcessor}
                Else
                    _inputDeviceIdx = -1
                End If
            End SyncLock
        End Set
    End Property

    ''' <summary>Имя аудиофайла.</summary>
    Public Property InputWavFile As String
        Get
            SyncLock _syncRoot
                Return If(_waveSource IsNot Nothing, _waveSource.Name, String.Empty)
            End SyncLock
        End Get
        Set(value As String)
            _inputDeviceIdx = -1
            SyncLock _syncRoot
                value = value.Trim()
                If Not String.IsNullOrEmpty(value) Then
                    _waveSource = New WaveFileSource(value, _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallBlockDuration) With {.SampleProcessor = AddressOf SampleProcessor}
                End If
            End SyncLock
        End Set
    End Property

    ''' <summary>Индекс устройства вывода аудио.</summary>
    Public Property OutputDeviceIdx As Integer
        Get
            SyncLock _syncRoot
                Return _outputDeviceIdx
            End SyncLock
        End Get
        Set(value As Integer)
            SyncLock _syncRoot
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
            SyncLock _syncRoot
                Return _sineGenerator.Volume
            End SyncLock
        End Get
        Set(value As Single)
            SyncLock _syncRoot
                _sineGenerator.Volume = value
            End SyncLock
        End Set
    End Property

    ''' <summary>Конфигурация доплеровского анализатора.</summary>
    Public Property Config As ExactDopplerConfig
        Get
            SyncLock _syncRoot
                Return _config
            End SyncLock
        End Get
        Set(value As ExactDopplerConfig)
            SyncLock _syncRoot
                _config = value
            End SyncLock
        End Set
    End Property

    ''' <summary>Текущая скорость воспроизведения.</summary>
    Public ReadOnly Property SpeedX As Double
        Get
            If GetType(WaveFileSource).IsAssignableFrom(_waveSource.GetType) Then
                Return CType(_waveSource, WaveFileSource).RealPlaybackSpeedX
            Else
                Return 1
            End If
        End Get
    End Property

    ''' <summary>Используется быстрый режим?</summary>
    Private _captureFastMode As Boolean
    Public Property FastMode As Boolean
        Get
            Return _captureFastMode
        End Get
        Set(value As Boolean)
            _captureFastMode = value
            If GetType(WaveFileSource).IsAssignableFrom(_waveSource.GetType) Then
                CType(_waveSource, WaveFileSource).FastMode = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Событие "Источник PCM-семплов"
    ''' </summary>
    Public ReadOnly Property WavSource As WaveSource
        Get
            Return _waveSource
        End Get
    End Property

    ''' <summary>
    ''' Событие "PCM-семплы обработаны"
    ''' </summary>
    ''' <param name="motionExplorerResult">"Результат анализа движения".</param>
    Public Event PcmSamplesProcessed(motionExplorerResult As MotionExplorer.Result)

    ''' <summary>
    ''' Событие "Источник PCM-семплов остановлен"
    ''' </summary>
    Public Event WaveSourceStopped()

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
        _waveSource = New WaveInSource(_inputDeviceIdx, _inputDeviceIdx.ToString(), _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallBlockDuration)
        _fftExplorer = New FFTExplorer(_windowSize, _windowStep, _sampleRate, _nBitsPalette, False)
        _lowpassFilter = BiQuadFilter.LowPassFilter(_sampleRate, 14000, 1)
        MotionExplorersInit()
        InputDeviceIdx = _config.InputDeviceIdx
        OutputDeviceIdx = _config.OutputDeviceIdx
        Volume = _config.Volume
    End Sub

    ''' <summary>
    ''' Основной метод обработки
    ''' </summary>
    ''' <param name="pcmSamples">PCM-семплы.</param>
    ''' <param name="pcmSamplesCount">Количество семплов (для учета режима моно/стерео).</param>
    ''' <param name="timestamp">Штамп даты/времени.</param>
    ''' <returns>"Результат анализа движения".</returns>
    Public Function Process(pcmSamples As Single(), pcmSamplesCount As Integer, timestamp As DateTime) As MotionExplorer.Result
        Dim motionExplorerResults As New List(Of MotionExplorer.Result)()

        SyncLock _syncRoot
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
                    Dim carrierIsOK = .CarrierLevel.Average() > _config.CarrierWarningLevel 'Несущая не должна иметь слишком низкий уровень
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
        Dim result As MotionExplorer.Result
        If motionExplorerResults.Count > 1 Then
            '...получение объединенного результата анализа (помещаем по нулевому индексу)...
            result = MotionExplorerResultsIntersection(motionExplorerResults.ToArray())
        Else
            '...иначе результатом является единственный и первый элемент последовательности
            result = motionExplorerResults.First
        End If

        'Фильтрация семплов аудио для отсечения ультразвука
        Dim lowpassAudio = New Single(pcmSamples.Length - 1) {}
        For i = 0 To pcmSamples.Length - 1
            lowpassAudio(i) = _lowpassFilter.Transform(pcmSamples(i))
        Next

        With result
            .Timestamp = timestamp 'Штамп даты и времени
            .DopplerImageRaw = SetTimeStampOnImage(.DopplerImageRaw, timestamp) 'Штамп даты и времени (1/3)
            .DopplerImage = SetTimeStampOnImage(.DopplerImage, timestamp) 'Штамп даты и времени (3/3)
            .LowpassAudio = lowpassAudio
        End With
        _dopplerLog.Add(result.DopplerLogItem)

        Return result
    End Function

    ''' <summary>
    ''' Включение генератора
    ''' </summary>
    Public Sub SwitchOnGen()
        SyncLock _syncRoot
            MotionExplorersInit()
            SwitchOnGen(_config.CenterFreqs.Select(Function(item) CSng(item)))
        End SyncLock
    End Sub

    ''' <summary>
    ''' Включение генератора
    ''' </summary>
    ''' <param name="frequencies">Частоты синусов.</param>
    Public Sub SwitchOnGen(frequencies As IEnumerable(Of Single))
        SyncLock _syncRoot
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
        SyncLock _syncRoot
            _config.CenterFreqs = {frequency}
            _sineGenerator.SwitchOn({frequency})
        End SyncLock
    End Sub

    ''' <summary>
    ''' Выключение генератора
    ''' </summary>
    Public Sub SwitchOffGen()
        SyncLock _syncRoot
            _sineGenerator.SwitchOff()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Запуск
    ''' </summary>
    Public Sub Start()
        SyncLock _syncRoot
            _pcmBlocksCounter = 0
            With _waveSource
                .SetSampleProcessor(AddressOf SampleProcessor)
                If GetType(WaveFileSource).IsAssignableFrom(_waveSource.GetType) Then
                    CType(_waveSource, WaveFileSource).FastMode = _captureFastMode
                End If
                .Start()
            End With
        End SyncLock
    End Sub

    ''' <summary>
    ''' Останов
    ''' </summary>
    Public Sub [Stop]()
        SyncLock _syncRoot
            _waveSource.Stop()
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
                    gr.DrawString(timeStr, New Font("Microsoft Sans Serif", 12.0F), Brushes.LightSlateGray, New PointF(0, 0))
                End Using
                stringImg = BitmapConverter.BitmapToRGBMatrix(bmp)
            End Using
            Parallel.For(0, 3, Sub(channel As Integer)
                                   'Текст с датой и временем...
                                   For y = 0 To stringImg.Height - 1
                                       For x = 0 To stringImg.Width - 1
                                           result.MatrixPixel(channel, x, y) = stringImg.MatrixPixel(channel, x, y)
                                       Next
                                   Next
                               End Sub)
            Parallel.For(0, 3, Sub(channel As Integer)
                                   For y = 0 To image.Height - 1
                                       'Строка изображения...
                                       For x = 0 To image.Width - 1
                                           result.MatrixPixel(channel, strW + x, y) = image.MatrixPixel(channel, x, y)
                                       Next
                                       '...и линии-ограничители
                                       Dim color = Drawing.Color.LightSlateGray
                                       If channel = SharedConsts.RedChannel Then
                                           result.MatrixPixel(channel, strW + 0, y) = color.R * 0.75
                                           result.MatrixPixel(channel, strW + 1, y) = color.R
                                           result.MatrixPixel(channel, strW + image.Width - 1, y) = color.R * 0.75
                                           result.MatrixPixel(channel, strW + image.Width - 2, y) = color.R
                                       End If
                                       If channel = SharedConsts.GreenChannel Then
                                           result.MatrixPixel(channel, strW + 0, y) = color.G * 0.75
                                           result.MatrixPixel(channel, strW + 1, y) = color.G
                                           result.MatrixPixel(channel, strW + image.Width - 1, y) = color.G * 0.75
                                           result.MatrixPixel(channel, strW + image.Width - 2, y) = color.G
                                       End If
                                       If channel = SharedConsts.BlueChannel Then
                                           result.MatrixPixel(channel, strW + 0, y) = color.B * 0.75
                                           result.MatrixPixel(channel, strW + 1, y) = color.B
                                           result.MatrixPixel(channel, strW + image.Width - 1, y) = color.B * 0.75
                                           result.MatrixPixel(channel, strW + image.Width - 2, y) = color.B
                                       End If
                                   Next
                               End Sub)
        End If

        Return result
    End Function

    ''' <summary>
    ''' Пересечение результатов доплеровского анализа на разных частотах (с выделением существенной части)
    ''' </summary>
    Private Function MotionExplorerResultsIntersection(motionExplorerResults As MotionExplorer.Result()) As MotionExplorer.Result
        Dim result As New MotionExplorer.Result()

        With result
            .Duration = motionExplorerResults.First.Duration
            .Timestamp = motionExplorerResults.First.Timestamp
            .DopplerLogItem = New DopplerLogItem(result.Timestamp,
                                                   motionExplorerResults.Select(Function(item) item.DopplerLogItem.LowDoppler).Min(),
                                                   motionExplorerResults.Select(Function(item) item.DopplerLogItem.HighDoppler).Min(),
                                                   motionExplorerResults.All(Function(item) item.DopplerLogItem.CarrierIsOK))
            .IsWarning = motionExplorerResults.Any(Function(item) item.IsWarning)
            .CarrierLevel = IntersectListsByMin(motionExplorerResults.Select(Function(item) item.CarrierLevel))
            .LowDoppler = IntersectListsByMin(motionExplorerResults.Select(Function(item) item.LowDoppler))
            .HighDoppler = IntersectListsByMin(motionExplorerResults.Select(Function(item) item.HighDoppler))
            .DopplerImageRaw = IntersectImagesByMin(motionExplorerResults.Select(Function(item) item.DopplerImageRaw))
            .DopplerImage = IntersectImagesByMin(motionExplorerResults.Select(Function(item) item.DopplerImage))
        End With

        Return result
    End Function

    ''' <summary>
    ''' Инициализация анализаторов движения (выделенно для каждой частоты)
    ''' </summary>
    Private Sub MotionExplorersInit()
        SyncLock _syncRoot
            _motionExplorers = New List(Of MotionExplorer)() 'Для каждой центральной частоты нужен свой MotionExplorer
            For Each centerFreq In _config.CenterFreqs
                _motionExplorers.Add(New MotionExplorer(_nBitsPalette, _fftExplorer))
            Next
        End SyncLock
    End Sub

    ''' <summary>
    ''' Обработчик PCM-семплов
    ''' </summary>
    ''' <param name="pcmSamples">PCM-семплы.</param>
    ''' <param name="pcmSamplesCount">Количество семплов (для учета режима моно/стерео).</param>
    ''' <param name="timestamp">Штамп даты/времени.</param>
    Private Sub SampleProcessor(pcmSamples As Single(), pcmSamplesCount As Integer, timestamp As DateTime)
        Dim motionExplorerResult = Process(pcmSamples, pcmSamplesCount, timestamp)
        If motionExplorerResult IsNot Nothing Then
            If motionExplorerResult.DopplerImageRaw.Height > 1 AndAlso motionExplorerResult.DopplerImage.Height > 1 Then
                SyncLock _syncRoot
                    _pcmBlocksCounter += 1
                End SyncLock
                RaiseEvent PcmSamplesProcessed(motionExplorerResult)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Попиксельное "пересечение" набора изображений
    ''' </summary>
    Private Function IntersectImagesByMin(sources As IEnumerable(Of RGBMatrix)) As RGBMatrix
        Dim W = sources.First.Width
        Dim H = sources.First.Height
        Dim N = sources.Count
        Dim result As New RGBMatrix(W, H)

        Parallel.For(0, 3, Sub(channel As Integer)
                               For y = 0 To H - 1
                                   For x = 0 To W - 1
                                       Dim minVal = sources(0).MatrixPixel(channel, x, y)
                                       For k = 1 To N - 1
                                           Dim currVal = sources(k).MatrixPixel(channel, x, y)
                                           minVal = If(currVal < minVal, currVal, minVal)
                                       Next
                                       result.MatrixPixel(channel, x, y) = minVal 'Минимум - как аналог пересечения через AND
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
        For Each list In sourceLists
            sourceListsArr.Add(list.ToArray())
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

    Private Sub WaveSourceStoppedHandler() Handles _waveSource.Stopped
        RaiseEvent WaveSourceStopped()
    End Sub
End Class
