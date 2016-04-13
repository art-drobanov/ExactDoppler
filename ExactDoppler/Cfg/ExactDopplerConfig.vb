Public Class ExactDopplerConfig
    Public Property InputDeviceIdx As Integer
    Public Property OutputDeviceIdx As Integer
    Public Property Volume As Single
    Public Property CenterFreq As Double
    Public Property BlindZone As Integer
    Public Property PcmOutput As Boolean
    Public Property ImageOutput As Boolean

    Public Sub New()
        Me.New(0, 0, 0.5, 21000, 70, True, True)
    End Sub

    Public Sub New(inputDeviceIdx As Integer, outputDeviceIdx As Integer, volume As Single,
                   centerFreq As Double, blindZone As Integer, pcmOutput As Boolean, imageOutput As Boolean)
        Me.InputDeviceIdx = inputDeviceIdx
        Me.OutputDeviceIdx = outputDeviceIdx
        Me.Volume = volume
        Me.CenterFreq = centerFreq
        Me.BlindZone = blindZone
        Me.PcmOutput = pcmOutput
        Me.ImageOutput = imageOutput
    End Sub
End Class
