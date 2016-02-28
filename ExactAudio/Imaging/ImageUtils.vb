Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Bwl.Imaging

Public Module ImageUtils
    <Extension>
    Public Function ToBitmap(rgbMatrix As RGBMatrix, scale As Single) As Bitmap
        If rgbMatrix IsNot Nothing Then
            Dim width = CInt(rgbMatrix.Width * scale)
            Dim height = CInt(rgbMatrix.Height * scale)
            Return New Bitmap(rgbMatrix.ToBitmap, width, height)
        Else
            Return Nothing
        End If
    End Function
End Module
