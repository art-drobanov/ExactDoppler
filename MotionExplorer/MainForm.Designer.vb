<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me._waterfallsGroupBox = New System.Windows.Forms.GroupBox()
        Me._waterfallDisplayRawBitmapControl = New Bwl.Imaging.DisplayBitmapControl()
        Me._fastModeCheckBox = New System.Windows.Forms.CheckBox()
        Me._waterfallDisplayBitmapControl = New Bwl.Imaging.DisplayBitmapControl()
        Me._openWavButton = New System.Windows.Forms.Button()
        Me._dopplerSettingsGroupBox = New System.Windows.Forms.GroupBox()
        Me._topFrequencyGroupBox = New System.Windows.Forms.GroupBox()
        Me._topFreqOnlyCheckBox = New System.Windows.Forms.CheckBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me._freq2Label = New System.Windows.Forms.Label()
        Me._freq1Label = New System.Windows.Forms.Label()
        Me._sineFreqTrackBar = New System.Windows.Forms.TrackBar()
        Me._centralBlindZoneGroupBox = New System.Windows.Forms.GroupBox()
        Me._blindZoneLabel = New System.Windows.Forms.Label()
        Me._blindZoneTrackBar = New System.Windows.Forms.TrackBar()
        Me._wavFileGroupBox = New System.Windows.Forms.GroupBox()
        Me._captureOffButton = New System.Windows.Forms.Button()
        Me._speedXLabel = New System.Windows.Forms.Label()
        Me._captureOnButton = New System.Windows.Forms.Button()
        Me._wavPositionTrackBar = New System.Windows.Forms.TrackBar()
        Me._speedXLabel_ = New System.Windows.Forms.Label()
        Me._dopplerLogGroupBox = New System.Windows.Forms.GroupBox()
        Me._dopplerLogTextBox = New System.Windows.Forms.TextBox()
        Me._alarmLabel = New System.Windows.Forms.Label()
        Me._waterfallsGroupBox.SuspendLayout()
        Me._dopplerSettingsGroupBox.SuspendLayout()
        Me._topFrequencyGroupBox.SuspendLayout()
        CType(Me._sineFreqTrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me._centralBlindZoneGroupBox.SuspendLayout()
        CType(Me._blindZoneTrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me._wavFileGroupBox.SuspendLayout()
        CType(Me._wavPositionTrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me._dopplerLogGroupBox.SuspendLayout()
        Me.SuspendLayout()
        '
        '_waterfallsGroupBox
        '
        Me._waterfallsGroupBox.Controls.Add(Me._waterfallDisplayRawBitmapControl)
        Me._waterfallsGroupBox.Controls.Add(Me._fastModeCheckBox)
        Me._waterfallsGroupBox.Controls.Add(Me._waterfallDisplayBitmapControl)
        Me._waterfallsGroupBox.Location = New System.Drawing.Point(12, 12)
        Me._waterfallsGroupBox.Name = "_waterfallsGroupBox"
        Me._waterfallsGroupBox.Size = New System.Drawing.Size(1091, 274)
        Me._waterfallsGroupBox.TabIndex = 19
        Me._waterfallsGroupBox.TabStop = False
        Me._waterfallsGroupBox.Text = "Waterfalls"
        '
        '_waterfallDisplayRawBitmapControl
        '
        Me._waterfallDisplayRawBitmapControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me._waterfallDisplayRawBitmapControl.Bitmap = CType(resources.GetObject("_waterfallDisplayRawBitmapControl.Bitmap"), System.Drawing.Bitmap)
        Me._waterfallDisplayRawBitmapControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me._waterfallDisplayRawBitmapControl.Location = New System.Drawing.Point(6, 22)
        Me._waterfallDisplayRawBitmapControl.Name = "_waterfallDisplayRawBitmapControl"
        Me._waterfallDisplayRawBitmapControl.Size = New System.Drawing.Size(536, 243)
        Me._waterfallDisplayRawBitmapControl.TabIndex = 22
        '
        '_fastModeCheckBox
        '
        Me._fastModeCheckBox.AutoSize = True
        Me._fastModeCheckBox.Checked = True
        Me._fastModeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked
        Me._fastModeCheckBox.Location = New System.Drawing.Point(74, -1)
        Me._fastModeCheckBox.Name = "_fastModeCheckBox"
        Me._fastModeCheckBox.Size = New System.Drawing.Size(76, 17)
        Me._fastModeCheckBox.TabIndex = 20
        Me._fastModeCheckBox.Text = "Fast Mode"
        Me._fastModeCheckBox.UseVisualStyleBackColor = True
        '
        '_waterfallDisplayBitmapControl
        '
        Me._waterfallDisplayBitmapControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me._waterfallDisplayBitmapControl.Bitmap = CType(resources.GetObject("_waterfallDisplayBitmapControl.Bitmap"), System.Drawing.Bitmap)
        Me._waterfallDisplayBitmapControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me._waterfallDisplayBitmapControl.Location = New System.Drawing.Point(548, 22)
        Me._waterfallDisplayBitmapControl.Name = "_waterfallDisplayBitmapControl"
        Me._waterfallDisplayBitmapControl.Size = New System.Drawing.Size(536, 243)
        Me._waterfallDisplayBitmapControl.TabIndex = 21
        '
        '_openWavButton
        '
        Me._openWavButton.Location = New System.Drawing.Point(752, 66)
        Me._openWavButton.Name = "_openWavButton"
        Me._openWavButton.Size = New System.Drawing.Size(75, 32)
        Me._openWavButton.TabIndex = 23
        Me._openWavButton.Text = "Open WAV"
        Me._openWavButton.UseVisualStyleBackColor = True
        '
        '_dopplerSettingsGroupBox
        '
        Me._dopplerSettingsGroupBox.Controls.Add(Me._topFrequencyGroupBox)
        Me._dopplerSettingsGroupBox.Controls.Add(Me._centralBlindZoneGroupBox)
        Me._dopplerSettingsGroupBox.Location = New System.Drawing.Point(12, 402)
        Me._dopplerSettingsGroupBox.Name = "_dopplerSettingsGroupBox"
        Me._dopplerSettingsGroupBox.Size = New System.Drawing.Size(542, 107)
        Me._dopplerSettingsGroupBox.TabIndex = 27
        Me._dopplerSettingsGroupBox.TabStop = False
        Me._dopplerSettingsGroupBox.Text = "Doppler Settings"
        '
        '_topFrequencyGroupBox
        '
        Me._topFrequencyGroupBox.Controls.Add(Me._topFreqOnlyCheckBox)
        Me._topFrequencyGroupBox.Controls.Add(Me.Label1)
        Me._topFrequencyGroupBox.Controls.Add(Me._freq2Label)
        Me._topFrequencyGroupBox.Controls.Add(Me._freq1Label)
        Me._topFrequencyGroupBox.Controls.Add(Me._sineFreqTrackBar)
        Me._topFrequencyGroupBox.Location = New System.Drawing.Point(6, 19)
        Me._topFrequencyGroupBox.Name = "_topFrequencyGroupBox"
        Me._topFrequencyGroupBox.Size = New System.Drawing.Size(214, 82)
        Me._topFrequencyGroupBox.TabIndex = 27
        Me._topFrequencyGroupBox.TabStop = False
        Me._topFrequencyGroupBox.Text = "Frequencies"
        '
        '_topFreqOnlyCheckBox
        '
        Me._topFreqOnlyCheckBox.AutoSize = True
        Me._topFreqOnlyCheckBox.Checked = True
        Me._topFreqOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked
        Me._topFreqOnlyCheckBox.Location = New System.Drawing.Point(116, 0)
        Me._topFreqOnlyCheckBox.Name = "_topFreqOnlyCheckBox"
        Me._topFreqOnlyCheckBox.Size = New System.Drawing.Size(93, 17)
        Me._topFreqOnlyCheckBox.TabIndex = 3
        Me._topFreqOnlyCheckBox.Text = "Top Freq Only"
        Me._topFreqOnlyCheckBox.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(68, 20)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(13, 13)
        Me.Label1.TabIndex = 7
        Me.Label1.Text = "+"
        '
        '_freq2Label
        '
        Me._freq2Label.AutoSize = True
        Me._freq2Label.Location = New System.Drawing.Point(87, 20)
        Me._freq2Label.Name = "_freq2Label"
        Me._freq2Label.Size = New System.Drawing.Size(13, 13)
        Me._freq2Label.TabIndex = 6
        Me._freq2Label.Text = "0"
        '
        '_freq1Label
        '
        Me._freq1Label.AutoSize = True
        Me._freq1Label.Location = New System.Drawing.Point(6, 20)
        Me._freq1Label.Name = "_freq1Label"
        Me._freq1Label.Size = New System.Drawing.Size(13, 13)
        Me._freq1Label.TabIndex = 4
        Me._freq1Label.Text = "0"
        '
        '_sineFreqTrackBar
        '
        Me._sineFreqTrackBar.Location = New System.Drawing.Point(6, 34)
        Me._sineFreqTrackBar.Maximum = 48
        Me._sineFreqTrackBar.Name = "_sineFreqTrackBar"
        Me._sineFreqTrackBar.Size = New System.Drawing.Size(202, 45)
        Me._sineFreqTrackBar.TabIndex = 4
        Me._sineFreqTrackBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft
        Me._sineFreqTrackBar.Value = 42
        '
        '_centralBlindZoneGroupBox
        '
        Me._centralBlindZoneGroupBox.Controls.Add(Me._blindZoneLabel)
        Me._centralBlindZoneGroupBox.Controls.Add(Me._blindZoneTrackBar)
        Me._centralBlindZoneGroupBox.Location = New System.Drawing.Point(226, 19)
        Me._centralBlindZoneGroupBox.Name = "_centralBlindZoneGroupBox"
        Me._centralBlindZoneGroupBox.Size = New System.Drawing.Size(310, 82)
        Me._centralBlindZoneGroupBox.TabIndex = 28
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
        Me._blindZoneTrackBar.Size = New System.Drawing.Size(298, 45)
        Me._blindZoneTrackBar.TabIndex = 13
        Me._blindZoneTrackBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft
        Me._blindZoneTrackBar.Value = 60
        '
        '_wavFileGroupBox
        '
        Me._wavFileGroupBox.Controls.Add(Me._captureOffButton)
        Me._wavFileGroupBox.Controls.Add(Me._speedXLabel)
        Me._wavFileGroupBox.Controls.Add(Me._captureOnButton)
        Me._wavFileGroupBox.Controls.Add(Me._wavPositionTrackBar)
        Me._wavFileGroupBox.Controls.Add(Me._openWavButton)
        Me._wavFileGroupBox.Controls.Add(Me._speedXLabel_)
        Me._wavFileGroupBox.Location = New System.Drawing.Point(12, 292)
        Me._wavFileGroupBox.Name = "_wavFileGroupBox"
        Me._wavFileGroupBox.Size = New System.Drawing.Size(1091, 104)
        Me._wavFileGroupBox.TabIndex = 28
        Me._wavFileGroupBox.TabStop = False
        Me._wavFileGroupBox.Text = "Wav File"
        '
        '_captureOffButton
        '
        Me._captureOffButton.BackColor = System.Drawing.Color.Silver
        Me._captureOffButton.Location = New System.Drawing.Point(1038, 66)
        Me._captureOffButton.Name = "_captureOffButton"
        Me._captureOffButton.Size = New System.Drawing.Size(47, 32)
        Me._captureOffButton.TabIndex = 31
        Me._captureOffButton.Text = "Off"
        Me._captureOffButton.UseVisualStyleBackColor = False
        '
        '_speedXLabel
        '
        Me._speedXLabel.AutoSize = True
        Me._speedXLabel.Location = New System.Drawing.Point(57, 76)
        Me._speedXLabel.Name = "_speedXLabel"
        Me._speedXLabel.Size = New System.Drawing.Size(13, 13)
        Me._speedXLabel.TabIndex = 24
        Me._speedXLabel.Text = "1"
        '
        '_captureOnButton
        '
        Me._captureOnButton.BackColor = System.Drawing.Color.MediumSpringGreen
        Me._captureOnButton.Location = New System.Drawing.Point(833, 66)
        Me._captureOnButton.Name = "_captureOnButton"
        Me._captureOnButton.Size = New System.Drawing.Size(199, 32)
        Me._captureOnButton.TabIndex = 30
        Me._captureOnButton.Text = "Capture On"
        Me._captureOnButton.UseVisualStyleBackColor = False
        '
        '_wavPositionTrackBar
        '
        Me._wavPositionTrackBar.Location = New System.Drawing.Point(6, 19)
        Me._wavPositionTrackBar.Maximum = 1000
        Me._wavPositionTrackBar.Name = "_wavPositionTrackBar"
        Me._wavPositionTrackBar.Size = New System.Drawing.Size(1078, 45)
        Me._wavPositionTrackBar.TabIndex = 26
        Me._wavPositionTrackBar.TickStyle = System.Windows.Forms.TickStyle.None
        '
        '_speedXLabel_
        '
        Me._speedXLabel_.AutoSize = True
        Me._speedXLabel_.Location = New System.Drawing.Point(9, 76)
        Me._speedXLabel_.Name = "_speedXLabel_"
        Me._speedXLabel_.Size = New System.Drawing.Size(48, 13)
        Me._speedXLabel_.TabIndex = 23
        Me._speedXLabel_.Text = "SpeedX:"
        '
        '_dopplerLogGroupBox
        '
        Me._dopplerLogGroupBox.Controls.Add(Me._alarmLabel)
        Me._dopplerLogGroupBox.Controls.Add(Me._dopplerLogTextBox)
        Me._dopplerLogGroupBox.Location = New System.Drawing.Point(560, 402)
        Me._dopplerLogGroupBox.Name = "_dopplerLogGroupBox"
        Me._dopplerLogGroupBox.Size = New System.Drawing.Size(543, 107)
        Me._dopplerLogGroupBox.TabIndex = 29
        Me._dopplerLogGroupBox.TabStop = False
        Me._dopplerLogGroupBox.Text = "DopplerLog"
        '
        '_dopplerLogTextBox
        '
        Me._dopplerLogTextBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me._dopplerLogTextBox.Location = New System.Drawing.Point(6, 19)
        Me._dopplerLogTextBox.Multiline = True
        Me._dopplerLogTextBox.Name = "_dopplerLogTextBox"
        Me._dopplerLogTextBox.Size = New System.Drawing.Size(530, 57)
        Me._dopplerLogTextBox.TabIndex = 23
        '
        '_alarmLabel
        '
        Me._alarmLabel.AutoSize = True
        Me._alarmLabel.Location = New System.Drawing.Point(6, 88)
        Me._alarmLabel.Name = "_alarmLabel"
        Me._alarmLabel.Size = New System.Drawing.Size(56, 13)
        Me._alarmLabel.TabIndex = 32
        Me._alarmLabel.Text = "ALARM: 0"
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1115, 520)
        Me.Controls.Add(Me._dopplerLogGroupBox)
        Me.Controls.Add(Me._wavFileGroupBox)
        Me.Controls.Add(Me._dopplerSettingsGroupBox)
        Me.Controls.Add(Me._waterfallsGroupBox)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "MainForm"
        Me.Text = "MotionExplorer"
        Me._waterfallsGroupBox.ResumeLayout(False)
        Me._waterfallsGroupBox.PerformLayout()
        Me._dopplerSettingsGroupBox.ResumeLayout(False)
        Me._topFrequencyGroupBox.ResumeLayout(False)
        Me._topFrequencyGroupBox.PerformLayout()
        CType(Me._sineFreqTrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me._centralBlindZoneGroupBox.ResumeLayout(False)
        Me._centralBlindZoneGroupBox.PerformLayout()
        CType(Me._blindZoneTrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me._wavFileGroupBox.ResumeLayout(False)
        Me._wavFileGroupBox.PerformLayout()
        CType(Me._wavPositionTrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me._dopplerLogGroupBox.ResumeLayout(False)
        Me._dopplerLogGroupBox.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents _waterfallsGroupBox As GroupBox
    Friend WithEvents _waterfallDisplayRawBitmapControl As Bwl.Imaging.DisplayBitmapControl
    Friend WithEvents _fastModeCheckBox As CheckBox
    Friend WithEvents _waterfallDisplayBitmapControl As Bwl.Imaging.DisplayBitmapControl
    Friend WithEvents _openWavButton As Button
    Friend WithEvents _dopplerSettingsGroupBox As GroupBox
    Friend WithEvents _topFrequencyGroupBox As GroupBox
    Friend WithEvents _topFreqOnlyCheckBox As CheckBox
    Friend WithEvents Label1 As Label
    Friend WithEvents _freq2Label As Label
    Friend WithEvents _freq1Label As Label
    Friend WithEvents _sineFreqTrackBar As TrackBar
    Friend WithEvents _centralBlindZoneGroupBox As GroupBox
    Friend WithEvents _blindZoneLabel As Label
    Friend WithEvents _blindZoneTrackBar As TrackBar
    Friend WithEvents _wavFileGroupBox As GroupBox
    Friend WithEvents _wavPositionTrackBar As TrackBar
    Friend WithEvents _dopplerLogGroupBox As GroupBox
    Friend WithEvents _dopplerLogTextBox As TextBox
    Friend WithEvents _speedXLabel As Label
    Friend WithEvents _speedXLabel_ As Label
    Friend WithEvents _captureOffButton As Button
    Friend WithEvents _captureOnButton As Button
    Friend WithEvents _alarmLabel As Label
End Class
