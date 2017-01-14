Imports Bwl.Imaging

''' <summary>
''' Результат анализа блока PCM
''' </summary>
Public Class MotionExplorerResult
    Public Property Timestamp As DateTime
    Public Property CarrierLevel As New LinkedList(Of Single)
    Public Property LowDoppler As New LinkedList(Of Single)
    Public Property HighDoppler As New LinkedList(Of Single)
    Public Property DopplerLogItem As DopplerLogItem
    Public Property Duration As Double
    Public Property RawDopplerImage As RGBMatrix
    Public Property DopplerImage As RGBMatrix
    Public Property LowpassAudio As Single()
    Public Property IsWarning As Boolean
End Class
