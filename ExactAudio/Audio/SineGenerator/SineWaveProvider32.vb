Imports NAudio.Wave

Public Class SineWaveProvider32
    Inherits WaveProvider32

    Private _sampleIdx As Integer
    Public Property Program As Queue(Of SineTaskBlock)

    Public Sub New()
    End Sub

    Public Sub New(program As Queue(Of SineTaskBlock))
        Me.Program = New Queue(Of SineTaskBlock)(program)
    End Sub

    Public Sub New(frequency As IEnumerable(Of Single))
        Me.Program = New Queue(Of SineTaskBlock)({New SineTaskBlock(frequency, frequency.Select(Function(item) 1.0F).ToArray())})
    End Sub

    Public Overrides Function Read(buffer As Single(), offset As Integer, sampleCount As Integer) As Integer
        Dim sampleRate As Integer = WaveFormat.SampleRate
        For i As Integer = 0 To sampleCount - 1
            buffer(i + offset) = 0
        Next
        For i As Integer = 0 To sampleCount - 1
            Dim pr = Me.Program.FirstOrDefault()
            If pr IsNot Nothing Then
                Dim ampSum = pr.Amplitude.Sum()
                For sineIdx = 0 To pr.Frequency.Count - 1
                    buffer(i + offset) += CSng((pr.Amplitude(sineIdx) / ampSum) * Math.Sin((2 * Math.PI * _sampleIdx * pr.Frequency(sineIdx)) / sampleRate))
                Next
                NextSample(sampleRate)
                If Not pr.NextSampleAllowed() Then
                    Me.Program.Dequeue()
                End If
            Else
                Return 0
            End If
        Next
        Return sampleCount
    End Function

    Private Sub NextSample(sampleRate As Integer)
        _sampleIdx += 1
        If _sampleIdx >= sampleRate Then
            _sampleIdx = 0
        End If
    End Sub
End Class
