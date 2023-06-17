using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomTextures : DirectoryTreeBrowser<FileInfo>
{
	private enum EditMode
	{
		None,
		Grid,
		Skybox,
		Emission
	}

	[SerializeField]
	private Material[] gridMaterials;

	[SerializeField]
	private OutdoorLightMaster olm;

	[SerializeField]
	private Material skyMaterial;

	[SerializeField]
	private Texture defaultGlow;

	[SerializeField]
	private Texture[] defaultGridTextures;

	[SerializeField]
	private Texture defaultEmission;

	[SerializeField]
	private Texture[] defaultSkyboxes;

	[SerializeField]
	private GameObject gridWrapper;

	[SerializeField]
	private Image gridBtnFrame;

	[SerializeField]
	private Image skyboxBtnFrame;

	[SerializeField]
	private Image emissionBtnFrame;

	private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();

	private EditMode currentEditMode;

	private bool editBase = true;

	private bool editTop = true;

	private bool editTopRow = true;

	[SerializeField]
	private Image baseBtnFrame;

	[SerializeField]
	private Image topBtnFrame;

	[SerializeField]
	private Image topRowBtnFrame;

	[SerializeField]
	private Slider glowSlider;

	private static readonly int EmissiveTex = Shader.PropertyToID("_EmissiveTex");

	public static readonly string[] AllowedExtensions = new string[3] { ".png", ".jpg", ".jpeg" };

	private string TexturesPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "CyberGrind", "Textures");

	protected override int maxPageLength => 14;

	protected override IDirectoryTree<FileInfo> baseDirectory => new FileDirectoryTree(TexturesPath);

	public bool TryToLoad(string key)
	{
		if (!File.Exists(Path.Combine(TexturesPath, key)))
		{
			Debug.LogError("Tried to load an invalid texture! " + key);
			return false;
		}
		LoadTexture(key);
		return true;
	}

	public void SetEditMode(int m)
	{
		GoToBase();
		EditMode editMode = (currentEditMode = (EditMode)m);
		switch (editMode)
		{
		case EditMode.Grid:
			gridBtnFrame.color = Color.red;
			skyboxBtnFrame.color = Color.white;
			emissionBtnFrame.color = Color.white;
			break;
		case EditMode.Skybox:
			gridBtnFrame.color = Color.white;
			skyboxBtnFrame.color = Color.red;
			emissionBtnFrame.color = Color.white;
			break;
		case EditMode.Emission:
			gridBtnFrame.color = Color.white;
			skyboxBtnFrame.color = Color.white;
			emissionBtnFrame.color = Color.red;
			break;
		}
		gridWrapper.SetActive(editMode != EditMode.None);
	}

	public void SetGridEditMode(int num)
	{
		switch (num)
		{
		case 0:
			if (editBase)
			{
				editBase = false;
			}
			else
			{
				editBase = true;
			}
			break;
		case 1:
			if (editTop)
			{
				editTop = false;
			}
			else
			{
				editTop = true;
			}
			break;
		case 2:
			if (editTopRow)
			{
				editTopRow = false;
			}
			else
			{
				editTopRow = true;
			}
			break;
		}
		if (editBase)
		{
			baseBtnFrame.color = Color.red;
		}
		else
		{
			baseBtnFrame.color = Color.white;
		}
		if (editTop)
		{
			topBtnFrame.color = Color.red;
		}
		else
		{
			topBtnFrame.color = Color.white;
		}
		if (editTopRow)
		{
			topRowBtnFrame.color = Color.red;
		}
		else
		{
			topRowBtnFrame.color = Color.white;
		}
	}

	public void SetTexture(string key)
	{
		switch (currentEditMode)
		{
		case EditMode.Grid:
			if (editBase)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGrid_" + 0, key);
				gridMaterials[0].mainTexture = imageCache[key];
			}
			if (editTop)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGrid_" + 1, key);
				gridMaterials[1].mainTexture = imageCache[key];
			}
			if (editTopRow)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGrid_" + 2, key);
				gridMaterials[2].mainTexture = imageCache[key];
			}
			break;
		case EditMode.Emission:
			if (editBase)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGlow_" + 0, key);
				gridMaterials[0].SetTexture(EmissiveTex, imageCache[key]);
			}
			if (editTop)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGlow_" + 1, key);
				gridMaterials[1].SetTexture(EmissiveTex, imageCache[key]);
			}
			if (editTopRow)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGlow_" + 2, key);
				gridMaterials[2].SetTexture(EmissiveTex, imageCache[key]);
			}
			break;
		case EditMode.Skybox:
			MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customSkybox", key);
			skyMaterial.mainTexture = imageCache[key];
			olm?.UpdateSkyboxMaterial();
			break;
		}
	}

	public void SetGlowIntensity()
	{
		MonoSingleton<EndlessGrid>.Instance.glowMultiplier = glowSlider.value;
		MonoSingleton<EndlessGrid>.Instance.UpdateGlow();
		MonoSingleton<PrefsManager>.Instance.SetFloatLocal("cyberGrind.glowIntensity", glowSlider.value);
	}

	private void Start()
	{
		string[] array = new int[3] { 0, 1, 2 }.Select((int i) => MonoSingleton<PrefsManager>.Instance.GetStringLocal("cyberGrind.customGrid_" + i)).ToArray();
		string[] array2 = new int[3] { 0, 1, 2 }.Select((int i) => MonoSingleton<PrefsManager>.Instance.GetStringLocal("cyberGrind.customGlow_" + i)).ToArray();
		for (int j = 0; j < array.Length; j++)
		{
			if (!string.IsNullOrEmpty(array[j]) && TryToLoad(array[j]))
			{
				gridMaterials[j].mainTexture = imageCache[array[j]];
			}
			else
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGrid_" + j);
			}
		}
		for (int k = 0; k < array2.Length; k++)
		{
			if (!string.IsNullOrEmpty(array2[k]) && TryToLoad(array2[k]))
			{
				gridMaterials[k].SetTexture(EmissiveTex, imageCache[array2[k]]);
				continue;
			}
			MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGlow_" + k);
			gridMaterials[k].SetTexture("_EmissiveTex", defaultGlow);
		}
		string stringLocal = MonoSingleton<PrefsManager>.Instance.GetStringLocal("cyberGrind.customSkybox");
		if (!string.IsNullOrEmpty(stringLocal) && TryToLoad(stringLocal))
		{
			skyMaterial.mainTexture = imageCache[stringLocal];
			olm?.UpdateSkyboxMaterial();
		}
		float floatLocal = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("cyberGrind.glowIntensity", -1f);
		if (floatLocal != -1f)
		{
			glowSlider.SetValueWithoutNotify(floatLocal);
			MonoSingleton<EndlessGrid>.Instance.glowMultiplier = glowSlider.value;
			MonoSingleton<EndlessGrid>.Instance.UpdateGlow();
		}
	}

	protected override Action BuildLeaf(FileInfo file, int indexInPage)
	{
		Texture2D texture2D = LoadTexture(file.FullName);
		GameObject btn = UnityEngine.Object.Instantiate(itemButtonTemplate, itemParent, worldPositionStays: false);
		Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f);
		sprite.texture.filterMode = FilterMode.Point;
		btn.GetComponent<Button>().onClick.RemoveAllListeners();
		btn.GetComponent<Button>().onClick.AddListener(delegate
		{
			SetTexture(file.FullName);
		});
		btn.GetComponent<Image>().sprite = sprite;
		btn.SetActive(value: true);
		return delegate
		{
			UnityEngine.Object.Destroy(btn);
		};
	}

	private Texture2D LoadTexture(string name)
	{
		if (imageCache.ContainsKey(name))
		{
			return imageCache[name];
		}
		byte[] data = File.ReadAllBytes(Path.Combine(TexturesPath, name));
		Texture2D texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, mipChain: false);
		texture2D.filterMode = FilterMode.Point;
		texture2D.LoadImage(data);
		imageCache[name] = texture2D;
		return texture2D;
	}

	public void RemoveCustomPrefs()
	{
		for (int i = 0; i < 3; i++)
		{
			MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGlow_" + i);
			MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGrid_" + i);
		}
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customSkybox");
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.glowIntensity");
	}

	public void ResetTexture()
	{
		int @int = MonoSingleton<PrefsManager>.Instance.GetInt("cyberGrind.theme");
		switch (currentEditMode)
		{
		case EditMode.Grid:
			if (editBase)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGrid_" + 0);
				gridMaterials[0].mainTexture = defaultGridTextures[@int];
			}
			if (editTop)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGrid_" + 1);
				gridMaterials[1].mainTexture = defaultGridTextures[@int];
			}
			if (editTopRow)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGrid_" + 2);
				gridMaterials[2].mainTexture = defaultGridTextures[@int];
			}
			break;
		case EditMode.Emission:
			if (editBase)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGlow_" + 0);
				gridMaterials[0].SetTexture(EmissiveTex, defaultEmission);
			}
			if (editTop)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGlow_" + 1);
				gridMaterials[1].SetTexture(EmissiveTex, defaultEmission);
			}
			if (editTopRow)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGlow_" + 2);
				gridMaterials[2].SetTexture(EmissiveTex, defaultEmission);
			}
			break;
		case EditMode.Skybox:
			skyMaterial.mainTexture = defaultSkyboxes[@int];
			olm?.UpdateSkyboxMaterial();
			MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customSkybox");
			break;
		}
	}
}
