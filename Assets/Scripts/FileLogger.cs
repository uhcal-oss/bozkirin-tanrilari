using UnityEngine;
using System.IO;
using System;

public static class FileLogger
{
    private static string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MenuDebugLog.txt");

    public static void Log(string message)
    {
        try
        {
            string line = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] {message}";
            File.AppendAllText(logPath, line + Environment.NewLine);
        }
        catch { }
    }

    public static void Clear()
    {
        try
        {
            File.WriteAllText(logPath, "--- NEW SESSION ---\n");
        }
        catch { }
    }
}
