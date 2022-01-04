Option Explicit On
Option Strict On
#Region "Legal"

' Copyright (c) 2013, Todos direitos reservados, Sonda-IT, Tecnologia da Informação - http://www.sondait.com.br/        ' 
'                                                                                                                       '
' Autor........: Equipe de Desenvolvimento Peculiar - Sonda-IT                                                          '
' Arquivo......: frm_Sobre.vb                                                                                           '
' Tipo.........: Formulário                                                                                             '   
' Versão.......:                                                                                                        '
' Propósito....: Tela de "Sobre" do sistema                                                                             '
' Uso..........: Não se aplica                                                                                          '
' Produto......: Conciliação Contábil                                                                                   '
'                                                                                                                       '
' Legal........: Este código é de propriedade do Banco Bradesco S/A e/ou Sonda Tecnologia da Informação, sua cópia      '   
'                e/ou distribuição é proibida.                                                                          '
'                                                                                                                       '
' Observações..: Gerado por Programadores Peculiares                                                                    '

#End Region
Imports System.IO

Public Class frmSobre
    Private Sub frmSobre_Load(sender As Object, e As EventArgs) Handles MyBase.Load


        Dim caminhovideo1 = Path.Combine(Application.StartupPath, "mosca.mp4")

        If (Not File.Exists(caminhovideo1)) Then
            File.WriteAllBytes(caminhovideo1, My.Resources.mosca_v)
        End If

        WMP.URL = caminhovideo1 '"C:\Users\CBuosi\Desktop\mosca.mp4"

        WMP.settings.setMode("loop", True)

        WMP.stretchToFit = True
        WMP.uiMode = "None"
        WMP.Ctlcontrols.play()
    End Sub

    Private Sub btnVoltar_Click(sender As Object, e As EventArgs) Handles btnVoltar.Click

        WMP.Ctlcontrols.stop()
        Me.Close()

    End Sub
End Class