Option Explicit On
Option Strict On

Imports GFT.Util.clsMsgBox
Imports System.ComponentModel
Imports System.Drawing

Public Class SuperMaskedBox
    Inherits MaskedTextBox

    Private txtObrigatorio As String = ""
    Private Obrigatorio As Boolean = False
    Private bAlterado As Boolean = False
    Private corDesabilitado As Color = Nothing
    Private bMudaAuto As Boolean = True

    Dim oErrorProvider As ErrorProvider

    Public Sub ResetaAvisos(Optional ByVal bLimpaCampos As Boolean = False)
        Try
            If oErrorProvider.GetError(Me) <> "" Then
                oErrorProvider.SetError(Me, "")
            End If

            If bLimpaCampos = True AndAlso Me.Text <> "" Then
                Me.Text = ""
            End If

        Catch ex As Exception
            LogaErro("Erro em SuperMaskedBox::ResetaAvisos: " & ex.Message)
        End Try
    End Sub


    Public Function VerificaObrigatorio(Optional ByVal bZerado As Boolean = False) As Boolean

        Try

            'Nao eh obrigatorio, retorna ok
            If Obrigatorio = False Then
                Return True
            End If

            If (Me.Text.Trim) = "" Then       've se tem texto
                oErrorProvider.SetError(Me, "O campo '" & txtObrigatorio & "' é obrigatório.")
                S_MsgBox("O campo '" & txtObrigatorio & "' é obrigatório.", eBotoes.Ok, , , eImagens.Atencao)
                Me.Focus()
                Return False
            End If

            If (IsNumeric(Me.Text) = True) Then
                If bZerado = True AndAlso CDec(Me.Text) = 0 Then
                    oErrorProvider.SetError(Me, "O campo '" & txtObrigatorio & "' não pode ser zero.")
                    S_MsgBox("O campo '" & txtObrigatorio & "' não pode ser zero.", eBotoes.Ok, , , eImagens.Atencao)
                    Me.Focus()
                    Return False
                End If
            End If

            Return True

        Catch ex As Exception
            LogaErro("Erro em SuperMaskedBox::VerificaObrigatorio: " & ex.Message)
            Return False
        End Try

    End Function


    <Category("SuperMaskedBox"), Description("Retorna false se o conteúdo do componente nao foi alterado")> _
    Public Property Alterado As Boolean
        Get
            Return bAlterado
        End Get
        Set(ByVal value As Boolean)
            bAlterado = value
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnGotFocus(ByVal e As System.EventArgs)
        Try

            If Len(Me.Text) <> 0 Then
                Me.SelectAll()
            End If

            Me.BackColor = corObjSelecionado

            If oErrorProvider.GetError(Me) <> "" Then
                Treme()
            End If

            MyBase.OnGotFocus(e)
        Catch ex As Exception
            LogaErro("Erro em SuperMaskedBox::OnGotFocus: " & ex.Message)
        End Try
    End Sub

    Protected Overrides Sub OnLostFocus(ByVal e As System.EventArgs)
        Try
            Me.BackColor = corObjNaoSelecionado
            MyBase.OnLostFocus(e)
        Catch ex As Exception
            LogaErro("Erro em SuperMaskedBox::OnLostFocus: " & ex.Message)
        End Try
    End Sub

    Public Sub SetarTextoAnimado(ByVal strValorTexto As String)
        Try

            If Me.TextMaskFormat <> MaskFormat.ExcludePromptAndLiterals Then
                Me.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals
            End If

            Me.Text = ""
            For i = 1 To strValorTexto.Length Step 1
                Me.Text = Me.Text & Mid(strValorTexto, i, 1)
                Threading.Thread.Sleep(40)
                Application.DoEvents()
            Next i

        Catch ex As Exception
            LogaErro("Erro em SuperMaskedBox::SetarTextoAnimado: " & ex.Message)
        End Try
    End Sub

    Protected Overrides Sub OnKeyDown(ByVal e As System.Windows.Forms.KeyEventArgs)
        Try

            If (e.KeyCode = Keys.Delete) Then
                bAlterado = True
            End If

            If bMudaAuto = True And _
               Me.Multiline = False Then

                If (e.KeyCode = Keys.Back And Me.Text = "") Or _
                    e.KeyCode = Keys.Up Then
                    SendKeys.Send("+{TAB}")
                End If

                If e.KeyCode = Keys.Down Or _
                    e.KeyCode = Keys.Return Then
                    SendKeys.Send("{TAB}")
                End If

            End If

            If e.KeyCode = Keys.Return And
               Me.Multiline = False Then
                e.SuppressKeyPress = True
            End If

            MyBase.OnKeyDown(e)

        Catch ex As Exception
            LogaErro("Erro em SuperMaskedBox::OnKeyDown: " & ex.Message)
        End Try
    End Sub

    Protected Overrides Sub OnTextChanged(ByVal e As System.EventArgs)
        Try

            'Se estiver sinalizando algum erro, limpa
            Me.ResetaAvisos()

            MyBase.OnTextChanged(e)

        Catch ex As Exception
            LogaErro("Erro em SuperMaskedBox::OnTextChanged: " & ex.Message)
        End Try
    End Sub



    Protected Overrides Sub OnKeyPress(ByVal e As System.Windows.Forms.KeyPressEventArgs)
        Try

            bAlterado = True

            'Se estiver sinalizando algum erro, limpa
            If oErrorProvider.GetError(Me) <> "" Then
                oErrorProvider.SetError(Me, "")
            End If

            MyBase.OnKeyPress(e)

        Catch ex As Exception
            LogaErro("Erro em SuperMaskedBox::OnKeyPress: " & ex.Message)
        End Try

    End Sub

    <Category("SuperMaskedBox"), Description("")> _
    Public Property SuperObrigatorio As Boolean
        Get
            Return Obrigatorio
        End Get
        Set(ByVal value As Boolean)
            Obrigatorio = value
            Me.Invalidate()
        End Set
    End Property

    <Category("SuperMaskedBox"), Description("")> _
    Public Property SuperTxtObrigatorio As String
        Get
            Return txtObrigatorio
        End Get
        Set(ByVal value As String)
            txtObrigatorio = value
            Me.Invalidate()
        End Set
    End Property

    <Category("SuperMaskedBox"), Description("")> _
    Public Property SuperTxtCorDesabilitado As Color
        Get
            Return corDesabilitado
        End Get
        Set(ByVal value As Color)
            corDesabilitado = value
            Me.Invalidate()
        End Set
    End Property


    Public Sub New()
        Try

            oErrorProvider = New System.Windows.Forms.ErrorProvider() 'Instancia o errorProvider
            oErrorProvider.ContainerControl = CType(Me.Container, ContainerControl) 'Binda o errorProvider com o objeto texto...

            Me.BackColor = corObjNaoSelecionado
            Me.Invalidate()

            bAlterado = False

        Catch ex As Exception
            LogaErro("Erro em SuperMaskedBox::New: " & ex.Message)
        End Try
    End Sub

    Protected Overrides Sub Finalize()

        If Not oErrorProvider Is Nothing Then
            oErrorProvider.Dispose()
        End If
        oErrorProvider = Nothing

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

