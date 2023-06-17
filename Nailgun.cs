using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

public class Nailgun : MonoBehaviour
{
	private InputManager inman;

	private WeaponIdentifier wid;

	public int variation;

	public bool altVersion;

	public GameObject[] shootPoints;

	private Spin[] barrels;

	private float spinSpeed;

	private int barrelNum;

	private Light[] barrelLights;

	[SerializeField]
	private Renderer[] barrelHeats;

	private float heatUp;

	private bool burnOut;

	public GameObject muzzleFlash;

	public GameObject muzzleFlash2;

	public AudioSource snapSound;

	public float fireRate;

	private float currentFireRate;

	private float fireCooldown;

	private AudioSource aud;

	private AudioSource barrelAud;

	public GameObject nail;

	public GameObject heatedNail;

	public GameObject magnetNail;

	private CameraController cc;

	public float spread;

	private float currentSpread;

	private int burstAmount;

	private Animator anim;

	private bool canShoot;

	private NewMovement nm;

	[Header("Magnet")]
	public Text ammoText;

	public GameObject noAmmoSound;

	public GameObject lastShotSound;

	private float harpoonCharge = 1f;

	private bool shotSuccesfully;

	[Header("Overheat")]
	public Color emptyColor;

	public Color fullColor;

	private Slider heatSlider;

	private Image sliderBg;

	private float heatSinks = 2f;

	private float heatSinkFill = 2f;

	public Image[] heatSinkImages;

	private ParticleSystem heatSteam;

	private AudioSource heatSteamAud;

	private float heatCharge = 1f;

	private WeaponCharges wc;

	private WeaponPos wpos;

	private GunControl gc;

	private bool lookingForValue;

	private CameraFrustumTargeter targeter;

	private string[] projectileVariationTypes;

	private void Start()
	{
		targeter = MonoSingleton<CameraFrustumTargeter>.Instance;
		inman = MonoSingleton<InputManager>.Instance;
		wid = GetComponent<WeaponIdentifier>();
		barrels = GetComponentsInChildren<Spin>();
		barrelLights = barrels[0].transform.parent.GetComponentsInChildren<Light>();
		SetHeat(0f);
		barrelAud = barrels[0].GetComponent<AudioSource>();
		aud = GetComponent<AudioSource>();
		anim = GetComponentInChildren<Animator>();
		cc = MonoSingleton<CameraController>.Instance;
		nm = MonoSingleton<NewMovement>.Instance;
		heatSlider = GetComponentInChildren<Slider>();
		if ((bool)heatSlider)
		{
			sliderBg = heatSlider.GetComponentInParent<Image>();
		}
		currentFireRate = fireRate;
		if (!altVersion)
		{
			heatSteam = GetComponentInChildren<ParticleSystem>();
			heatSteamAud = heatSteam.GetComponent<AudioSource>();
		}
		if ((bool)aud)
		{
			aud.volume -= wid.delay * 2f;
			if (aud.volume < 0f)
			{
				aud.volume = 0f;
			}
		}
		if ((bool)barrelAud)
		{
			barrelAud.volume -= wid.delay * 2f;
			if (barrelAud.volume < 0f)
			{
				barrelAud.volume = 0f;
			}
		}
		if ((bool)heatSteamAud)
		{
			heatSteamAud.volume -= wid.delay * 2f;
			if (heatSteamAud.volume < 0f)
			{
				heatSteamAud.volume = 0f;
			}
		}
		if (wc == null)
		{
			wc = MonoSingleton<WeaponCharges>.Instance;
		}
		wpos = GetComponent<WeaponPos>();
		gc = GetComponentInParent<GunControl>();
		projectileVariationTypes = new string[4];
		for (int i = 0; i < projectileVariationTypes.Length; i++)
		{
			projectileVariationTypes[i] = "nailgun" + i;
		}
		if (altVersion && variation == 0 && heatSinks == 2f)
		{
			heatSinks = 1f;
		}
		anim.SetLayerWeight(1, 0f);
	}

	private void OnDisable()
	{
		canShoot = false;
		harpoonCharge = 1f;
		wc.naiheatUp = heatUp;
		wc.nai0set = true;
		if (variation == 0)
		{
			if (!altVersion)
			{
				wc.naiHeatsinks = heatSinks;
			}
			else
			{
				wc.naiSawHeatsinks = heatSinks;
			}
		}
		MonoSingleton<WeaponCharges>.Instance.naiAmmoDontCharge = false;
	}

	private void OnEnable()
	{
		if (wc == null)
		{
			wc = MonoSingleton<WeaponCharges>.Instance;
		}
		if (variation == 0)
		{
			if (!altVersion)
			{
				heatSinks = wc.naiHeatsinks;
			}
			else
			{
				heatSinks = wc.naiSawHeatsinks;
			}
		}
		if (variation == 1 && (bool)aud)
		{
			RefreshHeatSinkFill(wc.naiMagnetCharge, playSound: true);
		}
		if (wc.nai0set)
		{
			wc.nai0set = false;
			if (heatSinks >= 1f)
			{
				heatUp = wc.naiheatUp;
			}
		}
		else
		{
			lookingForValue = true;
		}
		spinSpeed = 250f + heatUp * 2250f;
		anim?.SetLayerWeight(1, 0f);
	}

	private void Update()
	{
		if (NoWeaponCooldown.NoCooldown)
		{
			if (altVersion)
			{
				heatSinks = 1f;
			}
			else
			{
				heatSinks = 2f;
			}
			wc.naiAmmo = 100f;
			wc.naiSaws = 10f;
			wc.naiMagnetCharge = 3f;
		}
		if (lookingForValue && (bool)wc && wc.nai0set)
		{
			wc.nai0set = false;
			heatUp = wc.naiheatUp;
			spinSpeed = 250f + heatUp * 2250f;
		}
		if (burnOut || heatSinks < 1f)
		{
			heatUp = Mathf.MoveTowards(heatUp, 0f, Time.deltaTime);
			if (burnOut && heatUp <= 0f)
			{
				burnOut = false;
				heatSteam?.Stop();
				heatSteamAud?.Stop();
			}
		}
		else if (canShoot && !MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && heatUp < 1f && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			if (variation == 1)
			{
				heatUp = 1f;
			}
			else if (!altVersion)
			{
				heatUp = Mathf.MoveTowards(heatUp, 1f, Time.deltaTime * 0.55f);
			}
		}
		else if (heatUp > 0f && (!canShoot || !MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed))
		{
			if (!altVersion)
			{
				heatUp = Mathf.MoveTowards(heatUp, 0f, Time.deltaTime * 0.3f);
			}
			else if (fireCooldown <= 0f)
			{
				heatUp = Mathf.MoveTowards(heatUp, 0f, Time.deltaTime * 0.2f);
			}
			else
			{
				heatUp = Mathf.MoveTowards(heatUp, 0f, Time.deltaTime * 0.03f);
			}
		}
		if ((bool)heatSlider)
		{
			if (heatSlider.value != heatUp)
			{
				heatSlider.value = heatUp;
				sliderBg.color = Color.Lerp(emptyColor, fullColor, heatUp);
				if (heatUp <= 0f && heatSinks < 1f)
				{
					sliderBg.color = new Color(0f, 0f, 0f, 0f);
				}
			}
			else if (heatUp == 0f && heatSinks >= 1f && sliderBg.color.a == 0f)
			{
				sliderBg.color = emptyColor;
			}
		}
		if (canShoot && !MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			spinSpeed = 250f + heatUp * 2250f;
		}
		else
		{
			spinSpeed = Mathf.MoveTowards(spinSpeed, 0f, Time.deltaTime * 1000f);
		}
		if (variation != 0)
		{
			if (burnOut)
			{
				currentFireRate = fireRate - 2.5f;
			}
			else if (heatSinks >= 1f)
			{
				currentFireRate = fireRate + 3.5f - heatUp * 3.5f;
			}
			else
			{
				currentFireRate = fireRate + 10f;
			}
		}
		else if (burnOut)
		{
			if (altVersion)
			{
				currentFireRate = 3f;
			}
			else
			{
				currentFireRate = fireRate - 2.5f;
			}
		}
		else if (heatSinks >= 1f)
		{
			if (heatUp < 0.5f)
			{
				currentFireRate = fireRate;
			}
			else if (altVersion)
			{
				currentFireRate = fireRate + (heatUp - 0.5f) * 65f;
			}
			else
			{
				currentFireRate = fireRate + (heatUp - 0.5f) * 7f;
			}
		}
		else if (altVersion)
		{
			currentFireRate = fireRate + 50f;
		}
		else
		{
			currentFireRate = fireRate + 10f;
		}
		barrelAud.pitch = spinSpeed / 1500f * 2f;
		Spin[] array = barrels;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].speed = spinSpeed;
		}
		if (heatUp > 0f)
		{
			Light[] array2 = barrelLights;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].intensity = heatUp * 10f;
			}
			SetHeat(heatUp);
		}
		else
		{
			Light[] array2 = barrelLights;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].intensity = 0f;
			}
			SetHeat(0f);
		}
		if (variation == 1)
		{
			if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
			{
				MonoSingleton<WeaponCharges>.Instance.naiAmmoDontCharge = true;
			}
			else
			{
				MonoSingleton<WeaponCharges>.Instance.naiAmmoDontCharge = false;
			}
			if (altVersion)
			{
				ammoText.text = Mathf.RoundToInt(wc.naiSaws).ToString();
			}
			else
			{
				ammoText.text = Mathf.RoundToInt(wc.naiAmmo).ToString();
			}
		}
		if (canShoot && !burnOut && heatSinks >= 1f && heatUp >= 0.1f && variation == 0 && MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			if (altVersion)
			{
				SuperSaw();
			}
			else
			{
				burnOut = true;
				fireCooldown = 0f;
				heatSinks -= 1f;
				heatSteam?.Play();
				heatSteamAud?.Play();
				currentFireRate = fireRate - 2.5f;
			}
		}
		if (canShoot && MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && variation == 1 && (!wid || wid.delay == 0f))
		{
			if (wc.naiMagnetCharge >= 1f)
			{
				ShootMagnet();
			}
			else
			{
				Object.Instantiate(noAmmoSound);
			}
		}
		if (variation == 0 && !MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			if (heatSinks < 2f && !altVersion)
			{
				heatSinks = Mathf.MoveTowards(heatSinks, 2f, Time.deltaTime * 0.125f);
			}
			else if (altVersion && heatSinks < 1f)
			{
				heatSinks = Mathf.MoveTowards(heatSinks, 1f, Time.deltaTime * 0.125f);
			}
		}
		if (heatSinkImages != null && heatSinkImages.Length != 0)
		{
			float naiMagnetCharge = heatSinks;
			if (variation == 1)
			{
				naiMagnetCharge = wc.naiMagnetCharge;
			}
			RefreshHeatSinkFill(naiMagnetCharge, heatSinkFill != naiMagnetCharge);
		}
	}

	private void FixedUpdate()
	{
		if (fireCooldown > 0f)
		{
			fireCooldown = Mathf.MoveTowards(fireCooldown, 0f, Time.deltaTime * 100f);
			if (fireCooldown < 0.01f)
			{
				fireCooldown = 0f;
			}
		}
		if (canShoot && ((!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed) || burnOut) && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			if (fireCooldown != 0f)
			{
				return;
			}
			if (variation == 1 && ((!altVersion && Mathf.RoundToInt(wc.naiAmmo) <= 0) || (altVersion && Mathf.RoundToInt(wc.naiSaws) <= 0)))
			{
				fireCooldown = currentFireRate * 2f;
				if (shotSuccesfully)
				{
					Object.Instantiate(lastShotSound);
				}
				else
				{
					Object.Instantiate(noAmmoSound);
				}
				shotSuccesfully = false;
			}
			else if (!wid || wid.delay == 0f)
			{
				Shoot();
			}
			else
			{
				fireCooldown = currentFireRate;
				Invoke("Shoot", wid.delay / 10f);
			}
		}
		else if (!MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			shotSuccesfully = false;
		}
	}

	private void UpdateAnimationWeight()
	{
		if (!burnOut && variation == 0)
		{
			if (heatSinks < 1f)
			{
				anim.SetLayerWeight(1, 0.9f);
			}
			else if (wpos.currentDefault == wpos.middlePos)
			{
				anim.SetLayerWeight(1, 0.75f);
			}
			else
			{
				anim.SetLayerWeight(1, heatUp * 0.6f);
			}
		}
		else if (wpos.currentDefault == wpos.middlePos || ((bool)MonoSingleton<PowerUpMeter>.Instance && MonoSingleton<PowerUpMeter>.Instance.juice > 0f))
		{
			anim.SetLayerWeight(1, 0.75f);
		}
		else
		{
			anim.SetLayerWeight(1, 0f);
		}
	}

	private void SetHeat(float heat)
	{
		Renderer[] array = barrelHeats;
		for (int i = 0; i < array.Length; i++)
		{
			Material material = array[i].material;
			material.color = new Color(material.color.r, material.color.g, material.color.b, heat * 0.5f);
		}
		if (heat == 0f)
		{
			MonoSingleton<RumbleManager>.Instance.StopVibration("rumble.gun.nailgun_fire");
		}
		else if (barrelHeats != null && barrelHeats.Length != 0 && !altVersion)
		{
			bool flag = variation == 0 && burnOut;
			MonoSingleton<RumbleManager>.Instance.SetVibrationTracked("rumble.gun.nailgun_fire", barrelHeats[0].gameObject).intensityMultiplier = (flag ? 3f : heat);
		}
	}

	private void Shoot()
	{
		UpdateAnimationWeight();
		fireCooldown = currentFireRate;
		shotSuccesfully = true;
		if (variation == 1 && (!wid || wid.delay == 0f))
		{
			if (altVersion)
			{
				wc.naiSaws -= 1f;
			}
			else
			{
				wc.naiAmmo -= 1f;
			}
		}
		anim.SetTrigger("Shoot");
		barrelNum++;
		if (barrelNum >= shootPoints.Length)
		{
			barrelNum = 0;
		}
		GameObject gameObject = ((!burnOut) ? Object.Instantiate(muzzleFlash, shootPoints[barrelNum].transform) : Object.Instantiate(muzzleFlash2, shootPoints[barrelNum].transform));
		if (!altVersion)
		{
			AudioSource component = gameObject.GetComponent<AudioSource>();
			if (burnOut)
			{
				component.volume = 0.65f - wid.delay * 2f;
				if (component.volume < 0f)
				{
					component.volume = 0f;
				}
				component.pitch = 2f;
				currentSpread = spread * 2f;
			}
			else
			{
				if (heatSinks < 1f)
				{
					component.pitch = 0.25f;
					component.volume = 0.25f - wid.delay * 2f;
					if (component.volume < 0f)
					{
						component.volume = 0f;
					}
				}
				else
				{
					component.volume = 0.65f - wid.delay * 2f;
					if (component.volume < 0f)
					{
						component.volume = 0f;
					}
				}
				currentSpread = spread;
			}
		}
		else if (burnOut)
		{
			currentSpread = 45f;
		}
		else if (altVersion && variation == 0)
		{
			if (heatSinks < 1f)
			{
				currentSpread = 45f;
			}
			else
			{
				currentSpread = Mathf.Lerp(0f, 45f, Mathf.Max(0f, heatUp - 0.25f));
			}
		}
		else
		{
			currentSpread = 0f;
		}
		GameObject gameObject2 = ((!burnOut) ? Object.Instantiate(nail, cc.transform.position + cc.transform.forward, base.transform.rotation) : Object.Instantiate(heatedNail, cc.transform.position + cc.transform.forward, base.transform.rotation));
		if (altVersion && variation == 0 && heatSinks >= 1f)
		{
			heatUp = Mathf.MoveTowards(heatUp, 1f, 0.125f);
		}
		gameObject2.transform.forward = cc.transform.forward;
		if (Physics.Raycast(cc.transform.position, cc.transform.forward, 1f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			gameObject2.transform.position = cc.transform.position;
		}
		if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
		{
			gameObject2.transform.position = cc.transform.position + (targeter.CurrentTarget.bounds.center - cc.transform.position).normalized;
			gameObject2.transform.LookAt(targeter.CurrentTarget.bounds.center);
		}
		gameObject2.transform.Rotate(Random.Range((0f - currentSpread) / 3f, currentSpread / 3f), Random.Range((0f - currentSpread) / 3f, currentSpread / 3f), Random.Range((0f - currentSpread) / 3f, currentSpread / 3f));
		if (gameObject2.TryGetComponent<Rigidbody>(out var component2))
		{
			component2.velocity = gameObject2.transform.forward * 200f;
		}
		if (gameObject2.TryGetComponent<Nail>(out var component3))
		{
			component3.sourceWeapon = gc.currentWeapon;
			component3.weaponType = projectileVariationTypes[variation];
			if (altVersion && variation == 0)
			{
				if (heatSinks >= 1f)
				{
					component3.hitAmount = Mathf.Lerp(3f, 1f, heatUp);
				}
				else
				{
					component3.hitAmount = 1f;
				}
			}
			if (component3.sawblade)
			{
				component3.ForceCheckSawbladeRicochet();
			}
		}
		if (!burnOut)
		{
			cc.CameraShake(0.1f);
		}
		else
		{
			cc.CameraShake(0.35f);
		}
		if (altVersion)
		{
			MonoSingleton<RumbleManager>.Instance.SetVibration("rumble.gun.sawblade");
		}
	}

	public void BurstFire()
	{
		UpdateAnimationWeight();
		burstAmount--;
		barrelNum++;
		if (barrelNum >= shootPoints.Length)
		{
			barrelNum = 0;
		}
		Object.Instantiate(muzzleFlash2, shootPoints[barrelNum].transform);
		currentSpread = spread;
		GameObject gameObject = Object.Instantiate(heatedNail, base.transform.position + base.transform.forward, base.transform.rotation);
		gameObject.transform.forward = base.transform.forward * -1f;
		if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
		{
			gameObject.transform.LookAt(targeter.CurrentTarget.bounds.center);
			gameObject.transform.forward *= -1f;
		}
		gameObject.transform.Rotate(Random.Range((0f - currentSpread) / 3f, currentSpread / 3f), Random.Range((0f - currentSpread) / 3f, currentSpread / 3f), Random.Range((0f - currentSpread) / 3f, currentSpread / 3f));
		gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * -100f, ForceMode.VelocityChange);
		Nail component = gameObject.GetComponent<Nail>();
		component.weaponType = projectileVariationTypes[variation];
		component.sourceWeapon = gc.currentWeapon;
		cc.CameraShake(0.5f);
		if (burstAmount > 0)
		{
			Invoke("BurstFire", 0.03f);
		}
	}

	public void SuperSaw()
	{
		fireCooldown = currentFireRate;
		shotSuccesfully = true;
		anim.SetLayerWeight(1, 0f);
		anim.SetTrigger("SuperShoot");
		MonoSingleton<RumbleManager>.Instance.SetVibration("rumble.gun.super_saw");
		barrelNum++;
		if (barrelNum >= shootPoints.Length)
		{
			barrelNum = 0;
		}
		Object.Instantiate(muzzleFlash2, shootPoints[barrelNum].transform);
		currentSpread = 0f;
		GameObject gameObject = Object.Instantiate(heatedNail, cc.transform.position + cc.transform.forward, base.transform.rotation);
		gameObject.transform.forward = cc.transform.forward;
		if (Physics.Raycast(cc.transform.position, cc.transform.forward, 1f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			gameObject.transform.position = cc.transform.position;
		}
		if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
		{
			gameObject.transform.position = cc.transform.position + (targeter.CurrentTarget.bounds.center - cc.transform.position).normalized;
			gameObject.transform.LookAt(targeter.CurrentTarget.bounds.center);
		}
		if (gameObject.TryGetComponent<Rigidbody>(out var component))
		{
			component.velocity = gameObject.transform.forward * 200f;
		}
		if (gameObject.TryGetComponent<Nail>(out var component2))
		{
			component2.weaponType = projectileVariationTypes[variation];
			component2.multiHitAmount = Mathf.RoundToInt(heatUp * 3f);
			component2.ForceCheckSawbladeRicochet();
			component2.sourceWeapon = gc.currentWeapon;
		}
		heatSinks -= 1f;
		heatUp = 0f;
		cc.CameraShake(0.5f);
	}

	public void ShootMagnet()
	{
		UpdateAnimationWeight();
		GameObject gameObject = Object.Instantiate(magnetNail, cc.transform.position, base.transform.rotation);
		gameObject.transform.forward = base.transform.forward;
		if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
		{
			gameObject.transform.LookAt(targeter.CurrentTarget.bounds.center);
		}
		gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 100f, ForceMode.VelocityChange);
		anim.SetTrigger("Shoot");
		if (fireCooldown < 10f)
		{
			fireCooldown = 10f;
		}
		Magnet componentInChildren = gameObject.GetComponentInChildren<Magnet>();
		if ((bool)componentInChildren)
		{
			wc.magnets.Add(componentInChildren);
		}
		wc.naiMagnetCharge -= 1f;
		MonoSingleton<RumbleManager>.Instance.SetVibration("rumble.magnet_released");
	}

	public void CanShoot()
	{
		canShoot = true;
	}

	private void MaxCharge()
	{
		if (variation == 0 && (heatSinks < 1f || (heatSinks < 2f && !altVersion)))
		{
			if (altVersion)
			{
				heatSinks = 1f;
			}
			else
			{
				heatSinks = 2f;
			}
		}
	}

	private void RefreshHeatSinkFill(float charge, bool playSound = false)
	{
		heatSinkFill = Mathf.MoveTowards(heatSinkFill, charge, Time.deltaTime * (Mathf.Abs((charge - heatSinkFill) * 20f) + 1f));
		for (int i = 0; i < heatSinkImages.Length; i++)
		{
			if (heatSinkFill > (float)i)
			{
				heatSinkImages[i].fillAmount = heatSinkFill - (float)i;
				int num = CorrectVariation();
				if (heatSinkFill >= (float)(i + 1) && heatSinkImages[i].color != MonoSingleton<ColorBlindSettings>.Instance.variationColors[num])
				{
					if (playSound)
					{
						aud.pitch = (float)i * 0.5f + 1f;
						aud.Play();
					}
					heatSinkImages[i].color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[num];
				}
				else if (heatSinkFill < (float)(i + 1))
				{
					heatSinkImages[i].color = emptyColor;
				}
			}
			else
			{
				heatSinkImages[i].fillAmount = 0f;
			}
		}
		if ((bool)ammoText)
		{
			ammoText.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[CorrectVariation()];
		}
	}

	public void SnapSound()
	{
		Object.Instantiate(snapSound);
	}

	private int CorrectVariation()
	{
		if (variation == 0)
		{
			return 1;
		}
		if (variation == 1)
		{
			return 0;
		}
		return variation;
	}
}
