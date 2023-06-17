using UnityEngine;

public class PlatformerDancer : MonoBehaviour
{
	private AudioSource aud;

	public void Whoosh(float pitch)
	{
		if (!aud)
		{
			aud = GetComponent<AudioSource>();
		}
		aud.pitch = pitch + Random.Range(-0.05f, 0.05f);
		aud.Play();
	}

	public void DanceEnd()
	{
		MonoSingleton<PlatformerMovement>.Instance.transform.position = base.transform.position;
		MonoSingleton<PlatformerMovement>.Instance.gameObject.SetActive(value: true);
		base.gameObject.SetActive(value: false);
		MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>CLASH MODE</color> CHEAT UNLOCKED");
	}
}
