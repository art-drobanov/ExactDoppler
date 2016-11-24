''' <summary>
''' Вспомогательная "математика"
''' </summary>
Module ExactAudioMath
    Public Sub DbScale(data As Double()(), zeroDbLevel As Double)
        Parallel.For(0, data.Length, Sub(i As Integer)
                                         DbScale(data(i), zeroDbLevel)
                                     End Sub)
    End Sub

    Public Sub DbScale(data As Double(), zeroDbLevel As Double)
        Parallel.For(0, data.Length, Sub(i As Integer)
                                         data(i) = 10.0 * Math.Log(data(i) / zeroDbLevel) 'log
                                     End Sub)
    End Sub

    Public Sub DbSquelch(data As Double()(), squelchInDb As Double)
        Parallel.For(0, data.Length, Sub(i As Integer)
                                         DbSquelch(data(i), squelchInDb)
                                     End Sub)
    End Sub

    Public Sub DbSquelch(data As Double(), squelchInDb As Double)
        Parallel.For(0, data.Length, Sub(i As Integer)
                                         Dim val = data(i)
                                         val = If(val < squelchInDb, Double.MinValue, val) 'squelch
                                         data(i) = val
                                     End Sub)
    End Sub

    Public Function Db(value As Double, zeroDbLevel As Double) As Double
        Dim val = 10.0 * Math.Log(value / zeroDbLevel) 'log
        Return val
    End Function

    Public Function DbInv(value As Double, zeroDbLevel As Double) As Double
        Return Math.Exp(value / 10.0) * zeroDbLevel 'exp
    End Function
End Module
