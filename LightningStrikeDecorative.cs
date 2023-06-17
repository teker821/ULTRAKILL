using UnityEngine;

public class LightningStrikeDecorative : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer lightning;

	[SerializeField]
	private Light flash;

	[SerializeField]
	private AudioSource thunder;

	private float originalFlashIntensity;

	private Color originalColor;

	private float cooldown = 15f;

	private bool flashing;

	public bool flashOnStart;

	private void Awake()
	{
		originalFlashIntensity = flash.intensity;
		flash.intensity = 0f;
		originalColor = lightning.color;
		lightning.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
		cooldown = Random.Range(5f, 60f);
		if (flashOnStart)
		{
			FlashStart();
		}
	}

	private void Update()
	{
		cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		if (cooldown == 0f)
		{
			FlashStart();
		}
		if (flashing)
		{
			flash.intensity = Mathf.MoveTowards(flash.intensity, 0f, originalFlashIntensity / 1.5f * Time.deltaTime);
			lightning.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.MoveTowards(lightning.color.a, 0f, Time.deltaTime / 1.5f));
			if (flash.intensity == 0f && lightning.color.a == 0f)
			{
				flashing = false;
			}
		}
	}

	private void FlashStart()
	{
		flashing = true;
		cooldown = Random.Range(25f, 60f);
		thunder.pitch = Random.Range(0.8f, 1.2f);
		thunder.Play();
		flash.intensity = originalFlashIntensity;
		lightning.color = originalColor;
	}
}
