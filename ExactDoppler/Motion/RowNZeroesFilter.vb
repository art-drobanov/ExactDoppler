Public Class RowNZeroesFilter
    Private _memorySize As Double
    Private _NZeroes As Integer
    Private _zeroCount As Integer
    Private _binMemory As Queue(Of Double)
    Private _rowMemory As Queue(Of Double)

    Public Sub New(memorySize As Double, NZeroes As Integer)
        Reset(memorySize, NZeroes)
    End Sub

    Public Sub Reset(memorySize As Double, NZeroes As Integer)
        _memorySize = memorySize
        _NZeroes = NZeroes
        _zeroCount = 0
        _binMemory = New Queue(Of Double)
        _rowMemory = New Queue(Of Double)
        For k = 1 To _memorySize
            _binMemory.Enqueue(1)
        Next
    End Sub

    Public Function Process(val As Double) As Double
        Dim result As Double = 0
        Dim binValue = If(val > Double.MinValue, 1, 0)
        _binMemory.Dequeue() : _binMemory.Enqueue(binValue)
        If _binMemory.Sum() = 0 Then
            _zeroCount += 1
        End If
        If binValue Then
            _rowMemory.Enqueue(val)
        End If
        If _zeroCount > _NZeroes Then
            result = Double.MinValue
        Else
            result = If(_rowMemory.Any(), _rowMemory.Average(), Double.MinValue)
        End If
        Return result
    End Function
End Class
