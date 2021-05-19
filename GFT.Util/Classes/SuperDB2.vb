Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.IO
Imports System.Diagnostics

Imports IBM.Data.DB2


Public Class SuperDB2

    Private Const CMD_TIME_OUT As Integer = 300 '5x60 = 5 minutos!  

    Private strServer As String = ""
    Private strDatabase As String = ""
    Private strUserID As String = ""
    Private strPassword As String = ""
    Private intPortNumber As Integer = 0

    Private strCodErro As String = ""

    '------------------------------------------------------------------------
    ' ultimoErro() As String
    '------------------------------------------------------------------------
    Public ReadOnly Property ultimoErro() As String
        Get
            Return strCodErro
        End Get
    End Property

    '------------------------------------------------------------------------
    ' connectionString() As String
    '------------------------------------------------------------------------
    Public ReadOnly Property connectionString() As String

        Get

            Return "Server=" & strServer & ":" &
                   intPortNumber & ";Database=" &
                   strDatabase & ";UID=" &
                   strUserID & ";PWD=" &
                   strPassword

        End Get

    End Property

    ''------------------------------------------------------------------------
    '' New - Construtor da Classe (1)
    ''------------------------------------------------------------------------
    'Public Sub New(ByVal sUserID As String,
    '               ByVal sPassword As String)

    '    strServer = ObterConfig("DB2Server")
    '    strDatabase = ObterConfig("DB2Database")
    '    strUserID = sUserID
    '    strPassword = sPassword
    '    intPortNumber = CInt(ObterConfig("DB2Port"))
    '    strCodErro = ""

    'End Sub

    '------------------------------------------------------------------------
    ' New - Construtor da Classe (2)
    '------------------------------------------------------------------------
    Public Sub New(ByVal sServer As String,
                   ByVal iPortNumber As Integer,
                   ByVal sDatabase As String,
                   ByVal sUserID As String,
                   ByVal sPassword As String)
        strServer = sServer
        strDatabase = sDatabase
        strUserID = sUserID
        strPassword = sPassword
        intPortNumber = iPortNumber
        strCodErro = ""
    End Sub

    '------------------------------------------------------------------------
    ' obterUltimoErro() As String
    '------------------------------------------------------------------------
    Public Function obterUltimoErro() As String
        Return strCodErro
    End Function

    '------------------------------------------------------------------------
    ' executarQueryDB2(ByVal sQuery As String) As SuperDataSet
    '------------------------------------------------------------------------
    Public Function Executar(ByVal sQuery As String) As SuperDataSet

        Dim con As DB2Connection = Nothing
        Dim cmd As DB2Command = Nothing
        Dim dap As DB2DataAdapter = Nothing
        Dim oRelogio As Stopwatch = Nothing
        Dim oDataSet As SuperDataSet = Nothing

        Try

            Cursor.Current = CURSOR_OCUPADO

            strCodErro = "" 'Limpa o último código de erro

            oDataSet = New SuperDataSet
            oRelogio = New Stopwatch

            oRelogio.Start()

            con = New DB2Connection(connectionString)
            cmd = New DB2Command(sQuery, con)
            cmd.CommandType = CommandType.Text

            dap = New DB2DataAdapter(sQuery, con)
            cmd.CommandTimeout = CMD_TIME_OUT
            dap.SelectCommand = cmd

            con.Open()
            dap.Fill(oDataSet)
            oRelogio.Stop()
            'oDataSet.InfoPesquisa = "Quantidade de registro(s): " & oDataSet.TotalRegistros().ToString("0,0#") & ". Tempo: " & (oRelogio.ElapsedMilliseconds / 1000).ToString & " segundo(s)."
            oDataSet.InfoPesquisa = oDataSet.TotalRegistros().ToString("0,0#") & " registro(s). " & (oRelogio.ElapsedMilliseconds / 1000).ToString & " segundo(s)."

            LogaErro("DB2 OK! Tempo exec: [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs. Registros: [" & oDataSet.TotalRegistros & "].")
            con.Close()

            'Devolve dataset
            Return oDataSet

        Catch exx As DB2Exception
            strCodErro = "Erro: " & exx.Message
            Return Nothing
        Catch ex As Exception
            LogaErro("*** ERRO executarQueryDB2 ! *** FIM")
            LogaErro("Erro em SuperDB2::executarQueryDB2: [" & strServer & "] " & ex.Message)
            strCodErro = "Conexão em [" & strServer & ":" & intPortNumber & "\" & strDatabase & "] Usuário:[" & strUserID & "] ERRO: " & ex.Message
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

    Function ObteVersao() As String

        Dim mx As FileVersionInfo

        Try

            mx = FileVersionInfo.GetVersionInfo("IBM.Data.DB2.dll")
            Return mx.FileDescription() & " - v. " & mx.FileVersion()

        Catch ex As Exception
            LogaErro("Erro em SuperDB2::ObteVersao: " & ex.Message)
            Return ("Erro em SuperDB2::ObteVersao: " & ex.Message)
        End Try
    End Function


    Public Function Instalada() As Boolean

#If DEBUG Then
        Return True
#Else

        Dim con As DB2Connection = Nothing
        Dim cmd As DB2Command = Nothing
        Dim dap As DB2DataAdapter = Nothing

        Try

            strCodErro = "" 'Limpa o último código de erro


            con = New DB2Connection(connectionString)
            cmd = New DB2Command("", con)
            cmd.CommandType = CommandType.Text

            dap = New DB2DataAdapter("", con)
            cmd.CommandTimeout = CMD_TIME_OUT
            dap.SelectCommand = cmd

            Return True

        Catch exx As DB2Exception
            strCodErro = "Erro em SuperDB2:Instalada() : " & exx.Message
            Return False
        Catch ex As Exception
            LogaErro("*** ERRO executarQueryDB2 ! *** FIM")
            LogaErro("Erro em SuperDB2:Instalada() : " & ex.Message)
            Return False
        End Try

#End If
    End Function


End Class
