Imports System.IO
Imports System.Threading
Imports System.Drawing
Imports NAudio.Wave
Imports Bwl.Imaging

Public Class AlarmManager
    Public Class Result
        Public ReadOnly Property DopplerImageRaw As Bitmap
        Public ReadOnly Property DopplerImage As Bitmap
        Public ReadOnly Property LowpassAudio As Single()
        Public ReadOnly Property AlarmStartTime As DateTime

        Public Sub New(dopplerImageRaw As Bitmap, dopplerImage As Bitmap, lowpassAudio As Single(), alarmStartTime As DateTime)
            _DopplerImageRaw = dopplerImageRaw
            _DopplerImage = dopplerImage
            _LowpassAudio = lowpassAudio
            _AlarmStartTime = alarmStartTime
        End Sub
    End Class

    Private _warningMemorySize As Integer
    Private _warningsInMemoryToAlarm As Integer
    Private _warningMemory As New Queue(Of Single)
    Private _alarmWaterfallBlocksCount As Integer
    Private _alarmRecordWaterfallBlocksCount As Integer
    Private _pcmBlocksToSkip As Integer
    Private _pcmBlocksToSkipRemain As Integer
    Private _dataDir As String
    Private _alarmWaterfallRaw As DopplerWaterfall
    Private _alarmWaterfall As DopplerWaterfall
    Private _alarmRecordWaterfallRaw As DopplerWaterfall
    Private _alarmRecordWaterfall As DopplerWaterfall
    Private _alarmStartTime As DateTime
    Private _alarmCounter As Long
    Private WithEvents _exactDoppler As ExactDoppler

    Private _syncRoot As New Object()

    ''' <summary>Размер памяти для хранения тревог.</summary>
    Public Property WarningMemorySize As Integer
        Get
            SyncLock _syncRoot
                Return _warningMemorySize
            End SyncLock
        End Get
        Set(value As Integer)
            SyncLock _syncRoot
                _warningMemorySize = value
            End SyncLock
        End Set
    End Property

    ''' <summary>Количество предупреждений в памяти для активации тревоги.</summary>
    Public Property WarningsInMemoryToAlarm As Integer
        Get
            SyncLock _syncRoot
                Return _warningsInMemoryToAlarm
            End SyncLock
        End Get
        Set(value As Integer)
            SyncLock _syncRoot
                _warningsInMemoryToAlarm = value
            End SyncLock
        End Set
    End Property

    ''' <summary>Количество секунд в отображении тревоги.</summary>
    Public ReadOnly Property SecondsInAlarm As Double
        Get
            SyncLock _syncRoot
                Return _alarmWaterfallBlocksCount * _exactDoppler.WaterfallBlockDuration
            End SyncLock
        End Get
    End Property

    ''' <summary>Количество секунд в записи тревоги.</summary>
    Public ReadOnly Property SecondsInAlarmRecord As Double
        Get
            SyncLock _syncRoot
                Return _alarmRecordWaterfallBlocksCount * _exactDoppler.WaterfallBlockDuration
            End SyncLock
        End Get
    End Property

    ''' <summary>Количество блоков PCM которые нужно пропустить после старта.</summary>
    Public Property PcmBlocksToSkip As Integer
        Get
            SyncLock _syncRoot
                Return _pcmBlocksToSkip
            End SyncLock
        End Get
        Set(value As Integer)
            SyncLock _syncRoot
                _pcmBlocksToSkip = value
            End SyncLock
        End Set
    End Property

    ''' <summary>Путь к папке с данными.</summary>
    Public Property DataDir As String
        Get
            SyncLock _syncRoot
                Return _dataDir
            End SyncLock
        End Get
        Set(value As String)
            SyncLock _syncRoot
                _dataDir = value
            End SyncLock
        End Set
    End Property

    ''' <summary>Время начала записи тревоги.</summary>
    Public ReadOnly Property AlarmStartTime As DateTime
        Get
            SyncLock _syncRoot
                Return _alarmStartTime
            End SyncLock
        End Get
    End Property

    ''' <summary>В настоящее время отслеживается тревога?.</summary>
    Public ReadOnly Property AlarmDetected As Boolean
        Get
            SyncLock _syncRoot
                Return _alarmStartTime <> DateTime.MinValue
            End SyncLock
        End Get
    End Property

    ''' <summary>Счетчик количества зафиксированных тревог.</summary>
    Public ReadOnly Property AlarmCounter As Long
        Get
            SyncLock _syncRoot
                Return _alarmCounter
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Событие "PCM-семплы обработаны"
    ''' </summary>
    ''' <param name="motionExplorerResult">"Результат анализа движения".</param>
    Public Event PcmSamplesProcessed(motionExplorerResult As MotionExplorer.Result)

    ''' <summary>
    ''' Событие "Тревога!"
    ''' </summary>
    ''' <param name="alarm">Тревожные данные.</param>
    Public Event Alarm(alarm As Result)

    ''' <summary>
    ''' Событие "Тревога зафиксирована."
    ''' </summary>
    ''' <param name="alarm">Тревожные данные.</param>
    Public Event AlarmRecorded(alarm As Result)

    Public Sub New(exactDoppler As ExactDoppler)
        '44 блока - это горизонт накопления предупреждений в 30 секунд.
        'Требуется 1 полное предупреждение (по сумме уровней) для фиксации тревоги.
        'Глубина обзора при фиксации тревоги - 30 секунд. 2 минуты - длительность фиксации события (1 минута до и после тревоги).
        'Пропускается 15 блоков PCM при старте или сбросе.
        MyClass.New(exactDoppler, 44, 1, 30, 120, 15)
    End Sub

    Public Sub New(exactDoppler As ExactDoppler, warningMemorySize As Integer, warningsInMemoryToAlarm As Integer,
                   secondsInAlarmMemory As Double, secondsInAlarmRecord As Double, pcmBlocksToSkip As Integer)
        _exactDoppler = exactDoppler

        Me.WarningMemorySize = warningMemorySize
        Me.WarningsInMemoryToAlarm = warningsInMemoryToAlarm
        _alarmWaterfallBlocksCount = Math.Ceiling(secondsInAlarmMemory / _exactDoppler.WaterfallBlockDuration)
        _alarmRecordWaterfallBlocksCount = Math.Ceiling(secondsInAlarmRecord / _exactDoppler.WaterfallBlockDuration)
        _pcmBlocksToSkip = pcmBlocksToSkip
        _pcmBlocksToSkipRemain = _pcmBlocksToSkip

        _alarmWaterfallRaw = New DopplerWaterfall(True) With {.MaxBlockCount = _alarmWaterfallBlocksCount} 'Для первичного обнаружения тревоги - сразу полная емкость "водопада"
        _alarmWaterfall = New DopplerWaterfall(True) With {.MaxBlockCount = _alarmWaterfallBlocksCount} 'Для первичного обнаружения тревоги - сразу полная емкость "водопада"
        _alarmRecordWaterfallRaw = New DopplerWaterfall(True) With {.MaxBlockCount = Math.Ceiling(_alarmRecordWaterfallBlocksCount / 2.0)} 'В ожидании события храним только 1/2 емкости
        _alarmRecordWaterfall = New DopplerWaterfall(True) With {.MaxBlockCount = Math.Ceiling(_alarmRecordWaterfallBlocksCount / 2.0)} 'В ожидании события храним только 1/2 емкости

        _dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory(), "..", "data")
    End Sub

    ''' <summary>
    ''' Сброс состояния
    ''' </summary>
    Public Sub Reset()
        SyncLock _syncRoot
            _pcmBlocksToSkipRemain = _pcmBlocksToSkip
            _warningMemory.Clear()
            _alarmCounter = 0
            _alarmRecordWaterfallRaw = New DopplerWaterfall(True) With {.MaxBlockCount = Math.Ceiling(_alarmRecordWaterfallBlocksCount / 2.0)} 'В ожидании события храним только 1/2 емкости
            _alarmRecordWaterfall = New DopplerWaterfall(True) With {.MaxBlockCount = Math.Ceiling(_alarmRecordWaterfallBlocksCount / 2.0)} 'В ожидании события храним только 1/2 емкости
        End SyncLock
    End Sub

    ''' <summary>
    ''' Сохранение пары изображений в папке
    ''' </summary>
    ''' <param name="prefix">Префикс папки для сохранения (например, 'Alarm' или 'AlarmRecord').</param>
    ''' <param name="alarm">Тревожные данные.</param>
    Public Sub WriteAlarmData(prefix As String, alarm As Result)
        If alarm.DopplerImageRaw IsNot Nothing AndAlso alarm.DopplerImage IsNot Nothing Then
            Dim snapshotFilename = alarm.AlarmStartTime.ToString("yyyy-MM-dd__HH.mm.ss.ffff") 'Base FileName
            CheckDataDir()
            Dim alarmDir = Path.Combine(_dataDir, String.Format("{0}__{1}", prefix, snapshotFilename))
            If Directory.Exists(alarmDir) Then
                Try
                    Directory.Delete(alarmDir)
                Catch
                End Try
            End If
            Directory.CreateDirectory(alarmDir)
            alarm.DopplerImageRaw.Save(Path.Combine(alarmDir, "dopplerImgRaw__" + snapshotFilename + ".png"))
            alarm.DopplerImage.Save(Path.Combine(alarmDir, "dopplerImg__" + snapshotFilename + ".png"))
            Dim wavWriter = New WaveFileWriter(Path.Combine(alarmDir, "lowpassAudio__" + snapshotFilename + ".wav"), New WaveFormat(_exactDoppler.SampleRate, 1))
            With wavWriter
                .WriteSamples(alarm.LowpassAudio, 0, alarm.LowpassAudio.Length)
                .Flush()
                .Close()
            End With
        End If
    End Sub

    ''' <summary>
    ''' Проверка наличия и создание папки под данные
    ''' </summary>
    Public Sub CheckDataDir()
        SyncLock _syncRoot
            If Not Directory.Exists(_dataDir) Then
                Try
                    Directory.CreateDirectory(_dataDir)
                Catch
                End Try
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Обработчик события - "PCM-семплы обработаны"
    ''' </summary>
    ''' <param name="motionExplorerResult">Результат аназиза PCM-семплов.</param>
    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorer.Result) Handles _exactDoppler.PcmSamplesProcessed
        'Пропуск PCM-блоков
        Dim returnMotionExplorerResult = False
        SyncLock _syncRoot
            If _pcmBlocksToSkipRemain >= 1 Then
                _pcmBlocksToSkipRemain -= 1
                returnMotionExplorerResult = True
            End If
        End SyncLock
        If returnMotionExplorerResult Then
            RaiseEvent PcmSamplesProcessed(motionExplorerResult) 'Передаем семплы далее...
            Return
        End If

        SyncLock _syncRoot
            'Добавляем блоки во все водопады...
            _alarmWaterfallRaw.Add(motionExplorerResult.DopplerImageRaw, motionExplorerResult.LowpassAudio)
            _alarmWaterfall.Add(motionExplorerResult.DopplerImage)
            _alarmRecordWaterfallRaw.Add(motionExplorerResult.DopplerImageRaw, motionExplorerResult.LowpassAudio)
            _alarmRecordWaterfall.Add(motionExplorerResult.DopplerImage)

            'Если в данный момент не осуществляется запись события...
            If _alarmRecordWaterfallRaw.MaxBlockCount <> _alarmRecordWaterfallBlocksCount AndAlso _alarmRecordWaterfall.MaxBlockCount <> _alarmRecordWaterfallBlocksCount Then
                If motionExplorerResult.IsWarning Then
                    _warningMemory.Enqueue(1) 'Фиксируем одну "сработку"
                End If
                Dim warningElemsToRemove = _warningMemory.Count - _warningMemorySize 'Вычисляем количество извлекаемых элементов памяти
                For i = 1 To warningElemsToRemove
                    _warningMemory.Dequeue()
                Next
                Dim warningsInMemory = _warningMemory.Sum() 'Количество предупреждений в памяти
                If warningsInMemory >= _warningsInMemoryToAlarm Then
                    _warningMemory.Clear() 'Память предупреждений больше не нужна, очищаем!
                    _alarmRecordWaterfallRaw = New DopplerWaterfall(True) With {.MaxBlockCount = _alarmRecordWaterfallBlocksCount} 'Тревога - активируем полную емкость записи!
                    _alarmRecordWaterfall = New DopplerWaterfall(True) With {.MaxBlockCount = _alarmRecordWaterfallBlocksCount} 'Тревога - активируем полную емкость записи!
                    _alarmRecordWaterfallRaw.ScrolledBlockCount = 0 'Сбрасываем индикатор пропусков строк
                    _alarmRecordWaterfall.ScrolledBlockCount = 0 'Сбрасываем индикатор пропусков строк
                    _alarmStartTime = motionExplorerResult.Timestamp
                    _alarmCounter += 1
                    Dim thr1 = New Thread(Sub()
                                              RaiseEvent Alarm(New Result(_alarmWaterfallRaw.ToBitmap(),
                                                                          _alarmWaterfall.ToBitmap(),
                                                                          _alarmWaterfallRaw.ToPcm(),
                                                                          _alarmStartTime)) 'Alarm вызывается один раз - при активации события! Далее осуществляет запись.
                                          End Sub) With {.IsBackground = True}
                    thr1.Start()
                End If
            Else
                'Ведется запись события!
                'Во время записи события важно отслеживать момент, когда наступает переполнение и начинаются выпадения из 'водопадов' записи
                'Когда начинается запись события, "половинная" емкость водопадов для записи становится полной. Кроме того, сбрасывается счетчик выпадений.
                'Это обеспечивает расположение события в середине записи (при благоприятных условиях).
                'В любом случае, запись ведется до возникновения первого выпадения.
                If _alarmRecordWaterfallRaw.ScrolledBlockCount <> 0 AndAlso _alarmRecordWaterfall.ScrolledBlockCount <> 0 Then
                    _alarmRecordWaterfallRaw.MaxBlockCount = Math.Ceiling(_alarmRecordWaterfallBlocksCount / 2.0) 'В ожидании события храним только 1/2 емкости
                    _alarmRecordWaterfall.MaxBlockCount = Math.Ceiling(_alarmRecordWaterfallBlocksCount / 2.0) 'В ожидании события храним только 1/2 емкости
                    _alarmRecordWaterfallRaw.ScrolledBlockCount = 0 'Сбрасываем индикатор пропусков строк
                    _alarmRecordWaterfall.ScrolledBlockCount = 0 'Сбрасываем индикатор пропусков строк
                    Dim thr2 = New Thread(Sub()
                                              RaiseEvent AlarmRecorded(New Result(_alarmWaterfallRaw.ToBitmap(),
                                                                                  _alarmWaterfall.ToBitmap(),
                                                                                  _alarmWaterfallRaw.ToPcm(),
                                                                                  _alarmStartTime)) 'Тревога записана!
                                          End Sub) With {.IsBackground = True}
                    thr2.Start()
                    _alarmStartTime = DateTime.MinValue
                End If
            End If
        End SyncLock

        'Передаем семплы далее...
        RaiseEvent PcmSamplesProcessed(motionExplorerResult)
    End Sub
End Class
