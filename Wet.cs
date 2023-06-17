using UnityEngine;

public class Wet : MonoBehaviour
{
	private GameObject currentWetEffect;

	public float wetness;

	private bool drying;

	private void Start()
	{
		wetness = 5f;
	}

	private void Update()
	{
		if (!drying)
		{
			return;
		}
		if (wetness > 0f)
		{
			wetness = Mathf.MoveTowards(wetness, 0f, Time.deltaTime);
			return;
		}
		Flammable[] componentsInChildren = GetComponentsInChildren<Flammable>();
		if (componentsInChildren != null)
		{
			Flammable[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].wet = false;
			}
		}
		Object.Destroy(this);
	}

	public void Dry()
	{
		drying = true;
		if (MonoSingleton<DefaultReferenceManager>.Instance != null)
		{
			currentWetEffect = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.wetParticle, base.transform);
		}
	}

	public void Refill()
	{
		drying = false;
		wetness = 5f;
		if ((bool)currentWetEffect)
		{
			Object.Destroy(currentWetEffect);
		}
	}

	private void OnDestroy()
	{
		if ((bool)currentWetEffect)
		{
			Object.Destroy(currentWetEffect);
		}
	}
}
