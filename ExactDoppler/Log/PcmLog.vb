Imports NAudio

Public Class PcmLog
    Private _sampleRate As Integer

    Private _items As New LinkedList(Of Single())
    Public ReadOnly SyncRoot As New Object

    Public ReadOnly Property Items As Single()()
        Get
            SyncLock SyncRoot
                Return _items.ToArray()
            End SyncLock
        End Get
    End Property

    Public Sub New(sampleRate As Integer)
        _sampleRate = sampleRate
    End Sub

    Public Sub Add(pcm As Single())
        SyncLock SyncRoot
            If pcm IsNot Nothing Then
                _items.AddLast(pcm)
            End If
        End SyncLock
    End Sub

    Public Sub Write(filename As String)
        SyncLock SyncRoot
            If _items.Any() Then
                Dim wavFile As New Wave.WaveFileWriter(filename, New Wave.WaveFormat(_sampleRate, 1))
                For Each pcmBlock In _items
                    wavFile.WriteSamples(pcmBlock, 0, pcmBlock.Length)
                Next
                With wavFile
                    .Flush()
                    .Close()
                End With
            End If
        End SyncLock
    End Sub

    Public Sub Clear()
        SyncLock SyncRoot
            _items.Clear()
        End SyncLock
    End Sub
End Class
