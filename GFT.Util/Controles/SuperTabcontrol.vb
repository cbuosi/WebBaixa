Option Explicit On
Option Strict On

Imports System.Drawing

Public Class SuperTabControl
    Inherits TabControl

    Private AbaAtual As Integer = 0 'Global vergonhosa

    Public Sub HabilitaAba(ByVal nomeAba As String,
                           ByVal bHab As Boolean,
                           Optional bInverte As Boolean = False)
        Try

            If bInverte = True Then
                Me.TabPages(nomeAba).Enabled = Not Me.TabPages(nomeAba).Enabled
            Else
                Me.TabPages(nomeAba).Enabled = bHab
            End If

        Catch ex As Exception
            LogaErro("Erro em [SuperTabControl.HabilitaAba(1)] : " & ex.Message)
        End Try
    End Sub

    Public Sub HabilitaAba(ByVal idx As Integer,
                           ByVal bHab As Boolean,
                           Optional bInverte As Boolean = False)

        Try
            If bInverte = True Then
                Me.TabPages(idx).Enabled = Not Me.TabPages(idx).Enabled
            Else
                Me.TabPages(idx).Enabled = bHab
            End If

        Catch ex As Exception
            LogaErro("Erro em [SuperTabControl.HabilitaAba(2)] : " & ex.Message)
        End Try

    End Sub

    'Evento sempre chamado antes do OnSelectedIndexChanged
    Protected Overrides Sub OnDeselected(e As TabControlEventArgs)

        Try
            AbaAtual = e.TabPageIndex 'Guarda a aba que estavamos (desselecionou...)
            MyBase.OnDeselected(e)
        Catch ex As Exception
            LogaErro("Erro em [SuperTabControl.OnDeselected] : " & ex.Message)
        End Try

    End Sub

    Protected Overrides Sub OnSelectedIndexChanged(e As EventArgs)

        Try

            If AbaAtual >= 0 And AbaAtual <= Me.TabPages.Count Then
                If Me.TabPages(AbaAtual).Enabled = False And _
                    Me.TabPages(Me.SelectedIndex).Enabled = False Then 'nao tem pra onde voltar! 
                    Exit Sub
                End If

                If Me.TabPages(Me.SelectedIndex).Enabled = False Then 'Opa, aba de destino desabilitada...
                    Me.SelectedTab = Me.TabPages(AbaAtual) 'Volta pra onde estava...
                    Exit Sub
                End If

                MyBase.OnSelectedIndexChanged(e)
            End If

        Catch ex As Exception
            LogaErro("Erro em [SuperTabControl.OnSelectedIndexChanged] : " & ex.Message)
        End Try

    End Sub


    Protected Overrides Sub OnDrawItem(ByVal e As System.Windows.Forms.DrawItemEventArgs)

        'Firstly we'll define some parameters.
        Dim CurrentTab As TabPage = Me.TabPages(e.Index)
        Dim ItemRect As Rectangle = Me.GetTabRect(e.Index)
        Dim FillBrush As New SolidBrush(Color.LightGray)
        Dim TextBrush As New SolidBrush(Color.Black)
        Dim sf As New StringFormat
        sf.Alignment = StringAlignment.Center
        sf.LineAlignment = StringAlignment.Center

        If CBool(e.State And DrawItemState.Selected) Then
            FillBrush.Color = Color.White
            TextBrush.Color = Color.Black
            ItemRect.Inflate(2, 2)
        End If

        e.Graphics.FillRectangle(FillBrush, ItemRect)
        e.Graphics.DrawString(CurrentTab.Text, e.Font, TextBrush, RectangleF.op_Implicit(ItemRect), sf)
        e.Graphics.ResetTransform()
        FillBrush.Dispose()
        TextBrush.Dispose()

        MyBase.OnDrawItem(e)

    End Sub


    Protected Overrides Sub Finalize()

        MyBase.Finalize()
        Lixeiro()

    End Sub

    Public Sub Lixeiro()
        GC.SuppressFinalize(Me)
        GC.Collect()
    End Sub


End Class
