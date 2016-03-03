Imports System.IO
Imports NAudio
Imports Bwl.Imaging
Imports ExactAudio
Imports ExactAudio.MotionExplorer

Public Class MainForm
    Private _waterfall As New RGBWaterfall
    Private _dopplerPcm As New LinkedList(Of Single())

    Private _blocksCounter As Long = 0
    Private WithEvents _exactDoppler As New ExactDoppler()

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

    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorerResult) Handles _exactDoppler.SamplesProcessed
        'Waterfall
        Dim waterfallBlock = motionExplorerResult.Image
        _waterfall.Add(waterfallBlock)
        _waterfallDisplayBitmapControl.Invoke(Sub()
                                                  Dim disp = New Bitmap(waterfallBlock.ToBitmap(), _waterfallDisplayBitmapControl.Width, _waterfallDisplayBitmapControl.Height)
                                                  _waterfallDisplayBitmapControl.DisplayBitmap.DrawBitmap(disp)
                                                  _waterfallDisplayBitmapControl.Refresh()
                                              End Sub)
        'Pcm
        _dopplerPcm.AddLast(motionExplorerResult.Pcm)
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _outputAudioDevicesRefreshButton_Click(sender, e)
        _inputAudioDevicesRefreshButton_Click(sender, e)
        _sineFreqLTrackBar_Scroll(sender, e)
        _sineFreqRTrackBar_Scroll(sender, e)
        _deadZoneTrackBar_Scroll(sender, e)
    End Sub

    Private Sub UpdateExactDopplerConfig()
        If Not Me.Visible Then Return

        Dim centerFreq As Double
        Dim deadZone As Integer
        Dim displayLeft As Boolean
        Dim displayRightWithLeft As Boolean
        Dim displayCenter As Boolean
        Dim displayRight As Boolean

        _blocksCounter += 1
        Me.Invoke(Sub()
                      _blocksLabel.Text = _blocksCounter.ToString()
                      centerFreq = Math.Max(Convert.ToDouble(_sineFreqLLabel.Text), Convert.ToDouble(_sineFreqRLabel.Text))
                      deadZone = _deadZoneTrackBar.Value
                      displayLeft = _displayLeftCheckBox.Checked
                      displayRightWithLeft = _displayRightWithLeftCheckBox.Checked
                      displayCenter = _displayCenterCheckBox.Checked
                      displayRight = _displayRightCheckBox.Checked
                  End Sub)

        Dim pcmOutput = True
        Dim imageOutput = True
        _exactDoppler.Config = New ExactDoppler.ExactDopplerConfig(centerFreq, deadZone, displayLeft, displayRightWithLeft, displayCenter, displayRight, pcmOutput, imageOutput)
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
        For Each deviceName In AudioUtils.GetAudioDeviceNamesWaveOut()
            _outputAudioDevicesListBox.Items.Add(deviceName)
        Next
    End Sub

    Private Sub _inputAudioDevicesRefreshButton_Click(sender As Object, e As EventArgs) Handles _inputAudioDevicesRefreshButton.Click
        _inputAudioDevicesListBox.Items.Clear()
        For Each deviceName In AudioUtils.GetAudioDeviceNamesWaveIn()
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

    Private Sub _deadZoneTrackBar_Scroll(sender As Object, e As EventArgs) Handles _deadZoneTrackBar.Scroll
        _deadZoneLabel.Text = _deadZoneTrackBar.Value
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _displayLeftCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _displayLeftCheckBox.CheckedChanged
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _displayRightWithLeftCheckBox_CheckStateChanged(sender As Object, e As EventArgs) Handles _displayRightWithLeftCheckBox.CheckStateChanged
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _displayCenterCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _displayCenterCheckBox.CheckedChanged
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _displayRightCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _displayRightCheckBox.CheckedChanged
        UpdateExactDopplerConfig()
    End Sub

    Private Sub _captureOnButton_Click(sender As Object, e As EventArgs) Handles _captureOnButton.Click
        _waterfall.Reset()
        _exactDoppler.Start()
        _inputGroupBox.Text = "Input [ ON ]"
        _captureOnButton.BackColor = Me.BackColor
    End Sub

    Private Sub _captureOffButton_Click(sender As Object, e As EventArgs) Handles _captureOffButton.Click
        _exactDoppler.Stop()

        'FileName
        Dim snapshotFilename = DateTime.Now.ToString("dd.MM.yyyy__HH.mm.ss.ffff")

        'DopplerLog
        If _exactDoppler.DopplerLog.Items.Any() Then
            Dim logFilename = "dopplerLog__" + snapshotFilename + ".txt"
            Using logStream = File.OpenWrite(logFilename)
                _exactDoppler.DopplerLog.Write(logStream)
                logStream.Flush()
            End Using
            _exactDoppler.DopplerLog.Clear()
            Using logStreamR = File.OpenRead(logFilename)
                Dim dopplerLogTest As New DopplerLog()
                dopplerLogTest.Read(logStreamR)
                Using logStreamW = File.OpenWrite(logFilename.Replace(".txt", ".copy.txt"))
                    dopplerLogTest.Write(logStreamW)
                    logStreamW.Flush()
                End Using
            End Using
        End If

        'WaterFall
        Dim waterfall = _waterfall.ToBitmap()
        If waterfall IsNot Nothing Then
            waterfall.Save("waterfall___" + snapshotFilename + ".jpg")
        End If
        _waterfall.Reset()

        'PCM
        If _dopplerPcm.Any() Then
            Dim wavFile As New Wave.WaveFileWriter("dopplerWav__" + snapshotFilename + ".wav", New Wave.WaveFormat(_exactDoppler.SampleRate, 1))
            For Each pcmBlock In _dopplerPcm
                wavFile.WriteSamples(pcmBlock, 0, pcmBlock.Length)
            Next
            _dopplerPcm.Clear()
            With wavFile
                .Flush()
                .Close()
            End With
        End If

        'GUI
        _inputGroupBox.Text = "Input [ OFF ]"
        _captureOnButton.BackColor = Color.MediumSpringGreen
    End Sub
End Class
