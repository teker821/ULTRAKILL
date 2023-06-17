using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.InputSystem;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PlatformerMovement : MonoSingleton<PlatformerMovement>
{
	public Transform platformerCamera;

	public Vector3 cameraTarget = new Vector3(0f, 7f, -5.5f);

	private Vector3 defaultCameraTarget = new Vector3(0f, 7f, -5.5f);

	public Vector3 cameraRotation = new Vector3(20f, 0f, 0f);

	private Vector3 defaultCameraRotation = new Vector3(20f, 0f, 0f);

	[HideInInspector]
	public List<CameraTargetInfo> cameraTargets = new List<CameraTargetInfo>();

	private bool cameraTrack = true;

	public bool freeCamera;

	[HideInInspector]
	public float rotationY;

	[HideInInspector]
	public float rotationX;

	public GroundCheck groundCheck;

	[SerializeField]
	private GroundCheck slopeCheck;

	public Transform playerModel;

	[HideInInspector]
	public Rigidbody rb;

	private AudioSource aud;

	private CapsuleCollider playerCollider;

	private Animator anim;

	[SerializeField]
	private AudioClip jumpSound;

	[SerializeField]
	private AudioClip dodgeSound;

	[SerializeField]
	private AudioClip bounceSound;

	[HideInInspector]
	public bool activated = true;

	private Vector3 movementDirection;

	private Vector3 movementDirection2;

	private Vector3 airDirection;

	private Vector3 dodgeDirection;

	private float walkSpeed = 600f;

	private float jumpPower = 80f;

	private bool boost;

	private float boostCharge = 300f;

	private float boostLeft;

	[SerializeField]
	private GameObject staminaFailSound;

	[SerializeField]
	private GameObject dodgeParticle;

	[SerializeField]
	private GameObject dashJumpSound;

	[HideInInspector]
	public bool sliding;

	private bool crouching;

	private bool slideEnding;

	private float preSlideSpeed;

	private float preSlideDelay;

	private float slideSafety;

	private float slideLength;

	[SerializeField]
	private GameObject slideStopSound;

	[SerializeField]
	private GameObject slideEffect;

	[SerializeField]
	private GameObject slideScrape;

	private GameObject currentSlideEffect;

	private GameObject currentSlideScrape;

	private bool jumping;

	private bool jumpCooldown;

	[HideInInspector]
	public CustomGroundProperties groundProperties;

	public Transform jumpShadow;

	private bool falling;

	private float fallSpeed;

	private float fallTime;

	public float slamForce;

	private bool spinning;

	private float spinJuice;

	private Vector3 spinDirection;

	private float spinSpeed;

	private float spinCooldown;

	public Transform holder;

	private int difficulty;

	[SerializeField]
	private GameObject spinZone;

	[SerializeField]
	private GameObject coinGet;

	private float coinTimer;

	private float coinPitch;

	private int queuedCoins;

	private float coinEffectTimer;

	public int extraHits;

	private bool invincible;

	private float blinkTimer;

	public GameObject[] protectors;

	private float superTimer;

	public GameObject protectorGet;

	public GameObject protectorLose;

	public GameObject protectorOof;

	private InputBinding rbSlide;

	private InputBinding dpadMove;

	[Header("Death Stuff")]
	[SerializeField]
	private Material burnMaterial;

	[SerializeField]
	private GameObject defaultBurnEffect;

	[SerializeField]
	private GameObject ashParticle;

	[SerializeField]
	private GameObject ashSound;

	private GameObject currentCorpse;

	[SerializeField]
	private GameObject fallSound;

	[HideInInspector]
	public bool dead;

	protected override void Awake()
	{
		base.Awake();
		rbSlide = new InputBinding("<Gamepad>/rightShoulder", null, null, null, null, "rbSlide");
		dpadMove = new InputBinding("<Gamepad>/dpad", null, null, null, null, "dpadMove");
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		aud = GetComponent<AudioSource>();
		playerCollider = GetComponent<CapsuleCollider>();
		anim = GetComponent<Animator>();
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		base.transform.position = MonoSingleton<NewMovement>.Instance.gc.transform.position;
		rb.velocity = MonoSingleton<NewMovement>.Instance.rb.velocity;
		if (MonoSingleton<PlayerTracker>.Instance.pmov == null)
		{
			MonoSingleton<PlayerTracker>.Instance.currentPlatformerPlayerPrefab = base.transform.parent.gameObject;
			MonoSingleton<PlayerTracker>.Instance.pmov = this;
			MonoSingleton<PlayerTracker>.Instance.ChangeToPlatformer();
		}
	}

	public void CheckItem()
	{
		if ((bool)MonoSingleton<FistControl>.Instance && (bool)MonoSingleton<FistControl>.Instance.heldObject)
		{
			MonoSingleton<FistControl>.Instance.heldObject.transform.SetParent(holder, worldPositionStays: true);
			MonoSingleton<FistControl>.Instance.ResetHeldItemPosition();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		PlayerInput inputSource = MonoSingleton<InputManager>.Instance.InputSource;
		inputSource.Jump.Action.ApplyBindingOverride("<Gamepad>/buttonSouth", "Gamepad");
		inputSource.Fire1.Action.ApplyBindingOverride("<Gamepad>/buttonWest", "Gamepad");
		inputSource.Dodge.Action.ApplyBindingOverride("<Gamepad>/buttonNorth", "Gamepad");
		inputSource.Slide.Action.ApplyBindingOverride("<Gamepad>/buttonEast", "Gamepad");
		inputSource.Slide.Action.AddBinding(rbSlide);
		inputSource.Move.Action.AddBinding(dpadMove);
		slideLength = 0f;
	}

	private void OnDisable()
	{
		if (base.gameObject.scene.isLoaded)
		{
			PlayerInput inputSource = MonoSingleton<InputManager>.Instance.InputSource;
			inputSource.Jump.Action.RemoveAllBindingOverrides();
			inputSource.Fire1.Action.RemoveAllBindingOverrides();
			inputSource.Dodge.Action.RemoveAllBindingOverrides();
			inputSource.Slide.Action.RemoveAllBindingOverrides();
			inputSource.Slide.Action.ChangeBinding(rbSlide).Erase();
			inputSource.Move.Action.ChangeBinding(dpadMove).Erase();
			cameraTargets.Clear();
			if (sliding)
			{
				StopSlide();
			}
		}
	}

	private void Update()
	{
		if (MonoSingleton<OptionsManager>.Instance.paused)
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		if (activated)
		{
			zero = MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>();
			movementDirection = Vector3.ClampMagnitude(zero.x * Vector3.right + zero.y * Vector3.forward, 1f);
			movementDirection = Quaternion.Euler(0f, platformerCamera.rotation.eulerAngles.y, 0f) * movementDirection;
		}
		else
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			movementDirection = Vector3.zero;
		}
		if (movementDirection.magnitude > 0f)
		{
			anim.SetBool("Running", value: true);
		}
		else
		{
			anim.SetBool("Running", value: false);
		}
		if (rb.velocity.y < -100f)
		{
			rb.velocity = new Vector3(rb.velocity.x, -100f, rb.velocity.z);
		}
		if (activated && MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame && !falling && !jumpCooldown)
		{
			Jump();
		}
		if (!groundCheck.onGround)
		{
			if (fallTime < 1f)
			{
				fallTime += Time.deltaTime * 5f;
				if (fallTime > 1f)
				{
					falling = true;
				}
			}
			else if (rb.velocity.y < -2f)
			{
				fallSpeed = rb.velocity.y;
			}
		}
		else
		{
			fallTime = 0f;
		}
		if (groundCheck.onGround && falling && !jumpCooldown)
		{
			falling = false;
			fallSpeed = 0f;
			groundCheck.heavyFall = false;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasPerformedThisFrame && groundCheck.onGround && activated && !sliding)
		{
			StartSlide();
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasPerformedThisFrame && !groundCheck.onGround && !sliding && !jumping && activated && Physics.Raycast(groundCheck.transform.position + base.transform.up, base.transform.up * -1f, out var _, 2f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			StartSlide();
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasCanceledThisFrame && sliding)
		{
			StopSlide();
		}
		if (sliding && activated)
		{
			slideLength += Time.deltaTime;
			if (currentSlideEffect != null)
			{
				currentSlideEffect.transform.position = base.transform.position + dodgeDirection * 10f;
			}
			if (slideSafety > 0f)
			{
				slideSafety -= Time.deltaTime * 5f;
			}
			if (groundCheck.onGround)
			{
				currentSlideScrape.transform.position = base.transform.position + dodgeDirection;
			}
			else
			{
				currentSlideScrape.transform.position = Vector3.one * 5000f;
			}
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Dodge.WasPerformedThisFrame && activated)
		{
			if ((bool)groundProperties && !groundProperties.canDash)
			{
				if (!groundProperties.silentDashFail)
				{
					Object.Instantiate(staminaFailSound);
				}
			}
			else if (boostCharge >= 100f)
			{
				if (sliding)
				{
					StopSlide();
				}
				boostLeft = 100f;
				boost = true;
				anim.Play("Dash", -1, 0f);
				dodgeDirection = movementDirection;
				if (dodgeDirection == Vector3.zero)
				{
					dodgeDirection = playerModel.forward;
				}
				Quaternion identity = Quaternion.identity;
				identity.SetLookRotation(dodgeDirection * -1f);
				Object.Instantiate(dodgeParticle, base.transform.position + Vector3.up * 2f + dodgeDirection * 10f, identity).transform.localScale *= 2f;
				if (!MonoSingleton<AssistController>.Instance.majorEnabled || !MonoSingleton<AssistController>.Instance.infiniteStamina)
				{
					boostCharge -= 100f;
				}
				aud.clip = dodgeSound;
				aud.volume = 1f;
				aud.pitch = 1f;
				aud.Play();
			}
			else
			{
				Object.Instantiate(staminaFailSound);
			}
		}
		if (boostCharge != 300f && !sliding && !spinning)
		{
			float num = 1f;
			if (difficulty == 1)
			{
				num = 1.5f;
			}
			else if (difficulty == 0)
			{
				num = 2f;
			}
			boostCharge = Mathf.MoveTowards(boostCharge, 300f, 70f * Time.deltaTime * num);
		}
		if (spinCooldown > 0f)
		{
			spinCooldown = Mathf.MoveTowards(spinCooldown, 0f, Time.deltaTime);
		}
		if (activated && !spinning && spinCooldown <= 0f && !MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.Punch.WasPerformedThisFrame) && !MonoSingleton<OptionsManager>.Instance.paused)
		{
			Spin();
		}
		if (spinning)
		{
			playerModel.Rotate(Vector3.up, Time.deltaTime * 3600f, Space.Self);
		}
		else if (movementDirection.magnitude != 0f || boost)
		{
			Quaternion quaternion = Quaternion.LookRotation(movementDirection);
			if (boost)
			{
				quaternion = Quaternion.LookRotation(dodgeDirection);
			}
			playerModel.rotation = Quaternion.RotateTowards(playerModel.rotation, quaternion, (Quaternion.Angle(playerModel.rotation, quaternion) + 20f) * 35f * movementDirection.magnitude * Time.deltaTime);
		}
		if (cameraTrack)
		{
			if (!freeCamera)
			{
				CheckCameraTarget();
				platformerCamera.transform.position = Vector3.MoveTowards(platformerCamera.position, base.transform.position + cameraTarget, Time.deltaTime * 15f * (0.1f + Vector3.Distance(platformerCamera.position, cameraTarget)));
				platformerCamera.transform.rotation = Quaternion.RotateTowards(platformerCamera.transform.rotation, Quaternion.Euler(cameraRotation), Time.deltaTime * 15f * (0.1f + Vector3.Distance(platformerCamera.rotation.eulerAngles, cameraRotation)));
			}
			else if (!MonoSingleton<OptionsManager>.Instance.paused)
			{
				platformerCamera.transform.position = base.transform.position + defaultCameraTarget;
				platformerCamera.transform.rotation = Quaternion.Euler(defaultCameraRotation);
				Vector2 vector = MonoSingleton<InputManager>.Instance.InputSource.Look.ReadValue<Vector2>();
				if (!MonoSingleton<CameraController>.Instance.reverseY)
				{
					rotationX += vector.y * (MonoSingleton<OptionsManager>.Instance.mouseSensitivity / 10f);
				}
				else
				{
					rotationX -= vector.y * (MonoSingleton<OptionsManager>.Instance.mouseSensitivity / 10f);
				}
				if (!MonoSingleton<CameraController>.Instance.reverseX)
				{
					rotationY += vector.x * (MonoSingleton<OptionsManager>.Instance.mouseSensitivity / 10f);
				}
				else
				{
					rotationY -= vector.x * (MonoSingleton<OptionsManager>.Instance.mouseSensitivity / 10f);
				}
				if (rotationY > 180f)
				{
					rotationY -= 360f;
				}
				else if (rotationY < -180f)
				{
					rotationY += 360f;
				}
				rotationX = Mathf.Clamp(rotationX, -69f, 109f);
				float num2 = 2.5f;
				if (sliding || Physics.Raycast(base.transform.position + Vector3.up * 0.625f, Vector3.up, 2.5f, LayerMaskDefaults.Get(LMD.Environment)))
				{
					num2 = 0.625f;
				}
				if (Input.GetKeyDown(KeyCode.L))
				{
					Debug.Log("Height: " + num2);
				}
				Vector3 vector2 = base.transform.position + Vector3.up * num2;
				platformerCamera.RotateAround(vector2, Vector3.left, rotationX);
				platformerCamera.RotateAround(vector2, Vector3.up, rotationY);
				if (Physics.SphereCast(vector2, 0.25f, platformerCamera.position - vector2, out var hitInfo2, Vector3.Distance(vector2, platformerCamera.position), LayerMaskDefaults.Get(LMD.Environment)))
				{
					platformerCamera.position = hitInfo2.point + 0.5f * hitInfo2.normal;
				}
			}
		}
		if (Physics.SphereCast(base.transform.position + Vector3.up, 0.5f, Vector3.down, out var hitInfo3, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			jumpShadow.position = hitInfo3.point + Vector3.up * 0.05f;
			jumpShadow.forward = hitInfo3.normal;
		}
		else
		{
			jumpShadow.position = base.transform.position - Vector3.up * 1000f;
			jumpShadow.forward = Vector3.up;
		}
		if (coinTimer > 0f)
		{
			coinTimer = Mathf.MoveTowards(coinTimer, 0f, Time.deltaTime);
		}
		if (coinEffectTimer > 0f)
		{
			coinEffectTimer = Mathf.MoveTowards(coinEffectTimer, 0f, Time.deltaTime);
		}
		else if (queuedCoins > 0)
		{
			CoinGetEffect();
		}
		if (invincible && extraHits < 3)
		{
			if (blinkTimer > 0f)
			{
				blinkTimer = Mathf.MoveTowards(blinkTimer, 0f, Time.deltaTime);
			}
			else
			{
				blinkTimer = 0.05f;
				if (playerModel.gameObject.activeSelf)
				{
					playerModel.gameObject.SetActive(value: false);
				}
				else
				{
					playerModel.gameObject.SetActive(value: true);
				}
			}
		}
		if (superTimer > 0f)
		{
			if (!NoWeaponCooldown.NoCooldown)
			{
				superTimer = Mathf.MoveTowards(superTimer, 0f, Time.deltaTime);
			}
			if (superTimer == 0f)
			{
				GetHit();
			}
		}
	}

	private void CheckCameraTarget(bool instant = false)
	{
		Vector3 position = defaultCameraTarget;
		Vector3 rotation = defaultCameraRotation;
		if (cameraTargets.Count > 0)
		{
			for (int num = cameraTargets.Count - 1; num >= 0; num--)
			{
				if ((bool)cameraTargets[num].caller && cameraTargets[num].caller.activeInHierarchy)
				{
					position = cameraTargets[num].position;
					rotation = cameraTargets[num].rotation;
					break;
				}
				cameraTargets.RemoveAt(num);
			}
		}
		if (instant)
		{
			cameraTarget = position;
			cameraRotation = rotation;
		}
		else
		{
			cameraTarget = Vector3.MoveTowards(cameraTarget, position, Time.deltaTime * 2f * (0.1f + Vector3.Distance(cameraTarget, position)));
			cameraRotation = Vector3.MoveTowards(cameraRotation, rotation, Time.deltaTime * 2f * (0.1f + Vector3.Distance(cameraRotation, rotation)));
		}
	}

	private void FixedUpdate()
	{
		SlideValues();
		if (boost || spinning)
		{
			rb.useGravity = true;
			Dodge();
			return;
		}
		base.gameObject.layer = 2;
		if (groundCheck.onGround && !jumping)
		{
			anim.SetBool("InAir", value: false);
			float y = rb.velocity.y;
			if (slopeCheck.onGround && movementDirection.x == 0f && movementDirection.z == 0f)
			{
				y = 0f;
				rb.useGravity = false;
			}
			else
			{
				rb.useGravity = true;
			}
			float num = 2.75f;
			if ((bool)groundProperties)
			{
				num *= groundProperties.speedMultiplier;
			}
			movementDirection2 = new Vector3(movementDirection.x * walkSpeed * Time.deltaTime * num, y, movementDirection.z * walkSpeed * Time.deltaTime * num);
			float num2 = 2.5f;
			Vector3 zero = Vector3.zero;
			if ((bool)groundProperties)
			{
				num2 *= groundProperties.friction;
				if (groundProperties.push)
				{
					Vector3 vector = groundProperties.pushForce;
					if (groundProperties.pushDirectionRelative)
					{
						vector = groundProperties.transform.rotation * vector;
					}
					zero += vector;
				}
			}
			rb.velocity = Vector3.MoveTowards(rb.velocity, movementDirection2 + zero, num2);
			return;
		}
		anim.SetBool("InAir", value: true);
		rb.useGravity = true;
		movementDirection2 = new Vector3(movementDirection.x * walkSpeed * Time.deltaTime * 2.75f, rb.velocity.y, movementDirection.z * walkSpeed * Time.deltaTime * 2.75f);
		airDirection.y = 0f;
		if ((movementDirection2.x > 0f && rb.velocity.x < movementDirection2.x) || (movementDirection2.x < 0f && rb.velocity.x > movementDirection2.x))
		{
			airDirection.x = movementDirection2.x;
		}
		else
		{
			airDirection.x = 0f;
		}
		if ((movementDirection2.z > 0f && rb.velocity.z < movementDirection2.z) || (movementDirection2.z < 0f && rb.velocity.z > movementDirection2.z))
		{
			airDirection.z = movementDirection2.z;
		}
		else
		{
			airDirection.z = 0f;
		}
		rb.AddForce(airDirection.normalized * 6000f);
		LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
		if (rb.velocity.y < 0f)
		{
			layerMask = (int)layerMask | 0x1000;
		}
		if (!Physics.SphereCast(base.transform.position + Vector3.up * 2.5f * base.transform.localScale.y, (base.transform.localScale.x + base.transform.localScale.z) / 2f * 0.75f - 0.1f, Vector3.up * rb.velocity.y, out var hitInfo, 2.51f + rb.velocity.y * Time.fixedDeltaTime, layerMask))
		{
			return;
		}
		EnemyIdentifier component3;
		if (hitInfo.transform.gameObject.layer == 8 || hitInfo.transform.gameObject.layer == 24)
		{
			EnemyIdentifier component2;
			if (hitInfo.transform.TryGetComponent<Breakable>(out var component) && component.crate)
			{
				if (component.bounceHealth > 1)
				{
					aud.clip = bounceSound;
					aud.pitch = Mathf.Lerp(1f, 2f, (float)(component.originalBounceHealth - component.bounceHealth) / (float)component.originalBounceHealth);
					aud.volume = 0.75f;
					aud.Play();
				}
				component.Bounce();
				if (base.transform.position.y < hitInfo.transform.position.y)
				{
					rb.velocity = new Vector3(MonoSingleton<PlatformerMovement>.Instance.rb.velocity.x, -10f, MonoSingleton<PlatformerMovement>.Instance.rb.velocity.z);
				}
				else if (MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
				{
					Jump(silent: true, 1.35f);
				}
				else
				{
					Jump(silent: true, 0.75f);
				}
			}
			else if (hitInfo.transform.gameObject.CompareTag("Armor") && hitInfo.transform.TryGetComponent<EnemyIdentifier>(out component2))
			{
				component2.InstaKill();
			}
		}
		else if (hitInfo.transform.TryGetComponent<EnemyIdentifier>(out component3) && !component3.dead)
		{
			if (!component3.blessed)
			{
				component3.Splatter();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
			{
				Jump(silent: true, 1.25f);
			}
			else
			{
				Jump(silent: true, 0.75f);
			}
		}
	}

	public void Jump(bool silent = false, float multiplier = 1f)
	{
		float num = 1500f * multiplier;
		if ((bool)groundProperties)
		{
			if (!groundProperties.canJump)
			{
				if (!groundProperties.silentJumpFail)
				{
					aud.clip = jumpSound;
					aud.volume = 0.75f;
					aud.pitch = 0.25f;
					aud.Play();
				}
				return;
			}
			num *= groundProperties.jumpForceMultiplier;
		}
		anim.SetBool("InAir", value: true);
		anim.Play("Jump");
		falling = true;
		jumping = true;
		Invoke("NotJumping", 0.25f);
		if (!silent)
		{
			aud.clip = jumpSound;
			if (groundCheck.superJumpChance > 0f)
			{
				aud.volume = 0.85f;
				aud.pitch = 2f;
			}
			else
			{
				aud.volume = 0.75f;
				aud.pitch = 1f;
			}
			aud.Play();
		}
		rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
		if (sliding)
		{
			rb.AddForce(Vector3.up * jumpPower * num * 2f);
			StopSlide();
		}
		else if (boost)
		{
			if (boostCharge >= 100f)
			{
				if (!MonoSingleton<AssistController>.Instance.majorEnabled || !MonoSingleton<AssistController>.Instance.infiniteStamina)
				{
					boostCharge -= 100f;
				}
				Object.Instantiate(dashJumpSound);
			}
			else
			{
				rb.velocity = new Vector3(movementDirection.x * walkSpeed * Time.deltaTime * 2.75f, 0f, movementDirection.z * walkSpeed * Time.deltaTime * 2.75f);
				Object.Instantiate(staminaFailSound);
			}
			rb.AddForce(Vector3.up * jumpPower * num * 1.5f);
		}
		else if (groundCheck.superJumpChance > 0f || groundCheck.extraJumpChance > 0f)
		{
			if (slamForce < 5.5f)
			{
				rb.AddForce(Vector3.up * jumpPower * num * (3f + (slamForce - 1f)));
			}
			else
			{
				rb.AddForce(Vector3.up * jumpPower * num * 12.5f);
			}
			slamForce = 0f;
		}
		else
		{
			rb.AddForce(Vector3.up * jumpPower * num * 2.6f);
		}
		jumpCooldown = true;
		Invoke("JumpReady", 0.2f);
		boost = false;
	}

	private void Dodge()
	{
		if (spinning)
		{
			movementDirection2 = new Vector3(movementDirection.x * spinSpeed, rb.velocity.y, movementDirection.z * spinSpeed);
			if (movementDirection.magnitude == 0f && !falling)
			{
				rb.velocity = new Vector3(Mathf.MoveTowards(rb.velocity.x, 0f, Time.fixedDeltaTime * 150f), rb.velocity.y, Mathf.MoveTowards(rb.velocity.z, 0f, Time.fixedDeltaTime * 150f));
			}
			else
			{
				airDirection.y = 0f;
				if ((movementDirection2.x > 0f && rb.velocity.x < movementDirection2.x) || (movementDirection2.x < 0f && rb.velocity.x > movementDirection2.x))
				{
					airDirection.x = movementDirection2.x;
				}
				else
				{
					airDirection.x = 0f;
				}
				if ((movementDirection2.z > 0f && rb.velocity.z < movementDirection2.z) || (movementDirection2.z < 0f && rb.velocity.z > movementDirection2.z))
				{
					airDirection.z = movementDirection2.z;
				}
				else
				{
					airDirection.z = 0f;
				}
				if (falling)
				{
					rb.AddForce(airDirection.normalized * 4000f);
				}
				else
				{
					rb.AddForce(airDirection.normalized * 24000f);
				}
			}
			spinJuice = Mathf.MoveTowards(spinJuice, 0f, Time.fixedDeltaTime * 3f);
			if (spinJuice <= 0f)
			{
				StopSpin();
			}
			return;
		}
		if (sliding)
		{
			float num = 1f;
			if (preSlideSpeed > 1f)
			{
				if (preSlideSpeed > 3f)
				{
					preSlideSpeed = 3f;
				}
				num = preSlideSpeed;
				preSlideSpeed -= Time.fixedDeltaTime * preSlideSpeed;
				preSlideDelay = 0f;
			}
			if ((bool)groundProperties)
			{
				if (!groundProperties.canSlide)
				{
					StopSlide();
					return;
				}
				num *= groundProperties.speedMultiplier;
			}
			Vector3 velocity = new Vector3(dodgeDirection.x * walkSpeed * Time.fixedDeltaTime * 5f * num, rb.velocity.y, dodgeDirection.z * walkSpeed * Time.fixedDeltaTime * 5f * num);
			if ((bool)groundProperties && groundProperties.push)
			{
				Vector3 vector = groundProperties.pushForce;
				if (groundProperties.pushDirectionRelative)
				{
					vector = groundProperties.transform.rotation * vector;
				}
				velocity += vector;
			}
			rb.velocity = velocity;
			return;
		}
		float y = 0f;
		if (slideEnding)
		{
			y = rb.velocity.y;
		}
		float num2 = 2.25f;
		movementDirection2 = new Vector3(dodgeDirection.x * walkSpeed * Time.fixedDeltaTime * num2, y, dodgeDirection.z * walkSpeed * Time.fixedDeltaTime * num2);
		if (!slideEnding || groundCheck.onGround)
		{
			rb.velocity = movementDirection2 * 3f;
		}
		base.gameObject.layer = 15;
		boostLeft -= 4f;
		if (boostLeft <= 0f)
		{
			boost = false;
			if (!groundCheck.onGround && !slideEnding)
			{
				rb.velocity = movementDirection2;
			}
		}
		slideEnding = false;
	}

	private void Spin()
	{
		anim.Play("Spin", -1, 0f);
		anim.SetBool("Spinning", value: true);
		spinning = true;
		spinJuice = 1f;
		spinZone.SetActive(value: true);
		if (sliding)
		{
			float num = 1f;
			if (preSlideSpeed > 1f)
			{
				if (preSlideSpeed > 3f)
				{
					preSlideSpeed = 3f;
				}
				num = preSlideSpeed;
			}
			if ((bool)groundProperties)
			{
				num *= groundProperties.speedMultiplier;
			}
			spinDirection = dodgeDirection;
			spinSpeed = walkSpeed * 5f * num * Time.fixedDeltaTime;
			StopSlide();
			boostLeft = 0f;
			boost = false;
		}
		else if (boost)
		{
			spinDirection = dodgeDirection;
			spinSpeed = walkSpeed * 8.25f * Time.fixedDeltaTime;
			boostLeft = 0f;
			boost = false;
		}
		else
		{
			Vector3 velocity = rb.velocity;
			velocity += movementDirection * walkSpeed * Time.fixedDeltaTime;
			velocity.y = 0f;
			if (velocity.magnitude <= 0.25f)
			{
				spinDirection = playerModel.forward;
			}
			else
			{
				spinDirection = velocity;
			}
			spinSpeed = velocity.magnitude;
		}
		rb.velocity = new Vector3(spinDirection.normalized.x * spinSpeed, rb.velocity.y, spinDirection.normalized.z * spinSpeed);
	}

	private void StopSpin()
	{
		spinning = false;
		anim.SetBool("Spinning", value: false);
		spinJuice = 0f;
		playerModel.forward = spinDirection;
		spinCooldown = 0.2f;
		spinZone.SetActive(value: false);
	}

	private void StartSlide()
	{
		slideLength = 0f;
		anim.SetBool("Sliding", value: true);
		if (currentSlideEffect != null)
		{
			Object.Destroy(currentSlideEffect);
		}
		if (currentSlideScrape != null)
		{
			Object.Destroy(currentSlideScrape);
		}
		if ((bool)groundProperties && !groundProperties.canSlide)
		{
			if (!groundProperties.silentSlideFail)
			{
				StopSlide();
			}
			return;
		}
		playerCollider.height = 1.25f;
		playerCollider.center = Vector3.up * 0.625f;
		slideSafety = 1f;
		sliding = true;
		boost = true;
		dodgeDirection = movementDirection;
		if (dodgeDirection == Vector3.zero)
		{
			dodgeDirection = playerModel.forward;
		}
		Quaternion identity = Quaternion.identity;
		identity.SetLookRotation(dodgeDirection * -1f);
		currentSlideEffect = Object.Instantiate(slideEffect, base.transform.position + dodgeDirection * 10f, identity);
		currentSlideScrape = Object.Instantiate(slideScrape, base.transform.position + dodgeDirection * 2f, identity);
	}

	public void StopSlide()
	{
		anim.SetBool("Sliding", value: false);
		if (currentSlideEffect != null)
		{
			Object.Destroy(currentSlideEffect);
		}
		if (currentSlideScrape != null)
		{
			Object.Destroy(currentSlideScrape);
		}
		if (sliding)
		{
			Object.Instantiate(slideStopSound);
		}
		sliding = false;
		slideEnding = true;
		if (slideLength > MonoSingleton<NewMovement>.Instance.longestSlide)
		{
			MonoSingleton<NewMovement>.Instance.longestSlide = slideLength;
		}
		slideLength = 0f;
		if (!crouching)
		{
			playerCollider.height = 5f;
			playerCollider.center = Vector3.up * 2.5f;
		}
	}

	private void SlideValues()
	{
		if (sliding && slideSafety <= 0f)
		{
			Vector3 vector = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
			float num = 10f;
			if ((bool)groundProperties && groundProperties.speedMultiplier < 1f)
			{
				num *= groundProperties.speedMultiplier;
			}
			if (vector.magnitude < num)
			{
				slideSafety = Mathf.MoveTowards(slideSafety, -0.1f, Time.deltaTime);
				if (slideSafety <= -0.1f)
				{
					StopSlide();
				}
			}
			else
			{
				slideSafety = 0f;
			}
		}
		if (sliding || !activated)
		{
			return;
		}
		if (groundCheck.heavyFall)
		{
			preSlideDelay = 0.2f;
			preSlideSpeed = slamForce;
			if (Physics.SphereCast(base.transform.position - Vector3.up * 1.5f, 0.35f, Vector3.down, out var hitInfo, Time.fixedDeltaTime * Mathf.Abs(rb.velocity.y), LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
			{
				base.transform.position = hitInfo.point + Vector3.up * 1.5f;
				rb.velocity = Vector3.zero;
			}
		}
		else if (!boost && !falling && rb.velocity.magnitude / 24f > preSlideSpeed)
		{
			preSlideSpeed = rb.velocity.magnitude / 24f;
			preSlideDelay = 0.2f;
		}
		else
		{
			preSlideDelay = Mathf.MoveTowards(preSlideDelay, 0f, Time.fixedDeltaTime);
			if (preSlideDelay <= 0f)
			{
				preSlideDelay = 0.2f;
				preSlideSpeed = rb.velocity.magnitude / 24f;
			}
		}
	}

	public void EmptyStamina()
	{
		boostCharge = 0f;
	}

	public void FullStamina()
	{
		boostCharge = 300f;
	}

	private void JumpReady()
	{
		jumpCooldown = false;
	}

	private void NotJumping()
	{
		jumping = false;
	}

	public void AddExtraHit(int amount = 1)
	{
		extraHits = Mathf.Clamp(extraHits + amount, 0, 3);
		CheckProtector();
		Object.Instantiate(protectorGet, playerCollider.bounds.center, Quaternion.identity, base.transform);
		if (extraHits >= 3)
		{
			invincible = true;
			playerModel.gameObject.SetActive(value: true);
			superTimer = 20f;
		}
	}

	private void CheckProtector()
	{
		extraHits = Mathf.Clamp(extraHits, 0, 3);
		for (int i = 0; i <= 2; i++)
		{
			if (i == extraHits - 1)
			{
				protectors[i].SetActive(value: true);
			}
			else
			{
				protectors[i].SetActive(value: false);
			}
		}
	}

	private void GetHit()
	{
		MonoSingleton<StatsManager>.Instance.tookDamage = true;
		extraHits--;
		CheckProtector();
		Object.Instantiate(protectorLose, playerCollider.bounds.center, Quaternion.identity, base.transform);
		invincible = true;
		Invoke("StopInvincibility", 3f);
	}

	private void StopInvincibility()
	{
		playerModel.gameObject.SetActive(value: true);
		invincible = false;
	}

	private void Death()
	{
		cameraTrack = false;
		dead = true;
		MonoSingleton<StatsManager>.Instance.tookDamage = true;
		if (extraHits > 0)
		{
			extraHits = 0;
			CheckProtector();
		}
		if (!freeCamera)
		{
			platformerCamera.transform.position = base.transform.position + cameraTarget;
		}
		if (boost || spinning)
		{
			StopSpin();
			boost = false;
		}
	}

	public void Fall()
	{
		if (!dead)
		{
			Death();
			Object.Instantiate(fallSound, base.transform.position, Quaternion.identity);
			Invoke("DeathOver", 2f);
		}
	}

	public void Explode(bool ignoreInvincible = false)
	{
		if (dead || (!ignoreInvincible && invincible))
		{
			if (!dead && extraHits == 3)
			{
				Object.Instantiate(protectorOof, playerCollider.bounds.center, Quaternion.identity, base.transform);
				Jump(silent: true);
			}
			return;
		}
		if (!ignoreInvincible && extraHits > 0)
		{
			GetHit();
			return;
		}
		Death();
		GoreZone goreZone = GoreZone.ResolveGoreZone(base.transform);
		GameObject gameObject = Object.Instantiate(MonoSingleton<BloodsplatterManager>.Instance.head, playerCollider.bounds.center, Quaternion.identity, goreZone.goreZone);
		Transform[] componentsInChildren = playerModel.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.childCount <= 0 && !(Random.Range(0f, 1f) > 0.5f))
			{
				gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Body);
				if (!gameObject)
				{
					break;
				}
				gameObject.transform.parent = goreZone.goreZone;
				gameObject.transform.position = transform.position;
				gameObject.SetActive(value: true);
				gameObject = null;
			}
		}
		base.gameObject.SetActive(value: false);
		Invoke("DeathOver", 2f);
	}

	public void Burn(bool ignoreInvincible = false)
	{
		if (dead || (!ignoreInvincible && invincible))
		{
			if (!dead && extraHits == 3)
			{
				Object.Instantiate(protectorOof, playerCollider.bounds.center, Quaternion.identity, base.transform);
				Jump(silent: true);
			}
			return;
		}
		if (!ignoreInvincible && extraHits > 0)
		{
			GetHit();
			return;
		}
		if (currentCorpse != null)
		{
			CancelInvoke();
			Object.Destroy(currentCorpse);
		}
		Death();
		if ((bool)defaultBurnEffect)
		{
			Object.Instantiate(defaultBurnEffect, base.transform.position, Quaternion.identity);
		}
		currentCorpse = Object.Instantiate(playerModel.gameObject, playerModel.position, playerModel.rotation);
		base.gameObject.SetActive(value: false);
		SandboxUtils.StripForPreview(currentCorpse.transform, burnMaterial);
		Invoke("BecomeAsh", 1f);
	}

	private void BecomeAsh()
	{
		if ((bool)currentCorpse)
		{
			Object.Instantiate(ashSound, base.transform.position, Quaternion.identity);
			Transform[] componentsInChildren = currentCorpse.transform.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				Object.Instantiate(ashParticle, transform.position, Quaternion.identity);
			}
			Object.Destroy(currentCorpse);
			Invoke("DeathOver", 1f);
		}
	}

	private void DeathOver()
	{
		Respawn();
		MonoSingleton<StatsManager>.Instance.Restart();
	}

	public void Respawn()
	{
		cameraTrack = true;
		dead = false;
		jumping = false;
		jumpCooldown = false;
		extraHits = 0;
		boostCharge = 300f;
		rb.velocity = Vector3.zero;
		CancelInvoke();
		if ((bool)currentCorpse)
		{
			Object.Destroy(currentCorpse);
		}
		CheckProtector();
		StopInvincibility();
	}

	public void CoinGet()
	{
		queuedCoins++;
	}

	public void CoinGetEffect()
	{
		AudioSource component = Object.Instantiate(coinGet, playerCollider.bounds.center, Quaternion.identity).GetComponent<AudioSource>();
		if (coinTimer > 0f)
		{
			if (coinPitch < 1.35f)
			{
				coinPitch += 0.025f;
			}
			component.pitch = coinPitch;
		}
		else
		{
			coinPitch = 1f;
		}
		coinTimer = 1.5f;
		coinEffectTimer = 0.05f;
		queuedCoins--;
	}

	public void SnapCamera()
	{
		CheckCameraTarget(instant: true);
		platformerCamera.position = base.transform.position + cameraTarget;
		platformerCamera.rotation = Quaternion.Euler(cameraRotation);
	}

	public void SnapCamera(Vector3 targetPos, Vector3 targetRot)
	{
		cameraTarget = targetPos;
		platformerCamera.position = targetPos;
		cameraRotation = targetRot;
		platformerCamera.rotation = Quaternion.Euler(targetRot);
	}

	public void ResetCamera(float degreesY, float degreesX = 0f)
	{
		rotationY = degreesY;
		rotationX = degreesX;
	}
}
