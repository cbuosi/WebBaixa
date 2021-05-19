Option Explicit On
Option Strict Off

Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Security
Imports Microsoft.Win32
'Imports mshtml
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading

#Const LOGA_INFO = False

Public Class SuperNavegador
    Inherits Windows.Forms.WebBrowser

    Public msgAlert As String
    Public btnNameClick As String = ""
    Public bExecutaThread As Boolean

#Region "CONSTANTES"

    Public Const T_TRANSICAO As Decimal = 1000
    Public Const T_TIMEOUT As Decimal = 60 * 3000 '     x segundos
    Public Const TIMEOUT_PAGINA As Decimal = 60 * 10000 'x segundos

    Private Const AJAX_CDN As String = "http://ajax.googleapis.com/ajax/libs/jquery/2.2.0/jquery.min.js"

    'Private Const NIVEL_ZOOM = 70 ' XD
    Private Const INTERNET_OPTION_END_BROWSER_SESSION As Integer = 42

    Private Property pageready As Boolean = False

    Private Const InternetExplorerRootKey As String = "Software\Microsoft\Internet Explorer"
    Private Const BrowserKeyPath As String = "Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"
    Private Const InternetCookieHttponly As Int32 = &H2000

    Private Declare Function InternetSetCookie Lib "wininet.dll" _
     Alias "InternetSetCookieA" _
     (ByVal lpszUrlName As String,
     ByVal lpszCookieName As String,
     ByVal lpszCookieData As String) As Boolean

    Private Const SW_RESTORE = 9
    Private Const VK_TAB = &H9
    Private Const WM_LBUTTONDBLCLK As Integer = &H203
    Private Const WM_LBUTTONDOWN As UInteger = &H201
    Private Const WM_LBUTTONUP As UInteger = &H202
    Private Const WM_SETTEXT = &HC
    Private Const WM_KEYDOWN = &H100
    Private Const WM_KEYUP = &H101
    Private Const VK_RETURN = &HD
    Private Const BM_CLICK As UInteger = &HF5
    Private Const WM_ACTIVATE As UInteger = &H6
    Private Const WA_ACTIVE As Integer = 1
    Private Const MOUSEEVENTF_LEFTDOWN = &H2 ' left button down
    Private Const MOUSEEVENTF_LEFTUP = &H4 ' left button up
    Private Const MOUSEEVENTF_MIDDLEDOWN = &H20 ' middle button down
    Private Const MOUSEEVENTF_MIDDLEUP = &H40 ' middle button up
    Private Const MOUSEEVENTF_RIGHTDOWN = &H8 ' right button down
    Private Const MOUSEEVENTF_RIGHTUP = &H10 ' right button up
    Private Const WM_COMMAND As UInteger = &H111
    Private Const WM_SETFOCUS = &H7
    Private Const WM_MOUSE_ACTIVATE As UInteger = &H21
    Private parentHandle As IntPtr

#End Region

#Region "DEFINIÇÕES DE APIs"

    <DllImport("wininet.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function InternetGetCookieEx(url As String, cookieName As String, cookieData As StringBuilder, ByRef size As Integer, dwFlags As Int32, lpReserved As IntPtr) As Boolean
    End Function

    <DllImport("wininet.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function InternetSetOption(hInternet As IntPtr, dwOption As Integer, lpBuffer As IntPtr, lpdwBufferLength As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetDlgCtrlID(ByVal hWndCtl As IntPtr) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)> Private Shared Function OpenProcess(ByVal dwDesiredAccess As UInteger, ByVal bInheritHandle As Boolean, ByVal dwProcessId As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="FindWindow", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function FindWindowByClass(ByVal lpClassName As String, ByVal zero As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="FindWindow", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function FindWindowByCaption(ByVal zero As IntPtr, ByVal lpWindowName As String) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function FindWindowEx(ByVal hWnd1 As IntPtr, ByVal hWnd2 As Long, ByVal lpsz1 As String, ByVal lpsz2 As String) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function EnumChildWindows(ByVal hWndParent As System.IntPtr, ByVal lpEnumFunc As EnumWindowsProc, ByVal lParam As Integer) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Sub GetClassName(ByVal hWnd As System.IntPtr, ByVal lpClassName As System.Text.StringBuilder, ByVal nMaxCount As Integer)
    End Sub

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal uMsg As UInteger, ByVal wParam As Integer, ByVal lParam As IntPtr) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal uMsg As Integer, ByVal lParam As Integer, ByVal lpData As String) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="SendMessage", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal uMsg As UInteger, ByVal wParam As Integer, ByVal lParam As StringBuilder) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function BlockInput(ByVal fBlock As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function ShowCursor(ByVal lShow As Long) As Long
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function RegisterWindowMessage(lpString As String) As Integer
    End Function

    Const WM_GETTEXT As Integer = &HD
    Const WM_GETTEXTLENGTH As Integer = &HE

    Private Delegate Function EnumWindowsProc(ByVal hWnd As IntPtr, ByVal lParam As IntPtr) As Boolean

#End Region

#Region "ENUMERADORES"

    Public Property EvitaNovaJanela As Boolean = False
    Public slink As String = ""

    Public Enum Exec
        OLECMDID_OPTICAL_ZOOM = 63
    End Enum

    Private Enum execOpt
        OLECMDEXECOPT_DODEFAULT = 0
        OLECMDEXECOPT_PROMPTUSER = 1
        OLECMDEXECOPT_DONTPROMPTUSER = 2
        OLECMDEXECOPT_SHOWHELP = 3
    End Enum

    Public Enum BrowserEmulationVersion
        [Default] = 0
        ERRO_SEGURANCA = 1
        ERRO_AUTORIZACAO = 2
        Version6 = 6000
        Version7 = 7000
        Version8 = 8000
        Version8Standards = 8888
        Version9 = 9000
        Version9Standards = 9999
        Version10 = 10000
        Version10Standards = 10001
        Version11 = 11000
        Version11Edge = 11001
    End Enum

    Public Enum eCBPTWEB
        DESCONHECIDO = 0
        CBPTWEB_OK
        DESCONECTADO
        TIMEOUT
        ARQUIVO_INEXISTENTE
        GENERICO
    End Enum

    Public Enum eAutenticacao
        DESCONHECIDO = 0
        CAMPOS_FALTANTES
        LOGOUT
        LOGADO_OK
        ERRO_USUARIO_SENHA
        USUARIO_BLOQUEADO
        ERRO_CAPTCHA
        TIMEOUT
        SITE_INVALIDO
        SEM_INTERNET
        GENERICO
    End Enum

#End Region

#Region "VARIAVEIS GLOBAIS"

    Public e_Sttus As eAutenticacao
    Public Usuario As String = ""     'Esta NÃO É O USUARIO da autenticação do SITE, é o usuario do PROXY (se houver)
    Public Senha As String = ""       'Esta NÃO É A SENHA da autenticação do SITE, é a senha do PROXY (se houver)

    Public txtInfo As SuperTextBox = Nothing

#End Region

    Public Sub PerformZoom(ByVal Value As Integer)

        Dim Res As Object = Nothing
        Dim MyWeb As Object

        Try

            MyWeb = ActiveXInstance
            MyWeb.ExecWB(Exec.OLECMDID_OPTICAL_ZOOM, execOpt.OLECMDEXECOPT_PROMPTUSER, CObj(Value), CObj(IntPtr.Zero))
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
        End Try

    End Sub

    'Public Sub SetaInfo(ByRef _txtInfo As SuperTextBox)
    '    txtInfo = _txtInfo
    'End Sub

    Protected Overrides Sub OnNewWindow(ByVal e As CancelEventArgs)

        If EvitaNovaJanela = True Then
            'Me.Navigate(Me.StatusText)
            e.Cancel = True
        End If

        MyBase.OnNewWindow(e)

#If ORIGINAL Then

        Dim NovoUrl As String = ""
        Dim webBrowser As New SuperNavegador

        Dim CookiesArr As String() = Document.Cookie.Split(";")
        For Each Cookie In CookiesArr
            Dim Idx As Long = Cookie.IndexOf("=")
            If Idx <> -1 Then
                Dim CookieName As String = Cookie.Substring(0, Idx).Trim
                Dim CookieValue As String = Cookie.Substring(Idx + 1).Trim
                InternetSetCookie(Me.Url.ToString, Nothing, CookieName + " = " + CookieValue + ";")
            End If
        Next

        'Se setou para PREVINIR UMA NOVA JANELA....
        If EvitaNovaJanela = True Then
            EvitaNovaJanela = False
            NovoUrl = Me.Url.ToString
            e.Cancel = True

            webBrowser = Me
            webBrowser.Navegar(NovoUrl)
            AguardaProc(2000)
            webBrowser.AguardaBrowserOk()

            'Navigate(NovoUrl)
        Else
            MyBase.OnNewWindow(e)
        End If
#End If

    End Sub

    'Public Function NavegarSite(ByVal _eSites As String,
    '                            Optional ByRef ImgCaptcha As Image = Nothing) As Boolean
    '
    '    If Navegar(_eSites, True, True) = False Then
    '        If Navegar(_eSites, True, True) = False Then
    '            Return False
    '        End If
    '    End If
    '
    '    PerformZoom(NIVEL_ZOOM)
    '
    '    Return True
    '
    'End Function

    Public Function Navegar(ByVal strAlvo As String,
                            Optional bFinalizaSessao As Boolean = False,
                            Optional bAutenticacao As Boolean = False,
                            Optional bTela As Boolean = False) As Boolean

        Dim oRelogio As Stopwatch = Nothing
        Dim strHeader As String = ""
        Dim strNav As String

        Try

            mensagemBarraStatusNav("Navegando para: [" & strAlvo & "] TIMEOUT: [" & (T_TIMEOUT / 1000).ToString("0.00") & "] segundos. - Finaliza sessão: [" & IIf(bFinalizaSessao, "Sim", "Não").ToString & "].")

            oRelogio = New Stopwatch
            oRelogio.Start()

            ScriptErrorsSuppressed = True

            If bFinalizaSessao = True Then
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0)
                If Document <> Nothing Then
                    Document.ExecCommand("ClearAuthenticationCache", False, Nothing)
                End If
            End If

            If bAutenticacao = True Then

                strHeader = "Authorization: Basic " &
                            Convert.ToBase64String(Encoding.ASCII.GetBytes(Usuario & ":" + Senha)) &
                            System.Environment.NewLine

                If strAlvo.Contains("https://") Then '                                                Ex: https://www.buosi.org
                    strNav = String.Format("https://{0}:{1}@" & strAlvo.Substring(8), Usuario, Senha)
                ElseIf strAlvo.Contains("http://") Then '                                             Ex: http://www.buosi.org
                    strNav = String.Format("http://{0}:{1}@" & strAlvo.Substring(7), Usuario, Senha)
                Else '                                                                                Ex: www.buosi.org
                    strNav = String.Format("{0}:{1}@" & strAlvo, Usuario, Senha)
                End If

                'Segundo parametro:
                '       A case-sensitive string expression that evaluates to the name of the frame
                '       in which to display the resource. The possible values for this parameter are:
                '       -----------------------------------------------------------------------------------------------
                '           "_blank"  - Load the link into a New unnamed window.
                '           "_parent" - Load the link into the immediate parent of the document the link Is in.
                '           "_self"   - Load the link into the same window the link was clicked in.
                '           "_top"    - Load the link into the full body of the current window.

                If bTela Then
                    Navigate(strNav, "_top", Nothing, strHeader)
                Else
                    Navigate(strNav, False)
                End If
            Else
                Navigate(strAlvo, False)
            End If

            If strAlvo.Contains("about:blank") Then
                If CapturarAlertaNavegador() = False Then
                    Return False
                End If
            End If

            'If Not strAlvo.Contains("DirOrigem") Then
            '    If AguardaBrowserOk() = False Then
            '        mensagemBarraStatusNav("TIME_OUT!!!")
            '        Return False
            '    End If
            'End If

            oRelogio.Stop()
#If LOGA_INFO Then
            mensagemBarraStatusNav(ReadyState.ToString & " [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs.")
#End If

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ":  " & ex.Message)
            Return False
        End Try

    End Function

    Public Function GetInternetExplorerMajorVersion() As Integer

        Dim result As Integer

        result = 0

        Try
            Dim key As RegistryKey

            key = Registry.LocalMachine.OpenSubKey(InternetExplorerRootKey)

            If key IsNot Nothing Then
                Dim value As Object

                value = If(key.GetValue("svcVersion", Nothing), key.GetValue("Version", Nothing))

                If value IsNot Nothing Then
                    Dim version As String
                    Dim separator As Integer

                    version = value.ToString()
                    separator = version.IndexOf("."c)
                    If separator <> -1 Then
                        Integer.TryParse(version.Substring(0, separator), result)
                    End If
                End If
            End If

            ' The user does not have the permissions required to read from the registry key.
        Catch generatedExceptionName As SecurityException
            LogaErro("(SecurityException) Erro em " & NomeMetodo(Me) & ": " & generatedExceptionName.Message)
            ' The user does not have the necessary registry rights.
        Catch generatedExceptionName As UnauthorizedAccessException
            LogaErro("(UnauthorizedAccessException) Erro em " & NomeMetodo(Me) & ": " & generatedExceptionName.Message)
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
        End Try

        Return result

    End Function

    Private Sub RemoveBrowerKey()
        Dim key As Microsoft.Win32.RegistryKey
        key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(BrowserKeyPath.Substring(1), True)
        key.DeleteValue(Process.GetCurrentProcess.ProcessName & ".exe", False)
    End Sub

    Public Sub CreateBrowserKeyxxx(ByVal verIE As BrowserEmulationVersion,
                                Optional ByVal IgnoreIDocDirective As Boolean = False)

        Dim basekey As String
        'Dim value As Int32
        Dim thisAppsName As String

        Try

            basekey = Microsoft.Win32.Registry.CurrentUser.ToString

            thisAppsName = My.Application.Info.AssemblyName & ".exe"

            Microsoft.Win32.Registry.SetValue(Microsoft.Win32.Registry.CurrentUser.ToString & "\" & BrowserKeyPath,
                                                thisAppsName,
                                                CInt(verIE),
                                                Microsoft.Win32.RegistryValueKind.DWord)

            Microsoft.Win32.Registry.SetValue(Microsoft.Win32.Registry.CurrentUser.ToString & "\" & BrowserKeyPath,
                                                Process.GetCurrentProcess.ProcessName & ".exe",
                                                CInt(verIE),
                                                Microsoft.Win32.RegistryValueKind.DWord)
        Catch exS As SecurityException
            LogaErro("Erro em " & NomeMetodo(Me) & " (SecurityException): " & exS.Message)
        Catch exU As UnauthorizedAccessException
            LogaErro("Erro em " & NomeMetodo(Me) & " (UnauthorizedAccessException): " & exU.Message)
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
        End Try

    End Sub

    Public Sub AguardaProc(ByVal interval As Integer)

        Dim stopW As Stopwatch = Nothing

        Try

            stopW = New Stopwatch
            stopW.Start()
            Do While stopW.ElapsedMilliseconds < interval
                Application.DoEvents()
            Loop
            stopW.Stop()
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
        Finally
            stopW = Nothing
        End Try

    End Sub

    'Public Function InjetarJQuery() As Boolean
    '
    '    Dim head As HtmlElement
    '    Dim scriptEl As HtmlElement
    '    Dim element As IHTMLScriptElement
    '
    '    Try
    '
    '        If AguardaBrowserOk() = False Then
    '            Return False
    '        End If
    '
    '        head = Document.GetElementsByTagName("head")(0)
    '        scriptEl = Document.CreateElement("script")
    '        element = DirectCast(scriptEl.DomElement, IHTMLScriptElement)
    '        element.src = AJAX_CDN
    '        element.type = "text/javascript"
    '
    '        head.AppendChild(scriptEl)
    '
    '        Return True
    '    Catch ex As Exception
    '        LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
    '        Return False
    '    End Try
    '
    'End Function

    Public Function ExecutaJS(ByVal jCode As String) As Boolean

        Try

            If AguardaBrowserOk() = False Then
                Return False
            End If

            Document.InvokeScript("eval", New Object() {jCode})

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Function SetarValor_Name(ByVal NomeElemento As String,
                                ByVal ValorElemento As String) As Boolean

        Dim bExisteElemento As Boolean

        Try

            If AguardaBrowserOk() = False Then
                Return False
            End If

            bExisteElemento = False

            For Each element As HtmlElement In Document.All

                If element.Name = NomeElemento Then
                    bExisteElemento = True
                    Exit For
                End If

            Next

            If bExisteElemento = False Then

#If LOGA_INFO Then
                mensagemBarraStatusNav("Não existe elemento de nome [" & NomeElemento & "] nesta página!!!")
#End If

                Return False
            End If

            Dim jCode As String = ""
            jCode = "document.getElementsByName(""" & NomeElemento & """)(0).value = """ & ValorElemento & """"

            Document.InvokeScript("eval", New Object() {jCode})

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Function SetarValor_Id(ByVal IdElemento As String,
                            ByVal ValorElemento As String) As Boolean

        Dim bExisteElemento As Boolean

        Try

            If AguardaBrowserOk() = False Then
                Return False
            End If

            bExisteElemento = False

            For Each element As HtmlElement In Document.All

                If element.Id = IdElemento Then
                    bExisteElemento = True
                    Exit For
                End If

            Next

            If bExisteElemento = False Then

#If LOGA_INFO Then
                mensagemBarraStatusNav("Não existe elemento de Id [" & IdElemento & "] nesta página!!!")
#End If

                Return False
            End If

            Dim jCode As String = ""
            jCode = "document.getElementById(""" & IdElemento & """).value = """ & ValorElemento & """"

            Document.InvokeScript("eval", New Object() {jCode})

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Function Clica_Nome(ByVal NomeElemento As String) As Boolean

        Dim bExisteElemento As Boolean

        Try

            bExisteElemento = False

            If AguardaBrowserOk() = False Then
                Return False
            End If

            For Each element As HtmlElement In Document.All

                If element.Name = NomeElemento Then
                    bExisteElemento = True
                    Exit For
                End If

            Next

            If bExisteElemento = False Then

#If LOGA_INFO Then
                mensagemBarraStatusNav("Não existe elemento de nome [" & NomeElemento & "] nesta página!!!")
#End If

                Return False
            End If

            Dim jCode As String = ""
            jCode = "document.getElementsByName(""" & NomeElemento & """)(0).click();"

            Document.InvokeScript("eval", New Object() {jCode})

            If AguardaBrowserOk() = False Then
                Return False
            End If

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Function Clica_Id(ByVal IdElemento As String) As Boolean

        Dim bExisteElemento As Boolean

        Try

            bExisteElemento = False

            If AguardaBrowserOk() = False Then
                Return False
            End If

            For Each element As HtmlElement In Document.All

                If element.Id = IdElemento Then
                    bExisteElemento = True
                    Exit For
                End If

            Next

            If bExisteElemento = False Then

#If LOGA_INFO Then
                mensagemBarraStatusNav("Não existe elemento de id [" & IdElemento & "] nesta página!!!")
#End If

                Return False
            End If

            Dim jCode As String = ""
            jCode = "document.getElementById(""" & IdElemento & """).click();"

            Document.InvokeScript("eval", New Object() {jCode})

            If AguardaBrowserOk() = False Then
                Return False
            End If

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Function Clica_Classe(ByVal NomeClasse As String) As Boolean

        Try

            If AguardaBrowserOk() = False Then
                Return False
            End If

            Dim jCode As String = ""
            jCode = "document.getElementsByClassName(""" & NomeClasse & """)(0).click();"

            Document.InvokeScript("eval", New Object() {jCode})

            If AguardaBrowserOk() = False Then
                Return False
            End If

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    '    Function ObterImagem_Id(ByVal IdElemento As String) As Image
    '
    '        Dim doc As IHTMLDocument2
    '        Dim imgRange As IHTMLControlRange
    '        Dim oBmp As Bitmap
    '        Dim bExisteElemento As Boolean
    '
    '        Try
    '
    '            If AguardaBrowserOk() = False Then
    '                Return Nothing
    '            End If
    '
    '            bExisteElemento = False
    '
    '            For Each element As HtmlElement In Document.All
    '
    '                If element.Id = IdElemento Then
    '                    bExisteElemento = True
    '                    Exit For
    '                End If
    '
    '            Next
    '
    '            If bExisteElemento = False Then
    '
    '#If LOGA_INFO Then
    '                mensagemBarraStatusNav("Não existe elemento de id [" & IdElemento & "] nesta página!!!")
    '#End If
    '
    '                Return Nothing
    '            End If
    '
    '            doc = DirectCast(Document.DomDocument, IHTMLDocument2)
    '            imgRange = DirectCast(DirectCast(doc.body, HTMLBody).createControlRange(), IHTMLControlRange)
    '
    '            For Each img As mshtml.IHTMLImgElement In doc.images
    '
    '                If img.nameProp.ToUpper.Contains("CAPTCHA") = True Then 'Achou?
    '
    '                    imgRange.add(DirectCast(img, IHTMLControlElement))
    '                    imgRange.execCommand("Copy", False, Nothing)
    '                    oBmp = DirectCast(Clipboard.GetDataObject().GetData(DataFormats.Bitmap), Bitmap)
    '
    '                    Return oBmp
    '
    '                End If
    '
    '            Next
    '
    '            Return Nothing
    '        Catch Ex As System.Exception
    '            LogaErro("Erro em " & NomeMetodo(Me) & ": " & Ex.Message)
    '            Return Nothing
    '        End Try
    '
    '    End Function

    Public Function ClickEnviar(ByVal IdElemento As String,
                                ByVal TagElement As String) As Boolean

        Dim elemento As System.Windows.Forms.HtmlElementCollection
        Dim elemento2 As System.Windows.Forms.HtmlElementCollection

        Try

            If AguardaBrowserOk() = False Then
                Return False
            End If

            elemento = Document.GetElementById(IdElemento).Children

            For Each echild As System.Windows.Forms.HtmlElement In elemento

                elemento2 = CType(echild.Children, System.Windows.Forms.HtmlElementCollection)

                For i = 0 To elemento2.Count - 1 Step 1

                    If elemento2(i).InnerHtml.Contains("Enviar Arquivo") = True Then

                        Dim els As System.Windows.Forms.HtmlElementCollection = CType(elemento2(i).GetElementsByTagName(TagElement), System.Windows.Forms.HtmlElementCollection)
                        For Each el As HtmlElement In els

                            el.InvokeMember("click")
                            'WaitForPageLoad()
                            If AguardaBrowserOk() = False Then
                                Return False
                            End If
                            Return True

                        Next

                    End If

                Next i

            Next echild

            Return False ' nao achou...
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Private Sub SelecionaArquivoUpload()

        Dim oRelogio As Stopwatch = Nothing
        Dim iDialog As IntPtr = IntPtr.Zero
        Dim iBtnAbrir As IntPtr

        AguardaProc(T_TRANSICAO)

        ' 1 - Posicionar em
        ' - Window Caption = "Escolher arquivo a carregar"
        ' - Class name = "#32770 (Dialog)"

        oRelogio = New Stopwatch
        oRelogio.Start()

        While iDialog = IntPtr.Zero And oRelogio.Elapsed.Milliseconds < T_TIMEOUT * 30
            Thread.Sleep(100)
#If GFT Then
            iDialog = FindWindow("#32770", "Choose File to Upload") '"Escolher arquivo a carregar")
#Else
            iDialog = FindWindow("#32770", "Escolher arquivo a carregar") '"Escolher arquivo a carregar")
#End If

        End While

        If iDialog = IntPtr.Zero Then
            'Time_out
            Exit Sub
        End If

        SetForegroundWindow(iDialog)
        '
        '
        '2 - Setar valor (caminho) em:
        ' - Window Caption = ""
        ' - Class name = "Edit"
        AguardaProc(2000)

        Dim ChildrenList As New List(Of IntPtr)
        Dim ListHandle As GCHandle = GCHandle.Alloc(ChildrenList)
        EnumChildWindows(iDialog, AddressOf EnumWindow, GCHandle.ToIntPtr(ListHandle))

        AguardaProc(T_TRANSICAO)

        '3 executar botao:
        ' - Window Caption = "&Abrir"
        ' - Class name = "Button"

#If GFT Then
        iBtnAbrir = FindWindowEx(iDialog, IntPtr.Zero, "&Open", Nothing)
#Else
        iBtnAbrir = FindWindowEx(iDialog, IntPtr.Zero, "&Abrir", Nothing)
#End If

        SendMessage(iBtnAbrir, BM_CLICK, 0, IntPtr.Zero)

    End Sub

    Public Function CapturarAlert() As String

        Dim iAlert As IntPtr = IntPtr.Zero
        Dim iBtnOK As IntPtr = IntPtr.Zero
        Dim oRelogio As Stopwatch = Nothing

        Try

            'Alan
            AguardaProc(200)

            msgAlert = ""

            oRelogio = New Stopwatch
            oRelogio.Start()

            While iAlert = IntPtr.Zero And
                  oRelogio.Elapsed.Milliseconds < T_TIMEOUT * 50
                iAlert = FindWindow("#32770", "Mensagem da página da web")
                AguardaProc(100)
            End While

            oRelogio.Stop()
            If iAlert = IntPtr.Zero Then
                'Time_out
                Return Nothing
            End If

            Debug.Print("iAlert " & Hex(CInt(iAlert)))

            'Alan
            AguardaProc(200)

            Dim ChildrenList As New List(Of IntPtr)
            Dim ListHandle As GCHandle = GCHandle.Alloc(ChildrenList)
            EnumChildWindows(iAlert, AddressOf JanelaAlert, GCHandle.ToIntPtr(ListHandle))

            iBtnOK = FindWindowEx(iAlert, IntPtr.Zero, btnNameClick, Nothing)
            Debug.Print(btnNameClick & Hex(CInt(iBtnOK)))

            SendMessage(iBtnOK, BM_CLICK, 0, IntPtr.Zero)

            Return msgAlert
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function CapturarAlert2() As String

        Dim iAlert As IntPtr = IntPtr.Zero
        Dim iBtnOK As IntPtr = IntPtr.Zero
        Dim iBtnCancel As IntPtr = IntPtr.Zero
        Dim oRelogio As Stopwatch = Nothing

        Try

            AguardaProc(2000)

            msgAlert = ""

            oRelogio = New Stopwatch
            oRelogio.Start()

            While iAlert = IntPtr.Zero

                iAlert = FindWindow("#32770", "Mensagem da página da web") '"Escolher arquivo a carregar")

                If bExecutaThread = False Then
                    Return False
                End If

                If oRelogio.Elapsed.Milliseconds >= T_TIMEOUT Then
                    msgAlert = "TIMEOUT"
                    Return False
                End If

                System.Threading.Thread.Sleep(100)

            End While

            oRelogio.Stop()
            If iAlert = IntPtr.Zero Then
                'Time_out
                Return Nothing
            End If

            'Debug.Print("iAlert " & Hex(CInt(iAlert)))

            AguardaProc(T_TRANSICAO)

            Dim ChildrenList As New List(Of IntPtr)
            Dim ListHandle As GCHandle = GCHandle.Alloc(ChildrenList)
            EnumChildWindows(iAlert, AddressOf JanelaAlert, GCHandle.ToIntPtr(ListHandle))

            iBtnOK = FindWindowEx(iAlert, IntPtr.Zero, "OK", Nothing)
            iBtnCancel = FindWindowEx(iAlert, IntPtr.Zero, "CANCELAR", Nothing)

            If iBtnCancel <> IntPtr.Zero Then
                SendMessage(iBtnCancel, BM_CLICK, 0, IntPtr.Zero)
            Else
                SendMessage(iBtnOK, BM_CLICK, 0, IntPtr.Zero)
            End If

            Return msgAlert
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return Nothing
        Finally
            bExecutaThread = False
        End Try

    End Function

    Public Function SetarArquivoInput(ByVal IdElemento As String,
                                      ByVal strCaminhoArquivo As String,
                                      Optional Indice As Integer = 0) As Boolean

        Dim oFile As System.Windows.Forms.HtmlElement
        Dim bExisteElementoId As Boolean
        Dim bExisteElementoName As Boolean
        Dim bExisteElementoTagName As Boolean

        Dim tSelecionaArquivoUpload As New Threading.Thread(AddressOf SelecionaArquivoUpload)

        Try

            arqCaminho = strCaminhoArquivo

            If AguardaBrowserOk() = False Then
                Return False
            End If

            bExisteElementoId = False
            bExisteElementoName = False
            bExisteElementoTagName = False

            For Each element As HtmlElement In Document.All

                'mensagemBarraStatusNav(element.Id)
                If element.Id = IdElemento Then
                    bExisteElementoId = True
                    Exit For
                End If

                'mensagemBarraStatusNav(element.Name)
                If element.Name = IdElemento Then
                    bExisteElementoName = True
                    Exit For
                End If

                'mensagemBarraStatusNav(element.TagName)
                If element.TagName = IdElemento.ToUpper Then
                    bExisteElementoTagName = True
                    Exit For
                End If

            Next

            If bExisteElementoId = False AndAlso bExisteElementoName = False AndAlso bExisteElementoTagName = False Then

#If LOGA_INFO Then
                mensagemBarraStatusNav("Não existe elemento de tagname [" & IdElemento & "] nesta página!!!")
#End If

                Return False
            End If

            'Focus()
            BringWindowToFront("ModuloGerenciador.vshost")
            BringWindowToFront("ModuloGerenciador")
            'SetarInteracao(True)

            If bExisteElementoId = True Then
                oFile = Document.GetElementById(IdElemento)
            ElseIf bExisteElementoName = True Then
                oFile = Document.All.GetElementsByName(IdElemento).Item(Indice)
            Else

                oFile = Document.GetElementsByTagName(IdElemento).Item(Indice)
            End If

            tSelecionaArquivoUpload.IsBackground = True
            tSelecionaArquivoUpload.Start()

            oFile.Focus()
            oFile.InvokeMember("click")

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Public Function AguardaBrowserOk(Optional TempoInicialEspera As Integer = 100) As Boolean

        Dim oTime As Stopwatch = Nothing

        Try

            AguardaProc(TempoInicialEspera)

            oTime = New Stopwatch
            oTime.Start()

            mensagemBarraStatusNav("Aguardando página...", False)

            While ReadyState <> WebBrowserReadyState.Complete 'And ReadyState <> WebBrowserReadyState.Interactive)
                AguardaProc(250)
                'AguardaProc(T_TRANSICAO / 10)
#If LOGA_INFO Then
                mensagemBarraStatusNav("ReadyState: " & ReadyState.ToString)
#End If
                If oTime.ElapsedMilliseconds > T_TIMEOUT Then
                    mensagemBarraStatusNav("AguardaBrowserOk: TIMEOUT (1)")
                    Return False
                End If
                mensagemBarraStatusNav(".", False)

            End While

            oTime = New Stopwatch
            oTime.Start()

            While (IsBusy = True)
                AguardaProc(T_TRANSICAO / 10)
#If LOGA_INFO Then
                mensagemBarraStatusNav("IsBusy: " & IsBusy.ToString)
#End If
                If oTime.ElapsedMilliseconds > T_TIMEOUT Then
                    mensagemBarraStatusNav("AguardaBrowserOk: TIMEOUT (2) [" & (oTime.ElapsedMilliseconds / 1000).ToString & "] segs.")
                    Return False
                End If
            End While

            If DocumentText.Contains("Esta página não pode ser exibida") Then
                mensagemBarraStatusNav("AguardaBrowserOk: Esta página não pode ser exibida [" & (oTime.ElapsedMilliseconds / 1000).ToString & "] segs.")
                Return False
            End If

            If Document.Title.ToUpper.Contains("ERRO") Then
                mensagemBarraStatusNav("AguardaBrowserOk: ERRO [" & (oTime.ElapsedMilliseconds / 1000).ToString & "] segs.")
                Return False
            End If

            mensagemBarraStatusNav(".ok!")
            'mensagemBarraStatusNav("AguardaBrowserOk: OK! [" & (oTime.ElapsedMilliseconds / 1000).ToString & "] segs.")
            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        Finally
            oTime = Nothing
        End Try

    End Function

    Private Sub PageWaiter(ByVal sender As Object, ByVal e As WebBrowserDocumentCompletedEventArgs)

        If ReadyState = WebBrowserReadyState.Complete Then
            pageready = True
            RemoveHandler DocumentCompleted, New WebBrowserDocumentCompletedEventHandler(AddressOf PageWaiter)
        End If

    End Sub

    Public Function ClicarBotao_Tipo(ByVal IdElemento As String,
                                     Optional Indice As Integer = 0) As Boolean

        Dim oFile As System.Windows.Forms.HtmlElement
        Dim bExisteElemento As Boolean

        Try

            If AguardaBrowserOk() = False Then
                Return False
            End If

            bExisteElemento = False

            For Each element As HtmlElement In Document.All

                If element.TagName = IdElemento.ToUpper Then
                    bExisteElemento = True
                    Exit For
                End If

            Next

            If bExisteElemento = False Then
#If LOGA_INFO Then
                mensagemBarraStatusNav("Não existe elemento de tagname [" & IdElemento & "] nesta página!!!")
#End If
                Return False
            End If

            oFile = Document.GetElementsByTagName(IdElemento).Item(Indice)
            oFile.InvokeMember("click")

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Sub SetarInteracao(Optional ByVal Interacao As Boolean = False)
        Try
            If Interacao = True Then
                BlockInput(1)
                ShowCursor(0)
            Else
                BlockInput(0)
                ShowCursor(1)
            End If
        Catch ex As Exception
            LogaErro("Erro em SuperNavegador::SetarInteracao: " & ex.Message)
        End Try
    End Sub

    Sub Info()

        If Not txtInfo Is Nothing Then
            txtInfo.Text = ""
        End If

        mensagemBarraStatusNav("WebBrowser Versão: " & Version.ToString & " - IE: " & ObterVersaoNavegador.ToString)

    End Sub

    Function SetarCheckbox_Id(ByVal IdElemento As String,
                                ByVal Checado As Boolean) As Boolean

        Dim bExisteElemento As Boolean

        Try

            If AguardaBrowserOk() = False Then
                Return False
            End If

            bExisteElemento = False

            For Each element As HtmlElement In Document.All

                If element.Id = IdElemento Then
                    bExisteElemento = True
                    Exit For
                End If

            Next

            If bExisteElemento = False Then
#If LOGA_INFO Then
                mensagemBarraStatusNav("Não existe elemento de Id [" & IdElemento & "] nesta página!!!")
#End If
                Return False
            End If

            Dim jCode As String = ""
            jCode = "document.getElementById(""" & IdElemento & """).checked = " & Checado.ToString.ToLower & ";"

            Document.InvokeScript("eval", New Object() {jCode})

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Function ObterMensagem_Id(ByVal IdElemento As String) As String

        Dim bExisteElemento As Boolean

        Try

            If AguardaBrowserOk() = False Then
                Return Nothing
            End If

            bExisteElemento = False

            For Each element As HtmlElement In Document.All

                If element.Id = IdElemento Then
                    bExisteElemento = True
                    Exit For
                End If

            Next

            If bExisteElemento = False Then
#If LOGA_INFO Then
                mensagemBarraStatusNav("Não existe elemento de id [" & IdElemento & "] nesta página!!!")
#End If
                Return Nothing
            End If

            Dim oele As HtmlElement
            Dim oele2 As HtmlElement

            oele = Document.GetElementById(IdElemento)

            If oele.Children.Count > 0 Then
                oele2 = oele.Children(0)

                If oele2.InnerHtml Is Nothing Then
                    Return ""
                Else
                    Return oele2.InnerHtml
                End If

            End If

            If oele.InnerHtml Is Nothing Then
                Return ""
            Else
                Return oele.InnerHtml
            End If
        Catch Ex As System.Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & Ex.Message)
            Return Nothing
        End Try

    End Function

    Function ObterMensagem_Class(ByVal ClassElemento As String) As String

        Dim bExisteElemento As Boolean
        Dim oele As HtmlElement

        Try

            oele = Nothing

            If AguardaBrowserOk() = False Then
                Return Nothing
            End If

            bExisteElemento = False
            For Each curElement As HtmlElement In Document.GetElementsByTagName("div")
                If curElement.Id = "" Then
                    If curElement.OuterHtml.Contains(ClassElemento) Then
                        bExisteElemento = True
                        oele = curElement
                        Exit For
                    End If
                End If

            Next

            If bExisteElemento = False Then
#If LOGA_INFO Then
                mensagemBarraStatusNav("Não existe elemento de id [" & IdElemento & "] nesta página!!!")
#End If
                Return Nothing
            End If

            Return oele.InnerText
        Catch Ex As System.Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & Ex.Message)
            Return Nothing
        End Try

    End Function

    Function ExisteID(ByVal IdElemento As String) As Boolean

        Dim bExisteElemento As Boolean

        Try

            If AguardaBrowserOk() = False Then
                Return False
            End If

            bExisteElemento = False

            For Each element As HtmlElement In Document.All
                Dim newelement As String

                If element.Id Is Nothing Then
                    newelement = element.Id
                Else
                    newelement = element.Id.ToUpper()
                End If
                If newelement = IdElemento.ToUpper Then
                    bExisteElemento = True
                    Exit For
                End If

            Next

            Return bExisteElemento
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Function ObterVersaoNavegador() As BrowserEmulationVersion

        Dim strUserAgent As String

        strUserAgent = GetUserAgentAtual()

        If strUserAgent.Contains("rv:11") Then 'Mozilla/5.0 (Windows NT 6.2; WOW64; Trident/7.0; rv:11.0) like Gecko
            Return BrowserEmulationVersion.Version11
        ElseIf strUserAgent.Contains("MSIE 10.6") Then 'Mozilla/5.0 (compatible; MSIE 10.6; Windows NT 6.1; Trident/5.0; InfoPath.2; SLCC1; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 2.0.50727) 3gpp-gba UNTRUSTED/1.0
            Return BrowserEmulationVersion.Version10
        ElseIf strUserAgent.Contains("MSIE 10.0") Then 'Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 7.0; InfoPath.3; .NET CLR 3.1.40767; Trident/6.0; en-IN)
            Return BrowserEmulationVersion.Version10
        ElseIf strUserAgent.Contains("MSIE 9.0") Then 'Mozilla/5.0 (Windows; U; MSIE 9.0; WIndows NT 9.0; en-US))
            Return BrowserEmulationVersion.Version9
        ElseIf strUserAgent.Contains("MSIE 8.0") Then 'Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; GTB7.4; InfoPath.2; SV1; .NET CLR 3.3.69573; WOW64; en-US)
            Return BrowserEmulationVersion.Version8
        ElseIf strUserAgent.Contains("MSIE 7.0") Then 'Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; en-US)
            Return BrowserEmulationVersion.Version7
        ElseIf strUserAgent.Contains("MSIE 6.1") Then 'Mozilla/4.0 (compatible; MSIE 6.1; Windows XP; .NET CLR 1.1.4322; .NET CLR 2.0.50727)
            Return BrowserEmulationVersion.Version6
        ElseIf strUserAgent.Contains("MSIE 6.0") Then 'Mozilla/5.0 (Windows; U; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)
            Return BrowserEmulationVersion.Version6
        Else
            Return BrowserEmulationVersion.Default
        End If

    End Function

    'Public Function GetUserAgent() As String
    '
    '    Dim userAgent As String = Nothing
    '
    '    Try
    '
    '        Dim javascript As String = "<script type='text/javascript'>" +
    '                                    "function getUserAgent()" +
    '                                    "{document.write(navigator.userAgent)}" +
    '                                    "</script>"
    '
    '        Dim webBrowser As System.Windows.Forms.WebBrowser = New System.Windows.Forms.WebBrowser()
    '        webBrowser.Url = New System.Uri("about:blank")
    '        webBrowser.Document.Write(javascript)
    '        webBrowser.Document.InvokeScript("getUserAgent")
    '
    '        userAgent = webBrowser.DocumentText.Substring(javascript.Length)
    '
    '        webBrowser.Dispose()
    '
    '        webBrowser = Nothing
    '
    '    Catch ex As Exception
    '        Return "erro"
    '    End Try
    '
    '    Return userAgent
    '
    'End Function

    Public Function GetUserAgentAtual() As String

        'Dim userAgent As String = Nothing

        Try

            Dim javascript As String = "<script type='text/javascript'>" +
                                        "function getUserAgent()" +
                                        "{document.write(navigator.userAgent)}" +
                                        "</script>"

            'Dim webBrowser As System.Windows.Forms.WebBrowser = New System.Windows.Forms.WebBrowser()
            Me.Url = New System.Uri("about:blank")
            Me.Document.Write(javascript)
            Me.Document.InvokeScript("getUserAgent")

            Return Me.DocumentText.Substring(javascript.Length)
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return "Erro em " & NomeMetodo(Me) & ": " & ex.Message
        End Try

    End Function

    Function Envia_jCode(ByVal jCode As String) As Boolean

        Try

            If AguardaBrowserOk() = False Then
                Return False
            End If

            Document.InvokeScript("eval", New Object() {jCode})

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Private Function CapturarSalvar() As String

        Dim iAlert As IntPtr = IntPtr.Zero
        Dim iBtnOK As IntPtr = IntPtr.Zero
        Dim oRelogio As Stopwatch = Nothing

        Try

            AguardaProc(T_TRANSICAO)

            msgAlert = ""

            oRelogio = New Stopwatch
            oRelogio.Start()

            While iAlert = IntPtr.Zero And oRelogio.Elapsed.Milliseconds < T_TIMEOUT

                iAlert = FindWindow("#32770", "Download de Arquivos") '"Escolher arquivo a carregar")

            End While

            oRelogio.Stop()
            If iAlert = IntPtr.Zero Then
                'Time_out
                Return Nothing
            End If

            Debug.Print("iAlert " & Hex(CInt(iAlert)))

            AguardaProc(T_TRANSICAO)

            Dim ChildrenList As New List(Of IntPtr)
            Dim ListHandle As GCHandle = GCHandle.Alloc(ChildrenList)
            EnumChildWindows(iAlert, AddressOf JanelaAlert, GCHandle.ToIntPtr(ListHandle))

            iBtnOK = FindWindowEx(iAlert, IntPtr.Zero, "&Salvar", Nothing)
            Debug.Print("iBtnOK " & Hex(CInt(iBtnOK)))

            SendMessage(iBtnOK, BM_CLICK, 0, IntPtr.Zero)

            Return msgAlert
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return Nothing
        End Try

    End Function

    Private Function SelecionaArquivoUpload2() As String

        Dim oRelogio As Stopwatch = Nothing
        Dim iDialog As IntPtr = IntPtr.Zero
        Dim iBtnSalvar As IntPtr

        Try
            AguardaProc(T_TRANSICAO)

            msgAlert = ""

            ' 1 - Posicionar em
            ' - Window Caption = "Escolher arquivo a carregar"
            ' - Class name = "#32770 (Dialog)"

            oRelogio = New Stopwatch
            oRelogio.Start()

            While iDialog = IntPtr.Zero And oRelogio.Elapsed.Milliseconds < T_TIMEOUT

                iDialog = FindWindow("#32770", "Salvar como") '"Escolher arquivo a carregar")

            End While

            If iDialog = IntPtr.Zero Then
                'Time_out
                Return Nothing
            End If

            BringWindowToFront("ModuloGerenciador.vshost")

            '
            '
            '2 - Setar valor (caminho) em:
            ' - Window Caption = ""
            ' - Class name = "Edit"
            AguardaProc(T_TRANSICAO)

            Dim ChildrenList As New List(Of IntPtr)
            Dim ListHandle As GCHandle = GCHandle.Alloc(ChildrenList)
            EnumChildWindows(iDialog, AddressOf EnumWindow, GCHandle.ToIntPtr(ListHandle))

            AguardaProc(T_TRANSICAO)

            '3 executar botao:
            ' - Window Caption = "&Abrir"
            ' - Class name = "Button"

            iBtnSalvar = FindWindowEx(iDialog, IntPtr.Zero, "Sa&lvar", Nothing)

            SendMessage(iBtnSalvar, BM_CLICK, 0, IntPtr.Zero)

            'SendKeys.SendWait("{ENTER}")

            msgAlert = "OK"

            Return msgAlert
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function CapturarAlertaClicar(ByVal idBtn As String, Optional idElementByname As String = "") As String

        Dim threadResultado As Thread = Nothing
        Dim relogio As Stopwatch

        Try

            bExecutaThread = True

            threadResultado = New Thread(AddressOf CapturarAlert2)
            threadResultado.Name = "Thread criada para clicar no botão do alert: " & idBtn & " - " & idElementByname
            msgAlert = ""
            btnNameClick = idBtn
            threadResultado.Start()
            relogio = New Stopwatch()

            relogio.Start()

            While msgAlert = ""

                AguardaProc(200)
                Thread.Sleep(100)
                If Not String.IsNullOrEmpty(idElementByname) Then
                    If Document.GetElementsByTagName("input").GetElementsByName(idElementByname).Count > 0 Then
                        Exit While
                    End If
                End If

                If relogio.ElapsedMilliseconds > 10000 Then
                    Exit While
                End If

            End While

            relogio.Stop()

            Return msgAlert
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return ""
        Finally
            bExecutaThread = False
        End Try
    End Function

    Public Function CapturarAlertaEfetiva() As String

        Dim threadResultado As Thread = Nothing
        Dim relogio As Stopwatch

        Try

            bExecutaThread = True

            threadResultado = New Thread(AddressOf CapturarAlert2)
            threadResultado.Name = "Thread CapturarAlertaEfetiva"
            msgAlert = ""
            btnNameClick = "OK"
            threadResultado.Start()
            relogio = New Stopwatch()

            relogio.Start()

            While msgAlert = ""

                AguardaProc(200)
                Thread.Sleep(100)

                If relogio.ElapsedMilliseconds > TIMEOUT_PAGINA Then
                    msgAlert = "TIMEOUT"
                    Exit While
                End If

            End While

            relogio.Stop()

            Return msgAlert
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return ""
        Finally
            bExecutaThread = False
        End Try
    End Function

    Private Declare Function BitBlt _
     Lib "gdi32.dll" (
     ByVal hdcDest As IntPtr,
     ByVal x As Int32,
     ByVal y As Int32,
     ByVal Width As Int32,
     ByVal Height As Int32,
     ByVal hdcSrc As IntPtr,
     ByVal xSrc As Int32,
     ByVal ySrc As Int32,
     ByVal dwRop As Int32
     ) As Boolean
    Private Const SRCCOPY As Integer = &HCC0020

    Function CapturaImagemTela() As Bitmap
        Dim ctl As Control = Me
        Dim grpInput As Graphics = ctl.CreateGraphics
        ' Compatibila o Objeto Bitmap com o objecto Grafics
        Dim bmpOutput As New Bitmap(ctl.ClientRectangle.Width, ctl.ClientRectangle.Height, grpInput)
        Dim grpOutput As Graphics = Graphics.FromImage(bmpOutput)
        Dim sourceDC As IntPtr = grpInput.GetHdc
        Dim targetDC As IntPtr = grpOutput.GetHdc
        BringWindowToFront("ModuloGerenciador.vshost")
        BringWindowToFront("ModuloGerenciador")
        AguardaProc(1000)
        ctl.Parent.Focus()
        ctl.Focus()
        ' Copia Somento o Camanho do seu Controle
        BitBlt(targetDC, 0, 0, ctl.ClientRectangle.Width, ctl.ClientRectangle.Height, sourceDC, ctl.ClientRectangle.X, ctl.ClientRectangle.Y, SRCCOPY)
        ' Ajusta e libera da memoria os objetos
        grpInput.ReleaseHdc(sourceDC)
        grpInput.Dispose()
        grpOutput.ReleaseHdc(targetDC)
        grpOutput.Dispose()
        Return bmpOutput
    End Function

    Public Function CapturarAlertaNavegador() As Boolean

        Dim iAlert As IntPtr = IntPtr.Zero
        Dim iBtnOK As IntPtr = IntPtr.Zero
        Dim oRelogio As Stopwatch = Nothing

        Try

            AguardaProc(T_TRANSICAO)

            oRelogio = New Stopwatch
            oRelogio.Start()

            While iAlert = IntPtr.Zero And oRelogio.ElapsedMilliseconds < 2500

                iAlert = FindWindow("#32770", "Navegador da Web") '"Escolher arquivo a carregar")

            End While

            oRelogio.Stop()

            If oRelogio.ElapsedMilliseconds >= 2500 Then
                Return True
            End If

            If iAlert = IntPtr.Zero Then
                'Time_out
                Return False
            End If

            Debug.Print("iAlert " & Hex(CInt(iAlert)))

            AguardaProc(T_TRANSICAO)

            Dim ChildrenList As New List(Of IntPtr)
            Dim ListHandle As GCHandle = GCHandle.Alloc(ChildrenList)
            EnumChildWindows(iAlert, AddressOf JanelaAlert, GCHandle.ToIntPtr(ListHandle))

            iBtnOK = FindWindowEx(iAlert, IntPtr.Zero, "&Repetir", Nothing)
            Debug.Print("iBtnOK " & Hex(CInt(iBtnOK)))

            SendMessage(iBtnOK, BM_CLICK, 0, IntPtr.Zero)

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try
    End Function

    Private Function EnumWindow(ByVal Handle As IntPtr, ByVal Parameter As IntPtr) As Boolean

        Dim strObjeto As StringBuilder = New StringBuilder(256)

        GetClassName(Handle, strObjeto, 255)

        Debug.Print(strObjeto.ToString)

        If strObjeto.ToString() = "Edit" Then
            SendMessage(Handle, CInt(WM_SETTEXT), 0, arqCaminho) 'OK
            Return True
        End If

        Return True

    End Function

    Private Function JanelaAlert(ByVal Handle As IntPtr, ByVal Parameter As IntPtr) As Boolean

        Dim strObjeto As StringBuilder
        msgAlert = ""

        Try
            strObjeto = New StringBuilder(256)

            GetClassName(Handle, strObjeto, 255)

            Debug.Print(strObjeto.ToString)

            If strObjeto.ToString() = "Static" Then
                SendMessage(Handle, Int(WM_GETTEXT), strObjeto.Capacity, strObjeto)
                If strObjeto.Length > 0 Then
                    msgAlert = strObjeto.ToString()
                    Return True
                End If
            End If

            Return True
        Catch ex As Exception
            LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
            Return False
        End Try

    End Function

    Public Sub mensagemBarraStatusNav(ByVal sMensagem As String,
                                      Optional bPulaLinha As Boolean = True)

        If txtInfo Is Nothing Then
            Exit Sub
        End If

        If bPulaLinha = True Then
            txtInfo.AppendText(sMensagem & vbNewLine)
            LogaErro(sMensagem.Trim)
        Else
            txtInfo.AppendText(sMensagem)
        End If

        txtInfo.ScrollToCaret()

        'If txtInfo Is Nothing Then
        '    Exit Sub
        'End If
        '
        'txtInfo.AtualizaStatus(sMensagem.Trim)
        'LogaErro(sMensagem.Trim)

    End Sub

End Class