Imports NAudio

Public Class PcmLog
    Private _sampleRate As Integer
    Private _items As New LinkedList(Of Single())

    Public ReadOnly Property Items As LinkedList(Of Single())
        Get
            Return _items
        End Get
    End Property

    Public Sub New(sampleRate As Integer)
        _sampleRate = sampleRate
    End Sub

    Public Sub Add(pcm As Single())
        SyncLock Me
            _items.AddLast(pcm)
        End SyncLock
    End Sub

    Public Sub Write(filename As String)
        SyncLock Me
            Dim wavFile As New Wave.WaveFileWriter(filename, New Wave.WaveFormat(_sampleRate, 1))
            For Each pcmBlock In _items
                wavFile.WriteSamples(pcmBlock, 0, pcmBlock.Length)
            Next
            With wavFile
                .Flush()
                .Close()
            End With
        End SyncLock
    End Sub

    Public Sub Clear()
        SyncLock Me
            _items.Clear()
        End SyncLock
    End Sub
End Class
