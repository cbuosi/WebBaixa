Option Explicit On
Option Strict On

Public Class Point3D
    Protected m_x As Double, m_y As Double, m_z As Double

    Public Sub New(ByVal x As Double, ByVal y As Double, ByVal z As Double)
        Me.X = x
        Me.Y = y
        Me.Z = z
    End Sub

    Public Property X() As Double
        Get
            Return m_x
        End Get
        Set(ByVal value As Double)
            m_x = value
        End Set
    End Property

    Public Property Y() As Double
        Get
            Return m_y
        End Get
        Set(ByVal value As Double)
            m_y = value
        End Set
    End Property

    Public Property Z() As Double
        Get
            Return m_z
        End Get
        Set(ByVal value As Double)
            m_z = value
        End Set
    End Property

    Public Function RotateX(ByVal angle As Integer) As Point3D
        Dim rad As Double, cosa As Double, sina As Double, yn As Double, zn As Double

        rad = angle * Math.PI / 180
        cosa = Math.Cos(rad)
        sina = Math.Sin(rad)
        yn = Me.Y * cosa - Me.Z * sina
        zn = Me.Y * sina + Me.Z * cosa
        Return New Point3D(Me.X, yn, zn)
    End Function

    Public Function RotateY(ByVal angle As Integer) As Point3D
        Dim rad As Double, cosa As Double, sina As Double, Xn As Double, Zn As Double

        rad = angle * Math.PI / 180
        cosa = Math.Cos(rad)
        sina = Math.Sin(rad)
        Zn = Me.Z * cosa - Me.X * sina
        Xn = Me.Z * sina + Me.X * cosa

        Return New Point3D(Xn, Me.Y, Zn)
    End Function

    Public Function RotateZ(ByVal angle As Integer) As Point3D
        Dim rad As Double, cosa As Double, sina As Double, Xn As Double, Yn As Double

        rad = angle * Math.PI / 180
        cosa = Math.Cos(rad)
        sina = Math.Sin(rad)
        Xn = Me.X * cosa - Me.Y * sina
        Yn = Me.X * sina + Me.Y * cosa
        Return New Point3D(Xn, Yn, Me.Z)
    End Function

    Public Function Project(ByVal viewWidth As Double,
                            ByVal viewHeight As Double,
                            ByVal fov As Double,
                            ByVal viewDistance As Double) As Object

        Dim factor As Double, Xn As Double, Yn As Double
        factor = fov / (viewDistance + Me.Z)
        Xn = Me.X * factor + viewWidth / 2
        Yn = Me.Y * factor + viewHeight / 2
        Return New Point3D(Xn, Yn, Me.Z)
    End Function
End Class