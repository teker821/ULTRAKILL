using UnityEngine;

public class PlayerActivator : MonoBehaviour
{
	private NewMovement nm;

	private bool activated;

	[SerializeField]
	private bool startTimer;

	[SerializeField]
	private bool onlyActivatePlayer;

	private GunControl gc;

	public static Vector3 lastActivatedPosition;

	private void OnTriggerEnter(Collider other)
	{
		if (activated || !other.gameObject.CompareTag("Player"))
		{
			return;
		}
		nm = MonoSingleton<NewMovement>.Instance;
		gc = MonoSingleton<GunControl>.Instance;
		GameStateManager.Instance.PopState("pit-falling");
		if (!nm.activated)
		{
			nm.activated = true;
			nm.cc.activated = true;
			nm.cc.CameraShake(1f);
			AudioSource component = GetComponent<AudioSource>();
			if ((bool)component)
			{
				component.Play();
			}
		}
		activated = true;
		if (!onlyActivatePlayer)
		{
			gc.YesWeapon();
			ActivateObjects();
		}
		if (startTimer)
		{
			MonoSingleton<StatsManager>.Instance.StartTimer();
		}
		MonoSingleton<FistControl>.Instance.YesFist();
	}

	private void ActivateObjects()
	{
		MonoSingleton<PlayerActivatorRelay>.Instance.Activate();
		lastActivatedPosition = MonoSingleton<NewMovement>.Instance.transform.position;
	}
}
