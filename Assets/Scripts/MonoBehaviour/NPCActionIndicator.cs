using UnityEngine;

public class NPCActionIndicator: MonoBehaviour
{
	[SerializeField] private SpriteRenderer m_spriteRenderer;
	[SerializeField] private Sprite[] m_actionSprites;

	public Sprite GetIndicator(int index)
	{
		if (index < 0 || index > m_actionSprites.Length)
			return null;

		return m_actionSprites[index];
	}

	public static NPCActionIndicator Instance;
	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(this);
	}
}