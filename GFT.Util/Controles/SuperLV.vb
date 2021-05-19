Option Explicit On
Option Strict On

Imports System.ComponentModel
Imports System.Drawing

Public Class SuperLV
    Inherits ListView

    Private _bOrdena As Boolean = True
    Private _mSelect As Boolean = False
    Private _iChave As Integer = 0
    Private _Chave As Object = ""

    Private _bControlaLink As Boolean = False 'Por padrão, nao tem essa viadagem
    Private _TextoLink As String = ""
    Private _UrlLink As String

    Private m_ColunaOrdenada As ColumnHeader
    Private m_NovaColunaOrdenada As ColumnHeader
    Private OrdemClassificacao As System.Windows.Forms.SortOrder = SortOrder.Ascending

    Private Checado As String = "ü"
    Private Deschecado As String = "û"

    Public bAtualizando As Boolean = False

    Protected Overrides Sub OnGotFocus(ByVal e As System.EventArgs)
        Try
            'Me.BackColor = corObjSelecionado
            MyBase.OnGotFocus(e)
        Catch ex As Exception
            LogaErro("Erro em SuperLV::OnGotFocus: " & CStr(ex.ToString()))
        End Try
    End Sub

    Protected Overrides Sub OnLostFocus(ByVal e As System.EventArgs)
        Try
            'Me.BackColor = corObjNaoSelecionado
            MyBase.OnLostFocus(e)
        Catch ex As Exception
            LogaErro("Erro em SuperLV::OnLostFocus: " & CStr(ex.ToString()))
        End Try
    End Sub

    Public Sub New()
        Try
            Me.DoubleBuffered = True
        Catch ex As Exception
            LogaErro("Erro em SuperLV::New: " & CStr(ex.ToString()))
        End Try
    End Sub

    <Category("SuperLV"), Description("Deixa selecionar varias linhas na grid")>
    Public Property SelecionaVarios As Boolean
        Get
            Return _mSelect
        End Get
        Set(ByVal value As Boolean)
            _mSelect = value
            Me.Invalidate()
        End Set
    End Property

    <Category("SuperLV"), Description("Habilita Ordenação")>
    Public Property HabilitaOrdem As Boolean
        Get
            Return _bOrdena
        End Get
        Set(ByVal value As Boolean)
            _bOrdena = value
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnItemCheck(ByVal ice As System.Windows.Forms.ItemCheckEventArgs)
        Try

            If bAtualizando = True Then
                Exit Sub
            End If

            If _mSelect = False Then
                Dim i As Integer
                If ice.NewValue = CheckState.Checked Then
                    For i = 0 To Me.Items.Count - 1
                        If Me.Items(i).Checked = True Then
                            Me.Items(i).Checked = False
                        End If
                    Next i
                End If
            End If

            If ice.NewValue = CheckState.Checked Then
                Me.Items(ice.Index).Selected = True
                If _iChave = 0 Then 'se for da primeira coluna....
                    _Chave = Me.Items(ice.Index).Text
                Else 'se for da segunda em diante...
                    _Chave = Me.Items(ice.Index).SubItems(_iChave).Text
                End If
            Else
                _Chave = Nothing
            End If

            MyBase.OnItemCheck(ice)
        Catch ex As Exception
            LogaErro("Erro em SuperLV::OnItemCheck: " & ex.ToString())
        End Try
    End Sub

    Public Function ObterCodigoLinha(ByVal nLinha As Integer) As String
        Try

            If Me.Items.Count < nLinha Then
                Return ""
            End If

            If _iChave = 0 Then 'se for da primeira coluna....
                Return Me.Items(nLinha).Text
            Else 'se for da segunda em diante...
                Return Me.Items(nLinha).SubItems(_iChave).Text
            End If
        Catch ex As Exception
            LogaErro("Erro em SuperLV::ObterCodigoLinha: " & ex.ToString())
            Return ""
        End Try
    End Function

    Public Function ObterChave() As Decimal
        Try

            If Me.SelecionaVarios = False Then

                If IsNumeric(_Chave) = True Then
                    Return CDec(_Chave)
                Else
                    Return 0
                End If
            Else

                If Me.ObterTotalChecados <> 1 Then
                    Return 0D
                End If

                For i = 0 To Me.ObterTotalLinhas() - 1 Step 1
                    If Me.Items(i).Checked = True Then

                        If _iChave = 0 Then 'se for da primeira coluna....
                            _Chave = Me.Items(i).Text
                        Else 'se for da segunda em diante...
                            _Chave = Me.Items(i).SubItems(_iChave).Text
                        End If

                        Return CDec(_Chave)

                    End If
                Next i

                Return 0

            End If
        Catch ex As Exception
            LogaErro("Erro em SuperLV::ObterChave: " & ex.ToString())
            Return 0
        End Try
    End Function

    Public Function ObterChaveComposta() As String
        Try
            Return _Chave.ToString
        Catch ex As Exception
            LogaErro("Erro em SuperLV::ObterChaveComposta: " & ex.ToString())
            Return ""
        End Try
    End Function

    Public Function ObterTotalChecados() As Integer
        Try

            Dim i As Integer = 0
            Dim totChecked As Integer = 0

            For i = 0 To Me.ObterTotalLinhas() - 1 Step 1
                If Me.Items(i).Checked = True Then
                    totChecked += 1
                End If
            Next i

            Return totChecked
        Catch ex As Exception
            LogaErro("Erro em SuperLV::ObterTotalChecados : " & ex.Message)
            Return -1
        End Try

    End Function

    Public Function ObterTotalLinhas() As Integer
        Try
            Return Me.Items.Count
        Catch ex As Exception
            LogaErro("Erro em SuperLV::ObterTotalLinhas")
            Return -1
        End Try
    End Function

    Public Sub PreencheGridDS2(ByVal rs1 As SuperDataSet,
                               ByVal chk_box As Boolean,
                               Optional ByVal BarraTitulo As Boolean = True,
                               Optional ByVal Contador As Boolean = False,
                               Optional ByVal Zebrado As Boolean = True,
                               Optional ByVal nTabela As Integer = 0)

        Dim idxColuna As Integer
        Dim clmX As ColumnHeader
        Dim itmX As ListViewItem
        'Dim SubItm As ListViewItem.ListViewSubItem
        Dim strTamanho As String
        Dim intTamanho As Integer
        Dim nContador As Long
        Dim strCampo As String
        Dim posReg As Integer
        Dim ValorCampo As Object

        Try

            Me.bAtualizando = True
            Me.BeginUpdate()

            If rs1 Is Nothing Then
                Me.Items.Clear()
                LogaErro("SuperLV::PreencheGridDS [" & Me.Name & "] - ATENÇÃO: RecordSet=Nothing, por favor, verifique.")
                Exit Sub
            End If

            LogaErro("SuperLV::PreencheGridDS [" & Me.Name & "] - N.Registros [" & rs1.TotalRegistros(nTabela).ToString & "]")
            _iChave = 0

            Me.View = View.Details                              'Define o modo de exibição do listview
            Me.LabelEdit = False                                'Permite o usuario editar o item
            Me.AllowColumnReorder = False                       'Permite o usuario rearranjar as colunas
            Me.CheckBoxes = chk_box                             'Exibe as caixas de marcacao (check boxes.)
            Me.FullRowSelect = True                             'Seleciona um item e subitem quando a seleção é feita
            Me.GridLines = True                                 'Exibe as linhas
            Me.Sorting = SortOrder.None                         'Ordena os itens na list na ordem ascendente
            Me.MultiSelect = False                              'Deixa selecionar celula individual
            Me.BackgroundImageTiled = True                      'Liga fundo com imagem
            Me.DoubleBuffered = True                            'Duplo Amanteigado

            Dim lvChecado As New ListViewItem.ListViewSubItem

            If BarraTitulo = True Then
                Me.HeaderStyle = ColumnHeaderStyle.Clickable
            Else
                Me.HeaderStyle = ColumnHeaderStyle.None
            End If

            _iChave = 0

            nContador = 0

            Me.Clear()

            Me.ListViewItemSorter = Nothing

            'Se tiver Coluna de contador, inclui...
            idxColuna = 0

            If Contador = True Then
                clmX = New ColumnHeader()
                clmX.Name = "Nº"
                clmX.Text = "Nº"
                clmX.Width = 45
                Me.Columns.Add(clmX)
                idxColuna = idxColuna + 1
            End If

            ''#############################################
            ''#####     FOR pra montar as colunas.    #####
            ''#############################################
            For i = 0 To rs1.TotalColunas(nTabela) - 1 'FieldCount() - 1

                strCampo = rs1.NomeColuna(i, nTabela)

                'procura por # e tenta pegar um numerico depois dele. Ex: as_coluna#123 <- pega 123
                strTamanho = strCampo.Substring(strCampo.IndexOf("#") + 1, strCampo.Length - strCampo.IndexOf("#") - 1)

                'se deu certo o comando anterior OK, senao faz calculo (tam * 10)
                If IsNumeric(strTamanho) = True Then
                    intTamanho = CInt(Val(strTamanho))
                Else
                    intTamanho = Len(strCampo) * 10
                End If

                'So entra no grid se for as_ , me_ , nu_
                If (strCampo.Substring(0, 3) = "as_") Or
                   (strCampo.Substring(0, 3) = "me_") Or
                   (strCampo.Substring(0, 3) = "nu_") Then

                    clmX = New ColumnHeader
                    clmX.Name = Formata_Coluna(strCampo)
                    clmX.Text = Formata_Coluna(strCampo)
                    clmX.Width = intTamanho

                    'Se for numérico/moeda então alinha à direita
                    If strCampo.Substring(0, 3) = "me_" Or strCampo.Substring(0, 3) = "nu_" Then
                        clmX.TextAlign = HorizontalAlignment.Right
                    ElseIf (rs1.TipoDadosColuna(i, nTabela) Is GetType(Decimal)) Then
                        clmX.TextAlign = HorizontalAlignment.Right
                    ElseIf (rs1.TipoDadosColuna(i, nTabela) Is GetType(DateTime)) Then
                        clmX.TextAlign = HorizontalAlignment.Center
                    Else  'Se for alfanumérico então alinha à esquerda
                        clmX.TextAlign = HorizontalAlignment.Left
                    End If

                    Me.Columns.Add(clmX)

                ElseIf (strCampo.Substring(0, 3) = "id_") Then
                    clmX = New ColumnHeader()
                    clmX.Name = Formata_Coluna(strCampo)
                    clmX.Text = Formata_Coluna(strCampo, 1)
                    clmX.Width = 0
                    Me.Columns.Add(clmX)
                ElseIf (strCampo.Substring(0, 3) = "ck_") Then
                    clmX = New ColumnHeader()
                    clmX.Name = Formata_Coluna(strCampo)
                    clmX.Text = Formata_Coluna(strCampo)
                    clmX.Width = intTamanho
                    clmX.TextAlign = HorizontalAlignment.Center
                    Me.Columns.Add(clmX)
                End If

                'retorna em qual coluna esta o id_ (chave)
                If (strCampo.Substring(0, 3) = "as_") Or
                   (strCampo.Substring(0, 3) = "me_") Or
                   (strCampo.Substring(0, 3) = "id_") Or
                   (strCampo.Substring(0, 3) = "nu_") Or
                   (strCampo.Substring(0, 3) = "ck_") Then
                    If (strCampo.Substring(0, 3) = "id_") Then
                        'PreencheGridDS = idxColuna 'GUARDA A COLUNA QUE ESTA O ID DO REGISTRO 'tirar depois
                        _iChave = idxColuna
                    End If
                    idxColuna = idxColuna + 1 'aumenta o indice se entrar na grid ("id_" , "as_", "me_", "nu_", "ch_")
                End If
            Next i
            '#############################################
            '#####  FIM FOR pra montar as colunas.   #####
            '#############################################

            '################################################
            '#####     FOR pra Preencher as linhas.    #####
            '################################################
            'Do While rs1.Read

            Dim SubItem As ListViewItem.ListViewSubItem
            For posReg = 0 To (rs1.TotalRegistros(nTabela) - 1) Step 1

                idxColuna = 0
                nContador = nContador + 1

                itmX = New ListViewItem()

                itmX.UseItemStyleForSubItems = False

                If Contador = True Then
                    itmX.Name = "Contador"
                    itmX.Text = CStr(posReg + 1)
                    idxColuna = idxColuna + 1
                End If

                'i = indice da coluna (campo) do recordset
                For i = 0 To (rs1.TotalColunas(nTabela) - 1)

                    strCampo = rs1.NomeColuna(i, nTabela)
                    ValorCampo = rs1.ValorCampo(i, posReg, nTabela)

                    'se campo eh do tipo checkbox
                    If chk_box = True And idxColuna = 0 Then '
                        If IsDBNull(rs1.ValorCampo("chk", posReg, nTabela)) = True Then
                            If itmX.Checked = True Then
                                itmX.Checked = False
                            End If
                        Else
                            If CStr(rs1.ValorCampo("chk", posReg, nTabela)) = "" Then
                                If itmX.Checked = True Then
                                    itmX.Checked = False
                                End If
                            Else
                                If itmX.Checked = False Then
                                    itmX.Checked = True
                                End If
                            End If
                        End If
                    End If

                    If (strCampo.Substring(0, 3) = "as_") Then
                        If idxColuna = 0 Then
                            itmX.Name = Formata_Coluna(strCampo)
                            itmX.Text = ValorCampo.ToString
                        Else
                            SubItem = New ListViewItem.ListViewSubItem
                            SubItem.Name = Formata_Coluna(strCampo)
                            SubItem.Text = ValorCampo.ToString
                            itmX.SubItems.Add(SubItem)
                        End If
                        idxColuna = idxColuna + 1
                    ElseIf (strCampo.Substring(0, 3) = "me_") Then
                        If IsNumeric(ValorCampo.ToString) Then
                            If idxColuna = 0 Then
                                itmX.Name = Formata_Coluna(strCampo)
                                itmX.Text = CDec(ValorCampo.ToString).ToString("#,##0.00")
                            Else
                                SubItem = New ListViewItem.ListViewSubItem
                                'itmX.SubItems.Add(CDec(ValorCampo.ToString).ToString("#,##0.00"))
                                SubItem.Name = Formata_Coluna(strCampo)
                                SubItem.Text = CDec(ValorCampo.ToString).ToString("#,##0.00")
                                itmX.SubItems.Add(SubItem)
                            End If
                        Else
                            If idxColuna = 0 Then
                                itmX.Name = Formata_Coluna(strCampo)
                                itmX.Text = ValorCampo.ToString
                            Else
                                SubItem = New ListViewItem.ListViewSubItem
                                SubItem.Name = Formata_Coluna(strCampo)
                                SubItem.Text = ValorCampo.ToString
                                itmX.SubItems.Add(SubItem)
                                'itmX.SubItems.Add(ValorCampo.ToString)
                            End If
                        End If
                        idxColuna = idxColuna + 1
                    ElseIf (strCampo.Substring(0, 3) = "nu_") Then

                        If idxColuna = 0 Then
                            itmX.Name = Formata_Coluna(strCampo)
                            If IsNumeric(ValorCampo) Then
                                itmX.Text = ValorCampo.ToString()
                            Else
                                itmX.Text = ""
                            End If
                        Else
                            SubItem = New ListViewItem.ListViewSubItem
                            If IsNumeric(ValorCampo) Then
                                SubItem.Name = Formata_Coluna(strCampo)
                                SubItem.Text = ValorCampo.ToString()
                                itmX.SubItems.Add(SubItem)
                                'itmX.SubItems.Add(ValorCampo.ToString())
                            Else
                                SubItem.Name = Formata_Coluna(strCampo)
                                SubItem.Text = ""
                                itmX.SubItems.Add(SubItem)
                                'itmX.SubItems.Add("")
                            End If
                        End If
                        idxColuna = idxColuna + 1

                    ElseIf (strCampo.Substring(0, 3) = "id_") Then
                        If idxColuna = 0 Then
                            itmX.Name = Formata_Coluna(strCampo)
                            itmX.Text = ValorCampo.ToString
                        Else
                            SubItem = New ListViewItem.ListViewSubItem
                            SubItem.Name = Formata_Coluna(strCampo)
                            SubItem.Text = ValorCampo.ToString
                            itmX.SubItems.Add(SubItem)
                            'itmX.SubItems.Add(ValorCampo.ToString)
                        End If
                        idxColuna = idxColuna + 1
                    ElseIf (strCampo.Substring(0, 3) = "ck_") And idxColuna > 0 Then
                        lvChecado = New ListViewItem.ListViewSubItem
                        lvChecado.Name = Formata_Coluna(strCampo)
                        lvChecado.Font = New Drawing.Font("Wingdings", 14)
                        If IsDBNull(rs1.ValorCampo(i, posReg, nTabela)) = True Then
                            lvChecado.Text = Deschecado
                            lvChecado.ForeColor = Drawing.Color.Red
                        ElseIf CDec(rs1.ValorCampo(i, posReg, nTabela)) = 1 Then
                            lvChecado.Text = Checado
                            lvChecado.ForeColor = Drawing.Color.Green
                        Else
                            lvChecado.Text = Deschecado
                            lvChecado.ForeColor = Drawing.Color.Red
                        End If
                        idxColuna = idxColuna + 1
                        itmX.SubItems.Add(lvChecado)
                    End If
                Next i

                Me.Items.Add(itmX)

            Next posReg
            '################################################
            '#####  FIM FOR pra Preencher as linhas.   #####
            '################################################
        Catch ex As Exception
            LogaErro("Erro em Util::PreencheGridDS: " & ex.ToString())
        Finally
            Me.EndUpdate()
            Me.bAtualizando = False
        End Try

    End Sub

    Public Sub TransformaLink(ByVal nItem As Integer,
                              ByVal TextoLink As String,
                              Optional URL As String = "")

        Try

            _bControlaLink = True 'Opa! tem q controlar o mouse click e mouse move...
            _TextoLink = TextoLink
            _UrlLink = URL

            For i = 0 To Me.ObterTotalLinhas() - 1
                If Me.Items.Item(i).SubItems(nItem).Text.Length > 1 Then
                    If (Me.Items.Item(i).SubItems(nItem).Text.Substring(0, TextoLink.Length) = TextoLink) Then
                        Me.Items.Item(i).UseItemStyleForSubItems = False
                        Me.Items.Item(i).SubItems(nItem).ForeColor = Color.Blue
                        Me.Items.Item(i).SubItems(nItem).Font = New Font("Verdana", 9.0!, FontStyle.Underline)
                    End If
                End If
            Next
        Catch ex As Exception
            LogaErro("Erro em SuperLV::TransformaLink: " & ex.ToString())
        End Try

    End Sub

    Public Sub PreencheGridDS(ByVal rs1 As SuperDataSet,
                              ByVal chk_box As Boolean,
                              Optional ByVal BarraTitulo As Boolean = True,
                              Optional ByVal Contador As Boolean = False,
                              Optional ByVal Zebrado As Boolean = True,
                              Optional ByVal nTabela As Integer = 0)

        Dim idxColuna As Integer
        Dim clmX As ColumnHeader
        Dim itmX As ListViewItem
        Dim strTamanho As String
        Dim intTamanho As Integer
        Dim nContador As Long
        Dim strCampo As String
        Dim posReg As Integer
        Dim ValorCampo As Object

        Try

            Me.bAtualizando = True
            Me.BeginUpdate()

            If rs1 Is Nothing Then
                Me.Items.Clear()
                LogaErro("SuperLV::PreencheGridDS [" & Me.Name & "] - ATENÇÃO: RecordSet=Nothing, por favor, verifique.")
                Exit Sub
            End If

            LogaErro("SuperLV::PreencheGridDS [" & Me.Name & "] - N.Registros [" & rs1.TotalRegistros(nTabela).ToString & "]")
            _iChave = 0

            Me.View = View.Details                              'Define o modo de exibição do listview
            Me.LabelEdit = False                                'Permite o usuario editar o item
            Me.AllowColumnReorder = False                       'Permite o usuario rearranjar as colunas
            Me.CheckBoxes = chk_box                             'Exibe as caixas de marcacao (check boxes.)
            Me.FullRowSelect = True                             'Seleciona um item e subitem quando a seleção é feita
            Me.GridLines = True                                 'Exibe as linhas
            Me.Sorting = SortOrder.None                         'Ordena os itens na list na ordem ascendente
            Me.MultiSelect = False                              'Deixa selecionar celula individual
            Me.BackgroundImageTiled = True                      'Liga fundo com imagem
            Me.DoubleBuffered = True                            'Duplo Amanteigado

            Dim lvChecado As New ListViewItem.ListViewSubItem

            If BarraTitulo = True Then
                Me.HeaderStyle = ColumnHeaderStyle.Clickable
            Else
                Me.HeaderStyle = ColumnHeaderStyle.None
            End If

            _iChave = 0

            nContador = 0

            Me.Clear()

            Me.ListViewItemSorter = Nothing

            'Se tiver Coluna de contador, inclui...
            idxColuna = 0

            If Contador = True Then
                clmX = New ColumnHeader()
                clmX.Text = "Nº"
                clmX.Width = 45
                Me.Columns.Add(clmX)
                idxColuna = idxColuna + 1
            End If

            ''#############################################
            ''#####     FOR pra montar as colunas.    #####
            ''#############################################
            For i = 0 To rs1.TotalColunas(nTabela) - 1 'FieldCount() - 1

                strCampo = rs1.NomeColuna(i, nTabela)

                'procura por # e tenta pegar um numerico depois dele. Ex: as_coluna#123 <- pega 123
                strTamanho = strCampo.Substring(strCampo.IndexOf("#") + 1, strCampo.Length - strCampo.IndexOf("#") - 1)

                'se deu certo o comando anterior OK, senao faz calculo (tam * 10)
                If IsNumeric(strTamanho) = True Then
                    intTamanho = CInt(Val(strTamanho))
                Else
                    intTamanho = Len(strCampo) * 10
                End If

                'So entra no grid se for as_ , me_ , nu_
                If (strCampo.Substring(0, 3) = "as_") Or
                   (strCampo.Substring(0, 3) = "me_") Or
                   (strCampo.Substring(0, 3) = "nu_") Then

                    clmX = New ColumnHeader()
                    clmX.Text = Formata_Coluna(strCampo)
                    clmX.Width = intTamanho

                    'Se for numérico/moeda então alinha à direita
                    If strCampo.Substring(0, 3) = "me_" Or strCampo.Substring(0, 3) = "nu_" Then
                        clmX.TextAlign = HorizontalAlignment.Right
                    ElseIf (rs1.TipoDadosColuna(i, nTabela) Is GetType(Decimal)) Then
                        clmX.TextAlign = HorizontalAlignment.Right
                    ElseIf (rs1.TipoDadosColuna(i, nTabela) Is GetType(DateTime)) Then
                        clmX.TextAlign = HorizontalAlignment.Center
                    Else  'Se for alfanumérico então alinha à esquerda
                        clmX.TextAlign = HorizontalAlignment.Left
                    End If

                    Me.Columns.Add(clmX)

                ElseIf (strCampo.Substring(0, 3) = "id_") Then
                    clmX = New ColumnHeader()
                    clmX.Text = Formata_Coluna(strCampo, 1)
                    clmX.Width = 0
                    Me.Columns.Add(clmX)
                ElseIf (strCampo.Substring(0, 3) = "ck_") Then
                    clmX = New ColumnHeader()
                    clmX.Text = Formata_Coluna(strCampo)
                    clmX.Width = intTamanho
                    clmX.TextAlign = HorizontalAlignment.Center
                    Me.Columns.Add(clmX)
                End If

                'retorna em qual coluna esta o id_ (chave)
                If (strCampo.Substring(0, 3) = "as_") Or
                   (strCampo.Substring(0, 3) = "me_") Or
                   (strCampo.Substring(0, 3) = "id_") Or
                   (strCampo.Substring(0, 3) = "nu_") Or
                   (strCampo.Substring(0, 3) = "ck_") Then

                    If (strCampo.Substring(0, 3) = "id_") Then
                        'PreencheGridDS = idxColuna 'GUARDA A COLUNA QUE ESTA O ID DO REGISTRO 'tirar depois
                        _iChave = idxColuna
                    End If

                    idxColuna = idxColuna + 1 'aumenta o indice se entrar na grid ("id_" , "as_", "me_", "nu_", "ch_")

                End If
            Next i
            '#############################################
            '#####  FIM FOR pra montar as colunas.   #####
            '#############################################

            '################################################
            '#####     FOR pra Preencher as linhas.    #####
            '################################################
            'Do While rs1.Read
            For posReg = 0 To (rs1.TotalRegistros(nTabela) - 1) Step 1

                idxColuna = 0
                nContador = nContador + 1

                itmX = New ListViewItem()

                itmX.UseItemStyleForSubItems = False

                If Contador = True Then
                    itmX.Text = CStr(posReg + 1)
                    idxColuna = idxColuna + 1
                End If

                'i = indice da coluna (campo) do recordset
                For i = 0 To (rs1.TotalColunas(nTabela) - 1)

                    strCampo = rs1.NomeColuna(i, nTabela)
                    ValorCampo = rs1.ValorCampo(i, posReg, nTabela)

                    'se campo eh do tipo checkbox
                    If chk_box = True And idxColuna = 0 Then '
                        If IsDBNull(rs1.ValorCampo("chk", posReg, nTabela)) = True Then
                            If itmX.Checked = True Then
                                itmX.Checked = False
                            End If
                        Else
                            If CStr(rs1.ValorCampo("chk", posReg, nTabela)) = "" Then
                                If itmX.Checked = True Then
                                    itmX.Checked = False
                                End If
                            Else
                                If itmX.Checked = False Then
                                    itmX.Checked = True
                                End If
                            End If
                        End If
                    End If

                    If (strCampo.Substring(0, 3) = "as_") Then
                        If idxColuna = 0 Then
                            itmX.Text = ValorCampo.ToString
                        Else
                            itmX.SubItems.Add(ValorCampo.ToString)
                        End If
                        idxColuna = idxColuna + 1
                    ElseIf (strCampo.Substring(0, 3) = "me_") Then
                        If IsNumeric(ValorCampo.ToString) Then
                            If idxColuna = 0 Then
                                itmX.Text = CDec(ValorCampo.ToString).ToString("#,##0.00")
                            Else
                                itmX.SubItems.Add(CDec(ValorCampo.ToString).ToString("#,##0.00"))
                            End If
                        Else
                            If idxColuna = 0 Then
                                itmX.Text = ValorCampo.ToString
                            Else
                                itmX.SubItems.Add(ValorCampo.ToString)
                            End If
                        End If
                        idxColuna = idxColuna + 1
                    ElseIf (strCampo.Substring(0, 3) = "nu_") Then
                        If idxColuna = 0 Then
                            If IsNumeric(ValorCampo) Then
                                itmX.Text = ValorCampo.ToString()
                            Else
                                itmX.Text = ""
                            End If
                        Else
                            If IsNumeric(ValorCampo) Then
                                itmX.SubItems.Add(ValorCampo.ToString())
                            Else
                                itmX.SubItems.Add("")
                            End If
                        End If
                        idxColuna = idxColuna + 1
                    ElseIf (strCampo.Substring(0, 3) = "id_") Then
                        If idxColuna = 0 Then
                            itmX.Text = ValorCampo.ToString
                        Else
                            itmX.SubItems.Add(ValorCampo.ToString)
                        End If
                        idxColuna = idxColuna + 1
                    ElseIf (strCampo.Substring(0, 3) = "ck_") And idxColuna > 0 Then
                        lvChecado = New ListViewItem.ListViewSubItem
                        lvChecado.Font = New Drawing.Font("Wingdings", 14)
                        If IsDBNull(rs1.ValorCampo(i, posReg, nTabela)) = True Then
                            lvChecado.Text = Deschecado
                            lvChecado.ForeColor = Drawing.Color.Red
                        ElseIf CDec(rs1.ValorCampo(i, posReg, nTabela)) = 1 Then
                            lvChecado.Text = Checado
                            lvChecado.ForeColor = Drawing.Color.Green
                        Else
                            lvChecado.Text = Deschecado
                            lvChecado.ForeColor = Drawing.Color.Red
                        End If
                        idxColuna = idxColuna + 1
                        itmX.SubItems.Add(lvChecado)
                    End If
                Next i

                Me.Items.Add(itmX)

            Next posReg
            '################################################
            '#####  FIM FOR pra Preencher as linhas.   #####
            '################################################
        Catch ex As Exception
            LogaErro("Erro em Util::PreencheGridDS: " & ex.ToString())
        Finally
            Me.EndUpdate()
            Me.bAtualizando = False
        End Try

    End Sub

    Public Sub PreencheGridTudo(ByVal rs1 As SuperDataSet,
                                Optional ByVal Contador As Boolean = False)
        Try
            Dim idxColuna As Integer
            Dim clmX As ColumnHeader
            Dim itmX As ListViewItem
            Dim intTamanho As Integer
            Dim nContador As Long
            Dim strCampo As String
            Dim posReg As Integer
            Dim ValorCampo As Object

            Me.bAtualizando = True
            Me.BeginUpdate()

            If rs1 Is Nothing Then
                Me.Items.Clear()
                LogaErro("SuperLV::PreencheGridTudo [" & Me.Name & "] - ATENÇÃO: RecordSet=Nothing, por favor, verifique.")
                Exit Sub
            End If

            LogaErro("SuperLV::PreencheGridTudo [" & Me.Name & "] - N.Registros [" & rs1.TotalRegistros.ToString & "]")
            _iChave = 0

            Me.View = View.Details                              'Define o modo de exibição do listview
            Me.LabelEdit = False                                'Permite o usuario editar o item
            Me.AllowColumnReorder = False                       'Permite o usuario rearranjar as colunas
            Me.CheckBoxes = False                               'Exibe as caixas de marcacao (check boxes.)
            Me.FullRowSelect = True                             'Seleciona um item e subitem quando a seleção é feita
            Me.GridLines = True                                 'Exibe as linhas
            Me.Sorting = SortOrder.None                         'Ordena os itens na list na ordem ascendente
            Me.MultiSelect = False                              'Deixa selecionar celula individual
            Me.BackgroundImageTiled = True                      'Liga fundo com imagem
            Me.BackgroundImage = My.Resources.SuperLV           'Seleciona a imagem
            Me.DoubleBuffered = True                            'Duplo Amanteigado

            Me.HeaderStyle = ColumnHeaderStyle.Clickable

            _iChave = 0

            nContador = 0

            Me.Clear()

            Me.ListViewItemSorter = Nothing

            'Se tiver Coluna de contador, inclui...
            idxColuna = 0

            If Contador = True Then
                clmX = New ColumnHeader()
                clmX.Text = "Nº"
                clmX.Width = 45
                Me.Columns.Add(clmX)
                idxColuna = idxColuna + 1
            End If

            ''#############################################
            ''#####     FOR pra montar as colunas.    #####
            ''#############################################
            For i = 0 To rs1.TotalColunas() - 1 'FieldCount() - 1

                strCampo = rs1.NomeColuna(i)

                intTamanho = Len(strCampo) * 10

                clmX = New ColumnHeader()
                clmX.Text = strCampo
                clmX.Width = intTamanho

                If (rs1.TipoDadosColuna(i) Is GetType(Decimal)) Then
                    clmX.TextAlign = HorizontalAlignment.Right
                ElseIf (rs1.TipoDadosColuna(i) Is GetType(DateTime)) Then
                    clmX.TextAlign = HorizontalAlignment.Center
                Else  'Se for alfanumérico então alinha à esquerda
                    clmX.TextAlign = HorizontalAlignment.Left
                End If

                Me.Columns.Add(clmX)

            Next i
            '#############################################
            '#####  FIM FOR pra montar as colunas.   #####
            '#############################################

            '################################################
            '#####     FOR pra Preencher as colunas.    #####
            '################################################
            'Do While rs1.Read
            For posReg = 0 To (rs1.TotalRegistros() - 1) Step 1

                idxColuna = 0
                nContador = nContador + 1

                itmX = New ListViewItem()

                itmX.UseItemStyleForSubItems = False

                If Contador = True Then
                    itmX.Text = CStr(posReg + 1)
                    idxColuna = idxColuna + 1
                End If

                'i = indice da coluna (campo) do recordset
                For i = 0 To (rs1.TotalColunas() - 1)

                    strCampo = rs1.NomeColuna(i)
                    ValorCampo = rs1.ValorCampo(i, posReg)

                    If idxColuna = 0 Then
                        itmX.Text = ValorCampo.ToString
                    Else
                        itmX.SubItems.Add(ValorCampo.ToString)
                    End If
                    idxColuna = idxColuna + 1

                Next i

                Dim cor As Drawing.Color

                If posReg Mod 2 = 0 Then
                    cor = corGrid1
                Else
                    cor = corGrid2
                End If

                itmX.BackColor = cor
                For k = 0 To itmX.SubItems.Count - 1
                    itmX.SubItems(k).BackColor = cor
                Next k

                Me.Items.Add(itmX)

            Next posReg
            '################################################
            '#####  FIM FOR pra Preencher as colunas.   #####
            '################################################
        Catch ex As Exception
            LogaErro("Erro em Util::PreencheGridTudo: " & ex.ToString())
        Finally
            Me.EndUpdate()
            Me.bAtualizando = False
        End Try

    End Sub

    Public Function ObterCSVChaves(Optional ByVal separador As String = ";") As String
        Try
            Dim i As Integer
            Dim ret As String = ""

            For i = 0 To Me.Items.Count - 1
                If Me.Items(i).Checked = True Then
                    If _iChave = 0 Then 'se for da primeira coluna....
                        ret = ret & CStr(If(Len(ret) = 0, "", separador)) & Me.Items(i).Text
                    Else 'se for da segunda em diante...
                        ret = ret & CStr(If(Len(ret) = 0, "", separador)) & Me.Items(i).SubItems(_iChave).Text
                    End If
                End If
            Next i
            Return ret
        Catch ex As Exception
            LogaErro("Erro em " & Me.Name & "::obtemCSVChaves: " & CStr(ex.ToString()))
            Return ""
        End Try
    End Function

    Public Sub SelecionarTodos(ByVal bSelected As Boolean)
        Dim i As Integer
        Try
            Me.bAtualizando = True
            Me.BeginUpdate()
            For i = 0 To Me.Items.Count - 1
                If Me.Items(i).Checked <> bSelected Then
                    Me.Items(i).Checked = bSelected
                End If
            Next
            Me.bAtualizando = False
            Me.EndUpdate()
        Catch ex As Exception
            LogaErro("Erro em Util::SelecionarTodos: " & ex.ToString())
        End Try
    End Sub

    'se id <> 0, a coluna eh do tipo id_ (codigo)
    Private Function Formata_Coluna(ByVal sString As String,
                                    Optional ByVal id As Integer = 0) As String
        Try
            Dim nCerquilha As Integer
            Dim sstring2 As String

            If id = 0 Then
                sstring2 = Replace(xRight(sString, Len(sString) - 3), "_", " ")
            Else
                sstring2 = xRight(sString, Len(sString) - 3)
            End If

            nCerquilha = InStr(sstring2, "#")

            If nCerquilha > 0 Then
                sstring2 = xLeft(sstring2, nCerquilha - 1)
            End If

            Return sstring2
        Catch ex As Exception
            LogaErro("Erro em Util::Formata_Coluna: " & ex.ToString())
            Return ""
        End Try

    End Function

    Public Function xRight(ByVal s As String, ByVal n As Integer) As String
        If n > s.Length Then
            Return s
        ElseIf n < 1 Then
            Return ""
        Else
            Return s.Substring(s.Length - n, n)
        End If
    End Function

    Public Function xLeft(ByVal s As String, ByVal n As Integer) As String
        If n > s.Length Then
            Return s
        ElseIf n < 1 Then
            Return ""
        Else
            Return s.Substring(0, n)
        End If
    End Function

    Public Sub LimpaGrid()
        Me.Items.Clear()
    End Sub

    Public Function ObterColunaIndice() As Integer
        Return _iChave
    End Function

    Protected Overrides Sub OnColumnClick(e As ColumnClickEventArgs)

        If _bOrdena = True Then
            ' Get the new sorting column.
            m_NovaColunaOrdenada = Me.Columns(e.Column)
            ' Figure out the new sorting order.
            'Dim sort_order As System.Windows.Forms.SortOrder
            If m_ColunaOrdenada Is Nothing Then
                ' New column. Sort ascending.
                OrdemClassificacao = SortOrder.Ascending
            Else ' See if this is the same column.
                If m_NovaColunaOrdenada.Equals(m_ColunaOrdenada) Then
                    ' Same column. Switch the sort order.
                    If m_ColunaOrdenada.Text.StartsWith("> ") Then
                        OrdemClassificacao = SortOrder.Descending
                    Else
                        OrdemClassificacao = SortOrder.Ascending
                    End If
                Else
                    ' New column. Sort ascending.
                    OrdemClassificacao = SortOrder.Ascending
                End If
                ' Remove the old sort indicator.
                m_ColunaOrdenada.Text = m_ColunaOrdenada.Text.Substring(2)
            End If
            ' Display the new sort order.
            m_ColunaOrdenada = m_NovaColunaOrdenada
            If OrdemClassificacao = SortOrder.Ascending Then
                m_ColunaOrdenada.Text = "> " & m_ColunaOrdenada.Text
            Else
                m_ColunaOrdenada.Text = "< " & m_ColunaOrdenada.Text
            End If
            ' Create a comparer.
            Me.ListViewItemSorter = New clsListviewSorter(e.Column, OrdemClassificacao)
            ' Sort.
            Me.Sort()
        End If

        MyBase.OnColumnClick(e)

    End Sub

    Private Class clsListviewSorter ' Implements a comparer
        Implements IComparer

        Private m_ColumnNumber As Integer
        Private m_SortOrder As SortOrder

        Public Sub New(ByVal column_number As Integer, ByVal sort_order As SortOrder)
            m_ColumnNumber = column_number
            m_SortOrder = sort_order
        End Sub

        ' Compare the items in the appropriate column
        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare

            Dim item_x As ListViewItem = DirectCast(x, ListViewItem)
            Dim item_y As ListViewItem = DirectCast(y, ListViewItem)
            ' Get the sub-item values.
            Dim string_x As String
            If item_x.SubItems.Count <= m_ColumnNumber Then
                string_x = ""
            Else
                string_x = item_x.SubItems(m_ColumnNumber).Text
            End If
            Dim string_y As String
            If item_y.SubItems.Count <= m_ColumnNumber Then
                string_y = ""
            Else
                string_y = item_y.SubItems(m_ColumnNumber).Text
            End If
            ' Compare them.
            If m_SortOrder = SortOrder.Ascending Then
                If IsNumeric(string_x) And IsNumeric(string_y) Then
                    Return Val(string_x).CompareTo(Val(string_y))
                ElseIf IsDate(string_x) And IsDate(string_y) Then
                    Return DateTime.Parse(string_x).CompareTo(DateTime.Parse(string_y))
                Else
                    Return String.Compare(string_x, string_y)
                End If
            Else
                If IsNumeric(string_x) And IsNumeric(string_y) Then
                    Return Val(string_y).CompareTo(Val(string_x))
                ElseIf IsDate(string_x) And IsDate(string_y) Then
                    Return DateTime.Parse(string_y).CompareTo(DateTime.Parse(string_x))
                Else
                    Return String.Compare(string_y, string_x)
                End If
            End If
        End Function

    End Class

    Protected Overrides Sub Finalize()

        MyBase.Finalize()
        Lixeiro()

    End Sub

    Public Sub Lixeiro()
        GC.SuppressFinalize(Me)
        GC.Collect()
    End Sub

    Public Function ObterListaDecimal() As List(Of Decimal)

        Dim i As Integer
        Dim ret As List(Of Decimal)

        Try
            ret = New List(Of Decimal)

            For i = 0 To Me.Items.Count - 1
                If Me.Items(i).Checked = True Then
                    If _iChave = 0 Then 'se for da primeira coluna....
                        ret.Add(CDec(Me.Items(i).Text))
                    Else 'se for da segunda em diante...
                        ret.Add(CDec(Me.Items(i).SubItems(_iChave).Text))
                    End If
                End If
            Next i
            Return ret
        Catch ex As Exception
            LogaErro("Erro em " & Me.Name & "::obtemCSVChaves: " & CStr(ex.ToString()))
            Return Nothing
        End Try

    End Function

    Public Function ObterColunaChave() As Integer
        Return _iChave
    End Function

End Class