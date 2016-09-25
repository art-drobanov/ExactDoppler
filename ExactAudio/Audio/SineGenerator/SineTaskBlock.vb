Public Class SineTaskBlock
    Private _samplesTotal As ULong
    Private _samplesGenerated As ULong
    Public Property Frequency As List(Of Single)
    Public Property Amplitude As List(Of Single)

    Public Sub New(frequency As IEnumerable(Of Single), amplitude As IEnumerable(Of Single), Optional samplesTotal As ULong = ULong.MaxValue)
        _Frequency = New List(Of Single)(frequency)
        _Amplitude = New List(Of Single)(amplitude)
        _samplesTotal = samplesTotal
        _samplesGenerated = 0
    End Sub

    Public Function NextSampleAllowed() As Boolean
        _samplesGenerated += 1
        Return _samplesGenerated < _samplesTotal
    End Function
End Class
