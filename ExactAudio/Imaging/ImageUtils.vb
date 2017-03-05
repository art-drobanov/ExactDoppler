Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Bwl.Imaging

''' <summary>
''' Графические утилиты
''' </summary>
Public Module ImageUtils
    <Extension>
    Public Function ToBitmap(rgbMatrix As RGBMatrix, scale As Single) As Bitmap
        If rgbMatrix IsNot Nothing Then
            Dim width = Math.Round(rgbMatrix.Width * scale)
            Dim height = Math.Round(rgbMatrix.Height * scale)
            Return New Bitmap(rgbMatrix.ToBitmap(), width, height)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Расширение матрицы на указанное значение
    ''' </summary>
    <Extension>
    Public Function ExtendWidth(rgbMatrix As RGBMatrix, widthAddition As Integer) As RGBMatrix
        Dim result As New RGBMatrix(rgbMatrix.Width + widthAddition, rgbMatrix.Height)

        Dim shift = widthAddition \ 2
        Parallel.For(0, 3, Sub(channel As Integer)
                               For x = 0 To rgbMatrix.Width - 1
                                   For y = 0 To rgbMatrix.Height - 1
                                       result.MatrixPixel(channel, x + shift, y) = rgbMatrix.MatrixPixel(channel, x, y)
                                   Next
                               Next
                           End Sub)

        Return result
    End Function

    ''' <summary>
    ''' Полное клонирование массива массивов
    ''' </summary>
    <Extension>
    Public Function FullClone(input As Double()()) As Double()()
        Dim result As Double()() = input.Clone()
        For i = 0 To input.GetLength(0) - 1
            result(i) = input(i).Clone()
        Next
        Return result
    End Function
End Module
