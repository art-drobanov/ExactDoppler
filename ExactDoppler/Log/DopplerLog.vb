Imports System.IO
Imports System.Globalization

Public Class DopplerLog
    Public Class Item
        Private Const _diffThr = 15
        Public Property Time As DateTime
        Public Property LowDoppler As Single
        Public Property HighDoppler As Single
        Public Property CarrierIsOK As Boolean

        Public ReadOnly Property Carrier As String
            Get
                Return If(CarrierIsOK, "OK", "ERR")
            End Get
        End Property

        Public ReadOnly Property Type As String
            Get
                If CarrierIsOK Then
                    If (HighDoppler - LowDoppler) > _diffThr Then Return "Motion++"
                    If (LowDoppler - HighDoppler) > _diffThr Then Return "Motion--"
                    If (LowDoppler + HighDoppler) > _diffThr Then Return "Motion+-"
                    Return "NoMotion"
                Else
                    Return "Carrier!"
                End If
            End Get
        End Property

        Public Sub New(time As DateTime, L As Single, H As Single, carrierIsOK As Boolean)
            _Time = time
            If L > 100 OrElse H > 100 Then
                Dim top = Math.Max(L, H)
                L = (L / top) * 100
                H = (H / top) * 100
            End If
            _LowDoppler = If(L > 99.99, 99.99, If(L < 0, 0, L))
            _HighDoppler = If(H > 99.99, 99.99, If(H < 0, 0, H))
            _CarrierIsOK = carrierIsOK
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("DMY:{0}, L:{1}%, H:{2}%; Type:{3}, Carrier:{4};",
                                 Time.ToString(DateTimeFormat),
                                 LowDoppler.ToString("00.00").Replace(",", "."),
                                 HighDoppler.ToString("00.00").Replace(",", "."),
                                 Type,
                                 Carrier)
        End Function
    End Class

    Public Const DateTimeFormat As String = "yyyy.MM.dd__HH.mm.ss.ffff"
    Private _items As New LinkedList(Of Item)
    Public ReadOnly SyncRoot As New Object

    Public ReadOnly Property Items As LinkedList(Of Item)
        Get
            Return _items
        End Get
    End Property

    Public Sub Add(time As DateTime, L As Single, H As Single, carrierIsOK As Boolean)
        SyncLock SyncRoot
            Dim item = New Item(time, L, H, carrierIsOK)
            _items.AddLast(item)
        End SyncLock
    End Sub

    Public Sub Add(logItem As DopplerLog.Item)
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
            Dim logItemStrings = logItemString.ToUpper().Replace("DMY", String.Empty) _
                                                        .Replace("CARRIER", String.Empty) _
                                                        .Replace("L", String.Empty) _
                                                        .Replace("H", String.Empty) _
                                                        .Replace("TYPE", String.Empty) _
                                                        .Replace("NO", String.Empty) _
                                                        .Replace("MOTION", String.Empty) _
                                                        .Replace("!", String.Empty) _
                                                        .Replace("+", String.Empty) _
                                                        .Replace("-", String.Empty) _
                                                        .Replace(";", String.Empty) _
                                                        .Replace(":", String.Empty) _
                                                        .Replace(" ", String.Empty) _
                                                        .Split(",")

            Dim T As DateTime
            Dim L As Single
            Dim H As Single
            Dim C As Boolean

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

            If logItemStrings(3).Contains("OK") AndAlso Not logItemStrings(3).Contains("ERR") Then
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
