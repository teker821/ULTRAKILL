using UnityEngine;

public class FakeRoomActivator : MonoBehaviour
{
	public GameObject fake;

	private void OnEnable()
	{
		fake.SetActive(value: false);
	}

	private void OnDisable()
	{
		if ((bool)fake)
		{
			fake.SetActive(value: true);
		}
	}
}
