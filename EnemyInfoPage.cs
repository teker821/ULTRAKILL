using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoPage : ListComponent<EnemyInfoPage>
{
	[SerializeField]
	private Text enemyPageTitle;

	[SerializeField]
	private Text enemyPageContent;

	[SerializeField]
	private Transform enemyPreviewWrapper;

	[Space]
	[SerializeField]
	private Transform enemyList;

	[SerializeField]
	private GameObject buttonTemplate;

	[SerializeField]
	private Image buttonTemplateBackground;

	[SerializeField]
	private Image buttonTemplateForeground;

	[SerializeField]
	private Sprite lockedSprite;

	[Space]
	[SerializeField]
	private SpawnableObjectsDatabase objects;

	private SpawnableObject currentSpawnable;

	private void Start()
	{
		UpdateInfo();
	}

	public void UpdateInfo()
	{
		if (enemyList.childCount > 1)
		{
			for (int num = enemyList.childCount - 1; num > 0; num--)
			{
				Object.Destroy(enemyList.GetChild(num).gameObject);
			}
		}
		SpawnableObject[] enemies = objects.enemies;
		foreach (SpawnableObject spawnableObject in enemies)
		{
			if (spawnableObject == null)
			{
				continue;
			}
			bool flag = true;
			if (MonoSingleton<BestiaryData>.Instance.GetEnemy(spawnableObject.enemyType) < 1)
			{
				flag = false;
			}
			if (flag)
			{
				buttonTemplateBackground.color = spawnableObject.backgroundColor;
				buttonTemplateForeground.sprite = spawnableObject.gridIcon;
			}
			else
			{
				buttonTemplateBackground.color = Color.gray;
				buttonTemplateForeground.sprite = lockedSprite;
			}
			GameObject gameObject = Object.Instantiate(buttonTemplate, enemyList);
			gameObject.SetActive(value: true);
			if (flag)
			{
				gameObject.GetComponentInChildren<ShopButton>().deactivated = false;
				gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
				{
					currentSpawnable = spawnableObject;
					DisplayInfo(spawnableObject);
				});
			}
			else
			{
				gameObject.GetComponentInChildren<ShopButton>().deactivated = true;
			}
		}
		buttonTemplate.SetActive(value: false);
	}

	private void SwapLayers(Transform target, int layer)
	{
		foreach (Transform item in target)
		{
			item.gameObject.layer = layer;
			if (item.childCount > 0)
			{
				SwapLayers(item, layer);
			}
		}
	}

	private void DisplayInfo(SpawnableObject source)
	{
		enemyPageTitle.text = source.objectName;
		string text = "<color=orange>TYPE: " + source.type + "\n\nDATA:</color>\n";
		text = ((MonoSingleton<BestiaryData>.Instance.GetEnemy(source.enemyType) <= 1) ? (text + "???") : (text + source.description));
		text = text + "\n\n<color=orange>STRATEGY:</color>\n" + source.strategy;
		enemyPageContent.text = text;
		enemyPageContent.rectTransform.localPosition = new Vector3(enemyPageContent.rectTransform.localPosition.x, 0f, enemyPageContent.rectTransform.localPosition.z);
		for (int i = 0; i < enemyPreviewWrapper.childCount; i++)
		{
			Object.Destroy(enemyPreviewWrapper.GetChild(i).gameObject);
		}
		GameObject gameObject = Object.Instantiate(source.preview, enemyPreviewWrapper);
		int layer = enemyPreviewWrapper.gameObject.layer;
		SwapLayers(gameObject.transform, layer);
		gameObject.layer = layer;
		gameObject.transform.localPosition = source.menuOffset;
		Spin spin = gameObject.AddComponent<Spin>();
		spin.spinDirection = new Vector3(0f, 1f, 0f);
		spin.speed = 10f;
	}

	public void DisplayInfo()
	{
		if (!(currentSpawnable == null))
		{
			DisplayInfo(currentSpawnable);
		}
	}

	public void UndisplayInfo()
	{
		currentSpawnable = null;
	}
}
