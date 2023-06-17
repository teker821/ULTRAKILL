using UnityEngine;

public class CoinCollector : MonoBehaviour
{
	public GameObject coin;

	private void Start()
	{
		Invoke("Removal", 10f);
	}

	private void Removal()
	{
		Object.Destroy(coin);
		Object.Destroy(base.gameObject);
	}
}
