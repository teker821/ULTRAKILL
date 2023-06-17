using UnityEngine;
using UnityEngine.UI;

public class ShopMother : MonoBehaviour
{
	private ShopZone shop;

	public Text dailyTip;

	private Text origDailyTip;

	private GameObject menu;

	private CameraController cc;

	private void Start()
	{
		ShopZone[] array = Object.FindObjectsOfType<ShopZone>();
		foreach (ShopZone shopZone in array)
		{
			if (shopZone.gameObject != base.gameObject)
			{
				shop = shopZone;
			}
		}
		if (shop != null)
		{
			origDailyTip = shop.transform.GetChild(1).GetChild(4).GetChild(0)
				.GetChild(0)
				.GetChild(1)
				.GetComponent<Text>();
			dailyTip.text = origDailyTip.text;
		}
		menu = base.transform.GetChild(0).gameObject;
		cc = MonoSingleton<CameraController>.Instance;
	}

	private void Update()
	{
		if (MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame && menu.activeSelf)
		{
			TurnOff();
		}
	}

	public void TurnOn()
	{
		if (!menu.activeSelf)
		{
			menu.SetActive(value: true);
			cc.enabled = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	public void TurnOff()
	{
		if (menu.activeSelf)
		{
			menu.SetActive(value: false);
			cc.enabled = true;
		}
	}
}
