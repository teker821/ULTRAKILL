using UnityEngine;

public class HUDPos : MonoBehaviour
{
	private bool ready;

	public bool active;

	private Vector3 defaultPos;

	private Vector3 defaultRot;

	public Vector3 reversePos;

	public Vector3 reverseRot;

	private void Start()
	{
		CheckPos();
	}

	private void OnEnable()
	{
		CheckPos();
	}

	public void CheckPos()
	{
		if (active)
		{
			if (!ready)
			{
				ready = true;
				defaultPos = base.transform.localPosition;
				defaultRot = base.transform.localRotation.eulerAngles;
			}
			if (MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition") == 2)
			{
				base.transform.localPosition = reversePos;
				base.transform.localRotation = Quaternion.Euler(reverseRot);
			}
			else
			{
				base.transform.localPosition = defaultPos;
				base.transform.localRotation = Quaternion.Euler(defaultRot);
			}
		}
	}
}
