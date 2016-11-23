Imports DrAF.DSP

''' <summary>
''' Анализатор результатов работы FFT
''' </summary>
Public Class FFTExplorer
    Protected _sampleRate As Integer
    Protected _zeroDbLevel As Integer
    Protected _stereo As Boolean
    Protected _timeSliceDuration As Double
    Protected _fftObj As ExactFFT.CFFT_Object

    Public ReadOnly Property ZeroDbLevel As Double
        Get
            Return _zeroDbLevel
        End Get
    End Property

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
        Dim taperWindow = ExactFFT.TaperWindow.BLACKMAN_HARRIS_92dbPS
        Dim polyDiv2 = 1
        Dim isComplex = False
        _fftObj = ExactFFT.CFFT_Constructor_Cosine(frameWidth, taperWindow, polyDiv2, frameStep, isComplex)

        'Вычисляем длительность одной строки сонограммы
        Dim frameDuration, sleepCoeff, timeSliceDuration As Double
        ExactPlotter.GetFrameParameters(_fftObj.N, _fftObj.WindowStep, _sampleRate, frameDuration, sleepCoeff, timeSliceDuration)
        _timeSliceDuration = timeSliceDuration
    End Sub

    ''' <summary>
    ''' Прямое преобразование Фурье с выделением набора параметров (магнитуды, относительные фазы...)
    ''' </summary>
    ''' <param name="pcmSamples">Входные PCM-семплы.</param>
    ''' <param name="pcmSamplesCount">Количество семплов под обработку.</param>
    ''' <param name="magnitudesOnly">Вычислять только магнитуды?</param>
    Public Function Explore(pcmSamples As Single(), pcmSamplesCount As Integer, Optional magnitudesOnly As Boolean = False) As ExactPlotter.CFFT_ExploreResult
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
        Dim res = If(magnitudesOnly, ExactPlotter.ExploreMag(FFT_T, usePolyphase, _fftObj), ExactPlotter.Explore(FFT_T, usePolyphase, _fftObj))

        Return res
    End Function

    ''' <summary>
    ''' Выделение поддиапазона гармоник
    ''' </summary>
    ''' <param name="cfft"></param>
    ''' <param name="lowFreq">Нижняя частота поддиапазона.</param>
    ''' <param name="highFreq">Верхняя частота поддиапазона.</param>
    Public Function SubBand(cfft As ExactPlotter.CFFT_ExploreResult, lowFreq As Double, highFreq As Double, Optional magnitudesOnly As Boolean = False) As ExactPlotter.CFFT_ExploreResult
        Dim res = New ExactPlotter.CFFT_ExploreResult

        Dim lowHarmIdx As Integer = 0
        Dim highHarmIdx As Integer = 0
        Dim harmReverse As Boolean = False
        If magnitudesOnly Then
            With res
                .MagL = ExactPlotter.SubBand(cfft.MagL, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
                .MagR = ExactPlotter.SubBand(cfft.MagR, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
            End With
        Else
            With res
                .MagL = ExactPlotter.SubBand(cfft.MagL, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
                .MagR = ExactPlotter.SubBand(cfft.MagR, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
                .ACH = ExactPlotter.SubBand(cfft.ACH, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
                .ArgL = ExactPlotter.SubBand(cfft.ArgL, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
                .ArgR = ExactPlotter.SubBand(cfft.ArgR, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
                .PhaseLR = ExactPlotter.SubBand(cfft.PhaseLR, lowFreq, highFreq, lowHarmIdx, highHarmIdx, _fftObj, _sampleRate, harmReverse)
            End With
        End If

        Return res
    End Function
End Class
