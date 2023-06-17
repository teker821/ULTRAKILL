using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
	public DoorType doorType;

	private BigDoor[] bdoors;

	private SubDoor[] subdoors;

	public bool open;

	public bool gotPos;

	public Vector3 closedPos;

	public Vector3 openPos;

	[HideInInspector]
	public Vector3 openPosRelative;

	public bool startOpen;

	[HideInInspector]
	public Vector3 targetPos;

	public float speed;

	[HideInInspector]
	public bool inPos = true;

	public bool reverseDirection;

	public int requests;

	private AudioSource aud;

	public AudioClip openSound;

	public AudioClip closeSound;

	private AudioSource aud2;

	private MaterialPropertyBlock block;

	public bool locked;

	public GameObject noPass;

	private NavMeshObstacle nmo;

	public GameObject[] activatedRooms;

	public GameObject[] deactivatedRooms;

	public Light openLight;

	public UnityEvent onFullyOpened;

	private Door[] allDoors;

	public bool screenShake;

	public bool dontCloseWhenAnotherDoorOpens;

	public bool dontCloseOtherDoorsWhenOpening;

	private CameraController cc;

	private DoorLock thisLock;

	private List<DoorLock> locks = new List<DoorLock>();

	private int openLocks;

	[HideInInspector]
	public DoorController[] docons;

	[HideInInspector]
	public List<bool> origDoconStates = new List<bool>();

	private bool doconless;

	private Collider doconlessClosingCol;

	private MeshRenderer[] lightsMeshRenderers;

	public Color defaultLightsColor;

	public Color currentLightsColor;

	private OcclusionPortal occpor;

	public bool ignoreHookCheck;

	public bool openOnUnlock;

	private void Awake()
	{
		block = new MaterialPropertyBlock();
		lightsMeshRenderers = GetComponentsInChildren<MeshRenderer>();
		nmo = GetComponent<NavMeshObstacle>();
		occpor = GetComponent<OcclusionPortal>();
		if (!occpor)
		{
			occpor = GetComponentInChildren<OcclusionPortal>();
		}
		if (requests == 1)
		{
			requests = 0;
			startOpen = true;
		}
		if (nmo != null && startOpen)
		{
			nmo.enabled = false;
		}
		if (doorType == DoorType.Normal)
		{
			aud = GetComponent<AudioSource>();
			if (!gotPos)
			{
				GetPos();
			}
			else if (openPosRelative == Vector3.zero)
			{
				openPosRelative = base.transform.localPosition + openPos;
			}
		}
		else if (doorType == DoorType.BigDoorController)
		{
			bdoors = GetComponentsInChildren<BigDoor>();
			if (startOpen && !open)
			{
				BigDoor[] array = bdoors;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].open = true;
				}
			}
		}
		else if (doorType == DoorType.SubDoorController)
		{
			subdoors = GetComponentsInChildren<SubDoor>();
			if (startOpen)
			{
				SubDoor[] array2 = subdoors;
				foreach (SubDoor subDoor in array2)
				{
					subDoor.SetValues();
					if (subDoor.transform.localPosition == subDoor.origPos)
					{
						subDoor.transform.localPosition = subDoor.origPos + subDoor.openPos;
						subDoor.Open();
					}
				}
			}
		}
		if (noPass != null)
		{
			aud2 = base.transform.GetChild(0).GetComponent<AudioSource>();
			if (!aud2)
			{
				aud2 = GetComponentInChildren<AudioSource>();
			}
		}
		if (openLight != null && !startOpen)
		{
			openLight.enabled = false;
		}
		DoorLock[] componentsInChildren = GetComponentsInChildren<DoorLock>();
		if (componentsInChildren.Length != 0)
		{
			DoorLock[] array3 = componentsInChildren;
			foreach (DoorLock doorLock in array3)
			{
				if (doorLock.gameObject == base.gameObject)
				{
					thisLock = doorLock;
					continue;
				}
				locks.Add(doorLock);
				doorLock.parentDoor = this;
			}
		}
		if (doorType == DoorType.BigDoorController || doorType == DoorType.SubDoorController)
		{
			docons = GetComponentsInChildren<DoorController>();
		}
		else if (base.transform.parent != null)
		{
			docons = base.transform.parent.GetComponentsInChildren<DoorController>();
		}
		if (docons == null)
		{
			docons = new DoorController[0];
		}
		if (((componentsInChildren.Length != 0 && thisLock == null) || componentsInChildren.Length > 1) && docons.Length != 0)
		{
			for (int j = 0; j < docons.Length; j++)
			{
				if (docons[j].gameObject.activeInHierarchy)
				{
					origDoconStates.Add(item: true);
				}
				else
				{
					origDoconStates.Add(item: false);
				}
				docons[j].gameObject.SetActive(value: false);
			}
		}
		else if ((docons == null || docons.Length == 0) && base.gameObject.layer != 8 && base.gameObject.layer != 24)
		{
			doconlessClosingCol = GetComponent<Collider>();
			if ((bool)doconlessClosingCol && !doconlessClosingCol.isTrigger)
			{
				doconless = true;
				if (!startOpen && (doorType != DoorType.BigDoorController || bdoors.Length != 0) && (doorType != DoorType.SubDoorController || subdoors.Length != 0))
				{
					doconlessClosingCol.enabled = true;
				}
				else
				{
					doconlessClosingCol.enabled = false;
				}
			}
		}
		MeshRenderer[] array4 = lightsMeshRenderers;
		foreach (MeshRenderer meshRenderer in array4)
		{
			if ((bool)meshRenderer && (bool)meshRenderer.sharedMaterial && meshRenderer.sharedMaterial.HasProperty(UKShaderProperties.EmissiveColor))
			{
				meshRenderer.GetPropertyBlock(block);
				defaultLightsColor = meshRenderer.sharedMaterial.GetColor(UKShaderProperties.EmissiveColor);
				if (noPass != null && noPass.activeInHierarchy)
				{
					block.SetColor(UKShaderProperties.EmissiveColor, Color.red);
				}
				meshRenderer.SetPropertyBlock(block);
			}
		}
		currentLightsColor = defaultLightsColor;
		if (startOpen)
		{
			open = true;
			if ((bool)occpor)
			{
				occpor.open = true;
			}
		}
		else if ((bool)occpor)
		{
			occpor.open = false;
		}
	}

	private void GetPos()
	{
		gotPos = true;
		closedPos = base.transform.localPosition;
		openPosRelative = base.transform.localPosition + openPos;
		if (startOpen)
		{
			base.transform.localPosition = openPosRelative;
		}
	}

	public void AltarControlled()
	{
		doconlessClosingCol = GetComponent<Collider>();
		DoorController[] array = docons;
		foreach (DoorController doorController in array)
		{
			if (!doorController.dontDeactivateOnAltarControl)
			{
				doorController.gameObject.SetActive(value: false);
			}
		}
		if ((bool)doconlessClosingCol && !doconlessClosingCol.isTrigger)
		{
			doconless = true;
			if (!startOpen)
			{
				doconlessClosingCol.enabled = true;
			}
		}
	}

	private void Update()
	{
		if (doorType != 0 || inPos)
		{
			return;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, targetPos, Time.deltaTime * speed);
		if (screenShake)
		{
			if (cc == null)
			{
				cc = MonoSingleton<CameraController>.Instance;
			}
			cc.CameraShake(0.05f);
		}
		if (!(Vector3.Distance(base.transform.localPosition, targetPos) < 0.1f))
		{
			return;
		}
		base.transform.localPosition = targetPos;
		inPos = true;
		onFullyOpened?.Invoke();
		if (base.transform.localPosition == closedPos && (bool)openLight)
		{
			openLight.enabled = false;
		}
		if (closeSound != null && (bool)aud)
		{
			aud.clip = closeSound;
			aud.loop = false;
			aud.Play();
		}
		if (nmo != null)
		{
			if (base.transform.localPosition == closedPos)
			{
				nmo.enabled = true;
			}
			else
			{
				nmo.enabled = false;
			}
		}
		if ((bool)occpor && base.transform.localPosition == closedPos)
		{
			occpor.open = false;
		}
	}

	public void SimpleOpenOverride()
	{
		Open(enemy: false, skull: true);
	}

	public void Open(bool enemy = false, bool skull = false)
	{
		if (!gotPos)
		{
			GetPos();
		}
		if (!skull || docons.Length == 0)
		{
			requests++;
		}
		else if (skull && docons.Length != 0)
		{
			bool flag = false;
			DoorController[] array = docons;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].gameObject.activeInHierarchy)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				requests++;
			}
		}
		if (!(requests == 1 || skull) || (!(base.transform.localPosition != openPosRelative) && doorType == DoorType.Normal) || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		open = true;
		if ((bool)occpor)
		{
			occpor.open = true;
		}
		if (!enemy && !dontCloseOtherDoorsWhenOpening && docons.Length != 0)
		{
			allDoors = Object.FindObjectsOfType<Door>();
			Door[] array2 = allDoors;
			foreach (Door door in array2)
			{
				if (door != null && door != this && door.open && !door.startOpen && !door.dontCloseWhenAnotherDoorOpens)
				{
					DoorController doorController = null;
					if (door.doorType != 0)
					{
						doorController = door.GetComponentInChildren<DoorController>();
					}
					else if (door.transform.parent != null)
					{
						doorController = door.transform.parent.GetComponentInChildren<DoorController>();
					}
					if (doorController != null && doorController.type == 0 && !doorController.enemyIn)
					{
						door.Close();
					}
				}
			}
		}
		if (doorType == DoorType.Normal)
		{
			if (aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			if ((bool)aud)
			{
				aud.clip = openSound;
				if (closeSound != null)
				{
					aud.loop = true;
				}
				aud.Play();
			}
			targetPos = openPosRelative;
			inPos = false;
		}
		else
		{
			if (doorType == DoorType.BigDoorController)
			{
				BigDoor[] array3 = bdoors;
				foreach (BigDoor bigDoor in array3)
				{
					if (reverseDirection)
					{
						bigDoor.reverseDirection = true;
					}
					else
					{
						bigDoor.reverseDirection = false;
					}
					bigDoor.Open();
				}
			}
			else if (doorType == DoorType.SubDoorController)
			{
				SubDoor[] array4 = subdoors;
				foreach (SubDoor subDoor in array4)
				{
					if (!subDoor.dr)
					{
						subDoor.dr = this;
					}
					subDoor.Open();
				}
			}
			if ((bool)nmo)
			{
				nmo.enabled = false;
			}
		}
		if (activatedRooms.Length != 0)
		{
			GameObject[] array5 = activatedRooms;
			foreach (GameObject gameObject in array5)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: true);
				}
			}
		}
		if (openLight != null)
		{
			openLight.enabled = true;
		}
		if (thisLock != null)
		{
			thisLock.Open();
		}
		if (doconless && (bool)doconlessClosingCol)
		{
			doconlessClosingCol.enabled = false;
		}
	}

	public void Optimize()
	{
		if (deactivatedRooms.Length != 0)
		{
			GameObject[] array = deactivatedRooms;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
		}
	}

	public void Close(bool force = false)
	{
		if (!gotPos)
		{
			GetPos();
		}
		if (requests > 1 && !force)
		{
			requests--;
		}
		else
		{
			if (!(base.transform.localPosition != closedPos) && doorType == DoorType.Normal)
			{
				return;
			}
			open = false;
			if (requests > 0 && !force)
			{
				requests--;
			}
			else if (force)
			{
				requests = 0;
			}
			if (startOpen)
			{
				startOpen = false;
			}
			if (doorType == DoorType.Normal)
			{
				if (aud == null)
				{
					aud = GetComponent<AudioSource>();
				}
				if (aud != null)
				{
					aud.clip = openSound;
					if (closeSound != null)
					{
						aud.loop = true;
					}
					aud.Play();
				}
				targetPos = closedPos;
				inPos = false;
			}
			else if (doorType == DoorType.BigDoorController && bdoors != null)
			{
				BigDoor[] array = bdoors;
				foreach (BigDoor bigDoor in array)
				{
					bigDoor.Close();
					if (openLight != null)
					{
						bigDoor.openLight = openLight;
					}
				}
				if ((bool)nmo)
				{
					nmo.enabled = true;
				}
			}
			else if (doorType == DoorType.SubDoorController && subdoors != null)
			{
				SubDoor[] array2 = subdoors;
				foreach (SubDoor subDoor in array2)
				{
					if (!subDoor.dr)
					{
						subDoor.dr = this;
					}
					subDoor.Close();
				}
				if ((bool)nmo)
				{
					nmo.enabled = true;
				}
			}
			if (thisLock != null)
			{
				thisLock.Close();
			}
			if (doconless && (bool)doconlessClosingCol)
			{
				doconlessClosingCol.enabled = true;
			}
		}
	}

	public void Lock()
	{
		if (locked)
		{
			return;
		}
		locked = true;
		if ((bool)noPass)
		{
			noPass.SetActive(value: true);
		}
		if (doorType == DoorType.Normal)
		{
			if (base.transform.localPosition != closedPos)
			{
				Close(force: true);
			}
		}
		else if (doorType == DoorType.BigDoorController)
		{
			BigDoor[] array = bdoors;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].open)
				{
					Close(force: true);
					break;
				}
			}
		}
		else if (doorType == DoorType.SubDoorController)
		{
			SubDoor[] array2 = subdoors;
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i].isOpen)
				{
					Close(force: true);
					break;
				}
			}
		}
		if ((bool)aud2)
		{
			aud2.pitch = 0.2f;
			aud2.Play();
		}
		ChangeColor(Color.red);
	}

	public void Unlock()
	{
		if (locked && (bool)aud2)
		{
			aud2.pitch = 0.5f;
			aud2.Play();
		}
		locked = false;
		if ((bool)noPass)
		{
			noPass.SetActive(value: false);
		}
		ChangeColor(defaultLightsColor);
		if (openOnUnlock && !open)
		{
			Open();
		}
	}

	public void ChangeColor(Color targetColor)
	{
		currentLightsColor = targetColor;
		if (lightsMeshRenderers == null || lightsMeshRenderers.Length == 0)
		{
			return;
		}
		MeshRenderer[] array = lightsMeshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if ((bool)meshRenderer && meshRenderer.sharedMaterial.HasProperty(UKShaderProperties.EmissiveColor))
			{
				meshRenderer.GetPropertyBlock(block);
				block.SetColor(UKShaderProperties.EmissiveColor, targetColor);
				meshRenderer.SetPropertyBlock(block);
			}
		}
	}

	public void LockOpen()
	{
		openLocks++;
		if (openLocks != locks.Count)
		{
			return;
		}
		if (docons.Length != 0)
		{
			for (int i = 0; i < docons.Length; i++)
			{
				if (origDoconStates[i])
				{
					docons[i].gameObject.SetActive(value: true);
				}
			}
		}
		Open(enemy: false, skull: true);
	}

	public void LockClose()
	{
		openLocks--;
		if (openLocks == locks.Count - 1)
		{
			Close(force: true);
		}
	}

	public void BigDoorClosed()
	{
		if ((bool)occpor)
		{
			occpor.open = false;
		}
		if ((bool)openLight)
		{
			openLight.enabled = false;
		}
	}

	public void ForceStartOpen(bool force = true)
	{
		startOpen = force;
	}
}
