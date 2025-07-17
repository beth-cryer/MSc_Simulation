using Unity.Entities;

public partial class ActionRefreshSystemGroup: ComponentSystemGroup
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RateManager = new RateUtils.FixedRateCatchUpManager(1.0f);
    }
}