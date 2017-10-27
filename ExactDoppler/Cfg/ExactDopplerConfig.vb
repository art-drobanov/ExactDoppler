''' <summary>
''' Конфигурация ExactDoppler
''' </summary>
Public Class ExactDopplerConfig
    Public Property InputDeviceIdx As Integer
    Public Property OutputDeviceIdx As Integer
    Public Property Volume As Single
    Public Property CenterFreqs As Double()
    Public Property BlindZone As Integer
    Public Property CarrierWarningLevel As Integer

    Public Sub New()
        Me.New(0, 0, 0.5, {21000}, 60, 10)
    End Sub

    Public Sub New(inputDeviceIdx As Integer, outputDeviceIdx As Integer, volume As Single,
                   centerFreqs As IEnumerable(Of Double), blindZone As Integer, carrierWarningLevel As Integer)
        _InputDeviceIdx = inputDeviceIdx
        _OutputDeviceIdx = outputDeviceIdx
        _Volume = volume
        _CenterFreqs = centerFreqs.ToArray()
        _BlindZone = blindZone
        _CarrierWarningLevel = carrierWarningLevel
    End Sub
End Class
