using System;
using System.IO;

public static class Logger
{
    private static readonly string baseFolder;
    private static DateTime currentDate;
    private static StreamWriter writer;

    static Logger()
    {
        baseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GyeaRyong_WqMonitoring"
        );

        Directory.CreateDirectory(baseFolder);
        UpdateLogFile(DateTime.Now);
    }

    public static void WriteLineAndLog(string msg)
    {
        DateTime now = DateTime.Now;
        Console.WriteLine($"[{now:yyyy-MM-dd HH:mm:ss}] {msg}");


        // 날짜가 바뀌었으면 새 파일로 교체
        if (now.Date != currentDate.Date)
        {
            //UpdateLogFile(now);
        }

        try
        {
            writer.WriteLine($"[{now:yyyy-MM-dd HH:mm:ss}] {msg}");
            writer.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Logger] 파일 쓰기 실패: {ex.Message}");
        }
    }

    private static void UpdateLogFile(DateTime now)
    {
        try
        {
            writer?.Dispose();

            string logFilePath = Path.Combine(baseFolder, $"{now:yyyy-MM-dd}.log");
            writer = new StreamWriter(logFilePath, append: true)
            {
                AutoFlush = true
            };

            currentDate = now;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Logger] 로그 파일 열기 실패: {ex.Message}");
        }
    }

    public static void Close()
    {
        writer?.Close();
        writer = null;
    }
}
