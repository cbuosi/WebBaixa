Option Explicit On
Option Strict On

Public Class SuperDataTable
    Inherits DataTable

    Public NomeClasse As String
    Dim oDataColumn As DataColumn
    Dim oRow As DataRow

    Public Sub New(ByVal NomeTabela As String)
        NomeClasse = NomeTabela
        Me.TableName = NomeTabela
    End Sub

    Public Sub New()
        NomeClasse = "SuperDataTable"
        Me.TableName = NomeClasse
    End Sub


    Public Sub AdicionaColuna(ByVal NomeColuna As String,
                              ByVal TipoColuna As Type,
                              Optional ByVal bAutoIncrementa As Boolean = False)

        oDataColumn = New DataColumn()
        oDataColumn.DataType = TipoColuna
        oDataColumn.ColumnName = NomeColuna
        oDataColumn.AutoIncrement = bAutoIncrementa
        Me.Columns.Add(oDataColumn)

    End Sub

    Sub AdicionaLinha(ByVal Linha As DataRow)
        Me.Rows.Add(Linha)
    End Sub

    Sub AdicionaValorColuna(ByVal NomeColuna As String, ByVal valorColuna As Object)
        'Debug.Print(valorColuna.ToString())
        oRow(NomeColuna) = valorColuna
    End Sub

    Sub NovaLinha()
        oRow = Me.NewRow()
    End Sub

    Sub EfetivaLinha()
        Me.Rows.Add(oRow)
    End Sub

    Protected Overrides Sub Finalize()

        If Not oDataColumn Is Nothing Then
            oDataColumn.Dispose()
        End If

        oDataColumn = Nothing
        oRow = Nothing
        NomeClasse = Nothing

        MyBase.Finalize()

        Lixeiro()

    End Sub

    Public Sub Lixeiro()
        GC.SuppressFinalize(Me)
        GC.Collect()
    End Sub

End Class
