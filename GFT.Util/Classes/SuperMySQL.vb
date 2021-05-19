Option Explicit On
Option Strict On

#Region "Legal"

'************************************************************************************************************************
' Copyright (c) 2015, Todos direitos reservados, GFT-IT - Serviços de TI - http://www.GFTit.com.br/
'
' Autor........: Carlos Buosi (cbuosi@gmail.com)
' Arquivo......: SuperMySQL.vb
' Tipo.........: Modulo VB.
' Versao.......: 2.02+
' Propósito....: Modulo de banco de dados (MySQL 6.3+).
' Uso..........: Não se aplica
' Produto......: ModuloCentralizador
'
' Legal........: Este código é de propriedade do Banco Bradesco S/A e/ou GFT-IT - Serviços de TI, sua cópia
'                e/ou distribuição é proibida.
'
' GUID.........: {7CC81A98-9E60-4498-9681-7102635D1782}
' Observações..: nenhuma.
'************************************************************************************************************************

#End Region

Imports System
Imports MySql.Data.MySqlClient

#If DEBUG Then
#Const LOGAR_TODOS_PARAMETROS = True
#Else
#Const LOGAR_TODOS_PARAMETROS = False
#End If

Public Class SuperMySQL
    Implements IDisposable

#Const DEBUG_BULK = False

    Private Const TIMEOUT = 10                           'Tempo (em minutos) de time out.
    Private Const MAX_PARAM = 300                        'No maximo 300 parametros por procedimento.
    Private Const SQL_SERVER_DEFAULT_PORT = 1433         'Caso o windows do usuario esteja com a configuracao "danificada".
    Private Const PACKET_SIZE = 4096                     'Tamanho pacote dados da conexão.
    Private Const CMD_TIME_OUT = (TIMEOUT * 60)          '9x60 = 9 minutos.
    Private Const BULK_INSERT_BATCH_SIZE = 5000          'Insere lotes de 2500 registros (evita time out).
    Private Const BULK_INSERT_TIME_OUT = (TIMEOUT * 60)  '20x60 = 20 minutos.

    Private strConexao As String = ""
    Private strServer As String = ""
    Private strDatabase As String = ""
    Private strUserID As String = ""
    Private strPassword As String = ""
    Private strTabelaDestino As String = ""

    Dim ParametrosProc() As MySqlParameter ' DbParameter

    Private strCodErro As String = ""

    Public Sub New(ByVal _strServer As String,
                   ByVal _strDatabase As String,
                   ByVal _strUserID As String,
                   ByVal _strPassword As String)

        strServer = _strServer
        strDatabase = _strDatabase
        strUserID = _strUserID
        strPassword = _strPassword
        LimpaParametros()

    End Sub

    Sub New()
        strServer = ObterConfig("Servidor")
        strDatabase = ObterConfig("Banco")
        strUserID = ObterConfig("Usuario")
        strPassword = Decripta(ObterConfig("Senha"))
        LimpaParametros()
    End Sub

    Structure Campo
        Dim nome As String
        Dim tipo As DbType
        Dim tamanho As Integer
        Dim tamEscala As Integer

        Public Sub New(ByVal _nome As String,
                       ByVal _tipo As DbType,
                       ByVal _tamanho As Integer,
                       Optional ByVal _tamEscala As Integer = 0)
            nome = _nome
            tipo = _tipo
            tamanho = _tamanho
            tamEscala = _tamEscala
        End Sub

        Public Overrides Function ToString() As String
            Return Me.nome
        End Function

    End Structure

    Public Property TabelaDestino() As String
        Get
            Return strTabelaDestino
        End Get
        Set(ByVal value As String)
            strTabelaDestino = value
        End Set
    End Property

    Public Sub LimpaParametros()
        Try
            ReDim ParametrosProc(0)
        Catch ex As Exception
            LogaErro("Erro em SuperSuperMySQL::LimpaParametros: " & ex.Message)
            strCodErro = "SuperSuperMySQL::LimpaParametros: " & ex.Message
        End Try
    End Sub

    Public Sub AdicionaParametro(ByVal _Campo As Campo,
                                 ByVal valor As Object)
        Try
            Dim tamArray As Integer
            tamArray = ParametrosProc.Length - 1

            If (tamArray + 1) > MAX_PARAM Then
                LogaErro("Erro em SuperSuperMySQL::Muitos parametros! :)")
                Exit Sub
            End If

            'Se passar nothing, transforma em nulo.....
            If valor Is Nothing Then
                valor = DBNull.Value
            End If

            'Evita valores maior que campos (string)...
            If _Campo.tipo = DbType.String Then
                If valor.ToString.Length > _Campo.tamanho Then
                    valor = valor.ToString.Substring(0, _Campo.tamanho)
                End If
            End If

            ParametrosProc(tamArray) = New MySqlParameter() ' SqlParameter()
            ParametrosProc(tamArray).ParameterName = _Campo.nome
            ParametrosProc(tamArray).SourceVersion = DataRowVersion.Current
            ParametrosProc(tamArray).SourceColumn = String.Empty
            ParametrosProc(tamArray).SourceColumnNullMapping = False
            ParametrosProc(tamArray).Size = _Campo.tamanho
            ParametrosProc(tamArray).Direction = ParameterDirection.Input
            ParametrosProc(tamArray).DbType = _Campo.tipo
            ParametrosProc(tamArray).Value = valor

            ReDim Preserve ParametrosProc(tamArray + 1)
            '
        Catch ex As Exception
            LogaErro("Erro em SuperSuperMySQL::AdicionaParametro(1): " & ex.Message)
            strCodErro = "SuperSuperMySQL::AdicionaParametro(1): " & ex.Message
        End Try
    End Sub

    Public Sub AdicionaParametro(ByVal nome As String,
                                 ByVal valor As Object,
                                 ByVal tipo As DbType,
                                 ByVal tamanho As Integer,
                                 Optional ByVal tamEscala As Integer = 0)

        Dim tamArray As Integer

        Try

            tamArray = ParametrosProc.Length - 1

            If (tamArray + 1) > MAX_PARAM Then
                LogaErro("Erro em SuperSuperMySQL::Muitos parametros! :)")
                Exit Sub
            End If

            'Evita valores maior que campos (string)...
            If tipo = DbType.String Then
                If valor.ToString.Length > tamanho Then
                    valor = valor.ToString.Substring(0, tamanho)
                End If
            End If

            ParametrosProc(tamArray) = New MySqlParameter()
            ParametrosProc(tamArray).ParameterName = nome
            ParametrosProc(tamArray).SourceVersion = DataRowVersion.Current
            ParametrosProc(tamArray).SourceColumn = String.Empty
            ParametrosProc(tamArray).SourceColumnNullMapping = False
            ParametrosProc(tamArray).Size = tamanho
            ParametrosProc(tamArray).Direction = ParameterDirection.Input
            ParametrosProc(tamArray).DbType = tipo
            ParametrosProc(tamArray).Value = valor

            ReDim Preserve ParametrosProc(tamArray + 1)
        Catch ex As Exception
            LogaErro("Erro em SuperSuperMySQL::AdicionaParametro: " & ex.Message)
            strCodErro = "SuperSuperMySQL::AdicionaParametro: " & ex.Message
        End Try
    End Sub

    Private Function ObterConnectionString() As String

        Try

            'Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;

            Return "Server=" & strServer &
                   ";Database=" & strDatabase &
                   ";Uid=" & strUserID &
                   ";Pwd=" & strPassword & ";Character Set=utf8"
        Catch ex As Exception
            LogaErro("Erro em SuperSuperMySQL::ObterConnectionString: " & ex.Message)
            strCodErro = "SuperSuperMySQL::ObterConnectionString: " & ex.Message
            Return ""
        End Try
    End Function

    Public Function ObterQuery(ByVal txtQuery As String) As SuperDataSet

        Dim con As MySqlConnection = Nothing ' SqlConnection = Nothing
        Dim cmd As MySqlCommand = Nothing ' SqlCommand = Nothing
        Dim dap As MySqlDataAdapter = Nothing ' SqlDataAdapter = Nothing
        Dim oRelogio As Stopwatch = Nothing
        Dim oDataSet As SuperDataSet = Nothing

        Try

            Cursor.Current = CURSOR_OCUPADO

            strCodErro = ""

            oDataSet = New SuperDataSet
            oRelogio = New Stopwatch

            oRelogio.Start()

            con = New MySqlConnection(ObterConnectionString())
            cmd = New MySqlCommand(txtQuery, con)
            cmd.CommandType = CommandType.Text

            For Each DbParameter In ParametrosProc
                If Not DbParameter Is Nothing Then
                    cmd.Parameters.Add(DbParameter)
                End If
            Next

            dap = New MySqlDataAdapter(txtQuery, con)
            cmd.CommandTimeout = CMD_TIME_OUT
            dap.SelectCommand = cmd

            con.Open()
            dap.Fill(oDataSet)

            oRelogio.Stop()
            oDataSet.InfoPesquisa = oDataSet.TotalRegistros().ToString("0,0#") & " registro(s). " & (oRelogio.ElapsedMilliseconds / 1000).ToString & " segundo(s)."

            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtQuery & " [SEM PARÂMETROS]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs. Registros: [" & oDataSet.TotalRegistros & "].")
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtQuery & " [" & listarParametros() & "]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs. Registros: [" & oDataSet.TotalRegistros & "].")
#Else
                LogaErro(txtQuery & " [" & ParametrosProc(0).Value.ToString() & "]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs. Registros: [" & oDataSet.TotalRegistros & "].")
#End If
            End If

            con.Close()

            Return oDataSet
        Catch ex As MySqlException ' SqlException
            'For i = 0 To ex.e.Errors.Count - 1
            '    strCodErro += " Message: " & ex.Errors(i).Message & " Line#:" & ex.Errors(i).LineNumber.ToString & " Src:" & ex.Errors(i).Source & " Proc:" & ex.Errors(i).Procedure
            'Next i
            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtQuery & " [SEM PARÂMETROS]... Erro: " & strCodErro)
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtQuery & " [" & listarParametros() & "]... Erro: " & strCodErro)
#Else
                LogaErro(txtQuery & " [" & ParametrosProc(0).Value.ToString() & "]... Erro: " & strCodErro)
#End If
            End If
            oDataSet.InfoPesquisa = strCodErro
            Return Nothing
        Catch ex As Exception
            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtQuery & " [SEM PARÂMETROS]... Erro: " & ex.Message)
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtQuery & " [" & listarParametros() & "]... Erro: " & ex.Message)
#Else
                LogaErro(txtQuery & " [" & ParametrosProc(0).Value.ToString() & "]... Erro: " & ex.Message)
#End If
            End If
            strCodErro = "SuperSuperMySQL::Obter: [" & strServer & "] " & ex.Message
            oDataSet.InfoPesquisa = strCodErro
            Return Nothing
        Finally
            If Not con Is Nothing Then
                con.Close()
                con.Dispose()
                con = Nothing
            End If
            If Not oDataSet Is Nothing Then
                oDataSet.Dispose()
                oDataSet = Nothing
            End If
            If Not dap Is Nothing Then
                dap.Dispose()
                dap = Nothing
            End If
            If Not cmd Is Nothing Then
                cmd.Dispose()
                cmd = Nothing
            End If
            oRelogio = Nothing
        End Try
    End Function

    Public Function ExecutarQuery(ByVal txtQuery As String) As Boolean

        Dim con As MySqlConnection = Nothing ' SqlConnection = Nothing
        Dim cmd As MySqlCommand = Nothing ' SqlCommand = Nothing
        Dim dap As MySqlDataAdapter = Nothing ' SqlDataAdapter = Nothing
        Dim oRelogio As Stopwatch = Nothing
        'Dim oDataSet As SuperDataSet = Nothing

        Try

            Cursor.Current = CURSOR_OCUPADO

            strCodErro = ""

            'oDataSet = New SuperDataSet
            oRelogio = New Stopwatch

            oRelogio.Start()

            con = New MySqlConnection(ObterConnectionString())
            cmd = New MySqlCommand(txtQuery, con)
            cmd.CommandType = CommandType.Text

            For Each DbParameter In ParametrosProc
                If Not DbParameter Is Nothing Then
                    cmd.Parameters.Add(DbParameter)
                End If
            Next

            dap = New MySqlDataAdapter(txtQuery, con)
            cmd.CommandTimeout = CMD_TIME_OUT
            dap.SelectCommand = cmd

            con.Open()

            cmd.CommandTimeout = CMD_TIME_OUT
            cmd.ExecuteNonQuery()

            'dap.Fill(oDataSet)

            oRelogio.Stop()
            'oDataSet.InfoPesquisa = oDataSet.TotalRegistros().ToString("0,0#") & " registro(s). " & (oRelogio.ElapsedMilliseconds / 1000).ToString & " segundo(s)."

            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtQuery & " [SEM PARÂMETROS]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs.")
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtQuery & " [" & listarParametros() & "]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs.")
#Else
                LogaErro(txtQuery & " [" & ParametrosProc(0).Value.ToString() & "]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs.")
#End If
            End If

            con.Close()

            Return True
        Catch ex As MySqlException ' SqlException
            'For i = 0 To ex.Errors.Count - 1
            '    strCodErro += " Message: " & ex.Errors(i).Message & " Line#:" & ex.Errors(i).LineNumber.ToString & " Src:" & ex.Errors(i).Source & " Proc:" & ex.Errors(i).Procedure
            'Next i
            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtQuery & " [SEM PARÂMETROS]... Erro: " & strCodErro)
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtQuery & " [" & listarParametros() & "]... Erro: " & strCodErro)
#Else
                LogaErro(txtQuery & " [" & ParametrosProc(0).Value.ToString() & "]... Erro: " & strCodErro)
#End If
            End If
            'oDataSet.InfoPesquisa = strCodErro
            Return False
        Catch ex As Exception
            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtQuery & " [SEM PARÂMETROS]... Erro: " & ex.Message)
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtQuery & " [" & listarParametros() & "]... Erro: " & ex.Message)
#Else
                LogaErro(txtQuery & " [" & ParametrosProc(0).Value.ToString() & "]... Erro: " & ex.Message)
#End If
            End If
            strCodErro = "SuperSuperMySQL::Obter: [" & strServer & "] " & ex.Message
            'oDataSet.InfoPesquisa = strCodErro
            Return False
        Finally
            If Not con Is Nothing Then
                con.Close()
                con.Dispose()
                con = Nothing
            End If
            'If Not oDataSet Is Nothing Then
            '    oDataSet.Dispose()
            '    oDataSet = Nothing
            'End If
            If Not dap Is Nothing Then
                dap.Dispose()
                dap = Nothing
            End If
            If Not cmd Is Nothing Then
                cmd.Dispose()
                cmd = Nothing
            End If
            oRelogio = Nothing
        End Try
    End Function

    Public Function Obter(ByVal txtProcedure As String) As SuperDataSet

        Dim con As MySqlConnection = Nothing ' SqlConnection = Nothing
        Dim cmd As MySqlCommand = Nothing ' SqlCommand = Nothing
        Dim dap As MySqlDataAdapter = Nothing ' SqlDataAdapter = Nothing
        Dim oRelogio As Stopwatch = Nothing
        Dim oDataSet As SuperDataSet = Nothing

        Try

            Cursor.Current = CURSOR_OCUPADO

            strCodErro = ""

            oDataSet = New SuperDataSet
            oRelogio = New Stopwatch

            oRelogio.Start()

            con = New MySqlConnection(ObterConnectionString())
            cmd = New MySqlCommand(txtProcedure, con)
            cmd.CommandType = CommandType.StoredProcedure

            For Each DbParameter In ParametrosProc
                If Not DbParameter Is Nothing Then
                    cmd.Parameters.Add(DbParameter)
                End If
            Next

            dap = New MySqlDataAdapter(txtProcedure, con)
            cmd.CommandTimeout = CMD_TIME_OUT
            dap.SelectCommand = cmd

            con.Open()
            dap.Fill(oDataSet)

            oRelogio.Stop()
            oDataSet.InfoPesquisa = oDataSet.TotalRegistros().ToString("0,0#") & " registro(s). " & (oRelogio.ElapsedMilliseconds / 1000).ToString & " segundo(s)."

            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtProcedure & " [SEM PARÂMETROS]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs. Registros: [" & oDataSet.TotalRegistros & "].")
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtProcedure & " [" & listarParametros() & "]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs. Registros: [" & oDataSet.TotalRegistros & "].")
#Else
                LogaErro(txtProcedure & " [" & ParametrosProc(0).Value.ToString() & "]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs. Registros: [" & oDataSet.TotalRegistros & "].")
#End If
            End If

            con.Close()

            Return oDataSet
        Catch ex As MySqlException ' SqlException

            If Not ex.InnerException Is Nothing Then
                strCodErro = ex.InnerException.ToString
            Else
                strCodErro = ex.Message
            End If

            'For i = 0 To ex.Errors.Count - 1
            '    strCodErro += " Message: " & ex.Errors(i).Message & " Line#:" & ex.Errors(i).LineNumber.ToString & " Src:" & ex.Errors(i).Source & " Proc:" & ex.Errors(i).Procedure
            'Next i
            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtProcedure & " [SEM PARÂMETROS]... Erro: " & strCodErro)
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtProcedure & " [" & listarParametros() & "]... Erro: " & strCodErro)
#Else
                LogaErro(txtProcedure & " [" & ParametrosProc(0).Value.ToString() & "]... Erro: " & strCodErro)
#End If
            End If
            oDataSet.InfoPesquisa = strCodErro
            Return Nothing
        Catch ex As Exception
            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtProcedure & " [SEM PARÂMETROS]... Erro: " & ex.Message)
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtProcedure & " [" & listarParametros() & "]... Erro: " & ex.Message)
#Else
                LogaErro(txtProcedure & " [" & ParametrosProc(0).Value.ToString() & "]... Erro: " & ex.Message)
#End If
            End If
            strCodErro = "SuperSuperMySQL::Obter: [" & strServer & "] " & ex.Message
            oDataSet.InfoPesquisa = strCodErro
            Return Nothing
        Finally
            If Not con Is Nothing Then
                con.Close()
                con.Dispose()
                con = Nothing
            End If
            If Not oDataSet Is Nothing Then
                oDataSet.Dispose()
                oDataSet = Nothing
            End If
            If Not dap Is Nothing Then
                dap.Dispose()
                dap = Nothing
            End If
            If Not cmd Is Nothing Then
                cmd.Dispose()
                cmd = Nothing
            End If
            oRelogio = Nothing
        End Try
    End Function

    Public Function Executar(ByVal txtProcedure As String) As Boolean

        Dim con As MySqlConnection = Nothing ' SqlConnection = Nothing
        Dim cmd As MySqlCommand = Nothing ' SqlCommand = Nothing
        '----------------------------------------
        Dim oRelogio As Stopwatch = Nothing

        Try

            Cursor.Current = CURSOR_OCUPADO

            strCodErro = ""

            oRelogio = New Stopwatch

            oRelogio.Start()

            con = New MySqlConnection(ObterConnectionString())
            cmd = New MySqlCommand(txtProcedure, con)
            cmd.CommandType = CommandType.StoredProcedure

            For Each DbParameter In ParametrosProc
                If Not DbParameter Is Nothing Then
                    cmd.Parameters.Add(DbParameter)
                End If
            Next

            con.Open()

            cmd.CommandTimeout = CMD_TIME_OUT
            cmd.ExecuteNonQuery()

            oRelogio.Stop()

            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtProcedure & " [SEM PARÂMETROS]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs.")
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtProcedure & " [" & listarParametros() & "]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs.")
#Else
                LogaErro(txtProcedure & " [" & ParametrosProc(0).Value.ToString() & "]... OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs.")
#End If
            End If

            con.Close()

            Return True
        Catch ex As MySqlException
            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtProcedure & " [SEM PARÂMETROS]... Erro: " & ex.Message)
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtProcedure & " [" & listarParametros() & "]... Erro: " & ex.Message)
#Else
                LogaErro(txtProcedure & " [" & ParametrosProc(0).Value.ToString() & "]... Erro: " & strCodErro)
#End If
            End If
            Return False
        Catch ex As Exception
            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtProcedure & " [SEM PARÂMETROS]... Erro: " & ex.Message)
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtProcedure & " [" & listarParametros() & "]... Erro: " & ex.Message)
#Else
                LogaErro(txtProcedure & " [" & ParametrosProc(0).Value.ToString() & "]... Erro: " & ex.Message)
#End If
            End If
            strCodErro = "SuperSuperMySQL::Obter: [" & strServer & "] " & ex.Message
            Return False
        Finally
            If Not con Is Nothing Then
                con.Close()
                con.Dispose()
                con = Nothing
            End If

            If Not cmd Is Nothing Then
                cmd.Dispose()
                cmd = Nothing
            End If
            oRelogio = Nothing
        End Try

    End Function

    Public Function ObterUltimoErro() As String
        Return strCodErro
    End Function

#If LOGAR_TODOS_PARAMETROS Then

    Private Function listarParametros() As String

        Dim i As Integer
        Dim paramLen As Integer = ParametrosProc.Length - 2
        Dim sResult As String = ""
        Dim sParamValue As String

        For i = 0 To paramLen

            If ParametrosProc(i).Value Is Nothing Then
                sParamValue = "NULL"
            ElseIf ParametrosProc(i).Value Is DBNull.Value Then
                sParamValue = "NULL"
            Else
                If TypeOf ParametrosProc(i).Value Is String Or TypeOf ParametrosProc(i).Value Is DateTime Then
                    sParamValue = "'" & ParametrosProc(i).Value.ToString & "'"
                Else
                    sParamValue = ParametrosProc(i).Value.ToString
                End If
            End If

            sResult = sResult & sParamValue
            If i + 1 <= paramLen Then sResult = sResult & ", "
        Next

        Return sResult

    End Function

#End If


#Region "___DISPOSE___"

    Private disposed As Boolean = False

    Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
        If Not (disposed) Then
            If disposing Then
                Dim i As Integer = 0
                'VER ATEH ONDE VAI O INDICE DE PARAM (UBOUND)
                For i = 0 To ParametrosProc.Length - 1
                    ParametrosProc(i) = Nothing
                Next
                ParametrosProc = Nothing
                i = Nothing
                strConexao = Nothing
                strServer = Nothing
                strDatabase = Nothing
                strUserID = Nothing
                strPassword = Nothing
                strCodErro = Nothing
            End If
        End If
        disposed = True
    End Sub

    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

#End Region


#If False Then

    Assim como outros que já te responderam, não acho as queixas que leio aqui como fantasiosas. Já passei e já testemunhei situações muito ruins. 
    Só trabalhei em TI para uma empresa que não tinha TI como atividade fim uma única vez e suspeito ter encontrado uma causa raiz para esse tipo de problema.

    Quando entrei lá o meu objetivo era fazer a fusão de dois departamentos de TI de duas empresas que foram compradas por um grupo de comunicação. 
    O motivo de irem no mercado buscar um outro gestor com bagagem técnica para fazer a fusão era porque os dois gerentes de ambas as empresas tinham
    não só opiniões diferentes sobre cada assunto, eram opiniões diametralmente opostos: um falava A e outro falava Z. Foram atrás de uma terceira 
    opinião então. Depois da fusão, gostaram do meu trabalho e assumi a gestão do departamento unificado.

    O discurso que eu ouvia repetidas vezes lá é que o TI era um "prestador de serviço", o que me incomodava profundamente.
    E vi no dia a dia como isso se manifestava: vinha algum funcionário na área de suporte, chamava um técnico de suporte com gesto de "vem aqui", 
    era prontamente atendido até chegar na mesa do cidadão que perguntava coisas pessoais, como fazer um certo post no Facebook pessoal dele, 
    ou se um dado modelo de tablet era bom para a filha dele.

    Então era uma espécie de concierge de nerds, de serviçais. Como era o único departamento que você chama e vem, nossos técnicos eram acionados 
    para os problemas mais absurdos: bebedouro com água quente, ar condicionado que não gelava. Até fomos acionados uma vez para tirar uma espécie 
    de despacho de macumba dentro da empresa porque todo mundo estava com medo de mexer.

    Um caso emblemático foi um usuário que abriu num curto espaço de tempo (creio que 1 ou 2 dias), quatro chamados. E todos meus técnicos que foram 
    atendê-lo voltaram muito ofendidos e nervosos. No quinto chamado quem desceu fui eu. O cara muito nervoso, fazendo gestos bruscos com as mãos 
    e em tom de voz elevado ficou xingando o computador, que era lento, uma carroça, e que não conseguia trabalhar com aquela porcaria. 
    Era um daqueles iMacs de 21 polegadas que custa um carro popular.

    Quando vejo a situação da máquina, realmente bem lenta, o cidadão tinha Safari, Chrome e Firefox abertos, cada um com umas ~40 abas abertas.
    Ele era um aqueles usuários patológicos que usa aba de browser como ToDo list. Falei para ele que o problema era esse, que ele tinha que 
    usar um browser só e usar menos que 40 abas. O cara ficou furioso, começou a gritar. Falei para ele abaixar o tom de voz pois ele estava 
    falando com um colega de trabalho. O cara insistiu e soltou a segunda pérola:

    - "Não é você quem vai me dizer quantas abas eu vou usar no navegador!"
    Tirei um screenshot da situação e do gráfico de uso da RAM, mandei para o sistema de chamados para registrar, chamei o diretor da área dele 
    até a mesa dele e falei na frente de todo mundo:

    - "Este rapaz não tem a postura mínima de respeito para lidar com colegas de trabalho. Enquanto ele não se retrarar publicamente do que fez 
    e lidar profissionalmente e calmamente, nem eu nem ninguém da minha equipe irá atendê-lo. Oriente seu funcionário sobre como se comportar
    e se ele estiver pronto amanhã, irei reconsiderar minha decisão. Hoje eu e minha equipe não temos condições de olhar na cara desse cidadão."
    Virei as costas e fui embora. Logo depois o cara foi demitido, provavelmente fazia isso com colegas do departamento dele. Mas em outros 
    tempos, essa filosofia de que "TI é prestador de serviço" faria com que o caso passasse em branco e engolindo seco o escandaloso fosse 
    atendido nos seus mimos.

    Consegui mudar muita coisa dessa cultura. Implementei um sistema de chamados que funcionava muito bem, paramos de ser acionados por qualquer
    coisa que envolvia energia elétrica (pois passamos a recusar por escrito), fiz campanhas de conscientização, tutoriais para que os 
    usuários aprendessem a fazer por conta própria os procedimentos mais básicos. Mas ainda assim, é um processo que leva anos para vencer
    como a última reunião que tive que descrevo a seguir:

    Fui chamado para uma reunião com o diretor e cerca de 10 funcionários chave de um dos principais departamentos a empresa. O motivo? As 
    impressoras não funcionam. Abri meu sistema de chamados na frente de todos e mostrei que há 15 dias, nas 9 cidades onde minha equipe 
    atendia, não havia chamados sobre impressora e os últimos eram mera troca de cartucho.

    Gerente: - Mas hoje eu tentei imprimir e não funcionou!
    Eu: - Você está me falando isso agora e sabendo disso poderei tratar do assunto. Mas sem a abertura de chamado, não tenho como agir diante
    de um problema que não fomos notificados.
    Gerente: - Eu sei qual é o problema! Levantei da minha mesa e a impressora estava sem papel!
    Eu: [já puto] - Que bom que é um problema fácil de resolver e ao seu alcance de resolveu. Você pôs o papel e imprimiu normalmente?
    Gerente: - Sim! Mas é um absurdo eu ter que levantar da minha mesa para por papel na impressora.
    Eu: [mais puto ainda] - Realmente, para uma impressora imprimir em papel, ela precisa ter papel na bandeja.
    Gerente: - Sua equipe não pode deixar uma coisa dessas acontecer!
    Eu: - Você quer que eu pegue os técnicos que ficam apenas na matriz e façamos rondas pelas 9 cidades que operamos vendo se cada impressora 
    tem papel suficiente porque você que tem sua impressora no seu prédio, no seu andar, não quer correr o risco de eventualmente ter
    que se levantar para por papel na impressora? Quer que eu inclua o café das cafeteiras nas rondas também? O papel higiênico dos
    banheiros também? Imagina o risco de você ir no banheiro e não ter papel!
    O diretor dele viu que a reunião pendeu para um lado não favorável para ele e abortou a reunião. Eu fui até a sala do presidente e 
    entreguei minha carta de demissão e voltei a atuar em empresas em que TI é a atividade fim.

    TI não pode ser visto como prestador de serviço interno. Ele tem que fazer parte do processo produtivo da empresa, ajudar nas decisões
    estratégicas, poder estabelecer e exigir políticas, procedimentos e protocolo de todos da organização. Assim como o presidente de
    uma organização acata ordens do departamento jurídico mesmo sendo presidente, ele tem que obedecer sim as políticas de TI da empresa,
    mesmo sendo presidente. Uma empresa pode quebrar com ransomware ou vazamento de dados. Não dá para tratar o pessoal de TI como 
    se trata o pessoal da limpeza.


#End If


End Class