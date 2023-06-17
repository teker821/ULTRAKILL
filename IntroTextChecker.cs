using UnityEngine;

public class IntroTextChecker : MonoBehaviour
{
	public GameObject firstTime;

	public GameObject secondTime;

	private void Awake()
	{
		if (GameProgressSaver.GetIntro())
		{
			secondTime.SetActive(value: true);
		}
		else
		{
			firstTime.SetActive(value: true);
		}
		base.gameObject.SetActive(value: false);
	}
}
