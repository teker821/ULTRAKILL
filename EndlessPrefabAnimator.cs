using UnityEngine;

public class EndlessPrefabAnimator : MonoBehaviour
{
	private Vector3 origPos;

	private bool moving;

	public bool reverse;

	public bool reverseOnly;

	private EndlessGrid eg;

	private CyberPooledPrefab pooledId;

	public void Start()
	{
		if (!pooledId)
		{
			pooledId = GetComponent<CyberPooledPrefab>();
		}
		origPos = base.transform.position;
		if (!reverseOnly)
		{
			base.transform.position = origPos - Vector3.up * 20f;
			moving = true;
		}
	}

	private void Update()
	{
		if (moving)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, origPos, Time.deltaTime * 2f + 5f * Vector3.Distance(base.transform.position, origPos) * Time.deltaTime);
			if (base.transform.position == origPos)
			{
				moving = false;
				eg = GetComponentInParent<EndlessGrid>();
				eg.OnePrefabDone();
			}
		}
		else
		{
			if (!reverse)
			{
				return;
			}
			base.transform.position = Vector3.MoveTowards(base.transform.position, origPos - Vector3.up * 20f, Time.deltaTime * 2f + 5f * Vector3.Distance(base.transform.position, origPos) * Time.deltaTime);
			if (base.transform.position == origPos - Vector3.up * 20f)
			{
				if ((bool)pooledId)
				{
					base.gameObject.SetActive(value: false);
				}
				else
				{
					Object.Destroy(base.gameObject);
				}
			}
		}
	}
}
