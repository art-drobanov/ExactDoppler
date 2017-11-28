Imports NAudio.Wave

Public Delegate Sub SampleProcessorDelegate(samples As Single(), samplesCount As Integer, timestamp As DateTime)

''' <summary>
''' Базовый аудиоисточник
''' </summary>
Public MustInherit Class WaveSource
    Protected _started As Boolean
    Protected _waveFormat As WaveFormat
    Protected _minSamplesCountInBlock As Integer

    Public SampleProcessor As SampleProcessorDelegate

    Public MustOverride ReadOnly Property Name As String

    Public Sub New(sampleRate As Integer, bitDepth As Integer, stereo As Boolean, minSamplesCountInBlock As Integer)
        _started = False
        _waveFormat = New WaveFormat(sampleRate, bitDepth, If(stereo, 2, 1))
        If _waveFormat.BitsPerSample <> 16 AndAlso _waveFormat.BitsPerSample <> 24 Then
            Throw New Exception(String.Format("{0}: Wrong bit depth!", TypeName(Me)))
        End If
        _minSamplesCountInBlock = minSamplesCountInBlock
    End Sub

    Public MustOverride Sub Start()
    Public MustOverride Sub [Stop]()

    Protected Sub WaveDataAvailableBase(e As WaveInEventArgs)
        Dim timestamp = DateTime.Now
        Dim samples As Single() = Nothing
        Dim maxValue As Single = Math.Pow(2, _waveFormat.BitsPerSample - 1)
        Select Case _waveFormat.BitsPerSample
            Case 16
                samples = New Single((e.Buffer.Length \ 2) - 1) {}
                Parallel.For(0, samples.Length, Sub(bufferIdx As Integer)
                                                    Dim intSample = BitConverter.ToInt16(e.Buffer, bufferIdx * 2)
                                                    samples(bufferIdx) = intSample / maxValue
                                                End Sub)
            Case 24
                samples = New Single((e.Buffer.Length \ 3) - 1) {}
                Parallel.For(0, samples.Length, Sub(bufferIdx As Integer)
                                                    Dim intSample = BytesToInt24(e.Buffer, bufferIdx * 3)
                                                    samples(bufferIdx) = intSample / maxValue
                                                End Sub)
            Case Else
                Throw New Exception(String.Format("{0}: Wrong bit depth!", TypeName(Me)))
        End Select

        If SampleProcessor IsNot Nothing AndAlso samples IsNot Nothing Then
            SampleProcessor(samples, samples.Length / _waveFormat.Channels, timestamp)
        End If
    End Sub

    Private Function BytesToInt24(data As Byte(), offset As Integer) As Int32
        Dim buffer = {data(offset + 0), data(offset + 1), data(offset + 2)}
        Dim value As Integer = 0
        For i = buffer.Length - 1 To 0 Step -1
            value += (CInt(buffer(i)) << (i << 3))
        Next i
        If (buffer(buffer.Length - 1) And &H80) = &H80 Then
            value = value Or (&HFFFFFF << (buffer.Length << 3))
        End If
        Return value
    End Function
End Class
