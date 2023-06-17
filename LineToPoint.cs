using UnityEngine;

public class LineToPoint : MonoBehaviour
{
	private LineRenderer lr;

	public Transform[] targets;

	private void Update()
	{
		if (lr == null)
		{
			lr = GetComponent<LineRenderer>();
			lr.useWorldSpace = true;
		}
		for (int i = 0; i < targets.Length; i++)
		{
			lr.SetPosition(i, targets[i].position);
		}
	}
}
