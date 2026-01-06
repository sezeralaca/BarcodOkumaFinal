Imports System
Imports System.Data
'Imports Oracle.DataAccess
Imports Microsoft.VisualBasic
'Imports Oracle.DataAccess.Client
Imports System.Data.SqlClient



Public Class Database

    Enum enumDbType
        Oracle = 0
        Sql = 1
    End Enum
    Enum enumObjectType
        StrType = 0
        IntType = 1
        DblType = 2
        DateType = 3
        OtherType = 4
    End Enum
    Public Shared ConStr As String = "Server=TR-BILGISAYAR\WINCCFLEXEXPRESS;Database=SIMFER;User Id=SIMFER;Password=SIMFER"
    Public Event OnError(ByRef ErrorMessage As String)

    Public Shared Function CheckDBNull(ByVal obj As Object, Optional ByVal ObjectType As enumObjectType = enumObjectType.StrType) As Object
        Dim objReturn As Object
        objReturn = obj
        If (IsDBNull(obj)) Then
            Select Case ObjectType
                Case enumObjectType.DateType
                    objReturn = ""
                Case enumObjectType.DblType
                    objReturn = 0.0
                Case enumObjectType.IntType
                    objReturn = 0
                Case enumObjectType.StrType
                    objReturn = ""
                Case Else
                    objReturn = ""
            End Select
        End If


        If IsDBNull(objReturn) Then
            objReturn = ""
        ElseIf Trim(objReturn) = "0" Then
            objReturn = ""
        ElseIf Trim(objReturn) = "NA" Then
            objReturn = ""
        Else
            objReturn = objReturn
        End If



        Return objReturn
    End Function

    Public Shared Function Connect_DB_Select(ByVal strsql As String, ByVal DB_TYPE As enumDbType) As DataSet

        Connect_DB_Select = New DataSet
        Select Case DB_TYPE
            Case enumDbType.Oracle
                ' Connect_DB_Select = Connect_Oracle_Select(strsql)
            Case enumDbType.Sql
                Connect_DB_Select = Connect_Sql_Select(strsql)
        End Select

    End Function

   

    Public Shared Function Connect_Sql_Select(ByVal strsql As String) As DataSet

        Connect_Sql_Select = New DataSet
        Try
            Dim SqlCon As New SqlConnection(ConStr)
            Try
                Dim SqlDataAdap As New SqlDataAdapter(strsql, SqlCon)
                Try
                    SqlDataAdap.Fill(Connect_Sql_Select, "Tablo")
                    SqlDataAdap = Nothing
                    SqlCon.Close()
                Catch ex As Exception
                    SqlDataAdap = Nothing
                    SqlCon.Close()
                End Try

            Catch ex As Exception
                SqlCon.Close()
            End Try
        Catch ex As Exception
            Exit Function
        End Try



    End Function


    Public Shared Function Connect_DB_Execute(ByVal strsql As String, ByVal DB_TYPE As enumDbType) As Integer

        Connect_DB_Execute = 0
        Select Case DB_TYPE
            Case enumDbType.Oracle
                '  Connect_DB_Execute = Connect_Hirata_Execute(strsql)
            Case enumDbType.Sql
                Connect_DB_Execute = Connect_Sql_Execute(strsql)
        End Select

    End Function




    Shared Function Connect_Sql_Execute(ByVal strsql As String) As Integer

        Connect_Sql_Execute = 0

        Try
            Dim SqlCon As New SqlConnection(ConStr)
            Try
                Dim SqlCommand As New SqlClient.SqlCommand()
                Try
                    SqlCon.Open()

                    SqlCommand.Connection = SqlCon
                    SqlCommand.CommandType = Data.CommandType.Text
                    SqlCommand.CommandText = strsql

                    Connect_Sql_Execute = SqlCommand.ExecuteNonQuery

                    SqlCommand = Nothing
                    SqlCon.Close()
                Catch ex As Exception
                    SqlCommand = Nothing
                    SqlCon.Close()
                End Try

            Catch ex As Exception
                SqlCon.Close()
            End Try
        Catch ex As Exception
            Exit Function
        End Try



    End Function





End Class