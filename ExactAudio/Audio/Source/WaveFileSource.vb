Imports System.IO
Imports System.Threading
Imports NAudio.Wave

''' <summary>
''' Аудиоисточник на основе Wav-файла
''' </summary>
Public Class WaveFileSource
    Inherits WaveSource

    Private _fileName As String
    Private _sampleSize As Integer
    Private _bufferSize As Integer
    Private _bytesRead As Integer
    Private _startTime As DateTime
    Private _totalSamplesForOneX As Single
    Private _waveFile As WaveFileReader
    Private _thr As Thread
    Private _syncRoot As New Object

    Public ReadOnly Property SpeedX As Double
        Get
            Dim t = (Now - _startTime).TotalSeconds
            Dim s = (_bytesRead / (_sampleSize * _waveFormat.Channels)) / _waveFormat.SampleRate
            Return s / t
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return _fileName
        End Get
    End Property

    Public Property PlaybackSpeed As Single = 1
    Public Property FastMode As Boolean

    Public Sub New(fileName As String, sampleRate As Integer, bitDepth As Integer, stereo As Boolean, minSampleCountInBlock As Integer)
        MyBase.New(sampleRate, bitDepth, stereo, minSampleCountInBlock)
        _fileName = fileName
        _sampleSize = _waveFormat.BitsPerSample >> 3
        _bufferSize = _minSampleCountInBlock * _sampleSize * _waveFormat.Channels
        Dim wavBytes = File.ReadAllBytes(fileName)
        _waveFile = New WaveFileReader(New MemoryStream(wavBytes)) With {.Position = 0}
        If _waveFile.WaveFormat.SampleRate <> _waveFormat.SampleRate Then
            Throw New Exception(String.Format("{0}: Wav sample rate differs from desired!", TypeName(Me)))
        End If
        If _waveFile.WaveFormat.Channels <> _waveFormat.Channels Then
            Throw New Exception(String.Format("{0}: Wav channel count differs from desired!", TypeName(Me)))
        End If
    End Sub

    Public Overrides Sub Start()
        If Not _started Then
            Rewind()
            _started = True
            SyncLock _syncRoot
                If _thr Is Nothing Then
                    _thr = New Thread(AddressOf CaptureThread) With {.IsBackground = True}
                    _thr.Start()
                End If
            End SyncLock
        End If
    End Sub

    Public Overrides Sub [Stop]()
        If _started Then
            _started = False
            SyncLock _syncRoot
                If _thr IsNot Nothing Then
                    _thr = Nothing
                End If
            End SyncLock
        End If
    End Sub

    Private Sub CaptureThread()
        _startTime = Now
        While True
            If Not _started Then
                Return
            End If

            Dim pcmBytes = New Byte(_bufferSize - 1) {}
            Dim bytesRead = _waveFile.Read(pcmBytes, 0, _bufferSize)
            Interlocked.Add(_bytesRead, bytesRead)
            If bytesRead = _bufferSize Then
                Dim e = New WaveInEventArgs(pcmBytes, bytesRead)
                MyBase.WaveDataAvailableBase(e)
            Else
                _started = False
            End If

            While PlaybackSpeed <= 0
                Thread.Sleep(1)
            End While

            If Not FastMode Then
                Dim timeInCaptureInS = (Now - _startTime).TotalSeconds
                Dim totalSamplesForOneX = _waveFormat.SampleRate * _waveFormat.Channels * timeInCaptureInS * PlaybackSpeed
                Dim samplesOverprocessed = (_bytesRead / _sampleSize) - totalSamplesForOneX
                Dim timeToSleepInS = samplesOverprocessed / _waveFormat.SampleRate
                If timeToSleepInS > 0 Then
                    Thread.Sleep(TimeSpan.FromSeconds(timeToSleepInS))
                End If
            End If
        End While
    End Sub

    Private Sub Rewind()
        _bytesRead = 0
        _waveFile.Position = 0
    End Sub
End Class
