Imports ExactAudio.MotionExplorer

''' <summary>
''' Доплеровский акустический детектор
''' </summary>
Public Class ExactDoppler
    Public Class ExactDopplerConfig
        Public ReadOnly Property CenterFreq As Double
        Public ReadOnly Property DeadZone As Integer
        Public ReadOnly Property DisplayLeft As Boolean
        Public ReadOnly Property DisplayRightWithLeft As Boolean
        Public ReadOnly Property DisplayCenter As Boolean
        Public ReadOnly Property DisplayRight As Boolean
        Public ReadOnly Property PcmOutput As Boolean
        Public ReadOnly Property ImageOutput As Boolean

        Public Sub New(centerFreq As Double, deadZone As Integer, displayLeft As Boolean,
                       displayRightWithLeft As Boolean, displayCenter As Boolean, displayRight As Boolean,
                       pcmOutput As Boolean, imageOutput As Boolean)
            Me.CenterFreq = centerFreq
            Me.DeadZone = deadZone
            Me.DisplayLeft = displayLeft
            Me.DisplayRightWithLeft = displayRightWithLeft
            Me.DisplayCenter = displayCenter
            Me.DisplayRight = displayRight
            Me.PcmOutput = pcmOutput
            Me.ImageOutput = imageOutput
        End Sub
    End Class

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
                _capture = New WaveInSource(_inputDeviceIdx, _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallSeconds) With {.SampleProcessor = AddressOf SampleProcessor}
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

    Private _config As ExactDopplerConfig
    Public Property Config As ExactDopplerConfig
        Get
            SyncLock Me
                Return _config
            End SyncLock
        End Get
        Set(value As ExactDopplerConfig)
            _config = value
        End Set
    End Property

    Public Event SamplesProcessed(motionExplorerResult As MotionExplorerResult)

    Public Sub New()
        Me.New(Nothing)
    End Sub

    Public Sub New(config As ExactDopplerConfig)
        If config IsNot Nothing Then
            _config = config
        End If
        _generator = New Generator(_outputDeviceIdx, _sampleRate)
        _capture = New WaveInSource(_inputDeviceIdx, _sampleRate, _nBitsCapture, False, _sampleRate * _waterfallSeconds)
        _motionExplorer = New MotionExplorer(_windowSize, _windowStep, _sampleRate, _nBitsPalette, False)
    End Sub

    Public Function Process(samples As Single(), samplesCount As Integer) As MotionExplorerResult
        SyncLock Me
            'Параметры
            Dim lowFreq As Double = 0
            Dim highFreq As Double = 0
            Dim play As Boolean = False

            If _config.CenterFreq = 0 Then
                lowFreq = 0
                highFreq = _topFreq
            Else
                lowFreq = _config.CenterFreq - _dopplerSize
                highFreq = _config.CenterFreq + _dopplerSize
                lowFreq = If(lowFreq < 0, 0, lowFreq)
                highFreq = If(highFreq > _topFreq, _topFreq, highFreq)
            End If

            'Обработка
            Dim motionExplorerResult = _motionExplorer.Process(samples, samplesCount, lowFreq, highFreq, _config.DeadZone, _config.DisplayLeft,
                                                               _config.DisplayRightWithLeft, _config.DisplayCenter, _config.DisplayRight,
                                                               _config.PcmOutput, _config.ImageOutput)

            'Допплер-лог
            Dim nowTimeStamp = DateTime.Now
            Dim lowDopplerSum = motionExplorerResult.LowDoppler.Sum()
            Dim highDopplerSum = motionExplorerResult.HighDoppler.Sum()
            If lowDopplerSum <> 0 Or highDopplerSum <> 0 Then
                _dopplerLog.Add(nowTimeStamp, lowDopplerSum, highDopplerSum)
            End If

            Return motionExplorerResult
        End SyncLock
    End Function

    Private Sub SampleProcessor(samples As Single(), samplesCount As Integer)
        Dim motionExplorerResult = Process(samples, samplesCount)
        RaiseEvent SamplesProcessed(motionExplorerResult)
    End Sub

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
                .SampleProcessor = AddressOf SampleProcessor
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
