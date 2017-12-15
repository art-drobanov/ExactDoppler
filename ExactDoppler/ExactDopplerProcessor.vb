Imports System.Drawing
Imports System.IO
Imports Bwl.Imaging

Public Class ExactDopplerProcessor
    Public Class Result
        Public Property WaterfallDisplayRaw As Bitmap
        Public Property WaterfallDisplay As Bitmap
        Public Property PcmBlocksCounter As Long
        Public Property DopplerLogItem As String
        Public Property SpeedX As Double
        Public Property AlarmDetected As Boolean
        Public Property IsWarning As Boolean
    End Class

    Private _waterfallDisplayRaw As DopplerWaterfall
    Private _waterfallFullRaw As DopplerWaterfall

    Private _waterfallDisplay As DopplerWaterfall
    Private _waterfallFull As DopplerWaterfall

    Private _useWaterfallRaw As Boolean

    Private WithEvents _exactDoppler As ExactDoppler
    Private WithEvents _alarmManager As AlarmManager

    Private _waterfallSyncRoot As New Object

    Public Property WaterfallDisplaySize As Size

    Public ReadOnly Property WaterfallDisplayRaw As DopplerWaterfall
        Get
            Return _waterfallDisplayRaw
        End Get
    End Property

    Public ReadOnly Property WaterfallDisplay As DopplerWaterfall
        Get
            Return _waterfallDisplay
        End Get
    End Property

    Public Property WaterfallDisplayBlocksHeight As Integer
        Get
            Return _waterfallDisplay.MaxBlockCount
        End Get
        Set(value As Integer)
            If _waterfallDisplayRaw IsNot Nothing Then
                _waterfallDisplayRaw.MaxBlockCount = value
            End If
            If _waterfallDisplay IsNot Nothing Then
                _waterfallDisplay.MaxBlockCount = value
            End If
        End Set
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
    Public Event WaveSourceStopped()

    Public Sub New()
        Me.New(New Size(0, 0), True)
    End Sub

    Public Sub New(useWaterfallRaw As Boolean)
        Me.New(New Size(0, 0), useWaterfallRaw)
    End Sub

    Public Sub New(waterfallDisplaySize As Size, useWaterfallRaw As Boolean)
        Me.WaterfallDisplaySize = waterfallDisplaySize
        _useWaterfallRaw = useWaterfallRaw

        If _useWaterfallRaw Then
            _waterfallDisplayRaw = New DopplerWaterfall(True) With {.MaxBlockCount = 12}
            _waterfallFullRaw = New DopplerWaterfall(False)
        End If

        _waterfallDisplay = New DopplerWaterfall(True) With {.MaxBlockCount = 12}
        _waterfallFull = New DopplerWaterfall(False)

        _exactDoppler = New ExactDoppler()
        _alarmManager = New AlarmManager(_exactDoppler)
    End Sub

    Public Sub [Start](switchOnGen As Boolean)
        WaterfallsClear()
        If switchOnGen Then
            _exactDoppler.SwitchOnGen()
        End If
        _alarmManager.Reset()
        _exactDoppler.Start()
    End Sub

    Public Sub [Stop]()
        _exactDoppler.SwitchOffGen()
        _exactDoppler.Stop()
        WriteDopplerDataAndClear()
        WaterfallsClear()
    End Sub

    Public Sub WriteDopplerDataAndClear()
        Dim snapshotFilename = DateTime.Now.ToString("yyyy-MM-dd__HH.mm.ss.ffff") 'Base FileName
        _alarmManager.CheckDataDir()

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
        If _useWaterfallRaw Then
            Dim waterfallFullRaw = _waterfallFullRaw.ToBitmap()
            If waterfallFullRaw IsNot Nothing Then
                waterfallFullRaw.Save(Path.Combine(_alarmManager.DataDir, "dopplerImgRaw__" + snapshotFilename + ".png"))
            End If
        End If

        Dim waterfallFull = _waterfallFull.ToBitmap()
        If waterfallFull IsNot Nothing Then
            waterfallFull.Save(Path.Combine(_alarmManager.DataDir, "dopplerImg__" + snapshotFilename + ".png"))
        End If

        WaterfallsClear()
    End Sub

    Private Sub AlarmManagerAlarm(alarm As AlarmManager.Result) Handles _alarmManager.Alarm
        RaiseEvent alarm(alarm)
    End Sub

    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorer.Result) Handles _alarmManager.PcmSamplesProcessed
        SyncLock _waterfallSyncRoot
            Dim res = New ExactDopplerProcessor.Result()

            Dim waterfallFullRawClearRequest As Boolean = False
            Dim waterfallFullClearRequest As Boolean = False

            If _useWaterfallRaw Then
                Dim waterfallBlockRaw = motionExplorerResult.DopplerImageRaw
                _waterfallDisplayRaw.Add(waterfallBlockRaw)
                waterfallFullRawClearRequest = Not _waterfallFullRaw.Add(waterfallBlockRaw)
            End If

            Dim waterfallBlock = motionExplorerResult.DopplerImage
            _waterfallDisplay.Add(waterfallBlock)
            waterfallFullClearRequest = Not _waterfallFull.Add(waterfallBlock)

            'Результат обработки
            With res
                .WaterfallDisplayRaw = If(_useWaterfallRaw, WaterfallShortToBitmap(_waterfallDisplayRaw), Nothing)
                .WaterfallDisplay = WaterfallShortToBitmap(_waterfallDisplay)
                .PcmBlocksCounter = _exactDoppler.PcmBlocksCounter.ToString()
                .DopplerLogItem = motionExplorerResult.DopplerLogItem.ToString()
                .SpeedX = _exactDoppler.SpeedX
                .AlarmDetected = __alarmManager.AlarmDetected
                .IsWarning = motionExplorerResult.IsWarning
            End With
            RaiseEvent PcmSamplesProcessed(res)

            'Data
            If waterfallFullRawClearRequest OrElse waterfallFullClearRequest Then
                RaiseEvent WaterfallsAreFull(_waterfallFullRaw, _waterfallFull)
                If _useWaterfallRaw Then
                    _waterfallFullRaw.Clear()
                End If
                _waterfallFull.Clear()
            End If
        End SyncLock
    End Sub

    Private Function WaterfallShortToBitmap(waterfallShort As DopplerWaterfall) As Bitmap
        Dim wfBmp = waterfallShort.ToBitmap()
        If wfBmp IsNot Nothing Then
            Dim W = If(WaterfallDisplaySize.Width * WaterfallDisplaySize.Height <= 0, wfBmp.Width, WaterfallDisplaySize.Width)
            Dim H = If(WaterfallDisplaySize.Width * WaterfallDisplaySize.Height <= 0, wfBmp.Height, WaterfallDisplaySize.Height)
            Dim bmp = New Bitmap(wfBmp, W, H)
            Return bmp
        Else
            Return Nothing
        End If
    End Function

    Private Sub WaterfallsClear()
        SyncLock _waterfallSyncRoot
            If _useWaterfallRaw Then
                _waterfallDisplayRaw.Clear()
                _waterfallFullRaw.Clear()
            End If
            _waterfallDisplay.Clear()
            _waterfallFull.Clear()
        End SyncLock
    End Sub

    Private Sub WaveSourceStoppedHandler() Handles _exactDoppler.WaveSourceStopped
        RaiseEvent WaveSourceStopped()
    End Sub
End Class
