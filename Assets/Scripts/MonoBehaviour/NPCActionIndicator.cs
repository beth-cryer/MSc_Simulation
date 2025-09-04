using UnityEngine;

public class NPCActionIndicator: MonoBehaviour
{
	[SerializeField] private SpriteRenderer m_spriteRenderer;
	[SerializeField] private Sprite[] m_actionSprites;

	public void SetIndicator(int index)
	{
		if (index < 0 || index > m_actionSprites.Length)
			return;

		m_spriteRenderer.enabled = index > 0;
		m_spriteRenderer.sprite = m_actionSprites[index];
	}
}