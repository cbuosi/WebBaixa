#Region "Legal"
'************************************************************************************************************************
' Copyright (c) 2015, Todos direitos reservados, GFT-IT - Serviços de TI - http://www.GFTit.com.br/
'
' Autor........: Carlos Buosi (cbuosi@gmail.com)
' Arquivo......: SuperLetreiro.vb
' Tipo.........: Modulo VB.
' Versao.......: 2.14+
' Propósito....: Componente Letreiro
' Uso..........: Não se aplica
' Produto......: 
'
' Legal........: Este código é de propriedade do Banco Bradesco S/A e/ou GFT-IT - Serviços de TI, sua cópia
'                e/ou distribuição é proibida.
'
' GUID.........: {4D01DCC3-6DE1-42C8-ABBD-C43EED68539B}
' Observações..: nenhuma.
'
'************************************************************************************************************************
#End Region

Option Explicit On
Option Strict On

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.ComponentModel
Imports System.Windows.Forms

Public Class SuperLetreiro
    Inherits System.Windows.Forms.UserControl

    Friend WithEvents Timer As System.Windows.Forms.Timer

    Private posicaoInicial As Integer = 0
    Private m_TextoLetreiro As String = ""
    Private m_EsquerdaParaDireita As Direcao = Direcao.Direita
    Private m_VelocidadeRoagem As Integer = 5
    Private m_CorSombra As Color = Color.White
    'Private hDC As IntPtr

    Public Enum Direcao
        Esquerda
        Direita
    End Enum

    Public Sub New()
        MyBase.New()
        InitializeComponent()
        AddHandler MyBase.Paint, AddressOf OnPaint
    End Sub

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.Timer = New System.Windows.Forms.Timer(Me.components)
        Me.Timer.Enabled = True
        Me.Name = "SuperLetreiro"
        Me.Size = New System.Drawing.Size(120, 13)
    End Sub

    <Category("SuperLetreiro")> _
    <Description("Texto do Letreiro.")> _
    Public Property TextoLetreiro() As String
        Get
            TextoLetreiro = m_TextoLetreiro
        End Get
        Set(ByVal Value As String)
            m_TextoLetreiro = Value
            Invalidate()
        End Set
    End Property

    <Category("SuperLetreiro")> _
    <Description("Direção de rolagem do controle")> _
    Public Property RolagemLetreiro() As Direcao
        Get
            Return Me.m_EsquerdaParaDireita
        End Get
        Set(ByVal Value As Direcao)
            m_EsquerdaParaDireita = Value
            Invalidate()
        End Set
    End Property

    <Category("SuperLetreiro")> _
    <Description("Velocidade de rolagem do controle. Valores entre 1 e 10.")> _
    Public Property VelocidadeRolagem() As Integer
        Get
            VelocidadeRolagem = m_VelocidadeRoagem
        End Get
        Set(ByVal Value As Integer)

            If Value < 1 Then
                m_VelocidadeRoagem = 1
            ElseIf Value > 30 Then
                m_VelocidadeRoagem = 30
            Else
                m_VelocidadeRoagem = Value
            End If

            Me.Timer.Interval = Value * 10
            Invalidate()
        End Set
    End Property

    <Category("SuperLetreiro")> _
    <Description("Cor da sombra do texto.")> _
    Public Property CorSombraTexto() As Color
        Get
            CorSombraTexto = m_CorSombra
        End Get
        Set(ByVal Value As Color)
            m_CorSombra = Value
            Invalidate()
        End Set
    End Property

    Private Sub Letreiro_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.DoubleBuffer Or ControlStyles.ResizeRedraw Or ControlStyles.UserPaint, True)
    End Sub

    Private Sub Timer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer.Tick
        If m_TextoLetreiro.Length = 0 Then Exit Sub
        Invalidate()
    End Sub

    Private Sub Letreiro_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        Invalidate()
    End Sub

    Private Sub Letreiro_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.SizeChanged
        Invalidate()
    End Sub

    Private Overloads Sub OnPaint(ByVal sender As Object, _
                                  ByVal e As PaintEventArgs)

        'Dim str As String = m_TextoLetreiro
        Dim g As Graphics = e.Graphics
        Dim szf As SizeF

        g.SmoothingMode = SmoothingMode.HighQuality
        szf = g.MeasureString(m_TextoLetreiro, Me.Font)

        If m_EsquerdaParaDireita = Direcao.Direita Then
            If posicaoInicial > Me.Width Then
                posicaoInicial = CInt(-szf.Width)
            Else
                posicaoInicial += 1
            End If
        ElseIf m_EsquerdaParaDireita = Direcao.Esquerda Then
            If posicaoInicial < -szf.Width Then
                posicaoInicial = Me.Width
            Else
                posicaoInicial -= 1
            End If
        End If
        g.DrawString(m_TextoLetreiro, Me.Font, New SolidBrush(Me.ForeColor), posicaoInicial, CSng(0 + (Me.Height / 2) - (szf.Height / 2)))
    End Sub

    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
