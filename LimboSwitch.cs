using UnityEngine;
using UnityEngine.Events;

public class LimboSwitch : MonoBehaviour
{
	public bool beenPressed;

	public int switchNumber;

	private float fadeAmount;

	public bool dontSave;

	public UnityEvent onAlreadyPressed;

	public UnityEvent onDelayedEffect;

	private MaterialPropertyBlock block;

	private MeshRenderer mr;

	private void Start()
	{
		mr = GetComponent<MeshRenderer>();
		block = new MaterialPropertyBlock();
		if (!dontSave && GameProgressSaver.GetLimboSwitch(switchNumber - 1))
		{
			beenPressed = true;
			onAlreadyPressed?.Invoke();
			fadeAmount = 2f;
			mr.GetPropertyBlock(block);
			block.SetFloat(UKShaderProperties.EmissiveIntensity, fadeAmount);
			mr.SetPropertyBlock(block);
		}
	}

	private void Update()
	{
		if (beenPressed && fadeAmount < 2f)
		{
			fadeAmount = Mathf.MoveTowards(fadeAmount, 2f, Time.deltaTime);
			mr.GetPropertyBlock(block);
			block.SetFloat(UKShaderProperties.EmissiveIntensity, fadeAmount);
			mr.SetPropertyBlock(block);
		}
	}

	public void Pressed()
	{
		if (!beenPressed)
		{
			beenPressed = true;
			GetComponent<AudioSource>().Play();
			Invoke("DelayedEffect", 1f);
			if (!dontSave)
			{
				GameProgressSaver.SetLimboSwitch(switchNumber - 1);
			}
		}
	}

	private void DelayedEffect()
	{
		onDelayedEffect?.Invoke();
	}
}
