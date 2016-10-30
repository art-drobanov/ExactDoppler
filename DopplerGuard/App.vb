Imports System.IO
Imports System.Threading
Imports Bwl.Framework
Imports ExactAudio

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
        Console.WriteLine(String.Format("DopplerGuard {0}", My.Application.Info.Version.ToString()))
        Console.WriteLine(String.Empty)
        With _exactDoppler
            .SwitchOnGen()
            Console.WriteLine(".SwitchOnGen()")
            .Start()
            Console.WriteLine(".Start()")
        End With
        Console.WriteLine("")
        While True
            Thread.Sleep(10)
        End While
    End Sub

    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorerResult) Handles _exactDoppler.PcmSamplesProcessed
        Dim logItem = motionExplorerResult.DopplerLogItem
        Dim logItemString = logItem.ToString()
        If motionExplorerResult.IsWarning Then
            Console.WriteLine(logItemString)
            File.AppendAllText(Path.Combine(_consoleAppBase.DataFolder, "DopplerGuard.log.txt"), logItemString + vbCrLf)
            _exactDoppler.DopplerLog.Clear()
        End If
    End Sub
End Module
