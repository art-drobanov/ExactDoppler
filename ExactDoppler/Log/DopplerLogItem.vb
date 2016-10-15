Public Class DopplerLogItem
    Public Shared ReadOnly DateTimeFormat As String = "yyyy.MM.dd__HH.mm.ss.ffff"
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
                If (HighDoppler - LowDoppler) > _diffThr Then
                    Return "Motion++"
                End If
                If (LowDoppler - HighDoppler) > _diffThr Then
                    Return "Motion--"
                End If
                If (LowDoppler + HighDoppler) > _diffThr Then
                    Return "Motion+-"
                End If
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
