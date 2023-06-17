using UnityEngine;

public class Beamgun : MonoBehaviour
{
	[SerializeField]
	private Transform shootPoint;

	[SerializeField]
	private BeamgunBeam beam;

	[SerializeField]
	private GameObject beamDrone;

	private GameObject currentBeamDrone;

	private float tempWidthCooldown;

	private void Start()
	{
	}

	private void Update()
	{
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			if (!beam.gameObject.activeSelf)
			{
				beam.gameObject.SetActive(value: true);
			}
			beam.fakeStartPoint = shootPoint.position;
			float num = Mathf.Clamp(MonoSingleton<NewMovement>.Instance.rb.velocity.magnitude / 5f, 0.1f, 0.25f);
			if (beam.beamWidth > num)
			{
				tempWidthCooldown = Mathf.MoveTowards(tempWidthCooldown, 1f, Time.deltaTime * 50f);
				if (tempWidthCooldown >= 1f)
				{
					beam.beamWidth = num;
				}
			}
			else
			{
				tempWidthCooldown = Mathf.MoveTowards(tempWidthCooldown, 0f, Time.deltaTime * 50f);
				beam.beamWidth = num;
			}
			beam.beamCheckSpeed = 1f + (num - 0.1f) * 5f;
		}
		else if (beam.gameObject.activeSelf)
		{
			beam.gameObject.SetActive(value: false);
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame)
		{
			if (!currentBeamDrone)
			{
				currentBeamDrone = Object.Instantiate(beamDrone, base.transform.position + base.transform.forward, base.transform.rotation);
			}
			else
			{
				Object.Destroy(currentBeamDrone);
			}
		}
	}
}
