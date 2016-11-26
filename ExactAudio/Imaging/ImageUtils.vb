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
            Dim rgbMatrix4 = MatrixTools.RGBMatrixAlign4(rgbMatrix)
            Dim width = Math.Round(rgbMatrix4.Width * scale)
            Dim height = Math.Round(rgbMatrix4.Height * scale)
            width += 4 - width Mod 4
            Return New Bitmap(rgbMatrix4.ToBitmap(), width, height)
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
                               For i = 0 To rgbMatrix.Height - 1
                                   For j = 0 To rgbMatrix.Width - 1
                                       result.Matrix(channel)(j + shift, i) = rgbMatrix.Matrix(channel)(j, i)
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
