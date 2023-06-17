using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;

public class Magnet : MonoBehaviour
{
	private List<Rigidbody> affectedRbs = new List<Rigidbody>();

	private List<Rigidbody> removeRbs = new List<Rigidbody>();

	private List<EnemyIdentifier> eids = new List<EnemyIdentifier>();

	private List<Rigidbody> eidRbs = new List<Rigidbody>();

	public List<EnemyIdentifier> ignoredEids = new List<EnemyIdentifier>();

	public bool onEnemy;

	public List<Magnet> connectedMagnets = new List<Magnet>();

	public List<Rigidbody> sawblades = new List<Rigidbody>();

	public List<Rigidbody> rockets = new List<Rigidbody>();

	private SphereCollider col;

	public float strength;

	private LayerMask lmask;

	private RaycastHit rhit;

	[SerializeField]
	private float maxWeight = 10f;

	private TimeBomb tb;

	[HideInInspector]
	public float health = 3f;

	private float maxWeightFinal => maxWeight;

	private void Start()
	{
		col = GetComponent<SphereCollider>();
		lmask = (int)lmask | 0x400;
		lmask = (int)lmask | 0x800;
		tb = GetComponentInParent<TimeBomb>();
		col.enabled = false;
		col.enabled = true;
	}

	private void OnDestroy()
	{
		Launch();
		if (connectedMagnets.Count > 0)
		{
			for (int num = connectedMagnets.Count - 1; num >= 0; num--)
			{
				if (connectedMagnets[num] != null)
				{
					DisconnectMagnets(connectedMagnets[num]);
				}
			}
		}
		if ((bool)tb && tb.gameObject.activeInHierarchy)
		{
			Object.Destroy(tb.gameObject);
		}
	}

	public void Launch()
	{
		if (eids.Count > 0)
		{
			for (int num = eids.Count - 1; num >= 0; num--)
			{
				if ((bool)eids[num])
				{
					ExitEnemy(eids[num]);
				}
			}
		}
		if (affectedRbs.Count == 0 && sawblades.Count == 0)
		{
			return;
		}
		List<Nail> list = new List<Nail>();
		foreach (Rigidbody sawblade in sawblades)
		{
			if (!(sawblade != null))
			{
				continue;
			}
			sawblade.velocity = (base.transform.position - sawblade.transform.position).normalized * sawblade.velocity.magnitude;
			if (sawblade.TryGetComponent<Nail>(out var component))
			{
				component.MagnetRelease(this);
				if (component.magnets.Count == 0)
				{
					list.Add(component);
				}
			}
		}
		foreach (Rigidbody affectedRb in affectedRbs)
		{
			if (!(affectedRb != null))
			{
				continue;
			}
			affectedRb.velocity = Vector3.zero;
			if (Physics.SphereCast(new Ray(affectedRb.transform.position, affectedRb.transform.position - base.transform.position), 5f, out rhit, 100f, lmask))
			{
				affectedRb.AddForce((rhit.point - affectedRb.transform.position).normalized * strength * 10f);
			}
			else
			{
				affectedRb.AddForce((base.transform.position - affectedRb.transform.position).normalized * strength * -10f);
			}
			if (affectedRb.TryGetComponent<Nail>(out var component2))
			{
				component2.MagnetRelease(this);
				if (component2.magnets.Count == 0)
				{
					affectedRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
					list.Add(component2);
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		GameObject obj = new GameObject("NailBurstController");
		NailBurstController nailBurstController = obj.AddComponent<NailBurstController>();
		nailBurstController.nails = new List<Nail>(list);
		obj.AddComponent<RemoveOnTime>().time = 5f;
		foreach (Nail item in list)
		{
			item.nbc = nailBurstController;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Magnet component5;
		if (other.gameObject.layer == 14 && other.gameObject.tag == "Metal")
		{
			Rigidbody attachedRigidbody = other.attachedRigidbody;
			if (!(attachedRigidbody != null) || affectedRbs.Contains(attachedRigidbody))
			{
				return;
			}
			Grenade component2;
			if (attachedRigidbody.TryGetComponent<Nail>(out var component))
			{
				component.MagnetCaught(this);
				if (!component.sawblade)
				{
					affectedRbs.Add(attachedRigidbody);
					if (OptionsMenuToManager.simpleNailPhysics)
					{
						attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
					}
				}
				else if (!sawblades.Contains(attachedRigidbody))
				{
					sawblades.Add(attachedRigidbody);
				}
			}
			else if (attachedRigidbody.TryGetComponent<Grenade>(out component2))
			{
				if (onEnemy)
				{
					if (!component2.magnets.Contains(this))
					{
						component2.latestEnemyMagnet = this;
						component2.magnets.Add(this);
					}
					if (!rockets.Contains(attachedRigidbody))
					{
						rockets.Add(attachedRigidbody);
					}
				}
			}
			else
			{
				affectedRbs.Add(attachedRigidbody);
			}
		}
		else if (other.gameObject.layer == 12 || other.gameObject.layer == 11)
		{
			EnemyIdentifier component3 = other.gameObject.GetComponent<EnemyIdentifier>();
			if (component3 != null && !component3.bigEnemy && !eids.Contains(component3) && !ignoredEids.Contains(component3))
			{
				Rigidbody component4 = component3.GetComponent<Rigidbody>();
				if (component4 != null)
				{
					component4.mass /= 2f;
					eids.Add(component3);
					eidRbs.Add(component4);
				}
			}
		}
		else if (other.TryGetComponent<Magnet>(out component5) && component5 != this && !connectedMagnets.Contains(component5))
		{
			ConnectMagnets(component5);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == 14 && other.gameObject.tag == "Metal")
		{
			Rigidbody attachedRigidbody = other.attachedRigidbody;
			if (!(attachedRigidbody != null))
			{
				return;
			}
			if (affectedRbs.Contains(attachedRigidbody))
			{
				affectedRbs.Remove(attachedRigidbody);
				if (other.TryGetComponent<Nail>(out var component))
				{
					component.MagnetRelease(this);
					if (component.magnets.Count == 0)
					{
						attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
					}
				}
			}
			else if (sawblades.Contains(attachedRigidbody))
			{
				if (other.TryGetComponent<Nail>(out var component2))
				{
					component2.MagnetRelease(this);
				}
				sawblades.Remove(attachedRigidbody);
			}
			else if (rockets.Contains(attachedRigidbody))
			{
				if (other.TryGetComponent<Grenade>(out var component3) && component3.magnets.Contains(this))
				{
					component3.magnets.Remove(this);
				}
				rockets.Remove(attachedRigidbody);
			}
		}
		else if (other.gameObject.layer == 12)
		{
			EnemyIdentifier component4 = other.gameObject.GetComponent<EnemyIdentifier>();
			ExitEnemy(component4);
		}
	}

	public void ConnectMagnets(Magnet target)
	{
		if (!target.connectedMagnets.Contains(this))
		{
			target.connectedMagnets.Add(this);
		}
		if (!connectedMagnets.Contains(target))
		{
			connectedMagnets.Add(target);
		}
	}

	public void DisconnectMagnets(Magnet target)
	{
		if (target.connectedMagnets.Contains(this))
		{
			target.connectedMagnets.Remove(this);
		}
		if (connectedMagnets.Contains(target))
		{
			connectedMagnets.Remove(target);
		}
	}

	public void ExitEnemy(EnemyIdentifier eid)
	{
		if (eid != null && eids.Contains(eid))
		{
			int index = eids.IndexOf(eid);
			eids.RemoveAt(index);
			if (eidRbs[index] != null)
			{
				eidRbs[index].mass *= 2f;
			}
			eidRbs.RemoveAt(index);
		}
	}

	private void Update()
	{
		float num = 0f;
		if (affectedRbs.Count > 0)
		{
			foreach (Rigidbody affectedRb in affectedRbs)
			{
				if (affectedRb != null)
				{
					if (Mathf.Abs(Vector3.Dot(affectedRb.velocity, base.transform.position - affectedRb.transform.position)) < 1000f)
					{
						affectedRb.AddForce((base.transform.position - affectedRb.transform.position) * ((col.radius - Vector3.Distance(affectedRb.transform.position, base.transform.position)) / col.radius * strength * 50f * Time.deltaTime));
						num += affectedRb.mass;
					}
				}
				else
				{
					removeRbs.Add(affectedRb);
				}
			}
		}
		if (sawblades.Count > 0)
		{
			foreach (Rigidbody sawblade in sawblades)
			{
				if (sawblade != null)
				{
					num += sawblade.mass;
				}
				else
				{
					removeRbs.Add(sawblade);
				}
			}
		}
		if (removeRbs.Count > 0)
		{
			foreach (Rigidbody removeRb in removeRbs)
			{
				affectedRbs.Remove(removeRb);
			}
			removeRbs.Clear();
		}
		if (eids.Count > 0)
		{
			for (int i = 0; i < eids.Count; i++)
			{
				if (eids[i] != null && eidRbs[i] != null && !ignoredEids.Contains(eids[i]))
				{
					if (eids[i].nailsAmount > 0 && !eidRbs[i].isKinematic)
					{
						eids[i].useBrakes = false;
						eids[i].pulledByMagnet = true;
						eidRbs[i].AddForce((base.transform.position - eidRbs[i].transform.position).normalized * ((col.radius - Vector3.Distance(eidRbs[i].transform.position, base.transform.position)) / col.radius * strength * (float)eids[i].nailsAmount * 5f * Time.deltaTime));
						num += eidRbs[i].mass;
					}
				}
				else
				{
					eids.RemoveAt(i);
					eidRbs.RemoveAt(i);
				}
			}
		}
		float num2 = maxWeightFinal * (float)(connectedMagnets.Count + 1);
		if (num > num2 && !PauseTimedBombs.Paused)
		{
			Object.Destroy(tb.gameObject);
			return;
		}
		tb.beeperColor = Color.Lerp(Color.green, Color.red, num / num2);
		tb.beeperPitch = num / num2 / 2f + 0.25f;
		tb.beeperSizeMultiplier = num / num2 + 1f;
	}
}
