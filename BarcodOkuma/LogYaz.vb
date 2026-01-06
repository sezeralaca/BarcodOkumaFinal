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
