using UnityEngine;

public class SpiderLegLines : MonoBehaviour
{
	private GameObject body;

	public GameObject legEnd;

	private LineRenderer lr;

	private void Start()
	{
		body = base.transform.parent.GetChild(0).gameObject;
		lr = GetComponent<LineRenderer>();
	}

	private void Update()
	{
		lr.SetPosition(0, body.transform.position);
		lr.SetPosition(1, base.transform.position);
		lr.SetPosition(2, legEnd.transform.position);
	}
}
