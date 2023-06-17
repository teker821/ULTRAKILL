using UnityEngine;

public class CustomMusicPlaceholder : MonoBehaviour
{
	private float offset;

	private TimeSince sinceEnabled;

	private void Awake()
	{
		Random.Range(0, 1);
	}

	private void OnEnable()
	{
		sinceEnabled = offset;
	}

	private void Update()
	{
		base.transform.localRotation = Quaternion.Euler(0f, 0f, (float)sinceEnabled * -360f);
	}
}
