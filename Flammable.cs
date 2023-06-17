using UnityEngine;
using UnityEngine.Events;

public class Flammable : MonoBehaviour
{
	public float heat;

	public GameObject fire;

	public GameObject simpleFire;

	private GameObject currentFire;

	private AudioSource currentFireAud;

	private Light currentFireLight;

	public bool burning;

	private bool fading;

	public bool secondary;

	private bool enemy;

	private EnemyIdentifierIdentifier eidid;

	private Flammable[] flammables;

	public bool wet;

	public bool playerOnly;

	public bool specialFlammable;

	public UnityEvent onSpecialActivate;

	private Collider col;

	private void Start()
	{
		if (base.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
		{
			enemy = true;
			eidid = component;
		}
	}

	private void OnEnable()
	{
		if (burning)
		{
			Pulse();
		}
	}

	private void OnDisable()
	{
		CancelInvoke("Pulse");
	}

	public void Burn(float newHeat, bool noInstaDamage = false)
	{
		if (specialFlammable)
		{
			onSpecialActivate?.Invoke();
		}
		else
		{
			if ((enemy && (bool)eidid && (bool)eidid.eid && eidid.eid.blessed) || wet)
			{
				return;
			}
			if (col == null)
			{
				col = GetComponent<Collider>();
			}
			if (col != null)
			{
				if (newHeat > heat)
				{
					heat = newHeat;
				}
				if (currentFire == null)
				{
					if (!secondary)
					{
						currentFire = Object.Instantiate(fire);
					}
					else
					{
						currentFire = Object.Instantiate(simpleFire);
					}
					currentFire.transform.position = col.bounds.center;
					currentFire.transform.localScale = col.bounds.size;
					currentFire.transform.SetParent(base.transform, worldPositionStays: true);
					currentFireAud = currentFire.GetComponentInChildren<AudioSource>();
					if (!secondary && !MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleFire"))
					{
						currentFireLight = currentFire.GetComponent<Light>();
						currentFireLight.enabled = true;
					}
				}
				if (!secondary)
				{
					if (enemy && (!burning || !noInstaDamage))
					{
						if ((bool)eidid && (bool)eidid.eid && !eidid.eid.burners.Contains(this))
						{
							eidid.eid.burners.Add(this);
						}
						burning = true;
						CancelInvoke("Pulse");
						Pulse();
					}
					flammables = GetComponentsInChildren<Flammable>();
					Flammable[] array = flammables;
					foreach (Flammable flammable in array)
					{
						if (flammable != this)
						{
							flammable.secondary = true;
							flammable.Burn(heat);
							flammable.Pulse();
						}
					}
				}
				burning = true;
			}
			else
			{
				Object.Destroy(this);
			}
		}
	}

	private void Pulse()
	{
		if (burning)
		{
			if (enemy && !secondary && eidid != null && eidid.eid != null)
			{
				if (eidid.eid.blessed)
				{
					PutOut(getWet: false);
					return;
				}
				eidid.eid.hitter = "fire";
				eidid.eid.DeliverDamage(eidid.gameObject, Vector3.zero, eidid.transform.position, 0.2f, tryForExplode: false);
			}
			heat -= 0.25f;
			if (heat <= 0f)
			{
				burning = false;
				fading = true;
			}
			Invoke("Pulse", 0.5f);
		}
		else
		{
			if (!fading || !(currentFire != null))
			{
				return;
			}
			if (currentFire != null)
			{
				currentFire.transform.localScale *= 0.75f;
				if (currentFireAud == null)
				{
					currentFireAud = currentFire.GetComponentInChildren<AudioSource>();
				}
				currentFireAud.volume *= 0.75f;
				if (!secondary && currentFireLight != null)
				{
					currentFireLight.range *= 0.75f;
				}
			}
			if (currentFire.transform.localScale.x < 0.1f)
			{
				fading = false;
				Object.Destroy(currentFire);
			}
			else
			{
				Invoke("Pulse", Random.Range(0.25f, 0.5f));
			}
		}
	}

	public void PutOut(bool getWet = true)
	{
		wet = getWet;
		if ((bool)currentFire)
		{
			heat = 0f;
			burning = false;
			fading = false;
			Object.Destroy(currentFire);
		}
		if (secondary || flammables == null)
		{
			return;
		}
		Flammable[] array = flammables;
		foreach (Flammable flammable in array)
		{
			if (flammable != this)
			{
				flammable.PutOut();
			}
		}
	}

	private void OnDestroy()
	{
		if (currentFire != null)
		{
			Object.Destroy(currentFire);
		}
	}
}
