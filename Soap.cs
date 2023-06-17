using UnityEngine;

public class Soap : MonoBehaviour
{
	private ItemIdentifier itid;

	private Rigidbody rb;

	private Vector3 velocityBeforeCollision;

	private void Start()
	{
		itid = GetComponent<ItemIdentifier>();
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if ((bool)rb)
		{
			velocityBeforeCollision = rb.velocity;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (itid.pickedUp || !(velocityBeforeCollision.magnitude > 15f))
		{
			return;
		}
		Breakable component2;
		Bleeder component3;
		if ((collision.gameObject.layer == 11 || collision.gameObject.layer == 10) && collision.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
		{
			if ((bool)component.eid)
			{
				component.eid.DeliverDamage(collision.gameObject, Vector3.zero, collision.GetContact(0).point, 999999f, tryForExplode: true);
			}
			rb.velocity = Vector3.zero;
		}
		else if (collision.gameObject.TryGetComponent<Breakable>(out component2))
		{
			component2.Break();
			rb.velocity = Vector3.zero;
		}
		else if (collision.gameObject.TryGetComponent<Bleeder>(out component3))
		{
			component3.GetHit(base.transform.position, GoreType.Head);
		}
	}

	public void HitWith(GameObject target)
	{
		if (target.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
		{
			component.eid.DeliverDamage(target, Vector3.zero, target.transform.position, 999999f, tryForExplode: true);
		}
	}
}
