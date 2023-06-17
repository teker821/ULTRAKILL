using UnityEngine;

public class ObjectActivatorStay : MonoBehaviour
{
	public bool oneTime;

	public bool skippable;

	public bool disableOnExit;

	private bool activated;

	public float delay;

	public GameObject[] toActivate;

	public GameObject[] toDisActivate;

	public bool forEnemies;

	private void OnTriggerEnter(Collider other)
	{
		if ((!forEnemies && !activated && other.gameObject.tag == "Player") || (forEnemies && !activated && other.gameObject.tag == "Enemy"))
		{
			if (oneTime)
			{
				activated = true;
			}
			Invoke("Activate", delay);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!oneTime && ((!forEnemies && !activated && other.gameObject.tag == "Player") || (forEnemies && !activated && other.gameObject.tag == "Enemy")) && ((toActivate.Length != 0 && !toActivate[0].activeSelf) || (toDisActivate.Length != 0 && toDisActivate[0].activeSelf)))
		{
			Activate();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!disableOnExit)
		{
			return;
		}
		GameObject[] array = toDisActivate;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
			}
		}
		array = toActivate;
		foreach (GameObject gameObject2 in array)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(value: false);
			}
		}
	}

	private void Activate()
	{
		GameObject[] array = toDisActivate;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
		}
		array = toActivate;
		foreach (GameObject gameObject2 in array)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(value: true);
			}
		}
	}

	private void OnDisable()
	{
		CancelInvoke("Activate");
	}
}
