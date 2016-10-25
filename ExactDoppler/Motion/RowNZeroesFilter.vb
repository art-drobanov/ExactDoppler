Public Class RowNZeroesFilter
    Private _NZeroes As Integer 'Максимальное допустимое кол-во "настоящих нулей" строки
    Private _zeroCount As Integer 'Количество зафиксированных "настоящих нулей" строки
    Private _memoryWindowSize As Integer 'Размер "окна памяти"
    Private _memoryWindow As Queue(Of Double) '"Окно памяти" строки
    Private _rowAccumulator As Queue(Of Double) 'Память строки

    Public Sub New(memorySize As Integer, NZeroes As Integer)
        Reset(memorySize, NZeroes)
    End Sub

    Public Sub Reset(memorySize As Double, NZeroes As Integer)
        _memoryWindowSize = memorySize
        _NZeroes = NZeroes
        _zeroCount = 0
        _memoryWindow = New Queue(Of Double)
        _rowAccumulator = New Queue(Of Double)
        For k = 1 To _memoryWindowSize 'Изначально "окно памяти" заполняется "положительно"
            _memoryWindow.Enqueue(1)
        Next
    End Sub

    Public Function Process(val As Double) As Double
        Dim result As Double = 0
        Dim binValue = If(val > Double.MinValue, 1, 0)
        _memoryWindow.Dequeue() : _memoryWindow.Enqueue(binValue)
        If _memoryWindow.Sum() = 0 Then 'Если все элементы "окна памяти" равны нулю...
            _zeroCount += 1 '...фиксируем "настоящий ноль"
        End If
        If binValue Then 'Если текущее значение отлично от нуля...
            _rowAccumulator.Enqueue(val) '...фиксируем его в памяти значений строки
        End If
        If _zeroCount > _NZeroes Then 'Если превышено допустимое количество нулей...
            result = Double.MinValue '...результат всегда -Inf
        Else
            Dim correctionCoeff = 0.3 + 0.7 * (CDbl(_zeroCount + 1) / CDbl(_NZeroes)) 'Коэффициент корректировки выходного значения...
            result = If(_rowAccumulator.Any(), _rowAccumulator.Average() * correctionCoeff, Double.MinValue) '...учитывает кол-во нулей
        End If
        Return result
    End Function
End Class
