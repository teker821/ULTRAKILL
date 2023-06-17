using UnityEngine;

public class Spin : MonoBehaviour
{
	public Vector3 spinDirection;

	public float speed;

	public bool inLateUpdate;

	private Vector3 totalRotation;

	public bool notRelative;

	public bool gradual;

	public float gradualSpeed;

	private float currentSpeed;

	public bool off;

	private AudioSource aud;

	private float originalPitch;

	[HideInInspector]
	public float pitchMultiplier = 1f;

	private void Start()
	{
		if (gradual)
		{
			aud = GetComponent<AudioSource>();
			if ((bool)aud)
			{
				originalPitch = aud.pitch;
				aud.pitch = currentSpeed;
			}
		}
	}

	private void FixedUpdate()
	{
		if (inLateUpdate)
		{
			return;
		}
		if (gradual)
		{
			if (!off && currentSpeed != speed)
			{
				currentSpeed = Mathf.MoveTowards(currentSpeed, speed, Time.deltaTime * gradualSpeed);
			}
			else if (off && currentSpeed != 0f)
			{
				currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, Time.deltaTime * gradualSpeed);
			}
			if (currentSpeed != 0f)
			{
				if (!notRelative)
				{
					base.transform.Rotate(spinDirection, currentSpeed * Time.deltaTime, Space.Self);
				}
				else
				{
					base.transform.Rotate(spinDirection, currentSpeed * Time.deltaTime, Space.World);
				}
			}
			if ((bool)aud)
			{
				aud.pitch = currentSpeed / speed * originalPitch * pitchMultiplier;
			}
		}
		else if (!notRelative)
		{
			base.transform.Rotate(spinDirection, speed * Time.deltaTime, Space.Self);
		}
		else
		{
			base.transform.Rotate(spinDirection, speed * Time.deltaTime, Space.World);
		}
	}

	private void LateUpdate()
	{
		if (inLateUpdate)
		{
			if (totalRotation == Vector3.zero)
			{
				totalRotation = base.transform.localRotation.eulerAngles;
			}
			base.transform.localRotation = Quaternion.Euler(totalRotation);
			base.transform.Rotate(spinDirection, speed * Time.deltaTime);
			totalRotation = base.transform.localRotation.eulerAngles;
		}
	}

	public void ChangeState(bool on)
	{
		if (on)
		{
			off = false;
		}
		else
		{
			off = true;
		}
	}

	public void ChangeSpeed(float newSpeed)
	{
		speed = newSpeed;
	}

	public void ChangeGradualSpeed(float newGradualSpeed)
	{
		gradualSpeed = newGradualSpeed;
	}

	public void ChangePitchMultiplier(float newPitch)
	{
		pitchMultiplier = newPitch;
	}

	public void ChangeSpinDirection(Vector3 newDirection)
	{
		spinDirection = newDirection;
	}
}
