using UnityEngine;

public class CutsceneSkip : MonoBehaviour
{
	public float addToTimer;

	private float timeLeft;

	public UltrakillEvent onSkip;

	private bool waitingForInput;

	private bool printLengthOfCutscene = true;

	private float startTime;

	private void Start()
	{
		if (CheckLevelRank.CheckLevelStatus() || Debug.isDebugBuild)
		{
			waitingForInput = true;
			timeLeft = addToTimer;
			MonoSingleton<CutsceneSkipText>.Instance.Show();
			startTime = MonoSingleton<StatsManager>.Instance.seconds;
		}
	}

	private void OnEnable()
	{
		if (waitingForInput)
		{
			MonoSingleton<CutsceneSkipText>.Instance.Show();
		}
	}

	private void OnDisable()
	{
		if (waitingForInput)
		{
			MonoSingleton<CutsceneSkipText>.Instance.Hide();
			if (printLengthOfCutscene)
			{
				Debug.Log("Length of cutscene: " + (MonoSingleton<StatsManager>.Instance.seconds - startTime));
			}
		}
	}

	private void Update()
	{
		if (waitingForInput && MonoSingleton<StatsManager>.Instance.timer)
		{
			timeLeft = Mathf.MoveTowards(timeLeft, 0f, Time.deltaTime);
		}
		if (waitingForInput && MonoSingleton<NewMovement>.Instance.dead)
		{
			waitingForInput = false;
			MonoSingleton<CutsceneSkipText>.Instance.Hide();
		}
	}

	private void LateUpdate()
	{
		if (waitingForInput && MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame)
		{
			waitingForInput = false;
			printLengthOfCutscene = false;
			if (MonoSingleton<OptionsManager>.Instance.paused)
			{
				MonoSingleton<OptionsManager>.Instance.UnPause();
			}
			onSkip.Invoke();
			MonoSingleton<CutsceneSkipText>.Instance.Hide();
			if (timeLeft > 0f)
			{
				MonoSingleton<StatsManager>.Instance.seconds += timeLeft;
				MonoSingleton<WeaponCharges>.Instance.Charge(timeLeft);
			}
		}
	}
}
