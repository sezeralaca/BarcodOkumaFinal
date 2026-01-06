<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmBarkod
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.lblHatA = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnTop = New System.Windows.Forms.Button()
        Me.txtBarcode = New System.Windows.Forms.TextBox()
        Me.btnBarkod = New System.Windows.Forms.Button()
        Me.btnExcel = New System.Windows.Forms.Button()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.txt_tartim1 = New System.Windows.Forms.TextBox()
        Me.txt_tartim2 = New System.Windows.Forms.TextBox()
        Me.txt_tartim3 = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.SerialPort1 = New System.IO.Ports.SerialPort(Me.components)
        Me.SerialPort2 = New System.IO.Ports.SerialPort(Me.components)
        Me.SerialPort3 = New System.IO.Ports.SerialPort(Me.components)
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.Timer2 = New System.Windows.Forms.Timer(Me.components)
        Me.Timer3 = New System.Windows.Forms.Timer(Me.components)
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Timer4 = New System.Windows.Forms.Timer(Me.components)
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.lblConnectionA = New System.Windows.Forms.Label()
        Me.lblConnectionB = New System.Windows.Forms.Label()
        Me.lblConnectionC = New System.Windows.Forms.Label()
        Me.txtBarcodeA = New System.Windows.Forms.TextBox()
        Me.txtBarcodeB = New System.Windows.Forms.TextBox()
        Me.txtBarcodeC = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'lblHatA
        '
        Me.lblHatA.AutoSize = True
        Me.lblHatA.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.lblHatA.Location = New System.Drawing.Point(25, 46)
        Me.lblHatA.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblHatA.Name = "lblHatA"
        Me.lblHatA.Size = New System.Drawing.Size(77, 25)
        Me.lblHatA.TabIndex = 22
        Me.lblHatA.Text = "A Hattý"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.Label1.Location = New System.Drawing.Point(25, 112)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(76, 25)
        Me.Label1.TabIndex = 21
        Me.Label1.Text = "B Hattý"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.Label2.Location = New System.Drawing.Point(25, 183)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(84, 25)
        Me.Label2.TabIndex = 20
        Me.Label2.Text = "C  Hattý"
        '
        'btnTop
        '
        Me.btnTop.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.btnTop.Location = New System.Drawing.Point(372, 128)
        Me.btnTop.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnTop.Name = "btnTop"
        Me.btnTop.Size = New System.Drawing.Size(128, 75)
        Me.btnTop.TabIndex = 19
        Me.btnTop.Text = "Top 10 Son 30 Gün"
        '
        'txtBarcode
        '
        Me.txtBarcode.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.txtBarcode.Location = New System.Drawing.Point(54, 234)
        Me.txtBarcode.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtBarcode.Name = "txtBarcode"
        Me.txtBarcode.Size = New System.Drawing.Size(276, 26)
        Me.txtBarcode.TabIndex = 18
        '
        'btnBarkod
        '
        Me.btnBarkod.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.btnBarkod.Location = New System.Drawing.Point(372, 228)
        Me.btnBarkod.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnBarkod.Name = "btnBarkod"
        Me.btnBarkod.Size = New System.Drawing.Size(128, 36)
        Me.btnBarkod.TabIndex = 17
        Me.btnBarkod.Text = "Barkod"
        '
        'btnExcel
        '
        Me.btnExcel.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.btnExcel.Location = New System.Drawing.Point(372, 44)
        Me.btnExcel.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnExcel.Name = "btnExcel"
        Me.btnExcel.Size = New System.Drawing.Size(128, 76)
        Me.btnExcel.TabIndex = 16
        Me.btnExcel.Text = "Son 3 ay Excel'e at"
        '
        'TextBox1
        '
        Me.TextBox1.BackColor = System.Drawing.SystemColors.MenuText
        Me.TextBox1.ForeColor = System.Drawing.Color.Red
        Me.TextBox1.Location = New System.Drawing.Point(512, 2)
        Me.TextBox1.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.TextBox1.Size = New System.Drawing.Size(491, 280)
        Me.TextBox1.TabIndex = 15
        '
        'txt_tartim1
        '
        Me.txt_tartim1.BackColor = System.Drawing.SystemColors.Desktop
        Me.txt_tartim1.Font = New System.Drawing.Font("Microsoft Sans Serif", 27.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.txt_tartim1.ForeColor = System.Drawing.Color.Lime
        Me.txt_tartim1.Location = New System.Drawing.Point(1091, 214)
        Me.txt_tartim1.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txt_tartim1.Name = "txt_tartim1"
        Me.txt_tartim1.Size = New System.Drawing.Size(304, 60)
        Me.txt_tartim1.TabIndex = 14
        '
        'txt_tartim2
        '
        Me.txt_tartim2.BackColor = System.Drawing.SystemColors.Desktop
        Me.txt_tartim2.Font = New System.Drawing.Font("Microsoft Sans Serif", 27.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.txt_tartim2.ForeColor = System.Drawing.Color.Yellow
        Me.txt_tartim2.Location = New System.Drawing.Point(1091, 36)
        Me.txt_tartim2.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txt_tartim2.Name = "txt_tartim2"
        Me.txt_tartim2.Size = New System.Drawing.Size(304, 60)
        Me.txt_tartim2.TabIndex = 13
        '
        'txt_tartim3
        '
        Me.txt_tartim3.BackColor = System.Drawing.SystemColors.Desktop
        Me.txt_tartim3.Font = New System.Drawing.Font("Microsoft Sans Serif", 27.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.txt_tartim3.ForeColor = System.Drawing.Color.DarkOrange
        Me.txt_tartim3.Location = New System.Drawing.Point(1091, 124)
        Me.txt_tartim3.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txt_tartim3.Name = "txt_tartim3"
        Me.txt_tartim3.Size = New System.Drawing.Size(304, 60)
        Me.txt_tartim3.TabIndex = 12
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.Label3.Location = New System.Drawing.Point(1097, 7)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(164, 25)
        Me.Label3.TabIndex = 11
        Me.Label3.Text = "HAT A TARTIM"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.Label6.Location = New System.Drawing.Point(1099, 188)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(165, 25)
        Me.Label6.TabIndex = 10
        Me.Label6.Text = "HAT C TARTIM"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.Label4.Location = New System.Drawing.Point(1097, 97)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(163, 25)
        Me.Label4.TabIndex = 9
        Me.Label4.Text = "HAT B TARTIM"
        '
        'Timer1
        '
        '
        'Timer2
        '
        '
        'Timer3
        '
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.Red
        Me.Button1.Enabled = False
        Me.Button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Button1.Location = New System.Drawing.Point(1404, 223)
        Me.Button1.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(40, 37)
        Me.Button1.TabIndex = 8
        Me.Button1.UseVisualStyleBackColor = False
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.Color.Red
        Me.Button2.Enabled = False
        Me.Button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Button2.Location = New System.Drawing.Point(1404, 48)
        Me.Button2.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(40, 37)
        Me.Button2.TabIndex = 7
        Me.Button2.UseVisualStyleBackColor = False
        '
        'Button3
        '
        Me.Button3.BackColor = System.Drawing.Color.Red
        Me.Button3.Enabled = False
        Me.Button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Button3.Location = New System.Drawing.Point(1404, 138)
        Me.Button3.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(40, 37)
        Me.Button3.TabIndex = 6
        Me.Button3.UseVisualStyleBackColor = False
        '
        'Timer4
        '
        '
        'ListBox1
        '
        Me.ListBox1.BackColor = System.Drawing.Color.Plum
        Me.ListBox1.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.ListBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.ListBox1.ForeColor = System.Drawing.SystemColors.InfoText
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.HorizontalScrollbar = True
        Me.ListBox1.Location = New System.Drawing.Point(0, 307)
        Me.ListBox1.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.ListBox1.MultiColumn = True
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.ScrollAlwaysVisible = True
        Me.ListBox1.Size = New System.Drawing.Size(1580, 264)
        Me.ListBox1.TabIndex = 23
        '
        'lblConnectionA
        '
        Me.lblConnectionA.AutoSize = True
        Me.lblConnectionA.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.lblConnectionA.ForeColor = System.Drawing.Color.Red
        Me.lblConnectionA.Location = New System.Drawing.Point(132, 23)
        Me.lblConnectionA.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblConnectionA.Name = "lblConnectionA"
        Me.lblConnectionA.Size = New System.Drawing.Size(102, 20)
        Me.lblConnectionA.TabIndex = 5
        Me.lblConnectionA.Text = "Baglý Deðil"
        '
        'lblConnectionB
        '
        Me.lblConnectionB.AutoSize = True
        Me.lblConnectionB.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.lblConnectionB.ForeColor = System.Drawing.Color.Red
        Me.lblConnectionB.Location = New System.Drawing.Point(132, 87)
        Me.lblConnectionB.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblConnectionB.Name = "lblConnectionB"
        Me.lblConnectionB.Size = New System.Drawing.Size(102, 20)
        Me.lblConnectionB.TabIndex = 4
        Me.lblConnectionB.Text = "Baglý Deðil"
        '
        'lblConnectionC
        '
        Me.lblConnectionC.AutoSize = True
        Me.lblConnectionC.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.lblConnectionC.ForeColor = System.Drawing.Color.Red
        Me.lblConnectionC.Location = New System.Drawing.Point(132, 158)
        Me.lblConnectionC.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblConnectionC.Name = "lblConnectionC"
        Me.lblConnectionC.Size = New System.Drawing.Size(102, 20)
        Me.lblConnectionC.TabIndex = 3
        Me.lblConnectionC.Text = "Baglý Deðil"
        '
        'txtBarcodeA
        '
        Me.txtBarcodeA.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.txtBarcodeA.Location = New System.Drawing.Point(136, 47)
        Me.txtBarcodeA.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtBarcodeA.Name = "txtBarcodeA"
        Me.txtBarcodeA.ReadOnly = True
        Me.txtBarcodeA.Size = New System.Drawing.Size(203, 26)
        Me.txtBarcodeA.TabIndex = 2
        '
        'txtBarcodeB
        '
        Me.txtBarcodeB.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.txtBarcodeB.Location = New System.Drawing.Point(136, 111)
        Me.txtBarcodeB.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtBarcodeB.Name = "txtBarcodeB"
        Me.txtBarcodeB.ReadOnly = True
        Me.txtBarcodeB.Size = New System.Drawing.Size(203, 26)
        Me.txtBarcodeB.TabIndex = 1
        '
        'txtBarcodeC
        '
        Me.txtBarcodeC.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(162, Byte))
        Me.txtBarcodeC.Location = New System.Drawing.Point(136, 182)
        Me.txtBarcodeC.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtBarcodeC.Name = "txtBarcodeC"
        Me.txtBarcodeC.ReadOnly = True
        Me.txtBarcodeC.Size = New System.Drawing.Size(203, 26)
        Me.txtBarcodeC.TabIndex = 0
        '
        'frmBarkod
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.BurlyWood
        Me.ClientSize = New System.Drawing.Size(1580, 571)
        Me.Controls.Add(Me.txtBarcodeC)
        Me.Controls.Add(Me.txtBarcodeB)
        Me.Controls.Add(Me.txtBarcodeA)
        Me.Controls.Add(Me.lblConnectionC)
        Me.Controls.Add(Me.lblConnectionB)
        Me.Controls.Add(Me.lblConnectionA)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txt_tartim3)
        Me.Controls.Add(Me.txt_tartim2)
        Me.Controls.Add(Me.txt_tartim1)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.btnExcel)
        Me.Controls.Add(Me.btnBarkod)
        Me.Controls.Add(Me.txtBarcode)
        Me.Controls.Add(Me.btnTop)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lblHatA)
        Me.Controls.Add(Me.ListBox1)
        Me.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Name = "frmBarkod"
        Me.Text = "Barkod Okuma"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblHatA As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents btnTop As System.Windows.Forms.Button
    Friend WithEvents txtBarcode As System.Windows.Forms.TextBox
    Friend WithEvents btnBarkod As System.Windows.Forms.Button
    Friend WithEvents btnExcel As System.Windows.Forms.Button
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents txt_tartim1 As System.Windows.Forms.TextBox
    Friend WithEvents txt_tartim2 As System.Windows.Forms.TextBox
    Friend WithEvents txt_tartim3 As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents SerialPort1 As System.IO.Ports.SerialPort
    Friend WithEvents SerialPort2 As System.IO.Ports.SerialPort
    Friend WithEvents SerialPort3 As System.IO.Ports.SerialPort
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents Timer2 As System.Windows.Forms.Timer
    Friend WithEvents Timer3 As System.Windows.Forms.Timer
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents Timer4 As System.Windows.Forms.Timer
    Friend WithEvents ListBox1 As System.Windows.Forms.ListBox
    Friend WithEvents lblConnectionA As System.Windows.Forms.Label
    Friend WithEvents lblConnectionB As System.Windows.Forms.Label
    Friend WithEvents lblConnectionC As System.Windows.Forms.Label
    Friend WithEvents txtBarcodeA As System.Windows.Forms.TextBox
    Friend WithEvents txtBarcodeB As System.Windows.Forms.TextBox
    Friend WithEvents txtBarcodeC As System.Windows.Forms.TextBox

End Class
