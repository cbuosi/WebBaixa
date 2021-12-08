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
Option Explicit On
Option Strict On

Imports System.Drawing.Graphics
Imports System.Drawing.Color
Imports System.Drawing.Brush
Imports System.Drawing.Point
Imports System.Media
Imports System.IO
Imports GFT.Util

Public Class frmSobre

    Const strDatabase = ""
    Const GetBuildName = ""

    Const AppVersion = ""
    Const strTituloApp = ""
    Const strInfoBanco = ""
    Const strNomeBanco = ""
    Const sStatusConexaoDB2 = ""

    Dim oSnes As SNES_65816 = Nothing

    Const strUrlSondaSupermercados As String = "http://www.sondait.com.br"

    Protected m_vertices(8) As Point3D
    Protected m_faces(6, 4) As Integer
    Protected m_colors(6) As Color
    Protected m_brushes(6) As Brush
    Protected m_angleX As Integer
    Protected m_angleY As Integer

    Private iDeslocX As Double = 0
    Private iDeslocY As Double = 0
    Private iDeslocZ As Double = 0
    Private bEasterEgg As Boolean = True
    Private sinalX As Double = 1
    Private sinalY As Double = 1
    Private sinalZ As Double = 1

    '------------------------------------------------------------------------
    ' New()
    '------------------------------------------------------------------------
    Public Sub New()
        MyBase.New()
        Try
            Me.DoubleBuffered = True
            'Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            'Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            InitializeComponent()
        Catch ex As Exception
        End Try
    End Sub

    '------------------------------------------------------------------------
    ' frm_Sobre_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    '------------------------------------------------------------------------
    Private Sub frm_Sobre_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        iDeslocX = 0
        iDeslocY = 0
        iDeslocZ = 0

        Me.Opacity = 100

        Try

            Me.Show()

            InitCube()
            timerCubo.Enabled = True


            'oSnes = New SNES_65816
            'PicScreen.Visible = True
            'oSnes.Hi_Res_Timer_Initialize()
            'oSnes.Load_Rom("")
            'oSnes.Reset_65816()
            'oSnes.Reset_PPU()
            'oSnes.Reset_IO()
            'oSnes.SNES_On = True
            'PicScreen.Focus()
            'oSnes.Main_Loop()



            btnVoltar.Enabled = True



        Catch ex As Exception
            '
        End Try

    End Sub

    '------------------------------------------------------------------------
    ' PictureBox1_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles PictureBox1.DoubleClick
    '------------------------------------------------------------------------
    'Private Sub PictureBox1_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles PictureBox1.DoubleClick
    '    ALERT(strDeveloper, eBotoes.Ok, , , eImagens.Atencao)
    'End Sub

    '------------------------------------------------------------------------
    ' btnVoltar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVoltar.Click
    '------------------------------------------------------------------------
    Private Sub btnVoltar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVoltar.Click

        If Not oSnes Is Nothing Then
            oSnes.SNES_On = False
            oSnes = Nothing
        End If

        btnVoltar.Enabled = False
        'esmaecerFormulario(Me)
        Me.Close()

    End Sub

    '------------------------------------------------------------------------
    ' Pinta(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
    '------------------------------------------------------------------------
    Private Sub Pinta(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint

        If (bEasterEgg) Then

            Dim t(8) As Point3D
            Dim v As Point3D
            Dim avgZ(6) As Double
            Dim order(6) As Integer
            Dim tmp As Double
            Dim iMax As Integer

            ' Transform all the points and store them on the "t" array.
            For i = 0 To 7
                v = m_vertices(i)
                t(i) = v.RotateX(m_angleX).RotateY(m_angleY).RotateZ(Me.m_angleX)
                t(i) = CType(t(i).Project(iDeslocX, iDeslocY, 256, 8 + iDeslocZ), Point3D)
            Next

            ' Compute the average Z value of each face.
            For i = 0 To 5
                avgZ(i) = (t(m_faces(i, 0)).Z + t(m_faces(i, 1)).Z + t(m_faces(i, 2)).Z + t(m_faces(i, 3)).Z) / 4.0
                order(i) = i
            Next

            ' Next we sort the faces in descending order based on the Z value.
            ' The objective is to draw distant faces first. This is called 
            ' the PAINTERS ALGORITHM. So, the visible faces will hide the invisible ones.
            ' The sorting algorithm used is the SELECTION SORT.
            For i = 0 To 4
                iMax = i
                For j = i + 1 To 5
                    If avgZ(j) > avgZ(iMax) Then
                        iMax = j
                    End If
                Next
                If iMax <> i Then
                    tmp = avgZ(i)
                    avgZ(i) = avgZ(iMax)
                    avgZ(iMax) = tmp

                    tmp = order(i)
                    order(i) = order(iMax)
                    order(iMax) = CInt(tmp)
                End If
            Next

            ' Draw the faces using the PAINTERS ALGORITHM (distant faces first, closer faces last).
            For i = 0 To 5
                Dim points() As Point
                Dim index As Integer = order(i)
                points = New Point() {
                    New Point(CInt(t(m_faces(index, 0)).X), CInt(t(m_faces(index, 0)).Y)),
                    New Point(CInt(t(m_faces(index, 1)).X), CInt(t(m_faces(index, 1)).Y)),
                    New Point(CInt(t(m_faces(index, 2)).X), CInt(t(m_faces(index, 2)).Y)),
                    New Point(CInt(t(m_faces(index, 3)).X), CInt(t(m_faces(index, 3)).Y))
                }
                e.Graphics.FillPolygon(m_brushes(index), points)
            Next
        End If

    End Sub

    '------------------------------------------------------------------------
    ' AnimationLoop()
    '------------------------------------------------------------------------
    Private Sub AnimationLoop()
        ' Forces the Paint event to be called.
        Me.Invalidate()
        ' Update the variable after each frame.
        ' Update the variable after each frame.
        m_angleX += 2
        m_angleY += 1
    End Sub

    '------------------------------------------------------------------------
    ' InitCube()
    '------------------------------------------------------------------------
    Private Sub InitCube()
        ' Create the cube vertices.
        m_vertices = New Point3D() {
                     New Point3D(-1.0, 1.0, -1.0),
                     New Point3D(1.0, 1.0, -1.0),
                     New Point3D(1.0, -1.0, -1.0),
                     New Point3D(-1.0, -1.0, -1.0),
                     New Point3D(-1.0, 1.0, 1.0),
                     New Point3D(1.0, 1.0, 1.0),
                     New Point3D(1.0, -1.0, 1.0),
                     New Point3D(-1.0, -1.0, 1.0)}

        ' Create an array representing the 6 faces of a cube. Each face is composed by indices to the vertex array
        ' above.
        m_faces = New Integer(,) {{0, 1, 2, 3},
                                  {1, 5, 6, 2},
                                  {5, 4, 7, 6},
                                  {4, 0, 3, 7},
                                  {0, 4, 5, 1},
                                  {3, 2, 6, 7}}


        ' Define the colors of each face.
        m_colors = New Color() {Color.DarkCyan,
                                Color.Blue,
                                Color.Green,
                                Color.Magenta,
                                Color.Yellow,
                                Color.Red}


        ' Create the brushes to draw each face. Brushes are used to draw filled polygons.
        For i = 0 To 5
            m_brushes(i) = New SolidBrush(m_colors(i))
        Next

    End Sub

    '------------------------------------------------------------------------
    ' timerCubo_Tick(sender As System.Object, e As System.EventArgs) Handles tReloginhoCB.Tick
    '------------------------------------------------------------------------
    Private Sub timerCubo_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles timerCubo.Tick

        AnimationLoop()

        iDeslocX += (2 * sinalX)
        iDeslocY += (2.8 * sinalY)
        iDeslocZ += (0.1 * sinalZ)

        If iDeslocZ > 30 And sinalZ = 1 Then
            sinalZ = -1
        End If

        If iDeslocZ < 0 And sinalZ = -1 Then
            sinalZ = 1
        End If

        If iDeslocX > (Me.Width * 2) - 20 And sinalX = 1 Then
            sinalX = -1
        End If

        If iDeslocX < 0 And sinalX = -1 Then
            sinalX = 1
        End If

        If iDeslocY > (Me.Height * 2) - 70 And sinalY = 1 Then
            sinalY = -1
        End If

        If iDeslocY < 0 And sinalY = -1 Then
            sinalY = 1
        End If

    End Sub

    '------------------------------------------------------------------------
    ' PictureBox2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox2.Click
    '------------------------------------------------------------------------
    'Private Sub PictureBox2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    '
    '    'If usuarioLogado.PerfilUsuario = enumPerfilUsuario.Consulta Then
    '    '    Exit Sub
    '    'End If
    '
    '    bEasterEgg = Not bEasterEgg
    '
    '    'If bEasterEgg = True Then
    '    SLSomente.Visible = bEasterEgg
    '    timerCubo.Enabled = bEasterEgg
    '    'End If
    '
    'End Sub
    '
    'Private Sub imgSonda_Click(sender As Object, e As EventArgs)
    '    Try
    '        Process.Start(strUrlSondaSupermercados)
    '    Catch ex As Exception
    '        'LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
    '    End Try
    'End Sub
    '
    'Private Sub imgSonda_MouseMove(sender As Object, e As MouseEventArgs)
    '    Me.Cursor = Cursors.Hand
    'End Sub
    '
    'Private Sub imgSonda_MouseLeave(sender As Object, e As EventArgs)
    '    Me.Cursor = Cursors.Default
    'End Sub
    '
    'Private Sub cmdTesteDB2_Click(sender As Object, e As EventArgs)
    '    'Frm_TesteDB2.Opacity = 100
    '    'Frm_TesteDB2.ShowDialog(Me)
    'End Sub
    '
    'Private Sub frmSobre_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
    '
    '    Try
    '
    '
    '        If e.Control And e.KeyCode = Keys.M Then
    '
    '
    '        End If
    '
    '    Catch ex As Exception
    '        'LogaErro("Erro em " & NomeMetodo(Me) & ": " & ex.Message)
    '    End Try
    '
    'End Sub

    'Private Sub info_Click(sender As Object, e As EventArgs)
    '
    'End Sub
End Class