using UnityEngine;

public class Screenshaker : MonoBehaviour
{
	public float amount;

	public bool oneTime;

	public bool continuous;

	private bool alreadyShaken;

	private bool colliderless;

	private void Awake()
	{
		colliderless = GetComponent<Collider>() == null && GetComponent<Rigidbody>() == null;
	}

	private void OnEnable()
	{
		if (colliderless)
		{
			Shake();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			Shake();
		}
	}

	private void Update()
	{
		if (continuous && base.gameObject.activeInHierarchy)
		{
			MonoSingleton<CameraController>.Instance.CameraShake(amount);
		}
	}

	public void Shake()
	{
		if (!oneTime || !alreadyShaken)
		{
			alreadyShaken = true;
			MonoSingleton<CameraController>.Instance.CameraShake(amount);
			if (oneTime && !continuous)
			{
				Object.Destroy(this);
			}
		}
	}
}
