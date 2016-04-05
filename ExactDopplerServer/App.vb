Imports System.Threading
Imports ExactAudio
Imports ExactAudio.MotionExplorer

Module App
    Private WithEvents _exactDoppler As New ExactDoppler()
    Private _logTime As DateTime

    Sub Main()
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
            End If
        End If
    End Sub
End Module
