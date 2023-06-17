using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
	public static HudController Instance;

	public bool altHud;

	public bool colorless;

	private GameObject altHudObj;

	private HUDPos hudpos;

	public GameObject gunCanvas;

	public GameObject weaponIcon;

	public GameObject armIcon;

	public GameObject styleMeter;

	public GameObject styleInfo;

	public Image[] hudBackgrounds;

	private void Awake()
	{
		if (!altHud && !Instance)
		{
			Instance = this;
		}
	}

	private void Start()
	{
		if (MapInfoBase.InstanceAnyType.hideStockHUD)
		{
			weaponIcon.SetActive(value: false);
			armIcon.SetActive(value: false);
			return;
		}
		CheckSituation();
		if (!MonoSingleton<PrefsManager>.Instance.GetBool("weaponIcons"))
		{
			if (!altHud)
			{
				weaponIcon.transform.localPosition = new Vector3(weaponIcon.transform.localPosition.x, weaponIcon.transform.localPosition.y, 45f);
			}
			else
			{
				weaponIcon.SetActive(value: false);
			}
		}
		if (!MonoSingleton<PrefsManager>.Instance.GetBool("armIcons"))
		{
			if (!altHud)
			{
				armIcon.transform.localPosition = new Vector3(armIcon.transform.localPosition.x, armIcon.transform.localPosition.y, 0f);
			}
			else
			{
				armIcon.SetActive(value: false);
			}
		}
		if (!altHud)
		{
			if (!MonoSingleton<PrefsManager>.Instance.GetBool("styleMeter"))
			{
				styleMeter.transform.localPosition = new Vector3(styleMeter.transform.localPosition.x, styleMeter.transform.localPosition.y, -9999f);
			}
			if (!MonoSingleton<PrefsManager>.Instance.GetBool("styleInfo"))
			{
				styleInfo.transform.localPosition = new Vector3(styleInfo.transform.localPosition.x, styleInfo.transform.localPosition.y, -9999f);
				MonoSingleton<StyleHUD>.Instance.GetComponent<AudioSource>().enabled = false;
			}
		}
		float @float = MonoSingleton<PrefsManager>.Instance.GetFloat("hudBackgroundOpacity");
		if (@float != 50f)
		{
			SetOpacity(@float);
		}
	}

	public void CheckSituation()
	{
		if (altHud)
		{
			if (altHudObj == null)
			{
				altHudObj = base.transform.GetChild(0).gameObject;
			}
			if ((bool)altHudObj)
			{
				if (MonoSingleton<PrefsManager>.Instance.GetInt("hudType") == 2 && !colorless)
				{
					altHudObj.SetActive(value: true);
				}
				else if (MonoSingleton<PrefsManager>.Instance.GetInt("hudType") == 3 && colorless)
				{
					altHudObj.SetActive(value: true);
				}
				else
				{
					altHudObj.SetActive(value: false);
				}
			}
			return;
		}
		if (MonoSingleton<PrefsManager>.Instance.GetInt("hudType") != 1)
		{
			if (gunCanvas == null)
			{
				gunCanvas = base.transform.Find("GunCanvas").gameObject;
			}
			if (hudpos == null)
			{
				hudpos = gunCanvas.GetComponent<HUDPos>();
			}
			gunCanvas.transform.localPosition = new Vector3(gunCanvas.transform.localPosition.x, gunCanvas.transform.localPosition.y, -100f);
			if ((bool)hudpos)
			{
				hudpos.active = false;
			}
			return;
		}
		if (gunCanvas == null)
		{
			gunCanvas = base.transform.Find("GunCanvas").gameObject;
		}
		if (hudpos == null)
		{
			hudpos = gunCanvas.GetComponent<HUDPos>();
		}
		gunCanvas.transform.localPosition = new Vector3(gunCanvas.transform.localPosition.x, gunCanvas.transform.localPosition.y, 1f);
		if ((bool)hudpos)
		{
			hudpos.active = true;
			hudpos.CheckPos();
		}
	}

	public void SetOpacity(float amount)
	{
		Image[] array = hudBackgrounds;
		foreach (Image image in array)
		{
			if ((bool)image)
			{
				Color color = image.color;
				color.a = amount / 100f;
				image.color = color;
			}
		}
	}
}
