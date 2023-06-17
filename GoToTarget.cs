using UnityEngine;

public class GoToTarget : MonoBehaviour
{
	public ToDo onTargetReach;

	public float speed;

	public bool easeIn;

	public float easeInSpeed;

	private float currentSpeed;

	public Transform target;

	private Rigidbody rb;

	private bool stopped;

	public UltrakillEvent events;

	[HideInInspector]
	public bool activated;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (!stopped)
		{
			if (easeIn && currentSpeed != speed)
			{
				currentSpeed = Mathf.MoveTowards(currentSpeed, speed, Time.fixedDeltaTime * (Mathf.Abs(currentSpeed + 1f) * easeInSpeed));
			}
			else
			{
				currentSpeed = speed;
			}
			if (Vector3.Distance(base.transform.position, target.position) < rb.velocity.magnitude * Time.fixedDeltaTime)
			{
				Activate();
			}
			else
			{
				rb.velocity = (target.position - base.transform.position).normalized * currentSpeed;
			}
			if (Vector3.Distance(base.transform.position, target.position) < 0.5f)
			{
				Activate();
			}
		}
	}

	private void Activate()
	{
		if (!activated)
		{
			activated = true;
			if (events != null)
			{
				events.Invoke();
			}
			switch (onTargetReach)
			{
			case ToDo.Disable:
				Debug.Log("Try to Disable");
				base.gameObject.SetActive(value: false);
				break;
			case ToDo.Destroy:
				Object.Destroy(base.gameObject);
				break;
			default:
				stopped = true;
				rb.velocity = Vector3.zero;
				break;
			}
		}
	}
}
