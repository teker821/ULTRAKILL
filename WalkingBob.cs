using UnityEngine;

public class WalkingBob : MonoBehaviour
{
	private NewMovement nmov;

	private Vector3 originalPos;

	private Vector3 rightPos;

	private Vector3 leftPos;

	private Vector3 target;

	private bool backToStart;

	private float speed;

	private void Awake()
	{
		nmov = GetComponentInParent<NewMovement>();
		originalPos = base.transform.localPosition;
		rightPos = new Vector3(originalPos.x + 0.08f, originalPos.y - 0.025f, originalPos.z);
		leftPos = new Vector3(originalPos.x - 0.08f, originalPos.y - 0.025f, originalPos.z);
		target = rightPos;
	}

	private void Update()
	{
		if (nmov.walking)
		{
			speed = Time.deltaTime * (2f - Vector3.Distance(base.transform.localPosition, originalPos) * 3f) * (Mathf.Min(nmov.rb.velocity.magnitude, 15f) / 15f);
			if (backToStart)
			{
				base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, originalPos, speed * 0.25f);
			}
			else
			{
				base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, target, speed * 0.25f);
			}
			if (base.transform.localPosition == originalPos)
			{
				backToStart = false;
			}
			else if (base.transform.localPosition == rightPos)
			{
				backToStart = true;
				target = leftPos;
			}
			else if (base.transform.localPosition == leftPos)
			{
				backToStart = true;
				target = rightPos;
			}
		}
		else if (base.transform.localPosition != originalPos)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, originalPos, Time.deltaTime);
		}
	}
}
