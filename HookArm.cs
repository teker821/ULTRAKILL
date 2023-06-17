using System;
using System.Collections.Generic;
using Sandbox;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class HookArm : MonoSingleton<HookArm>
{
	public bool equipped;

	private LineRenderer lr;

	private Animator anim;

	private Vector3 hookPoint;

	private Vector3 previousHookPoint;

	[HideInInspector]
	public HookState state;

	private bool returning;

	[SerializeField]
	private GameObject model;

	private CapsuleCollider playerCollider;

	public Transform hand;

	public Transform hook;

	public GameObject hookModel;

	private Vector3 throwDirection;

	private float returnDistance;

	private LayerMask throwMask;

	private LayerMask enviroMask;

	private float throwWarp;

	private Transform caughtTransform;

	private Vector3 caughtPoint;

	private Collider caughtCollider;

	private EnemyIdentifier caughtEid;

	private List<EnemyType> deadIgnoreTypes = new List<EnemyType>();

	private List<EnemyType> lightEnemies = new List<EnemyType>();

	private GroundCheckEnemy enemyGroundCheck;

	private Rigidbody enemyRigidbody;

	private HookPoint caughtHook;

	private bool lightTarget;

	[SerializeField]
	private LineRenderer inspectLr;

	private bool forcingGroundCheck;

	private bool forcingFistControl;

	private AudioSource aud;

	[Header("Sounds")]
	public GameObject throwSound;

	public GameObject hitSound;

	public GameObject pullSound;

	public GameObject pullDoneSound;

	public GameObject catchSound;

	public GameObject errorSound;

	public AudioClip throwLoop;

	public AudioClip pullLoop;

	public GameObject wooshSound;

	private GameObject currentWoosh;

	public GameObject clinkSparks;

	public GameObject clinkObjectSparks;

	private float cooldown;

	private CameraFrustumTargeter targeter;

	[HideInInspector]
	public bool beingPulled;

	private List<Rigidbody> caughtObjects = new List<Rigidbody>();

	private float semiBlocked;

	private Grenade caughtGrenade;

	private Cannonball caughtCannonball;

	private void Start()
	{
		targeter = MonoSingleton<CameraFrustumTargeter>.Instance;
		lr = GetComponent<LineRenderer>();
		lr.enabled = false;
		anim = GetComponent<Animator>();
		playerCollider = MonoSingleton<NewMovement>.Instance.GetComponent<CapsuleCollider>();
		aud = GetComponent<AudioSource>();
		throwMask = (int)throwMask | 0x400;
		throwMask = (int)throwMask | 0x800;
		throwMask = (int)throwMask | 0x1000;
		throwMask = (int)throwMask | 0x4000;
		throwMask = (int)throwMask | 0x10000;
		throwMask = (int)throwMask | 0x400000;
		throwMask = (int)throwMask | 0x4000000;
		enviroMask = (int)enviroMask | 0x100;
		enviroMask = (int)enviroMask | 0x10000;
		enviroMask = (int)enviroMask | 0x40000;
		enviroMask = (int)enviroMask | 0x1000000;
		deadIgnoreTypes.Add(EnemyType.Drone);
		deadIgnoreTypes.Add(EnemyType.MaliciousFace);
		deadIgnoreTypes.Add(EnemyType.Virtue);
		lightEnemies.Add(EnemyType.Drone);
		lightEnemies.Add(EnemyType.Filth);
		lightEnemies.Add(EnemyType.Schism);
		lightEnemies.Add(EnemyType.Soldier);
		lightEnemies.Add(EnemyType.Stray);
		lightEnemies.Add(EnemyType.Streetcleaner);
		model.SetActive(value: false);
	}

	public void Inspect()
	{
		model.SetActive(value: true);
		inspectLr.enabled = true;
		anim.Play("Inspect", -1, 0f);
	}

	private void Update()
	{
		if (!MonoSingleton<OptionsManager>.Instance || MonoSingleton<OptionsManager>.Instance.paused)
		{
			return;
		}
		if (!equipped || MonoSingleton<FistControl>.Instance.shopping || !MonoSingleton<FistControl>.Instance.activated)
		{
			if (state != 0 || returning)
			{
				Cancel();
			}
			model.SetActive(value: false);
			return;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Hook.WasPerformedThisFrame)
		{
			if (state == HookState.Pulling)
			{
				StopThrow();
			}
			else if (cooldown <= 0f)
			{
				cooldown = 0.5f;
				model.SetActive(value: true);
				if (!forcingFistControl)
				{
					if ((bool)MonoSingleton<FistControl>.Instance.currentPunch)
					{
						MonoSingleton<FistControl>.Instance.currentPunch.CancelAttack();
					}
					MonoSingleton<FistControl>.Instance.forceNoHold++;
					forcingFistControl = true;
					MonoSingleton<FistControl>.Instance.transform.localRotation = Quaternion.identity;
				}
				lr.enabled = true;
				hookPoint = base.transform.position;
				previousHookPoint = hookPoint;
				if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
				{
					throwDirection = (targeter.CurrentTarget.bounds.center - base.transform.position).normalized;
				}
				else
				{
					throwDirection = base.transform.forward;
				}
				returning = false;
				if (caughtObjects.Count > 0)
				{
					foreach (Rigidbody caughtObject in caughtObjects)
					{
						if ((bool)caughtObject)
						{
							caughtObject.velocity = (MonoSingleton<NewMovement>.Instance.transform.position - caughtObject.transform.position).normalized * (100f + returnDistance / 2f);
						}
					}
					caughtObjects.Clear();
				}
				state = HookState.Throwing;
				lightTarget = false;
				throwWarp = 1f;
				anim.Play("Throw", -1, 0f);
				inspectLr.enabled = false;
				hand.transform.localPosition = new Vector3(0.09f, -0.051f, 0.045f);
				if (MonoSingleton<CameraController>.Instance.defaultFov > 105f)
				{
					hand.transform.localPosition += new Vector3(0.225f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f), -0.25f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f), 0.05f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f));
				}
				caughtPoint = Vector3.zero;
				caughtTransform = null;
				caughtCollider = null;
				caughtEid = null;
				UnityEngine.Object.Instantiate(throwSound);
				aud.clip = throwLoop;
				aud.panStereo = 0f;
				aud.Play();
				aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
				semiBlocked = 0f;
				MonoSingleton<RumbleManager>.Instance.SetVibrationTracked("rumble.whiplash.throw", base.gameObject);
			}
		}
		if (cooldown != 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		}
		if (lr.enabled)
		{
			throwWarp = Mathf.MoveTowards(throwWarp, 0f, Time.deltaTime * 6.5f);
			lr.SetPosition(0, hand.position);
			for (int i = 1; i < lr.positionCount - 1; i++)
			{
				float num = 3f;
				if (i % 2 == 0)
				{
					num = -3f;
				}
				lr.SetPosition(i, Vector3.Lerp(hand.position, hookPoint, (float)i / (float)lr.positionCount) + base.transform.up * num * throwWarp * (1f / (float)i));
			}
			lr.SetPosition(lr.positionCount - 1, hookPoint);
		}
		if (state == HookState.Pulling && !lightTarget && MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame)
		{
			if (MonoSingleton<NewMovement>.Instance.rb.velocity.y < 1f)
			{
				MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(MonoSingleton<NewMovement>.Instance.rb.velocity.x, 1f, MonoSingleton<NewMovement>.Instance.rb.velocity.z);
			}
			MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.ClampMagnitude(MonoSingleton<NewMovement>.Instance.rb.velocity, 30f);
			if (!MonoSingleton<NewMovement>.Instance.gc.touchingGround && !Physics.Raycast(MonoSingleton<NewMovement>.Instance.gc.transform.position, Vector3.down, 1.5f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				MonoSingleton<NewMovement>.Instance.rb.AddForce(Vector3.up * 15f, ForceMode.VelocityChange);
			}
			else if (!MonoSingleton<NewMovement>.Instance.jumping)
			{
				MonoSingleton<NewMovement>.Instance.Jump();
			}
			StopThrow(1f);
		}
		if (!MonoSingleton<FistControl>.Instance.currentPunch || !MonoSingleton<FistControl>.Instance.currentPunch.holding || !forcingFistControl)
		{
			return;
		}
		MonoSingleton<FistControl>.Instance.currentPunch.heldItem.transform.position = hook.position + hook.up * 0.2f;
		if (state != 0 || returning)
		{
			MonoSingleton<FistControl>.Instance.heldObject.hooked = true;
			if (MonoSingleton<FistControl>.Instance.heldObject.gameObject.layer != 22)
			{
				Transform[] componentsInChildren = MonoSingleton<FistControl>.Instance.heldObject.GetComponentsInChildren<Transform>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].gameObject.layer = 22;
				}
			}
			return;
		}
		MonoSingleton<FistControl>.Instance.heldObject.hooked = false;
		if (MonoSingleton<FistControl>.Instance.heldObject.gameObject.layer != 13)
		{
			Transform[] componentsInChildren = MonoSingleton<FistControl>.Instance.heldObject.GetComponentsInChildren<Transform>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].gameObject.layer = 13;
			}
		}
	}

	private void LateUpdate()
	{
		if (state != 0 || returning)
		{
			hook.position = hookPoint;
			hook.up = throwDirection;
			hookModel.layer = 2;
		}
		else
		{
			hookModel.layer = 13;
		}
	}

	private void FixedUpdate()
	{
		if ((bool)caughtGrenade && caughtGrenade.playerRiding)
		{
			if (caughtObjects.Contains(caughtGrenade.rb))
			{
				caughtObjects.Remove(caughtGrenade.rb);
			}
			else
			{
				caughtObjects.Clear();
			}
			caughtGrenade = null;
		}
		if (state == HookState.Ready && returning)
		{
			Vector3 vector = hookPoint;
			hookPoint = Vector3.MoveTowards(hookPoint, hand.position, Time.fixedDeltaTime * (100f + returnDistance / 2f));
			for (int num = caughtObjects.Count - 1; num >= 0; num--)
			{
				if (caughtObjects[num] != null)
				{
					caughtObjects[num].position = hookPoint;
				}
				else
				{
					caughtObjects.RemoveAt(num);
				}
			}
			if (hookPoint == hand.position)
			{
				lr.enabled = false;
				returning = false;
				anim.Play("Catch", -1, 0f);
				UnityEngine.Object.Instantiate(catchSound);
				aud.Stop();
				if (caughtObjects.Count > 0)
				{
					for (int num2 = caughtObjects.Count - 1; num2 >= 0; num2--)
					{
						if (caughtObjects[num2] != null)
						{
							if (caughtObjects[num2].TryGetComponent<Grenade>(out var component))
							{
								component.transform.position = MonoSingleton<NewMovement>.Instance.transform.position;
								if (component.rocket && !MonoSingleton<NewMovement>.Instance.ridingRocket && Vector3.Angle(Vector3.down, vector - MonoSingleton<NewMovement>.Instance.transform.position) < 45f)
								{
									component.PlayerRideStart();
								}
								else
								{
									component.Explode(big: false, harmless: false, component.rocket && !MonoSingleton<NewMovement>.Instance.gc.onGround);
								}
							}
							else
							{
								caughtObjects[num2].velocity = Vector3.zero;
							}
						}
					}
					caughtObjects.Clear();
				}
				caughtGrenade = null;
				caughtCannonball = null;
			}
		}
		if (state == HookState.Throwing)
		{
			if (!MonoSingleton<InputManager>.Instance.InputSource.Hook.IsPressed && (cooldown <= 0.1f || caughtObjects.Count > 0))
			{
				StopThrow();
			}
			else
			{
				float num3 = 250f * Time.fixedDeltaTime;
				bool flag = false;
				if (Physics.Raycast(hookPoint, throwDirection, out var hitInfo, num3, enviroMask, QueryTriggerInteraction.Ignore))
				{
					flag = true;
					num3 = hitInfo.distance;
				}
				RaycastHit[] array = Physics.SphereCastAll(hookPoint, Mathf.Min(Vector3.Distance(base.transform.position, hookPoint) / 15f, 5f), throwDirection, num3, throwMask, QueryTriggerInteraction.Collide);
				Array.Sort(array, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
				bool flag2 = false;
				for (int i = 0; i < array.Length; i++)
				{
					RaycastHit rhit = array[i];
					bool flag3 = false;
					switch (rhit.transform.gameObject.layer)
					{
					case 26:
						if (!rhit.collider.isTrigger)
						{
							StopThrow();
							UnityEngine.Object.Instantiate(clinkSparks, rhit.point, Quaternion.LookRotation(rhit.normal));
							flag2 = true;
							flag3 = true;
						}
						goto default;
					case 14:
						if (caughtObjects.Count < 5 && MonoSingleton<GrenadeList>.Instance.HasTransform(rhit.transform) && rhit.collider.attachedRigidbody != null && !caughtObjects.Contains(rhit.collider.attachedRigidbody))
						{
							if (MonoSingleton<GrenadeList>.Instance.grenadeList.Count > 0 && (bool)MonoSingleton<GrenadeList>.Instance.GetGrenade(rhit.transform) && MonoSingleton<GrenadeList>.Instance.GetGrenade(rhit.transform).rocket && !MonoSingleton<GrenadeList>.Instance.GetGrenade(rhit.transform).playerRiding)
							{
								caughtObjects.Add(rhit.collider.attachedRigidbody);
								UnityEngine.Object.Instantiate(clinkObjectSparks, rhit.point, Quaternion.LookRotation(rhit.normal));
								caughtGrenade = MonoSingleton<GrenadeList>.Instance.GetGrenade(rhit.transform);
								caughtGrenade.rideable = true;
							}
							else if (MonoSingleton<GrenadeList>.Instance.cannonballList.Count > 0 && (bool)MonoSingleton<GrenadeList>.Instance.GetCannonball(rhit.transform) && MonoSingleton<GrenadeList>.Instance.GetCannonball(rhit.transform).physicsCannonball)
							{
								Cannonball cannonball = MonoSingleton<GrenadeList>.Instance.GetCannonball(rhit.transform);
								caughtObjects.Add(rhit.collider.attachedRigidbody);
								UnityEngine.Object.Instantiate(clinkObjectSparks, rhit.point, Quaternion.LookRotation(rhit.normal));
								caughtCannonball = cannonball;
								cannonball.Unlaunch();
								cannonball.forceMaxSpeed = true;
								cannonball.InstaBreakDefenceCancel();
							}
						}
						goto default;
					case 16:
					{
						if (rhit.collider.isTrigger && rhit.transform.TryGetComponent<BulletCheck>(out var component6))
						{
							component6.ForceDodge();
						}
						flag3 = true;
						goto default;
					}
					case 11:
					case 12:
					{
						if (Physics.Raycast(hookPoint, rhit.collider.bounds.center - hookPoint, Vector3.Distance(hookPoint, rhit.collider.bounds.center), enviroMask, QueryTriggerInteraction.Ignore))
						{
							continue;
						}
						caughtEid = rhit.transform.GetComponentInParent<EnemyIdentifier>();
						if ((bool)caughtEid && caughtEid.enemyType == EnemyType.MaliciousFace && caughtEid.dead && !rhit.collider.Raycast(new Ray(hookPoint, throwDirection), out var _, num3))
						{
							caughtEid = null;
							continue;
						}
						if (caughtEid == null && rhit.transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component5))
						{
							caughtEid = component5.eid;
						}
						if ((bool)caughtEid)
						{
							if (caughtEid.hookIgnore)
							{
								caughtEid = null;
								goto default;
							}
							if ((bool)caughtCannonball && caughtCannonball.hitEnemies.Contains(caughtEid))
							{
								caughtEid = null;
								flag2 = true;
								StopThrow();
								return;
							}
							if (caughtEid.blessed)
							{
								caughtEid.hitter = "hook";
								caughtEid.DeliverDamage(caughtEid.gameObject, Vector3.zero, rhit.point, 1f, tryForExplode: false);
								caughtEid = null;
								continue;
							}
							if (caughtEid.enemyType == EnemyType.Idol)
							{
								caughtEid.hitter = "hook";
								caughtEid.DeliverDamage(caughtEid.gameObject, Vector3.zero, rhit.point, 1f, tryForExplode: false);
								UnityEngine.Object.Instantiate(clinkObjectSparks, rhit.point, Quaternion.LookRotation(rhit.normal));
								continue;
							}
							caughtEid.hitter = "hook";
							caughtEid.hooked = true;
							if (((caughtEid.enemyType != EnemyType.Drone && caughtEid.enemyType != EnemyType.Virtue) || !caughtEid.dead) && caughtEid.enemyType != EnemyType.Stalker)
							{
								caughtEid.DeliverDamage(caughtEid.gameObject, Vector3.zero, rhit.point, 0.2f, tryForExplode: false);
							}
							if (caughtEid == null)
							{
								return;
							}
							if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
							{
								GameObject gameObject = rhit.transform.gameObject;
								if (rhit.transform.gameObject.layer == 12)
								{
									EnemyIdentifierIdentifier componentInChildren = gameObject.GetComponentInChildren<EnemyIdentifierIdentifier>();
									if ((bool)componentInChildren)
									{
										gameObject = componentInChildren.gameObject;
									}
								}
								MonoSingleton<FistControl>.Instance.heldObject.SendMessage("HitWith", gameObject, SendMessageOptions.DontRequireReceiver);
							}
							if (caughtEid.dead)
							{
								if (!deadIgnoreTypes.Contains(caughtEid.enemyType))
								{
									goto default;
								}
								if (caughtEid.enemyType == EnemyType.Virtue || lightEnemies.Contains(caughtEid.enemyType))
								{
									lightTarget = true;
								}
								caughtEid = null;
							}
							else if (lightEnemies.Contains(caughtEid.enemyType))
							{
								lightTarget = true;
							}
						}
						flag2 = true;
						flag3 = true;
						caughtTransform = rhit.transform;
						hookPoint = rhit.collider.bounds.center;
						caughtPoint = hookPoint - caughtTransform.position;
						state = HookState.Caught;
						caughtCollider = rhit.collider;
						aud.Stop();
						UnityEngine.Object.Instantiate(hitSound, rhit.point, Quaternion.identity);
						goto default;
					}
					case 10:
					{
						if (rhit.transform.gameObject.tag == "Coin" && rhit.transform.TryGetComponent<Coin>(out var component4))
						{
							rhit.transform.position = hookPoint + throwDirection.normalized * rhit.distance;
							component4.Bounce();
						}
						goto default;
					}
					case 22:
					{
						if (rhit.transform.TryGetComponent<HookPoint>(out var component2))
						{
							if (component2.active && Vector3.Distance(base.transform.position, rhit.transform.position) > 5f)
							{
								flag2 = true;
								flag3 = true;
								caughtTransform = rhit.transform;
								hookPoint = rhit.transform.position;
								caughtPoint = Vector3.zero;
								state = HookState.Caught;
								caughtCollider = rhit.collider;
								aud.Stop();
								caughtHook = component2;
								component2.Hooked();
								goto default;
							}
						}
						else if ((bool)MonoSingleton<FistControl>.Instance.currentPunch && !MonoSingleton<FistControl>.Instance.currentPunch.holding)
						{
							if (rhit.transform.TryGetComponent<ItemIdentifier>(out var component3))
							{
								if (Physics.Raycast(hookPoint, rhit.transform.position - hookPoint, Vector3.Distance(hookPoint, rhit.transform.position), enviroMask, QueryTriggerInteraction.Ignore))
								{
									continue;
								}
								if (component3.infiniteSource)
								{
									component3 = component3.CreateCopy();
								}
								flag2 = true;
								if (component3.ipz == null || component3.ipz.CheckDoorBounds(previousHookPoint, hookPoint, reverseBounds: false))
								{
									MonoSingleton<FistControl>.Instance.currentPunch.ForceHold(component3);
								}
								else
								{
									ItemGrabError(rhit);
								}
								previousHookPoint = hookPoint;
							}
						}
						else
						{
							ItemPlaceZone[] components = rhit.transform.GetComponents<ItemPlaceZone>();
							bool flag4 = false;
							ItemPlaceZone[] array2 = components;
							foreach (ItemPlaceZone itemPlaceZone in array2)
							{
								if (itemPlaceZone.acceptedItemType == MonoSingleton<FistControl>.Instance.heldObject.itemType && !itemPlaceZone.CheckDoorBounds(previousHookPoint, hookPoint, reverseBounds: true))
								{
									flag4 = true;
								}
							}
							if (components.Length != 0)
							{
								if (Physics.Raycast(hookPoint, rhit.transform.position - hookPoint, Vector3.Distance(hookPoint, rhit.transform.position), enviroMask, QueryTriggerInteraction.Ignore))
								{
									continue;
								}
								flag2 = true;
								if (!flag4)
								{
									MonoSingleton<FistControl>.Instance.currentPunch.PlaceHeldObject(components, rhit.transform);
								}
								else
								{
									ItemGrabError(rhit);
								}
								previousHookPoint = hookPoint;
							}
						}
						if (flag2)
						{
							flag3 = true;
							StopThrow();
						}
						else if (!Physics.Raycast(hookPoint, rhit.transform.position - hookPoint, Vector3.Distance(hookPoint, rhit.transform.position), enviroMask, QueryTriggerInteraction.Ignore))
						{
							flag3 = true;
						}
						goto default;
					}
					default:
						if (flag3 && (bool)MonoSingleton<FistControl>.Instance.heldObject)
						{
							MonoSingleton<FistControl>.Instance.heldObject.SendMessage("HitWith", rhit.transform.gameObject, SendMessageOptions.DontRequireReceiver);
						}
						if (!flag2)
						{
							continue;
						}
						break;
					}
					break;
				}
				Vector3 point = hookPoint;
				if (flag && !flag2)
				{
					if (hitInfo.transform.TryGetComponent<Breakable>(out var component7) && component7.weak && !component7.precisionOnly)
					{
						component7.Break();
					}
					if (hitInfo.transform.gameObject.TryGetComponent<SandboxProp>(out var component8))
					{
						component8.rigidbody.AddForceAtPosition(base.transform.forward * -100f, hitInfo.point, ForceMode.VelocityChange);
					}
					else
					{
						UnityEngine.Object.Instantiate(clinkSparks, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
					}
					point = hitInfo.point;
					StopThrow();
					flag2 = true;
				}
				if (!flag2 && Vector3.Distance(base.transform.position, hookPoint) > 300f)
				{
					StopThrow();
				}
				else if (!flag2)
				{
					hookPoint += throwDirection * num3;
					point = hookPoint;
				}
				for (int num4 = caughtObjects.Count - 1; num4 >= 0; num4--)
				{
					if (caughtObjects[num4] != null)
					{
						caughtObjects[num4].position = point;
						if (flag2)
						{
							if (caughtObjects[num4].TryGetComponent<Grenade>(out var component9))
							{
								component9.Explode();
							}
							else
							{
								caughtObjects.RemoveAt(num4);
							}
						}
					}
					else
					{
						caughtObjects.RemoveAt(num4);
					}
				}
			}
		}
		else if (state == HookState.Caught)
		{
			if (caughtEid != null && (caughtEid.dead || caughtEid.hookIgnore || caughtEid.blessed))
			{
				if (!caughtEid.dead || !deadIgnoreTypes.Contains(caughtEid.enemyType))
				{
					StopThrow();
					return;
				}
				SolveDeadIgnore();
			}
			else if (!caughtTransform || Physics.Raycast(hand.position, caughtTransform.position + caughtPoint - hand.position, Vector3.Distance(hand.position, caughtTransform.position + caughtPoint), enviroMask, QueryTriggerInteraction.Ignore))
			{
				StopThrow();
				return;
			}
			hookPoint = caughtTransform.position + caughtPoint;
			if (!MonoSingleton<InputManager>.Instance.InputSource.Hook.IsPressed)
			{
				anim.Play("Pull", -1, 0f);
				hand.transform.localPosition = new Vector3(-0.015f, 0.071f, 0.04f);
				state = HookState.Pulling;
				MonoSingleton<RumbleManager>.Instance.SetVibrationTracked("rumble.whiplash.pull", base.gameObject);
				currentWoosh = UnityEngine.Object.Instantiate(wooshSound);
				UnityEngine.Object.Instantiate(pullSound);
				aud.clip = pullLoop;
				aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
				aud.panStereo = -0.5f;
				aud.Play();
				if (!forcingGroundCheck && !lightTarget)
				{
					ForceGroundCheck();
				}
				else if (lightTarget)
				{
					Rigidbody component10;
					if ((bool)caughtEid)
					{
						if (enemyGroundCheck != null)
						{
							enemyGroundCheck.StopForceOff();
						}
						enemyGroundCheck = caughtEid.gce;
						if ((bool)enemyGroundCheck)
						{
							enemyGroundCheck.ForceOff();
						}
						enemyRigidbody = caughtEid.GetComponent<Rigidbody>();
					}
					else if (caughtTransform.TryGetComponent<Rigidbody>(out component10))
					{
						enemyRigidbody = component10;
					}
					else
					{
						StopThrow();
					}
					if (!MonoSingleton<NewMovement>.Instance.gc.touchingGround)
					{
						if (!MonoSingleton<UnderwaterController>.Instance.inWater)
						{
							MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(0f, 15f, 0f);
						}
						else
						{
							MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(0f, 5f, 0f);
						}
					}
				}
			}
		}
		if (state == HookState.Pulling)
		{
			if (!caughtTransform || !caughtCollider)
			{
				StopThrow(1f);
				return;
			}
			Vector3 vector2 = caughtTransform.position + caughtPoint - hand.position;
			if (Physics.Raycast(hand.position, vector2.normalized, out var hitInfo3, vector2.magnitude, enviroMask, QueryTriggerInteraction.Ignore))
			{
				bool flag5 = true;
				EnemyIdentifier component11 = hitInfo3.transform.GetComponent<EnemyIdentifier>();
				if ((bool)component11 && component11.blessed)
				{
					flag5 = false;
				}
				if (flag5)
				{
					StopThrow(1f);
					return;
				}
			}
			if (caughtEid != null && (caughtEid.dead || caughtEid.hookIgnore || caughtEid.blessed))
			{
				if (!caughtEid.dead || !deadIgnoreTypes.Contains(caughtEid.enemyType))
				{
					StopThrow(1f);
					return;
				}
				SolveDeadIgnore();
			}
			if ((bool)caughtEid && !MonoSingleton<UnderwaterController>.Instance.inWater && (!MonoSingleton<AssistController>.Instance || !MonoSingleton<AssistController>.Instance.majorEnabled || !MonoSingleton<AssistController>.Instance.disableWhiplashHardDamage))
			{
				if (MonoSingleton<NewMovement>.Instance.antiHp + Time.fixedDeltaTime * 66f <= 50f)
				{
					MonoSingleton<NewMovement>.Instance.ForceAddAntiHP(Time.fixedDeltaTime * 66f, silent: true, dontOverwriteHp: true);
				}
				else if (MonoSingleton<NewMovement>.Instance.antiHp <= 50f)
				{
					MonoSingleton<NewMovement>.Instance.ForceAntiHP(50f, silent: true, dontOverwriteHp: true);
				}
			}
			Vector3 vector3 = playerCollider.ClosestPoint(hookPoint);
			if (Vector3.Distance(vector3, caughtCollider.ClosestPoint(vector3)) < 0.25f)
			{
				if ((bool)enemyRigidbody)
				{
					if (enemyGroundCheck == null || enemyGroundCheck.touchingGround || ((bool)caughtEid && caughtEid.underwater))
					{
						enemyRigidbody.velocity = Vector3.zero;
					}
					else
					{
						enemyRigidbody.velocity = new Vector3(0f, 15f, 0f);
					}
				}
				bool flag6 = false;
				if ((bool)caughtHook)
				{
					if (caughtHook.slingShot)
					{
						flag6 = true;
					}
					caughtHook.Reached(MonoSingleton<NewMovement>.Instance.rb.velocity.normalized);
				}
				StopThrow(1f);
				if (!MonoSingleton<NewMovement>.Instance.gc.touchingGround && !flag6)
				{
					if (MonoSingleton<UnderwaterController>.Instance.inWater)
					{
						MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.zero;
					}
					else if (base.transform.position.y < hookPoint.y)
					{
						MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(0f, 15f + (hookPoint.y - base.transform.position.y) * 3f, 0f);
					}
					else
					{
						MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(0f, 15f, 0f);
					}
				}
				return;
			}
			if ((bool)caughtEid && (bool)enemyRigidbody && caughtEid.enemyType == EnemyType.Drone)
			{
				if (enemyRigidbody.isKinematic)
				{
					lightTarget = false;
				}
				else
				{
					lightTarget = true;
				}
			}
			if (lightTarget && forcingGroundCheck)
			{
				StopForceGroundCheck();
			}
			else if (!lightTarget && !forcingGroundCheck)
			{
				ForceGroundCheck();
			}
			if (lightTarget)
			{
				if (!enemyRigidbody)
				{
					StopThrow(1f);
					return;
				}
				hookPoint = caughtTransform.position + caughtPoint;
				if (enemyGroundCheck != null)
				{
					enemyRigidbody.velocity = (MonoSingleton<NewMovement>.Instance.transform.position - hookPoint).normalized * 60f;
					caughtEid.transform.LookAt(new Vector3(MonoSingleton<CameraController>.Instance.transform.position.x, caughtEid.transform.position.y, MonoSingleton<CameraController>.Instance.transform.position.z));
					return;
				}
				enemyRigidbody.velocity = (MonoSingleton<CameraController>.Instance.transform.position - hookPoint).normalized * 60f;
				if ((bool)caughtEid)
				{
					caughtEid.transform.LookAt(MonoSingleton<CameraController>.Instance.transform.position);
				}
				else
				{
					caughtTransform.LookAt(MonoSingleton<CameraController>.Instance.transform.position);
				}
			}
			else
			{
				hookPoint = caughtTransform.position + caughtPoint;
				beingPulled = true;
				if (!MonoSingleton<NewMovement>.Instance.boost || MonoSingleton<NewMovement>.Instance.sliding)
				{
					MonoSingleton<NewMovement>.Instance.rb.velocity = (hookPoint - MonoSingleton<NewMovement>.Instance.transform.position).normalized * 60f;
				}
			}
		}
		else
		{
			beingPulled = false;
		}
	}

	private void SolveDeadIgnore()
	{
		if (!caughtEid)
		{
			return;
		}
		switch (caughtEid.enemyType)
		{
		case EnemyType.Virtue:
			lightTarget = true;
			enemyRigidbody = caughtEid.GetComponent<Rigidbody>();
			break;
		case EnemyType.MaliciousFace:
		{
			EnemyIdentifierIdentifier[] componentsInChildren = caughtEid.GetComponentsInChildren<EnemyIdentifierIdentifier>();
			if (componentsInChildren.Length == 0)
			{
				break;
			}
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].gameObject.layer == 11)
				{
					caughtTransform = componentsInChildren[i].transform;
					break;
				}
			}
			break;
		}
		}
		caughtEid = null;
	}

	private void ItemGrabError(RaycastHit rhit)
	{
		UnityEngine.Object.Instantiate(errorSound);
		MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
		MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=red>ERROR: BLOCKING DOOR WOULD CLOSE</color>", "", "", 0, silent: true);
	}

	public void StopThrow(float animationTime = 0f, bool sparks = false)
	{
		MonoSingleton<RumbleManager>.Instance.StopVibration("rumble.whiplash.throw");
		MonoSingleton<RumbleManager>.Instance.StopVibration("rumble.whiplash.pull");
		if (animationTime == 0f)
		{
			UnityEngine.Object.Instantiate(pullSound);
			aud.clip = pullLoop;
			aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
			aud.panStereo = -0.5f;
			aud.Play();
		}
		else
		{
			UnityEngine.Object.Instantiate(pullDoneSound);
		}
		if (forcingGroundCheck)
		{
			StopForceGroundCheck();
		}
		if (lightTarget)
		{
			if ((bool)enemyGroundCheck)
			{
				enemyGroundCheck.StopForceOff();
			}
			lightTarget = false;
			enemyGroundCheck = null;
			enemyRigidbody = null;
		}
		if ((bool)caughtEid)
		{
			caughtEid.hooked = false;
			caughtEid = null;
		}
		if ((bool)caughtHook)
		{
			caughtHook.Unhooked();
			caughtHook = null;
		}
		if (sparks)
		{
			UnityEngine.Object.Instantiate(clinkSparks, hookPoint, Quaternion.LookRotation(base.transform.position - hookPoint));
		}
		state = HookState.Ready;
		anim.Play("Pull", -1, animationTime);
		hand.transform.localPosition = new Vector3(-0.015f, 0.071f, 0.04f);
		if (MonoSingleton<CameraController>.Instance.defaultFov > 105f)
		{
			hand.transform.localPosition += new Vector3(0.25f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f), 0f, 0.05f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 60f));
		}
		else if (MonoSingleton<CameraController>.Instance.defaultFov < 105f)
		{
			hand.transform.localPosition -= new Vector3(0.05f * ((105f - MonoSingleton<CameraController>.Instance.defaultFov) / 60f), 0.075f * ((105f - MonoSingleton<CameraController>.Instance.defaultFov) / 60f), 0.125f * ((105f - MonoSingleton<CameraController>.Instance.defaultFov) / 60f));
		}
		returnDistance = Mathf.Max(Vector3.Distance(base.transform.position, hookPoint), 25f);
		returning = true;
		throwWarp = 0f;
		if ((bool)currentWoosh)
		{
			UnityEngine.Object.Destroy(currentWoosh);
		}
	}

	public void Cancel()
	{
		if (forcingGroundCheck)
		{
			StopForceGroundCheck();
		}
		if (forcingFistControl)
		{
			MonoSingleton<FistControl>.Instance.forceNoHold--;
			forcingFistControl = false;
			if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
			{
				MonoSingleton<FistControl>.Instance.heldObject.gameObject.layer = 13;
				MonoSingleton<FistControl>.Instance.heldObject.hooked = false;
			}
		}
		if (caughtObjects.Count > 0)
		{
			foreach (Rigidbody caughtObject in caughtObjects)
			{
				if ((bool)caughtObject)
				{
					caughtObject.velocity = (MonoSingleton<NewMovement>.Instance.transform.position - caughtObject.transform.position).normalized * (100f + returnDistance / 2f);
					if (caughtObject.TryGetComponent<Cannonball>(out var component))
					{
						component.hitEnemies.Clear();
						component.forceMaxSpeed = false;
					}
				}
			}
			caughtObjects.Clear();
		}
		caughtGrenade = null;
		caughtCannonball = null;
		if (lightTarget)
		{
			if ((bool)enemyGroundCheck)
			{
				enemyGroundCheck.StopForceOff();
			}
			lightTarget = false;
			enemyGroundCheck = null;
			enemyRigidbody = null;
		}
		if ((bool)caughtEid)
		{
			caughtEid.hooked = false;
			caughtEid = null;
		}
		if ((bool)caughtHook)
		{
			caughtHook.Unhooked();
			caughtHook = null;
		}
		state = HookState.Ready;
		anim.Play("Idle", -1, 0f);
		returning = false;
		throwWarp = 0f;
		lr.enabled = false;
		hookPoint = hand.position;
		aud.Stop();
		if ((bool)MonoSingleton<FistControl>.Instance.currentPunch && MonoSingleton<FistControl>.Instance.currentPunch.holding)
		{
			MonoSingleton<FistControl>.Instance.ResetHeldItemPosition();
		}
		if ((bool)currentWoosh)
		{
			UnityEngine.Object.Destroy(currentWoosh);
		}
		model.SetActive(value: false);
	}

	public void CatchOver()
	{
		if (state != 0 || returning)
		{
			return;
		}
		if (forcingFistControl)
		{
			MonoSingleton<FistControl>.Instance.forceNoHold--;
			forcingFistControl = false;
			if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
			{
				MonoSingleton<FistControl>.Instance.heldObject.hooked = false;
			}
		}
		if ((bool)MonoSingleton<FistControl>.Instance.currentPunch && MonoSingleton<FistControl>.Instance.currentPunch.holding)
		{
			MonoSingleton<FistControl>.Instance.ResetHeldItemPosition();
		}
		model.SetActive(value: false);
	}

	private void ForceGroundCheck()
	{
		if (MonoSingleton<NewMovement>.Instance.sliding)
		{
			MonoSingleton<NewMovement>.Instance.StopSlide();
		}
		if ((bool)MonoSingleton<NewMovement>.Instance.ridingRocket)
		{
			MonoSingleton<NewMovement>.Instance.ridingRocket.PlayerRideEnd();
		}
		forcingGroundCheck = true;
		MonoSingleton<NewMovement>.Instance.gc.ForceOff();
		MonoSingleton<NewMovement>.Instance.slopeCheck.ForceOff();
	}

	private void StopForceGroundCheck()
	{
		forcingGroundCheck = false;
		MonoSingleton<NewMovement>.Instance.gc.StopForceOff();
		MonoSingleton<NewMovement>.Instance.slopeCheck.StopForceOff();
	}

	private void SemiBlockCheck()
	{
		if (Physics.Raycast(hand.position, caughtTransform.position + caughtPoint - hand.position, out var hitInfo, Vector3.Distance(hand.position, caughtTransform.position + caughtPoint), 2048, QueryTriggerInteraction.Ignore) && hitInfo.collider.transform != caughtCollider.transform)
		{
			semiBlocked = Mathf.MoveTowards(semiBlocked, 1f, Time.fixedDeltaTime);
			if (semiBlocked >= 1f)
			{
				StopThrow();
			}
		}
		else
		{
			semiBlocked = 0f;
		}
	}
}
