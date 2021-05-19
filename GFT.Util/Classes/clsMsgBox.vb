Option Strict On
Option Explicit On

Imports System.Drawing

Public Class clsMsgBox
    Enum eBotoes
        Ok = 0
        SimNao
        OkCancel
        SimNaoCancel
    End Enum

    Enum eImagens
        Nenhuma = 0
        Ok
        Info
        Cancel
        Interrogacao
        Atencao
    End Enum

    Enum eRet
        Ok = 0
        Sim
        Nao
        Cancel
        Erro
    End Enum


    Public Shared Function S_MsgBox(ByVal Mensagem As String,
                                    ByVal Botoes As eBotoes,
                                    Optional ByVal titulo As String = strTituloApp,
                                    Optional ByVal btnSelecionado As Integer = 1,
                                    Optional ByVal imagem As eImagens = eImagens.Nenhuma) As eRet




        Dim frm_msgbox As frm_msgbox

        Try



            Dim btnMeio = 214
            Dim btnEsq = 156
            Dim btnDir = 272
            Dim tamMsg As Integer
            Dim strFonte As String
            'Dim tamFonte As Single
            Dim tipoFonte As FontStyle
            Dim myfont As Font

            LogaErro("MessageBox: [" & Mensagem & _
                     "] Botoes: [" & Botoes.ToString & _
                     "] Titulo: [" & titulo & _
                     "] btnSelecionado: [" & btnSelecionado.ToString & _
                     "]  imagem: [" & imagem.ToString & "] ")

            frm_msgbox = New frm_msgbox

            frm_msgbox.btn1.Tag = 0
            frm_msgbox.btn2.Tag = 0
            frm_msgbox.btn3.Tag = 0

            Select Case btnSelecionado
                Case 1
                    frm_msgbox.btn1.Tag = 1
                Case 2
                    frm_msgbox.btn2.Tag = 1
                Case 3
                    frm_msgbox.btn3.Tag = 1
            End Select

            Select Case imagem
                Case eImagens.Cancel
                    frm_msgbox.imgMsgBox.Image = My.Resources.Resources.btnCancel
                Case eImagens.Info
                    frm_msgbox.imgMsgBox.Image = My.Resources.Resources.btnInfo
                Case eImagens.Ok
                    frm_msgbox.imgMsgBox.Image = My.Resources.Resources.btnOk
                Case eImagens.Interrogacao
                    frm_msgbox.imgMsgBox.Image = My.Resources.Resources.btnQuestion
                Case eImagens.Atencao
                    frm_msgbox.imgMsgBox.Image = My.Resources.Resources.btnWarning
                Case eImagens.Nenhuma
                    frm_msgbox.imgMsgBox.Image = Nothing
                Case Else
                    frm_msgbox.imgMsgBox.Image = Nothing
            End Select

            If imagem = eImagens.Nenhuma Then
                frm_msgbox.lblTexto.Location = New System.Drawing.Point(9, 51)
                frm_msgbox.lblTexto.Size = New System.Drawing.Size(488, 109)
                frm_msgbox.imgMsgBox.Visible = False
            Else
                frm_msgbox.lblTexto.Location = New System.Drawing.Point(67, 51)
                frm_msgbox.lblTexto.Size = New System.Drawing.Size(403, 109)
                frm_msgbox.imgMsgBox.Visible = True
            End If

            If (Botoes <> eBotoes.Ok) And _
               (Botoes <> eBotoes.SimNao) And _
               (Botoes <> eBotoes.OkCancel) And _
                (Botoes <> eBotoes.SimNaoCancel) Then
                MsgBox("Parametros de S_MsgBox invalidos. Segundo parametro deve ser:" & vbNewLine & _
                       "vbOKOnly ou clsMsgBox.eBotoes.SimNao ou vbOKCancel")
                S_MsgBox = eRet.Erro
                Exit Function
            End If

            frm_msgbox.btn1.Visible = False
            frm_msgbox.btn2.Visible = False
            frm_msgbox.btn3.Visible = False


            frm_msgbox.Tag = Botoes
            frm_msgbox.Text = titulo

            '8 10 14
            tamMsg = Len(Mensagem)
            strFonte = frm_msgbox.lblTexto.Font.Name
            'tamFonte = frm_msgbox.lblTexto.Font.Size
            tipoFonte = frm_msgbox.lblTexto.Font.Style

            If tamMsg <= 100 Then
                myfont = New Font(strFonte, 14, tipoFonte)
            ElseIf tamMsg > 100 And tamMsg < 200 Then
                myfont = New Font(strFonte, 10, tipoFonte)
            Else
                myfont = New Font(strFonte, 8, tipoFonte)
            End If

            frm_msgbox.lblTexto.Font = myfont

            'vb.net tranparent labels
            frm_msgbox.lblTexto.BringToFront()
            frm_msgbox.lblTexto.BackColor = Color.Transparent
            frm_msgbox.lblTexto.Text = Mensagem


            If Botoes = eBotoes.Ok Then
                frm_msgbox.btn1.Visible = True
                frm_msgbox.btn1.Left = btnMeio
                frm_msgbox.btn1.Text = strOk
            End If

            If Botoes = eBotoes.OkCancel Then
                frm_msgbox.btn1.Visible = True
                frm_msgbox.btn2.Visible = True
                frm_msgbox.btn1.Left = btnEsq
                frm_msgbox.btn2.Left = btnDir
                frm_msgbox.btn1.Text = strOk
                frm_msgbox.btn2.Text = strCancela
            End If

            If Botoes = eBotoes.SimNao Then
                frm_msgbox.btn1.Visible = True
                frm_msgbox.btn2.Visible = True
                frm_msgbox.btn1.Left = btnEsq
                frm_msgbox.btn2.Left = btnDir
                frm_msgbox.btn1.Text = strSim
                frm_msgbox.btn2.Text = strNao
            End If

            If Botoes = eBotoes.SimNaoCancel Then
                frm_msgbox.btn1.Visible = True
                frm_msgbox.btn2.Visible = True
                frm_msgbox.btn3.Visible = True
                frm_msgbox.btn1.Left = 130  'btnEsqn
                frm_msgbox.btn2.Left = 214  'btnMeio
                frm_msgbox.btn3.Left = 298  'btnDir
                frm_msgbox.btn1.Text = strSim
                frm_msgbox.btn2.Text = strNao
                frm_msgbox.btn3.Text = strCancela
            End If

            frm_msgbox.Opacity = 100
            frm_msgbox.ShowDialog()
            frm_msgbox.Dispose()

            Return vMsgBox

        Catch ex As Exception
            LogaErro("Erro em Util::S_MsgBox: " & CStr(ex.ToString()))
            Return eRet.Erro
        Finally
            frm_msgbox = Nothing
        End Try

    End Function

End Class
