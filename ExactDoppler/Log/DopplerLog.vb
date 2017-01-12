Imports System.IO
Imports System.Globalization

''' <summary>
''' Доплеровский лог
''' </summary>
Public Class DopplerLog
    Private _items As New LinkedList(Of DopplerLogItem)
    Public ReadOnly SyncRoot As New Object

    Public ReadOnly Property Items As LinkedList(Of DopplerLogItem)
        Get
            Return _items
        End Get
    End Property

    Public Sub Add(timestamp As DateTime, L As Single, H As Single, carrierIsOK As Boolean)
        SyncLock SyncRoot
            Dim item = New DopplerLogItem(timestamp, L, H, carrierIsOK)
            _items.AddLast(item)
        End SyncLock
    End Sub

    Public Sub Add(logItem As DopplerLogItem)
        SyncLock SyncRoot
            _items.AddLast(logItem)
        End SyncLock
    End Sub

    Public Sub Write(stream As Stream)
        SyncLock SyncRoot
            If _items.Any() Then
                Dim sw = New StreamWriter(stream, Text.Encoding.UTF8)
                For Each logItem In _items
                    Dim str = logItem.ToString()
                    sw.WriteLine(str)
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
            Dim logItemStrings = logItemString.ToUpper().Replace("L:", String.Empty) _
                                                        .Replace("H:", String.Empty) _
                                                        .Replace("%", String.Empty) _
                                                        .Replace(";", String.Empty) _
                                                        .Split(",")
            Dim T As DateTime
            Dim L As Single
            Dim H As Single
            Dim C As Boolean

            If Not DateTime.TryParseExact(logItemStrings(0), DopplerLogItem.DateTimeFormat, Nothing, DateTimeStyles.None, T) Then
                Throw New Exception(String.Format("DopplerLog: Can't parse '{0}' from log", logItemStrings(0)))
            End If

            logItemStrings(1) = logItemStrings(1).Trim()
            If Not Single.TryParse(logItemStrings(1).Replace(".", ","), L) Then
                If Not Single.TryParse(logItemStrings(1).Replace(",", "."), L) Then
                    Throw New Exception(String.Format("DopplerLog: Can't parse 'L:{0}' from log", logItemStrings(1)))
                End If
            End If

            logItemStrings(2) = logItemStrings(2).Trim()
            If Not Single.TryParse(logItemStrings(2).Replace(".", ","), H) Then
                If Not Single.TryParse(logItemStrings(2).Replace(",", "."), H) Then
                    Throw New Exception(String.Format("DopplerLog: Can't parse 'H:{0}' from log", logItemStrings(2)))
                End If
            End If

            If logItemStrings(3).Contains("CARRIER") AndAlso logItemStrings(3).Contains("OK") AndAlso Not logItemStrings(3).Contains("ERR") Then
                C = True
            Else
                C = False
            End If

            newLog.Add(T, L, H, C)
        End While
        sr.Close()

        If newLog.Items.Any() Then
            SyncLock SyncRoot
                Clear()
                For Each item In newLog.Items
                    _items.AddLast(item)
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
