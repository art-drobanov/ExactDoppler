Imports Bwl.Framework
Imports System.Threading
Imports ExactAudio
Imports ExactAudio.MotionExplorer
Imports System.IO

Module App
    Private _logTime As DateTime
    Private _pcmLog As PcmLog
    Private _waterfall As RGBWaterfall

    Private _cfg As ExactDopplerConfigFromStorage
    Private WithEvents _exactDoppler As ExactDoppler

    Private _consoleAppBase As ConsoleAppBase

    Sub Main()
        _consoleAppBase = New ConsoleAppBase()
        _cfg = New ExactDopplerConfigFromStorage(_consoleAppBase.RootStorage)
        _cfg.Load()
        _exactDoppler = New ExactDoppler(_cfg)
        _pcmLog = New PcmLog(_exactDoppler.SampleRate)
        _waterfall = New RGBWaterfall()

        Console.WriteLine("ExactDoppler Server")
        Console.WriteLine("")
        With _exactDoppler
            .SwitchOnGen() : Console.WriteLine(".SwitchOnGen()")
            .Start() : Console.WriteLine(".Start()")
        End With
        Console.WriteLine("")
        While True
            Thread.Sleep(10)
        End While
    End Sub

    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorerResult) Handles _exactDoppler.PcmSamplesProcessed
        If _exactDoppler.DopplerLog.Items.Any() Then
            Dim logData = _exactDoppler.DopplerLog.Items.Last.Value
            If _logTime <> logData.Time Then
                _logTime = logData.Time
                Dim logString = logData.ToString()
                Console.WriteLine(logString)

                _pcmLog.Add(motionExplorerResult.Pcm)
                _waterfall.Add(motionExplorerResult.Image)

                File.WriteAllText(Path.Combine(_consoleAppBase.DataFolder, logData.Time.ToString("yyyy.MM.dd__HH..mm..ss") & ".txt"), logString)
                _pcmLog.Write(Path.Combine(_consoleAppBase.DataFolder, logData.Time.ToString("yyyy.MM.dd__HH..mm..ss") & ".wav"))
                _waterfall.Write(Path.Combine(_consoleAppBase.DataFolder, logData.Time.ToString("yyyy.MM.dd__HH..mm..ss") & ".png"))

                _exactDoppler.DopplerLog.Clear()
                _pcmLog.Clear()
                _waterfall.Clear()
            End If
        End If
    End Sub
End Module
