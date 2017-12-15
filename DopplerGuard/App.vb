Imports System.IO
Imports System.Threading
Imports Bwl.Framework
Imports Bwl.Imaging
Imports ExactAudio

Module App
    Private _consoleAppBase As ConsoleAppBase
    Private _logTime As DateTime
    Private _cfg As ExactDopplerConfigFromStorage

    Private WithEvents _exactDopplerProcessor As ExactDopplerProcessor = New ExactDopplerProcessor()

    Sub Main()
        _consoleAppBase = New ConsoleAppBase()
        _cfg = New ExactDopplerConfigFromStorage(_consoleAppBase.RootStorage)
        _cfg.Load()

        _exactDopplerProcessor.ExactDoppler.Config = _cfg

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
        With _exactDopplerProcessor.ExactDoppler
            .SwitchOnGen()
            Console.WriteLine(".SwitchOnGen()")
            .Start()
            Console.WriteLine(".Start()")
        End With
        Console.WriteLine(String.Format("Input [ ON ] at device with idx '{0}'", _exactDopplerProcessor.ExactDoppler.InputDeviceIdx))
        Console.WriteLine(String.Format("Output [ ON AIR! ] at device with idx '{0}'", _exactDopplerProcessor.ExactDoppler.OutputDeviceIdx))
        Console.WriteLine("")

        While True
            Thread.Sleep(10)
        End While
    End Sub

    Private Sub SamplesProcessedHandler(res As ExactDopplerProcessor.Result) Handles _exactDopplerProcessor.PcmSamplesProcessed
        Dim logItem = res.DopplerLogItem
        Dim logItemString = logItem.ToString()
        If res.IsWarning Then
            Console.WriteLine(logItemString)
            File.AppendAllText(Path.Combine(_consoleAppBase.DataFolder, "DopplerGuard.log.txt"), logItemString + vbCrLf)
            _exactDopplerProcessor.ExactDoppler.DopplerLog.Clear()
        End If
    End Sub

    Private Sub Alarm(alarm As AlarmManager.Result) Handles _exactDopplerProcessor.Alarm
        _exactDopplerProcessor.AlarmManager.WriteAlarmData("Alarm", alarm)
        Console.WriteLine()
        Console.WriteLine(String.Format("Alarm, {0}", alarm.AlarmStartTime.ToString("yyyy-MM-dd HH:mm:ss.ffff zzz")))
        Console.WriteLine()
    End Sub

    Private Sub AlarmRecorded(alarm As AlarmManager.Result) Handles _exactDopplerProcessor.AlarmRecorded
        _exactDopplerProcessor.AlarmManager.WriteAlarmData("AlarmRecorded", alarm)
        Console.WriteLine()
        Console.WriteLine(String.Format("AlarmRecorded, alarm start time: {0}", alarm.AlarmStartTime.ToString("yyyy-MM-dd HH:mm:ss.ffff zzz")))
        Console.WriteLine()
    End Sub
End Module
