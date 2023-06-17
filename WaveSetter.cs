using UnityEngine;
using UnityEngine.UI;

public class WaveSetter : MonoBehaviour
{
	public int wave;

	private WaveMenu wm;

	private ButtonState state;

	private ControllerPointer pointer;

	private bool prepared;

	[SerializeField]
	private GameObject buttonFail;

	[SerializeField]
	private GameObject buttonSuccess;

	[SerializeField]
	private Image buttonGraphic;

	[SerializeField]
	private Text buttonText;

	private void Awake()
	{
		if (!TryGetComponent<ControllerPointer>(out pointer))
		{
			pointer = base.gameObject.AddComponent<ControllerPointer>();
		}
		pointer.OnPressed.AddListener(OnPointerClick);
	}

	private void Start()
	{
		if (!prepared)
		{
			Prepare();
		}
	}

	private void OnPointerClick()
	{
		if (!prepared)
		{
			Prepare();
		}
		if (state == ButtonState.Locked)
		{
			Object.Instantiate(buttonFail);
			return;
		}
		Object.Instantiate(buttonSuccess);
		wm.SetCurrentWave(wave);
		Selected();
	}

	public void Selected()
	{
		if (state != ButtonState.Locked)
		{
			buttonGraphic.color = Color.white;
			buttonGraphic.fillCenter = true;
			buttonText.color = Color.black;
		}
	}

	public void Unselected()
	{
		if (state != ButtonState.Locked)
		{
			buttonGraphic.color = Color.white;
			buttonGraphic.fillCenter = false;
			buttonText.color = Color.white;
		}
	}

	public void Locked()
	{
		buttonGraphic.color = Color.red;
		buttonGraphic.fillCenter = false;
		buttonText.color = Color.red;
	}

	private void Prepare()
	{
		wm = GetComponentInParent<WaveMenu>();
		if (wave == 0)
		{
			buttonText.text = "1";
		}
		else
		{
			buttonText.text = wave.ToString();
		}
		if ((bool)wm)
		{
			prepared = true;
			state = wm.CheckWaveAvailability(this);
			if (state == ButtonState.Selected)
			{
				Selected();
			}
			else if (state == ButtonState.Locked)
			{
				Locked();
			}
		}
	}
}
