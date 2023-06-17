using UnityEngine;

public class OutdoorsChecker : MonoBehaviour
{
	private bool inverse;

	public bool nonSolid = true;

	public bool oneTime;

	public GameObject[] targets;

	private BoxCollider boxCol;

	private void Start()
	{
		if (!MonoSingleton<OutdoorLightMaster>.Instance)
		{
			base.enabled = false;
			return;
		}
		if (MonoSingleton<OutdoorLightMaster>.Instance.inverse)
		{
			inverse = true;
		}
		if (targets.Length == 0)
		{
			targets = new GameObject[1];
			targets[0] = base.gameObject;
		}
		boxCol = GetComponent<BoxCollider>();
		SlowUpdate();
	}

	public void SlowUpdate()
	{
		if (!oneTime)
		{
			Invoke("SlowUpdate", 0.5f);
		}
		GameObject[] array;
		if (CheckIfPositionOutdoors(boxCol ? boxCol.bounds.center : base.transform.position))
		{
			array = targets;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject && gameObject.layer != 13)
				{
					gameObject.layer = (nonSolid ? 25 : 24);
				}
			}
			return;
		}
		array = targets;
		foreach (GameObject gameObject2 in array)
		{
			if ((bool)gameObject2 && gameObject2.layer != 13)
			{
				gameObject2.layer = (nonSolid ? 27 : 8);
			}
		}
	}

	public static bool CheckIfPositionOutdoors(Vector3 position)
	{
		if (!MonoSingleton<OutdoorLightMaster>.Instance)
		{
			return false;
		}
		Collider[] array = Physics.OverlapSphere(position, 0.1f, 262144, QueryTriggerInteraction.Collide);
		if (array != null && array.Length != 0)
		{
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (!(collider == null) && MonoSingleton<OutdoorLightMaster>.Instance.outdoorsZones.Contains(collider))
				{
					return !MonoSingleton<OutdoorLightMaster>.Instance.inverse;
				}
			}
		}
		return MonoSingleton<OutdoorLightMaster>.Instance.inverse;
	}
}
