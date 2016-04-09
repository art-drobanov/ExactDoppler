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
        Me._switchOnButton = New System.Windows.Forms.Button()
        Me._switchOffButton = New System.Windows.Forms.Button()
        Me._sineFreqLTrackBar = New System.Windows.Forms.TrackBar()
        Me._frequencyGroupBox = New System.Windows.Forms.GroupBox()
        Me._mixCheckBox = New System.Windows.Forms.CheckBox()
        Me._sineFreqRLabel = New System.Windows.Forms.Label()
        Me._sineFreqLLabel = New System.Windows.Forms.Label()
        Me._labelR = New System.Windows.Forms.Label()
        Me._sineFreqRTrackBar = New System.Windows.Forms.TrackBar()
        Me._labelL = New System.Windows.Forms.Label()
        Me._volumeTrackBar = New System.Windows.Forms.TrackBar()
        Me._outputGroupBox = New System.Windows.Forms.GroupBox()
        Me._outputAudioDevicesRefreshButton = New System.Windows.Forms.Button()
        Me._outputAudioDevicesListBox = New System.Windows.Forms.ListBox()
        Me._inputGroupBox = New System.Windows.Forms.GroupBox()
        Me._blindZoneLabel = New System.Windows.Forms.Label()
        Me._sineHarmWidthLabel_ = New System.Windows.Forms.Label()
        Me._blindZoneTrackBar = New System.Windows.Forms.TrackBar()
        Me._inputAudioDevicesRefreshButton = New System.Windows.Forms.Button()
        Me._inputAudioDevicesListBox = New System.Windows.Forms.ListBox()
        Me._blocksLabel_ = New System.Windows.Forms.Label()
        Me._captureOffButton = New System.Windows.Forms.Button()
        Me._blocksLabel = New System.Windows.Forms.Label()
        Me._captureOnButton = New System.Windows.Forms.Button()
        Me._waterfallDisplayBitmapControl = New Bwl.Imaging.DisplayBitmapControl()
        Me._displayLeftCheckBox = New System.Windows.Forms.CheckBox()
        Me._displayRightCheckBox = New System.Windows.Forms.CheckBox()
        Me._displayRightWithLeftCheckBox = New System.Windows.Forms.CheckBox()
        Me._displayCenterCheckBox = New System.Windows.Forms.CheckBox()
        CType(Me._sineFreqLTrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me._frequencyGroupBox.SuspendLayout()
        CType(Me._sineFreqRTrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me._volumeTrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me._outputGroupBox.SuspendLayout()
        Me._inputGroupBox.SuspendLayout()
        CType(Me._blindZoneTrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        '_switchOnButton
        '
        Me._switchOnButton.BackColor = System.Drawing.Color.MediumSpringGreen
        Me._switchOnButton.Location = New System.Drawing.Point(6, 426)
        Me._switchOnButton.Name = "_switchOnButton"
        Me._switchOnButton.Size = New System.Drawing.Size(161, 23)
        Me._switchOnButton.TabIndex = 7
        Me._switchOnButton.Text = "Switch On"
        Me._switchOnButton.UseVisualStyleBackColor = False
        '
        '_switchOffButton
        '
        Me._switchOffButton.BackColor = System.Drawing.Color.Silver
        Me._switchOffButton.Location = New System.Drawing.Point(173, 426)
        Me._switchOffButton.Name = "_switchOffButton"
        Me._switchOffButton.Size = New System.Drawing.Size(47, 23)
        Me._switchOffButton.TabIndex = 8
        Me._switchOffButton.Text = "Off"
        Me._switchOffButton.UseVisualStyleBackColor = False
        '
        '_sineFreqLTrackBar
        '
        Me._sineFreqLTrackBar.Location = New System.Drawing.Point(6, 19)
        Me._sineFreqLTrackBar.Maximum = 48
        Me._sineFreqLTrackBar.Name = "_sineFreqLTrackBar"
        Me._sineFreqLTrackBar.Size = New System.Drawing.Size(202, 45)
        Me._sineFreqLTrackBar.TabIndex = 5
        Me._sineFreqLTrackBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft
        Me._sineFreqLTrackBar.Value = 42
        '
        '_frequencyGroupBox
        '
        Me._frequencyGroupBox.Controls.Add(Me._mixCheckBox)
        Me._frequencyGroupBox.Controls.Add(Me._sineFreqRLabel)
        Me._frequencyGroupBox.Controls.Add(Me._sineFreqLLabel)
        Me._frequencyGroupBox.Controls.Add(Me._labelR)
        Me._frequencyGroupBox.Controls.Add(Me._sineFreqRTrackBar)
        Me._frequencyGroupBox.Controls.Add(Me._sineFreqLTrackBar)
        Me._frequencyGroupBox.Controls.Add(Me._labelL)
        Me._frequencyGroupBox.Location = New System.Drawing.Point(6, 279)
        Me._frequencyGroupBox.Name = "_frequencyGroupBox"
        Me._frequencyGroupBox.Size = New System.Drawing.Size(214, 141)
        Me._frequencyGroupBox.TabIndex = 3
        Me._frequencyGroupBox.TabStop = False
        Me._frequencyGroupBox.Text = "Frequency"
        '
        '_mixCheckBox
        '
        Me._mixCheckBox.AutoSize = True
        Me._mixCheckBox.Location = New System.Drawing.Point(167, 0)
        Me._mixCheckBox.Name = "_mixCheckBox"
        Me._mixCheckBox.Size = New System.Drawing.Size(42, 17)
        Me._mixCheckBox.TabIndex = 4
        Me._mixCheckBox.Text = "Mix"
        Me._mixCheckBox.UseVisualStyleBackColor = True
        '
        '_sineFreqRLabel
        '
        Me._sineFreqRLabel.AutoSize = True
        Me._sineFreqRLabel.Location = New System.Drawing.Point(36, 119)
        Me._sineFreqRLabel.Name = "_sineFreqRLabel"
        Me._sineFreqRLabel.Size = New System.Drawing.Size(13, 13)
        Me._sineFreqRLabel.TabIndex = 0
        Me._sineFreqRLabel.Text = "0"
        '
        '_sineFreqLLabel
        '
        Me._sineFreqLLabel.AutoSize = True
        Me._sineFreqLLabel.Location = New System.Drawing.Point(36, 95)
        Me._sineFreqLLabel.Name = "_sineFreqLLabel"
        Me._sineFreqLLabel.Size = New System.Drawing.Size(13, 13)
        Me._sineFreqLLabel.TabIndex = 0
        Me._sineFreqLLabel.Text = "0"
        '
        '_labelR
        '
        Me._labelR.AutoSize = True
        Me._labelR.Location = New System.Drawing.Point(14, 119)
        Me._labelR.Name = "_labelR"
        Me._labelR.Size = New System.Drawing.Size(15, 13)
        Me._labelR.TabIndex = 0
        Me._labelR.Text = "R"
        '
        '_sineFreqRTrackBar
        '
        Me._sineFreqRTrackBar.Location = New System.Drawing.Point(6, 52)
        Me._sineFreqRTrackBar.Maximum = 48
        Me._sineFreqRTrackBar.Name = "_sineFreqRTrackBar"
        Me._sineFreqRTrackBar.Size = New System.Drawing.Size(202, 45)
        Me._sineFreqRTrackBar.TabIndex = 6
        Me._sineFreqRTrackBar.Value = 42
        '
        '_labelL
        '
        Me._labelL.AutoSize = True
        Me._labelL.Location = New System.Drawing.Point(14, 95)
        Me._labelL.Name = "_labelL"
        Me._labelL.Size = New System.Drawing.Size(13, 13)
        Me._labelL.TabIndex = 0
        Me._labelL.Text = "L"
        '
        '_volumeTrackBar
        '
        Me._volumeTrackBar.Location = New System.Drawing.Point(222, 279)
        Me._volumeTrackBar.Maximum = 100
        Me._volumeTrackBar.Name = "_volumeTrackBar"
        Me._volumeTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical
        Me._volumeTrackBar.Size = New System.Drawing.Size(45, 170)
        Me._volumeTrackBar.TabIndex = 9
        Me._volumeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None
        Me._volumeTrackBar.Value = 50
        '
        '_outputGroupBox
        '
        Me._outputGroupBox.Controls.Add(Me._outputAudioDevicesRefreshButton)
        Me._outputGroupBox.Controls.Add(Me._switchOnButton)
        Me._outputGroupBox.Controls.Add(Me._outputAudioDevicesListBox)
        Me._outputGroupBox.Controls.Add(Me._switchOffButton)
        Me._outputGroupBox.Controls.Add(Me._frequencyGroupBox)
        Me._outputGroupBox.Controls.Add(Me._volumeTrackBar)
        Me._outputGroupBox.Location = New System.Drawing.Point(12, 12)
        Me._outputGroupBox.Name = "_outputGroupBox"
        Me._outputGroupBox.Size = New System.Drawing.Size(269, 457)
        Me._outputGroupBox.TabIndex = 0
        Me._outputGroupBox.TabStop = False
        Me._outputGroupBox.Text = "Output [ OFF ]"
        '
        '_outputAudioDevicesRefreshButton
        '
        Me._outputAudioDevicesRefreshButton.Location = New System.Drawing.Point(6, 250)
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
        Me._outputAudioDevicesListBox.Size = New System.Drawing.Size(256, 225)
        Me._outputAudioDevicesListBox.TabIndex = 1
        '
        '_inputGroupBox
        '
        Me._inputGroupBox.Controls.Add(Me._blindZoneLabel)
        Me._inputGroupBox.Controls.Add(Me._sineHarmWidthLabel_)
        Me._inputGroupBox.Controls.Add(Me._blindZoneTrackBar)
        Me._inputGroupBox.Controls.Add(Me._inputAudioDevicesRefreshButton)
        Me._inputGroupBox.Controls.Add(Me._inputAudioDevicesListBox)
        Me._inputGroupBox.Controls.Add(Me._blocksLabel_)
        Me._inputGroupBox.Controls.Add(Me._captureOffButton)
        Me._inputGroupBox.Controls.Add(Me._blocksLabel)
        Me._inputGroupBox.Controls.Add(Me._captureOnButton)
        Me._inputGroupBox.Location = New System.Drawing.Point(287, 12)
        Me._inputGroupBox.Name = "_inputGroupBox"
        Me._inputGroupBox.Size = New System.Drawing.Size(267, 457)
        Me._inputGroupBox.TabIndex = 10
        Me._inputGroupBox.TabStop = False
        Me._inputGroupBox.Text = "Input [ OFF ]"
        '
        '_blindZoneLabel
        '
        Me._blindZoneLabel.AutoSize = True
        Me._blindZoneLabel.Location = New System.Drawing.Point(6, 315)
        Me._blindZoneLabel.Name = "_blindZoneLabel"
        Me._blindZoneLabel.Size = New System.Drawing.Size(13, 13)
        Me._blindZoneLabel.TabIndex = 38
        Me._blindZoneLabel.Text = "0"
        '
        '_sineHarmWidthLabel_
        '
        Me._sineHarmWidthLabel_.AutoSize = True
        Me._sineHarmWidthLabel_.Location = New System.Drawing.Point(6, 294)
        Me._sineHarmWidthLabel_.Name = "_sineHarmWidthLabel_"
        Me._sineHarmWidthLabel_.Size = New System.Drawing.Size(104, 13)
        Me._sineHarmWidthLabel_.TabIndex = 37
        Me._sineHarmWidthLabel_.Text = "Central ""Blind Zone"""
        '
        '_blindZoneTrackBar
        '
        Me._blindZoneTrackBar.Location = New System.Drawing.Point(6, 331)
        Me._blindZoneTrackBar.Maximum = 100
        Me._blindZoneTrackBar.Minimum = 1
        Me._blindZoneTrackBar.Name = "_blindZoneTrackBar"
        Me._blindZoneTrackBar.Size = New System.Drawing.Size(255, 45)
        Me._blindZoneTrackBar.TabIndex = 13
        Me._blindZoneTrackBar.Value = 70
        '
        '_inputAudioDevicesRefreshButton
        '
        Me._inputAudioDevicesRefreshButton.Location = New System.Drawing.Point(6, 250)
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
        Me._inputAudioDevicesListBox.Size = New System.Drawing.Size(256, 225)
        Me._inputAudioDevicesListBox.TabIndex = 11
        '
        '_blocksLabel_
        '
        Me._blocksLabel_.AutoSize = True
        Me._blocksLabel_.Location = New System.Drawing.Point(6, 398)
        Me._blocksLabel_.Name = "_blocksLabel_"
        Me._blocksLabel_.Size = New System.Drawing.Size(42, 13)
        Me._blocksLabel_.TabIndex = 37
        Me._blocksLabel_.Text = "Blocks:"
        '
        '_captureOffButton
        '
        Me._captureOffButton.BackColor = System.Drawing.Color.Silver
        Me._captureOffButton.Location = New System.Drawing.Point(214, 426)
        Me._captureOffButton.Name = "_captureOffButton"
        Me._captureOffButton.Size = New System.Drawing.Size(47, 23)
        Me._captureOffButton.TabIndex = 15
        Me._captureOffButton.Text = "Off"
        Me._captureOffButton.UseVisualStyleBackColor = False
        '
        '_blocksLabel
        '
        Me._blocksLabel.AutoSize = True
        Me._blocksLabel.Location = New System.Drawing.Point(54, 398)
        Me._blocksLabel.Name = "_blocksLabel"
        Me._blocksLabel.Size = New System.Drawing.Size(13, 13)
        Me._blocksLabel.TabIndex = 36
        Me._blocksLabel.Text = "0"
        '
        '_captureOnButton
        '
        Me._captureOnButton.BackColor = System.Drawing.Color.MediumSpringGreen
        Me._captureOnButton.Location = New System.Drawing.Point(6, 426)
        Me._captureOnButton.Name = "_captureOnButton"
        Me._captureOnButton.Size = New System.Drawing.Size(202, 23)
        Me._captureOnButton.TabIndex = 14
        Me._captureOnButton.Text = "Capture On"
        Me._captureOnButton.UseVisualStyleBackColor = False
        '
        '_waterfallDisplayBitmapControl
        '
        Me._waterfallDisplayBitmapControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me._waterfallDisplayBitmapControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me._waterfallDisplayBitmapControl.Location = New System.Drawing.Point(12, 475)
        Me._waterfallDisplayBitmapControl.Name = "_waterfallDisplayBitmapControl"
        Me._waterfallDisplayBitmapControl.Size = New System.Drawing.Size(542, 177)
        Me._waterfallDisplayBitmapControl.TabIndex = 16
        '
        '_displayLeftCheckBox
        '
        Me._displayLeftCheckBox.AutoSize = True
        Me._displayLeftCheckBox.Checked = True
        Me._displayLeftCheckBox.CheckState = System.Windows.Forms.CheckState.Checked
        Me._displayLeftCheckBox.Location = New System.Drawing.Point(12, 658)
        Me._displayLeftCheckBox.Name = "_displayLeftCheckBox"
        Me._displayLeftCheckBox.Size = New System.Drawing.Size(15, 14)
        Me._displayLeftCheckBox.TabIndex = 16
        Me._displayLeftCheckBox.UseVisualStyleBackColor = True
        '
        '_displayRightCheckBox
        '
        Me._displayRightCheckBox.AutoSize = True
        Me._displayRightCheckBox.Checked = True
        Me._displayRightCheckBox.CheckState = System.Windows.Forms.CheckState.Checked
        Me._displayRightCheckBox.Location = New System.Drawing.Point(540, 658)
        Me._displayRightCheckBox.Name = "_displayRightCheckBox"
        Me._displayRightCheckBox.Size = New System.Drawing.Size(15, 14)
        Me._displayRightCheckBox.TabIndex = 19
        Me._displayRightCheckBox.UseVisualStyleBackColor = True
        '
        '_displayRightWithLeftCheckBox
        '
        Me._displayRightWithLeftCheckBox.AutoSize = True
        Me._displayRightWithLeftCheckBox.Location = New System.Drawing.Point(30, 658)
        Me._displayRightWithLeftCheckBox.Name = "_displayRightWithLeftCheckBox"
        Me._displayRightWithLeftCheckBox.Size = New System.Drawing.Size(15, 14)
        Me._displayRightWithLeftCheckBox.TabIndex = 17
        Me._displayRightWithLeftCheckBox.UseVisualStyleBackColor = True
        '
        '_displayCenterCheckBox
        '
        Me._displayCenterCheckBox.AutoSize = True
        Me._displayCenterCheckBox.Checked = True
        Me._displayCenterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked
        Me._displayCenterCheckBox.Location = New System.Drawing.Point(278, 658)
        Me._displayCenterCheckBox.Name = "_displayCenterCheckBox"
        Me._displayCenterCheckBox.Size = New System.Drawing.Size(15, 14)
        Me._displayCenterCheckBox.TabIndex = 18
        Me._displayCenterCheckBox.UseVisualStyleBackColor = True
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(566, 676)
        Me.Controls.Add(Me._displayCenterCheckBox)
        Me.Controls.Add(Me._displayRightWithLeftCheckBox)
        Me.Controls.Add(Me._displayRightCheckBox)
        Me.Controls.Add(Me._displayLeftCheckBox)
        Me.Controls.Add(Me._waterfallDisplayBitmapControl)
        Me.Controls.Add(Me._inputGroupBox)
        Me.Controls.Add(Me._outputGroupBox)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "MainForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "ExactDopplerTest (09.04.2016 10:00)"
        CType(Me._sineFreqLTrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me._frequencyGroupBox.ResumeLayout(False)
        Me._frequencyGroupBox.PerformLayout()
        CType(Me._sineFreqRTrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me._volumeTrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me._outputGroupBox.ResumeLayout(False)
        Me._outputGroupBox.PerformLayout()
        Me._inputGroupBox.ResumeLayout(False)
        Me._inputGroupBox.PerformLayout()
        CType(Me._blindZoneTrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents _switchOnButton As Button
    Friend WithEvents _switchOffButton As Button
    Friend WithEvents _sineFreqLTrackBar As TrackBar
    Friend WithEvents _frequencyGroupBox As GroupBox
    Friend WithEvents _sineFreqRTrackBar As TrackBar
    Friend WithEvents _labelL As Label
    Friend WithEvents _labelR As Label
    Friend WithEvents _sineFreqRLabel As Label
    Friend WithEvents _sineFreqLLabel As Label
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
    Friend WithEvents _blindZoneLabel As Label
    Friend WithEvents _sineHarmWidthLabel_ As Label
    Friend WithEvents _blindZoneTrackBar As TrackBar
    Friend WithEvents _displayLeftCheckBox As CheckBox
    Friend WithEvents _displayRightCheckBox As CheckBox
    Friend WithEvents _displayRightWithLeftCheckBox As CheckBox
    Friend WithEvents _displayCenterCheckBox As CheckBox
    Friend WithEvents _mixCheckBox As CheckBox
End Class
