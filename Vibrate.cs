using UnityEngine;

public class Vibrate : MonoBehaviour
{
	public float intensity;

	private Vector3 origPos;

	public float speed;

	private Vector3 targetPos;

	private void Start()
	{
		origPos = base.transform.localPosition;
		targetPos = origPos;
	}

	private void Update()
	{
		if (speed == 0f)
		{
			base.transform.localPosition = new Vector3(origPos.x + Random.Range(0f - intensity, intensity), origPos.y + Random.Range(0f - intensity, intensity), origPos.z + Random.Range(0f - intensity, intensity));
		}
		else if (base.transform.localPosition == targetPos)
		{
			targetPos = new Vector3(origPos.x + Random.Range(0f - intensity, intensity), origPos.y + Random.Range(0f - intensity, intensity), origPos.z + Random.Range(0f - intensity, intensity));
		}
		else
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, targetPos, Time.deltaTime * speed);
		}
	}
}
