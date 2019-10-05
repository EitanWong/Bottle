

[System.Serializable]
public struct Setting
{
    public int FPSLimit;
    public Quality GameQuality;

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
