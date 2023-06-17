using ULTRAKILL.Cheats;
using UnityEngine;

public class ObjectActivator : MonoBehaviour
{
	public bool oneTime;

	public bool disableOnExit;

	public bool dontActivateOnEnable;

	public bool reactivateOnEnable;

	public bool forEnemies;

	public bool notIfEnemiesDisabled;

	public bool onlyIfPlayerIsAlive;

	[HideInInspector]
	public bool activated;

	public float delay;

	private bool nonCollider;

	private int playerIn;

	[Space(20f)]
	public ObjectActivationCheck obac;

	public bool onlyCheckObacOnce;

	[Space(10f)]
	public UltrakillEvent events;

	private void Start()
	{
		if (!dontActivateOnEnable && GetComponent<Collider>() == null && GetComponent<Rigidbody>() == null)
		{
			nonCollider = true;
			if ((!obac || obac.readyToActivate) && (!onlyIfPlayerIsAlive || !MonoSingleton<NewMovement>.Instance.dead) && (!oneTime || !activated))
			{
				Invoke("Activate", delay);
			}
		}
	}

	private void Update()
	{
		if ((nonCollider || playerIn > 0) && (!oneTime || !activated) && (bool)obac && obac.readyToActivate && !onlyCheckObacOnce && (!onlyIfPlayerIsAlive || !MonoSingleton<NewMovement>.Instance.dead))
		{
			activated = true;
			Invoke("Activate", delay);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((forEnemies && other.gameObject.CompareTag("Enemy")) || (!forEnemies && other.gameObject.CompareTag("Player")))
		{
			playerIn++;
		}
		if (((!forEnemies && (!oneTime || !activated) && other.gameObject.CompareTag("Player")) || (forEnemies && !activated && other.gameObject.CompareTag("Enemy"))) && playerIn == 1 && (!obac || obac.readyToActivate) && (!onlyIfPlayerIsAlive || !MonoSingleton<NewMovement>.Instance.dead))
		{
			if (oneTime)
			{
				activated = true;
			}
			Invoke("Activate", delay);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((forEnemies && other.gameObject.CompareTag("Enemy")) || (!forEnemies && other.gameObject.CompareTag("Player")))
		{
			playerIn--;
		}
		if (disableOnExit && ((!forEnemies && activated && other.gameObject.CompareTag("Player") && playerIn == 0) || (forEnemies && activated && other.gameObject.CompareTag("Enemy"))) && (!onlyIfPlayerIsAlive || !MonoSingleton<NewMovement>.Instance.dead))
		{
			Deactivate();
		}
	}

	public void Activate()
	{
		if (base.gameObject.activeSelf && (!onlyIfPlayerIsAlive || !MonoSingleton<NewMovement>.Instance.dead) && (!notIfEnemiesDisabled || !DisableEnemySpawns.DisableArenaTriggers))
		{
			activated = true;
			events.Invoke();
		}
	}

	public void Deactivate()
	{
		if (!oneTime)
		{
			activated = false;
		}
		events.Revert();
		CancelInvoke("Activate");
	}

	private void OnDisable()
	{
		if (activated && nonCollider && disableOnExit && (!onlyIfPlayerIsAlive || !MonoSingleton<NewMovement>.Instance.dead))
		{
			Deactivate();
		}
		playerIn = 0;
		CancelInvoke("Activate");
	}

	private void OnEnable()
	{
		if ((!activated || reactivateOnEnable) && nonCollider && (!obac || obac.readyToActivate) && (!onlyIfPlayerIsAlive || !MonoSingleton<NewMovement>.Instance.dead))
		{
			Invoke("Activate", delay);
		}
	}
}
