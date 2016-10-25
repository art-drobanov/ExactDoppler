Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Bwl.Imaging

Public Module ImageUtils
    <Extension>
    Public Function ToBitmap(rgbMatrix As RGBMatrix, scale As Single) As Bitmap
        If rgbMatrix IsNot Nothing Then
            Dim width = Math.Round(rgbMatrix.Width * scale)
            Dim height = Math.Round(rgbMatrix.Height * scale)
            width += 4 - width Mod 4
            height += 4 - height Mod 4
            Return New Bitmap(rgbMatrix.ToBitmap(), width, height)
        Else
            Return Nothing
        End If
    End Function
End Module
