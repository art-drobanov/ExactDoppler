Imports NAudio.Wave

''' <summary>
''' Аудиоисточник на основе аудиокарты
''' </summary>
Public Class WaveInSource
    Inherits WaveSource
    Implements IWaveSource

    Private _deviceNumber As Integer
    Private _deviceName As String
    Private WithEvents _waveIn As WaveInEvent

    Public Overrides ReadOnly Property Name As String Implements IWaveSource.Name
        Get
            Return _deviceName
        End Get
    End Property

    Public Event Stopped() Implements IWaveSource.Stopped

    Public Sub New(deviceNumber As Integer, deviceName As String, sampleRate As Integer, bitDepth As Integer, stereo As Boolean, minSampleCountInBlock As Integer)
        MyBase.New(sampleRate, bitDepth, stereo, minSampleCountInBlock)
        _deviceNumber = deviceNumber
        _deviceName = deviceName
        _waveIn = New WaveInEvent With {
                                           .DeviceNumber = deviceNumber,
                                           .WaveFormat = _waveFormat,
                                           .BufferMilliseconds = Math.Ceiling((minSampleCountInBlock / CDbl(sampleRate)) * 1000),
                                           .NumberOfBuffers = 3
                                       }
    End Sub

    Public Shadows Sub SetSampleProcessor(sampleProcessor As SampleProcessorDelegate) Implements IWaveSource.SetSampleProcessor
        MyBase.SetSampleProcessor(sampleProcessor)
    End Sub

    Public Overrides Sub Start() Implements IWaveSource.Start
        If _deviceNumber >= 0 AndAlso Not _started Then
            SyncLock _waveIn
                _waveIn.StartRecording()
                _started = True
            End SyncLock
        End If
    End Sub

    Public Overrides Sub [Stop]() Implements IWaveSource.Stop
        If _started Then
            SyncLock _waveIn
                _waveIn.StopRecording()
                _started = False
                RaiseEvent Stopped()
            End SyncLock
        End If
    End Sub

    Private Sub WaveDataAvailable(sender As Object, e As WaveInEventArgs) Handles _waveIn.DataAvailable
        MyBase.WaveDataAvailableBase(e, Now)
    End Sub
End Class
