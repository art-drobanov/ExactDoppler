Imports Bwl.Imaging
Imports ExactAudio

Public Class MainForm
    Private WithEvents _exactDoppler As New ExactDoppler()
    Private _waterfallShort As New RGBWaterfall With {.MaxBlocksCount = 12}
    Private _waterfallFull As New RGBWaterfall
    Private _blocksCounter As Long = 0

    Public Sub New()
        InitializeComponent()

        _outputAudioDevicesRefreshButton_Click(Nothing, Nothing)
        _inputAudioDevicesRefreshButton_Click(Nothing, Nothing)
        Application.DoEvents()

        If _exactDoppler.OutputDeviceIdx >= 0 Then
            _outputAudioDevicesListBox.SelectedIndex = _exactDoppler.OutputDeviceIdx
            _outputAudioDevicesRefreshButton.Text = _outputAudioDevicesListBox.Items(_exactDoppler.OutputDeviceIdx) + " / Refresh"
        Else
            _outputAudioDevicesRefreshButton.Text = "Refresh"
        End If
        If _exactDoppler.InputDeviceIdx >= 0 Then
            _inputAudioDevicesListBox.SelectedIndex = _exactDoppler.InputDeviceIdx
            _inputAudioDevicesRefreshButton.Text = _inputAudioDevicesListBox.Items(_exactDoppler.InputDeviceIdx) + " / Refresh"
        Else
            _inputAudioDevicesRefreshButton.Text = "Refresh"
        End If
    End Sub

    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorerResult) Handles _exactDoppler.PcmSamplesProcessed
        'Waterfall
        Dim waterfallBlock = motionExplorerResult.Image
        _waterfallShort.Add(waterfallBlock)
        _waterfallFull.Add(waterfallBlock)
        _waterfallDisplayBitmapControl.Invoke(Sub()
                                                  Dim wfBmp = _waterfallShort.ToBitmap(1.0)
                                                  If wfBmp IsNot Nothing Then
                                                      Dim bmp = New Bitmap(wfBmp, _waterfallDisplayBitmapControl.Width, _waterfallDisplayBitmapControl.Height)
                                                      With _waterfallDisplayBitmapControl
                                                          .DisplayBitmap.DrawBitmap(bmp)
                                                          .Refresh()
                                                      End With
                                                  End If
                                              End Sub)
        'GUI
        _blocksCounter += 1
        Me.Invoke(Sub()
                      _blocksLabel.Text = _blocksCounter.ToString()
                      Dim dopplerLogItem = motionExplorerResult.DopplerLogItem.ToString()
                      _dopplerLogTextBox.Lines = {dopplerLogItem}
                  End Sub)
    End Sub

    Private Sub _captureOffButton_Click(sender As Object, e As EventArgs) Handles _captureOffButton.Click
        _exactDoppler.Stop()

        'FileName
        Dim snapshotFilename = DateTime.Now.ToString("yyyy-MM-dd__HH.mm.ss.ffff")

        'DopplerLog
        If _exactDoppler.DopplerLog.Items.Any() Then
            'Log
            Dim logFilename = "dopplerLog__" + snapshotFilename + ".txt"
            With _exactDoppler.DopplerLog
                .Write(logFilename)
                .Clear()
            End With
            'Exact Doppler Log Write/Read Test
            Dim dopplerLogTest As New DopplerLog()
            With dopplerLogTest
                .Read(logFilename)
                .Write(logFilename.Replace(".txt", ".copy.txt"))
            End With
        End If

        'WaterFall
        Dim waterfall = _waterfallFull.ToBitmap()
        If waterfall IsNot Nothing Then
            waterfall.Save("dopplerImg__" + snapshotFilename + ".png")
        End If
        _waterfallFull.Clear()
        _waterfallShort.Clear()

        'GUI
        _inputGroupBox.Text = "Input [ OFF ]"
        _captureOnButton.BackColor = Color.MediumSpringGreen
    End Sub

    Private Sub _scrButton_Click(sender As Object, e As EventArgs) Handles _scrButton.Click
        Dim snapshotFilename = DateTime.Now.ToString("yyyy-MM-dd__HH.mm.ss.ffff")
        'WaterFall
        Dim waterfall1 = _waterfallShort.ToBitmap()
        If waterfall1 IsNot Nothing Then
            waterfall1.Save("dopplerScr1__" + snapshotFilename + ".png")
        End If
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _outputAudioDevicesRefreshButton_Click(sender, e)
        _inputAudioDevicesRefreshButton_Click(sender, e)
        _sineFreqTrackBar_Scroll(sender, e)
        _blindZoneTrackBar_Scroll(sender, e)
    End Sub

    Private Sub UpdateExactDopplerConfig()
        If Not Me.Visible Then
            Return
        End If
        Dim topCenterFreq As Double
        Dim blindZone As Integer
        Me.Invoke(Sub()
                      topCenterFreq = Convert.ToDouble(_freq2Label.Text)
                      blindZone = _blindZoneTrackBar.Value
                  End Sub)
        Dim pcmOutput = True
        Dim imageOutput = True
        Dim freq2 = topCenterFreq
        Dim freq1 = freq2 - 700
        If freq1 < 1000 Then
            Throw New Exception("freq1 < 1000")
        End If
        _exactDoppler.Config = New ExactDopplerConfig(0, 0, 1.0, {freq1, freq2}, blindZone, 10)
        _freq1Label.Text = freq1
        _freq2Label.Text = freq2
    End Sub

    Private Sub _sineGenButton_Click(sender As Object, e As EventArgs) Handles _switchOnButton.Click
        _exactDoppler.Volume = _volumeTrackBar.Value / 100.0F
        Dim freq2 = Convert.ToInt32(_freq2Label.Text)
        Dim freq1 = freq2 - 700
        If freq1 < 1000 Then
            Throw New Exception("freq1 < 1000")
        End If
        _exactDoppler.SwitchOnGen({freq1, freq2})
        _outputGroupBox.Text = "Output [ ON AIR! ]"
        _switchOnButton.BackColor = Me.BackColor
    End Sub

    Private Sub _switchOffButton_Click(sender As Object, e As EventArgs) Handles _switchOffButton.Click
        _exactDoppler.SwitchOffGen()
        _outputGroupBox.Text = "Output [ OFF ]"
        _switchOnButton.BackColor = Color.MediumSpringGreen
    End Sub

    Private Sub _captureOnButton_Click(sender As Object, e As EventArgs) Handles _captureOnButton.Click
        _waterfallShort.Clear()
        _exactDoppler.Start()
        _inputGroupBox.Text = "Input [ ON ]"
        _captureOnButton.BackColor = Me.BackColor
    End Sub

    Private Sub _sineFreqTrackBar_Scroll(sender As Object, e As EventArgs) Handles _sineFreqTrackBar.Scroll
        _freq2Label.Text = _sineFreqTrackBar.Value * _exactDoppler.DopplerSize
        _volumeTrackBar_Scroll(sender, e)
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _mixCheckBox_CheckedChanged(sender As Object, e As EventArgs)
        _sineGenButton_Click(sender, e)
    End Sub

    Private Sub _volumeTrackBar_Scroll(sender As Object, e As EventArgs) Handles _volumeTrackBar.Scroll
        _exactDoppler.Volume = _volumeTrackBar.Value / 100.0F
    End Sub

    Private Sub _outputAudioDevicesRefreshButton_Click(sender As Object, e As EventArgs) Handles _outputAudioDevicesRefreshButton.Click
        _outputAudioDevicesListBox.Items.Clear()
        For Each deviceName In _exactDoppler.OutputAudioDevices
            _outputAudioDevicesListBox.Items.Add(deviceName)
        Next
    End Sub

    Private Sub _inputAudioDevicesRefreshButton_Click(sender As Object, e As EventArgs) Handles _inputAudioDevicesRefreshButton.Click
        _inputAudioDevicesListBox.Items.Clear()
        For Each deviceName In _exactDoppler.InputAudioDevices
            _inputAudioDevicesListBox.Items.Add(deviceName)
        Next
    End Sub

    Private Sub _outputAudioDevicesListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _outputAudioDevicesListBox.SelectedIndexChanged
        _exactDoppler.OutputDeviceIdx = _outputAudioDevicesListBox.SelectedIndex
        _outputAudioDevicesRefreshButton.Text = _outputAudioDevicesListBox.Items(_exactDoppler.OutputDeviceIdx) + " / Refresh"
    End Sub

    Private Sub _inputAudioDevicesListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _inputAudioDevicesListBox.SelectedIndexChanged
        _captureOffButton_Click(sender, e)
        _exactDoppler.InputDeviceIdx = _inputAudioDevicesListBox.SelectedIndex
        _inputAudioDevicesRefreshButton.Text = _inputAudioDevicesListBox.Items(_exactDoppler.InputDeviceIdx) + " / Refresh"
    End Sub

    Private Sub _blindZoneTrackBar_Scroll(sender As Object, e As EventArgs) Handles _blindZoneTrackBar.Scroll
        _blindZoneLabel.Text = _blindZoneTrackBar.Value
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _displayLeftCheckBox_CheckedChanged(sender As Object, e As EventArgs)
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _displayRightWithLeftCheckBox_CheckStateChanged(sender As Object, e As EventArgs)
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _displayCenterCheckBox_CheckedChanged(sender As Object, e As EventArgs)
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _displayRightCheckBox_CheckedChanged(sender As Object, e As EventArgs)
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _outTestButton_Click(sender As Object, e As EventArgs) Handles _outTestButton.Click
        If MessageBox.Show("Play DTMF: '151 262 888 111'", "Output Test", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Dim dtmf = New DTMFGenerator(_outputAudioDevicesListBox.SelectedIndex, 48000) With {.Volume = _volumeTrackBar.Value / 100.0F}
            dtmf.Play("151 262 888 111")
        End If
    End Sub
End Class
