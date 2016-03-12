Imports NAudio.Wave
Imports Timer = System.Timers.Timer

''' <summary>
''' Стерео-генератор синусоидального сигнала
''' </summary>
Public Class Generator

    ''' <summary>
    ''' "Математический" генератор синусоидального сигнала
    ''' </summary>
    Private Class SineGenerator
        Public Const BitDepth = 24 '24

        Private _sineFreq As Integer
        Private _sampFreq As Integer
        Private _startPhase As Double
        Private _offset As Integer

        Public Sub New(sampFreq As Integer)
            _sampFreq = sampFreq
        End Sub

        Public Sub Reset()
            _sineFreq = -1
            _offset = 0
            _startPhase = 0
        End Sub

        Public Function Generate24BitsN(sineFreq As Integer, N As Integer) As Integer()
            Return ToNBits(Process(sineFreq, N), BitDepth)
        End Function

        Public Function Generate24BitsMs(sineFreq As Integer, durationInMs As Double) As Integer()
            Dim s As Double = durationInMs / 1000.0
            Dim n = CInt(Math.Floor(_sampFreq * s))
            Return ToNBits(Process(sineFreq, n), BitDepth)
        End Function

        Private Function Process(sineFreq As Integer, N As Integer) As Double()
            If _sineFreq <> sineFreq Then Reset()
            _sineFreq = sineFreq

            Dim gen = (2 * Math.PI * sineFreq) / _sampFreq
            Dim sineBlock = New Double(N - 1) {}
            Dim currentPhase As Double = 0
            For i As Integer = 0 To N - 1
                currentPhase = _startPhase + gen * (i + _offset)
                sineBlock(i) = Math.Sin(currentPhase)
            Next

            _startPhase = (currentPhase Mod (2 * Math.PI))
            _offset = 1
            Return sineBlock
        End Function
    End Class

    Private Const _bufferLengthInMs As Double = 2000 '2000
    Private _sineTimeToGenerateInMs As Double = _bufferLengthInMs \ 2 '2
    Private Const _minRemainToPlayInMs As Double = _bufferLengthInMs \ 2 '2
    Private Const _waveOutTimerIntervalInMs As Double = _minRemainToPlayInMs \ 10 '10

    Private _waveOut As WaveOut
    Private _waveFormat As WaveFormat
    Private _waveProvider As BufferedWaveProvider
    Private _waveOutTimer As Timer
    Private _waveOutChannelIdxs As Integer()

    Private _sampleRate As Integer
    Private _sineFreqL As Integer
    Private _sineFreqR As Integer

    Private _sineGenL As SineGenerator
    Private _sineGenR As SineGenerator

    Private _mix As Boolean

    Public ReadOnly SyncRoot As New Object

    Public Property Volume As Single
        Get
            Return _waveOut.Volume
        End Get
        Set(value As Single)
            If value < 0 Or value > 1 Then Throw New Exception("Volume < 0 Or Volume > 1")
            _waveOut.Volume = value
        End Set
    End Property

    Public Sub New(ByRef deviceNumber As Integer, sampleRate As Integer) 'DeviceNumber -> Out variable
        If deviceNumber < 0 Then deviceNumber = 0
        _sampleRate = sampleRate
        _waveFormat = New WaveFormat(_sampleRate, SineGenerator.BitDepth, 2)
        _waveProvider = New BufferedWaveProvider(_waveFormat) With {
                                                                        .DiscardOnBufferOverflow = True,
                                                                        .BufferDuration = TimeSpan.FromMilliseconds(_bufferLengthInMs)
                                                                   }

        Try
            _waveOut = New WaveOut() With {.DeviceNumber = deviceNumber}
            _waveOut.Init(_waveProvider)
        Catch
            For i = 0 To GetAudioDeviceNamesWaveOut().Length - 1
                Dim exc = False
                If i <> deviceNumber Then
                    Try
                        _waveOut = New WaveOut() With {.DeviceNumber = i}
                        _waveOut.Init(_waveProvider)
                        deviceNumber = i
                    Catch
                        _waveOut = Nothing
                        exc = True
                    End Try
                    If Not exc Then Exit For
                End If
            Next
        End Try

        If _waveOut Is Nothing Then
            Throw New Exception("Can't init output device")
        End If

        _sineGenL = New SineGenerator(_sampleRate)
        _sineGenR = New SineGenerator(_sampleRate)

        _waveOutTimer = New Timer(_waveOutTimerIntervalInMs)
    End Sub

    Public Sub SwitchOn(sineFreqL As Integer, sineFreqR As Integer, mix As Boolean)
        SyncLock SyncRoot
            _waveOutTimer.Stop()
            _waveOut.Stop()
            _waveProvider.ClearBuffer()
            _sineFreqL = sineFreqL
            _sineFreqR = sineFreqR
            _mix = mix

            AddWaveSamples(_sineTimeToGenerateInMs)
            AddHandler _waveOutTimer.Elapsed, AddressOf AddWaveSamples
            _waveOut.Play() : _waveOutTimer.Start()
        End SyncLock
    End Sub

    Public Sub SwitchOff()
        SyncLock SyncRoot
            _waveOutTimer.Stop() : RemoveHandler _waveOutTimer.Elapsed, AddressOf AddWaveSamples
            _waveOut.Stop()
        End SyncLock
        Threading.Thread.Sleep(_waveOutTimer.Interval)
        SyncLock SyncRoot
            _waveProvider.ClearBuffer()
        End SyncLock
    End Sub

    Public Sub Flash(sineFreqL As Integer, sineFreqR As Integer, mix As Boolean, milliseconds As Integer)
        SyncLock SyncRoot
            If milliseconds > _bufferLengthInMs Then
                Throw New Exception("milliseconds > _bufferLengthInMs")
            End If
            _waveOutTimer.Stop()
            _waveOut.Stop()
            _waveProvider.ClearBuffer()
            _sineFreqL = sineFreqL
            _sineFreqR = sineFreqR
            _mix = mix

            AddWaveSamples(milliseconds)
            _waveOut.Play()
        End SyncLock
    End Sub

    Public Sub Play(pcmSamples As Single(), stereo As Boolean)
        SyncLock SyncRoot
            If pcmSamples Is Nothing Then Return
            Dim samplesBytes = pcmSamples.Select(Function(item) CDbl(item)).ToArray().ToNBits(SineGenerator.BitDepth).ToByteArray24(Not stereo)
            _waveProvider.AddSamples(samplesBytes, 0, samplesBytes.Length)
            _waveOut.Play()
        End SyncLock
    End Sub

    Private Sub AddWaveSamples(sineTimeToGenerateInMs As Double)
        Dim remainToPlayInMs = (_waveProvider.BufferedDuration.TotalMilliseconds)
        If remainToPlayInMs < _minRemainToPlayInMs Then
            Dim sineIntsL As Integer()
            Dim sineIntsR As Integer()
            Dim sineL As Byte()
            Dim sineR As Byte()

            sineIntsL = _sineGenL.Generate24BitsMs(_sineFreqL, sineTimeToGenerateInMs)
            sineIntsR = _sineGenR.Generate24BitsMs(_sineFreqR, sineTimeToGenerateInMs)
            If _mix Then sineIntsL.MixWith(sineIntsR)
            sineL = sineIntsL.ToByteArray24(False)
            sineR = sineIntsR.ToByteArray24(False)

            Dim bytesPerSample As Integer = SineGenerator.BitDepth \ 8
            Dim sineSamplesCountL As Integer = sineL.Length \ bytesPerSample
            Dim sineSamplesCountR As Integer = sineR.Length \ bytesPerSample
            If sineSamplesCountL <> sineSamplesCountR Then Throw New Exception("sineSamplesCountL <> sineSamplesCountR")
            Dim sineSamplesCount = sineSamplesCountL

            Dim sineLR = New Byte((sineSamplesCount * bytesPerSample * 2) - 1) {}
            For sineSampleIdx = 0 To sineSamplesCount - 1
                Dim sineSourceOffset = sineSampleIdx * bytesPerSample
                Array.Copy(sineL, sineSourceOffset, sineLR, (sineSourceOffset * 2) + (0 * bytesPerSample), bytesPerSample)
                Array.Copy(sineR, sineSourceOffset, sineLR, (sineSourceOffset * 2) + (1 * bytesPerSample), bytesPerSample)
            Next sineSampleIdx

            _waveProvider.AddSamples(sineLR, 0, sineLR.Length)
        End If
    End Sub

    Private Sub AddWaveSamples(ByVal myObject As Object, ByVal myEventArgs As EventArgs)
        SyncLock SyncRoot
            If _waveOutTimer.Enabled Then
                AddWaveSamples(_sineTimeToGenerateInMs)
            End If
        End SyncLock
    End Sub
End Class
