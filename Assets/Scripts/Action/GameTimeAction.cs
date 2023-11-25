public struct SetEnvironmentLight : GameAction
{
    public EnvironmentLightData environmentLightData;
    public SetResult setResult { get; set; }
} 
public struct OverrideEnvironmentLight : GameAction
{
    public EnvironmentLightData environmentLightData;
    public SetResult setResult { get; set; }
}
public struct ClearOverrideEnvironmentLight : GameAction
{
     public SetResult setResult { get; set; }
}