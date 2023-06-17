using UnityEngine;

public class ShotgunAnimationReceiver : MonoBehaviour
{
	private Shotgun sgun;

	private void Start()
	{
		sgun = GetComponentInParent<Shotgun>();
	}

	public void ReleaseHeat()
	{
		sgun.ReleaseHeat();
	}

	public void ClickSound()
	{
		sgun.ClickSound();
	}

	public void ReadyGun()
	{
		sgun.ReadyGun();
	}

	public void Smack()
	{
		sgun.Smack();
	}

	public void Pump1Sound()
	{
		sgun.Pump1Sound();
	}

	public void Pump2Sound()
	{
		sgun.Pump2Sound();
	}
}
