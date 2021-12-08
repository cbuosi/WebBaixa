Imports GFT.Util

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSobre
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSobre))
        Me.timerCubo = New System.Windows.Forms.Timer(Me.components)
        Me.btnVoltar = New GFT.Util.SuperButton()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'timerCubo
        '
        Me.timerCubo.Interval = 10
        '
        'btnVoltar
        '
        Me.btnVoltar.BackColor = System.Drawing.Color.Transparent
        Me.btnVoltar.BackgroundImage = CType(resources.GetObject("btnVoltar.BackgroundImage"), System.Drawing.Image)
        Me.btnVoltar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.btnVoltar.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnVoltar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(150, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btnVoltar.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnVoltar.Font = New System.Drawing.Font("Verdana", 9.0!)
        Me.btnVoltar.ForeColor = System.Drawing.Color.Black
        Me.btnVoltar.Image = Global.WebBaixa.My.Resources.Resources.Erro
        Me.btnVoltar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnVoltar.Location = New System.Drawing.Point(187, 382)
        Me.btnVoltar.Name = "btnVoltar"
        Me.btnVoltar.Size = New System.Drawing.Size(92, 50)
        Me.btnVoltar.TabIndex = 12
        Me.btnVoltar.Text = "&Voltar"
        Me.btnVoltar.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnVoltar.UseCompatibleTextRendering = True
        Me.btnVoltar.UseVisualStyleBackColor = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(166, 7)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(126, 24)
        Me.Label1.TabIndex = 20
        Me.Label1.Text = "WebBaixa 1.1"
        '
        'frmSobre
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackgroundImage = Global.WebBaixa.My.Resources.Resources.moska
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(459, 442)
        Me.ControlBox = False
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnVoltar)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmSobre"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = ".: Sobre :."
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnVoltar As SuperButton
    Friend WithEvents timerCubo As System.Windows.Forms.Timer
    Friend WithEvents Label1 As Label
End Class
