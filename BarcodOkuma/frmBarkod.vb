Imports BarcodOkuma.LogYaz
Imports BarcodOkuma.Database
Imports Microsoft.Office.Interop
Imports System.Threading
Imports System.IO.Ports
Imports System.Net.Sockets
Imports Snap7
Imports System.Net
Imports BarcodOkuma.local.smfr.sersim.sapapp
Imports System.Collections.Concurrent

Partial Public Class frmBarkod

    Private port1 As New SerialPort("COM1", 9600, Parity.None, 8, StopBits.One)
    Private port2 As New SerialPort("COM2", 9600, Parity.None, 8, StopBits.One)
    Private port3 As New SerialPort("COM3", 9600, Parity.None, 8, StopBits.One)

    Private rdthread As System.Threading.Thread
    Private sclient As S7Client = New S7Client()
    Private res As Integer = sclient.ConnectTo("192.168.0.1", 0, 2)
    Private sclient2 As S7Client = New S7Client()
    Private res2 As Integer = sclient2.ConnectTo("192.168.0.2", 0, 2)
    Private sclient3 As S7Client = New S7Client()
    Private res3 As Integer = sclient3.ConnectTo("192.168.0.3", 0, 2)

    Private tcpClientA As TcpClient
    Private tcpClientB As TcpClient
    Private tcpClientC As TcpClient
    Private threadA As Thread
    Private threadB As Thread
    Private threadC As Thread
    Private cancellationSourceA As Threading.CancellationTokenSource
    Private cancellationSourceB As Threading.CancellationTokenSource
    Private cancellationSourceC As Threading.CancellationTokenSource

    ' Persistent SAP connections for performance optimization - one per hat for parallel processing
    Private sapAppA As ZSFR_MM_022_FM_01 = Nothing
    Private sapAppB As ZSFR_MM_022_FM_01 = Nothing
    Private sapAppC As ZSFR_MM_022_FM_01 = Nothing
    Private ReadOnly sapLockA As New Object()
    Private ReadOnly sapLockB As New Object()
    Private ReadOnly sapLockC As New Object()

    ' Configuration constants
    Private Const MAX_SHUTDOWN_QUEUE_ITEMS As Integer = 100  ' Max items to process during shutdown
    Private Const QUEUE_POLL_INTERVAL_MS As Integer = 500     ' Wait time when queue is empty (ms)
    Private Const SQL_PREVIEW_LENGTH As Integer = 100         ' SQL query preview length for logging
    Private Const MAX_STACK_TRACE_LENGTH As Integer = 200     ' Stack trace truncation length for logging

    ' Data structure for queued log items
    Private Class LogItem
        Public Property Barkod As String
        Public Property Hat As String
        Public Property Agirlik As String
        Public Property Sonuc As String
        Public Property Cevap As Boolean
        Public Property Tarih As DateTime
    End Class

    ' Concurrent queues for each hat (thread-safe)
    Private queueA As New ConcurrentQueue(Of LogItem)()
    Private queueB As New ConcurrentQueue(Of LogItem)()
    Private queueC As New ConcurrentQueue(Of LogItem)()
    Private queueError As New ConcurrentQueue(Of LogItem)()  ' For unexpected hat values

    ' Background logger threads
    Private loggerThreadA As Thread
    Private loggerThreadB As Thread
    Private loggerThreadC As Thread
    Private loggerThreadError As Thread
    Private loggerCancellationSource As CancellationTokenSource

    Public Sub New()
        InitializeComponent()
    End Sub

    ' Initialize persistent SAP connections for performance optimization - one per hat
    Private Sub InitializeSAPConnection()
        Try
            ' Create separate SAP connection for each hat to enable parallel processing
            CreateSAPConnection("A")
            CreateSAPConnection("B")
            CreateSAPConnection("C")
            
            ' Use BeginInvoke for thread-safe UI update
            If Me.InvokeRequired Then
                Me.BeginInvoke(New Action(Sub() ListBox1.Items.Add("SAP bağlantıları kuruldu (A, B, C)")))
            Else
                ListBox1.Items.Add("SAP bağlantıları kuruldu (A, B, C)")
            End If
        Catch ex As Exception
            ' Use BeginInvoke for thread-safe UI update
            If Me.InvokeRequired Then
                Me.BeginInvoke(New Action(Sub()
                                              ListBox1.Items.Add("SAP bağlantısı kurulamadı: " & ex.Message)
                                              TextBox1.Text += "SAP bağlantısı kurulamadı: " & ex.Message & vbCrLf
                                          End Sub))
            Else
                ListBox1.Items.Add("SAP bağlantısı kurulamadı: " & ex.Message)
                TextBox1.Text += "SAP bağlantısı kurulamadı: " & ex.Message & vbCrLf
            End If
        End Try
    End Sub

    ' Helper function to escape single quotes for SQL string concatenation (prevent SQL injection)
    Private Function SqlEscape(value As String) As String
        If String.IsNullOrEmpty(value) Then
            Return ""
        End If
        Return value.Replace("'", "''")
    End Function
    
    ' Helper function to get SQL query preview for logging
    Private Function GetSqlPreview(sql As String, Optional maxLength As Integer = SQL_PREVIEW_LENGTH) As String
        If String.IsNullOrEmpty(sql) Then
            Return ""
        End If
        If sql.Length > maxLength Then
            Return sql.Substring(0, maxLength) & "..."
        End If
        Return sql
    End Function

    ' Build SQL INSERT statement using exact format from 497e3a65 commit
    Private Function BuildSqlInsertStatement(item As LogItem) As String
        Dim sqlstr As String
        sqlstr = "INSERT INTO [SIMFER].[dbo].[AMBAR] ([BARKOD],[TARIH],[HAT],[CEVAP])  VALUES ( "
        sqlstr = sqlstr & "'" & SqlEscape(item.Barkod) & "', GETDATE(),"
        sqlstr = sqlstr & "'" & SqlEscape(item.Hat) & "',"
        If item.Cevap Then
            sqlstr = sqlstr & "'" & SqlEscape(item.Sonuc) & "')"
        Else
            sqlstr = sqlstr & "'False')"
        End If
        Return sqlstr
    End Function

    ' Background logger thread - processes queue items for one hat
    Private Sub BackgroundLoggerThread(queue As ConcurrentQueue(Of LogItem), hat As String, cancellationToken As CancellationToken)
        Try
            Me.BeginInvoke(New Action(Sub() ListBox1.Items.Add($"Logger thread başlatıldı ({hat})")))

            While Not cancellationToken.IsCancellationRequested
                Dim item As LogItem = Nothing
                If queue.TryDequeue(item) Then
                    Try
                        ' Process DB write (blocking operation moved to background)
                        ' Using exact SQL format from 497e3a65 commit with Connect_DB_Execute
                        Dim RET As Integer = 0
                        Dim sqlstr As String = BuildSqlInsertStatement(item)
                        Try
                            ' Log SQL insert start (using helper for preview)
                            Dim sqlPreview As String = GetSqlPreview(sqlstr)
                            LogYaz.LogAllOperations(item.Hat, "SQL_INSERT_START", $"Query: {sqlPreview} | Barkod: {item.Barkod} | Hat: {item.Hat} | Sonuç: {item.Sonuc}")
                            
                            RET = Connect_DB_Execute(sqlstr, enumDbType.Sql)
                            
                            ' Log successful SQL insert
                            LogYaz.LogAllOperations(item.Hat, "SQL_INSERT_SUCCESS", $"Rows Affected: {RET}")
                            ' Legacy log for backward compatibility
                            LogYaz.LogDebug(item.Hat, item.Barkod, $"SQL Insert: OK (RET={RET})")
                        Catch sqlEx As Exception
                            ' Log SQL insert error (using helper for preview)
                            Dim sqlPreview As String = GetSqlPreview(sqlstr)
                            LogYaz.LogAllOperations(item.Hat, "SQL_INSERT_ERROR", $"Mesaj: {sqlEx.Message} | Query: {sqlPreview}")
                            ' Legacy log for backward compatibility
                            LogYaz.LogDebug(item.Hat, item.Barkod, $"SQL Insert: ERROR ({sqlEx.Message})")
                            Throw ' Re-throw to be caught by outer catch
                        End Try

                        ' Process file write (non-blocking)
                        Dim fileName As String = ""
                        Try
                            Select Case item.Hat
                                Case "A"
                                    fileName = "Barcod1.txt"
                                    LogYaz.LogTutGenericAsync(item.Barkod & ";" & item.Sonuc, DosyaPath, fileName)
                                Case "B"
                                    fileName = "Barcod2.txt"
                                    LogYaz.LogTutGenericAsync(item.Barkod & ";" & item.Sonuc, DosyaPath, fileName)
                                Case "C"
                                    fileName = "Barcod3.txt"
                                    LogYaz.LogTutGenericAsync(item.Barkod & ";" & item.Sonuc, DosyaPath, fileName)
                                Case Else
                                    fileName = "Hata.txt"
                                    LogYaz.LogTutGenericAsync(item.Barkod & ";" & item.Sonuc, DosyaPath, fileName)
                            End Select
                            ' Log file write attempt
                            LogYaz.LogAllOperations(item.Hat, "LOG_FILE_WRITE", $"File: {fileName} | Data: {item.Barkod};{item.Sonuc}")
                            ' Legacy log for backward compatibility
                            LogYaz.LogDebug(item.Hat, item.Barkod, $"Dosya Yazma: {fileName} | Başlatıldı")
                        Catch fileEx As Exception
                            ' Log file write error
                            LogYaz.LogAllOperations(item.Hat, "LOG_FILE_ERROR", $"File: {fileName} | Mesaj: {fileEx.Message}")
                            ' Legacy log for backward compatibility
                            LogYaz.LogDebug(item.Hat, item.Barkod, $"Dosya Yazma: {fileName} | ERROR ({fileEx.Message})")
                        End Try

                    Catch ex As Exception
                        Me.BeginInvoke(New Action(Sub() ListBox1.Items.Add($"Logger hatası ({hat}) - Barkod: {item.Barkod}: {ex.Message}")))
                        ' Log general error
                        LogYaz.LogAllOperations(item.Hat, "ERROR", $"Tür: Logger Error | Mesaj: {ex.Message} | Barkod: {item.Barkod}")
                    End Try
                Else
                    ' Queue is empty, wait a bit before checking again (reduced CPU usage)
                    ' Use Task.Delay for proper cancellation support
                    Try
                        System.Threading.Tasks.Task.Delay(QUEUE_POLL_INTERVAL_MS, cancellationToken).Wait()
                    Catch ex As AggregateException
                        ' Expected when cancellation is requested
                    End Try
                End If
            End While

        Catch ex As Exception
            Me.BeginInvoke(New Action(Sub() ListBox1.Items.Add($"Logger thread hatası ({hat}): {ex.Message}")))
            LogYaz.LogAllOperations(hat, "ERROR", $"Tür: Logger Thread Error | Mesaj: {ex.Message}")
        Finally
            ' Process remaining items in queue before shutdown
            Dim item As LogItem = Nothing
            Dim shutdownProcessedCount As Integer = 0
            While queue.TryDequeue(item) AndAlso shutdownProcessedCount < MAX_SHUTDOWN_QUEUE_ITEMS
                Try
                    ' Quick processing of remaining items using exact SQL format from 497e3a65
                    Dim sqlstr As String = BuildSqlInsertStatement(item)
                    Dim RET As Integer = Connect_DB_Execute(sqlstr, enumDbType.Sql)
                    shutdownProcessedCount += 1
                Catch ex As Exception
                    ' Silently fail during shutdown to avoid blocking
                End Try
            End While
            
            Me.BeginInvoke(New Action(Sub() ListBox1.Items.Add($"Logger thread sonlandırıldı ({hat}) - Kalan {shutdownProcessedCount} kayıt işlendi")))
        End Try
    End Sub

    ' Create or reinitialize SAP connection for specific hat - enables parallel SAP calls
    Private Sub CreateSAPConnection(hat As String)
        Dim wsdlurl As String = "http://sapapp.sersim.smfr.local:8000/sap/bc/srt/wsdl/flv_10002A111AD1/bndg_url/sap/bc/srt/rfc/sap/zsfr_mm_008_fm_01/100/zsfr_mm_022_fm_01/zsfr_mm_022_fm_01?sap-client=100"
        Dim cre = New NetworkCredential("msk.services", "Sers!m2023.Prod").GetCredential(New Uri(wsdlurl), "Basic")
        
        Select Case hat
            Case "A"
                SyncLock sapLockA
                    sapAppA = New ZSFR_MM_022_FM_01()
                    sapAppA.Credentials = cre
                    sapAppA.Timeout = 15000 ' 15 second timeout for SAP operations
                End SyncLock
            Case "B"
                SyncLock sapLockB
                    sapAppB = New ZSFR_MM_022_FM_01()
                    sapAppB.Credentials = cre
                    sapAppB.Timeout = 15000 ' 15 second timeout for SAP operations
                End SyncLock
            Case "C"
                SyncLock sapLockC
                    sapAppC = New ZSFR_MM_022_FM_01()
                    sapAppC.Credentials = cre
                    sapAppC.Timeout = 15000 ' 15 second timeout for SAP operations
                End SyncLock
        End Select
    End Sub

    Sub tutorial()
        Dim T1, T2, T3, T4 As Thread

        T1 = New Thread(AddressOf myprocess)
        T2 = New Thread(AddressOf myprocess)
        T3 = New Thread(AddressOf myprocess)
        T4 = New Thread(AddressOf myprocess)

        T1.Start()
        T2.Start()
        T3.Start()
        T4.Start()

    End Sub

    Private Sub myprocess()

    End Sub

    Private Sub SurroundingSub()

        Dim DBNumber As Integer
        Dim Size As Integer
        Dim Result As Integer
        Dim buffer As Byte() = New Byte(0) {1}
        DBNumber = System.Convert.ToInt32(2)
        Size = System.Convert.ToInt32(1)
        Result = sclient.DBRead(DBNumber, 0, Size, buffer)


        If buffer(0) > 0 Then
            Timer2.Start()
        Else
            Timer2.Stop()


        End If

    End Sub
    Private Sub SurroundingSub2()

        Dim DBNumber As Integer
        Dim Size As Integer
        Dim Result As Integer
        Dim buffer As Byte() = New Byte(0) {1}
        DBNumber = System.Convert.ToInt32(1)
        Size = System.Convert.ToInt32(1)
        Result = sclient2.DBRead(DBNumber, 0, Size, buffer)


        If buffer(0) > 0 Then

            Timer3.Start()

        Else

            Timer3.Stop()
        End If

    End Sub
    Private Sub SurroundingSub3()

        Dim DBNumber As Integer
        Dim Size As Integer
        Dim Result As Integer
        Dim buffer As Byte() = New Byte(0) {1}
        DBNumber = System.Convert.ToInt32(1)
        Size = System.Convert.ToInt32(1)
        Result = sclient3.DBRead(DBNumber, 0, Size, buffer)


        If buffer(0) > 0 Then

            Timer1.Start()
        Else

            Timer1.Stop()
        End If

    End Sub

    Private Sub Window_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try
            ' Cancel all barcode reader threads
            If cancellationSourceA IsNot Nothing Then cancellationSourceA.Cancel()
            If cancellationSourceB IsNot Nothing Then cancellationSourceB.Cancel()
            If cancellationSourceC IsNot Nothing Then cancellationSourceC.Cancel()

            ' Cancel logger threads
            If loggerCancellationSource IsNot Nothing Then loggerCancellationSource.Cancel()

            If port1.IsOpen Then port1.Close()
            If port2.IsOpen Then port2.Close()
            If port3.IsOpen Then port3.Close()
            If sclient.Connected Then sclient.Disconnect()
            If sclient2.Connected Then sclient2.Disconnect()
            If sclient3.Connected Then sclient3.Disconnect()

            If tcpClientA IsNot Nothing Then tcpClientA.Close()
            If tcpClientB IsNot Nothing Then tcpClientB.Close()
            If tcpClientC IsNot Nothing Then tcpClientC.Close()

            If threadA IsNot Nothing AndAlso threadA.IsAlive Then threadA.Join(1000)
            If threadB IsNot Nothing AndAlso threadB.IsAlive Then threadB.Join(1000)
            If threadC IsNot Nothing AndAlso threadC.IsAlive Then threadC.Join(1000)

            ' Wait for logger threads to finish processing remaining queue items (longer timeout)
            If loggerThreadA IsNot Nothing AndAlso loggerThreadA.IsAlive Then loggerThreadA.Join(5000)
            If loggerThreadB IsNot Nothing AndAlso loggerThreadB.IsAlive Then loggerThreadB.Join(5000)
            If loggerThreadC IsNot Nothing AndAlso loggerThreadC.IsAlive Then loggerThreadC.Join(5000)
            If loggerThreadError IsNot Nothing AndAlso loggerThreadError.IsAlive Then loggerThreadError.Join(5000)

            ' Dispose cancellation sources
            If cancellationSourceA IsNot Nothing Then cancellationSourceA.Dispose()
            If cancellationSourceB IsNot Nothing Then cancellationSourceB.Dispose()
            If cancellationSourceC IsNot Nothing Then cancellationSourceC.Dispose()
            If loggerCancellationSource IsNot Nothing Then loggerCancellationSource.Dispose()

            ' Dispose SAP connections
            SyncLock sapLockA
                If sapAppA IsNot Nothing Then
                    sapAppA.Dispose()
                    sapAppA = Nothing
                End If
            End SyncLock
            
            SyncLock sapLockB
                If sapAppB IsNot Nothing Then
                    sapAppB.Dispose()
                    sapAppB = Nothing
                End If
            End SyncLock
            
            SyncLock sapLockC
                If sapAppC IsNot Nothing Then
                    sapAppC.Dispose()
                    sapAppC = Nothing
                End If
            End SyncLock
        Finally
            port1.Dispose()
            port2.Dispose()
            port3.Dispose()

        End Try
    End Sub

    Sub VeriOnay(Barkod As String, Hat As String, agirlik As String)

        Dim cevap As Boolean = False
        Dim inventserialid As String = ""
        Dim DataAtreaId As String = "SSM"
        Dim InvertSiteId As String = "1"
        Dim weight As String = ""
        Dim Sonuc As String = ""
        Dim kolon As String = ""
        Dim processStartTime As DateTime = DateTime.Now
        kolon = Hat
        weight = agirlik


        Try
            ' Log SAP call start
            LogYaz.LogAllOperations(Hat, "SAP_CALL_START", $"Barkod: {Barkod} | Ağırlık: {weight} | Tarih: {DateTime.Now.ToString("yyyy-MM-dd")}")
            
            ' Use hat-specific SAP connection for parallel processing
            ' Each hat (A, B, C) has its own connection and lock to avoid blocking
            Select Case Hat
                Case "A"
                    SyncLock sapLockA
                        If sapAppA Is Nothing Then
                            ' Fallback: initialize connection if not already done
                            CreateSAPConnection("A")
                        End If
                        
                        Dim p As ZSFR_MM_008_S_02 = New ZSFR_MM_008_S_02()
                        p.AGIRLIK = weight
                        p.SERINO = Barkod
                        p.TARIH = DateTime.Now.ToString("yyyy-MM-dd")
                        Dim parray As ZSFR_MM_008_S_02() = New ZSFR_MM_008_S_02(0) {}
                        parray(0) = p
                        Dim param As ZSFR_MM_008_FM_01 = New ZSFR_MM_008_FM_01()
                        param.IT_ITEMS = parray
                        Dim result = sapAppA.ZSFR_MM_008_FM_01(param)
                        If result.EV_STATUS = "S" Then
                            Sonuc = result.ET_ID(0).ZZAUFNR
                            ' Log successful SAP response
                            LogYaz.LogAllOperations(Hat, "SAP_RESPONSE", $"Status: S | Sonuç: {Sonuc} | Ağırlık: {weight}")
                            ' Legacy log for backward compatibility
                            LogYaz.LogDebug(Hat, Barkod, $"SAP: OK (Sonuç: {Sonuc})")
                        Else
                            ' Log SAP error response
                            LogYaz.LogAllOperations(Hat, "SAP_ERROR", $"Status: {result.EV_STATUS} | Barkod: {Barkod} | Mesaj: Non-success status returned")
                            ' Legacy log for backward compatibility
                            LogYaz.LogDebug(Hat, Barkod, $"SAP: ERROR (Status: {result.EV_STATUS})")
                        End If
                    End SyncLock
                    
                Case "B"
                    SyncLock sapLockB
                        If sapAppB Is Nothing Then
                            ' Fallback: initialize connection if not already done
                            CreateSAPConnection("B")
                        End If
                        
                        Dim p As ZSFR_MM_008_S_02 = New ZSFR_MM_008_S_02()
                        p.AGIRLIK = weight
                        p.SERINO = Barkod
                        p.TARIH = DateTime.Now.ToString("yyyy-MM-dd")
                        Dim parray As ZSFR_MM_008_S_02() = New ZSFR_MM_008_S_02(0) {}
                        parray(0) = p
                        Dim param As ZSFR_MM_008_FM_01 = New ZSFR_MM_008_FM_01()
                        param.IT_ITEMS = parray
                        Dim result = sapAppB.ZSFR_MM_008_FM_01(param)
                        If result.EV_STATUS = "S" Then
                            Sonuc = result.ET_ID(0).ZZAUFNR
                            ' Log successful SAP response
                            LogYaz.LogAllOperations(Hat, "SAP_RESPONSE", $"Status: S | Sonuç: {Sonuc} | Ağırlık: {weight}")
                            ' Legacy log for backward compatibility
                            LogYaz.LogDebug(Hat, Barkod, $"SAP: OK (Sonuç: {Sonuc})")
                        Else
                            ' Log SAP error response
                            LogYaz.LogAllOperations(Hat, "SAP_ERROR", $"Status: {result.EV_STATUS} | Barkod: {Barkod} | Mesaj: Non-success status returned")
                            ' Legacy log for backward compatibility
                            LogYaz.LogDebug(Hat, Barkod, $"SAP: ERROR (Status: {result.EV_STATUS})")
                        End If
                    End SyncLock
                    
                Case "C"
                    SyncLock sapLockC
                        If sapAppC Is Nothing Then
                            ' Fallback: initialize connection if not already done
                            CreateSAPConnection("C")
                        End If
                        
                        Dim p As ZSFR_MM_008_S_02 = New ZSFR_MM_008_S_02()
                        p.AGIRLIK = weight
                        p.SERINO = Barkod
                        p.TARIH = DateTime.Now.ToString("yyyy-MM-dd")
                        Dim parray As ZSFR_MM_008_S_02() = New ZSFR_MM_008_S_02(0) {}
                        parray(0) = p
                        Dim param As ZSFR_MM_008_FM_01 = New ZSFR_MM_008_FM_01()
                        param.IT_ITEMS = parray
                        Dim result = sapAppC.ZSFR_MM_008_FM_01(param)
                        If result.EV_STATUS = "S" Then
                            Sonuc = result.ET_ID(0).ZZAUFNR
                            ' Log successful SAP response
                            LogYaz.LogAllOperations(Hat, "SAP_RESPONSE", $"Status: S | Sonuç: {Sonuc} | Ağırlık: {weight}")
                            ' Legacy log for backward compatibility
                            LogYaz.LogDebug(Hat, Barkod, $"SAP: OK (Sonuç: {Sonuc})")
                        Else
                            ' Log SAP error response
                            LogYaz.LogAllOperations(Hat, "SAP_ERROR", $"Status: {result.EV_STATUS} | Barkod: {Barkod} | Mesaj: Non-success status returned")
                            ' Legacy log for backward compatibility
                            LogYaz.LogDebug(Hat, Barkod, $"SAP: ERROR (Status: {result.EV_STATUS})")
                        End If
                    End SyncLock
                    
                Case Else
                    ' Unknown hat - log error and skip processing
                    LogYaz.LogAllOperations(Hat, "ERROR", $"Tür: Unknown Hat | Mesaj: Invalid hat value: {Hat} | Barkod: {Barkod}")
                    LogYaz.LogDebug(Hat, Barkod, $"SAP: ERROR (Unknown hat: {Hat})")
                    Sonuc = "ERROR: Unknown Hat"
            End Select

            ' Thread-safe UI update
            If Not String.IsNullOrEmpty(Sonuc) AndAlso Sonuc <> "ERROR: Unknown Hat" Then
                Me.BeginInvoke(New Action(Sub() ListBox1.Items.Add(" " + Sonuc.ToString + "   " + weight)))
            End If

            cevap = True
        Catch ex As Exception
            ' Thread-safe UI updates
            Me.BeginInvoke(New Action(Sub()
                                          ListBox1.Items.Add(ex.ToString)
                                          TextBox1.Text += ex.ToString
                                      End Sub))
            ' Log SAP exception with stack trace (using constant for length, safe null handling)
            Dim stackTrace As String = ""
            If ex.StackTrace IsNot Nothing Then
                If ex.StackTrace.Length > MAX_STACK_TRACE_LENGTH Then
                    stackTrace = ex.StackTrace.Substring(0, MAX_STACK_TRACE_LENGTH)
                Else
                    stackTrace = ex.StackTrace
                End If
            End If
            LogYaz.LogAllOperations(Hat, "SAP_ERROR", $"Barkod: {Barkod} | Mesaj: {ex.Message} | Stack: {stackTrace}")
            ' Legacy log for backward compatibility
            LogYaz.LogDebug(Hat, Barkod, $"SAP: ERROR ({ex.Message})")

        End Try

        ' Queue the result for background processing (non-blocking)
        Dim logItem As New LogItem With {
            .Barkod = Barkod,
            .Hat = Hat,
            .Agirlik = agirlik,
            .Sonuc = Sonuc,
            .Cevap = cevap,
            .Tarih = DateTime.Now
        }

        ' Enqueue to appropriate hat queue
        Select Case Hat
            Case "A"
                queueA.Enqueue(logItem)
            Case "B"
                queueB.Enqueue(logItem)
            Case "C"
                queueC.Enqueue(logItem)
            Case Else
                ' Handle unexpected hat values with error queue (non-blocking)
                queueError.Enqueue(logItem)
        End Select

        ' Log process complete with total time
        Dim totalTime As Long = CLng((DateTime.Now - processStartTime).TotalMilliseconds)
        Dim resultStatus As String = If(cevap AndAlso Not String.IsNullOrEmpty(Sonuc) AndAlso Sonuc <> "ERROR: Unknown Hat", "Başarılı", "Hata")
        LogYaz.LogAllOperations(Hat, "PROCESS_COMPLETE", $"Barkod: {Barkod} | Sonuç: {resultStatus} | Süre: {totalTime}ms")

        ' Return immediately - background thread will process DB and file writes

    End Sub

    Function PrevInstance() As Boolean
        If UBound(Diagnostics.Process.GetProcessesByName(Diagnostics.Process.GetCurrentProcess.ProcessName)) > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub OnBarcodeReceived(deviceIp As String, barcode As String)
        ' Immediately update UI with barcode - non-blocking
        Select Case deviceIp
            Case "192.168.0.6"
                CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub() txtBarcodeA.Text = barcode), New Object() {})
                ' Process SAP query in background thread - non-blocking
                Dim hat As String = "A"
                Dim weight As String = txt_tartim2.Text
                ' Log barcode received event
                LogYaz.LogAllOperations(hat, "BARCODE_RECEIVED", $"Okunan Barkod: {barcode} | IP: {deviceIp} | Ağırlık: {weight}")
                System.Threading.ThreadPool.QueueUserWorkItem(Sub(state) VeriOnay(barcode, hat, weight))
            Case "192.168.0.13"
                CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub() txtBarcodeB.Text = barcode), New Object() {})
                ' Process SAP query in background thread - non-blocking
                Dim hat As String = "B"
                Dim weight As String = txt_tartim3.Text
                ' Log barcode received event
                LogYaz.LogAllOperations(hat, "BARCODE_RECEIVED", $"Okunan Barkod: {barcode} | IP: {deviceIp} | Ağırlık: {weight}")
                System.Threading.ThreadPool.QueueUserWorkItem(Sub(state) VeriOnay(barcode, hat, weight))
            Case "192.168.0.8"
                CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub() txtBarcodeC.Text = barcode), New Object() {})
                ' Process SAP query in background thread - non-blocking
                Dim hat As String = "C"
                Dim weight As String = txt_tartim1.Text
                ' Log barcode received event
                LogYaz.LogAllOperations(hat, "BARCODE_RECEIVED", $"Okunan Barkod: {barcode} | IP: {deviceIp} | Ağırlık: {weight}")
                System.Threading.ThreadPool.QueueUserWorkItem(Sub(state) VeriOnay(barcode, hat, weight))
        End Select
    End Sub

    Private Sub UpdateConnectionStatus(hat As String, isConnected As Boolean)
        ' Determine IP and port based on hat
        Dim ipPort As String = ""
        Select Case hat
            Case "A"
                ipPort = "192.168.0.6:2112"
            Case "B"
                ipPort = "192.168.0.13:2112"
            Case "C"
                ipPort = "192.168.0.8:2112"
        End Select
        
        ' Log connection status change
        Dim status As String = If(isConnected, "Bağlı", "Bağlı Değil")
        LogYaz.LogAllOperations(hat, "CONNECTION_STATUS", $"HAT: {hat} | Status: {status} | Adres: {ipPort}")
        
        CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                  Select Case hat
                                                                                      Case "A"
                                                                                          If isConnected Then
                                                                                              lblConnectionA.ForeColor = Color.Green
                                                                                              lblConnectionA.Text = "Bağlı"
                                                                                          Else
                                                                                              lblConnectionA.ForeColor = Color.Red
                                                                                              lblConnectionA.Text = "Bağlı Değil"
                                                                                          End If
                                                                                      Case "B"
                                                                                          If isConnected Then
                                                                                              lblConnectionB.ForeColor = Color.Green
                                                                                              lblConnectionB.Text = "Bağlı"
                                                                                          Else
                                                                                              lblConnectionB.ForeColor = Color.Red
                                                                                              lblConnectionB.Text = "Bağlı Değil"
                                                                                          End If
                                                                                      Case "C"
                                                                                          If isConnected Then
                                                                                              lblConnectionC.ForeColor = Color.Green
                                                                                              lblConnectionC.Text = "Bağlı"
                                                                                          Else
                                                                                              lblConnectionC.ForeColor = Color.Red
                                                                                              lblConnectionC.Text = "Bağlı Değil"
                                                                                          End If
                                                                                  End Select
                                                                              End Sub), New Object() {})
    End Sub

    Private Sub BarcodeReaderThread(ip As String, port As Integer, hat As String, ByRef client As TcpClient, cancellationToken As Threading.CancellationToken)
        Dim stream As NetworkStream = Nothing
        Dim lastLogTime As DateTime = DateTime.MinValue
        Const logInterval As Integer = 30 ' Log every 30 seconds

        Try
            ' Log thread start
            CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                       ListBox1.Items.Add($"Thread başlatıldı ({hat}): {ip}:{port}")
                                                                                   End Sub), New Object() {})

            While Not cancellationToken.IsCancellationRequested
                Try
                    ' Establish connection if not connected
                    If client Is Nothing OrElse Not client.Connected Then
                        If client IsNot Nothing Then
                            Try
                                client.Close()
                            Catch
                            End Try
                            client = Nothing
                        End If

                        ' Log connection attempt
                        CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                                   ListBox1.Items.Add($"Bağlantı kuruluyor ({hat}): {ip}:{port}")
                                                                                               End Sub), New Object() {})

                        client = New TcpClient()
                        client.Connect(ip, port)
                        stream = client.GetStream()
                        ' No timeout - use blocking read for immediate data capture
                        ' Requirement: Socket must listen indefinitely without timeout
                        ' Connection will be detected via bytesRead = 0 or IOException
                        stream.ReadTimeout = System.Threading.Timeout.Infinite

                        UpdateConnectionStatus(hat, True)
                        CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                                   ListBox1.Items.Add($"Bağlandı ({hat}): {ip}:{port}")
                                                                                               End Sub), New Object() {})
                    End If

                    ' Blocking read - waits indefinitely for data
                    Dim buffer As Byte() = New Byte(1023) {}
                    Dim bytesRead As Integer = stream.Read(buffer, 0, buffer.Length)

                    If bytesRead > 0 Then
                        Dim barcode As String = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim()
                        If Not String.IsNullOrWhiteSpace(barcode) Then
                            OnBarcodeReceived(ip, barcode)
                            CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                                       ListBox1.Items.Add($"Barkod alındı ({hat}): {barcode}")
                                                                                                   End Sub), New Object() {})
                        End If
                    ElseIf bytesRead = 0 Then
                        ' Connection closed by remote host
                        Throw New System.IO.IOException("Bağlantı uzak sunucu tarafından kapatıldı")
                    End If

                    ' Periodic "still alive" logging
                    If DateTime.Now.Subtract(lastLogTime).TotalSeconds >= logInterval Then
                        CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                                   ListBox1.Items.Add($"Dinleniyor ({hat}): {ip}:{port}")
                                                                                               End Sub), New Object() {})
                        lastLogTime = DateTime.Now
                    End If

                Catch ex As System.IO.IOException
                    ' Connection lost
                    UpdateConnectionStatus(hat, False)
                    ' Log connection error
                    LogYaz.LogAllOperations(hat, "ERROR", $"Tür: Connection Error | Mesaj: {ex.Message}")
                    CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                               ListBox1.Items.Add($"Bağlantı hatası ({hat}): {ex.Message}")
                                                                                           End Sub), New Object() {})

                    If stream IsNot Nothing Then
                        Try
                            stream.Close()
                        Catch
                        End Try
                        stream = Nothing
                    End If

                    If client IsNot Nothing Then
                        Try
                            client.Close()
                        Catch
                        End Try
                        client = Nothing
                    End If

                    ' Wait before reconnecting (cancellable)
                    cancellationToken.WaitHandle.WaitOne(2000)

                Catch ex As SocketException
                    ' Network error
                    UpdateConnectionStatus(hat, False)
                    ' Log network error
                    LogYaz.LogAllOperations(hat, "ERROR", $"Tür: Network Error | Mesaj: {ex.Message}")
                    CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                               ListBox1.Items.Add($"Ağ hatası ({hat}): {ex.Message}")
                                                                                           End Sub), New Object() {})

                    If stream IsNot Nothing Then
                        Try
                            stream.Close()
                        Catch
                        End Try
                        stream = Nothing
                    End If

                    If client IsNot Nothing Then
                        Try
                            client.Close()
                        Catch
                        End Try
                        client = Nothing
                    End If

                    ' Wait before reconnecting (cancellable)
                    cancellationToken.WaitHandle.WaitOne(2000)

                Catch ex As Exception
                    ' Other exceptions
                    LogYaz.LogAllOperations(hat, "ERROR", $"Tür: Barcode Reader Error | Mesaj: {ex.Message}")
                    CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                               ListBox1.Items.Add($"Beklenmeyen hata ({hat}): {ex.Message}")
                                                                                           End Sub), New Object() {})
                End Try
            End While

        Catch ex As Exception
            CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                       ListBox1.Items.Add($"Thread hatası ({hat}): {ex.Message}")
                                                                                   End Sub), New Object() {})
        Finally
            ' Clean up resources
            If stream IsNot Nothing Then
                Try
                    stream.Close()
                Catch
                End Try
            End If

            If client IsNot Nothing Then
                Try
                    client.Close()
                Catch
                End Try
            End If

            UpdateConnectionStatus(hat, False)
            CType(Me, System.ComponentModel.ISynchronizeInvoke).BeginInvoke(New Action(Sub()
                                                                                       ListBox1.Items.Add($"Thread sonlandırıldı ({hat})")
                                                                                   End Sub), New Object() {})
        End Try
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        txt_tartim1.Text = "0"
        txt_tartim2.Text = "0"
        txt_tartim3.Text = "0"

        ' Initialize persistent SAP connection for performance
        InitializeSAPConnection()

        ' Start background logger threads for parallel processing
        Try
            loggerCancellationSource = New CancellationTokenSource()
            
            loggerThreadA = New Thread(Sub() BackgroundLoggerThread(queueA, "A", loggerCancellationSource.Token))
            loggerThreadA.IsBackground = True
            loggerThreadA.Start()

            loggerThreadB = New Thread(Sub() BackgroundLoggerThread(queueB, "B", loggerCancellationSource.Token))
            loggerThreadB.IsBackground = True
            loggerThreadB.Start()

            loggerThreadC = New Thread(Sub() BackgroundLoggerThread(queueC, "C", loggerCancellationSource.Token))
            loggerThreadC.IsBackground = True
            loggerThreadC.Start()

            loggerThreadError = New Thread(Sub() BackgroundLoggerThread(queueError, "Error", loggerCancellationSource.Token))
            loggerThreadError.IsBackground = True
            loggerThreadError.Start()

            ListBox1.Items.Add("Background logger threads başlatıldı (A, B, C, Error)")
        Catch ex As Exception
            ListBox1.Items.Add("Logger thread başlatma hatası: " & ex.ToString)
            TextBox1.Text += "Logger thread başlatma hatası: " & ex.ToString & vbCrLf
        End Try

        Timer4.Start()

        Try
            If Not port1.IsOpen Then
                port1.Open()
                Button1.BackColor = Color.LimeGreen
            Else

            End If
        Catch ex As Exception When MsgBox("PORT HATASI")
            Timer1.Stop()
        End Try


        Try
            If Not port2.IsOpen Then
                port2.Open()
                Button2.BackColor = Color.LimeGreen
            Else

            End If
        Catch ex As Exception When MsgBox("PORT HATASI")
            Timer2.Stop()
        End Try


        Try

            If Not port3.IsOpen Then
                port3.Open()
                Button3.BackColor = Color.LimeGreen
            Else

            End If
        Catch ex As Exception When MsgBox("PORT HATASI")
            Timer3.Stop()
        End Try

        If PrevInstance() Then
            End
        End If

        If Not Klasor_Kontrol() Then
            Exit Sub
        End If

        Try
            cancellationSourceA = New Threading.CancellationTokenSource()
            threadA = New Thread(Sub() BarcodeReaderThread("192.168.0.6", 2112, "A", tcpClientA, cancellationSourceA.Token))
            threadA.IsBackground = True
            threadA.Start()
        Catch ex As Exception
            ListBox1.Items.Add(ex.ToString)
            TextBox1.Text += ex.ToString
        End Try

        Try
            cancellationSourceB = New Threading.CancellationTokenSource()
            threadB = New Thread(Sub() BarcodeReaderThread("192.168.0.13", 2112, "B", tcpClientB, cancellationSourceB.Token))
            threadB.IsBackground = True
            threadB.Start()
        Catch ex As Exception
            ListBox1.Items.Add(ex.ToString)
            TextBox1.Text += ex.ToString

        End Try

        Try
            cancellationSourceC = New Threading.CancellationTokenSource()
            threadC = New Thread(Sub() BarcodeReaderThread("192.168.0.8", 2112, "C", tcpClientC, cancellationSourceC.Token))
            threadC.IsBackground = True
            threadC.Start()
        Catch ex As Exception
            ListBox1.Items.Add(ex.ToString)
            TextBox1.Text += ex.ToString

        End Try

    End Sub

    Private Sub btnTop_Click(sender As Object, e As EventArgs) Handles btnTop.Click


        Dim sqlstr As String

        sqlstr = "SELECT TOP 10 BARKOD, COUNT (*) ADET FROM [SIMFER].[dbo].[AMBAR]WHERE [TARIH]>= [TARIH]-30 GROUP BY  BARKOD ORDER BY 2 DESC "

        Dim DS As DataSet
        DS = Connect_DB_Select(sqlstr, enumDbType.Sql)

        If DS.Tables.Count > 0 Then
            If DS.Tables(0).Rows.Count > 0 Then

                For i As Integer = 0 To DS.Tables(0).Rows.Count - 1
                    ListBox1.Items.Add(DS.Tables(0).Rows(i).Item(0) & "--" & DS.Tables(0).Rows(i).Item(1))
                Next

            End If

        End If

    End Sub


    Private Sub btnBarkod_Click(sender As Object, e As EventArgs) Handles btnBarkod.Click

        Dim sqlstr As String

        sqlstr = "SELECT TOP 10 BARKOD, COUNT (*) ADET FROM [SIMFER].[dbo].[AMBAR] WHERE BARKOD LIKE '" & txtBarcode.Text & "%' GROUP BY  BARKOD"

        Dim DS As DataSet
        DS = Connect_DB_Select(sqlstr, enumDbType.Sql)

        If DS.Tables.Count > 0 Then
            If DS.Tables(0).Rows.Count > 0 Then

                For i As Integer = 0 To DS.Tables(0).Rows.Count - 1
                    ListBox1.Items.Add(DS.Tables(0).Rows(i).Item(0) & "--" & DS.Tables(0).Rows(i).Item(1))
                Next

            End If

        End If

    End Sub

    Private Sub btnExcel_Click(sender As Object, e As EventArgs) Handles btnExcel.Click
        Dim sqlstr As String

        sqlstr = "SELECT * FROM [SIMFER].[dbo].[AMBAR] WHERE TARIH>=TARIH-92 ORDER BY TARIH"

        Dim DS As DataSet
        DS = Connect_DB_Select(sqlstr, enumDbType.Sql)

        If DS.Tables.Count > 0 Then
            If DS.Tables(0).Rows.Count > 0 Then

                ExcelYukle(DS.Tables(0))

            End If

        End If
    End Sub

    Sub ExcelYukle(dtTemp As DataTable)

        Dim _excel As New Excel.Application
        Dim wBook As Excel.Workbook
        Dim wSheet As Excel.Worksheet

        wBook = _excel.Workbooks.Add()
        wSheet = wBook.ActiveSheet()

        Dim dt As System.Data.DataTable = dtTemp
        Dim dc As System.Data.DataColumn
        Dim dr As System.Data.DataRow
        Dim colIndex As Integer = 0
        Dim rowIndex As Integer = 0

        For Each dc In dt.Columns
            colIndex = colIndex + 1
            wSheet.Cells(1, colIndex) = dc.ColumnName
        Next

        For Each dr In dt.Rows
            rowIndex = rowIndex + 1
            colIndex = 0
            For Each dc In dt.Columns
                colIndex = colIndex + 1
                wSheet.Cells(rowIndex + 1, colIndex) = dr(dc.ColumnName)
            Next
        Next
        wSheet.Columns.AutoFit()

        Dim strFileName As String = My.Application.Info.DirectoryPath & "\" & Format(Now, "yyyyMMddHHss") & ".xls"
        wBook.SaveAs(strFileName)

        ListBox1.Items.Add("Bilgiler Excele Aktar�lm��t�r.")
        ListBox1.Items.Add(strFileName)

        releaseObject(wSheet)
        wBook.Close(False)
        releaseObject(wBook)
        _excel.Quit()
        releaseObject(_excel)
        GC.Collect()

    End Sub


    Private Sub releaseObject(obj As Object)
        Try
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj)
            obj = Nothing
        Catch ex As Exception
            obj = Nothing
            MessageBox.Show("Unable to release the Object " + ex.ToString())
        Finally
            GC.Collect()
        End Try
    End Sub


    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Try
            If port1.IsOpen Then
                port1.Write("W")
                Dim i As Single = port1.ReadExisting()
                txt_tartim1.Text = i.ToString()
                txt_tartim1.ForeColor = Color.LimeGreen
                Button1.BackColor = Color.LimeGreen

            Else
                txt_tartim1.ForeColor = Color.Red
                txt_tartim1.Text = "NOT OPEN"

            End If



        Catch ex As Exception

        End Try
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Try
            If port2.IsOpen Then
                port2.Write("W")
                Button2.BackColor = Color.LimeGreen
                Dim j As Single = port2.ReadExisting()
                txt_tartim2.Text = j.ToString()
                txt_tartim2.ForeColor = Color.Yellow
            Else
                txt_tartim2.ForeColor = Color.Red
                txt_tartim2.Text = "NOT OPEN"

            End If




        Catch ex As Exception

        End Try
    End Sub


    Private Sub Timer3_Tick(sender As Object, e As EventArgs) Handles Timer3.Tick
        Try
            If port3.IsOpen Then
                port3.Write("W")

                Button3.BackColor = Color.LimeGreen
                Dim f As Single = port3.ReadExisting()
                txt_tartim3.Text = f.ToString()
                txt_tartim3.ForeColor = Color.Orange
            Else
                txt_tartim3.ForeColor = Color.Red

                txt_tartim3.Text = "NOT OPEN"
            End If




        Catch ex As Exception

        End Try
    End Sub

    Private Sub Timer4_Tick(sender As Object, e As EventArgs) Handles Timer4.Tick

        SurroundingSub()
        SurroundingSub2()
        SurroundingSub3()
    End Sub

    Private Sub txt_tartim2_TextChanged(sender As Object, e As EventArgs) Handles txt_tartim2.TextChanged

    End Sub

    Private Sub txt_tartim1_TextChanged(sender As Object, e As EventArgs) Handles txt_tartim1.TextChanged

    End Sub
End Class
