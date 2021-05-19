#Region "Legal"

'************************************************************************************************************************
' Copyright (c) 2015, Todos direitos reservados, GFT-IT - Serviços de TI - http://www.GFTit.com.br/
'
' Autor........: Carlos Buosi (cbuosi@gmail.com)
' Arquivo......: SuperComboBox.vb
' Tipo.........: Modulo VB.
' Versao.......: 2.02+
' Propósito....: Modulo de ComboBox
' Uso..........: Não se aplica
' Produto......: GerCor
'
' Legal........: Este código é de propriedade do Banco Bradesco S/A e/ou GFT-IT - Serviços de TI, sua cópia
'                e/ou distribuição é proibida.
'
' GUID.........: {7CC82C98-9E60-4498-9681-7102635D1782}
' Observações..: nenhuma.
'
'************************************************************************************************************************

#End Region

Option Explicit On
Option Strict On

Imports System.ComponentModel
Imports GFT.Util.clsMsgBox
Imports System.Reflection

Public Class SuperComboBox
    Inherits ComboBox

    Private Obrigatorio As Boolean = False
    Private txtObrigatorio As String = ""
    Private bAlterado As Boolean = False
    Dim oErrorProvider As ErrorProvider

    Enum PrimeiroValor
        Nada = 1
        Todos
        Selecione
        Nenhum
    End Enum

    <Category("SuperComboBox"), Description("")>
    Public Property SuperObrigatorio As Boolean
        Get
            Return Obrigatorio
        End Get
        Set(ByVal value As Boolean)
            Obrigatorio = value
            Me.Invalidate()
        End Set
    End Property

    <Category("SuperComboBox"), Description("")>
    Public Property SuperTxtObrigatorio As String
        Get
            Return txtObrigatorio
        End Get
        Set(ByVal value As String)
            txtObrigatorio = value
            Me.Invalidate()
        End Set
    End Property

    <Category("SuperComboBox"), Description("Retorna false se o conteúdo do componente nao foi alterado")>
    Public Property Alterado As Boolean
        Get
            Return bAlterado
        End Get
        Set(ByVal value As Boolean)
            bAlterado = value
            Me.Invalidate()
        End Set
    End Property

    Public Sub New()
        Try

            'Instancia o errorProvider
            oErrorProvider = New ErrorProvider()
            'Binda o errorProvider com o objeto texto...
            oErrorProvider.ContainerControl = CType(Me.Container, ContainerControl)
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::New: " & ex.Message)
        End Try
    End Sub

    Public Const strTODOS As String = ":: Todos ::"
    Public Const strSELECT As String = ":: Selecione ::"
    Public Const strNENHUM As String = ":: Nenhum ::"

    Public Structure DuplaCombo
        Dim chave As Object
        Dim descricao As String

        Public Sub New(ByVal _chave As Object, ByVal _descricao As String)
            chave = _chave
            descricao = _descricao
        End Sub

        Public Overrides Function ToString() As String
            Return Me.descricao
        End Function

        Public Shared Function Coll(ByRef oColl As Object) As DuplaCombo
            Return New DuplaCombo(CDec(oColl), CDec(oColl) & " - " & oColl.ToString)
        End Function

    End Structure

    Protected Overrides Sub OnKeyDown(ByVal e As System.Windows.Forms.KeyEventArgs)
        Try

            If e.KeyCode = Keys.Back Then
                SendKeys.Send("+{TAB}")
            End If

            If e.KeyCode = Keys.Return Then
                SendKeys.Send("{TAB}")
            End If
            MyBase.OnKeyDown(e)
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::OnKeyDown: " & ex.Message)
        End Try
    End Sub

    Protected Overrides Sub OnGotFocus(ByVal e As System.EventArgs)
        Try
            Me.SelectAll()
            Me.BackColor = corObjSelecionado

            If oErrorProvider.GetError(Me) <> "" Then
                Treme()
            End If

            MyBase.OnGotFocus(e)
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::OnGotFocus: " & ex.Message)
        End Try
    End Sub

    Protected Overrides Sub OnLostFocus(ByVal e As System.EventArgs)
        Try
            Me.BackColor = corObjNaoSelecionado
            MyBase.OnLostFocus(e)
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::OnLostFocus: " & ex.Message)
        End Try
    End Sub

    Public Sub ResetaAvisos(Optional ByVal bLimpaCampo As Boolean = False)
        Try
            'Resetou avisos, nao esta mais alterado!
            If oErrorProvider.GetError(Me) <> "" Then
                oErrorProvider.SetError(Me, "")
            End If

            Me.Alterado = False

            If bLimpaCampo = True Then
                Me.SelectedIndex = 0
            End If
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::ResetaAvisos: " & ex.Message)
        End Try
    End Sub

    Public Function VerificaObrigatorio(Optional ByVal bZerado As Boolean = False,
                                        Optional ByVal vlrMin As Decimal = Nothing,
                                        Optional ByVal vlrMax As Decimal = Nothing) As Boolean

        Try
            'Nao eh obrigatorio, retorna ok
            If Obrigatorio = False Then
                Return True
            End If

            Dim chaveCombo As String = Me.ObterChaveCombo()
            'If x(0) = "1" Then          'se estiver marcado com '1' (obrigatorio)
            If chaveCombo = "0" Or chaveCombo = "" Or chaveCombo = "-1" Then 'Vazio ou Todos...
                oErrorProvider.SetError(Me, "O campo '" & txtObrigatorio & "' é obrigatório.")
                S_MsgBox("O campo '" & Me.SuperTxtObrigatorio & "' é obrigatório.", eBotoes.Ok, , , eImagens.Atencao)
                Me.Focus()
                Return False
                Exit Function
            End If

            Return True
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::VerificaObrigatorio: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function PreencheComboDS(ByRef rs As SuperDataSet,
                                    ByVal nCampoString As String,
                                    ByVal ncampoChave As String,
                                    Optional ByVal pValor As PrimeiroValor = PrimeiroValor.Selecione) As Boolean

        Try

            Dim i As Integer
            Dim idx As Integer
            Dim chave As Object
            Dim descricao As String
            Dim valorZerado As Object

            If rs Is Nothing Then
                LogaErro("Preenhendo ComboBox [" & Me.Name & "]")
            Else
                LogaErro("Preenhendo ComboBox [" & Me.Name & "] - N.Registros [" & rs.TotalRegistros.ToString & "]")
            End If

            Me.DropDownStyle = ComboBoxStyle.DropDownList
            Me.Items.Clear()

            'Prepara campos Todos/Selecione
            If pValor = PrimeiroValor.Selecione Or
               pValor = PrimeiroValor.Todos Or
               pValor = PrimeiroValor.Nenhum Then

                If rs Is Nothing Then
                    valorZerado = -1D
                    'Dependendo do tipo (System.Type) do campo, preenche com 0 ou ""
                ElseIf (rs.TipoDadosColuna(ncampoChave) Is GetType(Decimal)) Then
                    valorZerado = -1D
                Else
                    valorZerado = ""
                End If

                Select Case pValor
                    Case PrimeiroValor.Nada
                        'Não adicionada nada a lista.
                    Case PrimeiroValor.Selecione
                        Me.Items.Add(New DuplaCombo(valorZerado, strSELECT))
                    Case PrimeiroValor.Todos
                        Me.Items.Add(New DuplaCombo(valorZerado, strTODOS))
                    Case PrimeiroValor.Nenhum
                        Me.Items.Add(New DuplaCombo(valorZerado, strNENHUM))
                End Select

            End If

            If Not rs Is Nothing Then
                For i = 0 To rs.TotalRegistros() - 1 Step 1
                    If IsNumeric(ncampoChave) Then
                        idx = CInt(Val(ncampoChave))
                        chave = rs.ValorCampo(idx, i)
                    Else
                        chave = rs.ValorCampo(ncampoChave, i)
                    End If

                    If IsNumeric(nCampoString) Then
                        idx = CInt(Val(nCampoString))
                        descricao = CStr(rs.ValorCampo(idx, i))
                    Else
                        descricao = CStr(rs.ValorCampo(nCampoString, i))
                    End If
                    Me.Items.Add(New DuplaCombo(chave, descricao))
                Next i
            End If

            Me.SelectedIndex = 0
            Me.Alterado = False

            Return True
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::PreencheComboDS: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function PreencheComboColl(ByRef col As Collection,
                                      Optional ByVal pValor As PrimeiroValor = PrimeiroValor.Nada) As Boolean

        Try

            Me.DropDownStyle = ComboBoxStyle.DropDownList
            Me.Items.Clear()

            'Prepara campos Todos/Selecione
            Select Case pValor
                Case PrimeiroValor.Nada
                    'Não adicionada nada a lista.
                Case PrimeiroValor.Selecione
                    Me.Items.Add(New DuplaCombo(CDec(-1), strSELECT))
                Case PrimeiroValor.Todos
                    Me.Items.Add(New DuplaCombo(CDec(-1), strTODOS))
                Case PrimeiroValor.Nenhum
                    Me.Items.Add(New DuplaCombo(CDec(-1), strNENHUM))
            End Select

            For i = 1 To col.Count Step 1
                Me.Items.Add(CType(col.Item(i), DuplaCombo))
            Next i

            Me.SelectedIndex = 0
            Me.Alterado = False

            Return True
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::PreencheComboColl: " & ex.Message)
            Return False
        End Try
    End Function

    'Private Function GetKey(Col As Collection, Index As Integer) As String
    '    Dim flg As BindingFlags = BindingFlags.Instance Or BindingFlags.NonPublic
    '    Dim InternalList As Object = Col.GetType.GetMethod("InternalItemsList", flg).Invoke(Col, Nothing)
    '    Dim Item As Object = InternalList.GetType.GetProperty("Item", flg).GetValue(InternalList, {Index - 1})
    '    Dim Key As String = Item.GetType.GetField("m_Key", flg).GetValue(Item).ToString
    '    Return Key
    'End Function

    <System.Diagnostics.DebuggerStepThrough()> Public Function ObterChaveCombo() As String
        Dim sel As DuplaCombo = Nothing
        Try
            If Me.SelectedIndex = -1 Then
                Return "0"
            Else
                sel = CType(Me.SelectedItem, DuplaCombo)
                Return CStr(sel.chave)
            End If
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::ObterChaveCombo: " & ex.Message)
            Return "0"
        End Try
    End Function

    Public Function ObterDescricaoCombo() As String
        Dim sel As DuplaCombo = Nothing
        Try
            If Me.SelectedIndex = -1 Then
                Return ""
            Else
                sel = CType(Me.SelectedItem, DuplaCombo)
                Return CStr(sel.descricao)
            End If
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::ObterDescicaoCombo: " & ex.Message)
            Return ""
        End Try
    End Function

    Public Sub PosicionaRegistroCombo(ByVal idx As Object)
        Try
            Dim i As Integer
            i = 0

            If IsDBNull(idx) = True Then
                Me.SelectedIndex = -1
                Exit Sub 'Se posicionou, sai da funcao...
            End If

            For i = 0 To (Me.Items.Count - 1)
                Dim sel As DuplaCombo = CType(Me.Items(i), DuplaCombo)
                'Tomar cuidado para o tipo de dados ser igual (na chave) pois pode dar erro oculto (nao posicionar corretamente): ex: cint(4) <> cdec(4)
                If sel.chave.GetType.ToString() = idx.GetType.ToString() Then
                    If sel.chave.ToString = idx.ToString Then
                        Me.SelectedIndex = i
                        Exit Sub 'Se posicionou, sai da funcao...
                    End If
                End If
            Next i
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::PosicionaRegistroCombo: " & ex.Message)
        End Try

    End Sub

    Protected Overrides Sub OnSelectedIndexChanged(ByVal e As System.EventArgs)
        Try
            'Alterado!
            bAlterado = True

            'Se estiver sinalizando algum erro, limpa
            If oErrorProvider.GetError(Me) <> "" Then
                oErrorProvider.SetError(Me, "")
            End If

            MyBase.OnSelectedIndexChanged(e)
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::OnSelectedIndexChanged: " & ex.Message)
        End Try
    End Sub

    Sub Adiciona(ByVal chave As Object,
                 ByVal valor As String)
        Try
            Me.Items.Add(New DuplaCombo(chave, valor))
        Catch ex As Exception
            LogaErro("Erro em SuperComboBox::Adiciona: " & ex.Message)
        End Try
    End Sub

    Sub Limpa()
        Try
            Me.DropDownStyle = ComboBoxStyle.DropDownList
            Me.Items.Clear()
            Me.Alterado = False
        Catch ex As Exception
        End Try
    End Sub

    Protected Overrides Sub Finalize()

        If Not oErrorProvider Is Nothing Then
            oErrorProvider.Dispose()
        End If
        oErrorProvider = Nothing
        Me.Items.Clear()

        MyBase.Finalize()

        Lixeiro()

    End Sub

    Public Sub Lixeiro()
        GC.SuppressFinalize(Me)
        GC.Collect()
    End Sub

    Private Sub Treme()

        Dim Posicao As Drawing.Point = Me.Location
        Dim Deslocamento As Integer = 4
        Dim Tempo As Integer = 30
        Dim Qtd As Integer = 2

        For i = 1 To Qtd

            Posicao.X += Deslocamento
            Me.Location = Posicao
            Me.Refresh()
            System.Threading.Thread.Sleep(Tempo)

            Posicao.X -= Deslocamento
            Me.Location = Posicao
            Me.Refresh()
            System.Threading.Thread.Sleep(Tempo)

        Next i

    End Sub

End Class