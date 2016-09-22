Imports NAudio.Wave

''' <summary>
''' Стерео-генератор синусоидального сигнала
''' </summary>
Public Class SineGenerator
    Private _deviceNumber As Integer
    Private _sampleRate As Integer
    Private _waveOutVolume As Single = 1.0
    Private _waveOut As WaveOut

    Public ReadOnly SyncRoot As New Object

    Public ReadOnly Property SampleRate As Integer
        Get
            Return _sampleRate
        End Get
    End Property

    Public Property Volume As Single
        Get
            SyncLock SyncRoot
                Return _waveOutVolume
            End SyncLock
        End Get
        Set(value As Single)
            SyncLock SyncRoot
                If value < 0 Or value > 1 Then Throw New Exception("SineGenerator: Volume < 0 Or Volume > 1")
                _waveOutVolume = value
                If _waveOut IsNot Nothing Then
                    _waveOut.Volume = _waveOutVolume
                End If
            End SyncLock
        End Set
    End Property

    Public Sub New(ByRef selectedDeviceNumber As Integer, sampleRate As Integer) 'DeviceNumber -> Out variable
        selectedDeviceNumber = If(selectedDeviceNumber < 0, 0, selectedDeviceNumber)
        _sampleRate = sampleRate
        Dim waveFormat = New WaveFormat(_sampleRate, 16, 1)
        Dim waveProvider = New BufferedWaveProvider(waveFormat)
        Try
            _waveOut = New WaveOut() With {.DeviceNumber = selectedDeviceNumber}
            _waveOut.Init(waveProvider)
            _deviceNumber = selectedDeviceNumber
        Catch
            For i = 0 To GetWaveOutNames().Length - 1
                Dim exc = False
                If i <> selectedDeviceNumber Then
                    Try
                        _waveOut = New WaveOut() With {.DeviceNumber = i}
                        _waveOut.Init(waveProvider)
                        selectedDeviceNumber = i
                    Catch
                        _waveOut = Nothing
                        exc = True
                    End Try
                    If Not exc Then Exit For
                End If
            Next
        End Try
        _waveOut = Nothing
    End Sub

    Public Sub Play(program As Queue(Of SineTaskBlock))
        SyncLock SyncRoot
            PlayWith(New SineWaveProvider32(program))
        End SyncLock
    End Sub

    Public Sub SwitchOn(frequency As Single())
        SyncLock SyncRoot
            PlayWith(New SineWaveProvider32(frequency))
        End SyncLock
    End Sub

    Public Sub SwitchOff()
        SyncLock SyncRoot
            If _waveOut IsNot Nothing Then
                With _waveOut
                    .Stop()
                    .Dispose()
                End With
                _waveOut = Nothing
            End If
        End SyncLock
    End Sub

    Private Sub PlayWith(sineWaveProvider As SineWaveProvider32)
        SyncLock SyncRoot
            If _waveOut Is Nothing Then
                sineWaveProvider.SetWaveFormat(_sampleRate, 1)
                _waveOut = New WaveOut() With {.DeviceNumber = _deviceNumber}
                With _waveOut
                    .Init(sineWaveProvider)
                    .Volume = _waveOutVolume
                    .Play()
                End With
            Else
                SwitchOff()
                PlayWith(sineWaveProvider)
            End If
        End SyncLock
    End Sub
End Class
