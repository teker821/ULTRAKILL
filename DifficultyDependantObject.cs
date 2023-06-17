using UnityEngine;
using UnityEngine.Events;

public class DifficultyDependantObject : MonoBehaviour
{
	public bool autoDeactivate = true;

	[Header("Active in difficulties:")]
	public bool veryEasy = true;

	public bool easy = true;

	public bool normal = true;

	public bool hard = true;

	public bool veryHard = true;

	public bool UKMD = true;

	[Header("Optional events: ")]
	public UnityEvent onRightDifficulty;

	public UnityEvent onWrongDifficulty;

	private void Awake()
	{
		int @int = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		bool flag = false;
		switch (@int)
		{
		case 0:
			if (veryEasy)
			{
				flag = true;
			}
			break;
		case 1:
			if (easy)
			{
				flag = true;
			}
			break;
		case 2:
			if (normal)
			{
				flag = true;
			}
			break;
		case 3:
			if (hard)
			{
				flag = true;
			}
			break;
		case 4:
			if (veryHard)
			{
				flag = true;
			}
			break;
		case 5:
			if (UKMD)
			{
				flag = true;
			}
			break;
		}
		if (flag)
		{
			onRightDifficulty?.Invoke();
			return;
		}
		onWrongDifficulty?.Invoke();
		if (autoDeactivate)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
