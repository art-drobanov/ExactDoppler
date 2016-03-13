Imports Bwl.Imaging
Imports DrAF.DSP

Public Class MotionExplorer
    Inherits FFTExplorer

    ''' <summary>
    ''' Результат анализа блока PCM
    ''' </summary>
    Public Class MotionExplorerResult
        Public Property LowDoppler As New LinkedList(Of Single)
        Public Property HighDoppler As New LinkedList(Of Single)
        Public Property Duration As Double
        Public Property Pcm As Single()
        Public Property Image As RGBMatrix
    End Class

    Private Const _redChannel = 0 '0
    Private Const _greenChannel = 1 '1
    Private Const _blueChannel = 2 '2
    Private Const _sideFormDivider = 20 '20
    Private Const _gainHarmRadius = 2 '2
    Private Const _gainStep = 1.0 '1.0
    Private Const _NZeroes = 3 ' 3
    Private Const _noiseGuardCoeff = 1.05 '1.05
    Private Const _noiseScannerDepth = 0.1 '0.1
    Private Const _brightness = 100 '100
    Private Const _minFreq = 200 '200
    Private Const _blindZone = 12 '12

    Private _targetNRG As Double = 0 '0
    Private _gain As Double = 1.0 '1.0

    Private _paletteProcessor As PaletteProcessor
    Private _waterfallPlayer As WaterfallPlayer

    ''' <summary>
    ''' Конструктор.
    ''' </summary>
    ''' <param name="frameWidth">Размер кадра.</param>
    ''' <param name="frameStep">Шаг окна БПФ.</param>
    ''' <param name="sampleRate">Частота семплирования.</param>
    ''' <param name="nBits">Разрядность.</param>
    ''' <param name="stereo">Стереорежим?</param>
    Public Sub New(frameWidth As Integer, frameStep As Integer, sampleRate As Integer, nBits As Integer, stereo As Boolean)
        MyBase.New(frameWidth, frameStep, sampleRate, nBits, stereo)
        _targetNRG = Math.Pow(2, nBits - 1)
        _paletteProcessor = New PaletteProcessor()
        _paletteProcessor.LoadPalette("..\..\..\palettes.raw\Uranium", "URANIUM_PALETTE")
        _waterfallPlayer = New WaterfallPlayer(frameWidth, frameStep, sampleRate, nBits, _minFreq)
    End Sub

    ''' <summary>
    ''' Основной метод обработки.
    ''' </summary>
    ''' <param name="pcmSamples">Pcm-семплы.</param>
    ''' <param name="pcmSamplesCount">Количество семплов (для учета режима моно/стерео).</param>
    ''' <param name="lowFreq">Нижняя частота области интереса.</param>
    ''' <param name="highFreq">Верхняя частота области интереса.</param>
    ''' <param name="blindZone">"Слепая зона" для подавления несущей частоты.</param>
    ''' <param name="displayLeft">Отображать разметку слева?</param>
    ''' <param name="displayRightWithLeft">Отображать "правую" разметку вместе с разметкой слева?</param>
    ''' <param name="displayCenter">Отображать центральную область разметки?</param>
    ''' <param name="displayRight">Отображать разметку справа?</param>
    ''' <param name="pcmOutput">Осуществлять вывод Pcm?</param>
    ''' <param name="imageOutput">Выводить изображение?</param>
    ''' <returns>"Результат анализа движения".</returns>
    Public Function Process(pcmSamples As Single(), pcmSamplesCount As Integer, lowFreq As Double, highFreq As Double, blindZone As Integer,
                            displayLeft As Boolean, displayRightWithLeft As Boolean, displayCenter As Boolean, displayRight As Boolean,
                            pcmOutput As Boolean, imageOutput As Boolean) As MotionExplorerResult
        'FFT
        Dim mag = MyBase.Explore(pcmSamples, pcmSamplesCount, lowFreq, highFreq).MagL
        Dim result As New MotionExplorerResult With {.Duration = mag(0).Length * MyBase.SonogramRowDuration,
                                                     .Pcm = If(pcmOutput, _waterfallPlayer.Process(mag, _blindZone), Nothing)}

        'DSP
        Dim squelchInDb = MyBase.Db(AutoGainAndGetSquelch(mag, _brightness), _zeroDbLevel)
        MyBase.DbScale(mag, _zeroDbLevel, squelchInDb)
        DopplerFilterDb(mag, _NZeroes)

        'Detection
        Return WaterfallDetector(result, mag, _zeroDbLevel, blindZone, displayLeft, displayRightWithLeft, displayCenter, displayRight, imageOutput)
    End Function

    ''' <summary>
    ''' Детектирование всплесков на "водопаде"
    ''' </summary>
    ''' <param name="result">Объект "Результат анализа движения" (под заполнение).</param>
    ''' <param name="mag">Магнитудная сонограмма ("водопад").</param>
    ''' <param name="zeroDbLevel">"Нулевой" уровень логарифмической шкалы.</param>
    ''' <param name="blindZone">"Слепая зона" для подавления несущей частоты.</param>
    ''' <param name="displayLeft">Отображать разметку слева?</param>
    ''' <param name="displayRightWithLeft">Отображать "правую" разметку вместе с разметкой слева?</param>
    ''' <param name="displayCenter">Отображать центральную область разметки?</param>
    ''' <param name="displayRight">Отображать разметку справа?</param>
    ''' <param name="imageOutput">Выводить изображение?</param>
    ''' <returns>"Результат анализа движения".</returns>
    Private Function WaterfallDetector(result As MotionExplorerResult, mag As Double()(), zeroDbLevel As Double, blindZone As Integer,
                                       displayLeft As Boolean, displayRightWithLeft As Boolean, displayCenter As Boolean, displayRight As Boolean,
                                       imageOutput As Boolean) As MotionExplorerResult
        Dim dopplerWindowWidth = (mag(0).Length - blindZone) \ 2
        Dim lowDopplerLowHarm = 0
        Dim lowDopplerHighHarm = dopplerWindowWidth - 1
        Dim highDopplerLowHarm = mag(0).Length - dopplerWindowWidth
        Dim highDopplerHighHarm = mag(0).Length - 1

        Dim lowDoppler = ExactPlotter.SubBand(mag, lowDopplerLowHarm, lowDopplerHighHarm)
        Dim highDoppler = ExactPlotter.SubBand(mag, highDopplerLowHarm, highDopplerHighHarm)
        Dim sideWidth = mag(0).Length \ _sideFormDivider
        Dim lowDopplerImage = MyBase.HarmSlicesSumImageDb(lowDoppler, sideWidth)
        Dim highDopplerImage = MyBase.HarmSlicesSumImageDb(highDoppler, sideWidth)

        Dim magRGB = If(imageOutput, _paletteProcessor.Process(mag), Nothing)
        Dim sideL = _paletteProcessor.Process(lowDopplerImage)
        Dim sideR = _paletteProcessor.Process(highDopplerImage)

        'Наполнение векторов данными о доплеровских всплесках
        For i = 0 To magRGB.Height - 1
            Dim isLowDopplerMotion = If(Math.Max(Math.Max(sideL.Red(0, i), sideL.Green(0, i)), sideL.Blue(0, i)) <> 0, 1, 0)
            Dim isHighDopplerMotion = If(Math.Max(Math.Max(sideR.Red(0, i), sideR.Green(0, i)), sideR.Blue(0, i)) <> 0, 1, 0)
            Dim lowDopplerMotionVal = isLowDopplerMotion * (100 / CSng(magRGB.Height))
            Dim highDopplerMotionVal = isHighDopplerMotion * (100 / CSng(magRGB.Height))
            With result
                .LowDoppler.AddLast(lowDopplerMotionVal)
                .HighDoppler.AddLast(highDopplerMotionVal)
            End With
        Next

        'Bitmap-вывод
        If imageOutput Then
            Dim rightSideOffset = magRGB.Width - sideWidth
            Parallel.For(0, 3, Sub(channel)

                                   'Изображения
                                   Dim image = magRGB.Matrix(channel)
                                   For i = 0 To magRGB.Height - 1
                                       'Разметка полос
                                       If displayCenter Then
                                           If channel = _blueChannel Then
                                               image(lowDopplerHighHarm, i) = Byte.MaxValue
                                           End If

                                           If channel = _redChannel Then
                                               image(highDopplerLowHarm, i) = Byte.MaxValue
                                           End If
                                       End If

                                       If displayLeft Then
                                           'Линии-ограничители "L"
                                           If channel = _blueChannel Then
                                               image(sideWidth - 1, i) = Byte.MaxValue * 1.0
                                               image(sideWidth, i) = Byte.MaxValue * 0.75
                                           Else
                                               If displayRightWithLeft Then
                                                   If channel = _greenChannel Then
                                                       image(sideWidth - 1, i) = 0
                                                       image(sideWidth, i) = 0
                                                   End If
                                               Else
                                                   image(sideWidth - 1, i) = 0
                                                   image(sideWidth, i) = 0
                                               End If
                                           End If

                                           'Всплески "L"
                                           For j = 0 To sideWidth - 2
                                               If channel = _blueChannel Then
                                                   image(j, i) = Math.Max(Math.Max(sideL.Red(j, i), sideL.Green(j, i)), sideL.Blue(j, i))
                                               Else
                                                   If displayRightWithLeft Then
                                                       If channel = _greenChannel Then
                                                           image(j, i) = 0
                                                       End If
                                                   Else
                                                       image(j, i) = 0
                                                   End If
                                               End If
                                           Next
                                       End If

                                       If displayRightWithLeft Then
                                           'Линии-ограничители "R"
                                           If channel = _redChannel Then
                                               image(sideWidth - 1, i) = Byte.MaxValue * 1.0
                                               image(sideWidth, i) = Byte.MaxValue * 0.75
                                           End If

                                           'Всплески "R"
                                           For j = 0 To sideWidth - 2
                                               If channel = _redChannel Then
                                                   image(j, i) = Math.Max(Math.Max(sideR.Red(j, i), sideR.Green(j, i)), sideR.Blue(j, i))
                                               End If
                                           Next
                                       End If

                                       If displayRight Then
                                           'Линии-ограничители "R"
                                           If channel = _redChannel Then
                                               image(rightSideOffset - 1, i) = Byte.MaxValue * 0.75
                                               image(rightSideOffset, i) = Byte.MaxValue * 1.0
                                           Else
                                               image(rightSideOffset - 1, i) = 0
                                               image(rightSideOffset, i) = 0
                                           End If

                                           'Всплески "R"
                                           For j = 0 To sideWidth - 2
                                               If channel = _redChannel Then
                                                   image(j + rightSideOffset + 1, i) = Math.Max(Math.Max(sideR.Red(j, i), sideR.Green(j, i)), sideR.Blue(j, i))
                                               Else
                                                   image(j + rightSideOffset + 1, i) = 0
                                               End If
                                           Next
                                       End If
                                   Next
                               End Sub)
        End If

        'Сохраняем графический результат - есть он или нет...
        result.Image = magRGB

        Return result
    End Function

    ''' <summary>
    ''' Фильтр доплеровских всплесков
    ''' </summary>
    ''' <param name="mag">Магнитудная сонограмма ("водопад").</param>
    ''' <param name="NZeroes">Допустимое количество "нулевых" уровней.</param>
    Private Sub DopplerFilterDb(mag As Double()(), NZeroes As Integer)
        Dim center = mag(0).Length / 2
        Parallel.For(0, mag.Length, Sub(i)
                                        Dim zeroCount As Integer
                                        Dim lastNonZero As New Queue(Of Double)
                                        Dim row = mag(i)

                                        'Нижняя доплеровская полоса
                                        zeroCount = 0
                                        lastNonZero.Clear()
                                        For j = center To 0 Step -1
                                            If row(j) > Double.MinValue Then lastNonZero.Enqueue(row(j))
                                            If row(j) = Double.MinValue Then zeroCount += 1
                                            If zeroCount > NZeroes Then row(j) = Double.MinValue Else row(j) = If(lastNonZero.Any(), lastNonZero.Average(), Double.MinValue)
                                        Next

                                        'Верхняя доплеровская полоса
                                        zeroCount = 0
                                        lastNonZero.Clear()
                                        For j = center To row.Length - 1
                                            If row(j) > Double.MinValue Then lastNonZero.Enqueue(row(j))
                                            If row(j) = Double.MinValue Then zeroCount += 1
                                            If zeroCount > NZeroes Then row(j) = Double.MinValue Else row(j) = If(lastNonZero.Any(), lastNonZero.Average(), Double.MinValue)
                                        Next
                                    End Sub)
    End Sub

    ''' <summary>
    ''' Применение автоусиления и вычисление порога отсечки (для разделения сигнал/шум)
    ''' </summary>
    ''' <param name="mag">Магнитудная сонограмма ("водопад").</param>
    ''' <param name="brightness">"Яркость".</param>
    ''' <returns>Порог отсечки (для разделения сигнал/шум).</returns>
    Private Function AutoGainAndGetSquelch(mag As Double()(), brightness As Double) As Double
        'Порог "отсечки"
        Dim squelchInDb As Double = 0

        'Корректировка яркости
        If brightness < 1 Then brightness = 1
        If brightness > 100 Then brightness = 100

        'Вычисление средней энергии по центру спектра
        Dim center = mag(0).Length / 2
        Dim currentNRG As Double = 0
        Parallel.For(0, mag.Length, Sub(i)
                                        Dim row = mag(i)
                                        For j = center - _gainHarmRadius To center + _gainHarmRadius Step 1
                                            currentNRG += row(j)
                                        Next
                                    End Sub)
        currentNRG /= CDbl(mag.Length * (2 * _gainHarmRadius + 1))

        'Невязка усиления и его корректировка
        Dim gain = (_targetNRG * (brightness / 100.0)) / currentNRG
        Dim gainDiff = gain - _gain
        Dim gainCorrection = gainDiff * _gainStep
        _gain += gainCorrection

        'Уровень "шумовой" энергии
        Dim noiseMaxNRG = Double.MinValue
        Parallel.For(0, mag.Length, Sub(i)
                                        'Усиление
                                        Dim row = mag(i)
                                        For j = 0 To row.Length - 1
                                            row(j) *= _gain
                                        Next

                                        'Максимальная энергия по границам слева и справа - её нужно отсечь!
                                        'Локатор сканирует боковые полосы - выбирая минимальный максимум из двух полос -
                                        'так обеспечивается невозможность "захвата" помехи в качестве уровня фонового шума.
                                        For k = 0 To Math.Round(mag(0).Length * _noiseScannerDepth) - 1
                                            noiseMaxNRG = Math.Min(If(row(k) > noiseMaxNRG, row(k), noiseMaxNRG),
                                                                   If(row(row.Length - k - 1) > noiseMaxNRG, row(row.Length - k - 1), noiseMaxNRG))
                                        Next

                                    End Sub)
        Dim result = noiseMaxNRG * _noiseGuardCoeff

        Return result
    End Function
End Class
