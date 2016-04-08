Imports System.IO
Imports System.Runtime.CompilerServices
Imports NAudio.Wave

''' <summary>
''' Аудиоутилиты
''' </summary>
Public Module AudioUtils
    Public Function GetAudioDeviceNamesWaveIn() As String()
        Dim deviceNames As New List(Of String)
        For i = 0 To WaveIn.DeviceCount - 1
            Dim deviceName = WaveIn.GetCapabilities(i).ProductName
            deviceNames.Add(deviceName)
        Next
        Return deviceNames.ToArray()
    End Function

    Public Function GetAudioDeviceNamesWaveOut() As String()
        Dim deviceNames As New List(Of String)
        For i = 0 To WaveOut.DeviceCount - 1
            Dim deviceName = WaveOut.GetCapabilities(i).ProductName
            deviceNames.Add(deviceName)
        Next
        Return deviceNames.ToArray()
    End Function

    <Extension>
    Public Function ToByteArray24(ints32 As Integer(), monoToStereo As Boolean) As Byte()
        Using ms = New MemoryStream()
            For Each elem In ints32
                Dim int24b = BitConverter.GetBytes(elem)
                If monoToStereo Then
                    ms.Write(int24b, 0, 3)
                    ms.Write(int24b, 0, 3)
                Else
                    ms.Write(int24b, 0, 3)
                End If
            Next
            ms.Flush()
            ms.Seek(0, SeekOrigin.Begin)
            Return ms.ToArray()
        End Using
    End Function

    <Extension>
    Public Function ToNBits(samples As Double(), NBits As Integer) As Integer()
        Dim bitAmp As Integer = CInt(Math.Floor(Math.Pow(2, (NBits - 1)))) - 1
        Dim result = New Integer(samples.Length - 1) {}
        For i = 0 To samples.Length - 1
            result(i) = CInt(Math.Floor(samples(i) * bitAmp))
        Next i
        Return result
    End Function

    <Extension>
    Public Sub MixWith(int32A As Integer(), int32B As Integer())
        If int32A.Length <> int32B.Length Then Throw New Exception("int32A.Length <> int32B.Length")
        For i = 0 To int32A.Length - 1
            Dim result = CInt(Math.Floor((int32A(i) * 0.5) + (int32B(i) * 0.5)))
            int32A(i) = result
            int32B(i) = result
        Next
    End Sub
End Module
