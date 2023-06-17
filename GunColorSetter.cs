using UnityEngine;
using UnityEngine.UI;

public class GunColorSetter : MonoBehaviour
{
	private int weaponNumber;

	public int colorNumber;

	private bool altVersion;

	private float redAmount;

	private float greenAmount;

	private float blueAmount;

	private float metalAmount;

	public Slider redSlider;

	public Slider greenSlider;

	public Slider blueSlider;

	public Slider metalSlider;

	public Image colorExample;

	public Image metalExample;

	private GunColorTypeGetter gctg;

	private void OnEnable()
	{
		if (gctg == null)
		{
			gctg = GetComponentInParent<GunColorTypeGetter>();
		}
		weaponNumber = gctg.weaponNumber;
		altVersion = gctg.altVersion;
		redAmount = MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + weaponNumber + "." + colorNumber + (altVersion ? ".a" : ".") + "r", 1f);
		greenAmount = MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + weaponNumber + "." + colorNumber + (altVersion ? ".a" : ".") + "g", 1f);
		blueAmount = MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + weaponNumber + "." + colorNumber + (altVersion ? ".a" : ".") + "b", 1f);
		metalAmount = MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + weaponNumber + "." + colorNumber + (altVersion ? ".a" : ".") + "a");
		redSlider.value = redAmount;
		greenSlider.value = greenAmount;
		blueSlider.value = blueAmount;
		metalSlider.value = metalAmount;
		colorExample.color = new Color(redAmount, greenAmount, blueAmount);
		float num = ((Vector3.Dot(Vector3.one, new Vector3(redAmount, greenAmount, blueAmount)) / 3f < 0.9f) ? 1f : 0.7f);
		metalExample.color = new Color(num, num, num, metalAmount);
		gctg.UpdatePreview();
	}

	public void SetRed(float amount)
	{
		redAmount = amount;
		UpdateColor();
	}

	public void SetGreen(float amount)
	{
		greenAmount = amount;
		UpdateColor();
	}

	public void SetBlue(float amount)
	{
		blueAmount = amount;
		UpdateColor();
	}

	public void SetMetal(float amount)
	{
		metalAmount = amount;
		UpdateColor();
	}

	public void UpdateColor()
	{
		if (gctg == null)
		{
			gctg = GetComponentInParent<GunColorTypeGetter>();
		}
		weaponNumber = gctg.weaponNumber;
		altVersion = gctg.altVersion;
		MonoSingleton<PrefsManager>.Instance.SetFloat("gunColor." + weaponNumber + "." + colorNumber + (altVersion ? ".a" : ".") + "r", redAmount);
		MonoSingleton<PrefsManager>.Instance.SetFloat("gunColor." + weaponNumber + "." + colorNumber + (altVersion ? ".a" : ".") + "g", greenAmount);
		MonoSingleton<PrefsManager>.Instance.SetFloat("gunColor." + weaponNumber + "." + colorNumber + (altVersion ? ".a" : ".") + "b", blueAmount);
		MonoSingleton<PrefsManager>.Instance.SetFloat("gunColor." + weaponNumber + "." + colorNumber + (altVersion ? ".a" : ".") + "a", metalAmount);
		colorExample.color = new Color(redAmount, greenAmount, blueAmount);
		float num = ((Vector3.Dot(Vector3.one, new Vector3(redAmount, greenAmount, blueAmount)) / 3f < 0.9f) ? 1f : 0.7f);
		metalExample.color = new Color(num, num, num, metalAmount);
		MonoSingleton<GunColorController>.Instance.UpdateGunColors();
		gctg.UpdatePreview();
	}
}
