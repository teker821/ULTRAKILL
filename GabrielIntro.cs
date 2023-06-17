using UnityEngine;

public class GabrielIntro : MonoBehaviour
{
	private Animator anim;

	private bool shaking;

	private bool tracking;

	private Quaternion previousRotation;

	private Vector3 defaultPos;

	private float shakeAmount;

	[SerializeField]
	private Transform root;

	[SerializeField]
	private SkinnedMeshRenderer sword1;

	[SerializeField]
	private GameObject fakeSword1;

	[SerializeField]
	private SkinnedMeshRenderer sword2;

	[SerializeField]
	private GameObject fakeSword2;

	[SerializeField]
	private AudioSource swordUnsheatheSound;

	[SerializeField]
	private GameObject rumbling;

	public void Begin()
	{
		anim = GetComponent<Animator>();
		anim.Play("Intro");
	}

	private void Update()
	{
		if (shaking)
		{
			shakeAmount = Mathf.MoveTowards(shakeAmount, 0.125f, Time.deltaTime * 0.0125f);
			MonoSingleton<CameraController>.Instance.CameraShake(shakeAmount * 2f);
			base.transform.position = new Vector3(defaultPos.x + Random.Range(0f - shakeAmount, shakeAmount), defaultPos.y + Random.Range(0f - shakeAmount, shakeAmount), defaultPos.z + Random.Range(0f - shakeAmount, shakeAmount));
		}
	}

	private void LateUpdate()
	{
		if (tracking)
		{
			root.rotation = Quaternion.RotateTowards(previousRotation, Quaternion.LookRotation(MonoSingleton<PlayerTracker>.Instance.GetTarget().position - root.position), Time.deltaTime * 720f);
			previousRotation = root.rotation;
		}
	}

	private void StartShaking()
	{
		shaking = true;
		defaultPos = base.transform.position;
		rumbling.SetActive(value: true);
	}

	private void StopShaking()
	{
		shaking = false;
		base.transform.position = defaultPos;
		rumbling.SetActive(value: false);
	}

	private void StartTracking()
	{
		tracking = true;
		previousRotation = root.rotation;
	}

	private void SwordPull(int sword)
	{
		switch (sword)
		{
		case 1:
			sword1.enabled = true;
			fakeSword1.SetActive(value: false);
			break;
		case 2:
			sword2.enabled = true;
			fakeSword2.SetActive(value: false);
			break;
		}
		Object.Instantiate(swordUnsheatheSound, root.position, Quaternion.identity);
	}
}
