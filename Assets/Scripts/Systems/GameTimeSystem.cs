using Unity.Entities;

// Tracks in-game minutes, hours and days
// Broadcasts global events at key times (eg. every x days, start new short term memory cycle)
[UpdateInGroup(typeof(ActionRefreshSystemGroup))]
public partial struct GameTimeSystem : ISystem
{
    int m_timeSteps;

    public void OnUpdate(ref SystemState state)
    {
        m_timeSteps++;
        GameTimeUI.Instance.UpdateTimeUI(m_timeSteps);
    }
}