Imports NAudio.Wave

''' <summary>
''' Поставщик семплов для аудиовывода генератора
''' </summary>
Public Class ProgrammedSineWaveProvider32
    Inherits WaveProvider32

    Private _sampleIdx As Integer

    ''' <summary>
    ''' "Программа", управляющая генератором.
    ''' </summary>
    Public Property Program As Queue(Of SineTaskBlock)

    Public Sub New()
    End Sub

    Public Sub New(program As Queue(Of SineTaskBlock))
        _Program = New Queue(Of SineTaskBlock)(program)
    End Sub

    Public Sub New(frequencies As IEnumerable(Of Single))
        _Program = New Queue(Of SineTaskBlock)({New SineTaskBlock(frequencies, frequencies.Select(Function(item) 1.0F).ToArray())})
    End Sub

    Public Overrides Function Read(buffer As Single(), offset As Integer, sampleCount As Integer) As Integer
        Dim sampleRate = MyBase.WaveFormat.SampleRate
        Dim nChannels = MyBase.WaveFormat.Channels
        sampleCount \= nChannels
        For sampleIdx = 0 To sampleCount - 1
            Dim program = _Program.FirstOrDefault()
            If program IsNot Nothing Then
                Dim ampSum = program.Amplitudes.Sum()
                Dim sample As Single = 0
                For sineIdx = 0 To program.Frequencies.Count - 1
                    sample += CSng((program.Amplitudes(sineIdx) / ampSum) * Math.Sin((2 * Math.PI * _sampleIdx * program.Frequencies(sineIdx)) / sampleRate))
                Next
                For channel = 0 To nChannels - 1
                    buffer(((nChannels * sampleIdx) + channel) + offset) = sample
                Next
                NextSample(sampleRate)
                If Not program.NextSampleAllowed() Then
                    _Program.Dequeue()
                End If
            Else
                Return 0
            End If
        Next
        Return sampleCount * nChannels
    End Function

    Private Sub NextSample(sampleRate As Integer)
        _sampleIdx += 1
        If _sampleIdx >= sampleRate Then
            _sampleIdx = 0
        End If
    End Sub
End Class
