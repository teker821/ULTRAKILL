using UnityEngine;

public class LaunchPlayer : MonoBehaviour
{
	public Vector3 direction;

	public bool relative;

	public bool oneTime;

	private bool beenLaunched;

	public bool dontLaunchOnEnable;

	private bool colliderless;

	private void Awake()
	{
		colliderless = GetComponent<Collider>() == null && GetComponent<Rigidbody>() == null;
	}

	private void OnEnable()
	{
		if (colliderless && !dontLaunchOnEnable && (!oneTime || !beenLaunched))
		{
			Launch();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((!oneTime || !beenLaunched) && other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			Launch();
		}
	}

	public void Launch()
	{
		if (!beenLaunched)
		{
			beenLaunched = true;
		}
		else if (oneTime)
		{
			return;
		}
		if (relative)
		{
			MonoSingleton<NewMovement>.Instance.Launch(base.transform.rotation * direction * 1000f);
		}
		else
		{
			MonoSingleton<NewMovement>.Instance.Launch(direction * 1000f);
		}
	}
}
