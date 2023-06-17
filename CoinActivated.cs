using UnityEngine;

public class CoinActivated : MonoBehaviour
{
	public bool disableCoin;

	public UltrakillEvent events;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Coin"))
		{
			events.Invoke();
			if (disableCoin)
			{
				other.gameObject.SetActive(value: false);
			}
			GetComponent<Collider>().enabled = false;
		}
	}
}
