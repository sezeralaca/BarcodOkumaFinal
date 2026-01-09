Imports System
Imports System.IO
Imports System.IO.File


Module LogYaz

    Public DosyaPath As String = Application.StartupPath & "\LOG\"
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
