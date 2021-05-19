'Classe criada automaticamente em 14/05/2021 20:08:29
Option Strict On
Option Explicit On
Option Infer On

Imports GFT.Util
Imports GFT.Util.BancoDados


Public Class pFichaImportacao

    'Operacao
    Public Shared OPERACAO As Campo = New Campo("OPERACAO", DbType.String, 4)
    'Estrutura da Tabela tFichaImportacao
    Class tFichaImportacao
        Public Shared cFichaImportacao As campo = New campo("cFichaImportacao", DbType.Decimal, 8, 0)
        Public Shared nm_pais_origem As campo = New campo("nm_pais_origem", DbType.String, 255)
        Public Shared qtd_comerc As Campo = New Campo("qtd_comerc", DbType.Decimal, 22, 6)
        Public Shared importador_endereco As campo = New campo("importador_endereco", DbType.String, 1000)
        Public Shared nm_pais_aquis As campo = New campo("nm_pais_aquis", DbType.String, 255)
        Public Shared via_transp As campo = New campo("via_transp", DbType.String, 255)
        Public Shared nome_unid_desembaraco As campo = New campo("nome_unid_desembaraco", DbType.String, 255)
        Public Shared anomes As campo = New campo("anomes", DbType.Decimal, 8, 0)
        Public Shared id_import As Campo = New Campo("id_import", DbType.Decimal, 22, 0)
        Public Shared val_fob_us_subitem As Campo = New Campo("val_fob_us_subitem", DbType.Decimal, 22, 6)
        Public Shared exportador_nome As campo = New campo("exportador_nome", DbType.String, 255)
        Public Shared desc_prodt As campo = New campo("desc_prodt", DbType.String, 4000)
        Public Shared tp_unid_comerc As campo = New campo("tp_unid_comerc", DbType.String, 255)
        Public Shared incoterm As campo = New campo("incoterm", DbType.String, 255)
        Public Shared importador_cnpj As campo = New campo("importador_cnpj", DbType.String, 22)
        Public Shared val_cif_un_us As Campo = New Campo("val_cif_un_us", DbType.Decimal, 22, 6)
        Public Shared val_fob_un_us As Campo = New Campo("val_fob_un_us", DbType.Decimal, 22, 6)
        Public Shared val_vmld_us_subitem As Campo = New Campo("val_vmld_us_subitem", DbType.Decimal, 22, 6)
        Public Shared cdncm_compl As Campo = New Campo("cdncm_compl", DbType.Decimal, 22, 0)
        Public Shared val_peso_liq_subitem As Campo = New Campo("val_peso_liq_subitem", DbType.Decimal, 22, 6)
        Public Shared importador_nome As campo = New campo("importador_nome", DbType.String, 255)
        Public Shared nome_adquirente As campo = New campo("nome_adquirente", DbType.String, 255)
        Public Shared val_frete_un_us As Campo = New Campo("val_frete_un_us", DbType.Decimal, 22, 6)
        Public Shared val_frete_us_subitem As Campo = New Campo("val_frete_us_subitem", DbType.Decimal, 22, 6)
        Public Shared val_seg_un_us As Campo = New Campo("val_seg_un_us", DbType.Decimal, 22, 6)
        Public Shared val_seg_us_subitem As Campo = New Campo("val_seg_us_subitem", DbType.Decimal, 22, 6)
        Public Shared cidade_import As campo = New campo("cidade_import", DbType.String, 255)
    End Class

    '01-Incluir
    Public Shared Function Incluir(ByVal _nm_pais_origem As String,
                                   ByVal _qtd_comerc As Decimal,
                                   ByVal _importador_endereco As String,
                                   ByVal _nm_pais_aquis As String,
                                   ByVal _via_transp As String,
                                   ByVal _nome_unid_desembaraco As String,
                                   ByVal _anomes As Decimal,
                                   ByVal _id_import As Decimal,
                                   ByVal _val_fob_us_subitem As Decimal,
                                   ByVal _exportador_nome As String,
                                   ByVal _desc_prodt As String,
                                   ByVal _tp_unid_comerc As String,
                                   ByVal _incoterm As String,
                                   ByVal _importador_cnpj As String,
                                   ByVal _val_cif_un_us As Decimal,
                                   ByVal _val_fob_un_us As Decimal,
                                   ByVal _val_vmld_us_subitem As Decimal,
                                   ByVal _cdncm_compl As Decimal,
                                   ByVal _val_peso_liq_subitem As Decimal,
                                   ByVal _importador_nome As String,
                                   ByVal _nome_adquirente As String,
                                   ByVal _val_frete_un_us As Decimal,
                                   ByVal _val_frete_us_subitem As Decimal,
                                   ByVal _val_seg_un_us As Decimal,
                                   ByVal _val_seg_us_subitem As Decimal,
                                   ByVal _cidade_import As String) As Boolean

        Dim bDados As BancoDados

        Try


            bDados = New BancoDados()

            bDados.LimpaParametros()
            bDados.AdicionaParametro(OPERACAO, "INSE")
            bDados.AdicionaParametro(tFichaImportacao.cFichaImportacao, Nothing)
            bDados.AdicionaParametro(tFichaImportacao.nm_pais_origem, _nm_pais_origem)
            bDados.AdicionaParametro(tFichaImportacao.qtd_comerc, _qtd_comerc)
            bDados.AdicionaParametro(tFichaImportacao.importador_endereco, _importador_endereco)
            bDados.AdicionaParametro(tFichaImportacao.nm_pais_aquis, _nm_pais_aquis)
            bDados.AdicionaParametro(tFichaImportacao.via_transp, _via_transp)
            bDados.AdicionaParametro(tFichaImportacao.nome_unid_desembaraco, _nome_unid_desembaraco)
            bDados.AdicionaParametro(tFichaImportacao.anomes, _anomes)
            bDados.AdicionaParametro(tFichaImportacao.id_import, _id_import)
            bDados.AdicionaParametro(tFichaImportacao.val_fob_us_subitem, _val_fob_us_subitem)
            bDados.AdicionaParametro(tFichaImportacao.exportador_nome, _exportador_nome)
            bDados.AdicionaParametro(tFichaImportacao.desc_prodt, _desc_prodt)
            bDados.AdicionaParametro(tFichaImportacao.tp_unid_comerc, _tp_unid_comerc)
            bDados.AdicionaParametro(tFichaImportacao.incoterm, _incoterm)
            bDados.AdicionaParametro(tFichaImportacao.importador_cnpj, _importador_cnpj)
            bDados.AdicionaParametro(tFichaImportacao.val_cif_un_us, _val_cif_un_us)
            bDados.AdicionaParametro(tFichaImportacao.val_fob_un_us, _val_fob_un_us)
            bDados.AdicionaParametro(tFichaImportacao.val_vmld_us_subitem, _val_vmld_us_subitem)
            bDados.AdicionaParametro(tFichaImportacao.cdncm_compl, _cdncm_compl)
            bDados.AdicionaParametro(tFichaImportacao.val_peso_liq_subitem, _val_peso_liq_subitem)
            bDados.AdicionaParametro(tFichaImportacao.importador_nome, _importador_nome)
            bDados.AdicionaParametro(tFichaImportacao.nome_adquirente, _nome_adquirente)
            bDados.AdicionaParametro(tFichaImportacao.val_frete_un_us, _val_frete_un_us)
            bDados.AdicionaParametro(tFichaImportacao.val_frete_us_subitem, _val_frete_us_subitem)
            bDados.AdicionaParametro(tFichaImportacao.val_seg_un_us, _val_seg_un_us)
            bDados.AdicionaParametro(tFichaImportacao.val_seg_us_subitem, _val_seg_us_subitem)
            bDados.AdicionaParametro(tFichaImportacao.cidade_import, _cidade_import)
            '---------------------------------------------------------------------------------------------------
            bDados.Executar("pFichaImportacao")

            If bDados.ObterUltimoErro = "" Then 'Sucesso!
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            'LogaErro("Erro em " & NomeMetodo("pFichaImportacao") & ": " & ex.Message)
            Return False
        Finally
            bDados = Nothing
        End Try

    End Function

    '02-Alterar
    Public Shared Function Alterar(ByVal _cFichaImportacao As Decimal,
                                    ByVal _nm_pais_origem As String,
                                    ByVal _qtd_comerc As Decimal,
                                    ByVal _importador_endereco As String,
                                    ByVal _nm_pais_aquis As String,
                                    ByVal _via_transp As String,
                                    ByVal _nome_unid_desembaraco As String,
                                    ByVal _anomes As Decimal,
                                    ByVal _id_import As Decimal,
                                    ByVal _val_fob_us_subitem As String,
                                    ByVal _exportador_nome As String,
                                    ByVal _desc_prodt As String,
                                    ByVal _tp_unid_comerc As String,
                                    ByVal _incoterm As String,
                                    ByVal _importador_cnpj As String,
                                    ByVal _val_cif_un_us As String,
                                    ByVal _val_fob_un_us As String,
                                    ByVal _val_vmld_us_subitem As String,
                                    ByVal _cdncm_compl As Decimal,
                                    ByVal _val_peso_liq_subitem As String,
                                    ByVal _importador_nome As String,
                                    ByVal _nome_adquirente As String,
                                    ByVal _val_frete_un_us As Decimal,
                                    ByVal _val_frete_us_subitem As Decimal,
                                    ByVal _val_seg_un_us As Decimal,
                                    ByVal _val_seg_us_subitem As Decimal,
                                    ByVal _cidade_import As String) As Boolean

        Dim bDados As BancoDados

        Try

            bDados = New BancoDados()

            bDados.LimpaParametros()
            bDados.AdicionaParametro(OPERACAO, "ALTE")
            bDados.AdicionaParametro(tFichaImportacao.cFichaImportacao, _cFichaImportacao)
            bDados.AdicionaParametro(tFichaImportacao.nm_pais_origem, _nm_pais_origem)
            bDados.AdicionaParametro(tFichaImportacao.qtd_comerc, _qtd_comerc)
            bDados.AdicionaParametro(tFichaImportacao.importador_endereco, _importador_endereco)
            bDados.AdicionaParametro(tFichaImportacao.nm_pais_aquis, _nm_pais_aquis)
            bDados.AdicionaParametro(tFichaImportacao.via_transp, _via_transp)
            bDados.AdicionaParametro(tFichaImportacao.nome_unid_desembaraco, _nome_unid_desembaraco)
            bDados.AdicionaParametro(tFichaImportacao.anomes, _anomes)
            bDados.AdicionaParametro(tFichaImportacao.id_import, _id_import)
            bDados.AdicionaParametro(tFichaImportacao.val_fob_us_subitem, _val_fob_us_subitem)
            bDados.AdicionaParametro(tFichaImportacao.exportador_nome, _exportador_nome)
            bDados.AdicionaParametro(tFichaImportacao.desc_prodt, _desc_prodt)
            bDados.AdicionaParametro(tFichaImportacao.tp_unid_comerc, _tp_unid_comerc)
            bDados.AdicionaParametro(tFichaImportacao.incoterm, _incoterm)
            bDados.AdicionaParametro(tFichaImportacao.importador_cnpj, _importador_cnpj)
            bDados.AdicionaParametro(tFichaImportacao.val_cif_un_us, _val_cif_un_us)
            bDados.AdicionaParametro(tFichaImportacao.val_fob_un_us, _val_fob_un_us)
            bDados.AdicionaParametro(tFichaImportacao.val_vmld_us_subitem, _val_vmld_us_subitem)
            bDados.AdicionaParametro(tFichaImportacao.cdncm_compl, _cdncm_compl)
            bDados.AdicionaParametro(tFichaImportacao.val_peso_liq_subitem, _val_peso_liq_subitem)
            bDados.AdicionaParametro(tFichaImportacao.importador_nome, _importador_nome)
            bDados.AdicionaParametro(tFichaImportacao.nome_adquirente, _nome_adquirente)
            bDados.AdicionaParametro(tFichaImportacao.val_frete_un_us, _val_frete_un_us)
            bDados.AdicionaParametro(tFichaImportacao.val_frete_us_subitem, _val_frete_us_subitem)
            bDados.AdicionaParametro(tFichaImportacao.val_seg_un_us, _val_seg_un_us)
            bDados.AdicionaParametro(tFichaImportacao.val_seg_us_subitem, _val_seg_us_subitem)
            bDados.AdicionaParametro(tFichaImportacao.cidade_import, _cidade_import)

            bDados.Executar("pFichaImportacao")

            If bDados.ObterUltimoErro = "" Then 'Sucesso!
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            'LogaErro("Erro em " & NomeMetodo("pFichaImportacao") & ": " & ex.Message)
            Return False
        Finally
            bDados = Nothing
        End Try
    End Function

    '03-Excluir
    Public Shared Function Excluir(ByVal _cFichaImportacao As Decimal) As Boolean

        Dim bDados As BancoDados

        Try

            bDados = New BancoDados()

            bDados.LimpaParametros()
            bDados.AdicionaParametro(OPERACAO, "DELE")
            bDados.AdicionaParametro(tFichaImportacao.cFichaImportacao, _cFichaImportacao)

            bDados.Executar("pFichaImportacao")

            If bDados.ObterUltimoErro = "" Then 'Sucesso!
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            'LogaErro("Erro em " & NomeMetodo("pSoltc") & ": " & ex.Message)
            Return False
        Finally
            bDados = Nothing
        End Try
    End Function

    '04-Listar
    Public Shared Function Listar(ByVal _cFichaImportacao As Decimal) As SuperDataSet

        Dim rsRet As SuperDataSet
        Dim bDados As BancoDados

        Try

            bDados = New BancoDados()

            bDados.LimpaParametros()
            bDados.AdicionaParametro(OPERACAO, "LIST")
            bDados.AdicionaParametro(tFichaImportacao.cFichaImportacao, _cFichaImportacao)

            rsRet = bDados.Obter("pFichaImportacao")

            Return rsRet

        Catch ex As Exception
            'LogaErro("Erro em " & NomeMetodo("pFichaImportacao") & ": " & ex.Message)
            Return Nothing
        Finally
            rsRet = Nothing
        End Try

    End Function

    Friend Shared Function ObterDadosGrid(ByVal cdncm_compl As String,
                                          ByVal desc_prodt As String,
                                          ByVal incoterm As String,
                                          ByVal importador_nome As String,
                                          ByVal exportador_nome As String,
                                          ByVal nm_pais_aquis As String,
                                          ByVal nm_pais_origem As String) As SuperDataSet

        Dim rsRet As SuperDataSet
        Dim bDados As BancoDados

        Try

            bDados = New BancoDados()

            bDados.LimpaParametros()
            bDados.AdicionaParametro(OPERACAO, "GRID")

            If incoterm.Trim = "" OrElse IsNumeric(cdncm_compl) = False Then
                bDados.AdicionaParametro(tFichaImportacao.cdncm_compl, DBNull.Value) 'decimal
            Else
                bDados.AdicionaParametro(tFichaImportacao.cdncm_compl, CDec(cdncm_compl)) 'decimal
            End If

            If desc_prodt.Trim = "" Then
                bDados.AdicionaParametro(tFichaImportacao.desc_prodt, DBNull.Value)
            Else
                bDados.AdicionaParametro(tFichaImportacao.desc_prodt, desc_prodt)
            End If

            If incoterm.Trim = "" Then
                bDados.AdicionaParametro(tFichaImportacao.incoterm, DBNull.Value)
            Else
                bDados.AdicionaParametro(tFichaImportacao.incoterm, incoterm)
            End If

            '---------------------------
            If importador_nome.Trim = "" Then
                bDados.AdicionaParametro(tFichaImportacao.importador_nome, DBNull.Value)
            Else
                bDados.AdicionaParametro(tFichaImportacao.importador_nome, importador_nome)
            End If

            If exportador_nome.Trim = "" Then
                bDados.AdicionaParametro(tFichaImportacao.exportador_nome, DBNull.Value)
            Else
                bDados.AdicionaParametro(tFichaImportacao.exportador_nome, exportador_nome)
            End If

            If nm_pais_aquis.Trim = "" Then
                bDados.AdicionaParametro(tFichaImportacao.nm_pais_aquis, DBNull.Value)
            Else
                bDados.AdicionaParametro(tFichaImportacao.nm_pais_aquis, nm_pais_aquis)
            End If

            If nm_pais_origem.Trim = "" Then
                bDados.AdicionaParametro(tFichaImportacao.nm_pais_origem, DBNull.Value)
            Else
                bDados.AdicionaParametro(tFichaImportacao.nm_pais_origem, nm_pais_origem)
            End If



            'nm_pais_aquis
            'nm_pais_origem

            'If nm_pais_aquis.Trim = "" Then
            '    bDados.AdicionaParametro(tFichaImportacao.nm_pais_aquis, "")
            'Else
            'End If
            '
            'If nm_pais_origem.Trim = "" Then
            '    bDados.AdicionaParametro(tFichaImportacao.nm_pais_origem, "")
            'Else
            'End If









            bDados.AdicionaParametro(tFichaImportacao.cFichaImportacao, DBNull.Value)

            rsRet = bDados.Obter("pFichaImportacao")

            Return rsRet

        Catch ex As Exception
            'LogaErro("Erro em " & NomeMetodo("pFichaImportacao") & ": " & ex.Message)
            Return Nothing
        Finally
            rsRet = Nothing
        End Try


    End Function

End Class 'pFichaImportacao
