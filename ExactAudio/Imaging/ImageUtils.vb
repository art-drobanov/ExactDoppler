Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Bwl.Imaging

Public Module ImageUtils
    <Extension>
    Public Function ToBitmap(rgbMatrix As RGBMatrix, scale As Single) As Bitmap
        If rgbMatrix IsNot Nothing Then
            rgbMatrix = rgbMatrix.Align4()
            Dim width = CInt(rgbMatrix.Width * scale)
            Dim height = CInt(rgbMatrix.Height * scale)
            Return New Bitmap(rgbMatrix.ToBitmap, width, height)
        Else
            Return Nothing
        End If
    End Function

    <Extension>
    Public Function Align4(img As RGBMatrix) As RGBMatrix
        Dim padding = 4 - img.Width Mod 4
        Dim paddingL = padding \ 2
        Dim paddingR = padding - paddingL
        Dim result = New RGBMatrix(img.Width + padding, img.Height)
        Parallel.For(0, 3, Sub(m As Integer)
                               For i = 0 To result.Height - 1
                                   For j = paddingL To result.Width - 1 - paddingR
                                       result.Matrix(m)(j, i) = img.Matrix(m)(j - paddingL, i)
                                   Next
                               Next
                           End Sub)
        Return result
    End Function
End Module
