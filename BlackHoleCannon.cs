using UnityEngine;

public class BlackHoleCannon : MonoBehaviour
{
	public Transform shootPoint;

	public GameObject bh;

	private GameObject currentbh;

	public LayerMask lmask;

	private RaycastHit rhit;

	private GameObject cam;

	private CameraController cc;

	private AudioSource aud;

	private WeaponHUD whud;

	public AudioClip emptyClick;

	private void Start()
	{
		cam = Camera.main.gameObject;
		cc = cam.GetComponent<CameraController>();
		aud = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (!MonoSingleton<GunControl>.Instance.activated)
		{
			return;
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame)
		{
			if (!currentbh)
			{
				Shoot();
			}
			else
			{
				aud.PlayOneShot(emptyClick, 1f);
			}
		}
		if (currentbh != null && MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && currentbh.TryGetComponent<BlackHoleProjectile>(out var component))
		{
			component.Activate();
		}
	}

	private void Shoot()
	{
		Vector3 position = cam.transform.position + cam.transform.forward;
		currentbh = Object.Instantiate(bh, position, cam.transform.rotation);
		if (Physics.Raycast(cam.transform.position, cam.transform.forward, out rhit, float.PositiveInfinity, lmask))
		{
			currentbh.transform.LookAt(rhit.point);
		}
		else
		{
			currentbh.transform.rotation = cam.transform.rotation;
		}
		aud.Play();
		cc.CameraShake(0.5f);
	}
}
