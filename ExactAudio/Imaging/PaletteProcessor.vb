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
    Private _minDbLevel As Integer = -100
    Private _maxPaletteIdx As Integer
    Private _normalizer As New Normalizer()

    Public Sub SetMinDbLevel(minDbLevel As Integer)
        _minDbLevel = minDbLevel
    End Sub

    Public Function Process(data As Double()()) As RGBMatrix
        CheckPalette()
        Dim rgbResult As New RGBMatrix(data(0).Length, data.Length)
        Parallel.For(0, data.Length(), Sub(i)
                                           Dim rgbRow = Process(data(i))
                                           For j = 0 To rgbRow.Width - 1
                                               Dim R = rgbRow.Red(j, 0)
                                               Dim G = rgbRow.Green(j, 0)
                                               Dim B = rgbRow.Blue(j, 0)
                                               rgbResult.Red(j, i) = R
                                               rgbResult.Green(j, i) = G
                                               rgbResult.Blue(j, i) = B
                                           Next
                                       End Sub)
        Return rgbResult
    End Function

    Public Function Process(data As Double()) As RGBMatrix
        CheckPalette()
        With _normalizer
            .Init(_minDbLevel, 0, 0, _maxPaletteIdx)
            .Normalize(data)
        End With
        Dim rgb = New RGBMatrix(data.Length, 1)
        Parallel.For(0, data.Length(), Sub(i)
                                           Dim val = If(Double.IsNaN(data(i)), 0, data(i))
                                           val = If(val < 0, 0, val)
                                           val = If(val > _maxPaletteIdx, _maxPaletteIdx, val)
                                           rgb.Red(i, 0) = _red(val)
                                           rgb.Green(i, 0) = _green(val)
                                           rgb.Blue(i, 0) = _blue(val)
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
