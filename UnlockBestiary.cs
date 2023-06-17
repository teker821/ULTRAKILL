using UnityEngine;

public class UnlockBestiary : MonoBehaviour
{
	public EnemyType enemy;

	public bool fullUnlock;

	private void Start()
	{
		MonoSingleton<BestiaryData>.Instance.SetEnemy(enemy, (!fullUnlock) ? 1 : 2);
	}
}
