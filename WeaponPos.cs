using System.Collections.Generic;
using UnityEngine;

public class WeaponPos : MonoBehaviour
{
	private bool ready;

	public Vector3 currentDefault;

	private Vector3 defaultPos;

	private Vector3 defaultRot;

	private Vector3 defaultScale;

	public Vector3 middlePos;

	public Vector3 middleRot;

	public Vector3 middleScale;

	public Transform[] moveOnMiddlePos;

	public Vector3[] middlePosValues;

	private List<Vector3> defaultPosValues = new List<Vector3>();

	public Vector3[] middleRotValues;

	private List<Vector3> defaultRotValues = new List<Vector3>();

	private void Start()
	{
		CheckPosition();
	}

	private void OnEnable()
	{
		CheckPosition();
	}

	public void CheckPosition()
	{
		if (!ready)
		{
			ready = true;
			defaultPos = base.transform.localPosition;
			defaultRot = base.transform.localRotation.eulerAngles;
			defaultScale = base.transform.localScale;
			if (middleScale == Vector3.zero)
			{
				middleScale = defaultScale;
			}
			if (middleRot == Vector3.zero)
			{
				middleRot = defaultRot;
			}
			if (moveOnMiddlePos != null && moveOnMiddlePos.Length != 0)
			{
				for (int i = 0; i < moveOnMiddlePos.Length; i++)
				{
					defaultPosValues.Add(moveOnMiddlePos[i].localPosition);
					defaultRotValues.Add(moveOnMiddlePos[i].localEulerAngles);
					if (middleRotValues[i] == Vector3.zero)
					{
						middleRotValues[i] = moveOnMiddlePos[i].localEulerAngles;
					}
				}
			}
		}
		if (MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition") == 1 && (!MonoSingleton<PowerUpMeter>.Instance || MonoSingleton<PowerUpMeter>.Instance.juice <= 0f))
		{
			base.transform.localPosition = middlePos;
			base.transform.localRotation = Quaternion.Euler(middleRot);
			base.transform.localScale = middleScale;
			if (moveOnMiddlePos != null && moveOnMiddlePos.Length != 0)
			{
				for (int j = 0; j < moveOnMiddlePos.Length; j++)
				{
					moveOnMiddlePos[j].localPosition = middlePosValues[j];
					moveOnMiddlePos[j].localEulerAngles = middleRotValues[j];
				}
			}
		}
		else
		{
			base.transform.localPosition = defaultPos;
			base.transform.localRotation = Quaternion.Euler(defaultRot);
			base.transform.localScale = defaultScale;
			if (moveOnMiddlePos != null && moveOnMiddlePos.Length != 0)
			{
				for (int k = 0; k < moveOnMiddlePos.Length; k++)
				{
					moveOnMiddlePos[k].localPosition = defaultPosValues[k];
					moveOnMiddlePos[k].localEulerAngles = defaultRotValues[k];
				}
			}
		}
		currentDefault = base.transform.localPosition;
	}
}
