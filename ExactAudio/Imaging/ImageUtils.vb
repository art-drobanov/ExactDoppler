Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Bwl.Imaging

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
End Module
