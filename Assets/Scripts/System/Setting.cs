


[System.Serializable]
public struct Setting
{
    public int FPSLimit;
    public Quality GameQuality;

    public UnityEngine.Vector3 Gravity;
}

public enum Quality
{
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh,
    Ultra,
}
