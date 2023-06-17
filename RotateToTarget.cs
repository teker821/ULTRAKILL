using UnityEngine;

public class RotateToTarget : MonoBehaviour
{
	public Vector3 target;

	public bool onEnable;

	public bool oneTime;

	private bool beenActivated;

	private bool rotating;

	public float speed;

	public bool easeTowards;

	public UltrakillEvent onComplete;

	private void OnEnable()
	{
		if (onEnable && (!oneTime || !beenActivated))
		{
			Rotate();
		}
	}

	public void Rotate()
	{
		if (!beenActivated || !oneTime)
		{
			beenActivated = true;
			rotating = true;
		}
	}

	private void Update()
	{
		if (rotating)
		{
			base.transform.localRotation = Quaternion.RotateTowards(base.transform.localRotation, Quaternion.Euler(target), speed * Time.deltaTime * (easeTowards ? (Mathf.Min(Quaternion.Angle(base.transform.localRotation, Quaternion.Euler(target)) + speed / 10f, speed) / speed) : 1f));
			if (Quaternion.Angle(base.transform.localRotation, Quaternion.Euler(target)) < 0.1f)
			{
				base.transform.localRotation = Quaternion.Euler(target);
				rotating = false;
				onComplete?.Invoke();
			}
		}
	}
}
