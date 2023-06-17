using UnityEngine;

public class RevolverCylinder : MonoBehaviour
{
	public int bulletAmount = 6;

	public Vector3 rotationAxis;

	public float speed;

	private AudioSource aud;

	private int target;

	private Quaternion currentRotation;

	private Quaternion[] allRotations;

	private bool freeSpinning;

	[HideInInspector]
	public float spinSpeed;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		currentRotation = base.transform.localRotation;
		allRotations = new Quaternion[bulletAmount];
		for (int i = 0; i < bulletAmount; i++)
		{
			allRotations[i] = base.transform.localRotation * Quaternion.Euler(rotationAxis * (360 / bulletAmount) * i);
		}
	}

	private void LateUpdate()
	{
		if (spinSpeed * 10f > speed)
		{
			freeSpinning = true;
			currentRotation *= Quaternion.Euler(rotationAxis * Time.deltaTime * spinSpeed * 10f);
		}
		else if (freeSpinning)
		{
			freeSpinning = false;
			target = GetClosestTarget();
		}
		if (!freeSpinning && Quaternion.Angle(currentRotation, allRotations[target]) > 0.1f)
		{
			currentRotation = Quaternion.RotateTowards(currentRotation, allRotations[target], Time.deltaTime * speed);
			if (Quaternion.Angle(currentRotation, allRotations[target]) <= 0.1f)
			{
				currentRotation = allRotations[target];
				aud?.Play();
			}
		}
		base.transform.localRotation = currentRotation;
	}

	public void DoTurn()
	{
		target++;
		if (target >= allRotations.Length)
		{
			target = 0;
		}
	}

	private int GetClosestTarget()
	{
		int result = 0;
		float num = Quaternion.Angle(currentRotation, allRotations[0]);
		for (int i = 1; i < bulletAmount; i++)
		{
			float num2 = Quaternion.Angle(currentRotation, allRotations[i]);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}
}
