using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class AgonyController : MonoSingleton<AgonyController>
{
	[SerializeField]
	private GameObject reloadPrompt;

	private bool reloadAvailable;

	private string reloadPath;

	private FileSystemWatcher watcher;

	private GUIStyle style;

	private string agonyVersion;

	private void Start()
	{
	}

	private void OnGUI()
	{
	}

	private void SubscribeToMapFile(string path)
	{
		Debug.Log("Subscribing to " + path);
		watcher?.Dispose();
		watcher = new FileSystemWatcher(Path.GetDirectoryName(path))
		{
			NotifyFilter = (NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size),
			Filter = Path.GetFileName(path)
		};
		watcher.Changed += delegate
		{
			reloadAvailable = true;
		};
		watcher.EnableRaisingEvents = true;
	}

	private void OnApplicationQuit()
	{
		DismissReloadPrompt();
	}

	public void ResetAgony()
	{
		Debug.Log("Resetting AgonyController");
		watcher?.Dispose();
		reloadAvailable = false;
		reloadPath = null;
		DismissReloadPrompt();
	}

	public void CustomLocalMapLoaded(string path)
	{
		Debug.Log("Custom map loaded: " + path);
		ResetAgony();
		reloadPath = path;
		reloadAvailable = false;
		SubscribeToMapFile(path);
	}

	public void ShowReloadPrompt()
	{
		Debug.Log("Showing reload prompt");
		reloadPrompt.SetActive(value: true);
		reloadAvailable = true;
	}

	public void DismissReloadPrompt()
	{
		Debug.Log("Dismissing reload prompt");
		reloadPrompt.SetActive(value: false);
	}

	private void Update()
	{
		if (reloadAvailable && !reloadPrompt.activeSelf)
		{
			ShowReloadPrompt();
		}
		if (reloadAvailable && Input.GetKeyDown(KeyCode.Return))
		{
			Debug.Log("Reloading map");
			StartCoroutine(ReloadMap());
		}
	}

	private IEnumerator ReloadMap()
	{
		SceneHelper.ShowLoadingBlocker();
		string path = reloadPath;
		reloadAvailable = false;
		Scene activeScene = SceneManager.GetActiveScene();
		Scene activeScene2 = SceneManager.CreateScene("Empty");
		yield return SceneManager.SetActiveScene(activeScene2);
		yield return SceneManager.UnloadSceneAsync(activeScene);
		Debug.Log("Unloaded scene " + activeScene.name);
		yield return MonoSingleton<SceneHelper>.Instance.LoadCustomMapAsync(path, GameStateManager.Instance.currentCustomGame);
		ResetAgony();
	}
}
