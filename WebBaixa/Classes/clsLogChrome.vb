Option Explicit On
Option Strict On

Namespace LogChrome


    <Serializable> Public Class Creator

        Dim m_name As String
        Dim m_version As String

        Public Property name As String
            Get
                Return m_name
            End Get
            Set(value As String)
                m_name = value
            End Set
        End Property

        Public Property version As String
            Get
                Return m_version
            End Get
            Set(value As String)
                m_version = value
            End Set
        End Property

    End Class

    <Serializable> Public Class PageTimings
        Public onContentLoad As Double
        Public onLoad As Double
    End Class

    <Serializable> Public Class Page
        Public startedDateTime As DateTime
        Public id As String
        Public title As String
        Public pageTimings As PageTimings
    End Class

    <Serializable> Public Class CallFrame
        Public functionName As String
        Public scriptId As String
        Public url As String
        Public lineNumber As Integer
        Public columnNumber As Integer
    End Class

    <Serializable> Public Class Parent
        Public description As String
        Public callFrames As List(Of CallFrame)
        Public parent As Parent
        Public parentId As ParentId
    End Class

    <Serializable> Public Class ParentId
        Public id As String
        Public debuggerId As String
    End Class

    <Serializable> Public Class Stack
        Public callFrames As List(Of CallFrame)
        Public parent As Parent
    End Class

    <Serializable> Public Class Initiator
        Public type As String
        Public stack As Stack
        Public url As String
        Public lineNumber As Integer?
    End Class

    <Serializable> Public Class WebSocketMessage
        Public type As String
        Public time As Double
        Public opcode As Integer
        Public data As String
    End Class

    <Serializable> Public Class Cache
    End Class

    <Serializable> Public Class Header
        Public name As String
        Public value As String
    End Class

    <Serializable> Public Class QueryString
        Public name As String
        Public value As String
    End Class

    <Serializable> Public Class Cooky
        Public name As String
        Public value As String
        Public expires As Object
        Public httpOnly As Boolean
        Public secure As Boolean
    End Class

    <Serializable> Public Class PostData
        Public mimeType As String
        Public text As String
    End Class

    <Serializable> Public Class Request
        Public method As String
        Public url As String
        Public httpVersion As String
        Public headers As List(Of Header)
        Public queryString As List(Of QueryString)
        Public cookies As List(Of Cooky)
        Public headersSize As Integer
        Public bodySize As Integer
        Public postData As PostData
    End Class

    <Serializable> Public Class Content
        Public size As Integer
        Public mimeType As String
        Public text As String
        Public encoding As String
        Public compression As Integer?
    End Class

    <Serializable> Public Class Response
        Public status As Integer
        Public statusText As String
        Public httpVersion As String
        Public headers As List(Of Header)
        Public cookies As List(Of Object)
        Public content As Content
        Public redirectURL As String
        Public headersSize As Integer
        Public bodySize As Integer
        Public _transferSize As Integer
        Public _error As Object
    End Class

    <Serializable> Public Class Timings
        Public blocked As Double
        Public dns As Double
        Public ssl As Double
        Public connect As Double
        Public send As Double
        Public wait As Double
        Public receive As Double
        Public _blocked_queueing As Double
    End Class

    <Serializable> Public Class Entry
        Public _initiator As Initiator
        Public _priority As String
        Public _resourceType As String
        Public _webSocketMessages As List(Of WebSocketMessage)
        Public cache As Cache
        Public pageref As String
        Public request As Request
        Public response As Response
        Public serverIPAddress As String
        Public startedDateTime As DateTime
        Public time As Double
        Public timings As Timings
        Public connection As String
        Public _fromCache As String
    End Class

    <Serializable> Public Class Log

        Dim m_version As String
        Dim m_creator As Creator
        Dim m_pages As List(Of Page)
        Dim m_entries As List(Of Entry)


        Public Property version As String
            Get
                Return m_version
            End Get
            Set(value As String)
                m_version = value
            End Set
        End Property

        Public Property creator As Creator
            Get
                Return m_creator
            End Get
            Set(value As Creator)
                m_creator = value
            End Set
        End Property
        Public Property pages As List(Of Page)
            Get
                Return m_pages
            End Get
            Set(value As List(Of Page))
                m_pages = value
            End Set
        End Property
        Public Property entries As List(Of Entry)
            Get
                Return m_entries
            End Get
            Set(value As List(Of Entry))
                m_entries = value
            End Set
        End Property

    End Class

    <Serializable> Public Class clsLogChrome
        Public log As Log
    End Class

End Namespace

'End Class
