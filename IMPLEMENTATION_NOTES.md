# SAP Async Requests Implementation

## Overview
This implementation enables parallel, non-blocking processing of barcode scans from three independent readers (A, B, C). Each reader can simultaneously read barcodes, query SAP, and write to SQL/LOG without blocking the others.

## Architecture

### 1. Barcode Reading Layer
- **Independent threads**: Each reader (A, B, C) has its own dedicated thread
- **TCP-based**: Connects to barcode scanners via TCP sockets (192.168.0.6:2112, 192.168.0.13:2112, 192.168.0.8:2112)
- **Non-blocking**: Uses ThreadPool.QueueUserWorkItem to process barcodes asynchronously
- **Location**: `OnBarcodeReceived()` method (lines 537-559)

### 2. SAP Processing Layer
- **Hat-specific connections**: Separate SAP connection for each reader (sapAppA, sapAppB, sapAppC)
- **Thread-safe**: Each connection protected by its own lock (sapLockA, sapLockB, sapLockC)
- **Parallel execution**: All three SAP queries can execute simultaneously
- **Location**: `VeriOnay()` method (lines 384-527)

### 3. Queue-Based Background Processing
- **ConcurrentQueue per hat**: Thread-safe queues (queueA, queueB, queueC, queueError)
- **Dedicated worker threads**: One background thread per queue (loggerThreadA, loggerThreadB, loggerThreadC, loggerThreadError)
- **Non-blocking**: VeriOnay returns immediately after enqueuing, background thread handles SQL/LOG
- **Location**: `BackgroundLoggerThread()` method (lines 126-206)

### 4. SQL & LOG Layer
- **SQL format preserved**: Uses exact string concatenation format from commit 497e3a65
- **Connect_DB_Execute**: Unchanged method as required
- **SQL injection protection**: SqlEscape function escapes single quotes
- **File logging**: Async file writes via LogTutGenericAsync
- **Location**: `BuildSqlInsertStatement()` method (lines 112-123)

## Data Flow

```
Barcode Scanner → TCP Socket → OnBarcodeReceived
                                      ↓
                            ThreadPool.QueueUserWorkItem
                                      ↓
                            VeriOnay (SAP Query)
                                      ↓
                            ConcurrentQueue.Enqueue
                                      ↓
                            BackgroundLoggerThread
                                      ↓
                        ┌───────────────────────┐
                        ↓                       ↓
                SQL Insert              File Write (LOG)
            (Connect_DB_Execute)    (LogTutGenericAsync)
```

## Key Components

### SqlEscape Function
```vb
Private Function SqlEscape(value As String) As String
    If String.IsNullOrEmpty(value) Then
        Return ""
    End If
    Return value.Replace("'", "''")
End Function
```
Standard SQL Server escaping: replaces single quotes with double single quotes.

### BuildSqlInsertStatement Function
```vb
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
```
Constructs SQL INSERT using exact format from 497e3a65 commit with SQL injection protection.

## Graceful Shutdown

When the application closes:
1. Cancellation tokens signal all threads to stop
2. Background logger threads process remaining queue items (up to 100 per queue)
3. All connections and resources are properly disposed
4. Location: `Window_FormClosing()` handler (lines 318-382)

## Thread Safety

### SAP Connections
- Each hat has its own SAP connection object
- Protected by dedicated locks (SyncLock sapLockA/B/C)
- Prevents race conditions during SAP queries

### Queues
- ConcurrentQueue<T> provides lock-free thread-safe operations
- Multiple threads can enqueue simultaneously
- Single background thread dequeues per queue

### UI Updates
- All UI updates use BeginInvoke for thread-safe marshaling
- Background threads never access UI directly

## Performance Characteristics

### Before Implementation
- Sequential processing: SAP → SQL → LOG
- Single-threaded bottleneck
- Reader A blocked by B and C
- Slow response times

### After Implementation
- Parallel processing: 3 SAP queries simultaneously
- 3 SQL inserts simultaneously
- Async file I/O
- Independent readers
- Fast response times

## SQL Format Compliance

The SQL code **exactly matches** the format from commit 497e3a65:
- String concatenation (not parameterized queries)
- Uses Connect_DB_Execute method (unchanged)
- Two spaces before VALUES keyword (preserved)
- Boolean false as 'False' string literal
- Added SqlEscape for security without changing format

## Testing Recommendations

1. **Concurrent barcode scanning**: Scan barcodes on all three readers simultaneously
2. **SAP load test**: Verify all three SAP connections can handle parallel requests
3. **Queue processing**: Monitor AllLogs.txt to verify parallel SQL inserts
4. **Graceful shutdown**: Close application with items in queue, verify they're processed
5. **SQL injection**: Test with barcodes containing single quotes (should be escaped)

## Monitoring

### Comprehensive Logging (AllLog.txt)
The system includes comprehensive logging to `AllLog.txt` with millisecond-precision timestamps. All log entries follow the format:
```
[YYYY-MM-DD HH:mm:ss.fff] HAT-{A|B|C} | {EVENT_TYPE} | {Details}
```

**Logged Events:**
1. **BARCODE_RECEIVED** - Barcode read from scanner
   - Example: `[2026-01-09 14:30:45.123] HAT-A | BARCODE_RECEIVED | Okunan Barkod: 12345ABC | IP: 192.168.0.6 | Ağırlık: 75.5`

2. **SAP_CALL_START** - Before SAP web service call
   - Example: `[2026-01-09 14:30:45.234] HAT-A | SAP_CALL_START | Barkod: 12345ABC | Ağırlık: 75.5 | Tarih: 2026-01-09`

3. **SAP_RESPONSE** - Successful SAP response
   - Example: `[2026-01-09 14:30:47.567] HAT-A | SAP_RESPONSE | Status: S | Sonuç: 456789 | Ağırlık: 75.5`

4. **SAP_ERROR** - SAP call failed
   - Example: `[2026-01-09 14:31:20.570] HAT-A | SAP_ERROR | Barkod: INVALID123 | Mesaj: Connection timeout | Stack: at System.Net...`

5. **SQL_INSERT_START** - Before SQL INSERT operation
   - Example: `[2026-01-09 14:30:47.580] HAT-A | SQL_INSERT_START | Query: INSERT INTO [SIMFER].[dbo].[AMBAR]... | Barkod: 12345ABC | Hat: A | Sonuç: 456789`

6. **SQL_INSERT_SUCCESS** - SQL INSERT completed
   - Example: `[2026-01-09 14:30:47.612] HAT-A | SQL_INSERT_SUCCESS | Rows Affected: 1`

7. **SQL_INSERT_ERROR** - SQL INSERT failed
   - Example: `[2026-01-09 14:32:10.123] HAT-C | SQL_INSERT_ERROR | Mesaj: Cannot insert duplicate key | Query: INSERT INTO...`

8. **LOG_FILE_WRITE** - Writing to Barcod1/2/3.txt
   - Example: `[2026-01-09 14:30:47.625] HAT-A | LOG_FILE_WRITE | File: Barcod1.txt | Data: 12345ABC;456789`

9. **LOG_FILE_ERROR** - File write failed
   - Example: `[2026-01-09 14:40:15.905] HAT-A | LOG_FILE_ERROR | File: Barcod1.txt | Mesaj: File is being used by another process`

10. **PROCESS_COMPLETE** - End-to-end processing complete
    - Example: `[2026-01-09 14:30:47.650] HAT-A | PROCESS_COMPLETE | Barkod: 12345ABC | Sonuç: Başarılı | Süre: 2527ms`

11. **ERROR** - General errors
    - Example: `[2026-01-09 14:31:05.555] HAT-A | ERROR | Tür: Connection Error | Mesaj: Unable to connect to remote server`

12. **CONNECTION_STATUS** - Barcode reader connection state
    - Example: `[2026-01-09 14:33:00.001] HAT-A | CONNECTION_STATUS | HAT: A | Status: Bağlı | Adres: 192.168.0.6:2112`

**File Management:**
- Location: `{Application.StartupPath}\LOG\AllLog.txt`
- Thread-safe async writes via ThreadPool
- Automatic rotation at 50MB (rotated files: `AllLog_yyyyMMdd_HHmmss.txt`)
- Non-blocking I/O (silent fail on errors to avoid production impact)

### Legacy Logging (AllLogs.txt)
Check `AllLogs.txt` for backward-compatible operation tracking:
- `SAP: OK` - Successful SAP query
- `SAP: ERROR` - SAP query failure
- `SQL Insert: OK` - Successful database insert
- `SQL Insert: ERROR` - Database insert failure
- `Dosya Yazma: <filename>` - File write operation

## Configuration

### Constants
- `MAX_SHUTDOWN_QUEUE_ITEMS = 100` - Max items processed during shutdown per queue
- `QUEUE_POLL_INTERVAL_MS = 500` - Wait time when queue is empty (milliseconds)

### Connection Details
- Reader A: 192.168.0.6:2112
- Reader B: 192.168.0.13:2112
- Reader C: 192.168.0.8:2112
- SQL Server: TR-BILGISAYAR\WINCCFLEXEXPRESS
- Database: SIMFER
- SAP: sapapp.sersim.smfr.local:8000

## Troubleshooting

### Reader not working
- Check TCP connection status (lblConnectionA/B/C)
- Verify network connectivity
- Check ListBox1 for connection messages

### SAP timeout
- Default timeout: 15 seconds
- Check network to SAP server
- Verify credentials (msk.services)

### SQL insert failures
- Check connection string in Database.vb
- Verify SQL Server is accessible
- Check AllLog.txt for SQL_INSERT_ERROR events with specific error messages
- Check AllLogs.txt for legacy error tracking

### Slow processing
- Check queue sizes (should remain small)
- Monitor CPU usage
- Check SAP response times in AllLog.txt (look for time between SAP_CALL_START and SAP_RESPONSE)
- Verify SQL Server performance
- Review PROCESS_COMPLETE entries for duration metrics

### Log file not created
- Verify LOG folder exists (created automatically via Klasor_Kontrol)
- Check write permissions on LOG folder
- Ensure application has write access to Application.StartupPath\LOG
- Check for file system errors in event viewer

### Missing log entries
- Verify logging is not silently failing (check application is running)
- Ensure sufficient disk space for log files
- Check if file rotation occurred (look for AllLog_*.txt files)
- Review thread pool status (logging uses async threads)
