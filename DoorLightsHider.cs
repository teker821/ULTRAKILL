using UnityEngine;

public class DoorLightsHider : MonoBehaviour
{
	public GameObject[] sideA;

	public GameObject[] sideB;

	private Door parentDoor;

	private bool currentSideIsA;

	private bool overridePreviousSide = true;

	private void Start()
	{
		parentDoor = GetComponentInParent<Door>();
		SlowUpdate();
	}

	private void SlowUpdate()
	{
		GameObject[] array;
		if ((bool)parentDoor && parentDoor.open)
		{
			array = sideA;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			array = sideB;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			overridePreviousSide = true;
			Invoke("SlowUpdate", 0.025f);
			return;
		}
		if (Vector3.Distance(base.transform.position, MonoSingleton<PlayerTracker>.Instance.GetTarget().position) > 200f)
		{
			array = sideA;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			array = sideB;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			overridePreviousSide = true;
			Invoke("SlowUpdate", 0.5f);
			return;
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		array = sideA;
		foreach (GameObject gameObject in array)
		{
			zero += gameObject.transform.position;
		}
		array = sideB;
		foreach (GameObject gameObject2 in array)
		{
			zero2 += gameObject2.transform.position;
		}
		zero /= (float)sideA.Length;
		zero2 /= (float)sideB.Length;
		if (Vector3.Distance(zero, MonoSingleton<PlayerTracker>.Instance.GetTarget().position) <= Vector3.Distance(zero2, MonoSingleton<PlayerTracker>.Instance.GetTarget().position))
		{
			SetSide(targetSideIsA: true);
		}
		else
		{
			SetSide(targetSideIsA: false);
		}
		Invoke("SlowUpdate", 0.1f);
	}

	public void SetSide(bool targetSideIsA)
	{
		if (overridePreviousSide || currentSideIsA != targetSideIsA)
		{
			GameObject[] array = sideA;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(targetSideIsA);
			}
			array = sideB;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(!targetSideIsA);
			}
			overridePreviousSide = false;
			currentSideIsA = targetSideIsA;
		}
	}
}
