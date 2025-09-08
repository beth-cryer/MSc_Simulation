using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class SelectedEntityUI : MonoBehaviour
{
	[SerializeField] private CanvasGroup m_entityLayoutGroup;
    [SerializeField] private GameObject m_prefab_needBar;
    [SerializeField] private TMP_Text m_text;
	[SerializeField] private TMP_Text m_textTraits;
    [SerializeField] private Transform m_needsLayoutGroup;
    public NPC DisplayedNPC;
    public List<Need> DisplayedNeeds;

    private NeedData[] m_needData;
    private TraitData[] m_traitData;
	private EmotionData[] m_emotionData;

    private void Start()
    {
        m_needData = Resources.LoadAll<NeedData>("Data/Needs/");
		m_traitData = Resources.LoadAll<TraitData>("Data/Traits/");
		m_emotionData = Resources.LoadAll<EmotionData>("Data/Emotions/");
	}

	public void HideUI()
	{
		m_entityLayoutGroup.alpha = 0.0f;
	}

    public void UpdateUI(NPC npc, List<Need> needs, List<ENeed> changingNeeds, List<Trait> traits, string name, string goal)
    {
		m_entityLayoutGroup.alpha = 1.0f;

		float3 moodValue = needs.FirstOrDefault(x => x.Type.Equals(ENeed.Mood)).Value;
		int moodIndex = 0;
		float bestDistance = Mathf.Infinity;
		for (int i = 0; i < m_emotionData.Length; i++)
		{
			float distance = math.distance(moodValue, m_emotionData[i].PADValue);
			if (distance < bestDistance)
			{
				bestDistance = distance;
				moodIndex = i;
			}
		}
		//string emotionName = string.Format(m_emotionData[moodIndex].Name + " - " + moodValue.ToString());
		string emotionName = m_emotionData[moodIndex].Name;

		string text = string.Format("{0}\n\nGOAL:\n{1}\n\nMOOD: {2}", name, goal, emotionName);
		string textTraits = "";
		foreach (var trait in traits)
		{
			TraitData traitData = m_traitData.FirstOrDefault(x => x.Type.Equals(trait.Type));
			if (traitData == null)
				continue;

			textTraits += traitData.Name + "\n";
		}
		m_textTraits.SetText(textTraits);

        // Needs bars-
        // Check how many there are in children, add extras if needed
        int childCount = m_needsLayoutGroup.childCount;
        if (needs.Count > childCount)
        {
            for (int i = 0; i < needs.Count - childCount; i++)
            {
                var newNeedBar = Instantiate(m_prefab_needBar, m_needsLayoutGroup);
            }
        }

        //Update all needs
        int needIndex = 0;

        // loop through needs transform children, find valid visible need to populate with
        // if we run out of needs and there's still children left, set remaining to inactive
        for (int i = 0; i < m_needsLayoutGroup.childCount; i++)
        {
            Transform needTransform = m_needsLayoutGroup.GetChild(i);

            bool needPopulated = false;
            while(needIndex < needs.Count)
            {
                NeedData needData = m_needData.FirstOrDefault(x => x.Type == needs[needIndex].Type);
                if (needData == null) break;
                if (!needData.IsVisibleNeed) break;

                NeedUI need = needTransform.GetComponent<NeedUI>();
                if (need == null) break;
				Color needFillColor = changingNeeds.Exists(x => x == needData.Type) ? Color.white : Color.green;
				need.SetNeed(needData.Name, needs[needIndex].Value[0], needData.MinValue[0], needData.MaxValue[0], needFillColor);

                needIndex++;
                needPopulated = true;
                break;
            }

            needTransform.gameObject.SetActive(needPopulated);
        }

        /*
            * 
        foreach (Need need in needs)
        {
            NeedData needData = m_needData.FirstOrDefault(x => x.Type == need.Type);

            if (needData == null) return;
            if (!needData.IsVisibleNeed) return;

            text += string.Format("{0}: {1}\n", needData.Name, DisplayFloat3(need.Value));
        }*/

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
