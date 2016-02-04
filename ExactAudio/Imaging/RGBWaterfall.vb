Imports System.Drawing
Imports Bwl.Imaging

Public Class RGBWaterfall
    Private _waterfallRowBlocks As New Queue(Of RGBMatrix)

    Public Sub Reset()
        SyncLock Me
            _waterfallRowBlocks.Clear()
        End SyncLock
    End Sub

    Public Sub Add(waterfallRowBlock As RGBMatrix)
        SyncLock Me
            If _waterfallRowBlocks.Any() Then
                If _waterfallRowBlocks.Peek().Width <> waterfallRowBlock.Width Then
                    Throw New Exception("_waterfallBlocks.Peek().Width <> waterfallBlock.Width")
                End If
            End If
            _waterfallRowBlocks.Enqueue(waterfallRowBlock)
        End SyncLock
    End Sub

    Public Function [Get]() As RGBMatrix
        SyncLock Me
            If Not _waterfallRowBlocks.Any() Then Return Nothing
            Dim rowsCounter = _waterfallRowBlocks.Sum(Function(item) item.Height)
            Dim waterfall = New RGBMatrix(_waterfallRowBlocks.Peek.Width, rowsCounter)

            Dim globalRowOffset As Integer = 0
            For Each rowBlock In _waterfallRowBlocks
                Parallel.For(0, 3, Sub(channel)
                                       Dim target = waterfall.Matrix(channel)
                                       Dim source = rowBlock.Matrix(channel)
                                       For i = 0 To rowBlock.Height - 1
                                           For j = 0 To rowBlock.Width - 1
                                               target(j, globalRowOffset + i) = source(j, i)
                                           Next
                                       Next
                                   End Sub)
                globalRowOffset += rowBlock.Height
            Next
            Return waterfall
        End SyncLock
    End Function

    Public Function ToBitmap(Optional scale As Single = 1.0) As Bitmap
        Dim waterfall = [Get]()
        If waterfall IsNot Nothing Then
            Return waterfall.ToBitmap(scale)
        Else
            Return Nothing
        End If
    End Function
End Class
