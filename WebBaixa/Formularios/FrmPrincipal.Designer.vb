<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmPrincipal
    Inherits System.Windows.Forms.Form

    'Descartar substituições de formulário para limpar a lista de componentes.
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

    'Exigido pelo Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'OBSERVAÇÃO: o procedimento a seguir é exigido pelo Windows Form Designer
    'Pode ser modificado usando o Windows Form Designer.  
    'Não o modifique usando o editor de códigos.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmPrincipal))
        Me.txtLog = New System.Windows.Forms.TextBox()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.pb1 = New System.Windows.Forms.ProgressBar()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.txt_nm_pais_aquis = New GFT.Util.SuperTextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txt_exportador_nome = New GFT.Util.SuperTextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.txt_importador_nome = New GFT.Util.SuperTextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.txt_incoterm = New GFT.Util.SuperTextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txt_nm_pais_origem = New GFT.Util.SuperTextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txt_desc_prodt = New GFT.Util.SuperTextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txt_cdncm_compl = New GFT.Util.SuperTextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtInfo = New GFT.Util.SuperLetreiro()
        Me.cmdXls = New GFT.Util.SuperButton()
        Me.cmdLimpa = New GFT.Util.SuperButton()
        Me.cmdPesq = New GFT.Util.SuperButton()
        Me.lvResult = New GFT.Util.SuperLV()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.cmdHelp = New GFT.Util.SuperButton()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtLog
        '
        Me.txtLog.Font = New System.Drawing.Font("Consolas", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLog.Location = New System.Drawing.Point(8, 39)
        Me.txtLog.Multiline = True
        Me.txtLog.Name = "txtLog"
        Me.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtLog.Size = New System.Drawing.Size(983, 587)
        Me.txtLog.TabIndex = 2
        Me.txtLog.WordWrap = False
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(916, 10)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(75, 23)
        Me.Button4.TabIndex = 5
        Me.Button4.Text = "GO!"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'pb1
        '
        Me.pb1.Location = New System.Drawing.Point(8, 10)
        Me.pb1.Name = "pb1"
        Me.pb1.Size = New System.Drawing.Size(902, 23)
        Me.pb1.TabIndex = 6
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Location = New System.Drawing.Point(5, 7)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(1301, 658)
        Me.TabControl1.TabIndex = 7
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.cmdHelp)
        Me.TabPage1.Controls.Add(Me.txt_nm_pais_aquis)
        Me.TabPage1.Controls.Add(Me.Label5)
        Me.TabPage1.Controls.Add(Me.txt_exportador_nome)
        Me.TabPage1.Controls.Add(Me.Label6)
        Me.TabPage1.Controls.Add(Me.txt_importador_nome)
        Me.TabPage1.Controls.Add(Me.Label7)
        Me.TabPage1.Controls.Add(Me.txt_incoterm)
        Me.TabPage1.Controls.Add(Me.Label4)
        Me.TabPage1.Controls.Add(Me.txt_nm_pais_origem)
        Me.TabPage1.Controls.Add(Me.Label3)
        Me.TabPage1.Controls.Add(Me.txt_desc_prodt)
        Me.TabPage1.Controls.Add(Me.Label2)
        Me.TabPage1.Controls.Add(Me.txt_cdncm_compl)
        Me.TabPage1.Controls.Add(Me.Label1)
        Me.TabPage1.Controls.Add(Me.txtInfo)
        Me.TabPage1.Controls.Add(Me.cmdXls)
        Me.TabPage1.Controls.Add(Me.cmdLimpa)
        Me.TabPage1.Controls.Add(Me.cmdPesq)
        Me.TabPage1.Controls.Add(Me.lvResult)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(1293, 632)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Pesquisa"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'txt_nm_pais_aquis
        '
        Me.txt_nm_pais_aquis.Alterado = False
        Me.txt_nm_pais_aquis.BackColor = System.Drawing.Color.White
        Me.txt_nm_pais_aquis.CorFundoSelecionado = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(251, Byte), Integer), CType(CType(206, Byte), Integer))
        Me.txt_nm_pais_aquis.Location = New System.Drawing.Point(111, 64)
        Me.txt_nm_pais_aquis.Name = "txt_nm_pais_aquis"
        Me.txt_nm_pais_aquis.Size = New System.Drawing.Size(219, 20)
        Me.txt_nm_pais_aquis.SuperMascara = ""
        Me.txt_nm_pais_aquis.SuperObrigatorio = False
        Me.txt_nm_pais_aquis.SuperTravaErrors = False
        Me.txt_nm_pais_aquis.SuperTxtCorDesabilitado = System.Drawing.Color.Empty
        Me.txt_nm_pais_aquis.SuperTxtObrigatorio = ""
        Me.txt_nm_pais_aquis.SuperUsaMascara = GFT.Util.SuperTextBox.TipoMascara_.NENHUMA
        Me.txt_nm_pais_aquis.TabIndex = 18
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(29, 68)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(76, 13)
        Me.Label5.TabIndex = 17
        Me.Label5.Text = "Pais Aquisição"
        '
        'txt_exportador_nome
        '
        Me.txt_exportador_nome.Alterado = False
        Me.txt_exportador_nome.BackColor = System.Drawing.Color.White
        Me.txt_exportador_nome.CorFundoSelecionado = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(251, Byte), Integer), CType(CType(206, Byte), Integer))
        Me.txt_exportador_nome.Location = New System.Drawing.Point(493, 38)
        Me.txt_exportador_nome.Name = "txt_exportador_nome"
        Me.txt_exportador_nome.Size = New System.Drawing.Size(219, 20)
        Me.txt_exportador_nome.SuperMascara = ""
        Me.txt_exportador_nome.SuperObrigatorio = False
        Me.txt_exportador_nome.SuperTravaErrors = False
        Me.txt_exportador_nome.SuperTxtCorDesabilitado = System.Drawing.Color.Empty
        Me.txt_exportador_nome.SuperTxtObrigatorio = ""
        Me.txt_exportador_nome.SuperUsaMascara = GFT.Util.SuperTextBox.TipoMascara_.NENHUMA
        Me.txt_exportador_nome.TabIndex = 16
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(401, 42)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(86, 13)
        Me.Label6.TabIndex = 15
        Me.Label6.Text = "Prov. Exportador"
        '
        'txt_importador_nome
        '
        Me.txt_importador_nome.Alterado = False
        Me.txt_importador_nome.BackColor = System.Drawing.Color.White
        Me.txt_importador_nome.CorFundoSelecionado = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(251, Byte), Integer), CType(CType(206, Byte), Integer))
        Me.txt_importador_nome.Location = New System.Drawing.Point(111, 38)
        Me.txt_importador_nome.Name = "txt_importador_nome"
        Me.txt_importador_nome.Size = New System.Drawing.Size(219, 20)
        Me.txt_importador_nome.SuperMascara = ""
        Me.txt_importador_nome.SuperObrigatorio = False
        Me.txt_importador_nome.SuperTravaErrors = False
        Me.txt_importador_nome.SuperTxtCorDesabilitado = System.Drawing.Color.Empty
        Me.txt_importador_nome.SuperTxtObrigatorio = ""
        Me.txt_importador_nome.SuperUsaMascara = GFT.Util.SuperTextBox.TipoMascara_.NENHUMA
        Me.txt_importador_nome.TabIndex = 14
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(20, 42)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(85, 13)
        Me.Label7.TabIndex = 13
        Me.Label7.Text = "Prov. Importador"
        '
        'txt_incoterm
        '
        Me.txt_incoterm.Alterado = False
        Me.txt_incoterm.BackColor = System.Drawing.Color.White
        Me.txt_incoterm.CorFundoSelecionado = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(251, Byte), Integer), CType(CType(206, Byte), Integer))
        Me.txt_incoterm.Location = New System.Drawing.Point(825, 12)
        Me.txt_incoterm.Name = "txt_incoterm"
        Me.txt_incoterm.Size = New System.Drawing.Size(219, 20)
        Me.txt_incoterm.SuperMascara = ""
        Me.txt_incoterm.SuperObrigatorio = False
        Me.txt_incoterm.SuperTravaErrors = False
        Me.txt_incoterm.SuperTxtCorDesabilitado = System.Drawing.Color.Empty
        Me.txt_incoterm.SuperTxtObrigatorio = ""
        Me.txt_incoterm.SuperUsaMascara = GFT.Util.SuperTextBox.TipoMascara_.NENHUMA
        Me.txt_incoterm.TabIndex = 12
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(771, 16)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(48, 13)
        Me.Label4.TabIndex = 11
        Me.Label4.Text = "Incoterm"
        '
        'txt_nm_pais_origem
        '
        Me.txt_nm_pais_origem.Alterado = False
        Me.txt_nm_pais_origem.BackColor = System.Drawing.Color.White
        Me.txt_nm_pais_origem.CorFundoSelecionado = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(251, Byte), Integer), CType(CType(206, Byte), Integer))
        Me.txt_nm_pais_origem.Location = New System.Drawing.Point(493, 64)
        Me.txt_nm_pais_origem.Name = "txt_nm_pais_origem"
        Me.txt_nm_pais_origem.Size = New System.Drawing.Size(219, 20)
        Me.txt_nm_pais_origem.SuperMascara = ""
        Me.txt_nm_pais_origem.SuperObrigatorio = False
        Me.txt_nm_pais_origem.SuperTravaErrors = False
        Me.txt_nm_pais_origem.SuperTxtCorDesabilitado = System.Drawing.Color.Empty
        Me.txt_nm_pais_origem.SuperTxtObrigatorio = ""
        Me.txt_nm_pais_origem.SuperUsaMascara = GFT.Util.SuperTextBox.TipoMascara_.NENHUMA
        Me.txt_nm_pais_origem.TabIndex = 10
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(422, 68)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(65, 13)
        Me.Label3.TabIndex = 9
        Me.Label3.Text = "País Origem"
        '
        'txt_desc_prodt
        '
        Me.txt_desc_prodt.Alterado = False
        Me.txt_desc_prodt.BackColor = System.Drawing.Color.White
        Me.txt_desc_prodt.CorFundoSelecionado = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(251, Byte), Integer), CType(CType(206, Byte), Integer))
        Me.txt_desc_prodt.Location = New System.Drawing.Point(493, 12)
        Me.txt_desc_prodt.Name = "txt_desc_prodt"
        Me.txt_desc_prodt.Size = New System.Drawing.Size(219, 20)
        Me.txt_desc_prodt.SuperMascara = ""
        Me.txt_desc_prodt.SuperObrigatorio = False
        Me.txt_desc_prodt.SuperTravaErrors = False
        Me.txt_desc_prodt.SuperTxtCorDesabilitado = System.Drawing.Color.Empty
        Me.txt_desc_prodt.SuperTxtObrigatorio = ""
        Me.txt_desc_prodt.SuperUsaMascara = GFT.Util.SuperTextBox.TipoMascara_.NENHUMA
        Me.txt_desc_prodt.TabIndex = 8
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(392, 16)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(95, 13)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "Descrição Produto"
        '
        'txt_cdncm_compl
        '
        Me.txt_cdncm_compl.Alterado = False
        Me.txt_cdncm_compl.BackColor = System.Drawing.Color.White
        Me.txt_cdncm_compl.CorFundoSelecionado = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(251, Byte), Integer), CType(CType(206, Byte), Integer))
        Me.txt_cdncm_compl.Location = New System.Drawing.Point(111, 12)
        Me.txt_cdncm_compl.Name = "txt_cdncm_compl"
        Me.txt_cdncm_compl.Size = New System.Drawing.Size(153, 20)
        Me.txt_cdncm_compl.SuperMascara = ""
        Me.txt_cdncm_compl.SuperObrigatorio = False
        Me.txt_cdncm_compl.SuperTravaErrors = False
        Me.txt_cdncm_compl.SuperTxtCorDesabilitado = System.Drawing.Color.Empty
        Me.txt_cdncm_compl.SuperTxtObrigatorio = ""
        Me.txt_cdncm_compl.SuperUsaMascara = GFT.Util.SuperTextBox.TipoMascara_.NENHUMA
        Me.txt_cdncm_compl.TabIndex = 6
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(74, 16)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(31, 13)
        Me.Label1.TabIndex = 5
        Me.Label1.Text = "NCM"
        '
        'txtInfo
        '
        Me.txtInfo.CorSombraTexto = System.Drawing.Color.White
        Me.txtInfo.Location = New System.Drawing.Point(14, 577)
        Me.txtInfo.Name = "txtInfo"
        Me.txtInfo.RolagemLetreiro = GFT.Util.SuperLetreiro.Direcao.Esquerda
        Me.txtInfo.Size = New System.Drawing.Size(406, 20)
        Me.txtInfo.TabIndex = 4
        Me.txtInfo.TextoLetreiro = "PRONTO!"
        Me.txtInfo.VelocidadeRolagem = 2
        '
        'cmdXls
        '
        Me.cmdXls.BackColor = System.Drawing.Color.Transparent
        Me.cmdXls.BackgroundImage = CType(resources.GetObject("cmdXls.BackgroundImage"), System.Drawing.Image)
        Me.cmdXls.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.cmdXls.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdXls.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(150, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.cmdXls.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdXls.Font = New System.Drawing.Font("Verdana", 9.0!)
        Me.cmdXls.ForeColor = System.Drawing.Color.Black
        Me.cmdXls.Image = Global.WebBaixa.My.Resources.Resources.xls
        Me.cmdXls.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdXls.Location = New System.Drawing.Point(705, 577)
        Me.cmdXls.Name = "cmdXls"
        Me.cmdXls.Size = New System.Drawing.Size(105, 46)
        Me.cmdXls.TabIndex = 3
        Me.cmdXls.Text = "XLS"
        Me.cmdXls.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cmdXls.UseVisualStyleBackColor = False
        '
        'cmdLimpa
        '
        Me.cmdLimpa.BackColor = System.Drawing.Color.Transparent
        Me.cmdLimpa.BackgroundImage = CType(resources.GetObject("cmdLimpa.BackgroundImage"), System.Drawing.Image)
        Me.cmdLimpa.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.cmdLimpa.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdLimpa.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(150, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.cmdLimpa.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdLimpa.Font = New System.Drawing.Font("Verdana", 9.0!)
        Me.cmdLimpa.ForeColor = System.Drawing.Color.Black
        Me.cmdLimpa.Image = Global.WebBaixa.My.Resources.Resources.prox
        Me.cmdLimpa.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdLimpa.Location = New System.Drawing.Point(594, 577)
        Me.cmdLimpa.Name = "cmdLimpa"
        Me.cmdLimpa.Size = New System.Drawing.Size(105, 46)
        Me.cmdLimpa.TabIndex = 2
        Me.cmdLimpa.Text = "Limpar"
        Me.cmdLimpa.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cmdLimpa.UseVisualStyleBackColor = False
        '
        'cmdPesq
        '
        Me.cmdPesq.BackColor = System.Drawing.Color.Transparent
        Me.cmdPesq.BackgroundImage = CType(resources.GetObject("cmdPesq.BackgroundImage"), System.Drawing.Image)
        Me.cmdPesq.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.cmdPesq.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdPesq.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(150, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.cmdPesq.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdPesq.Font = New System.Drawing.Font("Verdana", 9.0!)
        Me.cmdPesq.ForeColor = System.Drawing.Color.Black
        Me.cmdPesq.Image = Global.WebBaixa.My.Resources.Resources.novo_pesquisar
        Me.cmdPesq.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdPesq.Location = New System.Drawing.Point(483, 577)
        Me.cmdPesq.Name = "cmdPesq"
        Me.cmdPesq.Size = New System.Drawing.Size(105, 46)
        Me.cmdPesq.TabIndex = 1
        Me.cmdPesq.Text = "Pesquisar"
        Me.cmdPesq.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cmdPesq.UseVisualStyleBackColor = False
        '
        'lvResult
        '
        Me.lvResult.HabilitaOrdem = True
        Me.lvResult.HideSelection = False
        Me.lvResult.Location = New System.Drawing.Point(14, 93)
        Me.lvResult.Name = "lvResult"
        Me.lvResult.SelecionaVarios = False
        Me.lvResult.Size = New System.Drawing.Size(1270, 478)
        Me.lvResult.TabIndex = 0
        Me.lvResult.UseCompatibleStateImageBehavior = False
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.txtLog)
        Me.TabPage2.Controls.Add(Me.pb1)
        Me.TabPage2.Controls.Add(Me.Button4)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(1293, 632)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Importação"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'cmdHelp
        '
        Me.cmdHelp.BackColor = System.Drawing.Color.Transparent
        Me.cmdHelp.BackgroundImage = CType(resources.GetObject("cmdHelp.BackgroundImage"), System.Drawing.Image)
        Me.cmdHelp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.cmdHelp.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdHelp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(150, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.cmdHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdHelp.Font = New System.Drawing.Font("Verdana", 9.0!)
        Me.cmdHelp.ForeColor = System.Drawing.Color.Black
        Me.cmdHelp.Image = Global.WebBaixa.My.Resources.Resources.help_1
        Me.cmdHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdHelp.Location = New System.Drawing.Point(816, 577)
        Me.cmdHelp.Name = "cmdHelp"
        Me.cmdHelp.Size = New System.Drawing.Size(98, 46)
        Me.cmdHelp.TabIndex = 19
        Me.cmdHelp.Text = "Sobre"
        Me.cmdHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cmdHelp.UseVisualStyleBackColor = False
        '
        'FrmPrincipal
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1312, 669)
        Me.Controls.Add(Me.TabControl1)
        Me.Name = "FrmPrincipal"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Principal"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtLog As TextBox
    Friend WithEvents Button4 As Button
    Friend WithEvents pb1 As ProgressBar
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents txtInfo As GFT.Util.SuperLetreiro
    Friend WithEvents cmdXls As GFT.Util.SuperButton
    Friend WithEvents cmdLimpa As GFT.Util.SuperButton
    Friend WithEvents cmdPesq As GFT.Util.SuperButton
    Friend WithEvents lvResult As GFT.Util.SuperLV
    Friend WithEvents TabPage2 As TabPage
    Friend WithEvents txt_nm_pais_aquis As GFT.Util.SuperTextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents txt_exportador_nome As GFT.Util.SuperTextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents txt_importador_nome As GFT.Util.SuperTextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents txt_incoterm As GFT.Util.SuperTextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents txt_nm_pais_origem As GFT.Util.SuperTextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents txt_desc_prodt As GFT.Util.SuperTextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents txt_cdncm_compl As GFT.Util.SuperTextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents cmdHelp As GFT.Util.SuperButton
End Class
