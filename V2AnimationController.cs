using UnityEngine;

public class V2AnimationController : MonoBehaviour
{
	private V2 v2;

	private void Start()
	{
		v2 = GetComponentInParent<V2>();
	}

	public void IntroEnd()
	{
		v2.IntroEnd();
	}

	public void StareAtPlayer()
	{
		v2.StareAtPlayer();
	}

	public void BeginEscape()
	{
		v2.BeginEscape();
	}

	public void WingsOpen()
	{
		v2.SwitchPattern(0);
	}
}
