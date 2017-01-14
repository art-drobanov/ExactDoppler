Imports System.IO
Imports NAudio.Wave
Imports Bwl.Imaging

Public Class AlarmManager
    Private WithEvents _exactDoppler As ExactDoppler
    Private _warningMemorySize As Integer
    Private _warningsInMemoryToAlarm As Integer
    Private _warningMemory As New Queue(Of Integer)
    Private _alarmWaterfallBlocksCount As Integer
    Private _alarmRecordWaterfallBlocksCount As Integer
    Private _alarmWaterfallRaw As DopplerWaterfall
    Private _alarmWaterfall As DopplerWaterfall
    Private _alarmRecordWaterfallRaw As DopplerWaterfall
    Private _alarmRecordWaterfall As DopplerWaterfall
    Private _dataDir As String

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

    ''' <summary>
    ''' Событие "PCM-семплы обработаны"
    ''' </summary>
    ''' <param name="motionExplorerResult">"Результат анализа движения".</param>
    Public Event PcmSamplesProcessed(motionExplorerResult As MotionExplorerResult)

    ''' <summary>
    ''' Событие "Тревога!"
    ''' </summary>
    ''' <param name="rawDopplerImage">"Сырое" доплеровское изображение.</param>
    ''' <param name="dopplerImage">Доплеровское изображение.</param>
    ''' <param name="lowpassAudio">Аудиопоток без ультразвука.</param>
    Public Event Alarm(rawDopplerImage As RGBMatrix, dopplerImage As RGBMatrix, lowpassAudio As Single())

    ''' <summary>
    ''' Событие "Тревога зафиксирована."
    ''' </summary>
    ''' <param name="rawDopplerImage">"Сырое" доплеровское изображение.</param>
    ''' <param name="dopplerImage">Доплеровское изображение.</param>
    ''' <param name="lowpassAudio">Аудиопоток без ультразвука.</param>
    Public Event AlarmRecorded(rawDopplerImage As RGBMatrix, dopplerImage As RGBMatrix, lowpassAudio As Single())

    Public Sub New(exactDoppler As ExactDoppler)
        'MyClass.New(exactDoppler, 8, 4, 10, 120) 'Рабочие настройки
        MyClass.New(exactDoppler, 8, 4, 5, 30) 'Настройки для теста
    End Sub

    Public Sub New(exactDoppler As ExactDoppler, warningMemorySize As Integer, warningsInMemoryToAlarm As Integer, secondsInAlarmMemory As Double, secondsInAlarmRecord As Double)
        _exactDoppler = exactDoppler
        Me.WarningMemorySize = warningMemorySize
        Me.WarningsInMemoryToAlarm = warningsInMemoryToAlarm
        _alarmWaterfallBlocksCount = Math.Ceiling(secondsInAlarmMemory / _exactDoppler.WaterfallBlockDuration)
        _alarmRecordWaterfallBlocksCount = Math.Ceiling(secondsInAlarmRecord / _exactDoppler.WaterfallBlockDuration)

        _alarmWaterfallRaw = New DopplerWaterfall With {.MaxBlocksCount = _alarmWaterfallBlocksCount} 'Для первичного обнаружения тревоги - сразу полная емкость "водопада"
        _alarmWaterfall = New DopplerWaterfall With {.MaxBlocksCount = _alarmWaterfallBlocksCount} 'Для первичного обнаружения тревоги - сразу полная емкость "водопада"
        _alarmRecordWaterfallRaw = New DopplerWaterfall With {.MaxBlocksCount = Math.Ceiling(_alarmRecordWaterfallBlocksCount / 2.0)} 'В ожидании события храним только 1/2 емкости
        _alarmRecordWaterfall = New DopplerWaterfall With {.MaxBlocksCount = Math.Ceiling(_alarmRecordWaterfallBlocksCount / 2.0)} 'В ожидании события храним только 1/2 емкости

        _dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory(), "..", "data")
    End Sub

    ''' <summary>
    ''' Сохранение пары изображений в папке
    ''' </summary>
    ''' <param name="prefix">Префикс папки для сохранения (например, 'Alarm' или 'AlarmRecord').</param>
    ''' <param name="rawDopplerImage">"Сырое" доплеровское изображение.</param>
    ''' <param name="dopplerImage">Доплеровское изображение.</param>
    ''' <param name="lowpassAudio">Аудиопоток без ультразвука.</param>
    Public Sub SaveImages(prefix As String, rawDopplerImage As RGBMatrix, dopplerImage As RGBMatrix, lowpassAudio As Single())
        If rawDopplerImage IsNot Nothing AndAlso dopplerImage IsNot Nothing Then
            Dim rawDopplerImageBmp = MatrixTools.RGBMatrixAlign4(rawDopplerImage).ToBitmap()
            Dim dopplerImageBmp = MatrixTools.RGBMatrixAlign4(dopplerImage).ToBitmap()
            Dim snapshotFilename = DateTime.Now.ToString("yyyy-MM-dd__HH.mm.ss.ffff") 'Base FileName
            CheckDataDir()
            Dim alarmDir = Path.Combine(_dataDir, String.Format("{0}__{1}", prefix, snapshotFilename))
            If Directory.Exists(alarmDir) Then
                Try
                    Directory.Delete(alarmDir)
                Catch
                End Try
            End If
            Directory.CreateDirectory(alarmDir)
            rawDopplerImageBmp.Save(Path.Combine(alarmDir, "rawDopplerImg__" + snapshotFilename + ".png"))
            dopplerImageBmp.Save(Path.Combine(alarmDir, "dopplerImg__" + snapshotFilename + ".png"))
            Dim wavWriter = New WaveFileWriter(Path.Combine(alarmDir, "lowpassAudio__" + snapshotFilename + ".wav"), New WaveFormat(_exactDoppler.SampleRate, 1))
            With wavWriter
                .WriteSamples(lowpassAudio, 0, lowpassAudio.Length)
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
    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorerResult) Handles _exactDoppler.PcmSamplesProcessed
        'Добавляем блоки во все водопады...
        _alarmWaterfallRaw.Add(motionExplorerResult.RawDopplerImage, motionExplorerResult.LowpassAudio)
        _alarmWaterfall.Add(motionExplorerResult.DopplerImage)
        _alarmRecordWaterfallRaw.Add(motionExplorerResult.RawDopplerImage, motionExplorerResult.LowpassAudio)
        _alarmRecordWaterfall.Add(motionExplorerResult.DopplerImage)

        'Если в данный момент не осуществляется запись события...
        If _alarmRecordWaterfallRaw.MaxBlocksCount <> _alarmRecordWaterfallBlocksCount AndAlso _alarmRecordWaterfall.MaxBlocksCount <> _alarmRecordWaterfallBlocksCount Then
            _warningMemory.Enqueue(If(motionExplorerResult.IsWarning, 1, 0))
            Dim blocksToRemove = _warningMemory.Count - _warningMemorySize
            For i = 1 To blocksToRemove
                _warningMemory.Dequeue()
            Next
            Dim warningsInMemory = _warningMemory.Sum() 'Количество предупреждений в памяти
            If warningsInMemory >= _warningsInMemoryToAlarm Then
                _warningMemory.Clear() 'Память предупреждений больше не нужна, очищаем!
                _alarmRecordWaterfallRaw = New DopplerWaterfall With {.MaxBlocksCount = _alarmRecordWaterfallBlocksCount} 'Тревога - активируем полную емкость записи!
                _alarmRecordWaterfall = New DopplerWaterfall With {.MaxBlocksCount = _alarmRecordWaterfallBlocksCount} 'Тревога - активируем полную емкость записи!
                _alarmRecordWaterfallRaw.DroppedBlocksCount = 0 'Сбрасываем индикатор пропусков строк
                _alarmRecordWaterfall.DroppedBlocksCount = 0 'Сбрасываем индикатор пропусков строк
                RaiseEvent Alarm(_alarmWaterfallRaw.ToRGBMatrix(), _alarmWaterfall.ToRGBMatrix(), _alarmWaterfallRaw.ToPcm()) 'Alarm вызывается один раз - при активации события! Далее осуществляет запись.
            End If
        Else
            'Ведется запись события!
            'Во время записи события важно отслеживать момент, когда наступает переполнение и начинаются выпадения из 'водопадов' записи
            'Когда начинается запись события, "половинная" емкость водопадов для записи становится полной. Кроме того, сбрасывается счетчик выпадений.
            'Это обеспечивает расположение события в середине записи (при благоприятных условиях).
            'В любом случае, запись ведется до возникновения первого выпадения.
            If _alarmRecordWaterfallRaw.DroppedBlocksCount <> 0 AndAlso _alarmRecordWaterfall.DroppedBlocksCount <> 0 Then
                _alarmRecordWaterfallRaw.MaxBlocksCount = Math.Ceiling(_alarmRecordWaterfallBlocksCount / 2.0) 'В ожидании события храним только 1/2 емкости
                _alarmRecordWaterfall.MaxBlocksCount = Math.Ceiling(_alarmRecordWaterfallBlocksCount / 2.0) 'В ожидании события храним только 1/2 емкости
                _alarmRecordWaterfallRaw.DroppedBlocksCount = 0 'Сбрасываем индикатор пропусков строк
                _alarmRecordWaterfall.DroppedBlocksCount = 0 'Сбрасываем индикатор пропусков строк
                RaiseEvent AlarmRecorded(_alarmRecordWaterfallRaw.ToRGBMatrix(), _alarmRecordWaterfall.ToRGBMatrix(), _alarmRecordWaterfallRaw.ToPcm()) 'Тревога записана!
            End If
        End If

        'Передаем семплы далее...
        RaiseEvent PcmSamplesProcessed(motionExplorerResult)
    End Sub
End Class
