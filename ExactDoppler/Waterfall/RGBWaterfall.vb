Imports System.Drawing
Imports Bwl.Imaging
Imports ExactAudio

''' <summary>
''' "Водопад" в формате RGB
''' </summary>
Public Class RGBWaterfall
    Private _waterfallRowBlocks As New Queue(Of RGBMatrix)
    Private _maxBlocksCount As Integer = 1500
    Private _droppedBlocksCount As Long = 0

    Public Property MaxBlocksCount As Integer
        Get
            SyncLock SyncRoot
                Return _maxBlocksCount
            End SyncLock
        End Get
        Set(value As Integer)
            _maxBlocksCount = value
        End Set
    End Property

    Public ReadOnly Property BlocksCount As Integer
        Get
            SyncLock SyncRoot
                Return _waterfallRowBlocks.Count
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property DroppedBlocksCount As Long
        Get
            SyncLock SyncRoot
                Return _droppedBlocksCount
            End SyncLock
        End Get
    End Property

    Public ReadOnly SyncRoot As New Object

    Public Sub Clear()
        SyncLock SyncRoot
            _waterfallRowBlocks.Clear()
            _droppedBlocksCount = 0
        End SyncLock
    End Sub

    Public Sub Add(waterfallRowBlock As RGBMatrix)
        SyncLock SyncRoot
            If waterfallRowBlock IsNot Nothing Then
                If _waterfallRowBlocks.Any() Then
                    If _waterfallRowBlocks.Peek().Width <> waterfallRowBlock.Width Then
                        Throw New Exception("RGBWaterfall: _waterfallBlocks.Peek().Width <> waterfallBlock.Width")
                    End If
                End If
                _waterfallRowBlocks.Enqueue(waterfallRowBlock)
                Dim blocksToRemove = _waterfallRowBlocks.Count - MaxBlocksCount
                For i = 1 To blocksToRemove
                    _waterfallRowBlocks.Dequeue()
                    _droppedBlocksCount += 1
                Next
            End If
        End SyncLock
    End Sub

    Public Function [Get]() As RGBMatrix
        SyncLock SyncRoot
            If Not _waterfallRowBlocks.Any() Then Return Nothing
            Dim rowsCounter = _waterfallRowBlocks.Sum(Function(item) item.Height)
            Dim waterfall = New RGBMatrix(_waterfallRowBlocks.Peek.Width, rowsCounter)

            Dim globalRowOffset As Integer = 0
            For Each rowBlock In _waterfallRowBlocks
                Parallel.For(0, 3, Sub(channel As Integer)
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
        SyncLock SyncRoot
            Dim waterfall = [Get]()
            If waterfall IsNot Nothing Then
                Return waterfall.ToBitmap(scale)
            Else
                Return Nothing
            End If
        End SyncLock
    End Function

    Public Sub Write(filaName As String)
        SyncLock SyncRoot
            Dim bmp = ToBitmap()
            If bmp IsNot Nothing Then
                bmp.Save(filaName)
            End If
        End SyncLock
    End Sub
End Class
