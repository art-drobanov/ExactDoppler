Public Class SineTaskBlock
    Private _samplesTotal As ULong
    Private _samplesGenerated As ULong
    Public Property Frequencies As List(Of Single)
    Public Property Amplitudes As List(Of Single)

    Public Sub New(frequencies As IEnumerable(Of Single), amplitudes As IEnumerable(Of Single),
                   Optional samplesTotal As ULong = ULong.MaxValue)
        _Frequencies = New List(Of Single)(frequencies)
        _Amplitudes = New List(Of Single)(amplitudes)
        _samplesTotal = samplesTotal
        _samplesGenerated = 0
    End Sub

    Public Function NextSampleAllowed() As Boolean
        _samplesGenerated += 1
        Return _samplesGenerated < _samplesTotal
    End Function
End Class
