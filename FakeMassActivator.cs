using UnityEngine;

public class FakeMassActivator : MonoBehaviour
{
	private void OnEnable()
	{
		StatueIntroChecker instance = MonoSingleton<StatueIntroChecker>.Instance;
		base.transform.parent.GetComponentInChildren<MassAnimationReceiver>().GetComponent<Animator>().speed = 1f;
		if (instance != null && instance.beenSeen)
		{
			base.transform.parent.GetComponentInChildren<MassAnimationReceiver>().GetComponent<Animator>().Play("Intro", 0, 0.715f);
		}
		if (instance != null)
		{
			instance.beenSeen = true;
		}
	}
}
