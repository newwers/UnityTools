using UnityEngine;

public class LogManager  {
    
    
    public static void Log(string log)
    {
        Debug.Log(log);
    }

    public static void LogError(string log)
    {
        Debug.LogError(log);
    }
}
