Imports NAudio.Wave
Imports DrAF.DSP

Public Delegate Sub SampleProcessorDelegate(samples As Single(), samplesCount As Integer)

Public Class WaveInSource
    Private _started As Boolean
    Private _waveFormat As WaveFormat
    Private WithEvents _waveIn As WaveInEvent

    Public SampleProcessor As SampleProcessorDelegate

    Public Sub New(deviceNumber As Integer, sampleRate As Integer, bitDepth As Integer, stereo As Boolean, minSamplesCountInBlock As Integer)
        _started = False
        _waveFormat = New WaveFormat(sampleRate, bitDepth, If(stereo, 2, 1))
        _waveIn = New WaveInEvent With {
                                          .DeviceNumber = deviceNumber,
                                          .WaveFormat = _waveFormat,
                                          .BufferMilliseconds = Math.Ceiling(minSamplesCountInBlock / CDbl(sampleRate)) * 1000,
                                          .NumberOfBuffers = 3
                                       }
    End Sub

    Public Sub Start()
        If Not _started Then
            SyncLock _waveIn
                _waveIn.StartRecording()
                _started = True
            End SyncLock
        End If
    End Sub

    Public Sub [Stop]()
        If _started Then
            SyncLock _waveIn
                _waveIn.StopRecording()
                _started = False
            End SyncLock
        End If
    End Sub

    Private Sub WaveDataAvailable(sender As Object, e As WaveInEventArgs) Handles _waveIn.DataAvailable
        Dim samples As Single() = Nothing
        Dim maxValue As Single = Math.Pow(2, _waveFormat.BitsPerSample - 1)

        Select Case _waveFormat.BitsPerSample
            Case 16
                samples = New Single((e.Buffer.Length \ 2) - 1) {}
                Parallel.For(0, samples.Length, Sub(bufferIdx)
                                                    Dim intSample = BitConverter.ToInt16(e.Buffer, bufferIdx * 2)
                                                    samples(bufferIdx) = intSample / maxValue
                                                End Sub)
            Case 24
                samples = New Single((e.Buffer.Length \ 3) - 1) {}
                Parallel.For(0, samples.Length, Sub(bufferIdx)
                                                    Dim intSample = Me.ToInt24(e.Buffer, bufferIdx * 3)
                                                    samples(bufferIdx) = intSample / maxValue
                                                End Sub)
            Case Else
                Throw New Exception("Supported recording modes: 16 and 24 bit per sample only!")
        End Select

        If SampleProcessor IsNot Nothing AndAlso samples IsNot Nothing Then
            SampleProcessor(samples, samples.Length / _waveFormat.Channels)
        End If
    End Sub

    Private Function ToInt24(data As Byte(), offset As Integer) As Int32
        Dim result = New Byte(3) {data(offset), data(offset + 1), data(offset + 2), &H0}
        Return BitConverter.ToInt32(result, 0)
    End Function
End Class
