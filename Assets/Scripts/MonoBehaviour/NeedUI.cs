using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeedUI: MonoBehaviour
{
    [SerializeField] private Slider m_slider;
    [SerializeField] private TMP_Text m_text;

    public void SetNeed(string name, float value, float min, float max)
    {
        m_text.SetText(name);
        m_slider.value = (value - min) / (max - min);
    }
}