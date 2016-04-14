Public Class ExactDopplerConfig
    Public Property InputDeviceIdx As Integer
    Public Property OutputDeviceIdx As Integer
    Public Property Volume As Single
    Public Property CenterFreq As Double
    Public Property BlindZone As Integer

    Public Sub New()
        Me.New(0, 0, 0.5, 21000, 70)
    End Sub

    Public Sub New(inputDeviceIdx As Integer, outputDeviceIdx As Integer, volume As Single,
                   centerFreq As Double, blindZone As Integer)
        Me.InputDeviceIdx = inputDeviceIdx
        Me.OutputDeviceIdx = outputDeviceIdx
        Me.Volume = volume
        Me.CenterFreq = centerFreq
        Me.BlindZone = blindZone
    End Sub
End Class
