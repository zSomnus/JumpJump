using System.Collections.Generic;
using UnityEngine;

public static class GameTime
{
    public static bool IsGamePaused { get; private set; }
    public static float InitialTimeScale { get; set; } = 1;
    static float GlobalSlow = 1;
    const float GlobalSlowSpeed = 2;
    static readonly bool isUsingFullPause = true;

    public static float deltaTime
    {
        get
        {
            return IsGamePaused ? 0 : (Time.deltaTime * GetSlowValue());
        }
    }

    public static float fixedDeltaTime
    {
        get
        {
            return IsGamePaused ? 0 : (GameTime.deltaTime * GetSlowValue());
        }
    }

    static int pauseRequestCount;
    static HashSet<string> semaphore = new HashSet<string>();

    public static void Update()
    {
        GlobalSlow = Mathf.Min(GlobalSlow + GlobalSlowSpeed * Time.deltaTime, 1);
    }

    public static float GetSlowValue()
    {
        return Mathf.Pow(GlobalSlow, 3);
    }

    public static void StartGlobalSlow()
    {
        GlobalSlow = 0;
    }

    public static void RequestPause(Object context)
    {
        string log = "Request pause.";
        if (context != null)
        {
            log += " Context = " + context.ToString();
        }
        Debug.Log(log, context);
        string contextName = context.GetType().Name;

        bool wasGamePaused = IsGamePaused;

        if (!semaphore.Contains(contextName))
        {
            IsGamePaused = ++pauseRequestCount > 0;
            semaphore.Add(contextName);
        }

        if (isUsingFullPause && !wasGamePaused && IsGamePaused)
        {
            Time.timeScale = 0;
        }
    }

    public static void RequestUnpause(Object context)
    {
        string log = "Request unpause.";
        if (context != null)
        {
            log += " Context = " + context.ToString();
        }

        Debug.Log(log, context);
        string contextName = context.GetType().Name;

        bool wasGamePaused = IsGamePaused;
        if (semaphore.Contains(contextName))
        {
            semaphore.Remove(contextName);
            IsGamePaused = --pauseRequestCount > 0;
        }

        if (isUsingFullPause && wasGamePaused && !IsGamePaused)
        {
            Time.timeScale = GameTime.InitialTimeScale;
        }
    }
}
