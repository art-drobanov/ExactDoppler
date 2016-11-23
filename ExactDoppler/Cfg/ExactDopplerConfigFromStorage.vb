Imports Bwl.Framework

''' <summary>
''' Конфигурация ExactDoppler "из конфига"
''' </summary>
Public Class ExactDopplerConfigFromStorage
    Inherits ExactDopplerConfig

    Private _inputDeviceIdx As IntegerSetting
    Private _outputDeviceIdx As IntegerSetting
    Private _volume As DoubleSetting
    Private _centerFreqs As StringSetting
    Private _blindZone As IntegerSetting
    Private _carrierWarningLevel As IntegerSetting

    Public Sub New(storage As SettingsStorage)
        _inputDeviceIdx = New IntegerSetting(storage, "InputDeviceIdx", 0)
        _outputDeviceIdx = New IntegerSetting(storage, "OutputDeviceIdx", 0)
        _volume = New DoubleSetting(storage, "Volume", 0.5)
        _centerFreqs = New StringSetting(storage, "CenterFreqs", "20300,21000")
        _blindZone = New IntegerSetting(storage, "BlindZone", 80)
        _carrierWarningLevel = New IntegerSetting(storage, "CarrierWarningLevel", 10)
    End Sub

    Public Sub Load()
        With Me
            .InputDeviceIdx = _inputDeviceIdx.Value
            .OutputDeviceIdx = _outputDeviceIdx.Value
            .Volume = _volume.Value
            .CenterFreqs = _centerFreqs.Value.Trim.Split({";"c, ","c}).Where(Function(item) Not String.IsNullOrEmpty(item)) _
                                                                      .Select(Function(item2)
                                                                                  Return Convert.ToDouble(item2)
                                                                              End Function).ToArray()
            .BlindZone = _blindZone.Value
            .CarrierWarningLevel = _carrierWarningLevel.Value
        End With
    End Sub
End Class
