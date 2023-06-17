using UnityEngine;

public class CopyLineRenderer : MonoBehaviour
{
	private LineRenderer toCopy;

	private LineRenderer lr;

	private float origWidth;

	private void Start()
	{
		lr = GetComponent<LineRenderer>();
		toCopy = base.transform.parent.GetComponentInParent<LineRenderer>();
		origWidth = lr.widthMultiplier;
	}

	private void Update()
	{
		for (int i = 0; i < toCopy.positionCount; i++)
		{
			lr.SetPosition(i, toCopy.GetPosition(i));
		}
		lr.widthMultiplier = toCopy.widthMultiplier * origWidth;
	}
}
