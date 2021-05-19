#Region "Legal"

'************************************************************************************************************************
' Copyright (c) 2015, Todos direitos reservados, GFT IT - http://www.GFTit.com.br/
'
' Autor........: Carlos Buosi (cbuosi@gmail.com)
' Arquivo......: VersaoArquivos.vb
' Tipo.........: Modulo VB.
' Versao.......: 2.02+
' Propósito....: Garantir que a versao dos arquivos batam no Bradesco <-> Cliente
' Uso..........: Não se aplica
' Produto......: ModuloCentralizador - Bradesco.
'
' Legal........: Este código é de propriedade do Banco Bradesco S/A e/ou GFT IT, sua cópia
'                e/ou distribuição é proibida.
'
' GUID.........: {52313876-0D4A-4DA9-81BF-7F3D985F4927}
' Observações..: nenhuma.
'
'************************************************************************************************************************

#End Region

Option Explicit On
Option Strict On

Public Class clsVersaoArquivos

    Public Shared Function ObterVersaoSuperMySQL() As String
        Return AppVersion()
    End Function

    Public Shared Function ObterVersaoArquivos() As String
        Return AppVersion()
    End Function

    Public Function ObterSenhaZip() As String
        Return "hunter2"
    End Function

    Public Function ObterSenhaSQLite() As String
        Return ""
    End Function

    Public Shared Function AppData() As String
        Return "Dezembro 2019"
    End Function

End Class