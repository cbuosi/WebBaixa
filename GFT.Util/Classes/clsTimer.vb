Option Explicit On
Option Strict On

Public Class clsTimer

    Public strNomeTimer As String = "TIMER_1"
    Public oRelogio As Stopwatch = Nothing
    Private Tempo As Long = 0

    '--------------------------------------------------------------------------------------------------------
    ' New
    '--------------------------------------------------------------------------------------------------------
    Public Sub New(ByVal _strNomeTimer As String,
                   Optional AutoInicia As Boolean = true)
        strNomeTimer = _strNomeTimer
        oRelogio = New Stopwatch
        If AutoInicia = True Then
            IniciaTimer()
        End If
    End Sub

    '--------------------------------------------------------------------------------------------------------
    ' IniciaTimer()
    '--------------------------------------------------------------------------------------------------------
    Public Sub IniciaTimer()
        'LogaErro("TIMER:: " & strNomeTimer & " Iniciado.")
        oRelogio.Start()
    End Sub

    '--------------------------------------------------------------------------------------------------------
    ' ResetarTimer()
    '--------------------------------------------------------------------------------------------------------
    Public Sub ResetarTimer()
        oRelogio.Reset()
    End Sub

    '--------------------------------------------------------------------------------------------------------
    ' InfoTimer()
    '--------------------------------------------------------------------------------------------------------
    Public Sub InfoTimer()
        Tempo = oRelogio.ElapsedMilliseconds
        'LogaErro(strNomeTimer & " [" & (oRelogio.ElapsedMilliseconds / 1000).ToString & "] segs.")
    End Sub

    '--------------------------------------------------------------------------------------------------------
    ' PararTimer()
    '--------------------------------------------------------------------------------------------------------
    Public Sub PararTimer()
        oRelogio.Stop()
        InfoTimer()
        oRelogio.Reset()
    End Sub

    Public Function ObterTempo() As String
        Return (Tempo / 1000).ToString
    End Function


End Class
