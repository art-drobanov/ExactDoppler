Imports System.Drawing
Imports System.IO
Imports Bwl.Imaging

Public Class ExactDopplerProcessor
    Public Class Result
        Public Property WaterfallShortRaw As Bitmap
        Public Property WaterfallShort As Bitmap
        Public Property PcmBlocksCounter As Long
        Public Property DopplerLogItem As String
        Public Property SpeedX As Double
        Public Property AlarmDetected As Boolean
    End Class

    Private _waterfallShortRaw As DopplerWaterfall
    Private _waterfallShort As DopplerWaterfall

    Private _waterfallFullRaw As DopplerWaterfall
    Private _waterfallFull As DopplerWaterfall

    Private WithEvents _exactDoppler As ExactDoppler
    Private WithEvents _alarmManager As AlarmManager

    Private _waterfallSyncRoot As New Object

    Public Property WaterfallShortSize As Size

    Public ReadOnly Property WaterfallShortRaw As DopplerWaterfall
        Get
            Return _waterfallShortRaw
        End Get
    End Property

    Public ReadOnly Property WaterfallShort As DopplerWaterfall
        Get
            Return _waterfallShort
        End Get
    End Property

    Public ReadOnly Property ExactDoppler As ExactDoppler
        Get
            Return _exactDoppler
        End Get
    End Property

    Public ReadOnly Property AlarmManager As AlarmManager
        Get
            Return _alarmManager
        End Get
    End Property

    Public Event PcmSamplesProcessed(res As ExactDopplerProcessor.Result)
    Public Event WaterfallsAreFull(waterfallFullRaw As DopplerWaterfall, waterfallFull As DopplerWaterfall)
    Public Event Alarm(alarm As AlarmManager.Result)
    Public Event AlarmRecorded(alarm As AlarmManager.Result)

    Public Sub New()
        Me.New(New Size(0, 0))
    End Sub

    Public Sub New(waterfallShortSize As Size)
        Me.WaterfallShortSize = waterfallShortSize
        _waterfallShortRaw = New DopplerWaterfall(True) With {.MaxBlockCount = 12}
        _waterfallShort = New DopplerWaterfall(True) With {.MaxBlockCount = 12}
        _waterfallFullRaw = New DopplerWaterfall(False)
        _waterfallFull = New DopplerWaterfall(False)
        _exactDoppler = New ExactDoppler()
        _alarmManager = New AlarmManager(_exactDoppler)
    End Sub

    Public Sub WriteDopplerDataAndClear()
        Dim snapshotFilename = DateTime.Now.ToString("yyyy-MM-dd__HH.mm.ss.ffff") 'Base FileName

        'DopplerLog
        If _exactDoppler.DopplerLog.Items.Any() Then
            'Log
            Dim logFilename = "dopplerLog__" + snapshotFilename + ".txt"
            With _exactDoppler.DopplerLog
                .Write(Path.Combine(_alarmManager.DataDir, logFilename))
                .Clear()
            End With
        End If

        'WaterFall
        Dim waterfallFullRaw = _waterfallFullRaw.ToBitmap()
        If waterfallFullRaw IsNot Nothing Then
            waterfallFullRaw.Save(Path.Combine(_alarmManager.DataDir, "dopplerImgRaw__" + snapshotFilename + ".png"))
        End If
        _waterfallFullRaw.Clear()

        Dim waterfallFull = _waterfallFull.ToBitmap()
        If waterfallFull IsNot Nothing Then
            waterfallFull.Save(Path.Combine(_alarmManager.DataDir, "dopplerImg__" + snapshotFilename + ".png"))
        End If
        _waterfallFull.Clear()
    End Sub

    Private Sub AlarmManagerAlarm(alarm As AlarmManager.Result) Handles _alarmManager.Alarm
        RaiseEvent alarm(alarm)
    End Sub

    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorer.Result) Handles _alarmManager.PcmSamplesProcessed
        SyncLock _waterfallSyncRoot
            Dim res = New ExactDopplerProcessor.Result()

            'Блок водопада
            Dim waterfallBlockRaw = motionExplorerResult.DopplerImageRaw
            Dim waterfallBlock = motionExplorerResult.DopplerImage

            'В короткий водопад добавляем блок как обычно...
            _waterfallShortRaw.Add(waterfallBlockRaw)
            _waterfallShort.Add(waterfallBlock)

            'В большой водопад добавляем с проверкой на корректность последующего добавления
            Dim waterfallFullRawClearRequest = Not _waterfallFullRaw.Add(waterfallBlockRaw)
            Dim waterfallFullClearRequest = Not _waterfallFull.Add(waterfallBlock)

            'Результат обработки
            With res
                .WaterfallShortRaw = WaterfallShortToBitmap(_waterfallShortRaw)
                .WaterfallShort = WaterfallShortToBitmap(_waterfallShort)
                .PcmBlocksCounter = __exactDoppler.PcmBlocksCounter.ToString()
                .DopplerLogItem = motionExplorerResult.DopplerLogItem.ToString()
                .SpeedX = __exactDoppler.SpeedX
                .AlarmDetected = __alarmManager.AlarmDetected
            End With
            RaiseEvent PcmSamplesProcessed(res)

            'Data
            If waterfallFullRawClearRequest OrElse waterfallFullClearRequest Then
                RaiseEvent WaterfallsAreFull(_waterfallFullRaw, _waterfallFull)
                _waterfallFullRaw.Clear()
                _waterfallFull.Clear()
            End If
        End SyncLock
    End Sub

    Private Function WaterfallShortToBitmap(waterfallShort As DopplerWaterfall) As Bitmap
        Dim wfBmp = waterfallShort.ToBitmap()
        If wfBmp IsNot Nothing Then
            Dim W = If(WaterfallShortSize.Width * WaterfallShortSize.Height <= 0, wfBmp.Width, WaterfallShortSize.Width)
            Dim H = If(WaterfallShortSize.Width * WaterfallShortSize.Height <= 0, wfBmp.Height, WaterfallShortSize.Height)
            Dim bmp = New Bitmap(wfBmp, W, H)
            Return bmp
        Else
            Return Nothing
        End If
    End Function
End Class
