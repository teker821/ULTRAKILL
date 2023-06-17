using UnityEngine;
using UnityEngine.UI;

public class HudMessage : MonoBehaviour
{
	private HudMessageReceiver messageHud;

	public bool timed;

	public bool deactivating;

	public bool notOneTime;

	public bool dontActivateOnTriggerEnter;

	public bool silent;

	public bool deactiveOnTriggerExit;

	private bool activated;

	public string message;

	public string input;

	public string message2;

	private Image img;

	private Text text;

	public string playerPref;

	private bool colliderless;

	public float timerTime = 5f;

	private string PlayerPref
	{
		get
		{
			string text = playerPref;
			if (!(text == "SecMisTut"))
			{
				if (text == "ShoUseTut")
				{
					return "hideShotgunPopup";
				}
				return playerPref;
			}
			return "secretMissionPopup";
		}
	}

	private void Start()
	{
		if (GetComponent<Collider>() == null)
		{
			colliderless = true;
			if (PlayerPref == "" || playerPref == null)
			{
				PlayMessage();
			}
			else if (!MonoSingleton<PrefsManager>.Instance.GetBool(PlayerPref))
			{
				MonoSingleton<PrefsManager>.Instance.SetBool(PlayerPref, content: true);
				PlayMessage();
			}
		}
	}

	private void OnEnable()
	{
		if (colliderless && (!activated || notOneTime))
		{
			if (PlayerPref == "")
			{
				PlayMessage();
			}
			else if (!MonoSingleton<PrefsManager>.Instance.GetBool(PlayerPref))
			{
				MonoSingleton<PrefsManager>.Instance.SetBool(PlayerPref, content: true);
				PlayMessage();
			}
		}
	}

	private void Update()
	{
		if (activated && timed)
		{
			img.enabled = true;
			text.enabled = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!dontActivateOnTriggerEnter && other.gameObject.CompareTag("Player") && (!activated || notOneTime))
		{
			if (PlayerPref == "")
			{
				PlayMessage();
			}
			else if (!MonoSingleton<PrefsManager>.Instance.GetBool(PlayerPref))
			{
				MonoSingleton<PrefsManager>.Instance.SetBool(PlayerPref, content: true);
				PlayMessage();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!dontActivateOnTriggerEnter && other.gameObject.CompareTag("Player") && activated && deactiveOnTriggerExit)
		{
			Done();
		}
	}

	private void Done()
	{
		img.enabled = false;
		text.enabled = false;
		activated = false;
		Begone();
	}

	private void Begone()
	{
		if (!notOneTime)
		{
			Object.Destroy(this);
		}
	}

	public void PlayMessage(bool hasToBeEnabled = false)
	{
		if ((activated && !notOneTime) || (hasToBeEnabled && (!base.gameObject.activeInHierarchy || !base.enabled)))
		{
			return;
		}
		activated = true;
		messageHud = MonoSingleton<HudMessageReceiver>.Instance;
		this.text = messageHud.text;
		if (input == "" || input == null)
		{
			this.text.text = message;
		}
		else
		{
			string text = "";
			KeyCode keyCode = MonoSingleton<InputManager>.Instance.Inputs[input];
			switch (keyCode)
			{
			case KeyCode.Mouse0:
				text = "Left Mouse Button";
				break;
			case KeyCode.Mouse1:
				text = "Right Mouse Button";
				break;
			case KeyCode.Mouse2:
				text = "Middle Mouse Button";
				break;
			case KeyCode.Mouse3:
			case KeyCode.Mouse4:
			case KeyCode.Mouse5:
			case KeyCode.Mouse6:
			{
				text = keyCode.ToString();
				string s = text.Substring(text.Length - 1, 1);
				text = text.Substring(0, text.Length - 1);
				text += int.Parse(s) + 1;
				break;
			}
			default:
				text = keyCode.ToString();
				break;
			}
			text = MonoSingleton<InputManager>.Instance.GetBindingString(input) ?? text;
			this.text.text = message + text + message2;
		}
		this.text.text = this.text.text.Replace('$', '\n');
		this.text.enabled = true;
		img = messageHud.GetComponent<Image>();
		img.enabled = true;
		if (deactivating)
		{
			Done();
		}
		else if (!silent)
		{
			messageHud.GetComponent<AudioSource>().Play();
		}
		if (timed && notOneTime)
		{
			CancelInvoke("Done");
			Invoke("Done", timerTime);
		}
		else if (timed)
		{
			Invoke("Done", timerTime);
		}
		else if (!deactiveOnTriggerExit)
		{
			Invoke("Begone", 1f);
		}
		messageHud.GetComponent<HudOpenEffect>().Force();
	}

	public void ChangeMessage(string newMessage)
	{
		message = newMessage;
		input = "";
		message2 = "";
	}
}
