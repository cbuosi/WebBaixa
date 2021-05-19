#Region "Legal"
'************************************************************************************************************************
' Copyright (c) 2015, Todos direitos reservados, GFT-IT - Serviços de TI - http://www.GFTit.com.br/
'
' Autor........: Carlos Buosi (cbuosi@gmail.com)
' Arquivo......: SuperDatePicker.vb
' Tipo.........: Modulo VB.
' Versao.......: 2.02+
' Propósito....: Modulo de DatePicker
' Uso..........: Não se aplica
' Produto......: GerCor
'
' Legal........: Este código é de propriedade do Banco Bradesco S/A e/ou GFT-IT - Serviços de TI, sua cópia
'                e/ou distribuição é proibida.
'
' GUID.........: {1CC82C98-9E60-1298-9681-7102635D1782}
' Observações..: nenhuma.
'
'************************************************************************************************************************
#End Region
Option Explicit On
Option Strict On

Imports System.ComponentModel
Imports System.Drawing

Public Class SuperDatePicker
    Inherits DateTimePicker

    Private bAlterado As Boolean = False

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

            Me.BackColor = corObjNaoSelecionado
            Me.Invalidate()

            bAlterado = False

        Catch ex As Exception
            LogaErro("Erro em SuperTextBox::New: " & ex.Message)
        End Try
    End Sub

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
            Me.BackColor = corObjSelecionado
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

    Protected Overrides Sub Finalize()

        MyBase.Finalize()
        Lixeiro()

    End Sub

    Public Sub Lixeiro()
        GC.SuppressFinalize(Me)
        GC.Collect()
    End Sub

    Const WM_ERASEBKGND As Integer = &H14

    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        If m.Msg = WM_ERASEBKGND Then
            Dim g As Graphics = Graphics.FromHdc(m.WParam)
            g.FillRectangle(New SolidBrush(Me.BackColor), ClientRectangle)
            g.Dispose()
            Return
        End If

        MyBase.WndProc(m)
    End Sub

End Class
