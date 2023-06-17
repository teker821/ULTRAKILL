using UnityEngine;

public class LightningStrikeExplosive : MonoBehaviour
{
	public GameObject normal;

	public GameObject reflected;

	public bool safeForPlayer;

	public float damageMultiplier = 1f;

	public float enemyDamageMultiplier = 1f;

	private void Start()
	{
		GameObject gameObject = null;
		bool flag = false;
		if ((bool)MonoSingleton<CoinList>.Instance && MonoSingleton<CoinList>.Instance.revolverCoinsList.Count > 0)
		{
			for (int i = 0; i < MonoSingleton<CoinList>.Instance.revolverCoinsList.Count; i++)
			{
				if (MonoSingleton<CoinList>.Instance.revolverCoinsList[i].transform.position.y > MonoSingleton<PlayerTracker>.Instance.GetPlayer().position.y && Vector2.Distance(new Vector2(MonoSingleton<CoinList>.Instance.revolverCoinsList[i].transform.position.x, MonoSingleton<CoinList>.Instance.revolverCoinsList[i].transform.position.z), new Vector2(base.transform.position.x, base.transform.position.z)) < 2f)
				{
					flag = true;
					gameObject = Object.Instantiate(reflected, base.transform.position + Vector3.up * 100f, Quaternion.LookRotation(Vector3.down));
					if (damageMultiplier != 1f && gameObject.TryGetComponent<RevolverBeam>(out var component))
					{
						component.damage *= damageMultiplier;
					}
					break;
				}
			}
		}
		if (!flag)
		{
			gameObject = Object.Instantiate(normal, base.transform.position, Quaternion.identity);
			Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
			foreach (Explosion explosion in componentsInChildren)
			{
				if (damageMultiplier != 1f || enemyDamageMultiplier != 1f)
				{
					explosion.damage = Mathf.RoundToInt((float)explosion.damage * damageMultiplier);
					explosion.enemyDamageMultiplier *= enemyDamageMultiplier;
					explosion.maxSize *= damageMultiplier;
					explosion.speed *= damageMultiplier;
				}
				if (safeForPlayer)
				{
					explosion.canHit = AffectedSubjects.EnemiesOnly;
				}
			}
		}
		if ((bool)base.transform.parent)
		{
			gameObject.transform.SetParent(base.transform.parent, worldPositionStays: true);
		}
		Object.Destroy(base.gameObject);
	}
}
