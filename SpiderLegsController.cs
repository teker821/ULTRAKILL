using UnityEngine;

public class SpiderLegsController : MonoBehaviour
{
	private GameObject spiderBody;

	private Vector3 bodyRotV;

	private Quaternion bodyRotQ;

	private void Start()
	{
		spiderBody = base.transform.parent.GetChild(0).gameObject;
	}

	private void Update()
	{
		base.transform.position = spiderBody.transform.position;
		bodyRotV = spiderBody.transform.rotation.eulerAngles;
		bodyRotQ.eulerAngles = new Vector3(0f, bodyRotV.y, 0f);
		base.transform.rotation = bodyRotQ;
	}
}
