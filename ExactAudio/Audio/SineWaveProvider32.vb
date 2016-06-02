Imports NAudio.Wave

Public Class SineWaveProvider32
    Inherits WaveProvider32

    Private _sample As Integer

    Public Sub New()
        Frequency = 1000
        Amplitude = 0.25F
    End Sub

    Public Property Frequency() As Single
    Public Property Amplitude() As Single

    Public Overrides Function Read(buffer() As Single, offset As Integer, sampleCount As Integer) As Integer
        Dim sampleRate As Integer = WaveFormat.SampleRate
        For n As Integer = 0 To sampleCount - 1
            buffer(n + offset) = CSng(Amplitude * Math.Sin((2 * Math.PI * _sample * Frequency) / sampleRate))
            _sample += 1
            If _sample >= sampleRate Then
                _sample = 0
            End If
        Next
        Return sampleCount
    End Function
End Class
