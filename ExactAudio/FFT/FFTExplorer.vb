Imports DrAF.DSP

''' <summary>
''' FFT-анализатор
''' </summary>
Public Class FFTExplorer
    Protected _sampleRate As Integer
    Protected _zeroDbLevel As Integer
    Protected _stereo As Boolean
    Protected _timeSliceDuration As Double
    Protected _fftObj As ExactFFT.CFFT_Object

    Public ReadOnly Property SonogramRowDuration As Double
        Get
            Return _timeSliceDuration
        End Get
    End Property

    Public Sub New(frameWidth As Integer, frameStep As Integer, sampleRate As Integer, nBits As Integer, stereo As Boolean)
        'Общие параметры
        _sampleRate = sampleRate
        _zeroDbLevel = CInt(Math.Pow(2, ExactFFT.ToLowerPowerOf2(nBits) - 1))
        _stereo = stereo

        'Косинусное взвешивающее окно "BLACKMAN-HARRIS" (оптимальное по совокупности характеристик)
        Dim cosTW As ExactFFT.CosTW = ExactFFT.CosTW.BLACKMAN_HARRIS_92dbPS
        Dim polyDiv2 = 1
        Dim isComplex = False
        _fftObj = ExactFFT.CFFT_Constructor_Cosine(frameWidth, cosTW, polyDiv2, frameStep, isComplex)

        'Вычисляем длительность одной строки сонограммы
        Dim frameDuration, sleepCoeff, timeSliceDuration As Double
        ExactPlotter.GetFrameParameters(_fftObj.N, _fftObj.WindowStep, _sampleRate, frameDuration, sleepCoeff, timeSliceDuration)
        _timeSliceDuration = timeSliceDuration
    End Sub

    Public Function Explore(pcmSamples As Single(), pcmSamplesCount As Integer, lowFreq As Double, highFreq As Double) As ExactPlotter.CFFT_ExploreResult
        'Конфигурация: прямой проход FFT с нормализацией и использованием взвешивающего окна...
        Dim useTaperWindow As Boolean = True
        Dim recoverAfterTaperWindow As Boolean = False
        Dim useNorm As Boolean = True
        Dim direction As Boolean = True
        Dim usePolyphase As Boolean = False
        Dim isMirror As Boolean = True

        'Обеспечиваем L+R
        Dim samplesLR = If(_stereo, pcmSamples, pcmSamples.RealToComplex(0)) 'Если на входе "моно" - нагружаем им только левый канал!

        'Добавляем семплы в обработку (бесшовное соединение блоков samples)...
        ExactFFT.AddSamplesToProcessing(samplesLR, _zeroDbLevel, _fftObj)
        Dim pcmBlock = _fftObj.PlotterPcmQueue.Dequeue()

        'Прямое преобразование Фурье
        Dim remainArrayItemsLRCount As Integer
        Dim FFT_T = ExactPlotter.Process(pcmBlock, 0, useTaperWindow, recoverAfterTaperWindow,
                                         useNorm, direction, usePolyphase, remainArrayItemsLRCount,
                                         _fftObj)

        'Разбор данных после преобразования Фурье
        Dim res = ExactPlotter.Explore(FFT_T, usePolyphase, _fftObj)

        'Выделение поддиапазона гармоник
        Dim lowHarmIdx As Integer = 0
        Dim highHarmIdx As Integer = 0
        Dim harmReverse As Boolean = False
        With res
            .MagL = ExactPlotter.SubBand(res.MagL, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
            .MagR = ExactPlotter.SubBand(res.MagR, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
            .PhaseLR = ExactPlotter.SubBand(res.PhaseLR, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
            .ACH = Nothing
            .ArgL = Nothing
            .ArgR = Nothing
            .Mag = Nothing
            .Arg = Nothing
        End With

        Return res
    End Function
End Class
