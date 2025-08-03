using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class SelectedEntityUI : MonoBehaviour
{
    [SerializeField] private TMP_Text m_text;
    public NPC DisplayedNPC;
    public List<Need> DisplayedNeeds;

    private NeedData[] m_needData;

    private void Start()
    {
        m_needData = Resources.LoadAll<NeedData>("Data/Needs/");
    }

    public void UpdateUI(NPC npc, List<Need> needs, string goal)
    {
        string text = string.Format("{0}\n\nGOAL:\n{1}\n\nNEEDS:\n", npc.Name, goal);
        foreach (Need need in needs)
        {
            NeedData needData = m_needData.FirstOrDefault(x => x.Type == need.Type);

            if (needData == null) return;
            if (!needData.IsVisibleNeed) return;

            text += string.Format("{0}: {1}\n", needData.Name, DisplayFloat3(need.Value));
        }

        m_text.SetText(text);
    }

    private string DisplayFloat3(float3 values)
    {
        return string.Format(
            "{0}, {1}, {2}",
            values[0].ToString("F2"),
            values[1].ToString("F2"),
            values[2].ToString("F2"));
    }

    public static SelectedEntityUI Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
}
