Imports Bwl.Imaging
Imports DrAF.DSP

Public Class MotionExplorer
    Inherits FFTExplorer

    Private Const _redChannel = 0 '0
    Private Const _greenChannel = 1 '1
    Private Const _blueChannel = 2 '2
    Private Const _blankerLevel = 32 '32
    Private Const _sideFormDivider = 20 '20
    Private Const _gainHarmRadius = 2 '2
    Private Const _gainStep = 1.0 '1.0
    Private Const _NZeroes = 3 ' 3    
    Private Const _noiseScannerDepth = 0.1 '0.1
    Private Const _brightness = 100 '100
    Private Const _minFreq = 200 '200
    Private Const _blindZone = 12 '12
    Private Const _carrierRadius = 2 '2

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
    ''' <returns>"Результат анализа движения".</returns>
    Public Function Process(pcmSamples As Single(), pcmSamplesCount As Integer, lowFreq As Double, highFreq As Double, blindZone As Integer) As MotionExplorerResult
        'FFT
        Dim mag = MyBase.Explore(pcmSamples, pcmSamplesCount, lowFreq, highFreq).MagL
        Dim result As New MotionExplorerResult With {.Duration = mag(0).Length * MyBase.SonogramRowDuration,
                                                     .Pcm = _waterfallPlayer.Process(mag, _blindZone)}
        'DSP
        Dim squelchInDb = MyBase.Db(AutoGainAndGetSquelch(mag, _brightness), _zeroDbLevel)
        MyBase.DbScale(mag, _zeroDbLevel, squelchInDb)
        DopplerFilterDb(mag, _NZeroes)

        'Detection
        Return WaterfallDetector(result, mag, _zeroDbLevel, blindZone)
    End Function

    ''' <summary>
    ''' Детектирование всплесков на "водопаде"
    ''' </summary>
    ''' <param name="result">Объект "Результат анализа движения" (под заполнение).</param>
    ''' <param name="mag">Магнитудная сонограмма ("водопад").</param>
    ''' <param name="zeroDbLevel">"Нулевой" уровень логарифмической шкалы.</param>
    ''' <param name="blindZone">"Слепая зона" для подавления несущей частоты.</param>
    ''' <returns>"Результат анализа движения".</returns>
    Private Function WaterfallDetector(result As MotionExplorerResult, mag As Double()(), zeroDbLevel As Double, blindZone As Integer) As MotionExplorerResult
        Dim dopplerWindowWidth = (mag(0).Length - blindZone) \ 2
        Dim lowDopplerLowHarm = 0
        Dim lowDopplerHighHarm = dopplerWindowWidth - 1
        Dim highDopplerLowHarm = mag(0).Length - dopplerWindowWidth
        Dim highDopplerHighHarm = mag(0).Length - 1
        Dim centerHarm = (lowDopplerHighHarm + highDopplerLowHarm) \ 2
        Dim carrierLowHarm = (centerHarm - _carrierRadius)
        Dim carrierHighHarm = (centerHarm + _carrierRadius)
        Dim carrierNorm = ((carrierHighHarm - carrierLowHarm) - 1)

        Dim lowDoppler = ExactPlotter.SubBand(mag, lowDopplerLowHarm, lowDopplerHighHarm)
        Dim highDoppler = ExactPlotter.SubBand(mag, highDopplerLowHarm, highDopplerHighHarm)
        Dim sideWidth = mag(0).Length \ _sideFormDivider
        Dim lowDopplerImage = MyBase.HarmSlicesSumImageInDb(lowDoppler, 1)
        Dim highDopplerImage = MyBase.HarmSlicesSumImageInDb(highDoppler, 1)

        Dim magRGB = _paletteProcessor.Process(mag)
        Dim sideL = _paletteProcessor.Process(lowDopplerImage)
        Dim sideR = _paletteProcessor.Process(highDopplerImage)

        'Наполнение векторов данными о доплеровских всплесках
        For i = 0 To mag.Length - 1
            Dim lowDopplerNrg = MaxRGB(sideL.Red(0, i), sideL.Green(0, i), sideL.Blue(0, i))
            Dim highDopplerNrg = MaxRGB(sideR.Red(0, i), sideR.Green(0, i), sideR.Blue(0, i))
            Dim lowDopplerMotionVal = (lowDopplerNrg / CSng(Byte.MaxValue)) * 100
            Dim highDopplerMotionVal = (highDopplerNrg / CSng(Byte.MaxValue)) * 100
            With result
                .LowDoppler.AddLast(lowDopplerMotionVal)
                .HighDoppler.AddLast(highDopplerMotionVal)
            End With
            '...и рассчитываем уровень несущей
            Dim carrierLevel = 0
            For j = carrierLowHarm + 1 To carrierHighHarm - 1
                carrierLevel += MaxRGB(magRGB.Red(j, i), magRGB.Green(j, i), magRGB.Blue(j, i))
            Next
            carrierLevel /= carrierNorm
            carrierLevel *= (100.0 / Byte.MaxValue)
            result.CarrierLevel.AddLast(carrierLevel)
        Next

        'Bitmap-вывод
        Dim rightSideOffset = magRGB.Width - sideWidth
        Parallel.For(0, 3, Sub(channel)
                               Dim image = magRGB.Matrix(channel)
                               For i = 0 To magRGB.Height - 1
                                   For j = lowDopplerHighHarm To centerHarm - _carrierRadius
                                       If channel = _redChannel Then
                                           image(j, i) = MaxRGB(sideR.Red(0, i), sideR.Green(0, i), sideR.Blue(0, i))
                                       End If
                                       If channel = _greenChannel Then
                                           image(j, i) = _blankerLevel
                                       End If
                                       If channel = _blueChannel Then
                                           image(j, i) = MaxRGB(sideL.Red(0, i), sideL.Green(0, i), sideL.Blue(0, i))
                                       End If
                                   Next
                                   For j = centerHarm + _carrierRadius To highDopplerLowHarm
                                       If channel = _redChannel Then
                                           image(j, i) = MaxRGB(sideR.Red(0, i), sideR.Green(0, i), sideR.Blue(0, i))
                                       End If
                                       If channel = _greenChannel Then
                                           image(j, i) = _blankerLevel
                                       End If
                                       If channel = _blueChannel Then
                                           image(j, i) = MaxRGB(sideL.Red(0, i), sideL.Green(0, i), sideL.Blue(0, i))
                                       End If
                                   Next

                               Next
                           End Sub)

        'Сохраняем графический результат - есть он или нет...
        result.Image = magRGB

        Return result
    End Function

    Private Function MaxRGB(R As Byte, G As Byte, B As Byte) As Byte
        Return Math.Max(R, Math.Max(G, B))
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
                                            If zeroCount > NZeroes Then
                                                row(j) = Double.MinValue
                                            Else
                                                row(j) = If(lastNonZero.Any(), lastNonZero.Average(), Double.MinValue)
                                            End If
                                        Next

                                        'Верхняя доплеровская полоса
                                        zeroCount = 0
                                        lastNonZero.Clear()
                                        For j = center To row.Length - 1
                                            If row(j) > Double.MinValue Then lastNonZero.Enqueue(row(j))
                                            If row(j) = Double.MinValue Then zeroCount += 1
                                            If zeroCount > NZeroes Then
                                                row(j) = Double.MinValue
                                            Else
                                                row(j) = If(lastNonZero.Any(), lastNonZero.Average(), Double.MinValue)
                                            End If
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
        Dim result = noiseMaxNRG
        Return result
    End Function
End Class
