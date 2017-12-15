Imports System.IO
Imports System.Threading
Imports NAudio.Wave

''' <summary>
''' Аудиоисточник на основе Wav-файла
''' </summary>
Public Class WaveFileSource
    Inherits WaveSource
    Implements IWaveSource

    Private _fileName As String
    Private _sampleSize As Integer
    Private _bufferSize As Integer
    Private _wavBegin As DateTime

    Private _startTime As DateTime
    Private _bytesRead As Integer

    Private _waveFile As WaveFileReader
    Private _thr As Thread

    Private _syncRoot As New Object

    Public ReadOnly Property LengthS As Double
        Get
            Return (_waveFile.Length / (_sampleSize * _waveFormat.Channels)) / _waveFormat.SampleRate
        End Get
    End Property

    Public ReadOnly Property PositionS As Double
        Get
            Return If(_bytesRead > 0, (_bytesRead / (_sampleSize * _waveFormat.Channels)) / _waveFormat.SampleRate, 0)
        End Get
    End Property

    Public ReadOnly Property RealPlaybackSpeedX As Double
        Get
            Dim captureTime = (Now - _startTime).TotalSeconds
            Return If(PositionS > 0, PositionS / captureTime, PlaybackSpeedX)
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String Implements IWaveSource.Name
        Get
            Return _fileName
        End Get
    End Property

    Public Property PlaybackSpeedX As Single = 1
    Public Property FastMode As Boolean

    Public Event Stopped() Implements IWaveSource.Stopped

    Public Sub New(fileName As String, sampleRate As Integer, bitDepth As Integer, stereo As Boolean, minSampleCountInBlock As Integer)
        MyBase.New(sampleRate, bitDepth, stereo, minSampleCountInBlock)
        If Not File.Exists(fileName) Then
            Throw New Exception(String.Format("{0}: File '{1}' does not exists!", TypeName(Me), fileName))
        End If
        _fileName = fileName
        Dim wavBytes = File.ReadAllBytes(_fileName)
        _waveFile = New WaveFileReader(New MemoryStream(wavBytes)) With {.Position = 0}
        If _waveFile.WaveFormat.SampleRate <> _waveFormat.SampleRate Then
            Throw New Exception(String.Format("{0}: Wav sample rate differs from desired!", TypeName(Me)))
        End If
        If _waveFile.WaveFormat.Channels <> _waveFormat.Channels Then
            Throw New Exception(String.Format("{0}: Wav channel count differs from desired!", TypeName(Me)))
        End If
        _sampleSize = _waveFormat.BitsPerSample >> 3
        _bufferSize = _minSampleCountInBlock * _sampleSize * _waveFormat.Channels
        _wavBegin = New FileInfo(_fileName).LastWriteTime.AddSeconds(-1 * LengthS)
    End Sub

    Public Shadows Sub SetSampleProcessor(sampleProcessor As SampleProcessorDelegate) Implements IWaveSource.SetSampleProcessor
        MyBase.SetSampleProcessor(sampleProcessor)
    End Sub

    Public Overrides Sub Start() Implements IWaveSource.Start
        SyncLock _syncRoot
            If Not _started Then
                Rewind()
                _started = True
                If _thr Is Nothing Then
                    _thr = New Thread(AddressOf CaptureThread) With {.IsBackground = True}
                    _thr.Start()
                End If
            End If
        End SyncLock
    End Sub

    Public Overrides Sub [Stop]() Implements IWaveSource.Stop
        SyncLock _syncRoot
            _started = False
            If _thr IsNot Nothing Then
                _thr = Nothing
            End If
        End SyncLock
    End Sub

    Private Sub CaptureThread()
        _startTime = Now
        While True
            If Not _started Then
                RaiseEvent Stopped()
                Return
            End If

            Dim pcmBytes = New Byte(_bufferSize - 1) {}
            Dim bytesRead = _waveFile.Read(pcmBytes, 0, _bufferSize)
            Interlocked.Add(_bytesRead, bytesRead)
            If bytesRead = _bufferSize Then
                Dim waveDataTime = _wavBegin.AddSeconds(PositionS)
                Dim e = New WaveInEventArgs(pcmBytes, bytesRead)
                MyBase.WaveDataAvailableBase(e, waveDataTime)
            Else
                _started = False
            End If

            While PlaybackSpeedX <= 0
                Thread.Sleep(1)
                If Not _started Then
                    RaiseEvent Stopped()
                    Return
                End If
            End While

            If Not FastMode Then
                Dim totalCaptureTimeS = (Now - _startTime).TotalSeconds
                Dim totalSamplesForOneX = _waveFormat.SampleRate * totalCaptureTimeS * PlaybackSpeedX
                Dim samplesOverprocessed = (_bytesRead / (_sampleSize * _waveFormat.Channels)) - totalSamplesForOneX
                Dim timeToSleepInS = samplesOverprocessed / _waveFormat.SampleRate
                If timeToSleepInS > 0 Then
                    Dim timeToWakeUp = Now.AddSeconds(timeToSleepInS)
                    While Now < timeToWakeUp
                        Dim timeToSleepNowMs = (timeToWakeUp - Now).TotalMilliseconds
                        If timeToSleepNowMs > 0 Then
                            timeToSleepNowMs = If(timeToSleepNowMs > 1000, 1000, timeToSleepNowMs)
                            Thread.Sleep(timeToSleepNowMs)
                        End If
                        If FastMode Then
                            Exit While
                        End If
                    End While
                End If
            End If
        End While
    End Sub

    Private Sub Rewind()
        _bytesRead = 0
        _waveFile.Position = 0
    End Sub
End Class
