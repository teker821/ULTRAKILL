using System.Collections.Generic;
using System.Globalization;
using Sandbox.Arm;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SandboxAlterMenu : MonoSingleton<SandboxAlterMenu>
{
	[SerializeField]
	private GameObject shadow;

	[SerializeField]
	private GameObject menu;

	[Space]
	[SerializeField]
	private Text nameText;

	[Space]
	[SerializeField]
	private Toggle uniformSize;

	[SerializeField]
	private InputField sizeField;

	[SerializeField]
	private InputField sizeFieldX;

	[SerializeField]
	private InputField sizeFieldY;

	[SerializeField]
	private InputField sizeFieldZ;

	[Space]
	[SerializeField]
	private Toggle radianceEnabled;

	[SerializeField]
	private Slider radianceTier;

	[SerializeField]
	private Slider radianceHealth;

	[SerializeField]
	private Slider radianceDamage;

	[SerializeField]
	private Slider radianceSpeed;

	[Space]
	[SerializeField]
	private GameObject sizeContainer;

	[SerializeField]
	private GameObject uniformContainer;

	[SerializeField]
	private Toggle frozenCheckbox;

	[SerializeField]
	private GameObject splitContainer;

	[SerializeField]
	private GameObject enemyOptionsContainer;

	[SerializeField]
	private GameObject radianceSettings;

	[Space]
	[SerializeField]
	private GameObject scaleUpSound;

	[SerializeField]
	private GameObject scaleDownSound;

	[SerializeField]
	private GameObject scaleResetSound;

	[Space]
	[SerializeField]
	private AlterMenuElements elementManager;

	private SandboxSpawnableInstance editedObject;

	private AlterMode alterInstance;

	public Vector3 SafeSize(Vector3 originalSize)
	{
		float min = 0.00390625f;
		float max = 128f;
		float x = Mathf.Clamp(originalSize.x, min, max);
		float y = Mathf.Clamp(originalSize.y, min, max);
		float z = Mathf.Clamp(originalSize.z, min, max);
		return new Vector3(x, y, z);
	}

	protected override void Awake()
	{
		base.Awake();
		sizeFieldX.onValueChanged.AddListener(SetSizeX);
		sizeFieldY.onValueChanged.AddListener(SetSizeY);
		sizeFieldZ.onValueChanged.AddListener(SetSizeZ);
		sizeField.onValueChanged.AddListener(SetSize);
		sizeFieldX.onEndEdit.AddListener(delegate
		{
			UpdateSizeValues();
		});
		sizeFieldY.onEndEdit.AddListener(delegate
		{
			UpdateSizeValues();
		});
		sizeFieldZ.onEndEdit.AddListener(delegate
		{
			UpdateSizeValues();
		});
		sizeField.onEndEdit.AddListener(delegate
		{
			UpdateSizeValues();
		});
	}

	private void SetSizeX(string value)
	{
		if (!(editedObject == null))
		{
			Vector3 localScale = editedObject.transform.localScale;
			if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				editedObject.transform.localScale = SafeSize(new Vector3(result, localScale.y, localScale.z));
			}
		}
	}

	private void SetSizeY(string value)
	{
		if (!(editedObject == null))
		{
			Vector3 localScale = editedObject.transform.localScale;
			Debug.Log(value);
			if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				editedObject.transform.localScale = SafeSize(new Vector3(localScale.x, result, localScale.z));
			}
		}
	}

	private void SetSizeZ(string value)
	{
		if (!(editedObject == null))
		{
			Vector3 localScale = editedObject.transform.localScale;
			if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				editedObject.transform.localScale = SafeSize(new Vector3(localScale.x, localScale.y, result));
			}
		}
	}

	private void SetSize(string value)
	{
		if (!(editedObject == null) && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			editedObject.transform.localScale = SafeSize(new Vector3(result, result, result));
		}
	}

	public void SetJumpPadPower(float value)
	{
		if (!(editedObject == null))
		{
			JumpPad componentInChildren = editedObject.GetComponentInChildren<JumpPad>();
			if (!(componentInChildren == null))
			{
				componentInChildren.force = value;
			}
		}
	}

	public void SetFrozen(bool frozen)
	{
		if ((bool)editedObject)
		{
			editedObject.frozen = frozen;
		}
	}

	public void SetRadianceTier(float value)
	{
		if (editedObject is SandboxEnemy sandboxEnemy)
		{
			sandboxEnemy.radiance.tier = value;
			sandboxEnemy.UpdateRadiance();
		}
	}

	public void SetHealthBuff(float value)
	{
		if (editedObject is SandboxEnemy sandboxEnemy)
		{
			sandboxEnemy.radiance.healthBuff = value;
			sandboxEnemy.UpdateRadiance();
		}
	}

	public void SetDamageBuff(float value)
	{
		if (editedObject is SandboxEnemy sandboxEnemy)
		{
			Debug.Log("Setting Damage Buff: " + value);
			sandboxEnemy.radiance.damageBuff = value;
			sandboxEnemy.UpdateRadiance();
		}
	}

	public void SetSpeedBuff(float value)
	{
		if (editedObject is SandboxEnemy sandboxEnemy)
		{
			sandboxEnemy.radiance.speedBuff = value;
			sandboxEnemy.UpdateRadiance();
		}
	}

	public void ShowRadianceOptions(bool value)
	{
		radianceSettings.SetActive(value);
		if (editedObject is SandboxEnemy sandboxEnemy)
		{
			if (sandboxEnemy.radiance == null)
			{
				sandboxEnemy.radiance = new EnemyRadianceConfig(sandboxEnemy.enemyId);
			}
			sandboxEnemy.radiance.enabled = value;
			if (value)
			{
				Debug.Log("Loading Damage Buff: " + sandboxEnemy.radiance.damageBuff);
				radianceEnabled.SetIsOnWithoutNotify(value: true);
				radianceTier.SetValueWithoutNotify(sandboxEnemy.radiance.tier);
				radianceDamage.SetValueWithoutNotify(sandboxEnemy.radiance.damageBuff);
				radianceHealth.SetValueWithoutNotify(sandboxEnemy.radiance.healthBuff);
				radianceSpeed.SetValueWithoutNotify(sandboxEnemy.radiance.speedBuff);
			}
			else
			{
				radianceEnabled.SetIsOnWithoutNotify(value: false);
			}
			sandboxEnemy.UpdateRadiance();
		}
	}

	public void ShowUniformSizeMenu(bool value)
	{
		uniformContainer.SetActive(value);
		splitContainer.SetActive(!value);
		sizeFieldX.interactable = !value;
		sizeFieldY.interactable = !value;
		sizeFieldZ.interactable = !value;
		sizeField.interactable = value;
		if (value)
		{
			editedObject.transform.localScale = Vector3.one * editedObject.transform.localScale.y;
		}
		UpdateSizeValues();
	}

	public void DefaultSize()
	{
		if (!(editedObject == null))
		{
			editedObject.transform.localScale = editedObject.defaultSize * Vector3.one;
			UpdateSizeValues();
			Object.Instantiate(scaleResetSound, base.transform.position, Quaternion.identity);
		}
	}

	public void MultiplySize(float value)
	{
		if (!(editedObject == null))
		{
			Vector3 localScale = editedObject.transform.localScale;
			localScale *= value;
			localScale = SafeSize(localScale);
			editedObject.transform.localScale = localScale;
			ShowUniformSizeMenu(uniformContainer.activeSelf);
			UpdateSizeValues();
			Object.Instantiate((value > 1f) ? scaleUpSound : scaleDownSound, editedObject.transform.position, Quaternion.identity);
		}
	}

	public void UpdateSizeValues()
	{
		if (uniformContainer.activeSelf)
		{
			sizeField.SetTextWithoutNotify((editedObject.transform.localScale.x / editedObject.defaultSize).ToString(CultureInfo.InvariantCulture));
			return;
		}
		sizeFieldX.SetTextWithoutNotify((editedObject.transform.localScale.x / editedObject.defaultSize).ToString(CultureInfo.InvariantCulture));
		sizeFieldY.SetTextWithoutNotify((editedObject.transform.localScale.y / editedObject.defaultSize).ToString(CultureInfo.InvariantCulture));
		sizeFieldZ.SetTextWithoutNotify((editedObject.transform.localScale.z / editedObject.defaultSize).ToString(CultureInfo.InvariantCulture));
	}

	public void Show(SandboxSpawnableInstance prop, AlterMode instance)
	{
		prop.Pause(freeze: false);
		shadow.SetActive(value: true);
		elementManager.Reset();
		menu.SetActive(value: true);
		frozenCheckbox.SetIsOnWithoutNotify(prop.frozen);
		nameText.text = prop.name;
		editedObject = prop;
		alterInstance = instance;
		GameStateManager.Instance.RegisterState(new GameState("alter-menu", menu)
		{
			cursorLock = LockMode.Unlock,
			cameraInputLock = LockMode.Lock,
			playerInputLock = LockMode.Unlock
		});
		MonoSingleton<CameraController>.Instance.activated = false;
		MonoSingleton<GunControl>.Instance.activated = false;
		bool flag = !(prop is BrushBlock);
		sizeContainer.SetActive(flag);
		if (flag)
		{
			ShowUniformSizeMenu(prop.uniformSize);
		}
		if (prop is SandboxEnemy sandboxEnemy)
		{
			ShowRadianceOptions(sandboxEnemy.radiance.enabled);
			enemyOptionsContainer.SetActive(value: true);
		}
		else
		{
			enemyOptionsContainer.SetActive(value: false);
			radianceSettings.SetActive(value: false);
		}
		IAlter[] componentsInChildren = prop.GetComponentsInChildren<IAlter>();
		List<string> list = new List<string>();
		IAlter[] array = componentsInChildren;
		foreach (IAlter alter in array)
		{
			if (list.Contains(alter.alterKey))
			{
				continue;
			}
			if (alter.allowOnlyOne)
			{
				list.Add(alter.alterKey);
			}
			elementManager.CreateTitle(alter.alterCategoryName);
			if (alter is IAlterOptions<bool> alterOptions)
			{
				AlterOption<bool>[] options = alterOptions.options;
				foreach (AlterOption<bool> alterOption in options)
				{
					elementManager.CreateBoolRow(alterOption.name, alterOption.value, alterOption.callback);
				}
			}
			if (alter is IAlterOptions<float> alterOptions2)
			{
				AlterOption<float>[] options2 = alterOptions2.options;
				foreach (AlterOption<float> alterOption2 in options2)
				{
					elementManager.CreateFloatRow(alterOption2.name, alterOption2.value, alterOption2.callback, alterOption2.constraints);
				}
			}
		}
	}

	public void Close()
	{
		shadow.SetActive(value: false);
		menu.SetActive(value: false);
		editedObject = null;
		alterInstance.EndSession();
		MonoSingleton<CameraController>.Instance.activated = true;
		MonoSingleton<GunControl>.Instance.activated = true;
	}

	private void Update()
	{
		if (editedObject == null && menu.activeSelf)
		{
			Close();
		}
		if (!menu.activeSelf && shadow.activeSelf)
		{
			alterInstance.EndSession();
			shadow.SetActive(value: false);
			editedObject = null;
		}
	}
}
