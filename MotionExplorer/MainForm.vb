Public Class MainForm
    Public Sub New()
        InitializeComponent()

        Me.Text += " " + My.Application.Info.Version.ToString()
    End Sub
End Class
