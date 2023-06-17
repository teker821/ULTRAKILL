using UnityEngine;
using UnityEngine.UI;

public class RocketLauncher : MonoBehaviour
{
	public int variation;

	public GameObject rocket;

	public GameObject clunkSound;

	public float rateOfFire;

	private float cooldown = 0.25f;

	private bool lookingForValue;

	private AudioSource aud;

	private Animator anim;

	private WeaponIdentifier wid;

	public Transform shootPoint;

	public GameObject muzzleFlash;

	[SerializeField]
	private Image timerMeter;

	[SerializeField]
	private RectTransform timerArm;

	[SerializeField]
	private Image[] variationColorables;

	private float[] colorablesTransparencies;

	[Header("Freeze variation")]
	[SerializeField]
	private AudioSource timerFreezeSound;

	[SerializeField]
	private AudioSource timerUnfreezeSound;

	[SerializeField]
	private AudioSource timerTickSound;

	[HideInInspector]
	public AudioSource currentTimerTickSound;

	[SerializeField]
	private AudioSource timerWindupSound;

	private float lastKnownTimerAmount;

	[Header("Cannonball variation")]
	public Rigidbody cannonBall;

	[SerializeField]
	private AudioSource chargeSound;

	private float cbCharge;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		wid = GetComponent<WeaponIdentifier>();
		anim = GetComponent<Animator>();
		colorablesTransparencies = new float[variationColorables.Length];
		for (int i = 0; i < variationColorables.Length; i++)
		{
			colorablesTransparencies[i] = variationColorables[i].color.a;
		}
		if (variation == 0 && (!wid || wid.delay == 0f))
		{
			MonoSingleton<WeaponCharges>.Instance.rocketLauncher = this;
		}
	}

	private void OnEnable()
	{
		if (MonoSingleton<WeaponCharges>.Instance.rocketset)
		{
			MonoSingleton<WeaponCharges>.Instance.rocketset = false;
			if (MonoSingleton<WeaponCharges>.Instance.rocketcharge < 0.25f)
			{
				cooldown = 0.25f;
			}
			else
			{
				cooldown = MonoSingleton<WeaponCharges>.Instance.rocketcharge;
			}
		}
		else
		{
			lookingForValue = true;
		}
	}

	private void OnDisable()
	{
		MonoSingleton<WeaponCharges>.Instance.rocketcharge = cooldown;
		MonoSingleton<WeaponCharges>.Instance.rocketset = true;
		cbCharge = 0f;
	}

	private void OnDestroy()
	{
		if ((bool)currentTimerTickSound)
		{
			Object.Destroy(currentTimerTickSound);
		}
	}

	private void Update()
	{
		if (!MonoSingleton<ColorBlindSettings>.Instance)
		{
			return;
		}
		Color color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation];
		float num = 1f;
		if (MonoSingleton<WeaponCharges>.Instance.rocketset && lookingForValue)
		{
			MonoSingleton<WeaponCharges>.Instance.rocketset = false;
			lookingForValue = false;
			if (MonoSingleton<WeaponCharges>.Instance.rocketcharge < 0.25f)
			{
				cooldown = 0.25f;
			}
			else
			{
				cooldown = MonoSingleton<WeaponCharges>.Instance.rocketcharge;
			}
		}
		if (variation == 1)
		{
			if (MonoSingleton<GunControl>.Instance.activated && !GameStateManager.Instance.PlayerInputLocked)
			{
				if ((bool)timerArm)
				{
					timerArm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(360f, 0f, MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge));
				}
				if ((bool)timerMeter)
				{
					timerMeter.fillAmount = MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge;
				}
				if (lastKnownTimerAmount != MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge && (!wid || wid.delay == 0f))
				{
					for (float num2 = 4f; num2 > 0f; num2 -= 1f)
					{
						if (MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge >= num2 / 4f && lastKnownTimerAmount < num2 / 4f)
						{
							AudioSource audioSource = Object.Instantiate(timerWindupSound);
							audioSource.pitch = 1.6f + num2 * 0.1f;
							if (MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge < 1f)
							{
								audioSource.volume /= 2f;
							}
							break;
						}
					}
					lastKnownTimerAmount = MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge;
				}
				if (cooldown <= 0f)
				{
					if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && !MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge >= 1f)
					{
						if (!wid || wid.delay == 0f)
						{
							if (!chargeSound.isPlaying)
							{
								chargeSound.Play();
							}
							chargeSound.pitch = cbCharge + 0.5f;
						}
						cbCharge = Mathf.MoveTowards(cbCharge, 1f, Time.deltaTime);
						if ((bool)timerArm)
						{
							timerArm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(360f, 0f, cbCharge));
						}
						if ((bool)timerMeter)
						{
							timerMeter.fillAmount = cbCharge;
						}
					}
					else if (cbCharge > 0f)
					{
						chargeSound.Stop();
						if (!wid || wid.delay == 0f)
						{
							ShootCannonball();
						}
						else
						{
							Invoke("ShootCannonball", wid.delay);
							cooldown = 1f;
						}
						MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge = 0f;
					}
				}
			}
			if (cbCharge > 0f)
			{
				color = Color.Lerp(MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation], Color.red, cbCharge);
			}
			else if (MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge < 1f)
			{
				num = 0.5f;
			}
		}
		else if (variation == 0)
		{
			if (MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime > 0f && MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && !GameStateManager.Instance.PlayerInputLocked && (!wid || !wid.duplicate))
			{
				if (MonoSingleton<WeaponCharges>.Instance.rocketFrozen)
				{
					UnfreezeRockets();
				}
				else
				{
					FreezeRockets();
				}
			}
			if ((bool)timerArm)
			{
				timerArm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(360f, 0f, MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime / 5f));
			}
			if ((bool)timerMeter)
			{
				timerMeter.fillAmount = MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime / 5f;
			}
			if (lastKnownTimerAmount != MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime && (!wid || wid.delay == 0f))
			{
				for (float num3 = 4f; num3 > 0f; num3 -= 1f)
				{
					if (MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime / 5f >= num3 / 4f && lastKnownTimerAmount / 5f < num3 / 4f)
					{
						Object.Instantiate(timerWindupSound).pitch = 0.6f + num3 * 0.1f;
						break;
					}
				}
				lastKnownTimerAmount = MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime;
			}
		}
		if (cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		}
		else if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && MonoSingleton<GunControl>.Instance.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			if (!wid || wid.delay == 0f)
			{
				Shoot();
			}
			else
			{
				Invoke("Shoot", wid.delay);
				cooldown = 1f;
			}
		}
		for (int i = 0; i < variationColorables.Length; i++)
		{
			variationColorables[i].color = new Color(color.r, color.g, color.b, colorablesTransparencies[i] * num);
		}
	}

	public void Shoot()
	{
		if ((bool)aud)
		{
			aud.pitch = Random.Range(0.9f, 1.1f);
			aud.Play();
		}
		if (variation == 1 && cbCharge > 0f)
		{
			chargeSound.Stop();
			cbCharge = 0f;
		}
		Object.Instantiate(muzzleFlash, shootPoint.position, MonoSingleton<CameraController>.Instance.transform.rotation);
		anim.SetTrigger("Fire");
		cooldown = rateOfFire;
		GameObject gameObject = Object.Instantiate(rocket, MonoSingleton<CameraController>.Instance.transform.position, MonoSingleton<CameraController>.Instance.transform.rotation);
		if ((bool)MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget && MonoSingleton<CameraFrustumTargeter>.Instance.IsAutoAimed)
		{
			gameObject.transform.LookAt(MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget.bounds.center);
		}
		Grenade component = gameObject.GetComponent<Grenade>();
		if ((bool)component)
		{
			component.sourceWeapon = MonoSingleton<GunControl>.Instance.currentWeapon;
		}
		MonoSingleton<CameraController>.Instance.CameraShake(0.75f);
		MonoSingleton<RumbleManager>.Instance.SetVibrationTracked("rumble.gun.fire", base.gameObject);
	}

	public void ShootCannonball()
	{
		if ((bool)aud)
		{
			aud.pitch = Random.Range(0.6f, 0.8f);
			aud.Play();
		}
		Object.Instantiate(muzzleFlash, shootPoint.position, MonoSingleton<CameraController>.Instance.transform.rotation);
		anim.SetTrigger("Fire");
		cooldown = rateOfFire;
		Rigidbody rigidbody = Object.Instantiate(cannonBall, MonoSingleton<CameraController>.Instance.transform.position + MonoSingleton<CameraController>.Instance.transform.forward, MonoSingleton<CameraController>.Instance.transform.rotation);
		if ((bool)MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget && MonoSingleton<CameraFrustumTargeter>.Instance.IsAutoAimed)
		{
			rigidbody.transform.LookAt(MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget.bounds.center);
		}
		rigidbody.velocity = rigidbody.transform.forward * Mathf.Max(15f, cbCharge * 150f);
		if (rigidbody.TryGetComponent<Cannonball>(out var component))
		{
			component.sourceWeapon = MonoSingleton<GunControl>.Instance.currentWeapon;
		}
		MonoSingleton<CameraController>.Instance.CameraShake(0.75f);
		cbCharge = 0f;
	}

	public void FreezeRockets()
	{
		MonoSingleton<WeaponCharges>.Instance.rocketFrozen = true;
		if (!wid || wid.delay == 0f)
		{
			MonoSingleton<WeaponCharges>.Instance.timeSinceIdleFrozen = 0f;
			Object.Instantiate(timerFreezeSound);
			if (!currentTimerTickSound)
			{
				currentTimerTickSound = Object.Instantiate(timerTickSound);
			}
		}
	}

	public void UnfreezeRockets()
	{
		MonoSingleton<WeaponCharges>.Instance.rocketFrozen = false;
		if (!wid || wid.delay == 0f)
		{
			MonoSingleton<WeaponCharges>.Instance.canAutoUnfreeze = false;
			Object.Instantiate(timerUnfreezeSound);
			if ((bool)currentTimerTickSound)
			{
				Object.Destroy(currentTimerTickSound);
			}
		}
	}

	public void Clunk(float pitch)
	{
		GameObject obj = Object.Instantiate(clunkSound, base.transform.position, Quaternion.identity);
		MonoSingleton<CameraController>.Instance.CameraShake(0.25f);
		if (obj.TryGetComponent<AudioSource>(out var component))
		{
			component.pitch = Random.Range(pitch - 0.1f, pitch + 0.1f);
		}
	}
}
