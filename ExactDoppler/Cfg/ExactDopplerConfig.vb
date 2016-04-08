Public Class ExactDopplerConfig
    Public Property CenterFreq As Double
    Public Property BlindZone As Integer
    Public Property DisplayLeft As Boolean
    Public Property DisplayRightWithLeft As Boolean
    Public Property DisplayCenter As Boolean
    Public Property DisplayRight As Boolean
    Public Property PcmOutput As Boolean
    Public Property ImageOutput As Boolean

    Public Sub New()
        Me.New(21000, 70, True, False, True, True, True, True)
    End Sub

    Public Sub New(centerFreq As Double, blindZone As Integer, displayLeft As Boolean,
                   displayRightWithLeft As Boolean, displayCenter As Boolean, displayRight As Boolean,
                   pcmOutput As Boolean, imageOutput As Boolean)
        Me.CenterFreq = centerFreq
        Me.BlindZone = blindZone
        Me.DisplayLeft = displayLeft
        Me.DisplayRightWithLeft = displayRightWithLeft
        Me.DisplayCenter = displayCenter
        Me.DisplayRight = displayRight
        Me.PcmOutput = pcmOutput
        Me.ImageOutput = imageOutput
    End Sub
End Class
