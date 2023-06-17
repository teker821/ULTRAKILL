using UnityEngine;

public class BeatInfo : MonoBehaviour
{
	[HideInInspector]
	public bool valuesSet;

	public float bpm;

	public float timeSignature;

	[HideInInspector]
	public AudioSource aud;

	public TimeSignatureChange[] timeSignatureChanges;

	public void SetValues()
	{
		if (valuesSet)
		{
			return;
		}
		valuesSet = true;
		if (!aud)
		{
			aud = GetComponent<AudioSource>();
		}
		if (timeSignatureChanges.Length != 0)
		{
			float num = 0f;
			float num2 = timeSignature;
			float num3 = 0f;
			for (int i = 0; i < timeSignatureChanges.Length; i++)
			{
				timeSignatureChanges[i].time = num + 60f / bpm * 4f * (timeSignatureChanges[i].onMeasure - num3 - 1f) * num2;
				num = timeSignatureChanges[i].time;
				num2 = timeSignatureChanges[i].timeSignature;
				num3 = timeSignatureChanges[i].onMeasure - 1f;
			}
		}
	}
}
