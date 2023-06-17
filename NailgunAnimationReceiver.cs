using UnityEngine;

public class NailgunAnimationReceiver : MonoBehaviour
{
	private Nailgun ng;

	private void Start()
	{
		ng = GetComponentInParent<Nailgun>();
	}

	public void CanShoot()
	{
		ng.CanShoot();
	}

	public void SnapSound()
	{
		ng.SnapSound();
	}
}
