Imports Bwl.Framework
Imports System.Threading
Imports ExactAudio
Imports System.IO

Module App
    Private _consoleAppBase As ConsoleAppBase
    Private _logTime As DateTime
    Private _cfg As ExactDopplerConfigFromStorage
    Private WithEvents _exactDoppler As ExactDoppler

    Sub Main()
        _consoleAppBase = New ConsoleAppBase()
        _cfg = New ExactDopplerConfigFromStorage(_consoleAppBase.RootStorage)
        _cfg.Load()
        _exactDoppler = New ExactDoppler(_cfg)
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
        Dim logItem = motionExplorerResult.DopplerLogItem
        Dim logString = logItem.ToString()
        If motionExplorerResult.IsWarning Then
            Console.WriteLine(logString)
            File.AppendAllText(Path.Combine(_consoleAppBase.DataFolder, "ExactDopplerServer.txt"), logString + vbCrLf)
            _exactDoppler.DopplerLog.Clear()
        End If
    End Sub
End Module
