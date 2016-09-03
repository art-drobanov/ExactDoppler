<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me._switchOnButton = New System.Windows.Forms.Button()
        Me._switchOffButton = New System.Windows.Forms.Button()
        Me._sineFreqTrackBar = New System.Windows.Forms.TrackBar()
        Me._frequencyGroupBox = New System.Windows.Forms.GroupBox()
        Me._sineFreqLabel = New System.Windows.Forms.Label()
        Me._volumeTrackBar = New System.Windows.Forms.TrackBar()
        Me._outputGroupBox = New System.Windows.Forms.GroupBox()
        Me._outTestButton = New System.Windows.Forms.Button()
        Me._outputAudioDevicesRefreshButton = New System.Windows.Forms.Button()
        Me._outputAudioDevicesListBox = New System.Windows.Forms.ListBox()
        Me._inputGroupBox = New System.Windows.Forms.GroupBox()
        Me._centralBlindZoneGroupBox = New System.Windows.Forms.GroupBox()
        Me._blindZoneLabel = New System.Windows.Forms.Label()
        Me._blindZoneTrackBar = New System.Windows.Forms.TrackBar()
        Me._inputAudioDevicesRefreshButton = New System.Windows.Forms.Button()
        Me._inputAudioDevicesListBox = New System.Windows.Forms.ListBox()
        Me._blocksLabel_ = New System.Windows.Forms.Label()
        Me._captureOffButton = New System.Windows.Forms.Button()
        Me._blocksLabel = New System.Windows.Forms.Label()
        Me._captureOnButton = New System.Windows.Forms.Button()
        Me._waterfallDisplayBitmapControl = New Bwl.Imaging.DisplayBitmapControl()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me._dopplerLogItemLabel_ = New System.Windows.Forms.Label()
        Me._dopplerLogItemLabel = New System.Windows.Forms.Label()
        CType(Me._sineFreqTrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me._frequencyGroupBox.SuspendLayout()
        CType(Me._volumeTrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me._outputGroupBox.SuspendLayout()
        Me._inputGroupBox.SuspendLayout()
        Me._centralBlindZoneGroupBox.SuspendLayout()
        CType(Me._blindZoneTrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        '_switchOnButton
        '
        Me._switchOnButton.BackColor = System.Drawing.Color.MediumSpringGreen
        Me._switchOnButton.Location = New System.Drawing.Point(6, 327)
        Me._switchOnButton.Name = "_switchOnButton"
        Me._switchOnButton.Size = New System.Drawing.Size(161, 32)
        Me._switchOnButton.TabIndex = 7
        Me._switchOnButton.Text = "Sine Gen On"
        Me._switchOnButton.UseVisualStyleBackColor = False
        '
        '_switchOffButton
        '
        Me._switchOffButton.BackColor = System.Drawing.Color.Silver
        Me._switchOffButton.Location = New System.Drawing.Point(173, 327)
        Me._switchOffButton.Name = "_switchOffButton"
        Me._switchOffButton.Size = New System.Drawing.Size(47, 32)
        Me._switchOffButton.TabIndex = 8
        Me._switchOffButton.Text = "Off"
        Me._switchOffButton.UseVisualStyleBackColor = False
        '
        '_sineFreqTrackBar
        '
        Me._sineFreqTrackBar.Location = New System.Drawing.Point(6, 34)
        Me._sineFreqTrackBar.Maximum = 48
        Me._sineFreqTrackBar.Name = "_sineFreqTrackBar"
        Me._sineFreqTrackBar.Size = New System.Drawing.Size(202, 45)
        Me._sineFreqTrackBar.TabIndex = 5
        Me._sineFreqTrackBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft
        Me._sineFreqTrackBar.Value = 42
        '
        '_frequencyGroupBox
        '
        Me._frequencyGroupBox.Controls.Add(Me._sineFreqLabel)
        Me._frequencyGroupBox.Controls.Add(Me._sineFreqTrackBar)
        Me._frequencyGroupBox.Location = New System.Drawing.Point(6, 201)
        Me._frequencyGroupBox.Name = "_frequencyGroupBox"
        Me._frequencyGroupBox.Size = New System.Drawing.Size(214, 82)
        Me._frequencyGroupBox.TabIndex = 3
        Me._frequencyGroupBox.TabStop = False
        Me._frequencyGroupBox.Text = "Frequency"
        '
        '_sineFreqLabel
        '
        Me._sineFreqLabel.AutoSize = True
        Me._sineFreqLabel.Location = New System.Drawing.Point(6, 20)
        Me._sineFreqLabel.Name = "_sineFreqLabel"
        Me._sineFreqLabel.Size = New System.Drawing.Size(13, 13)
        Me._sineFreqLabel.TabIndex = 4
        Me._sineFreqLabel.Text = "0"
        '
        '_volumeTrackBar
        '
        Me._volumeTrackBar.Location = New System.Drawing.Point(222, 201)
        Me._volumeTrackBar.Maximum = 100
        Me._volumeTrackBar.Name = "_volumeTrackBar"
        Me._volumeTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical
        Me._volumeTrackBar.Size = New System.Drawing.Size(45, 158)
        Me._volumeTrackBar.TabIndex = 9
        Me._volumeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None
        Me._volumeTrackBar.Value = 90
        '
        '_outputGroupBox
        '
        Me._outputGroupBox.Controls.Add(Me._outTestButton)
        Me._outputGroupBox.Controls.Add(Me._outputAudioDevicesRefreshButton)
        Me._outputGroupBox.Controls.Add(Me._switchOnButton)
        Me._outputGroupBox.Controls.Add(Me._outputAudioDevicesListBox)
        Me._outputGroupBox.Controls.Add(Me._switchOffButton)
        Me._outputGroupBox.Controls.Add(Me._frequencyGroupBox)
        Me._outputGroupBox.Controls.Add(Me._volumeTrackBar)
        Me._outputGroupBox.Location = New System.Drawing.Point(12, 12)
        Me._outputGroupBox.Name = "_outputGroupBox"
        Me._outputGroupBox.Size = New System.Drawing.Size(269, 365)
        Me._outputGroupBox.TabIndex = 0
        Me._outputGroupBox.TabStop = False
        Me._outputGroupBox.Text = "Output [ OFF ]"
        '
        '_outTestButton
        '
        Me._outTestButton.BackColor = System.Drawing.SystemColors.ButtonFace
        Me._outTestButton.Location = New System.Drawing.Point(6, 289)
        Me._outTestButton.Name = "_outTestButton"
        Me._outTestButton.Size = New System.Drawing.Size(161, 32)
        Me._outTestButton.TabIndex = 6
        Me._outTestButton.Text = "dtmf.Play(""151 262 888 111"")"
        Me._outTestButton.UseVisualStyleBackColor = False
        '
        '_outputAudioDevicesRefreshButton
        '
        Me._outputAudioDevicesRefreshButton.Location = New System.Drawing.Point(6, 172)
        Me._outputAudioDevicesRefreshButton.Name = "_outputAudioDevicesRefreshButton"
        Me._outputAudioDevicesRefreshButton.Size = New System.Drawing.Size(256, 23)
        Me._outputAudioDevicesRefreshButton.TabIndex = 2
        Me._outputAudioDevicesRefreshButton.UseVisualStyleBackColor = True
        '
        '_outputAudioDevicesListBox
        '
        Me._outputAudioDevicesListBox.FormattingEnabled = True
        Me._outputAudioDevicesListBox.Location = New System.Drawing.Point(6, 19)
        Me._outputAudioDevicesListBox.Name = "_outputAudioDevicesListBox"
        Me._outputAudioDevicesListBox.Size = New System.Drawing.Size(256, 147)
        Me._outputAudioDevicesListBox.TabIndex = 1
        '
        '_inputGroupBox
        '
        Me._inputGroupBox.Controls.Add(Me._centralBlindZoneGroupBox)
        Me._inputGroupBox.Controls.Add(Me._inputAudioDevicesRefreshButton)
        Me._inputGroupBox.Controls.Add(Me._inputAudioDevicesListBox)
        Me._inputGroupBox.Controls.Add(Me._blocksLabel_)
        Me._inputGroupBox.Controls.Add(Me._captureOffButton)
        Me._inputGroupBox.Controls.Add(Me._blocksLabel)
        Me._inputGroupBox.Controls.Add(Me._captureOnButton)
        Me._inputGroupBox.Location = New System.Drawing.Point(287, 12)
        Me._inputGroupBox.Name = "_inputGroupBox"
        Me._inputGroupBox.Size = New System.Drawing.Size(267, 365)
        Me._inputGroupBox.TabIndex = 10
        Me._inputGroupBox.TabStop = False
        Me._inputGroupBox.Text = "Input [ OFF ]"
        '
        '_centralBlindZoneGroupBox
        '
        Me._centralBlindZoneGroupBox.Controls.Add(Me._blindZoneLabel)
        Me._centralBlindZoneGroupBox.Controls.Add(Me._blindZoneTrackBar)
        Me._centralBlindZoneGroupBox.Location = New System.Drawing.Point(9, 201)
        Me._centralBlindZoneGroupBox.Name = "_centralBlindZoneGroupBox"
        Me._centralBlindZoneGroupBox.Size = New System.Drawing.Size(252, 82)
        Me._centralBlindZoneGroupBox.TabIndex = 13
        Me._centralBlindZoneGroupBox.TabStop = False
        Me._centralBlindZoneGroupBox.Text = "Central ""Blind Zone"""
        '
        '_blindZoneLabel
        '
        Me._blindZoneLabel.AutoSize = True
        Me._blindZoneLabel.Location = New System.Drawing.Point(6, 20)
        Me._blindZoneLabel.Name = "_blindZoneLabel"
        Me._blindZoneLabel.Size = New System.Drawing.Size(13, 13)
        Me._blindZoneLabel.TabIndex = 14
        Me._blindZoneLabel.Text = "0"
        '
        '_blindZoneTrackBar
        '
        Me._blindZoneTrackBar.Location = New System.Drawing.Point(6, 34)
        Me._blindZoneTrackBar.Maximum = 100
        Me._blindZoneTrackBar.Minimum = 1
        Me._blindZoneTrackBar.Name = "_blindZoneTrackBar"
        Me._blindZoneTrackBar.Size = New System.Drawing.Size(240, 45)
        Me._blindZoneTrackBar.TabIndex = 15
        Me._blindZoneTrackBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft
        Me._blindZoneTrackBar.Value = 70
        '
        '_inputAudioDevicesRefreshButton
        '
        Me._inputAudioDevicesRefreshButton.Location = New System.Drawing.Point(6, 172)
        Me._inputAudioDevicesRefreshButton.Name = "_inputAudioDevicesRefreshButton"
        Me._inputAudioDevicesRefreshButton.Size = New System.Drawing.Size(256, 23)
        Me._inputAudioDevicesRefreshButton.TabIndex = 12
        Me._inputAudioDevicesRefreshButton.UseVisualStyleBackColor = True
        '
        '_inputAudioDevicesListBox
        '
        Me._inputAudioDevicesListBox.FormattingEnabled = True
        Me._inputAudioDevicesListBox.Location = New System.Drawing.Point(6, 19)
        Me._inputAudioDevicesListBox.Name = "_inputAudioDevicesListBox"
        Me._inputAudioDevicesListBox.Size = New System.Drawing.Size(256, 147)
        Me._inputAudioDevicesListBox.TabIndex = 11
        '
        '_blocksLabel_
        '
        Me._blocksLabel_.AutoSize = True
        Me._blocksLabel_.Location = New System.Drawing.Point(6, 297)
        Me._blocksLabel_.Name = "_blocksLabel_"
        Me._blocksLabel_.Size = New System.Drawing.Size(42, 13)
        Me._blocksLabel_.TabIndex = 16
        Me._blocksLabel_.Text = "Blocks:"
        '
        '_captureOffButton
        '
        Me._captureOffButton.BackColor = System.Drawing.Color.Silver
        Me._captureOffButton.Location = New System.Drawing.Point(214, 327)
        Me._captureOffButton.Name = "_captureOffButton"
        Me._captureOffButton.Size = New System.Drawing.Size(47, 32)
        Me._captureOffButton.TabIndex = 19
        Me._captureOffButton.Text = "Off"
        Me._captureOffButton.UseVisualStyleBackColor = False
        '
        '_blocksLabel
        '
        Me._blocksLabel.AutoSize = True
        Me._blocksLabel.Location = New System.Drawing.Point(48, 298)
        Me._blocksLabel.Name = "_blocksLabel"
        Me._blocksLabel.Size = New System.Drawing.Size(13, 13)
        Me._blocksLabel.TabIndex = 17
        Me._blocksLabel.Text = "0"
        '
        '_captureOnButton
        '
        Me._captureOnButton.BackColor = System.Drawing.Color.MediumSpringGreen
        Me._captureOnButton.Location = New System.Drawing.Point(9, 327)
        Me._captureOnButton.Name = "_captureOnButton"
        Me._captureOnButton.Size = New System.Drawing.Size(199, 32)
        Me._captureOnButton.TabIndex = 18
        Me._captureOnButton.Text = "Capture On"
        Me._captureOnButton.UseVisualStyleBackColor = False
        '
        '_waterfallDisplayBitmapControl
        '
        Me._waterfallDisplayBitmapControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me._waterfallDisplayBitmapControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me._waterfallDisplayBitmapControl.Location = New System.Drawing.Point(12, 383)
        Me._waterfallDisplayBitmapControl.Name = "_waterfallDisplayBitmapControl"
        Me._waterfallDisplayBitmapControl.Size = New System.Drawing.Size(542, 260)
        Me._waterfallDisplayBitmapControl.TabIndex = 20
        '
        'LinkLabel1
        '
        Me.LinkLabel1.AutoSize = True
        Me.LinkLabel1.Location = New System.Drawing.Point(12, 654)
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.Size = New System.Drawing.Size(106, 13)
        Me.LinkLabel1.TabIndex = 21
        Me.LinkLabel1.TabStop = True
        Me.LinkLabel1.Text = "http://iconleak.com/"
        '
        '_dopplerLogItemLabel_
        '
        Me._dopplerLogItemLabel_.AutoSize = True
        Me._dopplerLogItemLabel_.Font = New System.Drawing.Font("Arial Narrow", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me._dopplerLogItemLabel_.Location = New System.Drawing.Point(291, 645)
        Me._dopplerLogItemLabel_.Name = "_dopplerLogItemLabel_"
        Me._dopplerLogItemLabel_.Size = New System.Drawing.Size(114, 25)
        Me._dopplerLogItemLabel_.TabIndex = 22
        Me._dopplerLogItemLabel_.Text = "DopplerLog: "
        '
        '_dopplerLogItemLabel
        '
        Me._dopplerLogItemLabel.AutoSize = True
        Me._dopplerLogItemLabel.Font = New System.Drawing.Font("Arial Narrow", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me._dopplerLogItemLabel.Location = New System.Drawing.Point(411, 646)
        Me._dopplerLogItemLabel.Name = "_dopplerLogItemLabel"
        Me._dopplerLogItemLabel.Size = New System.Drawing.Size(98, 25)
        Me._dopplerLogItemLabel.TabIndex = 23
        Me._dopplerLogItemLabel.Text = " No Motion"
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(566, 676)
        Me.Controls.Add(Me._dopplerLogItemLabel)
        Me.Controls.Add(Me._dopplerLogItemLabel_)
        Me.Controls.Add(Me.LinkLabel1)
        Me.Controls.Add(Me._waterfallDisplayBitmapControl)
        Me.Controls.Add(Me._inputGroupBox)
        Me.Controls.Add(Me._outputGroupBox)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "MainForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "ExactDopplerTest (03.09.2016 00:34)"
        CType(Me._sineFreqTrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me._frequencyGroupBox.ResumeLayout(False)
        Me._frequencyGroupBox.PerformLayout()
        CType(Me._volumeTrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me._outputGroupBox.ResumeLayout(False)
        Me._outputGroupBox.PerformLayout()
        Me._inputGroupBox.ResumeLayout(False)
        Me._inputGroupBox.PerformLayout()
        Me._centralBlindZoneGroupBox.ResumeLayout(False)
        Me._centralBlindZoneGroupBox.PerformLayout()
        CType(Me._blindZoneTrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents _switchOnButton As Button
    Friend WithEvents _switchOffButton As Button
    Friend WithEvents _sineFreqTrackBar As TrackBar
    Friend WithEvents _frequencyGroupBox As GroupBox
    Friend WithEvents _sineFreqLabel As Label
    Friend WithEvents _volumeTrackBar As TrackBar
    Friend WithEvents _outputGroupBox As GroupBox
    Friend WithEvents _outputAudioDevicesRefreshButton As Button
    Friend WithEvents _outputAudioDevicesListBox As ListBox
    Friend WithEvents _inputGroupBox As GroupBox
    Friend WithEvents _inputAudioDevicesListBox As ListBox
    Friend WithEvents _inputAudioDevicesRefreshButton As Button
    Friend WithEvents _captureOffButton As Button
    Friend WithEvents _blocksLabel As Label
    Friend WithEvents _blocksLabel_ As Label
    Friend WithEvents _captureOnButton As Button
    Friend WithEvents _waterfallDisplayBitmapControl As Bwl.Imaging.DisplayBitmapControl
    Friend WithEvents _centralBlindZoneGroupBox As GroupBox
    Friend WithEvents _blindZoneLabel As Label
    Friend WithEvents _blindZoneTrackBar As TrackBar
    Friend WithEvents _outTestButton As Button
    Friend WithEvents LinkLabel1 As LinkLabel
    Friend WithEvents _dopplerLogItemLabel_ As Label
    Friend WithEvents _dopplerLogItemLabel As Label
End Class
