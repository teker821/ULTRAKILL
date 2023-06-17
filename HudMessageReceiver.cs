using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class HudMessageReceiver : MonoSingleton<HudMessageReceiver>
{
	private Image img;

	[HideInInspector]
	public Text text;

	private AudioSource aud;

	private HudOpenEffect hoe;

	private string message;

	private string input;

	private string message2;

	private bool noSound;

	private void Start()
	{
		img = GetComponent<Image>();
		text = GetComponentInChildren<Text>();
		aud = GetComponent<AudioSource>();
		hoe = GetComponent<HudOpenEffect>();
	}

	private void Done()
	{
		img.enabled = false;
		text.enabled = false;
	}

	public void SendHudMessage(string newmessage, string newinput = "", string newmessage2 = "", int delay = 0, bool silent = false)
	{
		message = newmessage;
		input = newinput;
		message2 = newmessage2;
		noSound = silent;
		Invoke("ShowHudMessage", delay);
	}

	private void ShowHudMessage()
	{
		if (input == "")
		{
			this.text.text = message;
		}
		else
		{
			string text = "";
			KeyCode keyCode = MonoSingleton<InputManager>.Instance.Inputs[input];
			text = keyCode switch
			{
				KeyCode.Mouse0 => "Left Mouse Button", 
				KeyCode.Mouse1 => "Right Mouse Button", 
				KeyCode.Mouse2 => "Middle Mouse Button", 
				_ => keyCode.ToString(), 
			};
			this.text.text = message + text + message2;
		}
		this.text.text = this.text.text.Replace('$', '\n');
		this.text.enabled = true;
		img.enabled = true;
		hoe.Force();
		if (!noSound)
		{
			aud.Play();
		}
		CancelInvoke("Done");
		Invoke("Done", 5f);
	}

	public void ClearMessage()
	{
		CancelInvoke("Done");
		Done();
	}
}
