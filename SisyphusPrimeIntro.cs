using UnityEngine;

public class SisyphusPrimeIntro : MonoBehaviour
{
	public GameObject groundImpactEffect;

	public UltrakillEvent onGroundImpact;

	private bool hasHitGround;

	private Rigidbody rb;

	private TimeSince ts;

	private bool tracking;

	private void Start()
	{
		ts = 0f;
		Vector3 position = MonoSingleton<PlayerTracker>.Instance.GetPlayer().position;
		position.y = base.transform.position.y;
		base.transform.rotation = Quaternion.LookRotation(position - base.transform.position);
	}

	private void Update()
	{
		if (tracking)
		{
			Vector3 position = MonoSingleton<PlayerTracker>.Instance.GetPlayer().position;
			position.y = base.transform.position.y;
			Quaternion quaternion = Quaternion.LookRotation(position - base.transform.position);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * 10f * Quaternion.Angle(base.transform.rotation, quaternion));
		}
	}

	private void FixedUpdate()
	{
		if (!hasHitGround)
		{
			if (!rb)
			{
				rb = GetComponent<Rigidbody>();
			}
			rb.velocity -= Vector3.up * Mathf.Lerp(0f, 100f, ts) * Time.fixedDeltaTime;
		}
	}

	private void OnCollisionEnter(Collision col)
	{
		if (!hasHitGround && (col.gameObject.layer == 8 || col.gameObject.layer == 24))
		{
			hasHitGround = true;
			GetComponent<Animator>().Play("Intro");
			Object.Instantiate(groundImpactEffect, base.transform.position, Quaternion.identity);
			onGroundImpact?.Invoke();
			rb.isKinematic = true;
			base.gameObject.layer = 16;
			tracking = true;
		}
	}
}
