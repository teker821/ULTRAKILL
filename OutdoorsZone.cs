using UnityEngine;

public class OutdoorsZone : MonoBehaviour
{
	private OutdoorLightMaster olm;

	private int hasRequested;

	private void Start()
	{
		Collider component2;
		if (TryGetComponent<Rigidbody>(out var component))
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if ((bool)collider.attachedRigidbody && collider.attachedRigidbody == component)
				{
					MonoSingleton<OutdoorLightMaster>.Instance.outdoorsZones.Add(collider);
				}
			}
		}
		else if (TryGetComponent<Collider>(out component2) && !MonoSingleton<OutdoorLightMaster>.Instance.outdoorsZones.Contains(component2))
		{
			MonoSingleton<OutdoorLightMaster>.Instance.outdoorsZones.Add(component2);
		}
	}

	private void OnDisable()
	{
		if (hasRequested > 0)
		{
			for (int num = hasRequested; num > 0; num--)
			{
				olm.RemoveRequest();
			}
			hasRequested = 0;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!olm)
		{
			olm = GetComponentInParent<OutdoorLightMaster>();
		}
		if ((bool)olm && other.gameObject.tag == "Player")
		{
			if (hasRequested == 0)
			{
				olm.AddRequest();
			}
			hasRequested++;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!olm)
		{
			olm = GetComponentInParent<OutdoorLightMaster>();
		}
		if ((bool)olm && other.gameObject.tag == "Player")
		{
			if (hasRequested == 1)
			{
				olm.RemoveRequest();
			}
			hasRequested--;
		}
	}
}
