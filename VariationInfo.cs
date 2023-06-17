using UnityEngine;
using UnityEngine.UI;

public class VariationInfo : MonoBehaviour
{
	public GameObject varPage;

	private int money;

	public Text moneyText;

	public int cost;

	public Text costText;

	public ShopButton buyButton;

	private Text buttonText;

	public GameObject buySound;

	public Button equipButton;

	private Image equipImage;

	public Sprite[] equipSprites;

	private int equipStatus;

	public bool alreadyOwned;

	public string weaponName;

	private GunSetter gs;

	private FistControl fc;

	private GameObject player;

	public GameObject orderButtons;

	private void Start()
	{
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		buttonText = buyButton.GetComponentInChildren<Text>();
		buyButton.variationInfo = this;
		equipImage = equipButton.transform.GetChild(0).GetComponent<Image>();
		if (GameProgressSaver.CheckGear(weaponName) > 0)
		{
			alreadyOwned = true;
		}
		UpdateMoney();
	}

	private void OnEnable()
	{
		UpdateMoney();
	}

	public void UpdateMoney()
	{
		money = GameProgressSaver.GetMoney();
		moneyText.text = MoneyText.DivideMoney(money) + "<color=orange>P</color>";
		if (!alreadyOwned && cost < 0 && GameProgressSaver.CheckGear(weaponName) > 0)
		{
			alreadyOwned = true;
		}
		if (!alreadyOwned)
		{
			if (cost < 0)
			{
				costText.text = "<color=red>UNAVAILABLE</color>";
				if (buttonText == null)
				{
					buttonText = buyButton.GetComponentInChildren<Text>();
				}
				buttonText.text = costText.text;
				buyButton.failure = true;
				buyButton.GetComponent<Button>().interactable = false;
				buyButton.GetComponent<Image>().color = Color.red;
				if (TryGetComponent<ShopButton>(out var component))
				{
					component.failure = true;
				}
			}
			else if (cost > money)
			{
				costText.text = "<color=red>" + MoneyText.DivideMoney(cost) + "P</color>";
				if (buttonText == null)
				{
					buttonText = buyButton.GetComponentInChildren<Text>();
				}
				buttonText.text = costText.text;
				buyButton.failure = true;
				buyButton.GetComponent<Button>().interactable = false;
				buyButton.GetComponent<Image>().color = Color.red;
			}
			else
			{
				costText.text = "<color=white>" + MoneyText.DivideMoney(cost) + "</color><color=orange>P</color>";
				if (buttonText == null)
				{
					buttonText = buyButton.GetComponentInChildren<Text>();
				}
				buttonText.text = costText.text;
				buyButton.failure = false;
				buyButton.GetComponent<Button>().interactable = true;
				buyButton.GetComponent<Image>().color = Color.white;
			}
			equipButton.gameObject.SetActive(value: false);
			return;
		}
		costText.text = "ALREADY OWNED";
		if (buttonText == null)
		{
			buttonText = buyButton.GetComponentInChildren<Text>();
		}
		buttonText.text = costText.text;
		buyButton.failure = true;
		buyButton.GetComponent<Button>().interactable = false;
		buyButton.GetComponent<Image>().color = Color.white;
		equipButton.gameObject.SetActive(value: true);
		equipButton.interactable = true;
		if (equipImage == null)
		{
			equipImage = equipButton.transform.GetChild(0).GetComponent<Image>();
		}
		int @int = MonoSingleton<PrefsManager>.Instance.GetInt("weapon." + weaponName, 1);
		if (@int == 2 && GameProgressSaver.CheckGear(weaponName.Substring(0, weaponName.Length - 1) + "alt") > 0)
		{
			equipStatus = 2;
		}
		else if (@int > 0)
		{
			equipStatus = 1;
		}
		else
		{
			equipStatus = 0;
		}
		if ((bool)orderButtons)
		{
			if (equipStatus != 0)
			{
				orderButtons.SetActive(value: true);
			}
			else
			{
				orderButtons.SetActive(value: false);
			}
		}
		equipImage.sprite = equipSprites[equipStatus];
		if (cost < 0 && TryGetComponent<ShopButton>(out var component2))
		{
			component2.failure = false;
		}
	}

	public void WeaponBought()
	{
		alreadyOwned = true;
		Object.Instantiate(buySound);
		GameProgressSaver.AddMoney(cost * -1);
		GameProgressSaver.AddGear(weaponName);
		MonoSingleton<PrefsManager>.Instance.SetInt("weapon." + weaponName, 1);
		UpdateMoney();
		MoneyText[] array = Object.FindObjectsOfType<MoneyText>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateMoney();
		}
		if (PlayerPrefs.GetInt("FirVar", 1) == 1)
		{
			GetComponentInParent<ShopZone>().firstVariationBuy = true;
		}
		if (gs == null)
		{
			gs = player.GetComponentInChildren<GunSetter>();
		}
		gs.ResetWeapons();
		gs.ForceWeapon(weaponName);
		gs.gunc.NoWeapon();
		if (fc == null)
		{
			fc = player.GetComponentInChildren<FistControl>();
		}
		fc.ResetFists();
	}

	public void ChangeEquipment(int value)
	{
		int num = equipStatus;
		num = ((value <= 0) ? (num - 1) : (num + 1));
		int num2 = num;
		if (num < 0)
		{
			num2 = ((GameProgressSaver.CheckGear(weaponName.Substring(0, weaponName.Length - 1) + "alt") <= 0) ? 1 : 2);
		}
		else if (num == 2)
		{
			num2 = ((GameProgressSaver.CheckGear(weaponName.Substring(0, weaponName.Length - 1) + "alt") > 0) ? 2 : 0);
		}
		else if (num > 2)
		{
			num2 = 0;
		}
		equipStatus = num2;
		equipImage.sprite = equipSprites[num2];
		MonoSingleton<PrefsManager>.Instance.SetInt("weapon." + weaponName, num2);
		if ((bool)orderButtons)
		{
			if (equipStatus == 0)
			{
				orderButtons.SetActive(value: false);
			}
			else
			{
				orderButtons.SetActive(value: true);
			}
		}
		if (gs == null)
		{
			gs = player.GetComponentInChildren<GunSetter>();
		}
		gs.ResetWeapons();
		if (fc == null)
		{
			fc = player.GetComponentInChildren<FistControl>();
		}
		fc.ResetFists();
	}
}
