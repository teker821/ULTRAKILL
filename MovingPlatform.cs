using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	[HideInInspector]
	public bool infoSet;

	public Vector3[] relativePoints;

	[HideInInspector]
	public Vector3 originalPosition;

	[HideInInspector]
	public Vector3 currentPosition;

	[HideInInspector]
	public Vector3 targetPosition;

	[HideInInspector]
	public int currentPoint;

	public float speed;

	public bool ease;

	public bool reverseAtEnd;

	public bool stopAtEnd;

	[HideInInspector]
	public bool forward = true;

	[HideInInspector]
	public bool moving;

	public float startOffset;

	public float moveDelay;

	[HideInInspector]
	public AudioSource aud;

	[HideInInspector]
	public float origPitch;

	public AudioClip moveSound;

	public AudioClip stopSound;

	public UltrakillEvent[] onReachPoint;

	private void Start()
	{
		if (relativePoints.Length >= 1)
		{
			bool flag = false;
			for (int i = 0; i < relativePoints.Length; i++)
			{
				if (relativePoints[i] == Vector3.zero)
				{
					flag = true;
					break;
				}
			}
			if (!flag && !stopAtEnd)
			{
				Vector3[] array = new Vector3[relativePoints.Length + 1];
				for (int j = 0; j < relativePoints.Length; j++)
				{
					array[j] = relativePoints[j];
				}
				array[^1] = Vector3.zero;
				relativePoints = array;
			}
		}
		if (!reverseAtEnd)
		{
			forward = true;
		}
		if (relativePoints.Length > 1 && !infoSet)
		{
			infoSet = true;
			aud = GetComponentInChildren<AudioSource>();
			if ((bool)aud)
			{
				origPitch = aud.pitch;
			}
			originalPosition = base.transform.position;
			currentPosition = originalPosition;
			Invoke("NextPoint", startOffset);
		}
		else if (infoSet && !moving)
		{
			Invoke("NextPoint", startOffset);
		}
	}

	private void FixedUpdate()
	{
		if (!moving)
		{
			return;
		}
		float num = speed;
		if (ease)
		{
			if (Vector3.Distance(base.transform.position, targetPosition) < speed / 2f)
			{
				num *= Vector3.Distance(base.transform.position, targetPosition) / (speed / 2f) + 0.1f;
			}
			if (Vector3.Distance(base.transform.position, currentPosition) < speed / 2f)
			{
				num *= Vector3.Distance(base.transform.position, currentPosition) / (speed / 2f) + 0.1f;
			}
		}
		base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, Time.deltaTime * num);
		if (Vector3.Distance(base.transform.position, targetPosition) < 0.01f)
		{
			base.transform.position = targetPosition;
			moving = false;
			if (onReachPoint.Length > currentPoint)
			{
				onReachPoint[currentPoint].Invoke();
			}
			Invoke("NextPoint", moveDelay);
			if ((bool)aud && (bool)stopSound)
			{
				aud.clip = stopSound;
				aud.loop = false;
				aud.pitch = origPitch + Random.Range(-0.1f, 0.1f);
				if (aud.clip != null)
				{
					aud.Play();
				}
			}
			else if ((bool)aud && (bool)moveSound)
			{
				aud.Stop();
			}
		}
		else if ((bool)aud && !aud.isPlaying && (bool)moveSound)
		{
			aud.clip = moveSound;
			aud.loop = true;
			aud.pitch = origPitch + Random.Range(-0.1f, 0.1f);
			if (aud.clip != null)
			{
				aud.Play();
			}
		}
	}

	private void NextPoint()
	{
		int num = 1;
		if (!forward)
		{
			num = -1;
		}
		if ((forward && currentPoint < relativePoints.Length - 1) || (!forward && currentPoint > 0))
		{
			currentPoint += num;
		}
		else
		{
			if (stopAtEnd)
			{
				return;
			}
			if (!reverseAtEnd)
			{
				currentPoint = 0;
			}
			else if (forward)
			{
				forward = false;
			}
			else
			{
				forward = true;
			}
		}
		currentPosition = targetPosition;
		targetPosition = originalPosition + relativePoints[currentPoint];
		moving = true;
		if ((bool)aud && (bool)moveSound)
		{
			aud.clip = moveSound;
			aud.loop = true;
			aud.pitch = origPitch + Random.Range(-0.1f, 0.1f);
			if (aud.clip != null)
			{
				aud.Play();
			}
		}
	}
}
