Imports Bwl.Imaging
Imports DrAF.DSP

''' <summary>
''' Анализатор доплеровских всплесков
''' </summary>
Public Class MotionExplorer
    Private Const _blankerLevel = 32 '32
    Private Const _sideFormDivider = 20 '20
    Private Const _gainHarmRadius = 2 '2
    Private Const _gainStep = 1.0 '1.0
    Private Const _rowFilterMemorySize = 3 '3
    Private Const _NZeroes = 7 '7
    Private Const _noiseScannerDepth = 0.1 '0.1
    Private Const _brightness = 100 '100
    Private Const _minFreq = 200 '200
    Private Const _carrierRadius = 2 '2
    Private _targetNRG As Double = 0 '0
    Private _gain As Double = 1.0 '1.0

    Private _paletteProcessor As PaletteProcessor 'Объект для работы с палитрой
    Private _fftExplorer As FFTExplorer 'Класс для обработки результатов FFT

    ''' <summary>
    ''' Конструктор
    ''' </summary>
    ''' <param name="nBits">Разрядность.</param>
    ''' <param name="fftExplorer">Класс для обработки результатов FFT.</param>
    Public Sub New(nBits As Integer, fftExplorer As FFTExplorer)
        _targetNRG = Math.Pow(2, nBits - 1)
        _paletteProcessor = New PaletteProcessor()
        _fftExplorer = fftExplorer
    End Sub

    ''' <summary>
    ''' Основной метод обработки
    ''' </summary>
    ''' <param name="mag">Магнитуды (фрагмент "водопада").</param>
    ''' <param name="blindZone">"Слепая зона" для подавления несущей частоты.</param>
    ''' <returns>"Результат анализа движения".</returns>
    Public Function Process(mag As Double()(), blindZone As Integer) As MotionExplorerResult
        Dim result As New MotionExplorerResult With {.Duration = mag.Length * _fftExplorer.SonogramRowDuration}

        'DbScale
        Dim squelchInDb = ExactAudioMath.Db(AutoGainAndGetSquelch(mag, _brightness), _fftExplorer.ZeroDbLevel)
        Dim magRaw = ImageUtils.FullClone(mag)
        ExactAudioMath.DbScale(mag, _fftExplorer.ZeroDbLevel, 10)
        ExactAudioMath.DbScale(magRaw, _fftExplorer.ZeroDbLevel, 20)

        'Raw Image
        result.RawDopplerImage = _paletteProcessor.Process(magRaw, -120) 'Необработанное изображение (включая несущую)

        'Doppler Filtering
        ExactAudioMath.DbSquelch(mag, squelchInDb)
        DopplerFilterDb(mag, _rowFilterMemorySize, _NZeroes)

        'Detection
        WaterfallDetector(result, mag, _fftExplorer.ZeroDbLevel, blindZone)

        Return result
    End Function

    ''' <summary>
    ''' Детектирование всплесков на "водопаде"
    ''' </summary>
    ''' <param name="result">Объект "Результат анализа движения" (под заполнение).</param>
    ''' <param name="mag">Магнитудная сонограмма ("водопад").</param>
    ''' <param name="zeroDbLevel">"Нулевой" уровень логарифмической шкалы.</param>
    ''' <param name="blindZone">"Слепая зона" для подавления несущей частоты.</param>
    Private Sub WaterfallDetector(result As MotionExplorerResult, mag As Double()(), zeroDbLevel As Double, blindZone As Integer)
        Dim dopplerWindowWidth = CInt(Math.Round((mag(0).Length - blindZone) / 2.0)) 'Ширина доплеровского окна
        Dim lowDopplerLowHarm = 0 'Нижняя гармоника нижнего доплеровского окна
        Dim lowDopplerHighHarm = dopplerWindowWidth - 1 'Верхняя гармоника нижнего доплеровского окна
        Dim highDopplerLowHarm = mag(0).Length - dopplerWindowWidth 'Нижняя гармоника верхнего доплеровского окна
        Dim highDopplerHighHarm = mag(0).Length - 1 'Верхняя гармоника верхнего доплеровского окна
        Dim carrierCenterHarm = CInt(Math.Round((lowDopplerHighHarm + highDopplerLowHarm) / 2.0)) 'Центральная гармоника сигнала
        Dim carrierLowHarm = (carrierCenterHarm - _carrierRadius) 'Нижняя граница несущей
        Dim carrierHighHarm = (carrierCenterHarm + _carrierRadius) 'Верхняя граница несущей
        Dim carrierNrgNormDivider = ((carrierHighHarm - carrierLowHarm) - 1) 'Нормализующий коэффициент для энергии несущей
        Dim lowDoppler = ExactPlotter.SubBand(mag, lowDopplerLowHarm, lowDopplerHighHarm) 'Выделение поддиапазона нижней доплеровской полосы
        Dim highDoppler = ExactPlotter.SubBand(mag, highDopplerLowHarm, highDopplerHighHarm) 'Выделение поддиапазона верхней доплеровской полосы
        Dim lowDopplerImage = HarmSlicesSumImageInDb(lowDoppler, 1) 'Суммирование энергии по нижней доплеровской полосе
        Dim highDopplerImage = HarmSlicesSumImageInDb(highDoppler, 1) 'Суммирование энергии по верхней доплеровской полосе

        Dim magRGB = _paletteProcessor.Process(mag) 'Получение изображения с участием палитры под дальнейшую разметку
        Dim sideL = _paletteProcessor.Process(lowDopplerImage) 'Изображение нижней доплеровской полосы
        Dim sideR = _paletteProcessor.Process(highDopplerImage) 'Изображение верхней доплеровской полосы

        'Наполнение векторов данными о доплеровских всплесках
        For y = 0 To mag.Length - 1
            Dim lowDopplerNrg = MaxRGB(sideL.RedPixel(0, y), sideL.GreenPixel(0, y), sideL.BluePixel(0, y))
            Dim highDopplerNrg = MaxRGB(sideR.RedPixel(0, y), sideR.GreenPixel(0, y), sideR.BluePixel(0, y))
            Dim lowDopplerMotionVal = (lowDopplerNrg / CSng(Byte.MaxValue)) * 100
            Dim highDopplerMotionVal = (highDopplerNrg / CSng(Byte.MaxValue)) * 100
            With result
                .LowDoppler.AddLast(lowDopplerMotionVal)
                .HighDoppler.AddLast(highDopplerMotionVal)
            End With
            '...и рассчитываем уровень несущей
            Dim carrierLevel = 0
            For x = carrierLowHarm + 1 To carrierHighHarm - 1 'Применяем защитные границы при суммировании
                carrierLevel += MaxRGB(magRGB.RedPixel(x, y), magRGB.GreenPixel(x, y), magRGB.BluePixel(x, y)) 'Палитра может быть любая (но линейная)!
            Next
            carrierLevel /= carrierNrgNormDivider
            carrierLevel *= (100.0 / Byte.MaxValue)
            result.CarrierLevel.AddLast(carrierLevel)
        Next

        'Bitmap-вывод        
        Parallel.For(0, 3, Sub(channel As Integer)
                               For y = 0 To magRGB.Height - 1
                                   'Левая и правая часть "индикатора"
                                   For x = lowDopplerHighHarm To carrierCenterHarm - _carrierRadius
                                       If channel = SharedConsts.RedChannel Then
                                           magRGB.MatrixPixel(channel, x, y) = MaxRGB(sideR.RedPixel(0, y), sideR.GreenPixel(0, y), sideR.BluePixel(0, y))
                                       End If
                                       If channel = SharedConsts.GreenChannel Then
                                           magRGB.MatrixPixel(channel, x, y) = _blankerLevel
                                       End If
                                       If channel = SharedConsts.BlueChannel Then
                                           magRGB.MatrixPixel(channel, x, y) = MaxRGB(sideL.RedPixel(0, y), sideL.GreenPixel(0, y), sideL.BluePixel(0, y))
                                       End If
                                   Next
                                   For x = carrierCenterHarm + _carrierRadius To highDopplerLowHarm
                                       If channel = SharedConsts.RedChannel Then
                                           magRGB.MatrixPixel(channel, x, y) = MaxRGB(sideR.RedPixel(0, y), sideR.GreenPixel(0, y), sideR.BluePixel(0, y))
                                       End If
                                       If channel = SharedConsts.GreenChannel Then
                                           magRGB.MatrixPixel(channel, x, y) = _blankerLevel
                                       End If
                                       If channel = SharedConsts.BlueChannel Then
                                           magRGB.MatrixPixel(channel, x, y) = MaxRGB(sideL.RedPixel(0, y), sideL.GreenPixel(0, y), sideL.BluePixel(0, y))
                                       End If
                                   Next
                               Next
                           End Sub)

        'Сохраняем графический результат - есть он или нет...
        result.DopplerImage = magRGB
    End Sub

    ''' <summary>
    ''' Получение изображения, каждая строка которого содержит "размноженную" сумму гармоник исходной строки
    ''' </summary>
    ''' <param name="mag">Магнитудная сонограмма ("водопад").</param>
    ''' <param name="width">Требуемая ширина итогового изображения.</param>
    Private Function HarmSlicesSumImageInDb(mag As Double()(), width As Integer) As Double()()
        Dim result = New Double(mag.Length - 1)() {}
        For i = 0 To result.Length - 1
            result(i) = New Double(width - 1) {}
        Next
        Parallel.For(0, mag.Length, Sub(y As Integer)
                                        Dim row = mag(y)
                                        Dim sum As Double = 0
                                        For x = 0 To row.Length - 1
                                            If row(x) > Double.MinValue Then
                                                sum += ExactAudioMath.DbInv(row(x), _fftExplorer.ZeroDbLevel) '[1] Re-Exp
                                            End If
                                        Next
                                        sum /= CDbl(row.Length)
                                        sum = ExactAudioMath.Db(sum, _fftExplorer.ZeroDbLevel) '[2] Re-Log

                                        Dim target = result(y)
                                        For col = 0 To target.Length - 1
                                            target(col) = If(Double.IsNaN(sum), Double.MinValue, sum)
                                        Next
                                    End Sub)
        Return result
    End Function

    ''' <summary>
    ''' Фильтр доплеровских всплесков
    ''' </summary>
    ''' <param name="mag">Магнитудная сонограмма ("водопад").</param>
    ''' <param name="rowFilterMemorySize">Размер "двоичной" памяти.</param>
    ''' <param name="NZeroes">Допустимое количество "нулевых" уровней.</param>
    Private Sub DopplerFilterDb(mag As Double()(), rowFilterMemorySize As Integer, NZeroes As Integer)
        Dim center = mag(0).Length / 2
        Parallel.For(0, mag.Length, Sub(y As Integer)
                                        Dim row = mag(y)

                                        'Нижняя доплеровская полоса
                                        Dim rowDopplerFilter As New RowDopplerFilter(rowFilterMemorySize, NZeroes)
                                        For x = center To 0 Step -1
                                            row(x) = rowDopplerFilter.Process(row(x))
                                        Next

                                        'Верхняя доплеровская полоса
                                        rowDopplerFilter.Reset(rowFilterMemorySize, NZeroes)
                                        For x = center To row.Length - 1
                                            row(x) = rowDopplerFilter.Process(row(x))
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
        'Корректировка яркости
        brightness = If(brightness < 1, 1, brightness)
        brightness = If(brightness > 100, 100, brightness)

        'Вычисление средней энергии по центру спектра
        Dim center = mag(0).Length / 2
        Dim currentNRG As Double = 0
        Parallel.For(0, mag.Length, Sub(y As Integer)
                                        Dim row = mag(y)
                                        For x = center - _gainHarmRadius To center + _gainHarmRadius Step 1
                                            currentNRG += row(x)
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
        Parallel.For(0, mag.Length, Sub(y As Integer)
                                        'Усиление
                                        Dim row = mag(y)
                                        For x = 0 To row.Length - 1
                                            row(x) *= _gain
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

    Private Function MaxRGB(R As Byte, G As Byte, B As Byte) As Byte
        Return Math.Max(R, Math.Max(G, B))
    End Function
End Class
