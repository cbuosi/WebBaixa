Option Explicit On
Option Strict On

Imports System.IO
Imports OfficeOpenXml
Imports OfficeOpenXml.Drawing 'use to load image inside excel document
Imports OfficeOpenXml.Style
Imports System.Drawing
Imports GFT.Util.clsMsgBox

Public Class SuperXLS

    Structure sPropriedadeCelula

        Enum eAlinhamento
            General = 0
            Left = 1
            Center = 2
            CenterContinuous = 3
            Right = 4
            Fill = 5
            Distributed = 6
            Justify = 7
        End Enum

        Sub New(ByVal _Linha As Integer,
                ByVal _Coluna As Integer,
                ByVal _Valor As Object,
                ByVal _Fonte As Font,
                ByVal _Cor As Color,
                ByVal _Negrito As Boolean,
                Optional ByVal _Numberformat As String = "",
                Optional ByVal _Alinhamento As eAlinhamento = eAlinhamento.General,
                Optional ByVal _Titulo As Boolean = False)

            Linha = _Linha
            Coluna = _Coluna
            Valor = _Valor
            Fonte = _Fonte
            Cor = _Cor
            Negrito = _Negrito
            Alinhamento = _Alinhamento
            Titulo = _Titulo
            Numberformat = _Numberformat

        End Sub

        Dim Linha As Integer
        Dim Coluna As Integer
        Dim Valor As Object
        Dim TamanhoCelula As Integer
        Dim Fonte As Font
        Dim Cor As Color
        Dim Negrito As Boolean
        Dim Alinhamento As eAlinhamento
        Dim Titulo As Boolean
        Dim FundoBradesco As Boolean
        Dim Numberformat As String
    End Structure

    Private strArquivoXLS As String = ""
    Private strAba As String = "WebImport"
    Private strTitulo As String = "WebImport - Relatório"
    Private strAutor As String = "WebImport"
    Private strComentario As String = "WebImport - WebImport"
    Private strCompania As String = "WebImport S/A"

    Const LINHA_CABECALHO_BRADESCO As Integer = 1
    Const LINHA_CABECALHO_RELATORIO As Integer = 2
    Const LINHA_CABECALHO_TABELA As Integer = 1
    Const COLUNA_INICIAL_DADOS As Integer = 1

    Private Pacote As ExcelPackage
    Private Planilha As ExcelWorksheet

    Sub New(ByVal _strArquivo As String)
        Me.Arquivo = _strArquivo
    End Sub

    Sub New()
    End Sub

#Region "Get_Set"
    Public Property Arquivo() As String
        Get
            Return strArquivoXLS
        End Get
        Set(ByVal Value As String)
            strArquivoXLS = Value
        End Set
    End Property

    Public Property Aba() As String
        Get
            Return strAba
        End Get
        Set(ByVal Value As String)
            strAba = Value
        End Set
    End Property

    Public Property Titulo() As String
        Get
            Return strTitulo
        End Get
        Set(ByVal Value As String)
            strTitulo = Value
        End Set
    End Property

    Public Property Autor() As String
        Get
            Return strAutor
        End Get
        Set(ByVal Value As String)
            strAutor = Value
        End Set
    End Property

    Public Property Comentario() As String
        Get
            Return strComentario
        End Get
        Set(ByVal Value As String)
            strComentario = Value
        End Set
    End Property

    Public Property Compania() As String
        Get
            Return strCompania
        End Get
        Set(ByVal Value As String)
            strCompania = Value
        End Set
    End Property
#End Region

    Public Function Imprimir(ByVal oDataset As SuperDataSet,
                             ByVal sNomeRelatorio As String,
                             Optional ByVal bAbrir As Boolean = False,
                             Optional ByVal bAutoFiltro As Boolean = True,
                             Optional ByVal bCongelarPainel As Boolean = True,
                             Optional ByVal bFundoBradesco As Boolean = True,
                             Optional ByVal bHabilitaGrade As Boolean = False) As Boolean

        Dim oArquivo As FileInfo
        Dim strCampo As String
        Dim iRow As Integer
        Dim iCol As Integer
        Dim ValorCampo As Object
        Dim sCabecalho As String
        Dim iTotalColunasDetalhe As Integer
        Dim sCaminhoRelat As String
        Dim CorSim As Color = System.Drawing.ColorTranslator.FromHtml("#F3F3F3")
        Dim CorNao As Color = System.Drawing.ColorTranslator.FromHtml("#FFFFFF")

        Try

            '
            Me.Arquivo = Me.Arquivo & "." & Format(Now, "yyyy.MM.dd-hh.mm.ss") & ".xlsx"
            '
            'sCaminhoRelat = Path.Combine(Environment.GetEnvironmentVariable("Temp"), "_TEAD_PRINT")
            sCaminhoRelat = Path.Combine(My.Computer.FileSystem.SpecialDirectories.Desktop, Me.Arquivo)
            ChecaCriaDiretorio(sCaminhoRelat)
            Me.Arquivo = Path.Combine(sCaminhoRelat, Me.Arquivo)

            If ExisteArquivo(Me.Arquivo) Then
                If S_MsgBox("Já existe o arquivo [" & Me.Arquivo & "]." & vbNewLine &
                         "Deseja sobrescrever?.", eBotoes.SimNao, , , eImagens.Interrogacao) = eRet.Nao Then
                    Return False
                Else
                    ApagaArquivo(Me.Arquivo)
                End If
            End If

            oArquivo = New FileInfo(Me.Arquivo)
            Pacote = New ExcelPackage(oArquivo)

            'Ajusta cabeçalho
            sCabecalho = sNomeRelatorio & " (" & Format(Now, "dd/MM/yyyy hh:mm:ss") & ")"

            Planilha = Pacote.Workbook.Worksheets.Add(Me.Aba)

            'Planilha.Cells("A1:AB1").Style.Font.Bold = True
            iCol = COLUNA_INICIAL_DADOS

            'Guarda o total de colunas
            iTotalColunasDetalhe = 0

            ''#############################################
            ''#####     FOR pra montar as colunas.    #####
            ''#############################################
            For i = 0 To oDataset.TotalColunas() - 1 'FieldCount() - 1
                strCampo = oDataset.NomeColuna(i)
                If (strCampo.Substring(0, 3) = "as_") Or
                   (strCampo.Substring(0, 3) = "me_") Or
                   (strCampo.Substring(0, 3) = "nu_") Then
                    strCampo = FormataColuna(strCampo)
                    Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Value = strCampo
                    Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Font.Bold = True
                    Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
                    Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Fill.BackgroundColor.SetColor(Color.LightGray)
                    Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center
                    iCol = iCol + 1
                    iTotalColunasDetalhe += 1
                End If
            Next i

            If bAutoFiltro = True Then
                Planilha.Cells(LINHA_CABECALHO_TABELA, COLUNA_INICIAL_DADOS, LINHA_CABECALHO_TABELA, iCol - 1).AutoFilter = bAutoFiltro
            End If

            If bCongelarPainel = True Then
                Planilha.View.FreezePanes(LINHA_CABECALHO_TABELA + 1, COLUNA_INICIAL_DADOS)
            End If

            iRow = LINHA_CABECALHO_TABELA + 1

            '################################################
            '#####     FOR pra Preencher as colunas.    #####
            '################################################
            For posReg = 0 To (oDataset.TotalRegistros() - 1) Step 1
                iCol = COLUNA_INICIAL_DADOS
                For i = 0 To (oDataset.TotalColunas() - 1)

                    strCampo = oDataset.NomeColuna(i)

                    If (strCampo.Substring(0, 3) = "as_") Or
                       (strCampo.Substring(0, 3) = "me_") Or
                       (strCampo.Substring(0, 3) = "nu_") Then

                        ValorCampo = oDataset.ValorCampo(i, posReg)

                        Planilha.Cells(iRow, iCol).Value = ValorCampo

                        Planilha.Cells(iRow, iCol).Style.Fill.PatternType = ExcelFillStyle.Solid

                        If iRow Mod 2 = 0 Then
                            Planilha.Cells(iRow, iCol).Style.Fill.BackgroundColor.SetColor(CorSim)
                        Else
                            Planilha.Cells(iRow, iCol).Style.Fill.BackgroundColor.SetColor(CorNao)
                        End If



                        If (oDataset.TipoDadosColuna(i) Is GetType(Decimal)) Or
                            (oDataset.TipoDadosColuna(i) Is GetType(Integer)) Then
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Right
                        ElseIf (oDataset.TipoDadosColuna(i) Is GetType(DateTime)) Then
                            Planilha.Cells(iRow, iCol).Style.Numberformat.Format = "dd/MM/yyyy"
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
                        Else
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Left
                        End If

                        iCol = iCol + 1
                    End If
                Next i
                iRow = iRow + 1
            Next posReg
            '################################################
            '#####  FIM FOR pra Preencher as colunas.   #####
            '################################################

            'Autoajuste das colunas
            For iCol = COLUNA_INICIAL_DADOS To COLUNA_INICIAL_DADOS + iTotalColunasDetalhe - 1
                Planilha.Column(iCol).AutoFit()
                Planilha.Column(iCol).Style.WrapText = True
            Next

            'Planilha.Row(1).Style = Style.ExcelStyle.

            'Coloca o cabeçalho do Bradesco
            'Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Value = "BRADESCO"
            'Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Color.SetColor(Color.White)
            'Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Bold = True
            'Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Size = 22

            'Ajuste da Cor da linha de cabeçalho bradesco das colunas
            'For iCol = COLUNA_INICIAL_DADOS To COLUNA_INICIAL_DADOS + iTotalColunasDetalhe - 1
            '    Planilha.Cells(LINHA_CABECALHO_BRADESCO, iCol).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
            '    Planilha.Cells(LINHA_CABECALHO_BRADESCO, iCol).Style.Fill.BackgroundColor.SetColor(Color.DarkRed)
            'Next iCol

            'Coloca o cabeçalho do relatório
            'Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Value = sCabecalho
            'AgrupaCelulaTitulo(LINHA_CABECALHO_RELATORIO)
            'Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Bold = True
            'Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Color.SetColor(Color.DarkBlue)
            'Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Size = 16

            'If bFundoBradesco = True Then
            '   Planilha.BackgroundImage.Image = My.Resources.SuperLV
            'End If

            Planilha.View.ShowGridLines = bHabilitaGrade
            Pacote.Workbook.Properties.Title = Me.Titulo
            Pacote.Workbook.Properties.Author = Me.Autor
            Pacote.Workbook.Properties.Comments = Me.Comentario
            Pacote.Workbook.Properties.Company = Me.Compania

            Pacote.Workbook.Properties.SetCustomPropertyValue("Criado em", Now.ToString)

            Pacote.Save()

            If bAbrir = True Then
                Me.Visualizar()
            End If

            Return True

        Catch ex As Exception
            LogaErro("Erro em SuperXLS::Imprimir: " & ex.Message)
            Return False
        Finally
            Pacote.Dispose()
            Planilha = Nothing
            Pacote = Nothing
        End Try

    End Function



    Public Function ImprimirListaDataSet(ByVal listoDataset As List(Of SuperDataSet),
                                         ByVal sNomeRelatorio As String,
                                         Optional ByVal bAbrir As Boolean = False,
                                         Optional ByVal bAutoFiltro As Boolean = True,
                                         Optional ByVal bCongelarPainel As Boolean = True,
                                         Optional ByVal bFundoBradesco As Boolean = True,
                                         Optional ByVal bHabilitaGrade As Boolean = False) As Boolean

        Dim oArquivo As FileInfo
        Dim strCampo As String
        Dim iRow As Integer
        Dim iCol As Integer
        Dim ValorCampo As Object
        Dim sCabecalho As String
        Dim iTotalColunasDetalhe As Integer
        Dim sCaminhoRelat As String
        Dim CorSim As Color = System.Drawing.ColorTranslator.FromHtml("#F3F3F3")
        Dim CorNao As Color = System.Drawing.ColorTranslator.FromHtml("#FFFFFF")

        Try

            Me.Arquivo = Me.Arquivo & "." & Format(Now, "yyyy.MM.dd-hh.mm.ss") & ".xlsx"

            sCaminhoRelat = Path.Combine(Environment.GetEnvironmentVariable("Temp"), "_TEAD_PRINT")
            ChecaCriaDiretorio(sCaminhoRelat)
            Me.Arquivo = Path.Combine(sCaminhoRelat, Me.Arquivo)

            If ExisteArquivo(Me.Arquivo) Then
                If S_MsgBox("Já existe o arquivo [" & Me.Arquivo & "]." & vbNewLine &
                         "Deseja sobrescrever?.", eBotoes.SimNao, , , eImagens.Interrogacao) = eRet.Nao Then
                    Return False
                Else
                    ApagaArquivo(Me.Arquivo)
                End If
            End If

            oArquivo = New FileInfo(Me.Arquivo)
            Pacote = New ExcelPackage(oArquivo)

            'Ajusta cabeçalho
            sCabecalho = sNomeRelatorio & " (" & Format(Now, "dd/MM/yyyy hh:mm:ss") & ")"

            Planilha = Pacote.Workbook.Worksheets.Add(Me.Aba)

            'Planilha.Cells("A1:AB1").Style.Font.Bold = True
            iCol = COLUNA_INICIAL_DADOS

            'Guarda o total de colunas
            iTotalColunasDetalhe = 0

            ''#############################################
            ''#####     FOR pra montar as colunas.    #####
            ''#############################################
            For Each oDataset In listoDataset


                For i = 0 To oDataset.TotalColunas() - 1 'FieldCount() - 1
                    strCampo = oDataset.NomeColuna(i)
                    If (strCampo.Substring(0, 3) = "as_") Or
                       (strCampo.Substring(0, 3) = "me_") Or
                       (strCampo.Substring(0, 3) = "nu_") Then
                        strCampo = FormataColuna(strCampo)
                        Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Value = strCampo
                        Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Font.Bold = True
                        Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
                        Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Fill.BackgroundColor.SetColor(Color.LightGray)
                        Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center
                        iCol = iCol + 1
                        iTotalColunasDetalhe += 1
                    End If
                Next i
                Exit For
            Next
            If bAutoFiltro = True Then
                Planilha.Cells(LINHA_CABECALHO_TABELA, COLUNA_INICIAL_DADOS, LINHA_CABECALHO_TABELA, iCol - 1).AutoFilter = bAutoFiltro
            End If

            If bCongelarPainel = True Then
                Planilha.View.FreezePanes(LINHA_CABECALHO_TABELA + 1, COLUNA_INICIAL_DADOS)
            End If

            iRow = LINHA_CABECALHO_TABELA + 1

            '################################################
            '#####     FOR pra Preencher as colunas.    #####
            '################################################
            For Each oDataset In listoDataset


                For posReg = 0 To (oDataset.TotalRegistros() - 1) Step 1
                    iCol = COLUNA_INICIAL_DADOS
                    For i = 0 To (oDataset.TotalColunas() - 1)

                        strCampo = oDataset.NomeColuna(i)

                        If (strCampo.Substring(0, 3) = "as_") Or
                       (strCampo.Substring(0, 3) = "me_") Or
                       (strCampo.Substring(0, 3) = "nu_") Then

                            ValorCampo = oDataset.ValorCampo(i, posReg)

                            Planilha.Cells(iRow, iCol).Value = ValorCampo

                            Planilha.Cells(iRow, iCol).Style.Fill.PatternType = ExcelFillStyle.Solid

                            If iRow Mod 2 = 0 Then
                                Planilha.Cells(iRow, iCol).Style.Fill.BackgroundColor.SetColor(CorSim)
                            Else
                                Planilha.Cells(iRow, iCol).Style.Fill.BackgroundColor.SetColor(CorNao)
                            End If



                            If (oDataset.TipoDadosColuna(i) Is GetType(Decimal)) Or
                            (oDataset.TipoDadosColuna(i) Is GetType(Integer)) Then
                                Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Right
                            ElseIf (oDataset.TipoDadosColuna(i) Is GetType(DateTime)) Then
                                Planilha.Cells(iRow, iCol).Style.Numberformat.Format = "dd/MM/yyyy"
                                Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
                            Else
                                Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Left
                            End If

                            iCol = iCol + 1
                        End If
                    Next i
                    iRow = iRow + 1
                Next posReg
            Next
            '################################################
            '#####  FIM FOR pra Preencher as colunas.   #####
            '################################################

            'Autoajuste das colunas
            For iCol = COLUNA_INICIAL_DADOS To COLUNA_INICIAL_DADOS + iTotalColunasDetalhe - 1
                Planilha.Column(iCol).AutoFit()
            Next

            'Coloca o cabeçalho do Bradesco
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Value = "BRADESCO"
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Color.SetColor(Color.White)
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Bold = True
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Size = 22

            'Ajuste da Cor da linha de cabeçalho bradesco das colunas
            For iCol = COLUNA_INICIAL_DADOS To COLUNA_INICIAL_DADOS + iTotalColunasDetalhe - 1
                Planilha.Cells(LINHA_CABECALHO_BRADESCO, iCol).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
                Planilha.Cells(LINHA_CABECALHO_BRADESCO, iCol).Style.Fill.BackgroundColor.SetColor(Color.DarkRed)
            Next iCol

            'Coloca o cabeçalho do relatório
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Value = sCabecalho
            AgrupaCelulaTitulo(LINHA_CABECALHO_RELATORIO)
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Bold = True
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Color.SetColor(Color.DarkBlue)
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Size = 16

            'If bFundoBradesco = True Then
            '   Planilha.BackgroundImage.Image = My.Resources.SuperLV
            'End If

            Planilha.View.ShowGridLines = bHabilitaGrade
            Pacote.Workbook.Properties.Title = Me.Titulo
            Pacote.Workbook.Properties.Author = Me.Autor
            Pacote.Workbook.Properties.Comments = Me.Comentario
            Pacote.Workbook.Properties.Company = Me.Compania

            Pacote.Workbook.Properties.SetCustomPropertyValue("Criado em", Now.ToString)

            Pacote.Save()

            If bAbrir = True Then
                Me.Visualizar()
            End If

            Return True

        Catch ex As Exception
            LogaErro("Erro em SuperXLS::Imprimir: " & ex.Message)
            Return False
        Finally
            Pacote.Dispose()
            Planilha = Nothing
            Pacote = Nothing
        End Try

    End Function

    Public Function ImprimirPopDet(ByVal oDataset As SuperDataSet,
                                   ByVal sNomeRelatorio As String,
                                   Optional ByVal Desc As String = "",
                                   Optional ByVal bAbrir As Boolean = False,
                                   Optional ByVal bAutoFiltro As Boolean = True,
                                   Optional ByVal bCongelarPainel As Boolean = True,
                                   Optional ByVal bFundoBradesco As Boolean = True,
                                   Optional ByVal bHabilitaGrade As Boolean = False) As Boolean

        Dim oArquivo As FileInfo
        Dim strCampo As String
        Dim iRow As Integer
        Dim iCol As Integer
        Dim ValorCampo As Object
        Dim sCabecalho As String
        Dim iTotalColunasDetalhe As Integer
        Dim sCaminhoRelat As String

        Try

            Me.Arquivo = Me.Arquivo & "." & Format(Now, "yyyy.MM.dd-hh.mm.ss") & ".xlsx"

            sCaminhoRelat = Path.Combine(Environment.GetEnvironmentVariable("Temp"), "_TEAD_PRINT")
            ChecaCriaDiretorio(sCaminhoRelat)
            Me.Arquivo = Path.Combine(sCaminhoRelat, Me.Arquivo)

            If ExisteArquivo(Me.Arquivo) Then
                If S_MsgBox("Já existe o arquivo [" & Me.Arquivo & "]." & vbNewLine &
                         "Deseja sobrescrever?.", eBotoes.SimNao, , , eImagens.Interrogacao) = eRet.Nao Then
                    Return False
                Else
                    ApagaArquivo(Me.Arquivo)
                End If
            End If

            oArquivo = New FileInfo(Me.Arquivo)
            Pacote = New ExcelPackage(oArquivo)

            'Ajusta cabeçalho
            sCabecalho = sNomeRelatorio & " (" & Format(Now, "dd/MM/yyyy hh:mm:ss") & ")"

            Planilha = Pacote.Workbook.Worksheets.Add(Me.Aba)

            iCol = COLUNA_INICIAL_DADOS

            'Guarda o total de colunas
            iTotalColunasDetalhe = 0
            ''TAB 1
            ''#############################################
            ''#####     FOR pra montar as colunas.    #####
            ''#############################################
            For i = 0 To oDataset.TotalColunas() - 1 'FieldCount() - 1
                strCampo = oDataset.NomeColuna(i)
                If (strCampo.Substring(0, 3) = "as_") Or
                   (strCampo.Substring(0, 3) = "me_") Or
                   (strCampo.Substring(0, 3) = "nu_") Then
                    strCampo = FormataColuna(strCampo)
                    Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Value = strCampo
                    Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Font.Bold = True
                    Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
                    Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Fill.BackgroundColor.SetColor(Color.LightGray)
                    Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center
                    iCol = iCol + 1
                    iTotalColunasDetalhe += 1
                End If
            Next i

            iRow = LINHA_CABECALHO_TABELA + 1

            '################################################
            '#####     FOR pra Preencher as colunas.    #####
            '################################################
            For posReg = 0 To (oDataset.TotalRegistros() - 1) Step 1
                iCol = COLUNA_INICIAL_DADOS
                For i = 0 To (oDataset.TotalColunas() - 1)

                    strCampo = oDataset.NomeColuna(i)

                    If (strCampo.Substring(0, 3) = "as_") Or
                       (strCampo.Substring(0, 3) = "me_") Or
                       (strCampo.Substring(0, 3) = "nu_") Then

                        ValorCampo = oDataset.ValorCampo(i, posReg)

                        Planilha.Cells(iRow, iCol).Value = ValorCampo

                        If (oDataset.TipoDadosColuna(i) Is GetType(Decimal)) Or
                            (oDataset.TipoDadosColuna(i) Is GetType(Integer)) Then
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Right
                        ElseIf (oDataset.TipoDadosColuna(i) Is GetType(DateTime)) Then
                            Planilha.Cells(iRow, iCol).Style.Numberformat.Format = "dd/MM/yyyy"
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
                        Else
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Left
                        End If

                        iCol = iCol + 1
                    End If
                Next i
                iRow = iRow + 1
            Next posReg
            '################################################
            '#####  FIM FOR pra Preencher as colunas.   #####
            '################################################
            ''TAB2
            iCol = COLUNA_INICIAL_DADOS

            'Guarda o total de colunas
            iTotalColunasDetalhe = 0
            ''#############################################
            ''#####     FOR pra montar as colunas.    #####
            ''#############################################
            For i = 0 To oDataset.TotalColunas(1) - 1 'FieldCount() - 1
                strCampo = oDataset.NomeColuna(i, 1)
                If (strCampo.Substring(0, 3) = "as_") Or
                   (strCampo.Substring(0, 3) = "me_") Or
                   (strCampo.Substring(0, 3) = "nu_") Then
                    strCampo = FormataColuna(strCampo)
                    Planilha.Cells(iRow, iCol).Value = strCampo
                    Planilha.Cells(iRow, iCol).Style.Font.Bold = True
                    Planilha.Cells(iRow, iCol).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
                    Planilha.Cells(iRow, iCol).Style.Fill.BackgroundColor.SetColor(Color.LightGray)
                    Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center
                    iCol = iCol + 1
                    iTotalColunasDetalhe += 1
                End If
            Next i

            iRow += 1

            '################################################
            '#####     FOR pra Preencher as colunas.    #####
            '################################################
            For posReg = 0 To (oDataset.TotalRegistros(1) - 1) Step 1
                iCol = COLUNA_INICIAL_DADOS
                For i = 0 To (oDataset.TotalColunas(1) - 1)

                    strCampo = oDataset.NomeColuna(i, 1)

                    If (strCampo.Substring(0, 3) = "as_") Or
                       (strCampo.Substring(0, 3) = "me_") Or
                       (strCampo.Substring(0, 3) = "nu_") Then

                        ValorCampo = oDataset.ValorCampo(i, posReg, 1)

                        Planilha.Cells(iRow, iCol).Value = ValorCampo

                        If (oDataset.TipoDadosColuna(i, 1) Is GetType(Decimal)) Or
                            (oDataset.TipoDadosColuna(i, 1) Is GetType(Integer)) Then
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Right
                        ElseIf (oDataset.TipoDadosColuna(i, 1) Is GetType(DateTime)) Then
                            Planilha.Cells(iRow, iCol).Style.Numberformat.Format = "dd/MM/yyyy"
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
                        Else
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Left
                        End If

                        iCol = iCol + 1
                    End If
                Next i
                iRow = iRow + 1
            Next posReg
            '################################################
            '#####  FIM FOR pra Preencher as colunas.   #####
            '################################################

            'Autoajuste das colunas
            For iCol = COLUNA_INICIAL_DADOS To COLUNA_INICIAL_DADOS + iTotalColunasDetalhe - 1
                Planilha.Column(iCol).AutoFit()
            Next

            ''TAB3
            iCol = COLUNA_INICIAL_DADOS

            'Guarda o total de colunas
            iTotalColunasDetalhe = 0
            ''#############################################
            ''#####     FOR pra montar as colunas.    #####
            ''#############################################
            For i = 0 To oDataset.TotalColunas(2) - 1 'FieldCount() - 1
                strCampo = oDataset.NomeColuna(i, 2)
                If (strCampo.Substring(0, 3) = "as_") Or
                   (strCampo.Substring(0, 3) = "me_") Or
                   (strCampo.Substring(0, 3) = "nu_") Then
                    strCampo = FormataColuna(strCampo)
                    Planilha.Cells(iRow, iCol).Value = strCampo
                    Planilha.Cells(iRow, iCol).Style.Font.Bold = True
                    Planilha.Cells(iRow, iCol).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
                    Planilha.Cells(iRow, iCol).Style.Fill.BackgroundColor.SetColor(Color.LightGray)
                    Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center
                    iCol = iCol + 1
                    iTotalColunasDetalhe += 1
                End If
            Next i

            iRow += 1

            '################################################
            '#####     FOR pra Preencher as colunas.    #####
            '################################################
            For posReg = 0 To (oDataset.TotalRegistros(2) - 1) Step 1
                iCol = COLUNA_INICIAL_DADOS
                For i = 0 To (oDataset.TotalColunas(2) - 1)

                    strCampo = oDataset.NomeColuna(i, 2)

                    If (strCampo.Substring(0, 3) = "as_") Or
                       (strCampo.Substring(0, 3) = "me_") Or
                       (strCampo.Substring(0, 3) = "nu_") Then

                        ValorCampo = oDataset.ValorCampo(i, posReg, 2)

                        Planilha.Cells(iRow, iCol).Value = ValorCampo

                        If (oDataset.TipoDadosColuna(i, 2) Is GetType(Decimal)) Or
                            (oDataset.TipoDadosColuna(i, 2) Is GetType(Integer)) Then
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Right
                        ElseIf (oDataset.TipoDadosColuna(i, 2) Is GetType(DateTime)) Then
                            Planilha.Cells(iRow, iCol).Style.Numberformat.Format = "dd/MM/yyyy"
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
                        Else
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Left
                        End If

                        iCol = iCol + 1
                    End If
                Next i
                iRow = iRow + 1
            Next posReg
            '################################################
            '#####  FIM FOR pra Preencher as colunas.   #####
            '################################################

            'Coloca o cabeçalho do Bradesco
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Value = "BRADESCO - " & sNomeRelatorio & " | " & Desc
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Color.SetColor(Color.White)
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Bold = True
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Size = 22

            'Ajuste da Cor da linha de cabeçalho bradesco das colunas
            For iCol = COLUNA_INICIAL_DADOS To COLUNA_INICIAL_DADOS + iTotalColunasDetalhe - 1
                Planilha.Cells(LINHA_CABECALHO_BRADESCO, iCol).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
                Planilha.Cells(LINHA_CABECALHO_BRADESCO, iCol).Style.Fill.BackgroundColor.SetColor(Color.DarkRed)
            Next iCol

            'Coloca o cabeçalho do relatório
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Value = sCabecalho
            AgrupaCelulaTitulo(LINHA_CABECALHO_RELATORIO)
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Bold = True
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Color.SetColor(Color.DarkBlue)
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Size = 16

            Planilha.View.ShowGridLines = bHabilitaGrade
            Pacote.Workbook.Properties.Title = Me.Titulo
            Pacote.Workbook.Properties.Author = Me.Autor
            Pacote.Workbook.Properties.Comments = Me.Comentario
            Pacote.Workbook.Properties.Company = Me.Compania

            Pacote.Workbook.Properties.SetCustomPropertyValue("Criado em", Now.ToString)

            Pacote.Save()

            If bAbrir = True Then
                Me.Visualizar()
            End If

            Return True

        Catch ex As Exception
            LogaErro("Erro em SuperXLS::Imprimir: " & ex.Message)
            Return False
        Finally
            Pacote.Dispose()
            Planilha = Nothing
            Pacote = Nothing
        End Try

    End Function

    '################################################
    '###CONVERTE O VALOR DO NÚMERO EM LETRA COLUNA###
    '################################################
    Function ConLetra(iNum As Integer) As String
        Dim iAlpha As Integer
        Dim iRemainder As Integer
        Dim Resul As String = ""
        iAlpha = iNum \ 27
        iRemainder = iNum - (iAlpha * 26)
        If iAlpha > 0 Then
            Resul = Chr(iAlpha + 64)
        End If
        If iRemainder > 0 Then
            Resul += Chr(iRemainder + 64)
        End If
        Return Resul
    End Function

    Public Function ImprimirTudo(ByVal oDataset As SuperDataSet,
                                 ByVal sNomeRelatorio As String,
                                 Optional ByVal bAbrir As Boolean = False,
                                 Optional ByVal bAutoFiltro As Boolean = True,
                                 Optional ByVal bCongelarPainel As Boolean = True,
                                 Optional ByVal bFundoBradesco As Boolean = True,
                                 Optional ByVal bHabilitaGrade As Boolean = False) As Boolean

        Dim oArquivo As FileInfo
        Dim strCampo As String
        'Dim iLinha As Integer = 1
        'Dim iColuna As Integer = 1
        Dim iRow As Integer
        Dim iCol As Integer
        Dim ValorCampo As Object
        Dim sCabecalho As String
        Dim iTotalColunasDetalhe As Integer
        Dim sCaminhoRelat As String

        Try

            Me.Arquivo = Me.Arquivo & "." & Format(Now, "yyyy.MM.dd-hh.mm.ss") & ".xlsx"

            sCaminhoRelat = Path.Combine(Environment.GetEnvironmentVariable("Temp"), "_TEAD_PRINT")
            ChecaCriaDiretorio(sCaminhoRelat)

            Me.Arquivo = Path.Combine(sCaminhoRelat, Me.Arquivo)

            If ExisteArquivo(Me.Arquivo) Then
                If S_MsgBox("Já existe o arquivo [" & Me.Arquivo & "]." & vbNewLine &
                         "Deseja sobrescrever?.", eBotoes.SimNao, , , eImagens.Interrogacao) = eRet.Nao Then
                    Return False
                Else
                    ApagaArquivo(Me.Arquivo)
                End If
            End If

            oArquivo = New FileInfo(Me.Arquivo)
            Pacote = New ExcelPackage(oArquivo)

            'Ajusta cabeçalho
            sCabecalho = sNomeRelatorio & " (" & Format(Now, "dd/MM/yyyy hh:mm:ss") & ")"

            Planilha = Pacote.Workbook.Worksheets.Add(Me.Aba)

            'Planilha.Cells("A1:AB1").Style.Font.Bold = True
            iCol = COLUNA_INICIAL_DADOS

            'Guarda o total de colunas
            iTotalColunasDetalhe = 0

            ''#############################################
            ''#####     FOR pra montar as colunas.    #####
            ''#############################################
            For i = 0 To oDataset.TotalColunas() - 1 'FieldCount() - 1
                strCampo = oDataset.NomeColuna(i)
                'strCampo = Formata_Coluna(strCampo)
                Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Value = strCampo
                Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Font.Bold = True
                Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
                Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.Fill.BackgroundColor.SetColor(Color.LightGray)
                Planilha.Cells(LINHA_CABECALHO_TABELA, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
                iCol = iCol + 1
                iTotalColunasDetalhe += 1
            Next i

            If bAutoFiltro = True Then
                Planilha.Cells(LINHA_CABECALHO_TABELA, COLUNA_INICIAL_DADOS, LINHA_CABECALHO_TABELA, iCol - 1).AutoFilter = bAutoFiltro
            End If

            If bCongelarPainel = True Then
                Planilha.View.FreezePanes(LINHA_CABECALHO_TABELA + 1, COLUNA_INICIAL_DADOS)
            End If

            iRow = LINHA_CABECALHO_TABELA + 1

            '################################################
            '#####     FOR pra Preencher as colunas.    #####
            '################################################
            For posReg = 0 To (oDataset.TotalRegistros() - 1) Step 1
                iCol = COLUNA_INICIAL_DADOS
                For i = 0 To (oDataset.TotalColunas() - 1)

                    strCampo = oDataset.NomeColuna(i)



                    ValorCampo = oDataset.ValorCampo(i, posReg)

                    Planilha.Cells(iRow, iCol).Value = ValorCampo

                    If (oDataset.TipoDadosColuna(i) Is GetType(Decimal)) Or
                        (oDataset.TipoDadosColuna(i) Is GetType(Integer)) Then
                        Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Right
                    ElseIf (oDataset.TipoDadosColuna(i) Is GetType(DateTime)) Then
                        Planilha.Cells(iRow, iCol).Style.Numberformat.Format = "dd/MM/yyyy"
                        Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
                    Else
                        Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Left
                    End If

                    iCol = iCol + 1

                Next i
                iRow = iRow + 1
            Next posReg
            '################################################
            '#####  FIM FOR pra Preencher as colunas.   #####
            '################################################

            'Autoajuste das colunas
            For iCol = COLUNA_INICIAL_DADOS To COLUNA_INICIAL_DADOS + iTotalColunasDetalhe - 1
                Planilha.Column(iCol).AutoFit()
            Next

            'Coloca o cabeçalho do Bradesco
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Value = "BRADESCO"
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Color.SetColor(Color.White)
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Bold = True
            Planilha.Cells(LINHA_CABECALHO_BRADESCO, COLUNA_INICIAL_DADOS).Style.Font.Size = 22

            'Ajuste da Cor da linha de cabeçalho bradesco das colunas
            For iCol = COLUNA_INICIAL_DADOS To COLUNA_INICIAL_DADOS + iTotalColunasDetalhe - 1
                Planilha.Cells(LINHA_CABECALHO_BRADESCO, iCol).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
                Planilha.Cells(LINHA_CABECALHO_BRADESCO, iCol).Style.Fill.BackgroundColor.SetColor(Color.DarkRed)
            Next iCol

            'Coloca o cabeçalho do relatório
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Value = sCabecalho
            AgrupaCelulaTitulo(LINHA_CABECALHO_RELATORIO)
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Bold = True
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Color.SetColor(Color.DarkBlue)
            Planilha.Cells(LINHA_CABECALHO_RELATORIO, COLUNA_INICIAL_DADOS).Style.Font.Size = 16

            If bFundoBradesco = True Then
                Planilha.BackgroundImage.Image = My.Resources.SuperLV
            End If

            Planilha.View.ShowGridLines = bHabilitaGrade
            Pacote.Workbook.Properties.Title = Me.Titulo
            Pacote.Workbook.Properties.Author = Me.Autor
            Pacote.Workbook.Properties.Comments = Me.Comentario
            Pacote.Workbook.Properties.Company = Me.Compania

            Pacote.Workbook.Properties.SetCustomPropertyValue("Criado em", Now.ToString)

            Pacote.Save()

            If bAbrir = True Then
                Me.Visualizar()
            End If

            Return True

        Catch ex As Exception
            LogaErro("Erro em SuperXLS::Imprimir: " & ex.Message)
            Return False
        Finally
            Pacote.Dispose()
            Planilha = Nothing
            Pacote = Nothing
        End Try

    End Function

    Public Function ImprimirDSColl(ByVal oDataset As SuperDataSet,
                                   ByVal iLinhaDataset As Integer,
                                   ByVal iColunaDataSet As Integer,
                                   ByVal colDados As Collection,
                                   ByVal sNomeRelatorio As String,
                                   Optional ByVal bAbrir As Boolean = False,
                                   Optional ByVal bFundoBradesco As Boolean = True,
                                   Optional ByVal bHabilitaGrade As Boolean = False) As Boolean

        Dim oArquivo As FileInfo
        Dim strCampo As String
        'Dim iLinha As Integer = 1
        'Dim iColuna As Integer = 1
        Dim iRow As Integer
        Dim iCol As Integer
        Dim ValorCampo As Object
        'Dim sCabecalho As String
        Dim iTotalColunasDetalhe As Integer
        Dim sCaminhoRelat As String
        Dim xTemp As sPropriedadeCelula

        Try

            Me.Arquivo = Me.Arquivo & "." & Format(Now, "yyyy.MM.dd-hh.mm.ss") & ".xlsx"

            sCaminhoRelat = Path.Combine(Environment.GetEnvironmentVariable("Temp"), "_TEAD_PRINT")
            ChecaCriaDiretorio(sCaminhoRelat)

            Me.Arquivo = Path.Combine(sCaminhoRelat, Me.Arquivo)

            If ExisteArquivo(Me.Arquivo) Then
                If S_MsgBox("Já existe o arquivo [" & Me.Arquivo & "]." & vbNewLine &
                         "Deseja sobrescrever?.", eBotoes.SimNao, , , eImagens.Interrogacao) = eRet.Nao Then
                    Return False
                Else
                    ApagaArquivo(Me.Arquivo)
                End If
            End If

            oArquivo = New FileInfo(Me.Arquivo)
            Pacote = New ExcelPackage(oArquivo)

            'Ajusta cabeçalho
            'sCabecalho = sNomeRelatorio & " (" & Format(Now, "dd/MM/yyyy hh:mm:ss") & ")"

            Planilha = Pacote.Workbook.Worksheets.Add(Me.Aba)

            'Planilha.Cells("A1:AB1").Style.Font.Bold = True
            iCol = iColunaDataSet

            'Guarda o total de colunas
            iTotalColunasDetalhe = 0

            ''#############################################
            ''#####     FOR pra montar as colunas.    #####
            ''#############################################
            For i = 0 To oDataset.TotalColunas() - 1 'FieldCount() - 1
                strCampo = oDataset.NomeColuna(i)
                If (strCampo.Substring(0, 3) = "as_") Or
                   (strCampo.Substring(0, 3) = "me_") Or
                   (strCampo.Substring(0, 3) = "nu_") Then
                    strCampo = FormataColuna(strCampo)
                    Planilha.Cells(iLinhaDataset, iCol).Value = strCampo
                    Planilha.Cells(iLinhaDataset, iCol).Style.Font.Bold = True
                    Planilha.Cells(iLinhaDataset, iCol).Style.Fill.PatternType = Style.ExcelFillStyle.Solid
                    Planilha.Cells(iLinhaDataset, iCol).Style.Fill.BackgroundColor.SetColor(Color.LightGray)
                    Planilha.Cells(iLinhaDataset, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
                    iCol = iCol + 1
                    iTotalColunasDetalhe += 1
                End If
            Next i

            'If bAutoFiltro = True Then
            'Planilha.Cells(LINHA_CABECALHO_TABELA, COLUNA_INICIAL_DADOS, LINHA_CABECALHO_TABELA, iCol - 1).AutoFilter = bAutoFiltro
            'End If

            'If bCongelarPainel = True Then
            ' Planilha.View.FreezePanes(LINHA_CABECALHO_TABELA + 1, COLUNA_INICIAL_DADOS)
            ' End If

            iRow = iLinhaDataset + 1

            'Dim bBorder = Planilha.Cells(iRow - 1, iColunaDataSet, iRow + oDataset.TotalRegistros() - 1, iColunaDataSet + oDataset.TotalColunas() - 2).Style.Border
            Dim bBorder = Planilha.Cells(iRow - 1, iColunaDataSet, iRow + oDataset.TotalRegistros() - 1, iColunaDataSet + iTotalColunasDetalhe - 1).Style.Border

            bBorder.Bottom.Style = ExcelBorderStyle.Thin
            bBorder.Top.Style = ExcelBorderStyle.Thin
            bBorder.Left.Style = ExcelBorderStyle.Thin
            bBorder.Right.Style = ExcelBorderStyle.Thin


            '################################################
            '#####     FOR pra Preencher as colunas.    #####
            '################################################
            For posReg = 0 To (oDataset.TotalRegistros() - 1) Step 1
                iCol = iColunaDataSet
                For i = 0 To (oDataset.TotalColunas() - 1)

                    strCampo = oDataset.NomeColuna(i)

                    If (strCampo.Substring(0, 3) = "as_") Or
                       (strCampo.Substring(0, 3) = "me_") Or
                       (strCampo.Substring(0, 3) = "nu_") Then


                        If (strCampo.Substring(0, 3) = "me_") Then
                            Planilha.Cells(iRow, iCol).Style.Numberformat.Format = "#,##0.00"
                        End If

                        ValorCampo = oDataset.ValorCampo(i, posReg)

                        Planilha.Cells(iRow, iCol).Value = ValorCampo

                        If (oDataset.TipoDadosColuna(i) Is GetType(Decimal)) Or
                            (oDataset.TipoDadosColuna(i) Is GetType(Integer)) Then
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Right
                        ElseIf (oDataset.TipoDadosColuna(i) Is GetType(DateTime)) Then
                            Planilha.Cells(iRow, iCol).Style.Numberformat.Format = "dd/MM/yyyy"
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
                        Else
                            Planilha.Cells(iRow, iCol).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Left
                        End If

                        iCol = iCol + 1
                    End If
                Next i
                iRow = iRow + 1
            Next posReg

            'Se veio algo...
            If Not colDados Is Nothing Then

                '####INICIO LOOP PERCORRE COLLECTION#########################################
                For i = 1 To colDados.Count Step 1

                    xTemp = CType(colDados.Item(i), sPropriedadeCelula)

                    'Pega a coluna maxima com dados
                    If iTotalColunasDetalhe < xTemp.Coluna Then
                        iTotalColunasDetalhe = xTemp.Coluna
                    End If


                    Planilha.Cells(xTemp.Linha, xTemp.Coluna).Value = xTemp.Valor
                    Planilha.Cells(xTemp.Linha, xTemp.Coluna).Style.Font.Color.SetColor(xTemp.Cor)

                    Planilha.Cells(xTemp.Linha, xTemp.Coluna).Style.Font.Size = xTemp.Fonte.Size
                    Planilha.Cells(xTemp.Linha, xTemp.Coluna).Style.Font.Name = xTemp.Fonte.Name
                    Planilha.Cells(xTemp.Linha, xTemp.Coluna).Style.Font.Bold = xTemp.Negrito


                    If xTemp.Numberformat <> "" Then
                        Planilha.Cells(xTemp.Linha, xTemp.Coluna).Style.Numberformat.Format = xTemp.Numberformat
                    End If



                    If xTemp.Alinhamento <> sPropriedadeCelula.eAlinhamento.General Then
                        Planilha.Cells(xTemp.Linha, xTemp.Coluna).Style.HorizontalAlignment = CType(xTemp.Alinhamento, ExcelHorizontalAlignment)
                    End If

                    If xTemp.Titulo = True Then
                        AgrupaCelulaTitulo(xTemp.Linha)
                    End If

                    'Planilha.Cells(xTemp.Linha, xTemp.Coluna).????? = xTemp.TamanhoCelula

                Next i
                '####FIM LOOP PERCORRE COLLECTION#########################################
            End If

            ''Autoajuste das colunas
            For iCol = iColunaDataSet To iColunaDataSet + iTotalColunasDetalhe - 1
                Planilha.Column(iCol).AutoFit()
            Next


            If bFundoBradesco = True Then
                Planilha.BackgroundImage.Image = My.Resources.SuperLV
            End If

            Planilha.View.ShowGridLines = bHabilitaGrade
            Pacote.Workbook.Properties.Title = Me.Titulo
            Pacote.Workbook.Properties.Author = Me.Autor
            Pacote.Workbook.Properties.Comments = Me.Comentario
            Pacote.Workbook.Properties.Company = Me.Compania

            Pacote.Workbook.Properties.SetCustomPropertyValue("Criado em", Now.ToString)

            Pacote.Save()

            If bAbrir = True Then
                Me.Visualizar()
            End If

            Return True

        Catch ex As Exception
            LogaErro("Erro em SuperXLS::Imprimir: " & ex.Message)
            Return False
        Finally
            Pacote.Dispose()
            Planilha = Nothing
            Pacote = Nothing
        End Try

    End Function


    Function NumeroParaColuna(ByVal Numero As Integer) As String
        Try

            Numero = Numero - 1

            If Numero < 0 Or Numero >= 27 * 26 Then
                NumeroParaColuna = "-" 'Invalido, retorna nada
            Else
                If Numero < 26 Then 'uma letra, apenas retorna a letra corresp.
                    NumeroParaColuna = Chr(Numero + 65)
                Else 'duas letras, obtem letra baseado no modulo e divisao de inteiro
                    NumeroParaColuna = Chr(Numero \ 26 + 64) + Chr(Numero Mod 26 + 65)
                End If
            End If
        Catch ex As Exception
            LogaErro("Erro em SuperXLS::NumeroParaColuna: " & ex.Message)
            Return ""
        End Try
    End Function

    Sub Visualizar()
        Process.Start(Me.Arquivo)
    End Sub


    Private Function FormataColunaImp(ByVal sString As String,
                                      Optional ByVal id As Integer = 0) As String


        Try

            sString = sString.Substring(3, sString.Length - 3)
            sString = sString.Replace("_", " ")

            Return sString

        Catch ex As Exception
            LogaErro("Erro em Util::Formata_Coluna: " & ex.ToString())
            Return ""
        End Try

    End Function


    'se id <> 0, a coluna eh do tipo id_ (codigo)
    Private Function FormataColuna(ByVal sString As String,
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

    Private Sub AgrupaCelulaTitulo(ByVal linha As Integer)

        Dim posColLinhaInicial As String
        Dim posColunaFinal As String
        Try

            ' retorna a coluna e linha do titulo(A1).
            posColLinhaInicial = Planilha.Dimension.Start.Address.ToString
            ' retorna a última coluna preenchida no excel(A).
            posColunaFinal = ConLetra(Planilha.Dimension.End.Column)
            'posColunaFinal = Planilha.Dimension.End.Address.First.ToString'' não pega coluna com dua ou mais letra AA, AAA, etc
            'Planilha.Cells("A1:A2").Merge = True
            Planilha.Cells(posColLinhaInicial & ":" & posColunaFinal & CStr(linha)).Merge = True
            Planilha.Cells(posColLinhaInicial & ":" & posColunaFinal & CStr(linha)).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center


        Catch ex As Exception
            LogaErro("Erro em SuperXLS::AgrupaCelulaTitulo: " & ex.Message)
        End Try
    End Sub

    Private Sub AgrupaCelulaTitulo2(ByVal linha As Integer)

        Dim posColLinhaInicial As String
        Dim posColunaFinal As String
        Try

            ' retorna a coluna e linha do titulo(A1).
            posColLinhaInicial = ConLetra(Planilha.Dimension.Start.Column)
            ' retorna a última coluna preenchida no excel(A).
            posColunaFinal = ConLetra(Planilha.Dimension.End.Column)
            'posColunaFinal = Planilha.Dimension.End.Address.First.ToString'' não pega coluna com dua ou mais letra AA, AAA, etc
            'Planilha.Cells("A1:A2").Merge = True
            Planilha.Cells(posColLinhaInicial & CStr(linha) & ":" & posColunaFinal & CStr(linha)).Merge = True
            Planilha.Cells(posColLinhaInicial & CStr(linha) & ":" & posColunaFinal & CStr(linha)).Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center


        Catch ex As Exception
            LogaErro("Erro em SuperXLS::AgrupaCelulaTitulo: " & ex.Message)
        End Try
    End Sub
    Protected Overrides Sub Finalize()

        If Not Pacote Is Nothing Then
            Pacote.Dispose()
        End If
        Pacote = Nothing

        Planilha = Nothing

        MyBase.Finalize()

        Lixeiro()

    End Sub

    Public Sub Lixeiro()
        GC.SuppressFinalize(Me)
        GC.Collect()
    End Sub

    Public Enum eStatusFianca
        DESCONHECIDO = 0
        ATIVO = 1
        BAIXADA = 2
    End Enum

    Public Enum e_cSttusComis
        AGUARDANDO_PROC = 1
        PAGA = 2
        DEVIDA = 3
        PAGTO_PARCIAL = 4
        PAGTO_SD_ACRESCIDO_DE_MORA = 5
        PREVISTA = 6
    End Enum

    Private Sub CelulaDetalhada(ByVal _nLinha As Integer,
                                ByVal _Coluna As Integer,
                                ByVal _Texto As String,
                                ByVal _Fonte As String,
                                ByVal _TamFonte As Single,
                                ByVal _Negrito As Boolean,
                                ByVal _CorFonte As Color,
                                ByVal _CorFundo As Color,
                                ByVal _Alinhamento As ExcelHorizontalAlignment,
                                Optional ByVal _Formato As String = "")

        Planilha.Cells(_nLinha, _Coluna).Value = _Texto
        Planilha.Cells(_nLinha, _Coluna).Style.Font.Name = _Fonte
        Planilha.Cells(_nLinha, _Coluna).Style.Font.Size = _TamFonte
        Planilha.Cells(_nLinha, _Coluna).Style.Font.Bold = _Negrito
        Planilha.Cells(_nLinha, _Coluna).Style.Font.Color.SetColor(_CorFonte)
        Planilha.Cells(_nLinha, _Coluna).Style.Fill.PatternType = ExcelFillStyle.Solid
        Planilha.Cells(_nLinha, _Coluna).Style.Fill.BackgroundColor.SetColor(_CorFundo)
        Planilha.Cells(_nLinha, _Coluna).Style.WrapText = True

        Planilha.Cells(_nLinha, _Coluna).Style.VerticalAlignment = ExcelVerticalAlignment.Top

        Planilha.Cells(_nLinha, _Coluna).Style.HorizontalAlignment = _Alinhamento

        If _Formato <> "" Then
            Planilha.Cells(_nLinha, _Coluna).Style.Numberformat.Format = _Formato
        End If


    End Sub

    Public Shared Function CorrigeWidth(width As Double) As Double

        'Nao copiei da internet, magina....

        'DEDUCE WHAT THE COLUMN WIDTH WOULD REALLY GET SET TO
        Dim z As Double = 1.0
        If width >= (1 + 2 / 3) Then
            z = Math.Round((Math.Round(7 * (width - 1 / 256), 0) - 5) / 7, 2)
        Else
            z = Math.Round((Math.Round(12 * (width - 1 / 256), 0) - Math.Round(5 * width, 0)) / 12, 2)
        End If

        'HOW FAR OFF? (WILL BE LESS THAN 1)
        Dim errorAmt As Double = width - z

        'CALCULATE WHAT AMOUNT TO TACK ONTO THE ORIGINAL AMOUNT TO RESULT IN THE CLOSEST POSSIBLE SETTING 
        Dim adj As Double = 0.0
        If width >= (1 + 2 / 3) Then
            adj = (Math.Round(7 * errorAmt - 7 / 256, 0)) / 7
        Else
            adj = ((Math.Round(12 * errorAmt - 12 / 256, 0)) / 12) + (2 / 12)
        End If

        'RETURN A SCALED-VALUE THAT SHOULD RESULT IN THE NEAREST POSSIBLE VALUE TO THE TRUE DESIRED SETTING
        If z > 0 Then
            Return width + adj
        End If

        Return 0.0

    End Function


End Class
