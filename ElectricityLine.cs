using UnityEngine;

public class ElectricityLine : MonoBehaviour
{
	private LineRenderer lr;

	public Material[] lightningMats;

	public float minWidth;

	public float maxWidth;

	public Gradient colors;

	private float cooldown;

	private void Update()
	{
		if (cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
			return;
		}
		cooldown = 0.05f;
		if (!lr)
		{
			lr = GetComponent<LineRenderer>();
		}
		lr.material = lightningMats[Random.Range(0, lightningMats.Length)];
		lr.widthMultiplier = Random.Range(minWidth, maxWidth);
		lr.startColor = colors.Evaluate(Random.Range(0f, 1f));
		lr.endColor = colors.Evaluate(Random.Range(0f, 1f));
	}
}
