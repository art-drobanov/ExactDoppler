Imports System.IO
Imports System.Threading
Imports ExactAudio

Public Class MainForm
    Private WithEvents _exactDopplerProcessor As ExactDopplerProcessor = New ExactDopplerProcessor()

    Public Sub New()
        InitializeComponent()
        Me.Text += " " + My.Application.Info.Version.ToString()
        With _exactDopplerProcessor
            .WaterfallDisplaySize = _waterfallDisplayBitmapControl.Size
            .ExactDoppler.FastMode = True
        End With
    End Sub

    Private Sub _openWavButton_Click(sender As Object, e As EventArgs) Handles _openWavButton.Click
        _captureOffButton_Click(sender, e)
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
                _wavFileGroupBox.Text = String.Format("Wav File: {0}", ofd.FileName)
            Catch ex As Exception
                _exactDopplerProcessor.ExactDoppler.InputWavFile = String.Empty
                _wavFileGroupBox.Text = "Wav File:"
                MessageBox.Show(ex.Message)
            End Try
        Else
            _exactDopplerProcessor.ExactDoppler.InputWavFile = String.Empty
            _wavFileGroupBox.Text = "Wav File:"
        End If
    End Sub

    Private Sub _captureOnButton_Click(sender As Object, e As EventArgs) Handles _processButton.Click
        _exactDopplerProcessor.WaterfallDisplay.Clear()
        With _exactDopplerProcessor
            .AlarmManager.Reset()
            If Not .ExactDoppler.Start(True) Then 'Указываем, что должен использоваться файловый режим
                Dim thr = New Thread(Sub()
                                         _captureOffButton_Click(sender, e)
                                     End Sub) With {.IsBackground = True}
                thr.Start()
            End If
        End With
        _processButton.BackColor = Me.BackColor
    End Sub

    Private Sub _captureOffButton_Click(sender As Object, e As EventArgs) Handles _captureOffButton.Click
        _exactDopplerProcessor.Stop()
        _processButton.BackColor = Color.MediumSpringGreen
    End Sub

    Private Sub _fastModeCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _fastModeCheckBox.CheckedChanged
        _exactDopplerProcessor.ExactDoppler.FastMode = _fastModeCheckBox.Checked
    End Sub

    Private Sub PcmSamplesProcessed(res As ExactDopplerProcessor.Result) Handles _exactDopplerProcessor.PcmSamplesProcessed
        Me.Invoke(Sub()
                      'WaterfallShort
                      Dim bmpRaw = res.WaterfallDisplayRaw
                      If bmpRaw IsNot Nothing Then
                          With _waterfallDisplayRawBitmapControl
                              .DisplayBitmap.DrawBitmap(bmpRaw)
                              .Refresh()
                          End With
                      End If

                      Dim bmp = res.WaterfallDisplay
                      If bmp IsNot Nothing Then
                          With _waterfallDisplayBitmapControl
                              .DisplayBitmap.DrawBitmap(bmp)
                              .Refresh()
                          End With
                      End If

                      'GUI
                      Dim wavSource = CType(_exactDopplerProcessor.ExactDoppler.WavSource, WaveFileSource)
                      Dim positionPerc = wavSource.PositionS / wavSource.LengthS
                      _wavPositionTrackBar.Value = 1000 * positionPerc

                      Dim dopplerLogItem = res.DopplerLogItem.ToString()
                      _dopplerLogTextBox.Lines = {dopplerLogItem}
                      _speedXLabel.Text = res.SpeedX.ToString("F1")

                      'Alarm
                      _alarmLabel.Text = String.Format("ALARM: {0}", _exactDopplerProcessor.AlarmManager.AlarmCounter)
                      If res.AlarmDetected Then
                          _alarmLabel.BackColor = Color.Red
                      Else
                          _alarmLabel.BackColor = Color.DeepSkyBlue
                      End If
                  End Sub)
    End Sub

    Private Sub WaterfallsAreFull(waterfallFullRaw As DopplerWaterfall, waterfallFull As DopplerWaterfall) Handles _exactDopplerProcessor.WaterfallsAreFull
        _exactDopplerProcessor.WriteDopplerDataAndClear()
    End Sub

    Private Sub Alarm(alarm As AlarmManager.Result) Handles _exactDopplerProcessor.Alarm
        Me.BeginInvoke(Sub()
                           _exactDopplerProcessor.AlarmManager.WriteAlarmData("Alarm", alarm)
                       End Sub)
    End Sub

    Private Sub AlarmRecorded(alarm As AlarmManager.Result) Handles _exactDopplerProcessor.AlarmRecorded
        Me.BeginInvoke(Sub()
                           _alarmLabel.BackColor = Color.DeepSkyBlue
                           _exactDopplerProcessor.AlarmManager.WriteAlarmData("AlarmRecorded", alarm)
                       End Sub)
    End Sub

    Private Sub WaveSourceStopped() Handles _exactDopplerProcessor.WaveSourceStopped
        Me.Invoke(Sub()
                      _captureOffButton_Click(Nothing, Nothing)
                  End Sub)
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
End Class
