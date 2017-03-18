''' <summary>
''' Генератор DTMF
''' </summary>
Public Class DTMFGenerator
    Private Structure FreqPair
        Public ReadOnly LowFreq As Single
        Public ReadOnly HighFreq As Single
        Public Sub New(lowFreq As Single, highFreq As Single)
            Me.LowFreq = lowFreq
            Me.HighFreq = highFreq
        End Sub
    End Structure

    Private _baseSymbolTime As Double = 160.0 '160.0
    Private _baseSpaceTime As Double = 40.0 '40.0
    Private _speed As Double = 1.0 '1.0
    Private _generator As SineGenerator
    Private _encodeMatrix As New Dictionary(Of Char, FreqPair)

    Private _syncRoot As New Object()

    Public ReadOnly Property SymbolTime As Double
        Get
            SyncLock _syncRoot
                Return _baseSymbolTime / Speed
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property SpaceTime As Double
        Get
            SyncLock _syncRoot
                Return _baseSpaceTime / Speed
            End SyncLock
        End Get
    End Property

    Public Property Speed As Double
        Get
            SyncLock _syncRoot
                Return _speed
            End SyncLock
        End Get
        Set(value As Double)
            SyncLock _syncRoot
                _speed = value
            End SyncLock
        End Set
    End Property

    Public Property Volume As Single
        Get
            SyncLock _syncRoot
                Return _generator.Volume
            End SyncLock
        End Get
        Set(value As Single)
            SyncLock _syncRoot
                _generator.Volume = value
            End SyncLock
        End Set
    End Property

    Public Sub New(ByRef deviceNumber As Integer, sampleRate As Integer)
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
        SyncLock _syncRoot
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
        End SyncLock
    End Sub
End Class
