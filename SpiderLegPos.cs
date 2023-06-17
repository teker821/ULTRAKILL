using UnityEngine;

public class SpiderLegPos : MonoBehaviour
{
	public GameObject childLeg;

	public SpiderLeg sl;

	private bool movingLeg;

	private RaycastHit hit;

	public bool backLeg;

	private void Start()
	{
		MoveLeg();
	}

	private void Update()
	{
		if (movingLeg)
		{
			childLeg.transform.position = Vector3.MoveTowards(childLeg.transform.position, base.transform.position, Time.deltaTime * (20f * Vector3.Distance(base.transform.position, childLeg.transform.position) + 0.1f));
			if (childLeg.transform.position == base.transform.position)
			{
				movingLeg = false;
			}
		}
		else if (Vector3.Distance(base.transform.position, childLeg.transform.position) > 3f)
		{
			MoveLeg();
		}
	}

	private void MoveLeg()
	{
		bool flag = false;
		if (!backLeg)
		{
			if (Physics.Raycast(base.transform.position, base.transform.up * -1f + (base.transform.forward + base.transform.right * -1f) * Random.Range(-1f, 2f), out hit, 35f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				flag = true;
			}
		}
		else if (Physics.Raycast(base.transform.position, base.transform.up * -1f + (base.transform.forward + base.transform.right) * -1f * Random.Range(-1f, 2f), out hit, 35f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			flag = true;
		}
		if (flag && hit.transform != null)
		{
			sl.target = hit.point;
			movingLeg = true;
		}
	}
}
