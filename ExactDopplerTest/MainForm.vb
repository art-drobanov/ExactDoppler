Imports Bwl.Imaging
Imports ExactAudio
Imports ExactAudio.MotionExplorer

Public Class MainForm
    Private WithEvents _exactDoppler As New ExactDoppler()
    Private _pcmLog As New PcmLog(_exactDoppler.SampleRate)
    Private _waterfall As New RGBWaterfall
    Private _blocksCounter As Long = 0

    Public Sub New()
        InitializeComponent()

        _outputAudioDevicesRefreshButton_Click(Nothing, Nothing)
        _inputAudioDevicesRefreshButton_Click(Nothing, Nothing)
        Application.DoEvents()

        _outputAudioDevicesListBox.SelectedIndex = _exactDoppler.OutputDeviceIdx
        _inputAudioDevicesListBox.SelectedIndex = _exactDoppler.InputDeviceIdx
        _outputAudioDevicesRefreshButton.Text = _outputAudioDevicesListBox.Items(_exactDoppler.OutputDeviceIdx) + " / Refresh"
        _inputAudioDevicesRefreshButton.Text = _inputAudioDevicesListBox.Items(_exactDoppler.InputDeviceIdx) + " / Refresh"
    End Sub

    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorerResult) Handles _exactDoppler.PcmSamplesProcessed
        'Waterfall
        Dim waterfallBlock = motionExplorerResult.Image
        _waterfall.Add(waterfallBlock)
        _waterfallDisplayBitmapControl.Invoke(Sub()
                                                  Dim disp = New Bitmap(waterfallBlock.ToBitmap(), _waterfallDisplayBitmapControl.Width, _waterfallDisplayBitmapControl.Height)
                                                  With _waterfallDisplayBitmapControl
                                                      .DisplayBitmap.DrawBitmap(disp)
                                                      .Refresh()
                                                  End With
                                              End Sub)
        'Pcm
        _pcmLog.Add(motionExplorerResult.Pcm)

        'GUI
        _blocksCounter += 1
        _blocksLabel.Invoke(Sub()
                                _blocksLabel.Text = _blocksCounter.ToString()
                            End Sub)
    End Sub

    Private Sub _captureOffButton_Click(sender As Object, e As EventArgs) Handles _captureOffButton.Click
        _exactDoppler.Stop()

        'FileName
        Dim snapshotFilename = DateTime.Now.ToString("dd.MM.yyyy__HH.mm.ss.ffff")

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
        Dim waterfall = _waterfall.ToBitmap()
        If waterfall IsNot Nothing Then
            waterfall.Save("waterfall___" + snapshotFilename + ".png")
        End If
        _waterfall.Clear()

        'PCM
        If _pcmLog.Items.Any() Then
            Dim wavFilename = "dopplerWav__" + snapshotFilename + ".wav"
            _pcmLog.Write(wavFilename)
            _pcmLog.Clear()
        End If

        'GUI
        _inputGroupBox.Text = "Input [ OFF ]"
        _captureOnButton.BackColor = Color.MediumSpringGreen
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _outputAudioDevicesRefreshButton_Click(sender, e)
        _inputAudioDevicesRefreshButton_Click(sender, e)
        _sineFreqLTrackBar_Scroll(sender, e)
        _sineFreqRTrackBar_Scroll(sender, e)
        _blindZoneTrackBar_Scroll(sender, e)
    End Sub

    Private Sub UpdateExactDopplerConfig()
        If Not Me.Visible Then Return

        Dim centerFreq As Double
        Dim blindZone As Integer
        Me.Invoke(Sub()
                      centerFreq = Math.Max(Convert.ToDouble(_sineFreqLLabel.Text), Convert.ToDouble(_sineFreqRLabel.Text))
                      blindZone = _blindZoneTrackBar.Value
                  End Sub)

        Dim pcmOutput = True
        Dim imageOutput = True
        _exactDoppler.Config = New ExactDopplerConfig(0, 0, 1.0, centerFreq, blindZone)
    End Sub

    Private Sub _sineGenButton_Click(sender As Object, e As EventArgs) Handles _switchOnButton.Click
        _exactDoppler.SwitchOnGen(_sineFreqLLabel.Text, _sineFreqRLabel.Text, _mixCheckBox.Checked)
        _outputGroupBox.Text = "Output [ ON AIR! ]"
        _switchOnButton.BackColor = Me.BackColor
    End Sub

    Private Sub _switchOffButton_Click(sender As Object, e As EventArgs) Handles _switchOffButton.Click
        _exactDoppler.SwitchOffGen()
        _outputGroupBox.Text = "Output [ OFF ]"
        _switchOnButton.BackColor = Color.MediumSpringGreen
    End Sub

    Private Sub _captureOnButton_Click(sender As Object, e As EventArgs) Handles _captureOnButton.Click
        _waterfall.Clear()
        _exactDoppler.Start()
        _inputGroupBox.Text = "Input [ ON ]"
        _captureOnButton.BackColor = Me.BackColor
    End Sub

    Private Sub _sineFreqLTrackBar_Scroll(sender As Object, e As EventArgs) Handles _sineFreqLTrackBar.Scroll
        _sineFreqLLabel.Text = _sineFreqLTrackBar.Value * _exactDoppler.DopplerSize
        _volumeTrackBar_Scroll(sender, e)
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _sineFreqRTrackBar_Scroll(sender As Object, e As EventArgs) Handles _sineFreqRTrackBar.Scroll
        _sineFreqRLabel.Text = _sineFreqRTrackBar.Value * _exactDoppler.DopplerSize
        _volumeTrackBar_Scroll(sender, e)
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _mixCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _mixCheckBox.CheckedChanged
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
End Class
