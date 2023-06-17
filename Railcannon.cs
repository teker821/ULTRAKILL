using ULTRAKILL.Cheats;
using UnityEngine;

public class Railcannon : MonoBehaviour
{
	public int variation;

	public GameObject beam;

	public Transform shootPoint;

	public GameObject fullCharge;

	public GameObject fireSound;

	private AudioSource fullAud;

	private bool pitchRise;

	private InputManager inman;

	public WeaponIdentifier wid;

	private float gotWidDelay;

	private AudioSource aud;

	private CameraController cc;

	private Camera cam;

	private GunControl gc;

	private Animator anim;

	private SkinnedMeshRenderer smr;

	private WeaponCharges wc;

	private WeaponPos wpos;

	private bool zooming;

	private bool gotStuff;

	private CameraFrustumTargeter targeter;

	private float altCharge;

	[SerializeField]
	private Light fullChargeLight;

	[SerializeField]
	private ParticleSystem fullChargeParticles;

	private void Awake()
	{
		if (!gotStuff)
		{
			wid = GetComponent<WeaponIdentifier>();
		}
	}

	private void Start()
	{
		if (!gotStuff)
		{
			gotStuff = true;
			GetStuff();
		}
	}

	private void OnEnable()
	{
		if (!gotStuff)
		{
			gotStuff = true;
			GetStuff();
		}
		if (wc.raicharge != 5f)
		{
			fullCharge.SetActive(value: false);
			base.transform.localPosition = wpos.currentDefault;
		}
		else if (variation == 2)
		{
			if (fullAud == null)
			{
				fullAud = fullCharge.GetComponent<AudioSource>();
			}
			pitchRise = true;
			fullAud.pitch = 0f;
		}
	}

	private void OnDisable()
	{
		if (wc == null)
		{
			wc = GetComponentInParent<WeaponCharges>();
		}
		if (wpos != null)
		{
			base.transform.localPosition = wpos.currentDefault;
		}
		if (zooming)
		{
			zooming = false;
			cc.StopZoom();
		}
	}

	private void Update()
	{
		if (wid.delay > 0f && altCharge < wc.raicharge)
		{
			altCharge = wc.raicharge;
		}
		float raicharge = wc.raicharge;
		if (wid.delay > 0f)
		{
			raicharge = altCharge;
		}
		if (raicharge < 5f && !NoWeaponCooldown.NoCooldown)
		{
			smr.material.SetFloat("_EmissiveIntensity", raicharge / 5f);
		}
		else
		{
			MonoSingleton<RumbleManager>.Instance.SetVibrationTracked("rumble.gun.railcannon_idle", fullCharge);
			if (!fullCharge.activeSelf)
			{
				fullCharge.SetActive(value: true);
				if (variation == 2)
				{
					pitchRise = true;
					fullAud.pitch = 0f;
				}
			}
			if (!wc.railChargePlayed)
			{
				wc.PlayRailCharge();
			}
			base.transform.localPosition = new Vector3(wpos.currentDefault.x + Random.Range(-0.005f, 0.005f), wpos.currentDefault.y + Random.Range(-0.005f, 0.005f), wpos.currentDefault.z + Random.Range(-0.005f, 0.005f));
			smr.material.SetFloat("_EmissiveIntensity", 1f);
			Color color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation];
			fullChargeLight.color = color;
			ParticleSystem.MainModule main = fullChargeParticles.main;
			main.startColor = color;
			if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
			{
				fullCharge.SetActive(value: false);
				base.transform.localPosition = wpos.currentDefault;
				wc.raicharge = 0f;
				wc.railChargePlayed = false;
				altCharge = 0f;
				if (!wid || wid.delay == 0f)
				{
					Shoot();
				}
				else
				{
					Invoke("Shoot", wid.delay);
				}
			}
		}
		if (!wid || wid.delay == 0f)
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
			{
				zooming = true;
				cc.Zoom(cc.defaultFov / 2f);
			}
			else if (zooming)
			{
				zooming = false;
				cc.StopZoom();
			}
		}
		if (wid.delay != gotWidDelay)
		{
			gotWidDelay = wid.delay;
			if ((bool)wid && wid.delay != 0f)
			{
				fullAud.volume -= wid.delay * 2f;
				if (fullAud.volume < 0f)
				{
					fullAud.volume = 0f;
				}
			}
		}
		if (pitchRise)
		{
			fullAud.pitch = Mathf.MoveTowards(fullAud.pitch, 2f, Time.deltaTime * 4f);
			if (fullAud.pitch == 2f)
			{
				pitchRise = false;
			}
		}
		smr.material.SetFloat("_EmissivePosition", wc.raicharge);
		smr.material.SetColor("_EmissiveColor", MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation]);
	}

	private void Shoot()
	{
		GameObject gameObject = Object.Instantiate(beam, cc.GetDefaultPos(), cc.transform.rotation);
		if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
		{
			gameObject.transform.LookAt(targeter.CurrentTarget.bounds.center);
		}
		if (variation != 1)
		{
			if (gameObject.TryGetComponent<RevolverBeam>(out var component))
			{
				component.sourceWeapon = gc.currentWeapon;
				component.alternateStartPoint = shootPoint.position;
			}
		}
		else
		{
			gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 250f, ForceMode.VelocityChange);
		}
		Object.Instantiate(fireSound);
		anim.SetTrigger("Shoot");
		cc.CameraShake(2f);
		MonoSingleton<RumbleManager>.Instance.SetVibration("rumble.gun.fire_strong");
	}

	private void GetStuff()
	{
		targeter = Camera.main.GetComponent<CameraFrustumTargeter>();
		inman = MonoSingleton<InputManager>.Instance;
		wid = GetComponent<WeaponIdentifier>();
		aud = GetComponent<AudioSource>();
		cc = MonoSingleton<CameraController>.Instance;
		cam = cc.GetComponent<Camera>();
		smr = GetComponentInChildren<SkinnedMeshRenderer>();
		gc = GetComponentInParent<GunControl>();
		anim = GetComponentInChildren<Animator>();
		wpos = GetComponent<WeaponPos>();
		fullAud = fullCharge.GetComponent<AudioSource>();
		wc = MonoSingleton<WeaponCharges>.Instance;
	}
}
