Imports System.IO
Imports System.Threading
Imports NAudio.Wave

''' <summary>
''' Аудиоисточник на основе Wav-файла
''' </summary>
Public Class WaveFileSource
    Inherits WaveSource

    Private _fileName As String
    Private _bufferSize As Integer
    Private _read As Integer
    Private _waveFile As WaveFileReader

    Public Overrides ReadOnly Property Name As String
        Get
            Return _fileName
        End Get
    End Property

    Public Sub New(fileName As String, sampleRate As Integer, bitDepth As Integer, stereo As Boolean, minSamplesCountInBlock As Integer)
        MyBase.New(sampleRate, bitDepth, stereo, minSamplesCountInBlock)
        _fileName = fileName
        _bufferSize = _minSamplesCountInBlock * (_waveFormat.BitsPerSample >> 3) * _waveFormat.Channels
        Dim wavBytes = File.ReadAllBytes(fileName)
        _waveFile = New WaveFileReader(New MemoryStream(wavBytes)) With {.Position = 0}
        If _waveFile.WaveFormat.SampleRate <> _waveFormat.SampleRate Then
            Throw New Exception(String.Format("{0}: Wav sample rate differs from desired!", TypeName(Me)))
        End If
        If _waveFile.WaveFormat.Channels <> _waveFormat.Channels Then
            Throw New Exception(String.Format("{0}: Wav channels differs from desired!", TypeName(Me)))
        End If
    End Sub

    Public Overrides Sub Start()
        If Not _started Then
            SyncLock _waveFile
                Rewind()
                _started = True
                Dim thr = New Thread(AddressOf CaptureThread) With {.IsBackground = True}
                thr.Start()
            End SyncLock
        End If
    End Sub

    Public Overrides Sub [Stop]()
        If _started Then
            SyncLock _waveFile
                _started = False
            End SyncLock
        End If
    End Sub

    Private Sub CaptureThread()
        While True
            If Not _started Then
                Return
            End If
            Dim pcmBytes = New Byte(_bufferSize - 1) {}
            Dim read = _waveFile.Read(pcmBytes, 0, _bufferSize)
            Interlocked.Add(_read, read)
            If read = _bufferSize Then
                Dim e = New WaveInEventArgs(pcmBytes, read)
                Dim t1 = Now
                MyBase.WaveDataAvailableBase(e)
                Dim t2 = Now
                Dim timeToSleepInS = ((_minSamplesCountInBlock / _waveFormat.SampleRate) - (t2 - t1).TotalSeconds)
                If timeToSleepInS > 0 Then
                    Thread.Sleep(TimeSpan.FromSeconds(timeToSleepInS))
                End If
                If (_read + _bufferSize) >= _waveFile.Length Then
                    Rewind()
                End If
            Else
                Rewind()
            End If
        End While
    End Sub

    Private Sub Rewind()
        _read = 0
        _waveFile.Position = 0
    End Sub
End Class
