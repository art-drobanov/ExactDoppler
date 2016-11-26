''' <summary>
''' Вспомогательная "математика" для работы с аудиосигналами
''' </summary>
Module ExactAudioMath
    Public Sub DbScale(data As Double()(), zeroDbLevel As Double, Optional mult As Double = 10.0)
        Parallel.For(0, data.Length, Sub(i As Integer)
                                         DbScale(data(i), zeroDbLevel, mult)
                                     End Sub)
    End Sub

    Public Sub DbScale(data As Double(), zeroDbLevel As Double, Optional mult As Double = 10.0)
        Parallel.For(0, data.Length, Sub(i As Integer)
                                         data(i) = mult * Math.Log(data(i) / zeroDbLevel) 'log
                                     End Sub)
    End Sub

    Public Function Db(value As Double, zeroDbLevel As Double, Optional mult As Double = 10.0) As Double
        Return mult * Math.Log(value / zeroDbLevel) 'log
    End Function

    Public Function DbInv(value As Double, zeroDbLevel As Double, Optional mult As Double = 10.0) As Double
        Return Math.Exp(value / mult) * zeroDbLevel 'exp
    End Function

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
End Module
