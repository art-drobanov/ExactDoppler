''' <summary>
''' Генератор DTMF
''' </summary>
Public Class DTMFGenerator
    Private Structure FreqPair
        Public ReadOnly LowFreq As Integer
        Public ReadOnly HighFreq As Integer
        Public Sub New(lowFreq As Integer, highFreq As Integer)
            Me.LowFreq = lowFreq
            Me.HighFreq = highFreq
        End Sub
    End Structure

    Private _baseSymbolTime As Integer = 160 '160
    Private _baseSpaceTime As Integer = 40 '40
    Private _generator As SineGenerator
    Private _encodeMatrix As New Dictionary(Of Char, FreqPair)

    Public ReadOnly SyncRoot As New Object

    Public ReadOnly Property SymbolTime As Integer
        Get
            Return _baseSymbolTime * SlowRate
        End Get
    End Property

    Public ReadOnly Property SpaceTime As Integer
        Get
            Return _baseSpaceTime * SlowRate
        End Get
    End Property

    Public Property SlowRate As Integer = 1.0 '1.0

    Public Property Volume As Single
        Get
            SyncLock SyncRoot
                Return _generator.Volume
            End SyncLock
        End Get
        Set(value As Single)
            SyncLock SyncRoot
                _generator.Volume = value
            End SyncLock
        End Set
    End Property

    Public Sub New(deviceNumber As Integer, sampleRate As Integer)
        _generator = New SineGenerator(deviceNumber, sampleRate)

        _encodeMatrix.Add("1", New FreqPair(697, 1209)) '697, 1209
        _encodeMatrix.Add("2", New FreqPair(697, 1336)) '697, 1336
        _encodeMatrix.Add("3", New FreqPair(697, 1477)) '697, 1477
        _encodeMatrix.Add("A", New FreqPair(697, 1633)) '697, 1633

        _encodeMatrix.Add("4", New FreqPair(770, 1209)) '770, 1209
        _encodeMatrix.Add("5", New FreqPair(770, 1336)) '770, 1336
        _encodeMatrix.Add("6", New FreqPair(770, 1477)) '770, 1477
        _encodeMatrix.Add("B", New FreqPair(770, 1633)) '770, 1633

        _encodeMatrix.Add("7", New FreqPair(852, 1209)) '852, 1209
        _encodeMatrix.Add("8", New FreqPair(852, 1336)) '852, 1336
        _encodeMatrix.Add("9", New FreqPair(852, 1477)) '852, 1477
        _encodeMatrix.Add("C", New FreqPair(852, 1633)) '852, 1633

        _encodeMatrix.Add("*", New FreqPair(941, 1209)) '941, 1209
        _encodeMatrix.Add("0", New FreqPair(941, 1336)) '941, 1336
        _encodeMatrix.Add("#", New FreqPair(941, 1477)) '941, 1477
        _encodeMatrix.Add("D", New FreqPair(941, 1633)) '941, 1633
    End Sub

    Public Sub Play(sequence As String)
        Dim program As New Queue(Of SineTaskBlock)()
        For Each s In sequence.ToUpper()
            If _encodeMatrix.ContainsKey(s) Then
                Dim freqPair = _encodeMatrix(s)
                program.Enqueue(New SineTaskBlock({freqPair.LowFreq, freqPair.HighFreq}, {1.0, 1.0}, ((Me.SymbolTime / 1000.0) * _generator.SampleRate))) 'symbol
                program.Enqueue(New SineTaskBlock({0, 0}, {0, 0}, ((Me.SpaceTime / 1000.0) * _generator.SampleRate))) 'space
            Else
                program.Enqueue(New SineTaskBlock({0, 0}, {0, 0}, ((Me.SymbolTime / 1000.0) * _generator.SampleRate))) 'space symbol
            End If
        Next
        _generator.Play(program)
    End Sub
End Class
