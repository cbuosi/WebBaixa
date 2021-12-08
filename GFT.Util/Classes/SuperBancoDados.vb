#Region "Legal"
'************************************************************************************************************************
' Copyright (c) 2015, Todos direitos reservados, Sonda-IT - Serviços de TI - http://www.sondait.com.br/
'
' Autor........: Carlos Buosi (cbuosi@gmail.com)
' Arquivo......: SuperBancoDados.vb
' Tipo.........: Modulo VB.
' Versao.......: 2.02+
' Propósito....: Modulo de banco de dados (SQL Server 2000+).
' Uso..........: Não se aplica
' Produto......: SDBI
'
' Legal........: Este código é de propriedade do Banco Bradesco S/A e/ou Sonda-IT - Serviços de TI, sua cópia
'                e/ou distribuição é proibida.
'
' GUID.........: {7CC82C98-9E60-4498-9681-7102635D1782}
' Observações..: nenhuma.
'
'************************************************************************************************************************
#End Region
Option Explicit On
Option Strict On

#If DEBUG Then
#Const LOGAR_TODOS_PARAMETROS = True
#Else
#Const LOGAR_TODOS_PARAMETROS = False
#End If

Imports System.Data.Common
Imports System.Data.SqlClient

Public Class BancoDados
    Implements IDisposable

#Const DEBUG_BULK = False

    Private Const TIMEOUT = 10                          ' Tempo (em minutos) de time out.
    Private Const MAX_PARAM = 300                       ' No maximo 300 parametros por procedimento.
    Private Const SQL_SERVER_DEFAULT_PORT = 1433        ' Caso o windows do usuario esteja com a configuracao "danificada".
    Private Const PACKET_SIZE = 4096                    ' Tamanho pacote dados da conexão.
    Private Const CMD_TIME_OUT = (TIMEOUT * 60)         ' 9x60 = 9 minutos.
    Private Const BULK_INSERT_BATCH_SIZE = 5000         ' Insere lotes de 2500 registros (evita time out).
    Private Const BULK_INSERT_TIME_OUT = (TIMEOUT * 60) ' 20x60 = 20 minutos.

    Private strConexao As String = ""
    Private strServer As String = ""
    Private strDatabase As String = ""
    Private strUserID As String = ""
    Private strPassword As String = ""
    Private strTabelaDestino As String = ""

    Dim ParametrosProc() As DbParameter

    Private strCodErro As String = ""

    Public Sub New(ByVal _strServer As String,
                   ByVal _strDatabase As String,
                   ByVal _strUserID As String,
                   ByVal _strPassword As String)
        '
        strServer = _strServer
        strDatabase = _strDatabase
        strUserID = _strUserID
        strPassword = _strPassword
        LimpaParametros()
        '
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
            LogaErro("Erro em SuperBancoDados::LimpaParametros: " & ex.Message)
            strCodErro = "SuperBancoDados::LimpaParametros: " & ex.Message
        End Try
    End Sub

    Public Sub AdicionaParametro(ByVal _Campo As Campo,
                                 ByVal valor As Object)
        Try
            Dim tamArray As Integer
            tamArray = ParametrosProc.Length - 1

            If (tamArray + 1) > MAX_PARAM Then
                LogaErro("Erro em SuperBancoDados::Muitos parametros! :)")
                Exit Sub
            End If

            'Se passar nothing, transforma em nulo.....
            If valor Is Nothing Then
                valor = DBNull.Value
            End If

            ParametrosProc(tamArray) = New SqlParameter()
            ParametrosProc(tamArray).ParameterName = _Campo.nome
            ParametrosProc(tamArray).SourceVersion = DataRowVersion.Current
            ParametrosProc(tamArray).SourceColumn = String.Empty
            ParametrosProc(tamArray).SourceColumnNullMapping = False
            ParametrosProc(tamArray).Size = _Campo.tamanho
            ParametrosProc(tamArray).Direction = ParameterDirection.Input
            ParametrosProc(tamArray).DbType = _Campo.tipo
            ParametrosProc(tamArray).Value = valor

            ReDim Preserve ParametrosProc(tamArray + 1)

        Catch ex As Exception
            LogaErro("Erro em SuperBancoDados::AdicionaParametro(1): " & ex.Message)
            strCodErro = "SuperBancoDados::AdicionaParametro(1): " & ex.Message
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
                LogaErro("Erro em SuperBancoDados::Muitos parametros! :)")
                Exit Sub
            End If

            ParametrosProc(tamArray) = New SqlParameter()
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
            LogaErro("Erro em SuperBancoDados::AdicionaParametro: " & ex.Message)
            strCodErro = "SuperBancoDados::AdicionaParametro: " & ex.Message
        End Try
    End Sub

    Private Function ObterConnectionString() As String

        Try

            Return "Server=" & strServer & '"," & SQL_SERVER_DEFAULT_PORT &
                   ";Asynchronous Processing=true" &
                   ";Database=" & strDatabase &
                   ";User Id=" & strUserID &
                   ";Password=" & strPassword &
                   ";Packet Size=" & PACKET_SIZE & ";"

        Catch ex As Exception
            LogaErro("Erro em SuperBancoDados::ObterConnectionString: " & ex.Message)
            strCodErro = "SuperBancoDados::ObterConnectionString: " & ex.Message
            Return ""
        End Try
    End Function

    Public Function ObterQuery(ByVal txtQuery As String) As SuperDataSet

        Dim con As SqlConnection = Nothing
        Dim cmd As SqlCommand = Nothing
        Dim dap As SqlDataAdapter = Nothing
        Dim oRelogio As Stopwatch = Nothing
        Dim oDataSet As SuperDataSet = Nothing

        Try

            Cursor.Current = CURSOR_OCUPADO

            strCodErro = ""

            oDataSet = New SuperDataSet
            oRelogio = New Stopwatch

            oRelogio.Start()

            con = New SqlConnection(ObterConnectionString())
            cmd = New SqlCommand(txtQuery, con)
            cmd.CommandType = CommandType.Text

            For Each DbParameter In ParametrosProc
                If Not DbParameter Is Nothing Then
                    cmd.Parameters.Add(DbParameter)
                End If
            Next

            dap = New SqlDataAdapter(txtQuery, con)
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

        Catch ex As SqlException
            For i = 0 To ex.Errors.Count - 1
                strCodErro += " Message: " & ex.Errors(i).Message & " Line#:" & ex.Errors(i).LineNumber.ToString & " Src:" & ex.Errors(i).Source & " Proc:" & ex.Errors(i).Procedure
            Next i
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
            strCodErro = "SuperBancoDados::Obter: [" & strServer & "] " & ex.Message
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

        Dim con As SqlConnection = Nothing
        Dim cmd As SqlCommand = Nothing
        Dim dap As SqlDataAdapter = Nothing
        Dim oRelogio As Stopwatch = Nothing
        'Dim oDataSet As SuperDataSet = Nothing

        Try

            Cursor.Current = CURSOR_OCUPADO

            strCodErro = ""

            'oDataSet = New SuperDataSet
            oRelogio = New Stopwatch

            oRelogio.Start()

            con = New SqlConnection(ObterConnectionString())
            cmd = New SqlCommand(txtQuery, con)
            cmd.CommandType = CommandType.Text

            For Each DbParameter In ParametrosProc
                If Not DbParameter Is Nothing Then
                    cmd.Parameters.Add(DbParameter)
                End If
            Next

            dap = New SqlDataAdapter(txtQuery, con)
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

        Catch ex As SqlException
            For i = 0 To ex.Errors.Count - 1
                strCodErro += " Message: " & ex.Errors(i).Message & " Line#:" & ex.Errors(i).LineNumber.ToString & " Src:" & ex.Errors(i).Source & " Proc:" & ex.Errors(i).Procedure
            Next i
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
            strCodErro = "SuperBancoDados::Obter: [" & strServer & "] " & ex.Message
            Return False
        Finally
            If Not con Is Nothing Then
                con.Close()
                con.Dispose()
                con = Nothing
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


    Public Function Obter(ByVal txtProcedure As String) As SuperDataSet

        Dim con As SqlConnection = Nothing
        Dim cmd As SqlCommand = Nothing
        Dim dap As SqlDataAdapter = Nothing
        Dim oRelogio As Stopwatch = Nothing
        Dim oDataSet As SuperDataSet = Nothing

        Try

            Cursor.Current = CURSOR_OCUPADO

            strCodErro = ""

            oDataSet = New SuperDataSet
            oRelogio = New Stopwatch

            oRelogio.Start()

            con = New SqlConnection(ObterConnectionString())
            cmd = New SqlCommand(txtProcedure, con)
            cmd.CommandType = CommandType.StoredProcedure

            For Each DbParameter In ParametrosProc
                If Not DbParameter Is Nothing Then
                    cmd.Parameters.Add(DbParameter)
                End If
            Next

            dap = New SqlDataAdapter(txtProcedure, con)
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

        Catch ex As SqlException
            For i = 0 To ex.Errors.Count - 1
                strCodErro += " Message: " & ex.Errors(i).Message & " Line#:" & ex.Errors(i).LineNumber.ToString & " Src:" & ex.Errors(i).Source & " Proc:" & ex.Errors(i).Procedure
            Next i
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
            strCodErro = "SuperBancoDados::Obter: [" & strServer & "] " & ex.Message
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

        Dim con As SqlConnection = Nothing
        Dim cmd As SqlCommand = Nothing
        Dim oRelogio As Stopwatch = Nothing

        Try

            Cursor.Current = CURSOR_OCUPADO

            strCodErro = ""

            oRelogio = New Stopwatch

            oRelogio.Start()

            con = New SqlConnection(ObterConnectionString())
            cmd = New SqlCommand(txtProcedure, con)
            cmd.CommandType = CommandType.StoredProcedure

            For Each DbParameter In ParametrosProc
                If Not DbParameter Is Nothing Then
                    cmd.Parameters.Add(DbParameter)
                End If
            Next

            con.Open()

            cmd.CommandTimeout = CMD_TIME_OUT

            cmd.ExecuteNonQuery()
            'cmd.BeginExecuteNonQuery()


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

        Catch ex As SqlException
            For i = 0 To ex.Errors.Count - 1
                strCodErro += " Message: " & ex.Errors(i).Message & " Line#:" & ex.Errors(i).LineNumber.ToString & " Src:" & ex.Errors(i).Source & " Proc:" & ex.Errors(i).Procedure
            Next i
            If ParametrosProc(0) Is Nothing Then
                LogaErro(txtProcedure & " [SEM PARÂMETROS]... Erro: " & strCodErro)
            Else
#If LOGAR_TODOS_PARAMETROS Then
                LogaErro(txtProcedure & " [" & listarParametros() & "]... Erro: " & strCodErro)
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
            strCodErro = "SuperBancoDados::Obter: [" & strServer & "] " & ex.Message
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

#If False Then
    Public Function TestaConexao() As Boolean

        Dim con As SqlConnection = Nothing
        Dim oRelogio As Stopwatch = Nothing

        Try

            strCodErro = ""
            Cursor.Current = CURSOR_OCUPADO

            oRelogio = New Stopwatch

            oRelogio.Start()

            LogaErro("*** INÍCIO TestaConexao")

            con = New SqlConnection(ObterConnectionString())
            con.Open()
            oRelogio.Stop()
            LogaErro("*** OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs. *** FIM")

            con.Close()

            Return True

        Catch ex As SqlException
            For i = 0 To ex.Errors.Count - 1
                strCodErro += " Message: " & ex.Errors(i).Message & " Line#:" & ex.Errors(i).LineNumber.ToString & " Src:" & ex.Errors(i).Source & " Proc:" & ex.Errors(i).Procedure
            Next i
            LogaErro("Erro em SuperBancoDados::Obter: " & strCodErro)
            Return False
        Catch ex As Exception
            LogaErro("Erro em SuperBancoDados::Obter: [" & strServer & "] " & ex.Message)
            strCodErro = "Conexão em [" & strServer & "\" & strDatabase & "] Usuário:[" & strUserID & "] *** ERRO: " & ex.Message
            Return False
        Finally
            If Not con Is Nothing Then
                con.Close()
                con.Dispose()
            End If
            con = Nothing
            oRelogio = Nothing
        End Try
    End Function
#End If

    Public Function InserirDataTableLote(ByVal oDataTable As SuperDataTable) As Boolean

        Dim connection As SqlConnection = Nothing
        Dim oRelogio As Stopwatch = Nothing
        Dim oSqlBulkCopy As SqlBulkCopy = Nothing

        Try
            strCodErro = ""
            Cursor.Current = CURSOR_OCUPADO
            oRelogio = New Stopwatch
            oRelogio.Start()
            LogaErro("*** INÍCIO InserirDataTableLote. Tabela destino: " & strTabelaDestino)
            connection = New SqlConnection(ObterConnectionString())
            connection.Open()
            oSqlBulkCopy = New SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, Nothing)

#If DEBUG_BULK Then 'Adiciona manipulador 
            AddHandler oSqlBulkCopy.SqlRowsCopied, AddressOf OnRegistrosCopiados
#End If

            oSqlBulkCopy.DestinationTableName = strTabelaDestino
            oSqlBulkCopy.BatchSize = BULK_INSERT_BATCH_SIZE
            oSqlBulkCopy.BulkCopyTimeout = BULK_INSERT_TIME_OUT
            oSqlBulkCopy.WriteToServer(oDataTable)
            oRelogio.Stop()
            LogaErro("*** OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs. *** FIM")
            Return True
        Catch ex As SqlException
            For i = 0 To ex.Errors.Count - 1
                strCodErro += " Message: " & ex.Errors(i).Message & " Line#:" & ex.Errors(i).LineNumber.ToString & " Src:" & ex.Errors(i).Source & " Proc:" & ex.Errors(i).Procedure
            Next i
            LogaErro("Erro em SuperBancoDados::InserirDataTableLote: " & strCodErro)
            Return False
        Catch ex As Exception
            LogaErro("Erro em SuperBancoDados::InserirDataTableLote: [" & strServer & "] " & ex.Message)
            strCodErro = "SuperBancoDados::InserirDataTableLote: [" & strServer & "] " & ex.Message
            Return False
        Finally
            If Not connection Is Nothing Then
                connection.Close()
                connection.Dispose()
                connection = Nothing
            End If
            If Not oSqlBulkCopy Is Nothing Then
                oSqlBulkCopy.Close()
                oSqlBulkCopy = Nothing
            End If
            oRelogio = Nothing
        End Try

    End Function

    Public Function InserirDataTableLote(ByVal oDataTable As DataTable) As Boolean

        Dim connection As SqlConnection = Nothing
        Dim oRelogio As Stopwatch = Nothing
        Dim oSqlBulkCopy As SqlBulkCopy = Nothing

        Try
            strCodErro = ""
            Cursor.Current = CURSOR_OCUPADO
            oRelogio = New Stopwatch
            oRelogio.Start()
            LogaErro("*** INÍCIO InserirDataTableLote. Tabela destino: " & strTabelaDestino & " " & oDataTable.Rows.Count().ToString("#0.00") & " registro(s).")
            connection = New SqlConnection(ObterConnectionString())
            connection.Open()
            oSqlBulkCopy = New SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, Nothing)

#If DEBUG_BULK Then 'Adiciona manipulador 
            AddHandler oSqlBulkCopy.SqlRowsCopied, AddressOf OnRegistrosCopiados
#End If

            oSqlBulkCopy.DestinationTableName = strTabelaDestino
            oSqlBulkCopy.BatchSize = BULK_INSERT_BATCH_SIZE
            oSqlBulkCopy.BulkCopyTimeout = BULK_INSERT_TIME_OUT
            oSqlBulkCopy.WriteToServer(oDataTable)
            oRelogio.Stop()

            strCodErro = (oRelogio.ElapsedMilliseconds / 1000).ToString

            LogaErro("*** OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs. *** FIM")
            Return True
        Catch ex As SqlException
            For i = 0 To ex.Errors.Count - 1
                strCodErro += " Message: " & ex.Errors(i).Message & " Line#:" & ex.Errors(i).LineNumber.ToString & " Src:" & ex.Errors(i).Source & " Proc:" & ex.Errors(i).Procedure
            Next i
            LogaErro("Erro em SuperBancoDados::InserirDataTableLote: " & strCodErro)
            Return False
        Catch ex As Exception
            LogaErro("Erro em SuperBancoDados::InserirDataTableLote: [" & strServer & "] " & ex.Message)
            strCodErro = "SuperBancoDados::InserirDataTableLote: [" & strServer & "] " & ex.Message
            Return False
        Finally
            If Not connection Is Nothing Then
                connection.Close()
                connection.Dispose()
                connection = Nothing
            End If
            If Not oSqlBulkCopy Is Nothing Then
                oSqlBulkCopy.Close()
                oSqlBulkCopy = Nothing
            End If
            oRelogio = Nothing
        End Try

    End Function

#If DEBUG_BULK Then
    Private Sub OnRegistrosCopiados(ByVal sender As Object, ByVal args As SqlRowsCopiedEventArgs)
        'Debug.Print("Registros copiados: {0}", args.RowsCopied)
        LogaErro("Registros copiados: " &  args.RowsCopied.ToString("00000000"))
    End Sub
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


End Class