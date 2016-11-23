Imports NAudio.Wave

''' <summary>
''' Аудиоутилиты
''' </summary>
Public Module AudioUtils
    ''' <summary>
    ''' Получение списка входных аудиоустройств
    ''' </summary>    
    Public Function GetWaveInNames() As String()
        Dim deviceNames As New List(Of String)
        For i = 0 To WaveIn.DeviceCount - 1
            Dim deviceName = WaveIn.GetCapabilities(i).ProductName
            deviceNames.Add(deviceName)
        Next
        Return deviceNames.ToArray()
    End Function

    ''' <summary>
    ''' Получение списка выходных аудиоустройств
    ''' </summary>
    Public Function GetWaveOutNames() As String()
        Dim deviceNames As New List(Of String)
        For i = 0 To WaveOut.DeviceCount - 1
            Dim deviceName = WaveOut.GetCapabilities(i).ProductName
            deviceNames.Add(deviceName)
        Next
        Return deviceNames.ToArray()
    End Function
End Module
