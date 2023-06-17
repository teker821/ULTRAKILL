using System.Collections.Generic;
using UnityEngine;

public class Punch : MonoBehaviour
{
	private InputManager inman;

	public FistType type;

	private string hitter;

	private float damage;

	private float screenShakeMultiplier;

	private float force;

	private bool tryForExplode;

	private float cooldownCost;

	public bool ready = true;

	[HideInInspector]
	public Animator anim;

	private SkinnedMeshRenderer smr;

	private Revolver rev;

	private AudioSource aud;

	private GameObject camObj;

	private CameraController cc;

	private RaycastHit hit;

	public LayerMask deflectionLayerMask;

	public LayerMask ignoreEnemyTrigger;

	public LayerMask environmentMask;

	private NewMovement nmov;

	private TrailRenderer tr;

	private Light parryLight;

	private GameObject currentDustParticle;

	public GameObject dustParticle;

	public AudioSource normalHit;

	public AudioSource heavyHit;

	public AudioSource specialHit;

	private StyleHUD shud;

	private StatsManager sman;

	public bool holding;

	public Transform holder;

	public ItemIdentifier heldItem;

	private FistControl fc;

	private bool shopping;

	private int shopRequests;

	public GameObject parriedProjectileHitObject;

	private ProjectileParryZone ppz;

	private bool returnToOrigRot;

	public GameObject blastWave;

	private bool holdingInput;

	public GameObject shell;

	public Transform shellEjector;

	private AudioSource ejectorAud;

	private bool alreadyBoostedProjectile;

	private bool ignoreDoublePunch;

	private bool hitSomething;

	private void Start()
	{
		inman = MonoSingleton<InputManager>.Instance;
		anim = GetComponent<Animator>();
		smr = GetComponentInChildren<SkinnedMeshRenderer>();
		rev = base.transform.parent.parent.GetComponentInChildren<Revolver>();
		camObj = MonoSingleton<CameraController>.Instance.gameObject;
		cc = MonoSingleton<CameraController>.Instance;
		aud = GetComponent<AudioSource>();
		parryLight = base.transform.Find("PunchZone").GetComponent<Light>();
		nmov = GetComponentInParent<NewMovement>();
		tr = GetComponentInChildren<TrailRenderer>();
		shud = MonoSingleton<StyleHUD>.Instance;
		sman = MonoSingleton<StatsManager>.Instance;
		holdingInput = false;
		if (fc == null)
		{
			fc = MonoSingleton<FistControl>.Instance;
		}
		switch (type)
		{
		case FistType.Standard:
			damage = 1f;
			screenShakeMultiplier = 1f;
			force = 25f;
			tryForExplode = false;
			cooldownCost = 2f;
			hitter = "punch";
			break;
		case FistType.Heavy:
			damage = 2.5f;
			screenShakeMultiplier = 2f;
			force = 100f;
			tryForExplode = true;
			cooldownCost = 3f;
			hitter = "heavypunch";
			break;
		}
	}

	private void OnEnable()
	{
		holdingInput = false;
		ReadyToPunch();
		ignoreDoublePunch = false;
		if (fc == null)
		{
			fc = GetComponentInParent<FistControl>();
			anim = GetComponent<Animator>();
		}
		if (fc.heldObject != null)
		{
			heldItem = fc.heldObject;
			heldItem.transform.SetParent(holder, worldPositionStays: true);
			holding = true;
			if (!heldItem.noHoldingAnimation && fc.forceNoHold <= 0)
			{
				anim.SetBool("SemiHolding", value: false);
				anim.SetBool("Holding", value: true);
				anim.Play("Holding", -1, 0f);
			}
			else
			{
				anim.SetBool("SemiHolding", value: true);
			}
			ResetHeldItemPosition();
		}
	}

	public void ResetHeldState()
	{
		holding = false;
		anim.SetBool("Holding", value: false);
		anim.SetBool("SemiHolding", value: false);
	}

	public void ForceThrow()
	{
		if (!heldItem)
		{
			ResetHeldState();
			return;
		}
		Rigidbody[] componentsInChildren = heldItem.GetComponentsInChildren<Rigidbody>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return;
		}
		heldItem.transform.SetParent(null, worldPositionStays: true);
		heldItem.pickedUp = false;
		if (heldItem.reverseTransformSettings)
		{
			heldItem.transform.localScale = Vector3.one;
		}
		else
		{
			heldItem.transform.localScale = heldItem.putDownScale;
		}
		Transform[] componentsInChildren2 = heldItem.GetComponentsInChildren<Transform>();
		foreach (Transform obj in componentsInChildren2)
		{
			obj.gameObject.layer = 22;
			if (obj.TryGetComponent<OutdoorsChecker>(out var component) && component.enabled)
			{
				component.CancelInvoke("SlowUpdate");
				component.SlowUpdate();
			}
		}
		Rigidbody[] array = componentsInChildren;
		foreach (Rigidbody obj2 in array)
		{
			obj2.isKinematic = false;
			obj2.AddForce((base.transform.parent.forward + Vector3.up * 0.1f) * 5000f);
		}
		Collider[] componentsInChildren3 = heldItem.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].enabled = true;
		}
		heldItem.transform.position = base.transform.parent.position + base.transform.parent.forward;
		heldItem.SendMessage("PutDown", SendMessageOptions.DontRequireReceiver);
		anim.SetBool("Holding", value: false);
		anim.SetBool("SemiHolding", value: false);
		holding = false;
		fc.heldObject = null;
		heldItem = null;
	}

	public void PlaceHeldObject(ItemPlaceZone[] placeZones, Transform target)
	{
		if (!heldItem)
		{
			ResetHeldState();
			return;
		}
		AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.IsName("JabHolding"))
		{
			ignoreDoublePunch = true;
			anim.Play("Jab", 0, currentAnimatorStateInfo.normalizedTime);
		}
		holding = false;
		anim.SetBool("Holding", value: false);
		anim.SetBool("SemiHolding", value: false);
		heldItem.transform.SetParent(target);
		heldItem.pickedUp = false;
		if (heldItem.reverseTransformSettings)
		{
			heldItem.transform.localPosition = Vector3.zero;
			heldItem.transform.localScale = Vector3.one;
			heldItem.transform.localRotation = Quaternion.identity;
		}
		else
		{
			heldItem.transform.localPosition = heldItem.putDownPosition;
			heldItem.transform.localScale = heldItem.putDownScale;
			heldItem.transform.localRotation = Quaternion.Euler(heldItem.putDownRotation);
		}
		Transform[] componentsInChildren = heldItem.GetComponentsInChildren<Transform>();
		foreach (Transform obj in componentsInChildren)
		{
			obj.gameObject.layer = 22;
			if (obj.TryGetComponent<OutdoorsChecker>(out var component) && component.enabled)
			{
				component.CancelInvoke("SlowUpdate");
				component.SlowUpdate();
			}
		}
		Collider[] componentsInChildren2 = heldItem.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = true;
		}
		heldItem.SendMessage("PutDown", SendMessageOptions.DontRequireReceiver);
		Object.Instantiate(heldItem.pickUpSound);
		heldItem = null;
		fc.heldObject = null;
		for (int i = 0; i < placeZones.Length; i++)
		{
			placeZones[i].CheckItem();
		}
		ResetHeldState();
	}

	public void ResetHeldItemPosition()
	{
		if (heldItem.reverseTransformSettings)
		{
			heldItem.transform.localPosition = heldItem.putDownPosition;
			heldItem.transform.localScale = heldItem.putDownScale;
			heldItem.transform.localRotation = Quaternion.Euler(heldItem.putDownRotation);
		}
		else
		{
			heldItem.transform.localPosition = Vector3.zero;
			heldItem.transform.localScale = Vector3.one;
			heldItem.transform.localRotation = Quaternion.identity;
		}
		Transform[] componentsInChildren = heldItem.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 13;
		}
	}

	public void ForceHold(ItemIdentifier itid)
	{
		holding = true;
		if (itid.TryGetComponent<FishObjectReference>(out var component) && (bool)MonoSingleton<FishManager>.Instance && MonoSingleton<FishManager>.Instance.recognizedFishes.ContainsKey(component.fishObject) && !MonoSingleton<FishManager>.Instance.recognizedFishes[component.fishObject])
		{
			MonoSingleton<FishManager>.Instance.UnlockFish(component.fishObject);
			MonoSingleton<FishingHUD>.Instance.ShowFishCaught(show: true, component.fishObject);
		}
		if (!itid.noHoldingAnimation && fc.forceNoHold <= 0)
		{
			anim.SetBool("SemiHolding", value: false);
			anim.SetBool("Holding", value: true);
		}
		else
		{
			anim.SetBool("SemiHolding", value: true);
		}
		AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.IsName("Jab") || currentAnimatorStateInfo.IsName("Jab2"))
		{
			ignoreDoublePunch = true;
			anim.Play("JabHolding", 0, currentAnimatorStateInfo.normalizedTime);
		}
		ItemPlaceZone[] componentsInParent = itid.GetComponentsInParent<ItemPlaceZone>();
		itid.ipz = null;
		heldItem = itid;
		itid.transform.SetParent(holder);
		fc.heldObject = itid;
		itid.pickedUp = true;
		ResetHeldItemPosition();
		Transform[] componentsInChildren = heldItem.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 13;
		}
		Rigidbody[] componentsInChildren2 = heldItem.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].isKinematic = true;
		}
		Collider[] componentsInChildren3 = heldItem.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].enabled = false;
		}
		Object.Instantiate(itid.pickUpSound);
		heldItem.SendMessage("PickUp", SendMessageOptions.DontRequireReceiver);
		if (componentsInParent != null && componentsInParent.Length != 0)
		{
			ItemPlaceZone[] array = componentsInParent;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CheckItem();
			}
		}
	}

	private void OnDisable()
	{
		holding = false;
		anim.SetBool("Holding", value: false);
		anim.SetBool("SemiHolding", value: false);
		ignoreDoublePunch = false;
	}

	private void Update()
	{
		if (MonoSingleton<OptionsManager>.Instance.paused)
		{
			return;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Punch.WasPerformedThisFrame && ready && !shopping && fc.fistCooldown <= 0f && fc.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			fc.weightCooldown += cooldownCost * 0.25f + fc.weightCooldown * cooldownCost * 0.1f;
			fc.fistCooldown += fc.weightCooldown;
			PunchStart();
			holdingInput = true;
		}
		if (holdingInput && MonoSingleton<InputManager>.Instance.InputSource.Punch.WasCanceledThisFrame)
		{
			holdingInput = false;
		}
		float layerWeight = anim.GetLayerWeight(1);
		if (shopping && layerWeight < 1f)
		{
			anim.SetLayerWeight(1, Mathf.MoveTowards(layerWeight, 1f, Time.deltaTime / 10f + 5f * Time.deltaTime * (1f - layerWeight)));
		}
		else if (!shopping && layerWeight > 0f)
		{
			anim.SetLayerWeight(1, Mathf.MoveTowards(layerWeight, 0f, Time.deltaTime / 10f + 5f * Time.deltaTime * layerWeight));
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && shopping)
		{
			anim.SetTrigger("ShopTap");
		}
		if (returnToOrigRot)
		{
			base.transform.parent.localRotation = Quaternion.RotateTowards(base.transform.parent.localRotation, Quaternion.identity, (Quaternion.Angle(base.transform.parent.localRotation, Quaternion.identity) * 5f + 5f) * Time.deltaTime * 5f);
			if (base.transform.parent.localRotation == Quaternion.identity)
			{
				returnToOrigRot = false;
			}
		}
		if (fc.shopping && !shopping)
		{
			ShopMode();
		}
		else if (!fc.shopping && shopping)
		{
			StopShop();
		}
		if (holding && (bool)heldItem)
		{
			if (!heldItem.noHoldingAnimation && fc.forceNoHold <= 0)
			{
				anim.SetBool("SemiHolding", value: false);
				anim.SetBool("Holding", value: true);
			}
			else
			{
				anim.SetBool("SemiHolding", value: true);
			}
		}
	}

	private void PunchStart()
	{
		if (ready)
		{
			ready = false;
			anim.SetFloat("PunchRandomizer", Random.Range(0f, 1f));
			anim.SetTrigger("Punch");
			aud.pitch = Random.Range(0.9f, 1.1f);
			aud.Play();
			tr.widthMultiplier = 0.5f;
			MonoSingleton<HookArm>.Instance.Cancel();
			if (holding && (bool)heldItem)
			{
				heldItem.SendMessage("PunchWith", SendMessageOptions.DontRequireReceiver);
			}
			MonoSingleton<RumbleManager>.Instance.SetVibration("rumble.punch");
		}
	}

	private void ActiveStart()
	{
		if (ignoreDoublePunch)
		{
			ignoreDoublePunch = false;
			return;
		}
		returnToOrigRot = false;
		hitSomething = false;
		if (type == FistType.Standard)
		{
			Collider[] array = Physics.OverlapSphere(cc.GetDefaultPos(), 0.01f, deflectionLayerMask, QueryTriggerInteraction.Collide);
			List<Transform> list = new List<Transform>();
			Collider[] array2;
			if (array.Length != 0)
			{
				array2 = array;
				foreach (Collider collider in array2)
				{
					list.Add(collider.transform);
					if (CheckForProjectile((collider.attachedRigidbody != null) ? collider.attachedRigidbody.transform : collider.transform))
					{
						break;
					}
				}
			}
			if ((!Physics.Raycast(cc.GetDefaultPos(), camObj.transform.forward, out hit, 4f, deflectionLayerMask) && !Physics.BoxCast(cc.GetDefaultPos(), Vector3.one * 0.3f, camObj.transform.forward, out hit, camObj.transform.rotation, 4f, deflectionLayerMask)) || list.Contains(hit.transform) || !CheckForProjectile(hit.transform))
			{
				if (ppz == null)
				{
					ppz = base.transform.parent.GetComponentInChildren<ProjectileParryZone>();
				}
				if (ppz != null)
				{
					Projectile projectile = ppz.CheckParryZone();
					if (projectile != null && !list.Contains(projectile.transform) && !projectile.undeflectable && (!alreadyBoostedProjectile || !projectile.playerBullet))
					{
						ParryProjectile(projectile);
						hitSomething = true;
					}
				}
			}
			Collider[] array3 = Physics.OverlapSphere(cc.GetDefaultPos() + camObj.transform.forward * 3f, 3f, deflectionLayerMask, QueryTriggerInteraction.Collide);
			bool flag = false;
			array2 = array3;
			foreach (Collider collider2 in array2)
			{
				if ((collider2.attachedRigidbody ? collider2.attachedRigidbody.TryGetComponent<Nail>(out var component) : collider2.TryGetComponent<Nail>(out component)) && component.sawblade && component.punchable)
				{
					flag = true;
					if (component.stopped)
					{
						component.stopped = false;
						component.rb.velocity = (GetParryLookTarget() - component.transform.position).normalized * component.originalVelocity.magnitude;
					}
					else
					{
						component.rb.velocity = (GetParryLookTarget() - component.transform.position).normalized * component.rb.velocity.magnitude;
					}
					component.punched = true;
					if (component.magnets.Count > 0)
					{
						component.punchDistance = Vector3.Distance(component.transform.position, component.GetTargetMagnet().transform.position);
					}
				}
			}
			if (!flag)
			{
				array2 = Physics.OverlapSphere(cc.GetDefaultPos() + camObj.transform.forward, 1f, 1, QueryTriggerInteraction.Collide);
				foreach (Collider collider3 in array2)
				{
					float num = Vector3.Distance(cc.GetDefaultPos() + camObj.transform.forward, collider3.transform.position);
					if (num < 6f || num > 12f || Mathf.Abs((cc.GetDefaultPos() + camObj.transform.forward).y - collider3.transform.position.y) > 3f || !collider3.TryGetComponent<Magnet>(out var component2) || component2.sawblades.Count <= 0)
					{
						continue;
					}
					float num2 = float.PositiveInfinity;
					float num3 = 0f;
					int num4 = -1;
					for (int num5 = component2.sawblades.Count - 1; num5 >= 0; num5--)
					{
						if (component2.sawblades[num5] == null)
						{
							component2.sawblades.RemoveAt(num5);
							if (flag)
							{
								num4--;
							}
						}
						else
						{
							num3 = Vector3.Distance(component2.sawblades[num5].transform.position, cc.GetDefaultPos());
							if (component2.sawblades[num5] != null && (num4 < 0 || num2 < num3))
							{
								num4 = num5;
								num2 = num3;
								flag = true;
							}
						}
					}
					if (!flag || !component2.sawblades[num4].TryGetComponent<Nail>(out var component3))
					{
						continue;
					}
					component3.transform.position = cc.GetDefaultPos() + cc.transform.forward;
					if (component3.stopped)
					{
						component3.stopped = false;
						component3.rb.velocity = (GetParryLookTarget() - component3.transform.position).normalized * component3.originalVelocity.magnitude;
					}
					else
					{
						component3.rb.velocity = (GetParryLookTarget() - component3.transform.position).normalized * component3.rb.velocity.magnitude;
					}
					component3.punched = true;
					if (component3.magnets.Count > 0)
					{
						Magnet targetMagnet = component3.GetTargetMagnet();
						if (Vector3.Distance(component3.transform.position + component3.rb.velocity.normalized, targetMagnet.transform.position) > Vector3.Distance(component3.transform.position, targetMagnet.transform.position))
						{
							component3.MagnetRelease(targetMagnet);
						}
						else
						{
							component3.punchDistance = Vector3.Distance(component3.transform.position, targetMagnet.transform.position);
						}
					}
					break;
				}
			}
			if (flag)
			{
				Object.Instantiate(specialHit, base.transform.position, Quaternion.identity);
				MonoSingleton<TimeController>.Instance.HitStop(0.1f);
				anim.Play("Hook", -1, 0.065f);
				hitSomething = true;
			}
		}
		else if (Physics.Raycast(cc.GetDefaultPos(), camObj.transform.forward, out hit, 4f, deflectionLayerMask) || Physics.BoxCast(cc.GetDefaultPos(), Vector3.one * 0.3f, camObj.transform.forward, out hit, camObj.transform.rotation, 4f, deflectionLayerMask))
		{
			MassSpear component4 = hit.transform.gameObject.GetComponent<MassSpear>();
			if (component4 != null && component4.hitPlayer)
			{
				Object.Instantiate(specialHit, base.transform.position, Quaternion.identity);
				MonoSingleton<TimeController>.Instance.HitStop(0.1f);
				cc.CameraShake(0.5f * screenShakeMultiplier);
				component4.GetHurt(25f);
				hitSomething = true;
			}
		}
		bool flag2 = holding;
		Collider[] array4 = Physics.OverlapSphere(cc.GetDefaultPos(), 0.1f, ignoreEnemyTrigger, QueryTriggerInteraction.Collide);
		if (array4 != null && array4.Length != 0)
		{
			Collider[] array2 = array4;
			foreach (Collider collider4 in array2)
			{
				PunchSuccess(cc.GetDefaultPos(), collider4.transform);
			}
			hitSomething = true;
		}
		else if (Physics.Raycast(cc.GetDefaultPos(), camObj.transform.forward, out hit, 4f, ignoreEnemyTrigger, QueryTriggerInteraction.Collide) || Physics.SphereCast(cc.GetDefaultPos(), 1f, camObj.transform.forward, out hit, 4f, ignoreEnemyTrigger, QueryTriggerInteraction.Collide))
		{
			bool flag3 = false;
			if (Physics.Raycast(cc.GetDefaultPos(), hit.point - cc.GetDefaultPos(), out var hitInfo, 5f, environmentMask) && Vector3.Distance(cc.GetDefaultPos(), hit.point) > Vector3.Distance(cc.GetDefaultPos(), hitInfo.point))
			{
				flag3 = true;
			}
			if (!flag3)
			{
				PunchSuccess(hit.point, hit.transform);
				hitSomething = true;
			}
		}
		if (Physics.CheckSphere(cc.GetDefaultPos(), 0.01f, environmentMask, QueryTriggerInteraction.Collide))
		{
			Collider[] array2 = Physics.OverlapSphere(cc.GetDefaultPos(), 0.01f, environmentMask);
			foreach (Collider collider5 in array2)
			{
				hitSomething = true;
				AltHit(collider5.transform);
			}
		}
		else if (Physics.Raycast(cc.GetDefaultPos(), camObj.transform.forward, out hit, 4f, environmentMask))
		{
			AltHit(hit.transform);
			if (!hitSomething && (hit.transform.gameObject.layer == 8 || hit.transform.gameObject.layer == 24))
			{
				base.transform.parent.localRotation = Quaternion.identity;
				cc.CameraShake(0.2f * screenShakeMultiplier);
				Object.Instantiate(normalHit, base.transform.position, Quaternion.identity);
				currentDustParticle = Object.Instantiate(dustParticle, hit.point, base.transform.rotation);
				currentDustParticle.transform.forward = hit.normal;
				Breakable component5 = hit.transform.gameObject.GetComponent<Breakable>();
				if (component5 != null && !component5.precisionOnly && (component5.weak || type == FistType.Heavy))
				{
					component5.Break();
				}
				if (hit.collider.gameObject.TryGetComponent<Bleeder>(out var component6))
				{
					if (type == FistType.Standard)
					{
						component6.GetHit(hit.point, GoreType.Body);
					}
					else
					{
						component6.GetHit(hit.point, GoreType.Head);
					}
				}
				if (type == FistType.Heavy)
				{
					Glass component7 = hit.collider.gameObject.GetComponent<Glass>();
					if (component7 != null && !component7.broken)
					{
						component7.Shatter();
					}
				}
			}
		}
		if (flag2 && holding && heldItem != null)
		{
			ForceThrow();
			cc.CameraShake(0.2f * screenShakeMultiplier);
		}
		else
		{
			cc.CameraShake(0.2f * screenShakeMultiplier);
		}
	}

	private bool CheckForProjectile(Transform target)
	{
		if (target.TryGetComponent<ParryHelper>(out var component))
		{
			target = component.target;
		}
		if (target.TryGetComponent<Projectile>(out var component2) && !component2.undeflectable && (!alreadyBoostedProjectile || !component2.playerBullet))
		{
			ParryProjectile(component2);
			hitSomething = true;
			return true;
		}
		if (target.TryGetComponent<Cannonball>(out var component3) && component3.launchable)
		{
			anim.Play("Hook", 0, 0.065f);
			if (!component3.parry)
			{
				MonoSingleton<TimeController>.Instance.ParryFlash();
			}
			else
			{
				Parry();
			}
			component3.transform.LookAt(GetParryLookTarget());
			component3.Launch();
			return true;
		}
		if (target.TryGetComponent<ParryReceiver>(out var component4))
		{
			if (!component4.enabled)
			{
				return false;
			}
			anim.Play("Hook", 0, 0.065f);
			if (component4.parryHeal)
			{
				Parry();
			}
			else
			{
				MonoSingleton<TimeController>.Instance.ParryFlash();
			}
			component4.Parry();
		}
		else
		{
			if (target.TryGetComponent<ThrownSword>(out var component5) && !component5.friendly && component5.active)
			{
				component5.GetParried();
				anim.Play("Hook", -1, 0.065f);
				Parry(hook: false, component5.returnTransform.GetComponentInParent<EnemyIdentifier>());
				hitSomething = true;
				return true;
			}
			if (target.TryGetComponent<MassSpear>(out var component6))
			{
				if (!component6.beenStopped)
				{
					component6.Deflected();
					anim.Play("Hook", -1, 0.065f);
					Parry();
					hitSomething = true;
				}
				else if (component6.hitPlayer)
				{
					Object.Instantiate(specialHit, base.transform.position, Quaternion.identity);
					MonoSingleton<TimeController>.Instance.HitStop(0.1f);
					cc.CameraShake(0.5f * screenShakeMultiplier);
					component6.GetHurt(5f);
					hitSomething = true;
				}
				return true;
			}
		}
		return false;
	}

	public void CoinFlip()
	{
		if (ready && MonoSingleton<FistControl>.Instance.forceNoHold <= 0)
		{
			anim.SetTrigger("CoinFlip");
		}
	}

	private void ActiveEnd()
	{
		tr.widthMultiplier = 0f;
		ignoreDoublePunch = false;
		if (type == FistType.Standard)
		{
			ResetFistRotation();
		}
	}

	public void ResetFistRotation()
	{
		returnToOrigRot = true;
	}

	private void PunchEnd()
	{
	}

	private void ReadyToPunch()
	{
		returnToOrigRot = true;
		holdingInput = false;
		ready = true;
		alreadyBoostedProjectile = false;
		ignoreDoublePunch = false;
	}

	private void PunchSuccess(Vector3 point, Transform target)
	{
		base.transform.parent.LookAt(point);
		if (Quaternion.Angle(base.transform.parent.localRotation, Quaternion.identity) > 45f)
		{
			Quaternion localRotation = base.transform.parent.localRotation;
			float num = localRotation.eulerAngles.x;
			float num2 = localRotation.eulerAngles.y;
			float num3 = localRotation.eulerAngles.z;
			if (num > 180f)
			{
				num -= 360f;
			}
			if (num2 > 180f)
			{
				num2 -= 360f;
			}
			if (num3 > 180f)
			{
				num3 -= 360f;
			}
			localRotation.eulerAngles = new Vector3(Mathf.Clamp(num, -45f, 45f), Mathf.Clamp(num2, -45f, 45f), Mathf.Clamp(num3, -45f, 45f));
			base.transform.parent.localRotation = localRotation;
		}
		EnemyIdentifier component3;
		if (target.gameObject.tag == "Enemy" || target.gameObject.tag == "Armor" || target.gameObject.tag == "Head" || target.gameObject.tag == "Body" || target.gameObject.tag == "Limb" || target.gameObject.tag == "EndLimb")
		{
			if (anim.GetFloat("PunchRandomizer") < 0.5f)
			{
				anim.Play("Jab", 0, 0.075f);
			}
			else
			{
				anim.Play("Jab2", 0, 0.075f);
			}
			Object.Instantiate(heavyHit, base.transform.position, Quaternion.identity);
			MonoSingleton<TimeController>.Instance.HitStop(0.1f);
			cc.CameraShake(0.5f * screenShakeMultiplier);
			EnemyIdentifier enemyIdentifier = null;
			if (target.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
			{
				enemyIdentifier = component.eid;
			}
			if ((bool)enemyIdentifier)
			{
				if (enemyIdentifier.drillers.Count > 0 && type != FistType.Heavy)
				{
					anim.Play("Hook", 0, 0.065f);
					MonoSingleton<TimeController>.Instance.ParryFlash();
					enemyIdentifier.drillers[enemyIdentifier.drillers.Count - 1].transform.forward = cc.transform.forward;
					enemyIdentifier.drillers[enemyIdentifier.drillers.Count - 1].transform.position = cc.GetDefaultPos();
					enemyIdentifier.drillers[enemyIdentifier.drillers.Count - 1].Punched();
				}
				enemyIdentifier.hitter = hitter;
				enemyIdentifier.DeliverDamage(target.gameObject, camObj.transform.forward * force * 1000f, point, damage, tryForExplode);
			}
			if (holding)
			{
				heldItem.SendMessage("HitWith", target.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (target.gameObject.tag == "Coin" && type == FistType.Standard)
		{
			Coin component2 = target.GetComponent<Coin>();
			if ((bool)component2 && component2.doubled)
			{
				anim.Play("Hook", 0, 0.065f);
				target.GetComponent<Coin>()?.DelayedPunchflection();
			}
		}
		else if (target.TryGetComponent<EnemyIdentifier>(out component3) && component3.enemyType == EnemyType.Idol)
		{
			component3.hitter = hitter;
			component3.DeliverDamage(target.gameObject, camObj.transform.forward * force * 1000f, point, damage, tryForExplode);
		}
	}

	public void Parry(bool hook = false, EnemyIdentifier eid = null)
	{
		if (hook)
		{
			anim.Play("Hook", 0, 0.065f);
		}
		aud.pitch = Random.Range(0.7f, 0.8f);
		MonoSingleton<TimeController>.Instance.ParryFlash();
		nmov.exploded = false;
		nmov.GetHealth(999, silent: false);
		nmov.FullStamina();
		if (!eid || !eid.blessed)
		{
			shud.AddPoints(100, "ultrakill.parry");
		}
	}

	private void ParryProjectile(Projectile proj)
	{
		proj.hittingPlayer = false;
		proj.friendly = true;
		proj.speed *= 2f;
		proj.homingType = HomingType.None;
		proj.explosionEffect = parriedProjectileHitObject;
		proj.precheckForCollisions = true;
		Rigidbody component = proj.GetComponent<Rigidbody>();
		if (proj.playerBullet)
		{
			alreadyBoostedProjectile = true;
			proj.boosted = true;
			proj.GetComponent<SphereCollider>().radius *= 4f;
			proj.damage = 0f;
			if ((bool)component)
			{
				component.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			}
			Color color = new Color(1f, 0.35f, 0f);
			if (proj.TryGetComponent<MeshRenderer>(out var component2) && (bool)component2.material && component2.material.HasProperty("_Color"))
			{
				component2.material.SetColor("_Color", color);
			}
			if (proj.TryGetComponent<TrailRenderer>(out var component3))
			{
				Gradient gradient = new Gradient();
				gradient.SetKeys(new GradientColorKey[2]
				{
					new GradientColorKey(color, 0f),
					new GradientColorKey(color, 1f)
				}, new GradientAlphaKey[2]
				{
					new GradientAlphaKey(1f, 0f),
					new GradientAlphaKey(0f, 1f)
				});
				component3.colorGradient = gradient;
			}
			if (proj.TryGetComponent<Light>(out var component4))
			{
				component4.color = color;
			}
		}
		if ((bool)component)
		{
			component.constraints = RigidbodyConstraints.FreezeRotation;
		}
		anim.Play("Hook", 0, 0.065f);
		if (!proj.playerBullet)
		{
			Parry();
		}
		else
		{
			MonoSingleton<TimeController>.Instance.ParryFlash();
		}
		if (proj.explosive)
		{
			proj.explosive = false;
		}
		Rigidbody component5 = proj.GetComponent<Rigidbody>();
		if ((bool)component5 && component5.useGravity)
		{
			component5.useGravity = false;
		}
		Vector3 parryLookTarget = GetParryLookTarget();
		proj.transform.LookAt(parryLookTarget);
		if (proj.speed == 0f)
		{
			component5.velocity = (parryLookTarget - base.transform.position).normalized * 250f;
		}
		else if (proj.speed < 100f)
		{
			proj.speed = 100f;
		}
		if (proj.spreaded)
		{
			ProjectileSpread componentInParent = proj.GetComponentInParent<ProjectileSpread>();
			if (componentInParent != null)
			{
				componentInParent.ParriedProjectile();
			}
		}
		proj.transform.SetParent(null, worldPositionStays: true);
	}

	public void BlastCheck()
	{
		if (MonoSingleton<InputManager>.Instance.InputSource.Punch.IsPressed)
		{
			holdingInput = false;
			anim.SetTrigger("PunchBlast");
			Vector3 position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + MonoSingleton<CameraController>.Instance.transform.forward * 2f;
			if (Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), MonoSingleton<CameraController>.Instance.transform.forward, out var hitInfo, 2f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				position = hitInfo.point - camObj.transform.forward * 0.1f;
			}
			Object.Instantiate(blastWave, position, MonoSingleton<CameraController>.Instance.transform.rotation);
		}
	}

	public void Eject()
	{
		if (ejectorAud == null)
		{
			ejectorAud = shellEjector.GetComponent<AudioSource>();
		}
		ejectorAud.Play();
		for (int i = 0; i < 2; i++)
		{
			GameObject gameObject = Object.Instantiate(shell, shellEjector.position + shellEjector.right * 0.075f, shellEjector.rotation);
			if (i == 1)
			{
				gameObject.transform.position = gameObject.transform.position - shellEjector.right * 0.15f;
			}
			gameObject.transform.Rotate(Vector3.forward, Random.Range(-45, 45), Space.Self);
			gameObject.GetComponent<Rigidbody>().AddForce((shellEjector.forward / 1.75f + shellEjector.up / 2f + Vector3.up / 1.75f) * Random.Range(8, 12), ForceMode.VelocityChange);
		}
	}

	public void Hide()
	{
	}

	public void ShopMode()
	{
		shopping = true;
		holdingInput = false;
		shopRequests++;
	}

	public void StopShop()
	{
		shopRequests--;
		if (shopRequests <= 0)
		{
			shopping = false;
		}
	}

	public void EquipAnimation()
	{
		if (anim == null)
		{
			anim = GetComponent<Animator>();
		}
		anim.SetTrigger("Equip");
	}

	private void AltHit(Transform target)
	{
		if (target.gameObject.layer == 22)
		{
			ItemIdentifier itemIdentifier = target.GetComponent<ItemIdentifier>();
			ItemPlaceZone[] components = target.GetComponents<ItemPlaceZone>();
			if ((bool)itemIdentifier && itemIdentifier.infiniteSource)
			{
				itemIdentifier = itemIdentifier.CreateCopy();
			}
			if (holding && components != null && components.Length != 0)
			{
				PlaceHeldObject(components, target);
			}
			else if (!holding && itemIdentifier != null)
			{
				ForceHold(itemIdentifier);
			}
		}
		if (holding)
		{
			heldItem.SendMessage("HitWith", target.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void CancelAttack()
	{
		anim.Rebind();
		anim.Update(0f);
		ActiveEnd();
		ReadyToPunch();
	}

	public static Vector3 GetParryLookTarget()
	{
		Vector3 vector = MonoSingleton<CameraController>.Instance.transform.forward;
		if ((bool)MonoSingleton<CameraFrustumTargeter>.Instance && (bool)MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget && MonoSingleton<CameraFrustumTargeter>.Instance.IsAutoAimed)
		{
			vector = MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.transform.position;
		}
		if (Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), vector, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Ignore))
		{
			return hitInfo.point;
		}
		return MonoSingleton<CameraController>.Instance.GetDefaultPos() + vector * 1000f;
	}
}
