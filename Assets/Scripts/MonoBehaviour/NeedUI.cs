using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeedUI: MonoBehaviour
{
    [SerializeField] private Slider m_slider;
    [SerializeField] private TMP_Text m_text;
	[SerializeField] private Image m_fillImage; // we care about the colour of this

    public void SetNeed(string name, float value, float min, float max, Color color)
    {
        m_text.SetText(name);
        m_slider.value = (value - min) / (max - min);
		m_fillImage.color = color;
    }
}