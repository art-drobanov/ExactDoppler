Imports System.IO
Imports NAudio
Imports Bwl.Imaging
Imports ExactAudio
Imports ExactDoppler

Public Class MainForm

    'Константы
    Private Const _windowSize = 32768 '32768
    Private Const _windowStep = 1214 '1214 = Round(32768 / (3 * 3 * 3))
    Private Const _sampleRate = 48000 '48000
    Private Const _dopplerSize = 500 '500
    Private Const _nBitsCapture = 16 '16
    Private Const _nBitsPalette = 8 '8
    Private Const _waterfallSeconds = 2 '2
    Private Const _highFreq = 23000 '23000
    Private Const _scale = 1.0 '1.0

    'Данные
    Private _outputDeviceIdx As Integer
    Private _inputDeviceIdx As Integer
    Private _blocksCounter As Long

    'Объекты
    Private _generator As Generator
    Private _generator2 As Generator
    Private _capture As WaveInSource
    Private _motionExplorer As MotionExplorer
    Private _waterfall As RGBWaterfall
    Private _dopplerPcm As New LinkedList(Of Single())
    Private _dopplerLog As New DopplerLog()

    Public Sub New()
        InitializeComponent()

        _outputAudioDevicesRefreshButton_Click(Nothing, Nothing)
        _inputAudioDevicesRefreshButton_Click(Nothing, Nothing)
        Application.DoEvents()

        _outputDeviceIdx = 0
        _inputDeviceIdx = 0

        _generator = New Generator(_outputDeviceIdx, _sampleRate)
        _generator2 = New Generator(0, _sampleRate)
        _capture = New WaveInSource(_inputDeviceIdx, _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallSeconds) With {.SampleProcessor = AddressOf Me.SampleProcessor}
        _motionExplorer = New MotionExplorer(_windowSize, _windowStep, _sampleRate, _nBitsPalette, False)
        _waterfall = New RGBWaterfall()

        _outputAudioDevicesListBox.SelectedIndex = _outputDeviceIdx
        _inputAudioDevicesListBox.SelectedIndex = _inputDeviceIdx

        _outputAudioDevicesRefreshButton.Text = _outputAudioDevicesListBox.Items(_outputDeviceIdx) + " / Refresh"
        _inputAudioDevicesRefreshButton.Text = _inputAudioDevicesListBox.Items(_inputDeviceIdx) + " / Refresh"
    End Sub

    Private Sub SampleProcessor(samples As Single(), samplesCount As Integer)
        Dim lowFreq As Double = 0
        Dim highFreq As Double = 0
        Dim deadZone As Integer = 0
        Dim displayLeft As Boolean = False
        Dim displayRightWithLeft As Boolean = False
        Dim displayCenter As Boolean = False
        Dim displayRight As Boolean = False
        Dim play As Boolean = False

        _blocksCounter += 1
        Me.Invoke(Sub()
                      _blocksLabel.Text = _blocksCounter.ToString()

                      Dim centerFreq = Math.Max(Convert.ToDouble(_sineFreqLLabel.Text), Convert.ToDouble(_sineFreqRLabel.Text))
                      If centerFreq = 0 Then
                          lowFreq = 0
                          highFreq = _highFreq
                      Else
                          lowFreq = centerFreq - _dopplerSize
                          highFreq = centerFreq + _dopplerSize
                          lowFreq = If(lowFreq < 0, 0, lowFreq)
                          highFreq = If(highFreq > _highFreq, _highFreq, highFreq)
                      End If

                      deadZone = _deadZoneTrackBar.Value
                      displayLeft = _displayLeftCheckBox.Checked
                      displayRightWithLeft = _displayRightWithLeftCheckBox.Checked
                      displayCenter = _displayCenterCheckBox.Checked
                      displayRight = _displayRightCheckBox.Checked
                      play = _playCheckBox.Checked
                  End Sub)

        Dim motionExplorerResult = _motionExplorer.Process(samples, samplesCount, lowFreq, highFreq, deadZone, displayLeft, displayRightWithLeft, displayCenter, displayRight, True, True)

        'DopplerLog
        Dim nowTimeStamp = DateTime.Now
        Dim lowDopplerSum = motionExplorerResult.LowDoppler.Sum()
        Dim highDopplerSum = motionExplorerResult.HighDoppler.Sum()
        If lowDopplerSum <> 0 Or highDopplerSum <> 0 Then
            _dopplerLog.Add(nowTimeStamp, lowDopplerSum, highDopplerSum)
        End If

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
        If play Then
            _generator2.Play(motionExplorerResult.Pcm, False)
        End If
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _outputAudioDevicesRefreshButton_Click(sender, e)
        _inputAudioDevicesRefreshButton_Click(sender, e)
        _sineFreqLTrackBar_Scroll(sender, e)
        _sineFreqRTrackBar_Scroll(sender, e)
        _deadZoneTrackBar_Scroll(sender, e)
    End Sub

    Private Sub _sineGenButton_Click(sender As Object, e As EventArgs) Handles _switchOnButton.Click
        _generator.SwitchOn(_sineFreqLLabel.Text, _sineFreqRLabel.Text, _mixCheckBox.Checked)
        _outputGroupBox.Text = "Output [ ON AIR! ]"
        _switchOnButton.BackColor = Me.BackColor
    End Sub

    Private Sub _switchOffButton_Click(sender As Object, e As EventArgs) Handles _switchOffButton.Click
        _generator.SwitchOff()
        _outputGroupBox.Text = "Output [ OFF ]"
        _switchOnButton.BackColor = Color.MediumSpringGreen
    End Sub

    Private Sub _sineFreqLTrackBar_Scroll(sender As Object, e As EventArgs) Handles _sineFreqLTrackBar.Scroll
        _sineFreqLLabel.Text = _sineFreqLTrackBar.Value * _dopplerSize
        _volumeTrackBar_Scroll(sender, e)
    End Sub

    Private Sub _sineFreqRTrackBar_Scroll(sender As Object, e As EventArgs) Handles _sineFreqRTrackBar.Scroll
        _sineFreqRLabel.Text = _sineFreqRTrackBar.Value * _dopplerSize
        _volumeTrackBar_Scroll(sender, e)
    End Sub

    Private Sub _mixCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _mixCheckBox.CheckedChanged
        _sineGenButton_Click(sender, e)
    End Sub

    Private Sub _volumeTrackBar_Scroll(sender As Object, e As EventArgs) Handles _volumeTrackBar.Scroll
        _generator.Volume = _volumeTrackBar.Value / 100.0F
    End Sub

    Private Sub _flashButton_Click(sender As Object, e As EventArgs) Handles _flashButton.Click
        _generator.Flash(_sineFreqLLabel.Text, _sineFreqRLabel.Text, _mixCheckBox.Checked, 1000)
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
        If _outputAudioDevicesListBox.SelectedIndex = 0 Then
            With _playCheckBox
                .Checked = False
                .Enabled = False
            End With
        Else
            With _playCheckBox
                .Enabled = True
                .Checked = True
            End With
        End If
        _generator = New Generator(_outputAudioDevicesListBox.SelectedIndex, _sampleRate)
        _outputDeviceIdx = _outputAudioDevicesListBox.SelectedIndex
        _outputAudioDevicesRefreshButton.Text = _outputAudioDevicesListBox.Items(_outputDeviceIdx) + " / Refresh"
    End Sub

    Private Sub _inputAudioDevicesListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _inputAudioDevicesListBox.SelectedIndexChanged
        _captureOffButton_Click(sender, e)
        _capture = New WaveInSource(_inputAudioDevicesListBox.SelectedIndex, _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallSeconds) With {.SampleProcessor = AddressOf Me.SampleProcessor}
        _inputDeviceIdx = _inputAudioDevicesListBox.SelectedIndex
        _inputAudioDevicesRefreshButton.Text = _inputAudioDevicesListBox.Items(_inputDeviceIdx) + " / Refresh"
    End Sub

    Private Sub _captureOnButton_Click(sender As Object, e As EventArgs) Handles _captureOnButton.Click
        _waterfall.Reset()
        _capture.Start()
        _inputGroupBox.Text = "Input [ ON ]"
        _captureOnButton.BackColor = Me.BackColor
    End Sub

    Private Sub _captureOffButton_Click(sender As Object, e As EventArgs) Handles _captureOffButton.Click
        _capture.Stop()

        'FileName
        Dim snapshotFilename = DateTime.Now.ToString("dd.MM.yyyy__HH.mm.ss.ffff")

        'LOG
        If _dopplerLog.Items.Any() Then
            Dim logFilename = "dopplerLog__" + snapshotFilename + ".txt"
            Using logStream = File.OpenWrite(logFilename)
                _dopplerLog.Write(logStream)
                logStream.Flush()
            End Using
            _dopplerLog.Clear()
            Using logStreamR = File.OpenRead(logFilename)
                Dim dopplerLogTest As New DopplerLog()
                dopplerLogTest.Read(logStreamR)
                Using logStreamW = File.OpenWrite(logFilename.Replace(".txt", ".copy.txt"))
                    dopplerLogTest.Write(logStreamW)
                    logStreamW.Flush()
                End Using
            End Using
        End If

        'WATERFALL
        Dim waterfall = _waterfall.ToBitmap(_scale)
        If waterfall IsNot Nothing Then
            waterfall.Save("waterfall___" + snapshotFilename + ".jpg")
        End If
        _waterfall.Reset()

        'PCM
        If _dopplerPcm.Any() Then
            Dim wavFile As New Wave.WaveFileWriter("dopplerWav__" + snapshotFilename + ".wav", New Wave.WaveFormat(_sampleRate, 1))
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

    Private Sub _deadZoneTrackBar_Scroll(sender As Object, e As EventArgs) Handles _deadZoneTrackBar.Scroll
        _deadZoneLabel.Text = _deadZoneTrackBar.Value
    End Sub
End Class
