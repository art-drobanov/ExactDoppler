Public Interface IWaveSource
    ReadOnly Property Name As String
    Event Stopped()
    Sub SetSampleProcessor(sampleProcessor As SampleProcessorDelegate)
    Sub Start()
    Sub [Stop]()
End Interface
