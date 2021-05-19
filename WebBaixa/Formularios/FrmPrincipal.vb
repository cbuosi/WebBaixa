Option Explicit On
Option Strict On
Imports System.IO
Imports GFT.Util
Imports Newtonsoft.Json
Imports WebBaixa.DadosFichas
Imports WebBaixa.LogChrome

Public Class FrmPrincipal

    Dim strArquivo As String = "C:\Users\Carlos Buosi\Desktop\Dados3.html"
    Dim strDados As String

    Dim TOT_NAV As Integer = 0

    Structure sRegistro
        Dim ID As Integer
        Dim DATA As String
        Dim NCM As String
        Dim INCOTERM As String
        Dim MODAL As String
        Dim PRODUCT_DESCRIPTION As String
        Dim LIKELY_EXPORTER As String
        Dim LIKELY_IMPORTER As String
        'Dim LIKELY_NOTIFIED As String
        Dim FOB_UNIT As String
        Dim FOB_TOT As String
        Dim CIF_UNIT As String
        Dim CIF_TOT As String
        Dim MARKETED_QTY As String
        Dim NET_WEIGHT As String
        Dim ACQUISITION_COUNTRY As String
        Dim ORIGIN_COUNTRY As String
        Dim CLEARANCE_BRANCH As String

        Sub New(ByVal _ID As Integer)
            ID = _ID
            DATA = ""
            NCM = ""
            INCOTERM = ""
            MODAL = ""
            PRODUCT_DESCRIPTION = ""
            LIKELY_EXPORTER = ""
            LIKELY_IMPORTER = ""
            'LIKELY_NOTIFIED = ""

            FOB_UNIT = ""
            FOB_TOT = ""
            CIF_UNIT = ""
            CIF_TOT = ""

            MARKETED_QTY = ""
            NET_WEIGHT = ""
            ACQUISITION_COUNTRY = ""
            ORIGIN_COUNTRY = ""
            CLEARANCE_BRANCH = ""
        End Sub


    End Structure




    Private Sub DP1(ByVal strLog As String)
        Diagnostics.Debug.Print(strLog)

        'txtLog.Text = txtLog.Text & strLog & vbNewLine

        txtLog.AppendText(strLog & vbNewLine)
        txtLog.Select(txtLog.TextLength, 0)
        txtLog.ScrollToCaret()


        Application.DoEvents()
    End Sub

    'Private Sub oWB_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs)
    '    TOT_NAV -= 1
    '    DP1(TOT_NAV.ToString & " - oWB_DocumentCompleted:" & e.Url.ToString)
    'End Sub
    '
    'Private Sub oWB_Navigating(sender As Object, e As WebBrowserNavigatingEventArgs)
    '    TOT_NAV += 1
    '    DP1(TOT_NAV.ToString & " - oWB_Navigating:" & e.Url.ToString)
    'End Sub
    '
    'Private Sub oWB_LocationChanged(sender As Object, e As EventArgs)
    '    DP1(TOT_NAV.ToString & " - oWB_LocationChanged:")
    'End Sub
    '
    'Private Sub oWB_CausesValidationChanged(sender As Object, e As EventArgs)
    '    DP1("oWB_CausesValidationChanged")
    'End Sub
    '
    'Private Sub oWB_ClientSizeChanged(sender As Object, e As EventArgs)
    '    DP1("oWB_ClientSizeChanged")
    'End Sub
    '
    'Private Sub oWB_Navigated(sender As Object, e As WebBrowserNavigatedEventArgs)
    '    DP1("oWB_Navigated: " & e.Url.ToString)
    'End Sub
    '
    'Private Sub oWB_ProgressChanged(sender As Object, e As WebBrowserProgressChangedEventArgs)
    '    DP1("oWB_ProgressChanged: " & e.CurrentProgress.ToString & " / " & e.MaximumProgress.ToString)
    'End Sub
    '
    'Private Sub oWB_RegionChanged(sender As Object, e As EventArgs)
    '    DP1("oWB_RegionChanged: ")
    'End Sub
    '
    'Private Sub oWB_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs)
    '    DP1("oWB_Validating: " & e.Cancel.ToString)
    'End Sub
    '
    'Private Sub oWB_Validated(sender As Object, e As EventArgs)
    '    DP1("oWB_Validated: ")
    'End Sub
    '
    'Private Sub oWB_NewWindow(sender As Object, e As System.ComponentModel.CancelEventArgs)
    '    DP1("oWB_NewWindow: ")
    'End Sub

    'Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
    '
    '    Dim Reg As sRegistro
    '    Dim Registros As List(Of sRegistro)
    '    Dim oDiv As HtmlElementCollection
    '    Dim i As Integer = 0
    '
    '    Try
    '
    '        Registros = New List(Of sRegistro)
    '
    '        oDiv = oWB.Document.GetElementsByTagName("div")
    '
    '        For Each oDivFicha As HtmlElement In oDiv
    '
    '            If oDivFicha.GetAttribute("className") = "listCard v-card v-sheet theme--light" Then
    '                '                                     listCard v-card v-sheet theme--light
    '                '
    '                '----------------------------------------------------------------------------------------------------------------------------------------------------------
    '                Reg = New sRegistro(i)
    '                '----------------------------------------------------------------------------------------------------------------------------------------------------------
    '                Reg.DATA = oDivFicha.Children(0).Children(0).Children(0).Children(1).InnerText.Trim
    '                Reg.NCM = oDivFicha.Children(0).Children(0).Children(1).Children(1).InnerText.Trim
    '                Reg.INCOTERM = oDivFicha.Children(0).Children(0).Children(2).Children(1).InnerText.Trim
    '                Reg.MODAL = oDivFicha.Children(0).Children(0).Children(3).Children(1).InnerText.Trim
    '                Reg.PRODUCT_DESCRIPTION = oDivFicha.Children(0).Children(3).Children(0).Children(0).Children(0).Children(0).Children(2).Children(0).InnerText.Trim
    '                '----------------------------------------------------------------------------------------------------------------------------------------------------------
    '                Reg.LIKELY_EXPORTER = oDivFicha.Children(0).Children(3).Children(0).Children(0).Children(1).Children(0).Children(0).Children(0).Children(2).InnerText.Trim
    '                Reg.LIKELY_IMPORTER = oDivFicha.Children(0).Children(3).Children(0).Children(0).Children(1).Children(1).Children(0).Children(0).Children(2).InnerText.Trim
    '                '----------------------------------------------------------------------------------------------------------------------------------------------------------
    '                Reg.FOB_UNIT = oDivFicha.Children(0).Children(3).Children(0).Children(0).Children(2).Children(0).Children(0).Children(1).Children(1).InnerText
    '                Reg.FOB_TOT = oDivFicha.Children(0).Children(3).Children(0).Children(0).Children(2).Children(0).Children(0).Children(2).Children(1).InnerText
    '                Reg.CIF_UNIT = oDivFicha.Children(0).Children(3).Children(0).Children(0).Children(2).Children(0).Children(0).Children(1).Children(2).InnerText
    '                Reg.CIF_TOT = oDivFicha.Children(0).Children(3).Children(0).Children(0).Children(2).Children(0).Children(0).Children(2).Children(2).InnerText
    '                '----------------------------------------------------------------------------------------------------------------------------------------------------------
    '                Reg.MARKETED_QTY = oDivFicha.Children(0).Children(3).Children(1).Children(0).Children(0).Children(1).InnerText.Trim
    '                Reg.NET_WEIGHT = oDivFicha.Children(0).Children(3).Children(1).Children(0).Children(1).Children(1).InnerText.Trim
    '                Reg.ACQUISITION_COUNTRY = oDivFicha.Children(0).Children(3).Children(1).Children(0).Children(2).Children(1).InnerText.Trim
    '                Reg.ORIGIN_COUNTRY = oDivFicha.Children(0).Children(3).Children(1).Children(0).Children(3).Children(1).InnerText.Trim
    '                Reg.CLEARANCE_BRANCH = oDivFicha.Children(0).Children(3).Children(1).Children(0).Children(4).Children(1).InnerText.Trim
    '                '----------------------------------------------------------------------------------------------------------------------------------------------------------
    '                Registros.Add(Reg) '-- 
    '                '----------------------------------------------------------------------------------------------------------------------------------------------------------
    '                i += 1
    '                '----------------------------------------------------------------------------------------------------------------------------------------------------------
    '                '
    '            End If
    '
    '        Next oDivFicha
    '
    '        DP1("TOT:" & i.ToString)
    '
    '    Catch ex As Exception
    '        DP1("Erro: " & ex.Message)
    '    End Try
    '
    '
    'End Sub

    'Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
    '
    '    oWB.Navigate("https://plataforma.logcomex.io/signIn/")
    '
    'End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click

        Dim oLogChrome As clsLogChrome
        Dim Ficha As clsDadosFichas
        Dim lstFicha As List(Of Ficha)
        Dim strJSONLog As String
        Dim strJSONConteudo As String
        Dim tot As Integer = 0


        Try


            Dim fd As OpenFileDialog = New OpenFileDialog()
            Dim strFileName As String = ""

            fd.Title = "Open File Dialog"
            fd.InitialDirectory = "C:\"
            fd.Filter = "Log (*.har)|*.har"
            fd.FilterIndex = 2
            fd.RestoreDirectory = True

            If fd.ShowDialog() <> DialogResult.OK Then
                Exit Sub
            End If

            strFileName = fd.FileName


            oLogChrome = New clsLogChrome()
            Ficha = New clsDadosFichas()
            lstFicha = New List(Of Ficha)
            strJSONLog = File.ReadAllText(strFileName)

            oLogChrome = JsonConvert.DeserializeObject(Of clsLogChrome)(strJSONLog)

            DP1("#########################################################################################")

            For Each oEntry As Entry In oLogChrome.log.entries


                If oEntry.response.content.mimeType = "application/json" Then

                    strJSONConteudo = oEntry.response.content.text

                    If Not strJSONConteudo Is Nothing AndAlso strJSONConteudo.Contains("""data"":{""data""") Then


                        strJSONConteudo = strJSONConteudo.Replace("""data"":{""data""", """data"":{""Ficha""")

                        DP1("-----------------------------------------------------------------------------------------")
                        'DP1(oEntry.response.content.text)

                        Ficha = JsonConvert.DeserializeObject(Of clsDadosFichas)(strJSONConteudo)


                        For Each _ficha As Ficha In Ficha.data.ficha
                            lstFicha.Add(_ficha)
                        Next _ficha


                        DP1("TOT JSON: " & Ficha.data.ficha.Count.ToString)

                        tot += Ficha.data.ficha.Count

                    End If

                End If

            Next oEntry

            DP1("-----------------------------------------------------------------------------------------")
            DP1("TOT GERAL: " & tot.ToString)

            pb1.Minimum = 0
            pb1.Maximum = lstFicha.Count
            pb1.Value = 0

            DP1("#########################################################################################")
            DP1("## INICIO INSERÇÃO BANCO DE DADOS...")

            For Each _ficha As Ficha In lstFicha

                Incluir(_ficha)

                pb1.Value += 1

                If pb1.Value Mod 10 = 0 Then
                    DP1("## ATUALIZANDO: " & pb1.Value.ToString & " / " & lstFicha.Count.ToString)
                    Application.DoEvents()
                    pb1.Refresh()
                End If


            Next _ficha

            DP1("## FIM INSERÇÃO BANCO DE DADOS...")
            DP1("#########################################################################################")


        Catch ex As Exception
            MsgBox("Erro em: " & ex.Message)
        End Try

    End Sub

    Private Sub Incluir(ByVal oFicha As Ficha)

        Try
            '
            pFichaImportacao.Incluir(oFicha.nm_pais_origem,
                                     xCDec(oFicha.qtd_comerc),
                                     oFicha.importador_endereco,
                                     oFicha.nm_pais_aquis,
                                     oFicha.via_transp,
                                     oFicha.nome_unid_desembaraco,
                                     xCDec(oFicha.anomes),
                                     xCDec(oFicha.id_import),
                                     xCDec(oFicha.val_fob_us_subitem),
                                     oFicha.exportador_nome,
                                     oFicha.desc_prodt,
                                     oFicha.tp_unid_comerc,
                                     oFicha.incoterm,
                                     oFicha.importador_cnpj,
                                     xCDec(oFicha.val_cif_un_us),
                                     xCDec(oFicha.val_fob_un_us),
                                     xCDec(oFicha.val_vmld_us_subitem),
                                     xCDec(oFicha.cdncm_compl),
                                     xCDec(oFicha.val_peso_liq_subitem),
                                     oFicha.importador_nome,
                                     oFicha.nome_adquirente,
                                     xCDec(oFicha.val_frete_un_us),
                                     xCDec(oFicha.val_frete_us_subitem),
                                     xCDec(oFicha.val_seg_un_us),
                                     xCDec(oFicha.val_seg_us_subitem),
                                     oFicha.cidade_import)
            '
        Catch ex As Exception
            MsgBox("Erro em: " & ex.Message)
        End Try


    End Sub

    Private Function xCDec(ByVal _valor As String) As Decimal

        Try


            If IsNumeric(_valor.Replace(".", ",")) Then
                Return CDec(_valor.Replace(".", ","))
            Else
                Return Nothing
            End If

        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    Private Sub FrmPrincipal_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        DP1("## INICIO PROGRAMA")
    End Sub

    Private Sub cmdPesq_Click(sender As Object, e As EventArgs) Handles cmdPesq.Click

        Dim rsPesq As SuperDataSet

        rsPesq = pFichaImportacao.ObterDadosGrid(txt_cdncm_compl.Text.Trim,
                                                 txt_desc_prodt.Text.Trim,
                                                 txt_incoterm.Text.Trim,
                                                 txt_importador_nome.Text.Trim,
                                                 txt_exportador_nome.Text.Trim,
                                                 txt_nm_pais_aquis.Text.Trim,
                                                 txt_nm_pais_origem.Text.Trim)

        lvResult.PreencheGridDS(rsPesq, False)

        txtInfo.TextoLetreiro = rsPesq.InfoPesquisa


    End Sub

    Private Sub cmdLimpa_Click(sender As Object, e As EventArgs) Handles cmdLimpa.Click
        lvResult.Items.Clear()
    End Sub

    Private Sub cmdXls_Click(sender As Object, e As EventArgs) Handles cmdXls.Click

        Dim rsPesq As SuperDataSet
        Dim oSuperXls As SuperXLS

        rsPesq = pFichaImportacao.ObterDadosGrid(txt_cdncm_compl.Text.Trim,
                                                 txt_desc_prodt.Text.Trim,
                                                 txt_incoterm.Text.Trim,
                                                 txt_importador_nome.Text.Trim,
                                                 txt_exportador_nome.Text.Trim,
                                                 txt_nm_pais_aquis.Text.Trim,
                                                 txt_nm_pais_origem.Text.Trim)


        oSuperXls = New SuperXLS("Pesquisa")

        oSuperXls.Imprimir(rsPesq, "WebBaixa", True,,, False)

        'lvResult.PreencheGridDS(rsPesq, False)
        'txtInfo.TextoLetreiro = rsPesq.InfoPesquisa


    End Sub

    Private Sub cmdHelp_Click(sender As Object, e As EventArgs) Handles cmdHelp.Click

        frmSobre.ShowDialog()

    End Sub
End Class
