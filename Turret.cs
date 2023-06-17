using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class Turret : MonoBehaviour
{
	public bool stationary;

	private Vector3 stationaryPosition;

	private NavMeshPath path;

	private Transform target;

	private Vector3 aimPos;

	[HideInInspector]
	public bool lodged;

	[HideInInspector]
	public bool aiming;

	private float outOfSightTimer;

	private float aimTime;

	private float maxAimTime = 5f;

	private float flashTime;

	private float nextBeepTime;

	private bool whiteLine;

	private Color defaultColor = new Color(1f, 0.44f, 0.74f);

	private Vector3 lastPlayerPosition;

	private int difficulty;

	private float cooldown = 2f;

	private float kickCooldown = 1f;

	[HideInInspector]
	public bool inAction;

	private bool bodyRotate;

	private bool bodyTrackPlayer;

	private bool bodyReset;

	private Quaternion currentBodyRotation;

	private bool walking;

	private Vector3 walkTarget;

	public Color defaultLightsColor;

	public Color attackingLightsColor;

	private float lightsIntensityTarget = 1.5f;

	private float currentLightsIntensity = 1.25f;

	[Header("Defaults")]
	[SerializeField]
	private Transform torso;

	[SerializeField]
	private Transform turret;

	[SerializeField]
	private Transform shootPoint;

	[SerializeField]
	private LineRenderer aimLine;

	[SerializeField]
	private RevolverBeam beam;

	[SerializeField]
	private GameObject warningFlash;

	[SerializeField]
	private ParticleSystem antennaFlash;

	[SerializeField]
	private Light antennaLight;

	[SerializeField]
	private AudioSource antennaSound;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	private Machine mach;

	[SerializeField]
	private EnemyIdentifier eid;

	[SerializeField]
	private GameObject head;

	[SerializeField]
	private NavMeshAgent nma;

	public GameObject antenna;

	public List<Transform> interruptables = new List<Transform>();

	[SerializeField]
	private AudioSource interruptSound;

	[SerializeField]
	private AudioSource cancelSound;

	[SerializeField]
	private AudioSource footStep;

	[SerializeField]
	private AudioSource extendSound;

	[SerializeField]
	private AudioSource thunkSound;

	[SerializeField]
	private AudioSource kickWarningSound;

	[SerializeField]
	private AudioSource aimWarningSound;

	private AudioSource currentSound;

	[SerializeField]
	private GameObject rubble;

	[SerializeField]
	private GameObject rubbleLeft;

	[SerializeField]
	private GameObject rubbleRight;

	private bool leftLodged;

	private bool rightLodged;

	[SerializeField]
	private SkinnedMeshRenderer smr;

	[SerializeField]
	private GameObject unparryableFlash;

	[SerializeField]
	private SwingCheck2 sc;

	[SerializeField]
	private TrailRenderer tr;

	private void Start()
	{
		target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
		currentBodyRotation = base.transform.rotation;
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		path = new NavMeshPath();
		if (stationary)
		{
			stationaryPosition = base.transform.position;
		}
		Invoke("SlowUpdate", 0.5f);
		switch (difficulty)
		{
		case 0:
			maxAimTime = 7.5f;
			anim.speed = 0.5f;
			break;
		case 1:
			maxAimTime = 5f;
			anim.speed = 0.75f;
			break;
		case 2:
			maxAimTime = 5f;
			break;
		case 3:
			maxAimTime = 4f;
			break;
		case 4:
		case 5:
			maxAimTime = 3f;
			break;
		}
		if (difficulty >= 2)
		{
			anim.speed = 1f;
		}
		anim.speed *= eid.totalSpeedModifier;
	}

	private void UpdateBuff()
	{
		if (difficulty >= 2)
		{
			anim.speed = 1f;
		}
		else if (difficulty == 1)
		{
			anim.speed = 0.75f;
		}
		else
		{
			anim.speed = 0.5f;
		}
		anim.speed *= eid.totalSpeedModifier;
	}

	private void OnEnable()
	{
		Unlodge();
		CancelAim(instant: true);
		DamageStop();
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 0.25f);
		if (BlindEnemies.Blind)
		{
			return;
		}
		if (!inAction && mach.grounded && nma.isOnNavMesh)
		{
			if (stationary)
			{
				if (!(Vector3.Distance(base.transform.position, stationaryPosition) > 1f))
				{
					return;
				}
				NavMesh.CalculatePath(base.transform.position, stationaryPosition, nma.areaMask, path);
				if (path.status == NavMeshPathStatus.PathComplete)
				{
					nma.path = path;
					return;
				}
			}
			bool flag = false;
			if (Physics.CheckSphere(torso.position - Vector3.up * 0.5f, 1.5f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)) || Physics.SphereCast(torso.position - Vector3.up * 0.5f, 1.5f, target.position + Vector3.up - torso.position, out var _, Vector3.Distance(target.position + Vector3.up, torso.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				NavMesh.CalculatePath(base.transform.position, target.position, nma.areaMask, path);
				if (path.status == NavMeshPathStatus.PathComplete)
				{
					walking = false;
					flag = true;
					nma.path = path;
				}
			}
			if (!walking && !flag)
			{
				Vector3 onUnitSphere = Random.onUnitSphere;
				onUnitSphere = new Vector3(onUnitSphere.x, 0f, onUnitSphere.z);
				RaycastHit hitInfo3;
				if (Physics.Raycast(torso.position, onUnitSphere, out var hitInfo2, 25f, LayerMaskDefaults.Get(LMD.Environment)))
				{
					if (NavMesh.SamplePosition(hitInfo2.point, out var hit, 5f, nma.areaMask))
					{
						walkTarget = hit.position;
					}
					else if (Physics.SphereCast(hitInfo2.point, 1f, Vector3.down, out hitInfo2, 25f, LayerMaskDefaults.Get(LMD.Environment)))
					{
						walkTarget = hitInfo2.point;
					}
				}
				else if (Physics.Raycast(torso.position + onUnitSphere * 25f, Vector3.down, out hitInfo3, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
				{
					walkTarget = hitInfo3.point;
				}
				NavMesh.CalculatePath(base.transform.position, walkTarget, nma.areaMask, path);
				nma.path = path;
				walking = true;
			}
			else if (Vector3.Distance(base.transform.position, walkTarget) < 1f || nma.path.status != 0)
			{
				walking = false;
			}
		}
		else
		{
			walking = false;
		}
	}

	private void Update()
	{
		if (aiming && !mach.grounded)
		{
			CancelAim(instant: true);
		}
		if (!inAction)
		{
			rubbleLeft.SetActive(value: false);
			rubbleRight.SetActive(value: false);
		}
		if (!inAction && mach.grounded && !BlindEnemies.Blind)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			kickCooldown = Mathf.MoveTowards(kickCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			if (stationary && Vector3.Distance(base.transform.position, stationaryPosition) <= 1f)
			{
				base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
			}
			RaycastHit hitInfo;
			if (Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, base.transform.position) < 5f && kickCooldown <= 0f && difficulty >= 2)
			{
				Kick();
			}
			else if (cooldown <= 0f && !Physics.CheckSphere(torso.position - Vector3.up * 0.5f, 1.5f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)) && !Physics.SphereCast(torso.position - Vector3.up * 0.5f, 1.5f, target.position + Vector3.up - torso.position, out hitInfo, Vector3.Distance(target.position + Vector3.up, torso.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				StartWindup();
			}
			else if (nma.velocity.magnitude >= 1f)
			{
				anim.SetBool("Running", value: true);
			}
			else
			{
				anim.SetBool("Running", value: false);
			}
		}
		else
		{
			anim.SetBool("Running", value: false);
		}
		if (aiming)
		{
			if (BlindEnemies.Blind)
			{
				CancelAim();
				return;
			}
			RaycastHit hitInfo3;
			if (difficulty < 2 && aimTime >= maxAimTime)
			{
				if (difficulty == 1)
				{
					lastPlayerPosition = Vector3.MoveTowards(lastPlayerPosition, target.position, Time.deltaTime * Vector3.Distance(lastPlayerPosition, target.position) * 5f * eid.totalSpeedModifier);
				}
				if (Physics.Raycast(torso.position, lastPlayerPosition - torso.position, out var hitInfo2, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.EnvironmentAndPlayer)))
				{
					aimPos = hitInfo2.point;
				}
				else
				{
					aimPos = torso.position + (lastPlayerPosition - torso.position).normalized * 10000f;
				}
				outOfSightTimer = 0f;
			}
			else if (Physics.Raycast(torso.position, target.position - torso.position, out hitInfo3, Vector3.Distance(torso.position, target.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				aimPos = hitInfo3.point;
				if (flashTime == 0f)
				{
					outOfSightTimer += Time.deltaTime;
				}
			}
			else
			{
				aimPos = target.position;
				outOfSightTimer = 0f;
			}
			aimTime = Mathf.MoveTowards(aimTime, maxAimTime, Time.deltaTime * eid.totalSpeedModifier);
			if (outOfSightTimer >= 1f)
			{
				if ((bool)currentSound)
				{
					Object.Destroy(currentSound.gameObject);
				}
				currentSound = Object.Instantiate(cancelSound, torso);
				CancelAim();
			}
			else if (aimTime >= maxAimTime && (outOfSightTimer == 0f || flashTime != 0f))
			{
				if (flashTime == 0f)
				{
					mach.parryable = true;
					Object.Instantiate(warningFlash, shootPoint.transform).transform.localScale *= 2.5f;
					ChangeLineColor(new Color(1f, 0.75f, 0.5f));
					lastPlayerPosition = target.position;
					ChangeLightsColor(attackingLightsColor);
				}
				flashTime = Mathf.MoveTowards(flashTime, 1f, Time.deltaTime * (float)((difficulty < 2) ? 1 : 2) * eid.totalSpeedModifier);
				if (flashTime >= 1f)
				{
					Shoot();
				}
			}
			else if (aimTime >= nextBeepTime)
			{
				if (whiteLine)
				{
					ChangeLineColor(defaultColor);
				}
				else
				{
					ChangeLineColor(Color.white);
				}
				antennaFlash?.Play();
				antennaSound?.Play();
				whiteLine = !whiteLine;
				nextBeepTime = aimTime + (maxAimTime - aimTime) / 6f;
			}
		}
		currentLightsIntensity = Mathf.MoveTowards(currentLightsIntensity, lightsIntensityTarget, Time.deltaTime / 4f);
		if (currentLightsIntensity == lightsIntensityTarget)
		{
			lightsIntensityTarget = ((lightsIntensityTarget == 1.5f) ? (lightsIntensityTarget = 1.25f) : (lightsIntensityTarget = 1.5f));
		}
		ChangeLightsIntensity(currentLightsIntensity);
	}

	private void LateUpdate()
	{
		if (aiming)
		{
			if (difficulty < 2 && aimTime >= maxAimTime)
			{
				AimAt(lastPlayerPosition);
			}
			else
			{
				AimAt(target.position);
			}
		}
		else
		{
			if (!bodyRotate)
			{
				return;
			}
			if (bodyTrackPlayer || bodyReset)
			{
				Quaternion quaternion = Quaternion.LookRotation(target.position - torso.position);
				if (bodyReset)
				{
					quaternion = base.transform.rotation;
				}
				float num = 10f;
				if (bodyTrackPlayer)
				{
					num = 35f;
				}
				currentBodyRotation = Quaternion.RotateTowards(currentBodyRotation, quaternion, Time.deltaTime * (Quaternion.Angle(quaternion, currentBodyRotation) * num + num) * eid.totalSpeedModifier);
				if (bodyReset && currentBodyRotation == quaternion)
				{
					bodyRotate = false;
					bodyReset = false;
				}
			}
			torso.rotation = currentBodyRotation;
			torso.Rotate(Vector3.up * -90f, Space.Self);
		}
	}

	private void StartWindup()
	{
		anim.SetBool("Aiming", value: true);
		if (nma.isOnNavMesh)
		{
			nma.SetDestination(base.transform.position);
		}
		base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
		inAction = true;
		kickCooldown = 0f;
		if ((bool)currentSound)
		{
			Object.Destroy(currentSound.gameObject);
		}
		currentSound = Object.Instantiate(aimWarningSound, torso);
	}

	private void BodyTrack()
	{
		bodyRotate = true;
		bodyTrackPlayer = true;
		bodyReset = false;
	}

	private void BodyFreeze()
	{
		bodyRotate = true;
		bodyTrackPlayer = false;
		bodyReset = false;
	}

	private void BodyReset()
	{
		bodyRotate = true;
		bodyTrackPlayer = false;
		bodyReset = true;
	}

	private void StartAiming()
	{
		aiming = true;
		whiteLine = false;
		ChangeLineColor(defaultColor);
		nextBeepTime = aimTime + (maxAimTime - aimTime) / 6f;
		flashTime = 0f;
		eid.weakPoint = antenna;
		antennaFlash?.Play();
		antennaSound?.Play();
	}

	private void Kick()
	{
		anim.SetTrigger("Kick");
		if (nma.isOnNavMesh)
		{
			nma.SetDestination(base.transform.position);
		}
		base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
		inAction = true;
		ChangeLightsColor(new Color(0.35f, 0.55f, 1f));
		kickCooldown = 1f;
		if ((bool)currentSound)
		{
			Object.Destroy(currentSound.gameObject);
		}
		currentSound = Object.Instantiate(kickWarningSound, torso);
		UnparryableFlash();
	}

	private void StopAction()
	{
		inAction = false;
		rubbleLeft.SetActive(value: false);
		rubbleRight.SetActive(value: false);
	}

	private void AimAt(Vector3 position)
	{
		torso.LookAt(position);
		currentBodyRotation = torso.rotation;
		torso.Rotate(Vector3.up * -90f, Space.Self);
		turret.LookAt(position, torso.up);
		turret.Rotate(Vector3.up * -90f, Space.Self);
		aimLine.enabled = true;
		aimLine.SetPosition(0, shootPoint.position);
		aimLine.SetPosition(1, aimPos);
	}

	private void Shoot()
	{
		RevolverBeam revolverBeam = Object.Instantiate(beam, new Vector3(base.transform.position.x, shootPoint.transform.position.y, base.transform.position.z), shootPoint.transform.rotation);
		revolverBeam.alternateStartPoint = shootPoint.transform.position;
		if (eid.totalDamageModifier != 1f && revolverBeam.TryGetComponent<RevolverBeam>(out var component))
		{
			component.damage *= eid.totalDamageModifier;
		}
		anim.Play("Shoot");
		CancelAim();
		BodyFreeze();
		cooldown = Random.Range(2.5f, 3.5f);
	}

	private void ChangeLineColor(Color clr)
	{
		Gradient gradient = new Gradient();
		GradientColorKey[] array = new GradientColorKey[1];
		array[0].color = clr;
		GradientAlphaKey[] array2 = new GradientAlphaKey[1];
		array2[0].alpha = 1f;
		gradient.SetKeys(array, array2);
		aimLine.colorGradient = gradient;
		nextBeepTime = (maxAimTime - aimTime) / 2f;
	}

	public void CancelAim(bool instant = false)
	{
		ChangeLightsColor(defaultLightsColor);
		aiming = false;
		aimLine.enabled = false;
		aimTime = 0f;
		outOfSightTimer = 0f;
		anim.SetBool("Aiming", value: false);
		BodyReset();
		eid.weakPoint = head;
		mach.parryable = false;
		if (instant)
		{
			inAction = false;
			if (mach.grounded)
			{
				anim.Play("Idle");
			}
		}
		if (cooldown < 1f)
		{
			cooldown = 1f;
		}
	}

	public void LodgeFoot(int type)
	{
		if (type == 0)
		{
			leftLodged = true;
			rubbleLeft.SetActive(value: true);
		}
		else
		{
			rightLodged = true;
			rubbleRight.SetActive(value: true);
		}
		if (leftLodged && rightLodged)
		{
			lodged = true;
		}
	}

	public void UnlodgeFoot(int type)
	{
		if (type == 0 && leftLodged)
		{
			leftLodged = false;
			rubbleLeft.SetActive(value: false);
			Object.Instantiate(rubble, rubbleLeft.transform.position, base.transform.rotation);
		}
		else if (type == 1 && rightLodged)
		{
			rightLodged = false;
			rubbleRight.SetActive(value: false);
			Object.Instantiate(rubble, rubbleRight.transform.position, base.transform.rotation);
		}
		lodged = false;
	}

	public void Unlodge()
	{
		UnlodgeFoot(0);
		UnlodgeFoot(1);
		kickCooldown = 0.25f;
	}

	public void Interrupt()
	{
		if (!mach.limp)
		{
			anim.SetTrigger("Interrupt");
			CancelAim();
			BodyFreeze();
			cooldown = 3f;
			if ((bool)currentSound)
			{
				Object.Destroy(currentSound.gameObject);
			}
			currentSound = Object.Instantiate(interruptSound, torso);
		}
	}

	public void OnDeath()
	{
		CancelAim();
		if ((bool)currentSound)
		{
			Object.Destroy(currentSound.gameObject);
		}
		ChangeLightsColor(new Color(0.05f, 0.05f, 0.05f, 1f));
		if ((bool)antennaLight)
		{
			antennaLight.enabled = false;
		}
		Unlodge();
		if ((bool)sc)
		{
			sc.gameObject.SetActive(value: false);
		}
		Object.Destroy(this);
	}

	private void FootStep(float targetPitch)
	{
		if (targetPitch == 0f)
		{
			targetPitch = 1.5f;
		}
		Object.Instantiate(footStep, base.transform.position, Quaternion.identity).pitch = Random.Range(targetPitch - 0.1f, targetPitch + 0.1f);
	}

	private void Thunk()
	{
		Object.Instantiate(thunkSound, base.transform.position, Quaternion.identity);
	}

	private void ExtendBarrel()
	{
		Object.Instantiate(extendSound, base.transform.position, Quaternion.identity);
	}

	private void GotParried()
	{
		Interrupt();
	}

	public void UnparryableFlash()
	{
		Object.Instantiate(unparryableFlash, torso.position + base.transform.forward, base.transform.rotation).transform.localScale *= 2.5f;
	}

	public void DamageStart()
	{
		sc.DamageStart();
		tr.enabled = true;
	}

	public void DamageStop()
	{
		sc.DamageStop();
		tr.enabled = false;
		ChangeLightsColor(defaultLightsColor);
	}

	public void ChangeLightsColor(Color target)
	{
		if ((bool)smr && (bool)smr.sharedMaterial && smr.sharedMaterial.HasProperty("_EmissiveColor"))
		{
			smr.material.SetColor("_EmissiveColor", target);
			if ((bool)antennaLight)
			{
				antennaLight.color = target;
			}
		}
	}

	public void ChangeLightsIntensity(float amount)
	{
		if ((bool)smr && (bool)smr.sharedMaterial && smr.sharedMaterial.HasProperty("_EmissiveIntensity"))
		{
			smr.material.SetFloat("_EmissiveIntensity", amount);
			if ((bool)antennaLight)
			{
				antennaLight.intensity = amount * 8f;
			}
		}
	}
}
