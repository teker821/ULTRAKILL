using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ULTRAKILL/Spawnable Object")]
public class SpawnableObject : ScriptableObject
{
	public enum SpawnableObjectDataType
	{
		Object = 0,
		Enemy = 1,
		Tool = 3,
		Unlockable = 4
	}

	public string identifier;

	public SpawnableObjectDataType spawnableObjectType;

	public UnlockableType unlockableType;

	public string objectName;

	public string type;

	[TextArea]
	public string description;

	[TextArea]
	public string strategy;

	public GameObject gameObject;

	public GameObject preview;

	public string iconKey;

	public Sprite gridIcon;

	public Color backgroundColor;

	public EnemyType enemyType;

	public SpawnableType spawnableType;

	public Vector3 armOffset = Vector3.zero;

	[FormerlySerializedAs("rotationOffset")]
	public Vector3 armRotationOffset = Vector3.zero;

	public Vector3 menuOffset = Vector3.zero;

	public float spawnOffset;

	public bool isWater;
}
