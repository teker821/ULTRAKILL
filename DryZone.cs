using System.Collections.Generic;
using UnityEngine;

public class DryZone : MonoBehaviour
{
	private List<Collider> cols = new List<Collider>();

	private void Awake()
	{
		if (!MonoSingleton<DryZoneController>.Instance.dryZones.Contains(this))
		{
			MonoSingleton<DryZoneController>.Instance.dryZones.Add(this);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.attachedRigidbody != null)
		{
			cols.Add(other);
			MonoSingleton<DryZoneController>.Instance.AddCollider(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (cols.Contains(other))
		{
			cols.Remove(other);
			MonoSingleton<DryZoneController>.Instance.RemoveCollider(other);
		}
	}

	private void OnDisable()
	{
		if (!base.gameObject.scene.isLoaded)
		{
			return;
		}
		foreach (Collider col in cols)
		{
			MonoSingleton<DryZoneController>.Instance.RemoveCollider(col);
		}
		if (MonoSingleton<DryZoneController>.Instance.dryZones.Contains(this))
		{
			MonoSingleton<DryZoneController>.Instance.dryZones.Remove(this);
		}
	}

	private void OnEnable()
	{
		foreach (Collider col in cols)
		{
			MonoSingleton<DryZoneController>.Instance.AddCollider(col);
		}
		if (!MonoSingleton<DryZoneController>.Instance.dryZones.Contains(this))
		{
			MonoSingleton<DryZoneController>.Instance.dryZones.Add(this);
		}
	}
}
