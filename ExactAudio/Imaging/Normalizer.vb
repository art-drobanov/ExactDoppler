''' <summary>
''' Нормализатор набора данных double
''' </summary>
Public Class Normalizer
    Public Property TargetMin As Double
    Public Property TargetMax As Double

    Public Property PreOffset As Double
    Public Property Mult As Double
    Public Property PostOffset As Double

    Public Sub Init(currentMin As Double, currentMax As Double, targetMin As Double, targetMax As Double)
        _TargetMin = targetMin
        _TargetMax = targetMax

        Dim currentDelta = currentMax - currentMin 'Текущий размах
        Dim targetDelta = targetMax - targetMin 'Целевой размах
        _PreOffset = -currentMin + 1 'Предварительное смещение должно приводить выборку к состоянию,
        'когда перед домножением нет элементов, которые меньше "1"
        _Mult = targetDelta / currentDelta 'Нормирующий множитель
        _PostOffset = _TargetMin - _Mult 'Пост-смещение
    End Sub

    Public Sub Normalize(data As Double()())
        Parallel.For(0, data.Length, Sub(i)
                                         Normalize(data(i))
                                     End Sub)
    End Sub

    Public Sub Normalize(data As Double())
        Parallel.For(0, data.Length, Sub(i)
                                         data(i) = ((data(i) + _PreOffset) * _Mult) + _PostOffset
                                     End Sub)
    End Sub
End Class
