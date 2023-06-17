using UnityEngine;

internal class RotateToFaceFrustumTarget : MonoBehaviour
{
	[SerializeField]
	private CameraFrustumTargeter targeter;

	[SerializeField]
	private float maxDegreesDelta;

	private void Update()
	{
		Quaternion to = (base.transform.parent ? base.transform.parent.rotation : Quaternion.identity);
		if ((bool)targeter && targeter.isActiveAndEnabled && CameraFrustumTargeter.IsEnabled && (bool)targeter.CurrentTarget)
		{
			to = Quaternion.LookRotation(targeter.CurrentTarget.bounds.center - base.transform.position);
		}
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * maxDegreesDelta);
	}
}
