Imports System
Imports System.IO
Imports System.IO.File
Imports System.Windows.Forms


Module LogYaz

    Public DosyaPath As String = Application.StartupPath & "\LOG\"

    ' Log dosya yönetimi sabitleri
    Private Const MAX_LOG_FILE_SIZE As Long = 10485760  ' 10MB - dosya boyut limiti
    Private Const LOG_RETENTION_MINUTES As Integer = 10  ' Kaç dakika öncesine kadar log tutulsun
    Private lastCleanupTime As DateTime = DateTime.MinValue  ' Son temizlik zamanı

    ' Eski log dosyalarını temizle (LOG_RETENTION_MINUTES dakikadan eski)
    Private Sub CleanupOldLogs()
        Try
            ' Dakikada en fazla 1 kez temizlik yap
            If DateTime.Now.Subtract(lastCleanupTime).TotalSeconds < 60 Then Return
            lastCleanupTime = DateTime.Now

            Dim logDir As New DirectoryInfo(DosyaPath)
            If Not logDir.Exists Then Return

            Dim cutoffTime As DateTime = DateTime.Now.AddMinutes(-LOG_RETENTION_MINUTES)
            For Each fi As FileInfo In logDir.GetFiles("*.txt")
                If fi.LastWriteTime < cutoffTime Then
                    Try
                        fi.Delete()
                    Catch
                        ' Silinemeyen dosya varsa atla
                    End Try
                End If
            Next
        Catch
            ' Temizlik hatası üretimi engellemesin
        End Try
    End Sub

    ' Günlük log dosya adını döndür (AllLog_20260325.txt gibi)
    Private Function GetDailyLogPath(baseName As String) As String
        Return DosyaPath & baseName & "_" & DateTime.Now.ToString("yyyyMMdd") & ".txt"
    End Function

    ' Dosya boyut kontrolü - limit aşılırsa yeni dosya aç
    Private Sub CheckAndRotate(filePath As String)
        Try
            Dim fi As New FileInfo(filePath)
            If fi.Exists AndAlso fi.Length > MAX_LOG_FILE_SIZE Then
                Dim backupPath As String = filePath.Replace(".txt", "_" & DateTime.Now.ToString("HHmmss") & ".txt")
                Try
                    System.IO.File.Move(filePath, backupPath)
                Catch
                    System.IO.File.WriteAllText(filePath, "=== Log rotated ===" & vbCrLf)
                End Try
            End If
        Catch
        End Try
    End Sub

    Public Sub LogTut(ByVal Bilgi As String, ByVal DosyaYeri As String, Optional ByVal DosyaAdi As String = "")
        On Error GoTo hata

        Dim stream_writer As StreamWriter
        stream_writer = AppendText(DosyaYeri & DosyaAdi)
        stream_writer.WriteLine(Now & " " & Bilgi)
        stream_writer.Close()
        Exit Sub
hata:

    End Sub


    Public Sub LogTutGeneric(ByVal Bilgi As String, ByVal DosyaYeri As String, Optional ByVal DosyaAdi As String = "")
        On Error GoTo hata

        Dim stream_writer As StreamWriter
        stream_writer = CreateText(DosyaYeri & DosyaAdi)
        stream_writer.WriteLine(Bilgi)
        stream_writer.Close()
        Exit Sub
hata:

    End Sub

    Public Sub LogTutGenericAsync(ByVal Bilgi As String, ByVal DosyaYeri As String, ByVal DosyaAdi As String)
        Try
            System.Threading.ThreadPool.QueueUserWorkItem(Sub(state)
                                                              Try
                                                                  Dim fullPath As String = DosyaYeri & DosyaAdi
                                                                  System.IO.File.WriteAllText(fullPath, Bilgi & vbCrLf)
                                                              Catch ex As Exception
                                                              End Try
                                                          End Sub)
        Catch ex As Exception
        End Try
    End Sub

    Public Sub LogDebug(ByVal Hat As String, ByVal Barkod As String, ByVal Mesaj As String)
        Try
            System.Threading.ThreadPool.QueueUserWorkItem(Sub(state)
                                                              Try
                                                                  Dim fullPath As String = GetDailyLogPath("AllLogs")
                                                                  Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                                                  Dim logEntry As String = $"[{timestamp}] Hat-{Hat} | Barkod: {Barkod} | {Mesaj}"

                                                                  SyncLock GetType(LogYaz)
                                                                      System.IO.File.AppendAllText(fullPath, logEntry & vbCrLf)
                                                                      CheckAndRotate(fullPath)
                                                                  End SyncLock
                                                              Catch ex As Exception
                                                              End Try
                                                          End Sub)
        Catch ex As Exception
        End Try
    End Sub

    Public Sub LogAllOperations(ByVal Hat As String, ByVal EventType As String, ByVal Details As String)
        Try
            System.Threading.ThreadPool.QueueUserWorkItem(Sub(state)
                                                              Try
                                                                  ' Günlük log dosyası
                                                                  Dim fullPath As String = GetDailyLogPath("AllLog")
                                                                  Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                                                                  Dim logEntry As String = $"[{timestamp}] HAT-{Hat} | {EventType} | {Details}"

                                                                  SyncLock GetType(LogYaz)
                                                                      System.IO.File.AppendAllText(fullPath, logEntry & vbCrLf)
                                                                      CheckAndRotate(fullPath)
                                                                      ' Eski logları temizle (günde 1 kez)
                                                                      CleanupOldLogs()
                                                                  End SyncLock
                                                              Catch ex As Exception
                                                              End Try
                                                          End Sub)
        Catch ex As Exception
        End Try
    End Sub

    Function Klasor_Kontrol() As Boolean

        On Error GoTo hata

        Klasor_Kontrol = False
        DosyaPath = Application.StartupPath & "\LOG\"

        If Not (IO.Directory.Exists(Application.StartupPath & "\LOG")) Then
            System.IO.Directory.CreateDirectory(Application.StartupPath & "\LOG")
            LogTut("Klasör Yaratıldı : ", DosyaPath, Format(Now, "yyyyMMdd") & ".LOG")
        End If
        Klasor_Kontrol = True
        Exit Function

hata:

    End Function
End Module
