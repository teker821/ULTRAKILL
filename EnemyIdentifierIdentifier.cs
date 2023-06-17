using UnityEngine;

public class EnemyIdentifierIdentifier : MonoBehaviour
{
	[HideInInspector]
	public EnemyIdentifier eid;

	private bool deactivated;

	private Vector3 startPos;

	private void Start()
	{
		if (!eid)
		{
			eid = GetComponentInParent<EnemyIdentifier>();
		}
		startPos = base.transform.position;
		SlowCheck();
	}

	private void SlowCheck()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Vector3 position = base.transform.position;
			if (base.transform.position.y > 0f)
			{
				position.y = startPos.y;
			}
			if (Vector3.Distance(base.transform.position, startPos) > 9999f || (Vector3.Distance(base.transform.position, startPos) > 999f && (eid == null || eid.dead)))
			{
				deactivated = true;
				base.gameObject.SetActive(value: false);
				base.transform.position = new Vector3(-100f, -100f, -100f);
				if (eid != null && !eid.dead)
				{
					eid.InstaKill();
				}
			}
		}
		if (!deactivated)
		{
			Invoke("SlowCheck", 3f);
		}
	}
}
