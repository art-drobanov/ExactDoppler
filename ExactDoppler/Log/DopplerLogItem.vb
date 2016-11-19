Public Class DopplerLogItem
    Public Shared ReadOnly DateTimeFormat As String = "yyyy-MM-dd HH:mm:ss.ffff zzz"
    Public Property Time As DateTime
    Public Property LowDoppler As Single
    Public Property HighDoppler As Single
    Public Property CarrierIsOK As Boolean

    Public ReadOnly Property Carrier As String
        Get
            Return If(CarrierIsOK, "OK", "ERR")
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
        Return String.Format("{0}, L:{1}%, H:{2}%, Carrier:{3};",
                             Time.ToString(DateTimeFormat),
                             LowDoppler.ToString("00.00").Replace(",", "."),
                             HighDoppler.ToString("00.00").Replace(",", "."),
                             Carrier)
    End Function
End Class
