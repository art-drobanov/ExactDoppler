Imports DrAF.DSP

Public Class WaterfallPlayer
    Private _sampleRate As Integer
    Private _zeroDbLevel As Integer
    Private _timeSliceDuration As Double
    Private _minFreqIdx As Integer

    Private _fftObj As ExactFFT.CFFT_Object

    Public ReadOnly Property SonogramRowDuration As Double
        Get
            Return _timeSliceDuration
        End Get
    End Property

    Public Sub New(frameWidth As Integer, frameStep As Integer, sampleRate As Integer, nBits As Integer, minFreq As Double)
        _sampleRate = sampleRate
        _zeroDbLevel = CInt(Math.Pow(2, ExactFFT.ToLowerPowerOf2(nBits) - 1))

        'Косинусное взвешивающее окно "BLACKMAN-HARRIS" (оптимальное по совокупности характеристик)
        Dim cosTW As ExactFFT.CosTW = ExactFFT.CosTW.BLACKMAN_HARRIS_92dbPS
        Dim polyDiv2 = 1
        Dim isComplex = False
        _fftObj = ExactFFT.CFFT_Constructor_Cosine(frameWidth, cosTW, polyDiv2, frameStep, isComplex)
        _minFreqIdx = ExactFFT.FFT_Node(minFreq, sampleRate, _fftObj.N, False)

        'Считываем длительность одной строки сонограммы
        Dim frameDuration, sleepCoeff, timeSliceDuration As Double
        ExactPlotter.GetFrameParameters(_fftObj.N, _fftObj.WindowStep, _sampleRate, frameDuration, sleepCoeff, timeSliceDuration)
        _timeSliceDuration = timeSliceDuration
    End Sub

    Public Function Process(mag As Double()(), deadZone As Integer) As Single()
        Dim pcm As New Queue(Of Single)

        For Each magRow In mag
            Dim rowPcm = ProcessMagRow(magRow, deadZone)
            For Each sample In rowPcm
                pcm.Enqueue(sample)
            Next
        Next

        Return pcm.ToArray()
    End Function

    Private Function ProcessMagRow(magRow As Double(), deadZone As Integer) As Single()

        'Требуется определить, в какой части спектра будет располагаться сонограмма.
        'Если ширина блока больше ширины, установленной в конструкторе - исключение!
        'Если это полный блок - просто воспроизводим.
        'Если фрагмент - размещаем так, чтобы "было слышно".

        Dim N2 = _fftObj.N >> 1 '(Количество точек FFT / 2) - количество гармоник вместе с нулевой
        If magRow.Length > N2 Then
            Throw New Exception("magRow.Length > _fftObj.N")
        Else
            If magRow.Length <> N2 Then 'Входной блок слишком большой
                If ((_minFreqIdx + 1) + magRow.Length) > N2 Then
                    Throw New Exception("((_minFreqIdx + 1) + magRow.Length) > _fftObj.N") 'Не вписать!
                End If
            End If
        End If

        'Зануление центра
        magRow = magRow.Clone()
        Dim center = magRow.Length / 2
        Dim blankRadius = deadZone / 2
        Dim left = center - blankRadius
        Dim right = left + deadZone
        For i = left To right
            magRow(i) = 0
        Next

        'Формирование полной строки магнитуд
        Dim magRowFull As Double()
        If magRow.Length <> N2 Then
            magRowFull = New Double(N2 - 1) {}
            For i = 0 To magRow.Length - 1
                magRowFull(_minFreqIdx + i) = magRow(i)
            Next
        Else
            magRowFull = magRow
        End If

        'Получение массива FFT_T для целей обратного преобразования
        Dim FFT_T = New Double(_fftObj.NN - 1) {}
        Dim FFT_S = New Double(_fftObj.NN - 1) {}
        Parallel.For(1, N2, Sub(i)
                                Dim magValue = magRowFull(i - 1)
                                FFT_T((i << 1) + 0) = magValue
                                FFT_T((i << 1) + 1) = 0
                                FFT_T(((_fftObj.N - i) << 1) + 0) = magValue
                                FFT_T(((_fftObj.N - i) << 1) + 1) = 0
                            End Sub)

        'Используем аннигиляцию взвешивающего окна, работаем
        'с нормализацией - направление обратное
        Dim useTaperWindow = True
        Dim FFT_S_Offset = 0
        Dim recoverAfterTaperWindow = True
        Dim useNorm = False
        Dim direction = False
        Dim usePolyphase = False
        ExactFFT.CFFT_Process(FFT_T, FFT_S_Offset, FFT_S, useTaperWindow, recoverAfterTaperWindow, useNorm, direction, usePolyphase, _fftObj)
        Dim rowPcm = GetPcm(FFT_S)

        Return rowPcm
    End Function

    Private Function GetPcm(FFT_S As Double()) As Single()
        Dim pcmOut = New Single(_fftObj.WindowStep - 1) {}
        Dim fullPcmWidth = FFT_S.Length / 2
        Dim center = CInt(Math.Round(fullPcmWidth / 2.0))
        Dim radius = CInt(Math.Round((_fftObj.WindowStep - 1) / 2.0))
        Dim leftBound = center - radius
        Dim rightBound = leftBound + (pcmOut.Length - 1)
        Dim boundSize = (rightBound - leftBound) + 1
        If boundSize <> pcmOut.Length Then
            Throw New Exception("boundSize <> pcmOut.Length")
        End If

        For i = leftBound To rightBound
            pcmOut(i - leftBound) = CSng(FFT_S(i << 1))
        Next

        Return pcmOut
    End Function
End Class
