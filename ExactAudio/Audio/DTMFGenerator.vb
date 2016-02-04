Imports System.Threading

''' <summary>
''' Генератор DTMF
''' </summary>
Public Class DTMFGenerator
    Private _symbolTime As Integer
    Private _spaceTime As Integer
    Private _generator As SineGenerator

    Public Structure FreqPair
        Public Sub New(lowFreq As Integer, highFreq As Integer)
            Me.LowFreq = lowFreq
            Me.HighFreq = highFreq
        End Sub
        Public ReadOnly LowFreq As Integer
        Public ReadOnly HighFreq As Integer
    End Structure

    Private _encodeMatrix As New Dictionary(Of Char, FreqPair)

    Public Sub New(deviceNumber As Integer, sampleRate As Integer,
                   Optional symbolTime As Integer = 100, Optional spaceTime As Integer = 40)
        _generator = New SineGenerator(deviceNumber, sampleRate)

        _symbolTime = symbolTime
        _spaceTime = spaceTime

        _encodeMatrix.Add("1", New FreqPair(697, 1209))
        _encodeMatrix.Add("2", New FreqPair(697, 1336))
        _encodeMatrix.Add("3", New FreqPair(697, 1477))
        _encodeMatrix.Add("A", New FreqPair(697, 1633))

        _encodeMatrix.Add("4", New FreqPair(770, 1209))
        _encodeMatrix.Add("5", New FreqPair(770, 1336))
        _encodeMatrix.Add("6", New FreqPair(770, 1477))
        _encodeMatrix.Add("B", New FreqPair(770, 1633))

        _encodeMatrix.Add("7", New FreqPair(852, 1209))
        _encodeMatrix.Add("8", New FreqPair(852, 1336))
        _encodeMatrix.Add("9", New FreqPair(852, 1477))
        _encodeMatrix.Add("C", New FreqPair(852, 1633))

        _encodeMatrix.Add("*", New FreqPair(941, 1209))
        _encodeMatrix.Add("0", New FreqPair(941, 1336))
        _encodeMatrix.Add("#", New FreqPair(941, 1477))
        _encodeMatrix.Add("D", New FreqPair(941, 1633))
    End Sub

    Public Sub Play(sequence As String)
        _generator.Flash(0, 0, True, _symbolTime) : Thread.Sleep(_symbolTime)
        _generator.Flash(0, 0, True, _symbolTime) : Thread.Sleep(_symbolTime)
        For Each s In sequence.ToUpper()
            If _encodeMatrix.ContainsKey(s) Then
                Dim freqPair = _encodeMatrix(s)
                _generator.Flash(freqPair.LowFreq, freqPair.HighFreq, True, _symbolTime) : Thread.Sleep(_symbolTime)
                _generator.Flash(0, 0, True, _spaceTime) : Thread.Sleep(_spaceTime)
            Else
                Throw New Exception("Unknown DTMF Symbol")
            End If
        Next
        _generator.SwitchOff()
    End Sub
End Class
