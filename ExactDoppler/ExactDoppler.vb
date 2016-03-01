Imports ExactAudio.MotionExplorer

''' <summary>
''' Доплеровский акустический детектор
''' </summary>
Public Class ExactDoppler

    'Константы
    Private Const _windowSize = 32768 '32768
    Private Const _windowStep = 1214 '1214 = Round(32768 / (3 * 3 * 3))
    Private Const _sampleRate = 48000 '48000
    Private Const _dopplerSize = 500 '500
    Private Const _nBitsCapture = 16 '16
    Private Const _nBitsPalette = 8 '8
    Private Const _waterfallSeconds = 2 '2
    Private Const _topFreq = 23000 '23000

    'Данные
    Private _inputDeviceIdx As Integer = 0
    Private _outputDeviceIdx As Integer = 0
    Private _dopplerLog As New DopplerLog()

    'Объекты
    Private _capture As WaveInSource
    Private _generator As Generator
    Private _motionExplorer As MotionExplorer

    Public ReadOnly Property SampleRate As Integer
        Get
            Return _sampleRate
        End Get
    End Property

    Public ReadOnly Property TopFreq As Integer
        Get
            Return _topFreq
        End Get
    End Property

    Public ReadOnly Property DopplerSize
        Get
            Return _dopplerSize
        End Get
    End Property

    Public Property InputDeviceIdx As Integer
        Get
            Return _inputDeviceIdx
        End Get
        Set(value As Integer)
            SyncLock Me
                _inputDeviceIdx = value
                _capture = New WaveInSource(_inputDeviceIdx, _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallSeconds) With {.SampleProcessor = Me.SampleProcessor}
            End SyncLock
        End Set
    End Property

    Public Property OutputDeviceIdx As Integer
        Get
            Return _outputDeviceIdx
        End Get
        Set(value As Integer)
            SyncLock Me
                _outputDeviceIdx = value
                _generator = New Generator(_outputDeviceIdx, _sampleRate)
            End SyncLock
        End Set
    End Property

    Public ReadOnly Property DopplerLog As DopplerLog
        Get
            Return _dopplerLog
        End Get
    End Property

    Public Property Volume As Single
        Get
            Return _generator.Volume
        End Get
        Set(value As Single)
            SyncLock Me
                _generator.Volume = value
            End SyncLock
        End Set
    End Property

    Public Property SampleProcessor As SampleProcessorDelegate
        Get
            Return _capture.SampleProcessor
        End Get
        Set(value As SampleProcessorDelegate)
            SyncLock Me
                _capture.SampleProcessor = value
            End SyncLock
        End Set
    End Property

    Public Sub New()
        _generator = New Generator(_outputDeviceIdx, _sampleRate)
        _capture = New WaveInSource(_inputDeviceIdx, _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallSeconds)
        _motionExplorer = New MotionExplorer(_windowSize, _windowStep, _sampleRate, _nBitsPalette, False)
    End Sub

    Public Function Process(samples As Single(), samplesCount As Integer, centerFreq As Double, deadZone As Integer,
                            displayLeft As Boolean, displayRightWithLeft As Boolean, displayCenter As Boolean, displayRight As Boolean,
                            pcmOutput As Boolean, imageOutput As Boolean) As MotionExplorerResult
        SyncLock Me
            Dim lowFreq As Double = 0
            Dim highFreq As Double = 0
            Dim play As Boolean = False

            If centerFreq = 0 Then
                lowFreq = 0
                highFreq = _topFreq
            Else
                lowFreq = centerFreq - _dopplerSize
                highFreq = centerFreq + _dopplerSize
                lowFreq = If(lowFreq < 0, 0, lowFreq)
                highFreq = If(highFreq > _topFreq, _topFreq, highFreq)
            End If

            'Processing
            Dim motionExplorerResult = _motionExplorer.Process(samples, samplesCount, lowFreq, HighFreq, deadZone, displayLeft,
                                                               displayRightWithLeft, displayCenter, displayRight, pcmOutput, imageOutput)

            'DopplerLog
            Dim nowTimeStamp = DateTime.Now
            Dim lowDopplerSum = motionExplorerResult.LowDoppler.Sum()
            Dim highDopplerSum = motionExplorerResult.HighDoppler.Sum()
            If lowDopplerSum <> 0 Or highDopplerSum <> 0 Then
                _dopplerLog.Add(nowTimeStamp, lowDopplerSum, highDopplerSum)
            End If

            Return motionExplorerResult
        End SyncLock
    End Function

    Public Sub SwitchOnGen(sineFreqL As Integer, sineFreqR As Integer, mix As Boolean)
        SyncLock Me
            _generator.SwitchOn(sineFreqL, sineFreqR, mix)
        End SyncLock
    End Sub

    Public Sub SwitchOffGen()
        SyncLock Me
            _generator.SwitchOff()
        End SyncLock
    End Sub

    Public Sub Start()
        SyncLock Me
            With _capture
                .SampleProcessor = Me.SampleProcessor
                .Start()
            End With
        End SyncLock
    End Sub

    Public Sub [Stop]()
        SyncLock Me
            _capture.Stop()
        End SyncLock
    End Sub
End Class
