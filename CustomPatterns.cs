using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CustomPatterns : MonoBehaviour
{
	private Dictionary<string, ArenaPattern> patternCache = new Dictionary<string, ArenaPattern>();

	private Dictionary<string, ArenaPattern> enabledPatterns = new Dictionary<string, ArenaPattern>();

	private Dictionary<string, ArenaPattern[]> enabledPatternPacks = new Dictionary<string, ArenaPattern[]>();

	private Dictionary<string, GameObject> patternActiveIndicators = new Dictionary<string, GameObject>();

	private Dictionary<string, GameObject> patternPackActiveIndicators = new Dictionary<string, GameObject>();

	private int currentPage = 1;

	private int maxPages = 1;

	private int maxItemsPerPage = 15;

	[SerializeField]
	private AnimationCurve colorCurve;

	[SerializeField]
	private Texture2D parsingErrorTexture;

	[SerializeField]
	private GameObject buttonTemplate;

	[SerializeField]
	private GameObject packButtonTemplate;

	[SerializeField]
	private Transform grid;

	[SerializeField]
	private Text stateButtonText;

	[SerializeField]
	private Text pageText;

	public GameObject enableWhenCustom;

	private string PatternsPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "CyberGrind", "Patterns");

	private ArenaPattern[] AllEnabledPatterns
	{
		get
		{
			List<ArenaPattern> list = new List<ArenaPattern>();
			list.AddRange(enabledPatterns.Values.ToArray());
			foreach (ArenaPattern[] value in enabledPatternPacks.Values)
			{
				list.AddRange(value);
			}
			return list.ToArray();
		}
	}

	private void Awake()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(PatternsPath));
		Directory.CreateDirectory(PatternsPath);
		LoadEnabledPatterns();
		BuildButtons();
	}

	public void Toggle()
	{
		UnityEngine.Debug.Log("Toggling custom patterns");
		bool customPatternMode = MonoSingleton<EndlessGrid>.Instance.customPatternMode;
		MonoSingleton<EndlessGrid>.Instance.customPatternMode = !customPatternMode;
		stateButtonText.text = (customPatternMode ? "DISABLED" : "ENABLED");
		enableWhenCustom?.SetActive(!customPatternMode);
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("cyberGrind.customPool", MonoSingleton<EndlessGrid>.Instance.customPatternMode);
	}

	public void SaveEnabledPatterns()
	{
		ActivePatterns obj = new ActivePatterns
		{
			enabledPatterns = enabledPatterns.Keys.ToArray(),
			enabledPatternPacks = enabledPatternPacks.Keys.ToArray()
		};
		MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.enabledPatterns", JsonUtility.ToJson(obj));
	}

	public void LoadEnabledPatterns()
	{
		string stringLocal = MonoSingleton<PrefsManager>.Instance.GetStringLocal("cyberGrind.enabledPatterns");
		if (!string.IsNullOrEmpty(stringLocal))
		{
			ActivePatterns activePatterns = JsonUtility.FromJson<ActivePatterns>(stringLocal);
			enabledPatterns = new Dictionary<string, ArenaPattern>();
			enabledPatternPacks = new Dictionary<string, ArenaPattern[]>();
			if (activePatterns.enabledPatterns != null)
			{
				string[] array = activePatterns.enabledPatterns;
				foreach (string text in array)
				{
					string text2 = text;
					if (Path.GetFileName(text) != text)
					{
						text2 = Path.GetFileName(text);
					}
					ArenaPattern arenaPattern = LoadPattern(text2);
					if ((bool)arenaPattern)
					{
						enabledPatterns.Add(text2, arenaPattern);
					}
				}
			}
			if (activePatterns.enabledPatternPacks != null)
			{
				string[] array = activePatterns.enabledPatternPacks;
				foreach (string text3 in array)
				{
					if (Directory.Exists(Path.Combine(PatternsPath, text3)))
					{
						string[] array2 = (from f in Directory.GetFiles(Path.Combine(PatternsPath, text3))
							select Path.GetFileName(f)).ToArray();
						List<ArenaPattern> list = new List<ArenaPattern>();
						string[] array3 = array2;
						foreach (string path in array3)
						{
							ArenaPattern item = LoadPattern(Path.Combine(text3, path));
							list.Add(item);
						}
						enabledPatternPacks.Add(text3, list.ToArray());
					}
				}
			}
			MonoSingleton<EndlessGrid>.Instance.customPatterns = AllEnabledPatterns;
		}
		MonoSingleton<EndlessGrid>.Instance.customPatterns = AllEnabledPatterns;
		MonoSingleton<EndlessGrid>.Instance.customPatternMode = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("cyberGrind.customPool");
		bool flag = !MonoSingleton<EndlessGrid>.Instance.customPatternMode;
		stateButtonText.text = (flag ? "DISABLED" : "ENABLED");
		enableWhenCustom.SetActive(!flag);
	}

	public void BuildButtons()
	{
		GridTile[] collection = (from f in Directory.GetDirectories(PatternsPath, "*", SearchOption.TopDirectoryOnly)
			select new GridTile
			{
				path = Path.GetFileName(f),
				folder = true
			}).ToArray();
		GridTile[] collection2 = (from f in Directory.GetFiles(PatternsPath, "*.cgp", SearchOption.TopDirectoryOnly)
			select new GridTile
			{
				path = Path.GetFileName(f),
				folder = false
			}).ToArray();
		List<GridTile> list = new List<GridTile>();
		list.AddRange(collection);
		list.AddRange(collection2);
		for (int i = 2; i < grid.childCount; i++)
		{
			Object.Destroy(grid.GetChild(i).gameObject);
		}
		maxPages = Mathf.CeilToInt((float)list.Count / (float)maxItemsPerPage);
		for (int j = (currentPage - 1) * maxItemsPerPage; j < list.Count && j < currentPage * maxItemsPerPage; j++)
		{
			GridTile tile = list[j];
			if (tile.folder)
			{
				string[] array = (from f in Directory.GetFiles(Path.Combine(PatternsPath, tile.path))
					select Path.GetFileName(f)).ToArray();
				Texture2D target = new Texture2D(48, 48);
				for (int k = 0; k < 48; k++)
				{
					for (int l = 0; l < 48; l++)
					{
						target.SetPixel(k, l, Color.black);
					}
				}
				for (int m = 0; m < array.Length && m < 7; m++)
				{
					ArenaPattern pattern = LoadPattern(Path.Combine(tile.path, array[m]));
					Vector2Int offset = new Vector2Int(m % 3, (m > 2) ? 1 : 0);
					GeneratePatternPreview(pattern, offset, ref target);
				}
				target.Apply();
				GameObject gameObject = Object.Instantiate(packButtonTemplate, grid, worldPositionStays: false);
				Sprite sprite = Sprite.Create(target, new Rect(0f, 0f, 48f, 48f), new Vector2(0.5f, 0.5f), 100f);
				sprite.texture.filterMode = FilterMode.Point;
				gameObject.GetComponentInChildren<Text>(includeInactive: true).text = tile.path;
				gameObject.GetComponent<Image>().sprite = sprite;
				gameObject.GetComponent<ControllerPointer>().OnPressed.AddListener(delegate
				{
					TogglePattern(tile.path, isPack: true);
				});
				patternPackActiveIndicators[tile.path] = gameObject.transform.GetChild(0).gameObject;
				patternPackActiveIndicators[tile.path].SetActive(enabledPatternPacks.ContainsKey(tile.path));
				gameObject.SetActive(value: true);
				continue;
			}
			string path = list[j].path;
			ArenaPattern arenaPattern = LoadPattern(path);
			GameObject gameObject2 = Object.Instantiate(buttonTemplate, grid, worldPositionStays: false);
			if (arenaPattern == null)
			{
				Sprite sprite2 = Sprite.Create(parsingErrorTexture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 100f);
				sprite2.texture.filterMode = FilterMode.Point;
				gameObject2.GetComponent<Image>().sprite = sprite2;
				gameObject2.transform.GetChild(0).gameObject.SetActive(value: false);
				gameObject2.SetActive(value: true);
				continue;
			}
			Texture2D target2 = new Texture2D(16, 16);
			bool num = GeneratePatternPreview(arenaPattern, Vector2Int.zero, ref target2);
			target2.Apply();
			if (!num)
			{
				Sprite sprite3 = Sprite.Create(parsingErrorTexture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 100f);
				sprite3.texture.filterMode = FilterMode.Point;
				gameObject2.GetComponent<Image>().sprite = sprite3;
				gameObject2.transform.GetChild(0).gameObject.SetActive(value: false);
				gameObject2.SetActive(value: true);
				continue;
			}
			Sprite sprite4 = Sprite.Create(target2, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 100f);
			sprite4.texture.filterMode = FilterMode.Point;
			gameObject2.GetComponent<Image>().sprite = sprite4;
			gameObject2.SetActive(value: true);
			string key = Path.GetFileName(path);
			gameObject2.GetComponent<ControllerPointer>().OnPressed.AddListener(delegate
			{
				TogglePattern(key, isPack: false);
			});
			patternActiveIndicators[key] = gameObject2.transform.GetChild(0).gameObject;
			patternActiveIndicators[key].SetActive(enabledPatterns.ContainsKey(key));
		}
		pageText.text = $"{currentPage}/{maxPages}";
		buttonTemplate.SetActive(value: false);
	}

	private bool GeneratePatternPreview(ArenaPattern pattern, Vector2Int offset, ref Texture2D target)
	{
		string[] array = pattern.heights.Split('\n');
		if (array.Length != 16)
		{
			UnityEngine.Debug.LogError("[Heights] Pattern \"" + pattern.name + "\" has " + array.Length + " rows instead of " + 16);
			UnityEngine.Debug.Log(pattern.heights);
			return false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			int[] array2 = new int[16];
			if (array[i].Length != 16)
			{
				if (array[i].Length < 16)
				{
					UnityEngine.Debug.LogError("[Heights] Pattern \"" + pattern.name + "\" has " + array[i].Length + " elements in row " + i + " instead of " + 16);
					return false;
				}
				int num = 0;
				bool flag = false;
				string text = "";
				for (int j = 0; j < array[i].Length; j++)
				{
					if (int.TryParse(array[i][j].ToString(), out var result) || array[i][j] == '-')
					{
						if (!flag)
						{
							array2[num] = result;
							num++;
							continue;
						}
						text += array[i][j];
					}
					if (array[i][j] == '(')
					{
						if (flag)
						{
							UnityEngine.Debug.LogError("[Heights] Pattern \"" + pattern.name + "\", Error while parsing extended numbers!");
							return false;
						}
						flag = true;
					}
					if (array[i][j] == ')')
					{
						if (!flag)
						{
							UnityEngine.Debug.LogError("[Heights] Pattern \"" + pattern.name + "\", Error while parsing extended numbers!");
							return false;
						}
						array2[num] = int.Parse(text);
						flag = false;
						text = "";
						num++;
					}
				}
				if (num != 16)
				{
					UnityEngine.Debug.LogError("[Heights] Pattern \"" + pattern.name + "\" has " + array[i].Length + " elements in row " + num + " instead of " + 16);
					return false;
				}
			}
			else
			{
				for (int k = 0; k < array[i].Length; k++)
				{
					array2[k] = int.Parse(array[i][k].ToString());
				}
			}
			for (int l = 0; l < array2.Length; l++)
			{
				float num2 = colorCurve.Evaluate(array2[l]);
				target.SetPixel(offset.x * 16 + l, offset.y * 16 + i, new Color(num2, num2, num2));
			}
		}
		return true;
	}

	private void TogglePattern(string key, bool isPack)
	{
		if (isPack)
		{
			if (enabledPatternPacks.ContainsKey(key))
			{
				enabledPatternPacks.Remove(key);
				patternPackActiveIndicators[key].SetActive(value: false);
			}
			else if (Directory.Exists(Path.Combine(PatternsPath, key)))
			{
				string[] array = (from f in Directory.GetFiles(Path.Combine(PatternsPath, key))
					select Path.GetFileName(f)).ToArray();
				List<ArenaPattern> list = new List<ArenaPattern>();
				string[] array2 = array;
				foreach (string path in array2)
				{
					ArenaPattern item = LoadPattern(Path.Combine(key, path));
					list.Add(item);
				}
				enabledPatternPacks.Add(key, list.ToArray());
				enabledPatternPacks[key] = list.ToArray();
				patternPackActiveIndicators[key].SetActive(value: true);
			}
			if (!MonoSingleton<EndlessGrid>.Instance.customPatternMode)
			{
				Toggle();
			}
		}
		else
		{
			if (enabledPatterns.ContainsKey(key))
			{
				enabledPatterns.Remove(key);
				patternActiveIndicators[key].SetActive(value: false);
			}
			else
			{
				enabledPatterns[key] = LoadPattern(key);
				patternActiveIndicators[key].SetActive(value: true);
			}
			if (!MonoSingleton<EndlessGrid>.Instance.customPatternMode)
			{
				Toggle();
			}
		}
		MonoSingleton<EndlessGrid>.Instance.customPatterns = AllEnabledPatterns;
		SaveEnabledPatterns();
	}

	private ArenaPattern LoadPattern(string relativePath)
	{
		if (patternCache.ContainsKey(relativePath))
		{
			return patternCache[relativePath];
		}
		if (!File.Exists(Path.Combine(PatternsPath, relativePath)))
		{
			return null;
		}
		string[] array = File.ReadAllLines(Path.Combine(PatternsPath, relativePath));
		if (array.Length != 33)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 16; i++)
		{
			stringBuilder.Append(array[i]);
			if (i != 15)
			{
				stringBuilder.Append('\n');
			}
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int j = 17; j < 33; j++)
		{
			stringBuilder2.Append(array[j]);
			if (j != 32)
			{
				stringBuilder2.Append('\n');
			}
		}
		ArenaPattern arenaPattern = ScriptableObject.CreateInstance<ArenaPattern>();
		arenaPattern.heights = stringBuilder.ToString();
		arenaPattern.prefabs = stringBuilder2.ToString();
		arenaPattern.name = relativePath;
		patternCache[relativePath] = arenaPattern;
		return arenaPattern;
	}

	public void NextPage()
	{
		if (currentPage != maxPages)
		{
			currentPage++;
			BuildButtons();
		}
	}

	public void PreviousPage()
	{
		if (currentPage != 1)
		{
			currentPage--;
			BuildButtons();
		}
	}

	public void OpenEditor()
	{
		try
		{
			Process process = new Process();
			process.StartInfo.FileName = Path.Combine(Application.streamingAssetsPath, "cgef", "cgef.exe");
			process.Start();
		}
		catch
		{
			Application.OpenURL("https://cyber.pitr.dev/");
		}
	}
}
