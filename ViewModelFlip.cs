using UnityEngine;

public class ViewModelFlip : MonoBehaviour
{
	private void Start()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition") == 2)
		{
			Left();
		}
		else
		{
			Right();
		}
	}

	public void Left()
	{
		base.transform.localScale = new Vector3(-1f, 1f, 1f);
	}

	public void Right()
	{
		base.transform.localScale = new Vector3(1f, 1f, 1f);
	}
}
