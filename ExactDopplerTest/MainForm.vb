Imports System.IO
Imports Bwl.Imaging
Imports ExactAudio

Public Class MainForm
    Private WithEvents _exactDoppler As New ExactDoppler()
    Private WithEvents _alarmManager As New AlarmManager(_exactDoppler)
    Private _waterfallShort As New DopplerWaterfall With {.MaxBlocksCount = 12}
    Private _waterfallFull As New DopplerWaterfall

    Public Sub New()
        InitializeComponent()

        Me.Text += " " + My.Application.Info.Version.ToString()

        _outputAudioDevicesRefreshButton_Click(Nothing, Nothing)
        _inputAudioDevicesRefreshButton_Click(Nothing, Nothing)
        Application.DoEvents()

        If _exactDoppler.OutputDeviceIdx >= 0 Then
            _outputAudioDevicesListBox.SelectedIndex = _exactDoppler.OutputDeviceIdx
            _outputAudioDevicesRefreshButton.Text = _outputAudioDevicesListBox.Items(_exactDoppler.OutputDeviceIdx) + " / Refresh"
            _inputGroupBox.Text = String.Format("Input [ OFF ] at device with zero-based index '{0}'", _exactDoppler.InputDeviceIdx)
            _outputGroupBox.Text = String.Format("Output [ OFF ] at device with zero-based index '{0}'", _exactDoppler.OutputDeviceIdx)
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

    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorerResult) Handles _alarmManager.PcmSamplesProcessed
        Me.Invoke(Sub()
                      'Waterfall
                      Dim waterfallBlock As RGBMatrix = Nothing
                      If _rawImageCheckBox.Checked Then
                          waterfallBlock = motionExplorerResult.RawDopplerImage
                      Else
                          waterfallBlock = motionExplorerResult.DopplerImage
                      End If

                      _waterfallShort.Add(waterfallBlock)
                      _waterfallFull.Add(waterfallBlock)

                      'WaterfallDisplayBitmapControl
                      Dim wfBmp = _waterfallShort.ToBitmap(1.0)
                      If wfBmp IsNot Nothing Then
                          Dim bmp = New Bitmap(wfBmp, _waterfallDisplayBitmapControl.Width, _waterfallDisplayBitmapControl.Height)
                          With _waterfallDisplayBitmapControl
                              .DisplayBitmap.DrawBitmap(bmp)
                              .Refresh()
                          End With
                      End If

                      'GUI
                      _blocksLabel.Text = _exactDoppler.PcmBlocksCounter.ToString()
                      Dim dopplerLogItem = motionExplorerResult.DopplerLogItem.ToString()
                      _dopplerLogTextBox.Lines = {dopplerLogItem}
                  End Sub)
    End Sub

    Private Sub Alarm(rawDopplerImage As RGBMatrix, dopplerImage As RGBMatrix, lowpassAudio As Single()) Handles _alarmManager.Alarm
        Me.Invoke(Sub()
                      If _alarmCheckBox.Checked Then
                          _alarmCheckBox.BackColor = Color.Red
                          _alarmManager.Save("Alarm", rawDopplerImage, dopplerImage, lowpassAudio)
                      Else
                          _alarmCheckBox.BackColor = Color.DeepSkyBlue
                      End If
                  End Sub)
    End Sub

    Private Sub AlarmRecorded(rawDopplerImage As RGBMatrix, dopplerImage As RGBMatrix, lowpassAudio As Single()) Handles _alarmManager.AlarmRecorded
        Me.Invoke(Sub()
                      _alarmCheckBox.BackColor = Color.DeepSkyBlue
                      If _alarmCheckBox.Checked Then
                          _alarmManager.Save("AlarmRecord", rawDopplerImage, dopplerImage, lowpassAudio)
                      End If
                  End Sub)
    End Sub

    Private Sub _captureOffButton_Click(sender As Object, e As EventArgs) Handles _captureOffButton.Click
        _alarmCheckBox.BackColor = Color.DeepSkyBlue
        _exactDoppler.Stop()
        _alarmManager.CheckDataDir()

        Dim snapshotFilename = DateTime.Now.ToString("yyyy-MM-dd__HH.mm.ss.ffff") 'Base FileName

        'DopplerLog
        If _exactDoppler.DopplerLog.Items.Any() Then
            'Log
            Dim logFilename = "dopplerLog__" + snapshotFilename + ".txt"
            With _exactDoppler.DopplerLog
                .Write(Path.Combine(_alarmManager.DataDir, logFilename))
                .Clear()
            End With
            'Exact Doppler Log Write/Read Test
            Dim dopplerLogTest As New DopplerLog()
            With dopplerLogTest
                .Read(Path.Combine(_alarmManager.DataDir, logFilename))
                .Write(Path.Combine(_alarmManager.DataDir, logFilename.Replace(".txt", ".copy.txt")))
            End With
        End If

        'WaterFall
        Dim waterfall = _waterfallFull.ToBitmap()
        If waterfall IsNot Nothing Then
            waterfall.Save(Path.Combine(_alarmManager.DataDir, "dopplerImg__" + snapshotFilename + ".png"))
        End If
        _waterfallFull.Clear()
        _waterfallShort.Clear()

        'GUI
        _inputGroupBox.Text = String.Format("Input [ OFF ] at device with zero-based index '{0}'", _exactDoppler.InputDeviceIdx)
        _captureOnButton.BackColor = Color.MediumSpringGreen

        _inputAudioDevicesListBox.Enabled = True
    End Sub

    Private Sub _scrButton_Click(sender As Object, e As EventArgs) Handles _scrButton.Click
        Dim snapshotFilename = DateTime.Now.ToString("yyyy-MM-dd__HH.mm.ss.ffff")
        'WaterFall
        Dim waterfall1 = _waterfallShort.ToBitmap()
        If waterfall1 IsNot Nothing Then
            _alarmManager.CheckDataDir()
            waterfall1.Save(Path.Combine(_alarmManager.DataDir, "dopplerScr__" + snapshotFilename + ".png"))
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
                      topCenterFreq = Convert.ToDouble(_freq2Label.Text.Replace("Hz", String.Empty))
                      blindZone = _blindZoneTrackBar.Value
                  End Sub)
        Dim pcmOutput = True
        Dim imageOutput = True
        Dim freq2 = topCenterFreq
        Dim freq1 = freq2 - 700
        If freq1 < 1000 Then
            Return
        End If
        _exactDoppler.Config = New ExactDopplerConfig(0, 0, 1.0, {freq1, freq2}, blindZone, 10)
        '_exactDoppler.Config = New ExactDopplerConfig(0, 0, 1.0, {freq2}, blindZone, 10)
        _freq1Label.Text = String.Format("{0} Hz", freq1)
        _freq2Label.Text = String.Format("{0} Hz", freq2)
    End Sub

    Private Sub _sineGenButton_Click(sender As Object, e As EventArgs) Handles _switchOnButton.Click
        _outputAudioDevicesListBox.Enabled = False
        _exactDoppler.Volume = _volumeTrackBar.Value / 100.0F
        Dim freq2 = Convert.ToInt32(_freq2Label.Text.Replace("Hz", String.Empty))
        Dim freq1 = freq2 - 700
        If freq1 < 1000 Then
            Throw New Exception("freq1 < 1000")
        End If
        _exactDoppler.SwitchOnGen({freq1, freq2})
        '_exactDoppler.SwitchOnGen({freq2})
        _outputGroupBox.Text = String.Format("Output [ ON AIR! ] at device with zero-based index '{0}'", _exactDoppler.OutputDeviceIdx)
        _switchOnButton.BackColor = Me.BackColor
    End Sub

    Private Sub _sineGenSwitchOffButton_Click(sender As Object, e As EventArgs) Handles _sineGenSwitchOffButton.Click
        _exactDoppler.SwitchOffGen()
        _outputGroupBox.Text = String.Format("Output [ OFF ] at device with zero-based index '{0}'", _exactDoppler.OutputDeviceIdx)
        _switchOnButton.BackColor = Color.MediumSpringGreen
        _outputAudioDevicesListBox.Enabled = True
    End Sub

    Private Sub _captureOnButton_Click(sender As Object, e As EventArgs) Handles _captureOnButton.Click
        _inputAudioDevicesListBox.Enabled = False
        _waterfallShort.Clear()
        _alarmCheckBox.BackColor = Color.DeepSkyBlue
        _alarmManager.Reset()
        _exactDoppler.Start()
        _inputGroupBox.Text = String.Format("Input [ ON ] at device with zero-based index '{0}'", _exactDoppler.InputDeviceIdx)
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
        If _exactDoppler.OutputDeviceIdx >= 0 Then
            _outputAudioDevicesListBox.SelectedIndex = _exactDoppler.OutputDeviceIdx
            _outputAudioDevicesRefreshButton.Text = _outputAudioDevicesListBox.Items(_exactDoppler.OutputDeviceIdx) + " / Refresh"
            _outputGroupBox.Text = String.Format("Output [ OFF ] at device with zero-based index '{0}'", _exactDoppler.OutputDeviceIdx)
        Else
            _outputAudioDevicesRefreshButton.Text = "Refresh"
        End If
    End Sub

    Private Sub _inputAudioDevicesListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _inputAudioDevicesListBox.SelectedIndexChanged
        _captureOffButton_Click(sender, e)
        _exactDoppler.InputDeviceIdx = _inputAudioDevicesListBox.SelectedIndex
        If _exactDoppler.InputDeviceIdx >= 0 Then
            _inputAudioDevicesRefreshButton.Text = _inputAudioDevicesListBox.Items(_exactDoppler.InputDeviceIdx) + " / Refresh"
            _inputGroupBox.Text = String.Format("Input [ OFF ] at device with zero-based index '{0}'", _exactDoppler.InputDeviceIdx)
        Else
            _inputAudioDevicesRefreshButton.Text = "Refresh"
        End If
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
