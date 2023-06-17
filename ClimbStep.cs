using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class ClimbStep : MonoSingleton<ClimbStep>
{
	private InputManager inman;

	private Rigidbody rb;

	private int layerMask;

	private NewMovement newMovement;

	private float step = 2.1f;

	private float allowedAngle = 0.1f;

	private float allowedSpeed = 0.1f;

	private float allowedInput = 0.5f;

	private float cooldown;

	private float cooldownMax = 0.1f;

	private float deltaVertical;

	private float deltaHorizontal = 0.6f;

	private Vector3 position;

	private Vector3 gizmoPosition1;

	private Vector3 gizmoPosition2;

	private Vector3 movementDirection;

	private new void Awake()
	{
		rb = GetComponent<Rigidbody>();
		layerMask = LayerMask.GetMask("Environment", "Outdoors");
	}

	private void Start()
	{
		newMovement = MonoSingleton<NewMovement>.Instance;
		inman = MonoSingleton<InputManager>.Instance;
	}

	private void FixedUpdate()
	{
		if (cooldown <= 0f)
		{
			cooldown = 0f;
		}
		else
		{
			cooldown -= Time.deltaTime;
		}
		Vector2 vector = MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>();
		movementDirection = Vector3.ClampMagnitude(vector.x * base.transform.right + vector.y * base.transform.forward, 1f);
	}

	private void OnCollisionStay(Collision collisionInfo)
	{
		if (MonoSingleton<NewMovement>.Instance.gc.forcedOff > 0 || layerMask != (layerMask | (1 << collisionInfo.collider.gameObject.layer)) || cooldown != 0f)
		{
			return;
		}
		ContactPoint[] contacts = collisionInfo.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if ((!(rb.velocity.y < allowedSpeed) && allowedSpeed != 0f) || cooldown != 0f || ((!(Vector3.Dot(movementDirection, -Vector3.ProjectOnPlane(contactPoint.normal, Vector3.up).normalized) > allowedInput) || newMovement.boost) && (!(Vector3.Dot(newMovement.dodgeDirection, -Vector3.ProjectOnPlane(contactPoint.normal, Vector3.up).normalized) > allowedInput) || !newMovement.boost)) || !(Mathf.Abs(Vector3.Dot(Vector3.up, contactPoint.normal)) < allowedAngle))
			{
				continue;
			}
			position = base.transform.position + Vector3.up * step + Vector3.up * 0.25f;
			if (newMovement.sliding)
			{
				position += Vector3.up * 1.125f;
			}
			Collider[] array = Physics.OverlapCapsule(position - Vector3.up * step, position + Vector3.up * 1.25f, 0.499999f, layerMask);
			Collider[] array2 = Physics.OverlapCapsule(position - Vector3.up * 1.25f - Vector3.ProjectOnPlane(contactPoint.normal, Vector3.up) * 0.5f, position + Vector3.up * 1.25f - Vector3.ProjectOnPlane(contactPoint.normal, Vector3.up) * 0.5f, 0.5f, layerMask);
			if (array.Length == 0 && array2.Length == 0)
			{
				cooldown = cooldownMax;
				Vector3 vector = MonoSingleton<CameraController>.Instance.transform.position;
				bool flag = true;
				if (!newMovement.rising)
				{
					newMovement.rising = true;
					flag = true;
				}
				float num = 1.75f;
				if (Physics.Raycast(position - Vector3.up * num - Vector3.ProjectOnPlane(contactPoint.normal, Vector3.up).normalized * deltaHorizontal, -Vector3.up, out var hitInfo, step, layerMask))
				{
					rb.velocity -= new Vector3(0f, rb.velocity.y, 0f);
					base.transform.position += Vector3.up * (step + deltaVertical - hitInfo.distance) - Vector3.ProjectOnPlane(contactPoint.normal, Vector3.up).normalized * deltaHorizontal;
					rb.velocity = -collisionInfo.relativeVelocity;
				}
				else
				{
					base.transform.position += Vector3.up * (step + deltaVertical) - Vector3.ProjectOnPlane(contactPoint.normal, Vector3.up).normalized * deltaHorizontal;
					rb.velocity = -collisionInfo.relativeVelocity;
				}
				if (flag)
				{
					MonoSingleton<CameraController>.Instance.transform.position = vector;
					MonoSingleton<CameraController>.Instance.defaultPos = MonoSingleton<CameraController>.Instance.transform.localPosition;
				}
			}
		}
	}
}
