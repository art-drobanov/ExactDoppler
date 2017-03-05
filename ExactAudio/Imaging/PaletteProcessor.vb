Imports Bwl.Imaging
Imports DrAF.DSP

''' <summary>
''' "Раскрашиватель" монохромных изображений
''' </summary>
Public Class PaletteProcessor
    Private _red As Byte()
    Private _green As Byte()
    Private _blue As Byte()
    Private _nBits As Integer
    Private _maxPaletteIdx As Integer
    Private _normalizer As New Normalizer()

    Public Function Process(data As Double()(), Optional minDbLevel As Double = -100) As RGBMatrix
        CheckPalette()
        Dim rgbResult As New RGBMatrix(data(0).Length, data.Length)
        Parallel.For(0, data.Length(), Sub(y As Integer)
                                           Dim rgbRow = Process(data(y), minDbLevel)
                                           For x = 0 To rgbRow.Width - 1
                                               Dim R = rgbRow.RedPixel(x, 0)
                                               Dim G = rgbRow.GreenPixel(x, 0)
                                               Dim B = rgbRow.BluePixel(x, 0)
                                               rgbResult.RedPixel(x, y) = R
                                               rgbResult.GreenPixel(x, y) = G
                                               rgbResult.BluePixel(x, y) = B
                                           Next
                                       End Sub)
        Return rgbResult
    End Function

    Public Function Process(data As Double(), Optional minDbLevel As Double = -100) As RGBMatrix
        CheckPalette()
        Dim dataNorm As Double() = data.Clone()
        With _normalizer
            .Init(minDbLevel, 0, 0, _maxPaletteIdx)
            .Normalize(dataNorm)
        End With
        Dim rgb = New RGBMatrix(dataNorm.Length, 1)
        Parallel.For(0, dataNorm.Length(), Sub(x As Integer)
                                               Dim val = If(Double.IsNaN(dataNorm(x)), 0, dataNorm(x))
                                               val = If(val < 0, 0, val)
                                               val = If(val > _maxPaletteIdx, _maxPaletteIdx, val)
                                               rgb.RedPixel(x, 0) = _red(val)
                                               rgb.GreenPixel(x, 0) = _green(val)
                                               rgb.BluePixel(x, 0) = _blue(val)
                                           End Sub)
        Return rgb
    End Function

    Public Sub DefaultPalette()
        _red = New Byte(Byte.MaxValue) {}
        _green = New Byte(Byte.MaxValue) {}
        _blue = New Byte(Byte.MaxValue) {}
        For i = 0 To Byte.MaxValue
            _red(i) = i \ 2
            _green(i) = i
            _blue(i) = i \ 2
        Next
        ChechPaletteBits()
    End Sub

    Public Sub LoadPalette(path As String, name As String)
        Dim redPath = IO.Path.Combine(path, name + "_R.raw")
        Dim greenPath = IO.Path.Combine(path, name + "_G.raw")
        Dim bluePath = IO.Path.Combine(path, name + "_B.raw")
        If Not IO.File.Exists(redPath) Then
            Throw New Exception(String.Format("PaletteProcessor: Red channel of {0} is not accessible!", name))
        End If
        If Not IO.File.Exists(greenPath) Then
            Throw New Exception(String.Format("PaletteProcessor: Green channel of {0} is not accessible!", name))
        End If
        If Not IO.File.Exists(bluePath) Then
            Throw New Exception(String.Format("PaletteProcessor: Blue channel of {0} is not accessible!", name))
        End If
        _red = IO.File.ReadAllBytes(redPath)
        _green = IO.File.ReadAllBytes(greenPath)
        _blue = IO.File.ReadAllBytes(bluePath)
        ChechPaletteBits()
    End Sub

    Public Sub ChechPaletteBits()
        If _red.Length <> _green.Length OrElse _red.Length <> _blue.Length OrElse _green.Length <> _blue.Length Then
            Throw New Exception("PaletteProcessor: Wrong palette!")
        End If
        Dim bit8 = CInt(Math.Pow(2, 8))
        Dim bit16 = CInt(Math.Pow(2, 16))
        Dim bit24 = CInt(Math.Pow(2, 24))
        Dim N = ExactFFT.ToLowerPowerOf2(_red.Length)
        Select Case N
            Case bit8
                _nBits = 8
            Case bit16
                _nBits = 16
            Case bit24
                _nBits = 24
            Case Else
                Throw New Exception("PaletteProcessor: Palette is broken!")
        End Select
        _maxPaletteIdx = Math.Pow(2, _nBits) - 1
    End Sub

    Private Sub CheckPalette()
        If _red Is Nothing OrElse _green Is Nothing OrElse _blue Is Nothing Then
            DefaultPalette()
        End If
        If _red.Length <> _green.Length OrElse _red.Length <> _blue.Length OrElse _green.Length <> _blue.Length Then
            Throw New Exception("PaletteProcessor: Palette is broken!")
        End If
    End Sub
End Class
