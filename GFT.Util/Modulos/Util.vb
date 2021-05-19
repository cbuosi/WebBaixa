#Region "Legal"

'************************************************************************************************************************
' Copyright (c) 2015, Todos direitos reservados, GFT-IT - Serviços de TI - http://www.GFTit.com.br/
'
' Autor........: Carlos Buosi (cbuosi@gmail.com)
' Arquivo......: Util.vb
' Tipo.........: Modulo VB.
' Versao.......: 2.02+
' Propósito....: Utilitarios
' Uso..........: Não se aplica
' Produto......: GerCor
'
' Legal........: Este código é de propriedade do Banco Bradesco S/A e/ou GFT-IT - Serviços de TI, sua cópia
'                e/ou distribuição é proibida.
'
' GUID.........: {7CC82C98-9E60-4498-9681-7102635D1782}
' Observações..: nenhuma.
'
'************************************************************************************************************************

#End Region

Option Explicit On
Option Strict On

Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms
Imports GFT.Util.clsMsgBox
Imports System.Text
Imports System.Xml
Imports System.Reflection
Imports System.Threading

Module Util

    Private xmlConfig As XmlDocument = Nothing
    Public usuarioLogado As usuario_
    Public Const ARQUIVO_DATABASE_CONFIG As String = "Config.xml"
    Public Const strTituloApp As String = "Termo Adesão - Sistemas Investimentos"
    Public Const strOk As String = "&Ok"
    Public Const strCancela As String = "&Cancela"
    Public Const strSim As String = "&Sim"
    Public Const strNao As String = "&Não"
    Public arqCaminho As String = ""
    Public vMsgBox As eRet
#If DEBUG Then
    Const bLogaErro As Boolean = True
#Else
    Const bLogaErro As Boolean = False
#End If
    Public strCaminhoLog As String = "" 'Path.Combine(Environment.GetEnvironmentVariable("Temp"), "_GLOBAL_LOG")
    Public corObjSelecionado As Color = Color.FromArgb(255, 251, 206)
    Public corObjNaoSelecionado As Color = Color.White
    Public corObjBorda As Color = Color.Blue
    Public CURSOR_OCUPADO As System.Windows.Forms.Cursor = Cursors.WaitCursor

    Public corSelecionadaDentro As Color = Color.FromArgb(177, 153, 92)
    Public corSelecionadaFora As Color = Color.FromArgb(230, 194, 124)

    Public corDesselecionada As Color = Color.FromArgb(127, 157, 185)

    Public corGrid1 As Color = Color.Transparent
    Public corGrid2 As Color = Color.FromArgb(240, 240, 240)
    Public corObjDisabled As Color = Color.FromArgb(213, 220, 232)

    Dim oCrypto As New clsCrypto()

    Public Function Encripta(ByVal strTexto As String) As String
        Try
            Return oCrypto.Encripta(strTexto)
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function Decripta(ByVal strTexto As String) As String
        Try
            Return oCrypto.DeCripta(strTexto)
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Public Structure ParamAutomaticos
        Public Usuario As String
        Public Senha As String
        Public Timeout As Decimal
    End Structure
    Public Structure usuario_
        Public ParAuto As ParamAutomaticos
    End Structure

    '------------------------------------------------------------------------
    ' LogaErro(ByVal sTexto As String) CDBF
    '------------------------------------------------------------------------
    Public Sub LogaErro(ByVal sTexto As String)

        Dim hArq As StreamWriter = Nothing
        Dim sLineHeader As String = ""
        Dim mtx As Mutex

        Try

            If bLogaErro = False Then
                Exit Sub
            End If

            mtx = New Mutex(False, "_MUTEX_LOGA_ERRO_")
            If mtx.WaitOne(5000, False) = False Then
                Exit Sub
            End If

            If strCaminhoLog = "" Then
                strCaminhoLog = ObterConfig("CAMINHO_LOG")
            End If

            If Directory.Exists(strCaminhoLog) = False Then
                Exit Sub
            End If

            hArq = New StreamWriter(strCaminhoLog &
                                    "\Log_" &
                                    Format(Now.Year, "0000") &
                                    Format(Now.Month, "00") &
                                    Format(Now.Day, "00") & "_" &
                                    ObterNomeMaquina() & "_" &
                                    ObterNomeUsuarioLogado() & ".txt",
                                    True,
                                    Encoding.Unicode)
            hArq.AutoFlush = True

            sLineHeader = Format(Now().Hour, "00") & ":" & Format(Now().Minute, "00") & ":" & Format(Now().Second, "00") & "." & Format(Now().Millisecond, "0000") & " CDBF "

            hArq.WriteLine(sLineHeader & sTexto)
#If DEBUG Then
            System.Diagnostics.Debug.Print(sLineHeader & sTexto)
#End If
            hArq.Flush()
            hArq.Close()

        Catch ex As Exception
#If DEBUG Then
            System.Diagnostics.Debug.Print("Erro em LogaErro: " & ex.Message)
#End If
        Finally
            If Not hArq Is Nothing Then
                hArq.Dispose()
                hArq = Nothing
            End If
        End Try
    End Sub

    '    Public Sub LogaErro(ByVal texto As String,
    '                        Optional bForcaLog As Boolean = False)
    '
    '        Dim xxx As Mutex
    '
    '        Dim hArq As StreamWriter = Nothing
    '        Dim sLineHeader As String = ""
    '
    '        Try
    '
    '            If bLogaErro = False And bForcaLog = False Then
    '                Exit Sub
    '            End If
    '
    '            xxx = New Mutex(False, "_MUTEX_LOGA_ERRO_")
    '            If xxx.WaitOne(5000, False) = False Then
    '                Exit Sub
    '            End If
    '
    '            strCaminhoLog = Path.Combine(Environment.GetEnvironmentVariable("Temp"), "_CCAPN_LOG")
    '
    '            hArq = New StreamWriter(strCaminhoLog &
    '                                    "\Log_" &
    '                                    Format(Now.Year, "0000") &
    '                                    Format(Now.Month, "00") &
    '                                    Format(Now.Day, "00") & "_" &
    '                                    ObterNomeMaquina() & "_" &
    '                                    ObterNomeUsuarioLogado() & ".txt",
    '                                    True,
    '                                    Encoding.Default)
    '
    '            sLineHeader = Format(Now().Hour, "00") & ":" & Format(Now().Minute, "00") & ":" & Format(Now().Second, "00") & "." & Format(Now().Millisecond, "0000") & Space(1) & "UTIL" & Space(1)
    '
    '            hArq.AutoFlush = True
    '            hArq.WriteLine(sLineHeader & texto)
    '#If DEBUG Then
    '            Debug.Print(sLineHeader & texto)
    '#End If
    '        Catch ex As Exception
    '#If DEBUG Then
    '            Debug.Print("Erro em LogaErro: " & ex.Message)
    '#End If
    '        Finally
    '            If Not hArq Is Nothing Then
    '                hArq.Close()
    '                hArq.Dispose()
    '                hArq = Nothing
    '            End If
    '        End Try
    '    End Sub

    Public Function ObterNomeUsuarioLogado() As String
        Try
            Return Environment.UserName
        Catch ex As Exception
            LogaErro("Erro em Util::obterNomeUsuarioLogado: " & ex.Message)
            Return ""
        End Try
    End Function

    Public Function ObterNomeMaquina() As String
        Try
            Return Environment.MachineName
        Catch ex As Exception
            LogaErro("Erro em Util::obterNomeMaquina: " & ex.Message)
            Return ""
        End Try
    End Function

    Public Sub ApagaArquivo(ByVal strArquivo As String)
        File.Delete(strArquivo)
    End Sub

    Public Function ExisteArquivo(ByVal strArquivo As String) As Boolean
        Return File.Exists(strArquivo)
    End Function

    Public Function ObterConfig(ByVal chave As String) As String
        Try
            If xmlConfig Is Nothing Then
                xmlConfig = New XmlDocument()
                xmlConfig.Load(Path.Combine(Application.StartupPath, ARQUIVO_DATABASE_CONFIG))
            End If
            Return xmlConfig.DocumentElement.SelectSingleNode("//WEBB").SelectSingleNode("//" & chave).Attributes.ItemOf("valor").InnerText
        Catch ex As Exception
            LogaErro("Erro em Util::ObterConfig: " & ex.Message)
            Return ""
        End Try
    End Function

    Public Function AppVersion() As String
        Try
            Return Application.ProductVersion
        Catch ex As Exception
            LogaErro("Erro em Util::AppVersion: " & CStr(ex.ToString()))
            Return "Erro em AppVersion()"
        End Try
    End Function

    Public Function FormataTrunca(ByVal dValor As Decimal,
                                  ByVal nTam As Integer) As String
        Try

            Dim strValor As String
            Dim Formato As String

            Formato = "{0:" & Replicar("0", nTam) & "}" '=> {0:000000000}
            strValor = String.Format(Formato, dValor) 'OK

            If strValor.Length > nTam Then '0000000001
                strValor = strValor.Substring(strValor.Length - nTam, nTam)
            End If

            Return strValor
        Catch ex As Exception
            LogaErro("Erro em Util::FormataTrunca: " & CStr(ex.ToString()))
            Return ""
        End Try

    End Function

    Private Function Replicar(ByVal str As String, ByVal Times As Integer) As String
        Try

            Dim ret As String = ""
            For i As Integer = 1 To Times
                ret += str
            Next
            Return ret
        Catch ex As Exception
            LogaErro("Erro em Util::Replicar: " & CStr(ex.ToString()))
            Return ""
        End Try
    End Function

    Public Function fncFormataString(ByVal par1 As Object,
                                     ByVal tamanho As Decimal,
                                     ByVal caracter As String) As String

        Try

            Dim strAux As String

            If par1 Is Nothing Then
                Return Replicar(caracter, CInt(tamanho))
            End If

            If IsNumeric(caracter) = True Then
                strAux = Replicar(caracter, CInt(tamanho)) + CStr(par1).Replace(".", "")
                Return strAux.Substring(CInt(strAux.Length - tamanho), CInt(tamanho))
            Else
                Return par1.ToString.Trim() & Replicar(caracter, CInt(tamanho - par1.ToString.Length))
            End If
        Catch ex As Exception
            LogaErro("Erro em Util::fncFormataString: " & CStr(ex.ToString()))
            Return ""
        End Try

    End Function

    Public Sub ChecaCriaDiretorio(ByVal strCaminhoDiretorio As String)
        Try
            LogaErro("Checando existência de [" & strCaminhoDiretorio & "]")
            If ExisteCaminho(strCaminhoDiretorio) = False Then
                LogaErro("Não existe [" & strCaminhoDiretorio & "], criando...")
                Call System.IO.Directory.CreateDirectory(strCaminhoDiretorio)
            End If
        Catch ex As Exception
            LogaErro("Erro em ChecaCriaDiretorio: " & ex.Message)
        End Try
    End Sub

    Public Function ExisteCaminho(ByVal strCaminho As String) As Boolean
        Return System.IO.Directory.Exists(strCaminho)
    End Function

    '------------------------------------------------------------------------
    ' NomeMetodo(ByVal oObjeto As Object) As String
    '------------------------------------------------------------------------
    Public Function NomeMetodo(ByVal oObjeto As Object) As String

        Dim stackTrace As StackTrace
        Dim stackFrame As StackFrame
        Dim methodBase As MethodBase

        Try

            stackTrace = New StackTrace()
            stackFrame = stackTrace.GetFrame(1)
            methodBase = stackFrame.GetMethod()

            Return oObjeto.GetType().Name & "::" & methodBase.Name
        Catch ex As Exception
            Return "Erro em Util::NomeMetodo(1)"
        End Try

    End Function

    '------------------------------------------------------------------------
    ' NomeMetodo(Optional ByVal strObjeto As String = "") As String
    '------------------------------------------------------------------------
    Public Function NomeMetodo(Optional ByVal strObjeto As String = "") As String

        Dim stackTrace As StackTrace
        Dim stackFrame As StackFrame
        Dim methodBase As MethodBase

        Try

            stackTrace = New StackTrace()
            stackFrame = stackTrace.GetFrame(1)
            methodBase = stackFrame.GetMethod()

            Return strObjeto & "::" & methodBase.Name
        Catch ex As Exception
            Return "Erro em Util::NomeMetodo(2)"
        End Try

    End Function

    <System.Runtime.InteropServices.DllImport("user32.dll")>
    Private Function ShowWindow(hWnd As IntPtr, flags As ShowWindowEnum) As <System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
    End Function

    Private Enum ShowWindowEnum
        Hide = 0
        ShowNormal = 1
        ShowMinimized = 2
        ShowMaximized = 3
        Maximize = 3
        ShowNormalNoActivate = 4
        Show = 5
        Minimize = 6
        ShowMinNoActivate = 7
        ShowNoActivate = 8
        Restore = 9
        ShowDefault = 10
        ForceMinimized = 11
    End Enum

    <System.Runtime.InteropServices.DllImport("user32.dll")>
    Public Function SetForegroundWindow(hwnd As IntPtr) As Integer
    End Function

    Public Sub BringWindowToFront(ByVal processName As String)
        'Dim processName As String = "name"
        'Dim processFilePath As String = "C:\Program Files\B\B.exe"
        'get the process
        Dim bProcess As Process = Process.GetCurrentProcess '. GetProcesses ByName(processName).FirstOrDefault()
        'check if the process is nothing or not.
        If bProcess IsNot Nothing Then
            'get the  hWnd of the process
            Dim hwnd As IntPtr = bProcess.MainWindowHandle
            If hwnd = IntPtr.Zero Then
                'the window is hidden so try to restore it before setting focus.
                ShowWindow(bProcess.Handle, ShowWindowEnum.Restore)
            End If

            'set user the focus to the window
            SetForegroundWindow(CType(bProcess.MainWindowHandle, IntPtr))
            'Else
            '    'tthe process is nothing, so start it
            '    Process.Start(processName)
        End If
    End Sub

End Module