using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CutsceneSkipText : MonoSingleton<CutsceneSkipText>
{
	private Text txt;

	private void Start()
	{
		txt = GetComponent<Text>();
		Hide();
	}

	public void Show()
	{
		txt.enabled = true;
	}

	public void Hide()
	{
		txt.enabled = false;
	}
}
