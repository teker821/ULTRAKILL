using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-200)]
public abstract class MonoSingleton : MonoBehaviour
{
}
public abstract class MonoSingleton<T> : MonoSingleton where T : MonoSingleton<T>
{
	private static T instance;

	private static readonly SingletonFlags flags;

	public static T Instance
	{
		get
		{
			if (!(Object)instance)
			{
				return instance = Initialize();
			}
			return instance;
		}
		protected set
		{
			instance = value;
		}
	}

	static MonoSingleton()
	{
		flags = typeof(T).GetCustomAttribute<ConfigureSingletonAttribute>()?.Flags ?? SingletonFlags.None;
	}

	private static T Initialize()
	{
		if (flags.HasFlag(SingletonFlags.NoAutoInstance))
		{
			if (!SceneManager.GetActiveScene().isLoaded)
			{
				return Object.FindObjectOfType<T>();
			}
			return null;
		}
		GameObject gameObject = new GameObject(typeof(T).FullName);
		T result = gameObject.AddComponent<T>();
		if (flags.HasFlag(SingletonFlags.HideAutoInstance))
		{
			gameObject.hideFlags = HideFlags.HideAndDontSave;
		}
		if (flags.HasFlag(SingletonFlags.PersistAutoInstance))
		{
			Object.DontDestroyOnLoad(gameObject);
		}
		return result;
	}

	protected virtual void Awake()
	{
		if ((bool)(Object)instance && flags.HasFlag(SingletonFlags.DestroyDuplicates) && instance != this)
		{
			Object.Destroy(this);
		}
		else if (!flags.HasFlag(SingletonFlags.NoAwakeInstance) && (!(Object)instance || !instance.isActiveAndEnabled || base.isActiveAndEnabled))
		{
			Instance = (T)this;
		}
	}

	protected virtual void OnEnable()
	{
		Instance = (T)this;
	}

	protected virtual void OnDestroy()
	{
	}
}
