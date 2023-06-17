using UnityEngine;

public class SplashingElement : MonoBehaviour
{
	public SplashingElement previousElement;

	private bool _isSplashing;

	private Vector3 _splashPosition;

	public bool isSplashing => _isSplashing;

	public Vector3 splashPosition => _splashPosition;

	public void FixedUpdate()
	{
		if ((bool)previousElement)
		{
			Vector3 position = base.transform.position;
			Vector3 position2 = previousElement.transform.position;
			Ray ray = new Ray(position, position2 - position);
			Ray ray2 = new Ray(position2, position - position2);
			float maxDistance = Vector3.Distance(position2, base.transform.position);
			RaycastHit hitInfo;
			bool flag = Physics.Raycast(ray, out hitInfo, maxDistance, LayerMask.GetMask("Water"));
			if (!flag)
			{
				flag = Physics.Raycast(ray2, out hitInfo, maxDistance, LayerMask.GetMask("Water"));
			}
			_isSplashing = flag;
			if (flag)
			{
				_splashPosition = hitInfo.point;
			}
		}
	}
}
