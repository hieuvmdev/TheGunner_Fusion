using UnityEngine;

public static class IdleCheck
{
    public static int Timeout = 300;

    private static float lastAction;
    public static void ReportAction()
    {
        lastAction = Time.time;
    }

    public static bool IsIdle
    {
        get
        {
            return (Time.time - lastAction) > Timeout;
        }
    }
}