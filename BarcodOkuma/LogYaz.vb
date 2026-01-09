Imports System
Imports System.IO
Imports System.IO.File
Imports System.Windows.Forms


Module LogYaz

    Public DosyaPath As String = Application.StartupPath & "\LOG\"
    
    ' Constants for file management
    Private Const MAX_LOG_FILE_SIZE As Long = 52428800 ' 50MB
    
    Public Sub LogTut(ByVal Bilgi As String, ByVal DosyaYeri As String, Optional ByVal DosyaAdi As String = "")
        On Error GoTo hata

        Dim stream_writer As StreamWriter
        'Dim Tarih As String = Format(Now, "YYYYMMdd")
        stream_writer = AppendText(DosyaYeri & DosyaAdi)
        stream_writer.WriteLine(Now & " " & Bilgi)
        stream_writer.Close()
        Exit Sub
hata:

    End Sub


    Public Sub LogTutGeneric(ByVal Bilgi As String, ByVal DosyaYeri As String, Optional ByVal DosyaAdi As String = "")
        On Error GoTo hata

        Dim stream_writer As StreamWriter
        'Dim Tarih As String = Format(Now, "YYYYMMdd")
        stream_writer = CreateText(DosyaYeri & DosyaAdi)
        stream_writer.WriteLine(Bilgi)
        stream_writer.Close()
        Exit Sub
hata:

    End Sub

    ' Async version for performance-critical barcode logging to Simatic system
    Public Sub LogTutGenericAsync(ByVal Bilgi As String, ByVal DosyaYeri As String, ByVal DosyaAdi As String)
        Try
            ' Queue file write operation to ThreadPool for non-blocking I/O
            System.Threading.ThreadPool.QueueUserWorkItem(Sub(state)
                                                              Try
                                                                  Dim fullPath As String = DosyaYeri & DosyaAdi
                                                                  ' Use File.WriteAllText for atomic write operation
                                                                  System.IO.File.WriteAllText(fullPath, Bilgi & vbCrLf)
                                                              Catch ex As Exception
                                                                  ' Silent fail to avoid blocking production
                                                              End Try
                                                          End Sub)
        Catch ex As Exception
            ' Silent fail to avoid blocking production
        End Try
    End Sub

    ' Debug logging method for AllLogs.txt - tracks all operations with timestamps
    Public Sub LogDebug(ByVal Hat As String, ByVal Barkod As String, ByVal Mesaj As String)
        Try
            ' Queue file write operation to ThreadPool for non-blocking I/O
            System.Threading.ThreadPool.QueueUserWorkItem(Sub(state)
                                                              Try
                                                                  Dim fullPath As String = DosyaPath & "AllLogs.txt"
                                                                  Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                                                  Dim logEntry As String = $"[{timestamp}] Hat-{Hat} | Barkod: {Barkod} | {Mesaj}"
                                                                  
                                                                  ' Append to file (thread-safe)
                                                                  SyncLock GetType(LogYaz)
                                                                      System.IO.File.AppendAllText(fullPath, logEntry & vbCrLf)
                                                                  End SyncLock
                                                              Catch ex As Exception
                                                                  ' Silent fail for file I/O exceptions to avoid blocking production operations
                                                              End Try
                                                          End Sub)
        Catch ex As Exception
            ' Silent fail to avoid blocking production
        End Try
    End Sub

    ' Comprehensive logging method for AllLog.txt - logs all operations with millisecond precision
    ' Supports all event types: BARCODE_RECEIVED, SAP_CALL_START, SAP_RESPONSE, SAP_ERROR, 
    ' SQL_INSERT_START, SQL_INSERT_SUCCESS, SQL_INSERT_ERROR, LOG_FILE_WRITE, LOG_FILE_ERROR,
    ' PROCESS_COMPLETE, ERROR, CONNECTION_STATUS
    Public Sub LogAllOperations(ByVal Hat As String, ByVal EventType As String, ByVal Details As String)
        Try
            ' Queue file write operation to ThreadPool for non-blocking I/O
            System.Threading.ThreadPool.QueueUserWorkItem(Sub(state)
                                                              Try
                                                                  Dim fullPath As String = DosyaPath & "AllLog.txt"
                                                                  ' Millisecond precision timestamp
                                                                  Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                                                                  Dim logEntry As String = $"[{timestamp}] HAT-{Hat} | {EventType} | {Details}"
                                                                  
                                                                  ' Append to file (thread-safe)
                                                                  SyncLock GetType(LogYaz)
                                                                      System.IO.File.AppendAllText(fullPath, logEntry & vbCrLf)
                                                                      
                                                                      ' File size management - rotate if > 50MB
                                                                      Dim fileInfo As New System.IO.FileInfo(fullPath)
                                                                      If fileInfo.Exists AndAlso fileInfo.Length > MAX_LOG_FILE_SIZE Then
                                                                          Dim backupPath As String = DosyaPath & "AllLog_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".txt"
                                                                          Try
                                                                              System.IO.File.Move(fullPath, backupPath)
                                                                          Catch
                                                                              ' If move fails, just truncate the file
                                                                              System.IO.File.WriteAllText(fullPath, "=== Log rotated due to size limit ===" & vbCrLf)
                                                                          End Try
                                                                      End If
                                                                  End SyncLock
                                                              Catch ex As Exception
                                                                  ' Silent fail for file I/O exceptions to avoid blocking production operations
                                                              End Try
                                                          End Sub)
        Catch ex As Exception
            ' Silent fail to avoid blocking production
        End Try
    End Sub

    Function Klasor_Kontrol() As Boolean

        On Error GoTo hata

        Klasor_Kontrol = False
        DosyaPath = Application.StartupPath & "\LOG\" ' & Format(Now, "yyyyMMdd") & ".LOG"

        If Not (IO.Directory.Exists(Application.StartupPath & "\LOG")) Then
            System.IO.Directory.CreateDirectory(Application.StartupPath & "\LOG")
            LogTut("Klasör Yaratıldı : ", DosyaPath, Format(Now, "yyyyMMdd") & ".LOG")
        End If
        Klasor_Kontrol = True
        Exit Function

hata:

    End Function
End Module
