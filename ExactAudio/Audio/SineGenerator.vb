Imports NAudio.Wave

''' <summary>
''' Стерео-генератор синусоидального сигнала
''' </summary>
Public Class SineGenerator
    Private _sampleRate As Integer
    Private _waveOutVolume As Single = 1.0
    Private _waveOut As WaveOut

    Public ReadOnly SyncRoot As New Object

    Public Property Volume As Single
        Get
            Return If(_waveOut IsNot Nothing, _waveOut.Volume, _waveOutVolume)
        End Get
        Set(value As Single)
            If value < 0 Or value > 1 Then Throw New Exception("Volume < 0 Or Volume > 1")
            If _waveOut IsNot Nothing Then
                _waveOut.Volume = value
            Else
                _waveOutVolume = value
            End If
        End Set
    End Property

    Public Sub New(ByRef deviceNumber As Integer, sampleRate As Integer) 'DeviceNumber -> Out variable
        If deviceNumber < 0 Then deviceNumber = 0
        _sampleRate = sampleRate
        Dim waveFormat = New WaveFormat(_sampleRate, 16, 1)
        Dim waveProvider = New BufferedWaveProvider(waveFormat)
        Try
            _waveOut = New WaveOut() With {.DeviceNumber = deviceNumber}
            _waveOut.Init(waveProvider)
        Catch
            For i = 0 To GetAudioDeviceNamesWaveOut().Length - 1
                Dim exc = False
                If i <> deviceNumber Then
                    Try
                        _waveOut = New WaveOut() With {.DeviceNumber = i}
                        _waveOut.Init(waveProvider)
                        deviceNumber = i
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

    Public Sub SwitchOn(sineFreq As Integer)
        SyncLock SyncRoot
            If _waveOut Is Nothing Then
                Dim sineWaveProvider = New SineWaveProvider32()
                With sineWaveProvider
                    .SetWaveFormat(_sampleRate, 1)
                    .Frequency = sineFreq
                    .Amplitude = Me.Volume
                End With
                _waveOut = New WaveOut()
                With _waveOut
                    .Init(sineWaveProvider)
                    .Play()
                End With
            End If
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
End Class
