using UnityEngine;

public class StreetcleanerAnimations : MonoBehaviour
{
	private Streetcleaner sc;

	private AudioSource aud;

	private void Start()
	{
		sc = GetComponentInParent<Streetcleaner>();
		aud = GetComponent<AudioSource>();
	}

	public void SlapOver()
	{
		sc.SlapOver();
	}

	public void OverrideOver()
	{
		sc.OverrideOver();
	}

	public void DodgeEnd()
	{
		sc.DodgeEnd();
	}

	public void StopMoving()
	{
		sc.StopMoving();
	}

	public void Step()
	{
		aud.pitch = Random.Range(0.85f, 1.15f);
		aud.Play();
	}
}
