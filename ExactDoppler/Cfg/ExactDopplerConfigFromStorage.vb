Imports Bwl.Framework

Public Class ExactDopplerConfigFromStorage
    Inherits ExactDopplerConfig

    Private _inputDeviceIdx As IntegerSetting
    Private _outputDeviceIdx As IntegerSetting
    Private _volume As DoubleSetting
    Private _centerFreq As DoubleSetting
    Private _blindZone As IntegerSetting
    Private _pcmOutput As BooleanSetting
    Private _imageOutput As BooleanSetting

    Public Sub New(storage As SettingsStorage)
        _inputDeviceIdx = New IntegerSetting(storage, "InputDeviceIdx", 0)
        _outputDeviceIdx = New IntegerSetting(storage, "OutputDeviceIdx", 0)
        _volume = New DoubleSetting(storage, "Volume", 0.5)
        _centerFreq = New DoubleSetting(storage, "CenterFreq", 21000)
        _blindZone = New IntegerSetting(storage, "BlindZone", 70)
    End Sub

    Public Sub Load()
        With Me
            .InputDeviceIdx = _inputDeviceIdx.Value
            .OutputDeviceIdx = _outputDeviceIdx.Value
            .Volume = _volume.Value
            .CenterFreq = _centerFreq.Value
            .BlindZone = _blindZone.Value
        End With
    End Sub
End Class
