using UnityEngine;

public class WeaponTrail : MonoBehaviour
{
	private GameObject trailTemplate;

	private GameObject currentTrail;

	private void Awake()
	{
		if (!trailTemplate)
		{
			trailTemplate = base.transform.GetChild(0).gameObject;
			trailTemplate.SetActive(value: false);
		}
	}

	public void AddTrail()
	{
		if (!trailTemplate)
		{
			trailTemplate = base.transform.GetChild(0).gameObject;
			trailTemplate.SetActive(value: false);
		}
		if (!currentTrail)
		{
			currentTrail = Object.Instantiate(trailTemplate, base.transform);
			currentTrail.SetActive(value: true);
		}
	}

	public void RemoveTrail()
	{
		if ((bool)currentTrail)
		{
			currentTrail.AddComponent<RemoveOnTime>().time = 5f;
			currentTrail.transform.parent = null;
			currentTrail = null;
		}
	}
}
