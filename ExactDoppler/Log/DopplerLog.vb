Imports System.IO
Imports System.Globalization

Public Class DopplerLog
    Public Class Item
        Private Const _diffThr = 15
        Public Property Time As DateTime
        Public Property LowDoppler As Single
        Public Property HighDoppler As Single
        Public ReadOnly Property Type As String
            Get
                If (HighDoppler - LowDoppler) > _diffThr Then Return "Incoming motion"
                If (LowDoppler - HighDoppler) > _diffThr Then Return "Outcoming motion"
                Return "Motion"
            End Get
        End Property

        Public Sub New(T As DateTime, L As Single, H As Single)
            Me.Time = T
            Me.LowDoppler = If(L > 99.99, 99.99, If(L < 0, 0, L))
            Me.HighDoppler = If(H > 99.99, 99.99, If(H < 0, 0, H))
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("DMY:{0}, L:{1}%, H:{2}%; Type:{3}",
                                 Time.ToString(DateTimeFormat),
                                 LowDoppler.ToString("00.00").Replace(",", "."),
                                 HighDoppler.ToString("00.00").Replace(",", "."),
                                 Type)
        End Function
    End Class

    Public Const DateTimeFormat As String = "dd.MM.yyyy HH.mm.ss"

    Private _items As New LinkedList(Of Item)

    Public ReadOnly SyncRoot As New Object

    Public ReadOnly Property Items As LinkedList(Of Item)
        Get
            Return _items
        End Get
    End Property

    Public Sub Add(T As DateTime, L As Single, H As Single)
        SyncLock SyncRoot
            _items.AddLast(New Item(T, L, H))
        End SyncLock
    End Sub

    Public Sub Write(stream As Stream)
        SyncLock SyncRoot
            If _items.Any() Then
                Dim sw = New StreamWriter(stream, Text.Encoding.UTF8)
                For Each logItem In _items.Select(Function(item) item.ToString())
                    sw.WriteLine(logItem)
                Next
                sw.Flush()
            End If
        End SyncLock
    End Sub

    Public Sub Read(stream As Stream)
        Dim newLog As New DopplerLog()

        Dim sr = New StreamReader(stream)
        While True
            Dim logItemString = sr.ReadLine()
            If logItemString Is Nothing Then Exit While
            Dim logItemStrings = logItemString.ToUpper().Replace("DMY:", String.Empty).Replace("L:", String.Empty).Replace("H:", String.Empty) _
                                                        .Replace("TYPE:", String.Empty).Replace("INCOMING", String.Empty).Replace("OUTCOMING", String.Empty) _
                                                        .Replace("MOTION", String.Empty).Replace(";", String.Empty).Split(",")

            Dim T As DateTime
            Dim L As Single
            Dim H As Single

            If Not DateTime.TryParseExact(logItemStrings(0), DateTimeFormat, Nothing, DateTimeStyles.None, T) Then
                Throw New Exception(String.Format("Can't parse 'DMY:{0}' from log", logItemStrings(0)))
            End If

            logItemStrings(1) = logItemStrings(1).Replace("%", String.Empty)
            If Not Single.TryParse(logItemStrings(1).Replace(".", ","), L) Then
                If Not Single.TryParse(logItemStrings(1).Replace(",", "."), L) Then
                    Throw New Exception(String.Format("Can't parse 'L:{0}' from log", logItemStrings(1)))
                End If
            End If

            logItemStrings(2) = logItemStrings(2).Replace("%", String.Empty)
            If Not Single.TryParse(logItemStrings(2).Replace(".", ","), H) Then
                If Not Single.TryParse(logItemStrings(2).Replace(",", "."), H) Then
                    Throw New Exception(String.Format("Can't parse 'H:{0}' from log", logItemStrings(2)))
                End If
            End If

            newLog.Add(T, L, H)
        End While
        sr.Close()

        If newLog.Items.Any() Then
            SyncLock SyncRoot
                Clear()
                For Each li In newLog.Items
                    _items.AddLast(li)
                Next
                newLog.Clear()
                newLog = Nothing
            End SyncLock
        End If
    End Sub

    Public Sub Clear()
        SyncLock SyncRoot
            _items.Clear()
        End SyncLock
    End Sub

    Public Sub Write(filename As String)
        Using logStream = File.OpenWrite(filename)
            Write(logStream)
            logStream.Flush()
        End Using
    End Sub

    Public Sub Read(filename As String)
        Using logStream = File.OpenRead(filename)
            Read(logStream)
        End Using
    End Sub
End Class
