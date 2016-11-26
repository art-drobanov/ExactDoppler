Imports System.Drawing
Imports Bwl.Imaging
Imports ExactAudio

''' <summary>
''' "Водопад" в формате RGB
''' </summary>
Public Class RGBWaterfall
    Private Const _defaultWidth = 892

    Private _waterfallRowBlocks As New Queue(Of RGBMatrix)
    Private _maxBlocksCount As Integer = 1500
    Private _droppedBlocksCount As Long = 0
    Private _width As Integer = -1

    Public Sub New()
    End Sub

    Public Sub New(width As Integer)
        _width = width
    End Sub

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
            If _width = -1 Then
                _width = _defaultWidth
            End If
            If waterfallRowBlock IsNot Nothing Then
                If waterfallRowBlock.Width > _width Then 'Если поступающий на вход блок слишком большой - ошибка!
                    Throw New Exception("RGBWaterfall: waterfallRowBlock.Width > _width")
                Else
                    Dim widthAddition = _width - waterfallRowBlock.Width 'Рассчитываем "добавку" к ширине...
                    If widthAddition = 0 Then
                        _waterfallRowBlocks.Enqueue(waterfallRowBlock)
                    Else
                        Dim waterfallRowBlockAligned = ImageUtils.ExtendWidth(waterfallRowBlock, widthAddition) '...и обеспечиваем её
                        _waterfallRowBlocks.Enqueue(waterfallRowBlockAligned)
                    End If
                End If
            End If

            'Удаление блоков, выходящих за допустимые границы
            Dim blocksToRemove = _waterfallRowBlocks.Count - MaxBlocksCount
            For i = 1 To blocksToRemove
                _waterfallRowBlocks.Dequeue()
                _droppedBlocksCount += 1
            Next
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
