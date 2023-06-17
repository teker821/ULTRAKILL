using UnityEngine;

public class Skull : MonoBehaviour
{
	private Light lit;

	private float origRange;

	private float litTime;

	private AudioSource aud;

	private void Start()
	{
		lit = GetComponent<Light>();
		origRange = lit.range;
		aud = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (litTime > 0f)
		{
			litTime = Mathf.MoveTowards(litTime, 0f, Time.deltaTime);
		}
		else if (lit.range > origRange)
		{
			lit.range = Mathf.MoveTowards(lit.range, origRange, Time.deltaTime * 5f);
		}
	}

	public void PunchWith()
	{
		if (lit.range == origRange)
		{
			litTime = 1f;
			lit.range = origRange * 2.5f;
			aud.Play();
		}
	}

	public void HitWith(GameObject target)
	{
		Flammable component = target.gameObject.GetComponent<Flammable>();
		if (component != null)
		{
			component.Burn(4f);
		}
	}
}
