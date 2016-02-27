Imports System.IO

Public Class DopplerLog
    Public Class Item
        Public Property T As DateTime
        Public Property L As Single
        Public Property H As Single

        Public Sub New(T As DateTime, L As Single, H As Single)
            Me.T = T
            Me.L = L
            Me.H = H
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("T:{0}, L:{1}, H:{2}",
                                   T.ToString(DateTimeFormat),
                                   L.ToString("000.00").Replace(",", "."),
                                   H.ToString("000.00").Replace(",", "."))
        End Function
    End Class

    Public Const DateTimeFormat As String = "dd.MM.yyyy HH.mm.ss"

    Private _log As New LinkedList(Of Item)

    Public ReadOnly Property Log As LinkedList(Of Item)
        Get
            Return _log
        End Get
    End Property

    Public Sub Add(T As DateTime, L As Single, H As Single)
        _log.AddLast(New Item(T, L, H))
    End Sub

    Public Sub Write(fileName As String)
        If _log.Any() Then
            File.WriteAllLines("dopplerLog__" + fileName + ".txt", _log.Select(Function(item) item.ToString()).ToArray())
            _log.Clear()
        End If
    End Sub

    Public Sub Read()
        Clear()
        '...
    End Sub

    Public Sub Clear()
        SyncLock Me
            _log.Clear()
        End SyncLock
    End Sub
End Class
