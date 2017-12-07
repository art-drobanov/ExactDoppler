Imports System.IO
Imports ExactAudio
Imports Bwl.Imaging

Public Class MainForm
    Private _wavFileMarker = "[Wav File / PCM]"

    Private WithEvents _exactDopplerProcessor As ExactDopplerProcessor = New ExactDopplerProcessor()

    Public Sub New()
        InitializeComponent()

        Me.Text += " " + My.Application.Info.Version.ToString()

        _outputAudioDevicesRefreshButton_Click(Nothing, Nothing)
        _inputAudioDevicesRefreshButton_Click(Nothing, Nothing)
        Application.DoEvents()

        _exactDopplerProcessor.WaterfallShortSize = _waterfallDisplayBitmapControl.Size
        If _exactDopplerProcessor.ExactDoppler.OutputDeviceIdx >= 0 Then
            _outputAudioDevicesListBox.SelectedIndex = _exactDopplerProcessor.ExactDoppler.OutputDeviceIdx
            _outputAudioDevicesRefreshButton.Text = _outputAudioDevicesListBox.Items(_exactDopplerProcessor.ExactDoppler.OutputDeviceIdx) + " / Refresh"
            _inputGroupBox.Text = String.Format("Input [ OFF ] at device with zero-based idx '{0}'", _exactDopplerProcessor.ExactDoppler.InputDeviceIdx)
            _outputGroupBox.Text = String.Format("Output [ OFF ] at device with zero-based idx '{0}'", _exactDopplerProcessor.ExactDoppler.OutputDeviceIdx)
        Else
            _outputAudioDevicesRefreshButton.Text = "Refresh"
        End If
        If _exactDopplerProcessor.ExactDoppler.InputDeviceIdx >= 0 Then
            _inputAudioDevicesListBox.SelectedIndex = _exactDopplerProcessor.ExactDoppler.InputDeviceIdx
            _inputAudioDevicesRefreshButton.Text = _inputAudioDevicesListBox.Items(_exactDopplerProcessor.ExactDoppler.InputDeviceIdx) + " / Refresh"
        Else
            _inputAudioDevicesRefreshButton.Text = "Refresh"
        End If
    End Sub

    Private Sub PcmSamplesProcessed(res As ExactDopplerProcessor.Result) Handles _exactDopplerProcessor.PcmSamplesProcessed
        Me.Invoke(Sub()
                      'WaterfallShort
                      Dim bmp = If(_rawImageCheckBox.Checked, res.WaterfallShortRaw, res.WaterfallShort)
                      If bmp IsNot Nothing Then
                          With _waterfallDisplayBitmapControl
                              .DisplayBitmap.DrawBitmap(bmp)
                              .Refresh()
                          End With
                      End If

                      'GUI
                      _blocksLabel.Text = res.PcmBlocksCounter.ToString()
                      Dim dopplerLogItem = res.DopplerLogItem.ToString()
                      _dopplerLogTextBox.Lines = {dopplerLogItem}
                      _speedXLabel.Text = res.SpeedX.ToString("F1")

                      'Alarm
                      If res.AlarmDetected Then
                          _alarmCheckBox.BackColor = Color.Red
                      Else
                          _alarmCheckBox.BackColor = Color.DeepSkyBlue
                      End If
                  End Sub)
    End Sub

    Private Sub WaterfallsAreFull(waterfallFullRaw As DopplerWaterfall, waterfallFull As DopplerWaterfall) Handles _exactDopplerProcessor.WaterfallsAreFull
        _exactDopplerProcessor.WriteDopplerDataAndClear()
    End Sub

    Private Sub Alarm(alarm As AlarmManager.Result) Handles _exactDopplerProcessor.Alarm
        Me.BeginInvoke(Sub()
                           If _alarmCheckBox.Checked Then
                               _exactDopplerProcessor.AlarmManager.WriteAlarmData("Alarm", alarm)
                           Else
                               _alarmCheckBox.BackColor = Color.DeepSkyBlue
                           End If
                       End Sub)
    End Sub

    Private Sub AlarmRecorded(alarm As AlarmManager.Result) Handles _exactDopplerProcessor.AlarmRecorded
        Me.BeginInvoke(Sub()
                           _alarmCheckBox.BackColor = Color.DeepSkyBlue
                           If _alarmCheckBox.Checked Then
                               _exactDopplerProcessor.AlarmManager.WriteAlarmData("AlarmRecorded", alarm)
                           End If
                       End Sub)
    End Sub

    Private Sub _captureOffButton_Click(sender As Object, e As EventArgs) Handles _captureOffButton.Click
        _alarmCheckBox.BackColor = Color.DeepSkyBlue

        With _exactDopplerProcessor
            .ExactDoppler.Stop()
            .AlarmManager.CheckDataDir()
            .WriteDopplerDataAndClear()
            .WaterfallShort.Clear()
        End With

        'GUI
        _inputGroupBox.Text = String.Format("Input [ OFF ] at device with zero-based idx '{0}'", _exactDopplerProcessor.ExactDoppler.InputDeviceIdx)
        _captureOnButton.BackColor = Color.MediumSpringGreen

        _inputAudioDevicesListBox.Enabled = True
    End Sub

    Private Sub _scrButton_Click(sender As Object, e As EventArgs) Handles _scrButton.Click
        Dim snapshotFilename = DateTime.Now.ToString("yyyy-MM-dd__HH.mm.ss.ffff")
        'WaterFall

        With _exactDopplerProcessor
            Dim waterfall1 = .WaterfallShort.ToBitmap()
            If waterfall1 IsNot Nothing Then
                _exactDopplerProcessor.AlarmManager.CheckDataDir()
                waterfall1.Save(Path.Combine(.AlarmManager.DataDir, "dopplerScr__" + snapshotFilename + ".png"))
            End If
        End With
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
        If Not _topFreqOnlyCheckBox.Checked Then
            _exactDopplerProcessor.ExactDoppler.Config = New ExactDopplerConfig(0, 0, 1.0, {freq1, freq2}, blindZone, 10)
        Else
            _exactDopplerProcessor.ExactDoppler.Config = New ExactDopplerConfig(0, 0, 1.0, {freq2}, blindZone, 10)
        End If
        _freq1Label.Text = String.Format("{0} Hz", freq1)
        _freq2Label.Text = String.Format("{0} Hz", freq2)
    End Sub

    Private Sub _sineGenButton_Click(sender As Object, e As EventArgs) Handles _switchOnButton.Click
        If MessageBox.Show("High power ultrasound may damage the tweeter, " +
                           "and also may damage your hearing. " +
                           "By pressing 'Yes' i confirm, that: " +
                           "I'm sure that the chosen output is not connected to the headphones. " +
                           "I'm sure that the chosen output is not connected to the expensive speaker system. " +
                           "The audio output device transmits the signal to the built-in laptop speakers " +
                           "or to the cheap USB-speakers equipped with one speaker per channel.",
                           "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Information) = DialogResult.No Then
            Return
        End If
        _outputAudioDevicesListBox.Enabled = False
        _exactDopplerProcessor.ExactDoppler.Volume = _volumeTrackBar.Value / 100.0F
        Dim freq2 = Convert.ToInt32(_freq2Label.Text.Replace("Hz", String.Empty))
        Dim freq1 = freq2 - 700
        If freq1 < 1000 Then
            Throw New Exception("freq1 < 1000")
        End If
        If Not _topFreqOnlyCheckBox.Checked Then
            _exactDopplerProcessor.ExactDoppler.SwitchOnGen({freq1, freq2})
        Else
            _exactDopplerProcessor.ExactDoppler.SwitchOnGen({freq2})
        End If
        _outputGroupBox.Text = String.Format("Output [ ON AIR! ] at device with zero-based idx '{0}'", _exactDopplerProcessor.ExactDoppler.OutputDeviceIdx)
        _switchOnButton.BackColor = Me.BackColor
    End Sub

    Private Sub _sineGenSwitchOffButton_Click(sender As Object, e As EventArgs) Handles _sineGenSwitchOffButton.Click
        _exactDopplerProcessor.ExactDoppler.SwitchOffGen()
        _outputGroupBox.Text = String.Format("Output [ OFF ] at device with zero-based idx '{0}'", _exactDopplerProcessor.ExactDoppler.OutputDeviceIdx)
        _switchOnButton.BackColor = Color.MediumSpringGreen
        _outputAudioDevicesListBox.Enabled = True
    End Sub

    Private Sub _captureOnButton_Click(sender As Object, e As EventArgs) Handles _captureOnButton.Click
        _inputAudioDevicesListBox.Enabled = False
        _exactDopplerProcessor.WaterfallShort.Clear()
        _alarmCheckBox.BackColor = Color.DeepSkyBlue
        _exactDopplerProcessor.AlarmManager.Reset()
        _exactDopplerProcessor.ExactDoppler.Start()
        _inputGroupBox.Text = String.Format("Input [ ON ] at device with zero-based idx '{0}'", _exactDopplerProcessor.ExactDoppler.InputDeviceIdx)
        _captureOnButton.BackColor = Me.BackColor
    End Sub

    Private Sub _sineFreqTrackBar_Scroll(sender As Object, e As EventArgs) Handles _sineFreqTrackBar.Scroll
        _freq2Label.Text = _sineFreqTrackBar.Value * _exactDopplerProcessor.ExactDoppler.DopplerSize
        _volumeTrackBar_Scroll(sender, e)
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _mixCheckBox_CheckedChanged(sender As Object, e As EventArgs)
        _sineGenButton_Click(sender, e)
    End Sub

    Private Sub _volumeTrackBar_Scroll(sender As Object, e As EventArgs) Handles _volumeTrackBar.Scroll
        _exactDopplerProcessor.ExactDoppler.Volume = _volumeTrackBar.Value / 100.0F
    End Sub

    Private Sub _outputAudioDevicesRefreshButton_Click(sender As Object, e As EventArgs) Handles _outputAudioDevicesRefreshButton.Click
        _outputAudioDevicesListBox.Items.Clear()
        For Each deviceName In _exactDopplerProcessor.ExactDoppler.OutputAudioDevices
            _outputAudioDevicesListBox.Items.Add(deviceName)
        Next
    End Sub

    Private Sub _inputAudioDevicesRefreshButton_Click(sender As Object, e As EventArgs) Handles _inputAudioDevicesRefreshButton.Click
        _inputAudioDevicesListBox.Items.Clear()
        For Each deviceName In _exactDopplerProcessor.ExactDoppler.InputAudioDevices
            _inputAudioDevicesListBox.Items.Add(deviceName)
        Next
        _inputAudioDevicesListBox.Items.Add(_wavFileMarker)
    End Sub

    Private Sub _outputAudioDevicesListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _outputAudioDevicesListBox.SelectedIndexChanged
        _exactDopplerProcessor.ExactDoppler.OutputDeviceIdx = _outputAudioDevicesListBox.SelectedIndex
        If _exactDopplerProcessor.ExactDoppler.OutputDeviceIdx >= 0 Then
            _outputAudioDevicesListBox.SelectedIndex = _exactDopplerProcessor.ExactDoppler.OutputDeviceIdx
            _outputAudioDevicesRefreshButton.Text = _outputAudioDevicesListBox.Items(_exactDopplerProcessor.ExactDoppler.OutputDeviceIdx) + " / Refresh"
            _outputGroupBox.Text = String.Format("Output [ OFF ] at device with zero-based idx '{0}'", _exactDopplerProcessor.ExactDoppler.OutputDeviceIdx)
        Else
            _outputAudioDevicesRefreshButton.Text = "Refresh"
        End If
    End Sub

    Private Sub _inputAudioDevicesListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _inputAudioDevicesListBox.SelectedIndexChanged
        _captureOffButton_Click(sender, e)
        If CType(_inputAudioDevicesListBox.SelectedItem, String).Contains(_wavFileMarker) Then
            Dim ofd = New OpenFileDialog
            With ofd
                .RestoreDirectory = True
                .AddExtension = True
                .DefaultExt = ".wav"
                .Filter = "Wave files (*.wav)|*.wav"
            End With
            If ofd.ShowDialog() = DialogResult.OK Then
                Try
                    _exactDopplerProcessor.ExactDoppler.InputWavFile = ofd.FileName
                Catch ex As Exception
                    MessageBox.Show(ex.Message)
                End Try
            End If
        Else
            _exactDopplerProcessor.ExactDoppler.InputDeviceIdx = _inputAudioDevicesListBox.SelectedIndex
            If _exactDopplerProcessor.ExactDoppler.InputDeviceIdx >= 0 Then
                _inputAudioDevicesRefreshButton.Text = _inputAudioDevicesListBox.Items(_exactDopplerProcessor.ExactDoppler.InputDeviceIdx) + " / Refresh"
                _inputGroupBox.Text = String.Format("Input [ OFF ] at device with zero-based idx '{0}'", _exactDopplerProcessor.ExactDoppler.InputDeviceIdx)
            Else
                _inputAudioDevicesRefreshButton.Text = "Refresh"
            End If
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
        Dim outputAudioDeviceSelectedIndex = _outputAudioDevicesListBox.SelectedIndex
        If MessageBox.Show("Play DTMF: '151 262 888 111'", "Output Test", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Dim dtmf = New DTMFGenerator(outputAudioDeviceSelectedIndex, 48000) With {.Volume = _volumeTrackBar.Value / 100.0F}
            dtmf.Play("151 262 888 111")
        End If
    End Sub

    Private Sub _fastModeCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _fastModeCheckBox.CheckedChanged
        _exactDopplerProcessor.ExactDoppler.FastMode = _fastModeCheckBox.Checked
    End Sub
End Class
