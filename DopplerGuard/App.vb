Imports System.IO
Imports System.Threading
Imports Bwl.Framework
Imports Bwl.Imaging
Imports ExactAudio

Module App
    Private _consoleAppBase As ConsoleAppBase
    Private _logTime As DateTime
    Private _cfg As ExactDopplerConfigFromStorage
    Private WithEvents _exactDoppler As ExactDoppler
    Private WithEvents _alarmManager As AlarmManager

    Sub Main()
        _consoleAppBase = New ConsoleAppBase()
        _cfg = New ExactDopplerConfigFromStorage(_consoleAppBase.RootStorage)
        _cfg.Load()
        _exactDoppler = New ExactDoppler(_cfg)
        _alarmManager = New AlarmManager(_exactDoppler)
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine(String.Format("DopplerGuard {0}", My.Application.Info.Version.ToString()))
        Console.WriteLine(String.Empty)
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine(" High power ultrasound may damage the tweeter,                                     " + vbCrLf +
                          " and also may damage your hearing.                                                 " + vbCrLf +
                          " By pressing 'Yes' i confirm, that:                                                " + vbCrLf +
                          " I'm sure that the chosen output is not connected to the headphones.               " + vbCrLf +
                          " I'm sure that the chosen output is not connected to the expensive speaker system. " + vbCrLf +
                          " The audio output device transmits the signal to the built-in laptop speakers      " + vbCrLf +
                          " or to the cheap USB-speakers equipped with one speaker per channel.               ")
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine()
        With _exactDoppler
            .SwitchOnGen()
            Console.WriteLine(".SwitchOnGen()")
            .Start()
            Console.WriteLine(".Start()")
        End With
        Console.WriteLine(String.Format("Input [ ON ] at device with idx '{0}'", _exactDoppler.InputDeviceIdx))
        Console.WriteLine(String.Format("Output [ ON AIR! ] at device with idx '{0}'", _exactDoppler.OutputDeviceIdx))
        Console.WriteLine("")
        While True
            Thread.Sleep(10)
        End While
    End Sub

    Private Sub SamplesProcessedHandler(motionExplorerResult As MotionExplorerResult) Handles _alarmManager.PcmSamplesProcessed
        Dim logItem = motionExplorerResult.DopplerLogItem
        Dim logItemString = logItem.ToString()
        If motionExplorerResult.IsWarning Then
            Console.WriteLine(logItemString)
            File.AppendAllText(Path.Combine(_consoleAppBase.DataFolder, "DopplerGuard.log.txt"), logItemString + vbCrLf)
            _exactDoppler.DopplerLog.Clear()
        End If
    End Sub

    Private Sub Alarm(rawDopplerImage As RGBMatrix, dopplerImage As RGBMatrix, lowpassAudio As Single(), alarmStartTime As DateTime) Handles _alarmManager.Alarm
        _alarmManager.Save("Alarm", rawDopplerImage, dopplerImage, lowpassAudio)
        Console.WriteLine()
        Console.WriteLine(String.Format("Alarm, {0}", alarmStartTime.ToString("yyyy-MM-dd HH:mm:ss.ffff zzz")))
        Console.WriteLine()
    End Sub

    Private Sub AlarmRecorded(rawDopplerImage As RGBMatrix, dopplerImage As RGBMatrix, lowpassAudio As Single(), alarmStartTime As DateTime) Handles _alarmManager.AlarmRecorded
        _alarmManager.Save("AlarmRecorded", rawDopplerImage, dopplerImage, lowpassAudio)
        Console.WriteLine()
        Console.WriteLine(String.Format("AlarmRecorded, alarm start time: {0}", alarmStartTime.ToString("yyyy-MM-dd HH:mm:ss.ffff zzz")))
        Console.WriteLine()
    End Sub
End Module
