using UnityEngine;

public class Movement : MonoBehaviour
{
	public float walkSpeed;

	public float jumpPower;

	private bool jumpCooldown;

	private bool falling;

	private Rigidbody rb;

	private Vector3 movementDirection;

	private Vector3 airDirection;

	public float timeBetweenSteps;

	private float stepTime;

	private int currentStep;

	public Animator anim;

	private GameObject body;

	private Quaternion tempRotation;

	private GameObject forwardPoint;

	private GroundCheck gc;

	private WallCheck wc;

	private PlayerAnimations pa;

	private Vector3 wallJumpPos;

	private int currentWallJumps;

	private AudioSource aud;

	private AudioSource aud2;

	private AudioSource aud3;

	private int currentSound;

	public AudioClip[] jumpSounds;

	public AudioClip landingSound;

	public AudioClip finalWallJump;

	private void Awake()
	{
		QualitySettings.vSyncCount = 0;
		rb = GetComponent<Rigidbody>();
		aud = GetComponent<AudioSource>();
		anim = GetComponentInChildren<Animator>();
		body = GameObject.FindWithTag("Body");
		gc = GetComponentInChildren<GroundCheck>();
		wc = GetComponentInChildren<WallCheck>();
		aud2 = gc.GetComponent<AudioSource>();
		pa = GetComponentInChildren<PlayerAnimations>();
		aud3 = wc.GetComponent<AudioSource>();
		forwardPoint = GameObject.FindWithTag("Forward");
	}

	private void Update()
	{
		float axisRaw = Input.GetAxisRaw("Horizontal");
		float axisRaw2 = Input.GetAxisRaw("Vertical");
		if (gc.onGround != pa.onGround)
		{
			pa.onGround = gc.onGround;
		}
		if (!gc.onGround && rb.velocity.y < -10f)
		{
			falling = true;
		}
		if (!gc.onGround && rb.velocity.y < -20f)
		{
			if (rb.velocity.y > -120f)
			{
				aud3.pitch = rb.velocity.y * -1f / 80f;
			}
			else
			{
				aud3.pitch = 1.5f;
			}
			aud3.volume = rb.velocity.y * -1f / 80f;
		}
		else if (rb.velocity.y > -20f)
		{
			aud3.pitch = 0f;
			aud3.volume = 0f;
		}
		if (gc.onGround && falling)
		{
			falling = false;
			aud2.clip = landingSound;
			aud2.volume = 0.5f;
			aud2.Play();
		}
		movementDirection = (axisRaw * base.transform.right + axisRaw2 * base.transform.forward).normalized;
		if (gc.onGround)
		{
			aud.pitch = 1f;
			currentWallJumps = 0;
			movementDirection = new Vector3(movementDirection.x * walkSpeed * Time.deltaTime, 0f, movementDirection.z * walkSpeed * Time.deltaTime);
			rb.velocity = movementDirection;
			anim.SetBool("Run", value: false);
		}
		else
		{
			movementDirection = new Vector3(movementDirection.x * walkSpeed * Time.deltaTime, rb.velocity.y, movementDirection.z * walkSpeed * Time.deltaTime);
			airDirection.y = 0f;
			if ((movementDirection.x > 0f && rb.velocity.x < movementDirection.x) || (movementDirection.x < 0f && rb.velocity.x > movementDirection.x))
			{
				airDirection.x = movementDirection.x;
			}
			else
			{
				airDirection.x = 0f;
			}
			if ((movementDirection.z > 0f && rb.velocity.z < movementDirection.z) || (movementDirection.z < 0f && rb.velocity.z > movementDirection.z))
			{
				airDirection.z = movementDirection.z;
			}
			else
			{
				airDirection.z = 0f;
			}
			rb.AddForce(airDirection.normalized * 15000f * Time.deltaTime);
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame && gc.onGround && !jumpCooldown)
		{
			currentSound = Random.Range(0, jumpSounds.Length);
			aud.clip = jumpSounds[currentSound];
			aud.volume = 0.75f;
			aud.pitch = 1f;
			aud.Play();
			rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
			rb.AddForce(Vector3.up * jumpPower * 1000f);
			jumpCooldown = true;
			Invoke("JumpReady", 0.1f);
		}
		else if (MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame && !gc.onGround && wc.onWall && !jumpCooldown && currentWallJumps < 3)
		{
			currentWallJumps++;
			currentSound = Random.Range(0, jumpSounds.Length);
			aud.clip = jumpSounds[currentSound];
			aud.pitch += 0.25f;
			aud.volume = 0.75f;
			aud.Play();
			if (currentWallJumps == 3)
			{
				aud2.clip = finalWallJump;
				aud2.volume = 0.75f;
				aud2.Play();
			}
			wallJumpPos = base.transform.position - wc.poc;
			rb.velocity = new Vector3(0f, 0f, 0f);
			rb.AddForceAtPosition(wallJumpPos.normalized * 10000f, base.transform.position);
			rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
			jumpCooldown = true;
			Invoke("JumpReady", 0.1f);
		}
		if (axisRaw2 < 0f)
		{
			forwardPoint.transform.localPosition = new Vector3(axisRaw * -1f, body.transform.localPosition.y, axisRaw2 * -1f);
		}
		else if (axisRaw2 == 0f && axisRaw == 0f)
		{
			forwardPoint.transform.localPosition = new Vector3(0f, body.transform.localPosition.y, 1f);
		}
		else
		{
			forwardPoint.transform.localPosition = new Vector3(axisRaw, body.transform.localPosition.y, axisRaw2);
		}
		if (axisRaw2 > 0f)
		{
			anim.SetBool("WalkF", value: true);
			anim.SetBool("WalkB", value: false);
		}
		else if (axisRaw2 < 0f)
		{
			anim.SetBool("WalkB", value: true);
			anim.SetBool("WalkF", value: false);
		}
		else if (axisRaw != 0f)
		{
			anim.SetBool("WalkF", value: true);
			anim.SetBool("WalkB", value: false);
		}
		else if (axisRaw == 0f && axisRaw2 == 0f)
		{
			anim.SetBool("WalkF", value: false);
			anim.SetBool("WalkB", value: false);
		}
		body.transform.LookAt(forwardPoint.transform);
	}

	private void JumpReady()
	{
		jumpCooldown = false;
	}
}
