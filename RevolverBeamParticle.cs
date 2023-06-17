using UnityEngine;

public class RevolverBeamParticle : MonoBehaviour
{
	public int type;

	private Revolver rev;

	private SecondaryRevolver secRev;

	private void Awake()
	{
		if (type == 0)
		{
			rev = MonoSingleton<CameraController>.Instance.GetComponentInChildren<Revolver>();
		}
		else if (type == 1)
		{
			secRev = MonoSingleton<CameraController>.Instance.GetComponentInChildren<SecondaryRevolver>();
		}
		if (rev != null)
		{
			base.transform.forward = rev.hit.normal;
		}
		else if (secRev != null)
		{
			base.transform.forward = secRev.hit.normal;
		}
		Invoke("Destroy", 2f);
	}

	private void Destroy()
	{
		Object.Destroy(base.gameObject);
	}
}
