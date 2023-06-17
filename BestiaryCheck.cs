using UnityEngine;

public class BestiaryCheck : MonoBehaviour
{
	public EnemyType enemy;

	public bool killRequired;

	public UltrakillEvent onEnemyUnlocked;

	private void Start()
	{
		int[] bestiary = GameProgressSaver.GetBestiary();
		if (bestiary.Length > (int)enemy && bestiary[(int)enemy] >= ((!killRequired) ? 1 : 2))
		{
			onEnemyUnlocked?.Invoke();
		}
	}
}
