Imports Bwl.Framework

Public Class ExactDopplerConfigFromStorage
    Inherits ExactDopplerConfig

    Private _centerFreq As DoubleSetting
    Private _blindZone As IntegerSetting
    Private _displayLeft As BooleanSetting
    Private _displayRightWithLeft As BooleanSetting
    Private _displayCenter As BooleanSetting
    Private _displayRight As BooleanSetting
    Private _pcmOutput As BooleanSetting
    Private _imageOutput As BooleanSetting

    Public Sub New(storage As SettingsStorage)
        _centerFreq = New DoubleSetting(storage, "CenterFreq", 21000)
        _blindZone = New IntegerSetting(storage, "BlindZone", 70)
        _displayLeft = New BooleanSetting(storage, "DisplayLeft", True)
        _displayRightWithLeft = New BooleanSetting(storage, "DisplayRightWithLeft", False)
        _displayCenter = New BooleanSetting(storage, "DisplayCenter", True)
        _displayRight = New BooleanSetting(storage, "DisplayRight", True)
        _pcmOutput = New BooleanSetting(storage, "PcmOutput", True)
        _imageOutput = New BooleanSetting(storage, "ImageOutput", True)
    End Sub

    Public Sub Load()
        With Me
            .CenterFreq = _centerFreq.Value
            .BlindZone = _blindZone.Value
            .DisplayLeft = _displayLeft.Value
            .DisplayRightWithLeft = _displayRightWithLeft.Value
            .DisplayCenter = _displayCenter.Value
            .DisplayRight = _displayRight.Value
            .PcmOutput = _pcmOutput.Value
            .ImageOutput = _imageOutput.Value
        End With
    End Sub
End Class
