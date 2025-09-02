using System;
using TMPro;
using UnityEngine;

public class GameTimeUI: MonoBehaviour
{
    [SerializeField] private TMP_Text m_text;
    [SerializeField] private int m_minsPerTimeStep = 5;

    public void UpdateTimeUI(int timeSteps)
    {
        TimeSpan timeSpan = TimeSpan.FromMinutes(timeSteps * m_minsPerTimeStep);
        string text = string.Format("DAYS: {0}\nHOURS: {1}\nMINS: {2}", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes);
        m_text.SetText(text);
    }

    public static GameTimeUI Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
}