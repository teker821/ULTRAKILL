using UnityEngine;

public class FerrymanFake : MonoBehaviour
{
	private bool activated;

	private bool trackPlayer;

	private bool jumping;

	private Quaternion originalRotation;

	public UltrakillEvent onCoinBlow;

	public GameObject realFerryman;

	private Rigidbody rb;

	public void CoinCatch()
	{
		originalRotation = base.transform.rotation;
		GetComponent<Animator>().SetTrigger("CatchCoin");
		trackPlayer = true;
		activated = true;
	}

	private void Update()
	{
		if (trackPlayer)
		{
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(new Vector3(MonoSingleton<NewMovement>.Instance.transform.position.x, base.transform.position.y, MonoSingleton<NewMovement>.Instance.transform.position.z) - base.transform.position), Time.deltaTime * 1200f);
		}
		else if (activated)
		{
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, originalRotation, Time.deltaTime * 10f * Mathf.Max(Mathf.Min(Quaternion.Angle(base.transform.rotation, originalRotation), 20f), 0.01f));
		}
		if (jumping)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, new Vector3(realFerryman.transform.position.x, base.transform.position.y, realFerryman.transform.position.z), Time.deltaTime * 10f);
			if (base.transform.position.y < realFerryman.transform.position.y + 1f)
			{
				OnLand();
			}
		}
	}

	private void FixedUpdate()
	{
		if (jumping)
		{
			rb.AddForce(Vector3.down * 9.81f * Time.fixedDeltaTime, ForceMode.VelocityChange);
		}
	}

	public void ReturnToRotation()
	{
		trackPlayer = false;
	}

	public void BlowCoin()
	{
		onCoinBlow.Invoke();
	}

	public void StartFight()
	{
		if (TryGetComponent<Animator>(out var component))
		{
			component.SetTrigger("Jump");
			GetComponent<Collider>().enabled = false;
			rb = GetComponent<Rigidbody>();
			if ((bool)rb)
			{
				rb.isKinematic = false;
				rb.useGravity = true;
				rb.AddForce(base.transform.up * 25f, ForceMode.VelocityChange);
			}
			jumping = true;
		}
	}

	public void OnLand()
	{
		realFerryman.SetActive(value: true);
		Object.Destroy(base.gameObject);
	}
}
