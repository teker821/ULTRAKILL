using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public class RumbleManager : MonoSingleton<RumbleManager>
{
	public readonly Dictionary<string, PendingVibration> pendingVibrations = new Dictionary<string, PendingVibration>();

	public float currentIntensity { get; private set; }

	public PendingVibration SetVibration(string key)
	{
		if (pendingVibrations.ContainsKey(key))
		{
			pendingVibrations[key].timeSinceStart = 0f;
			if (pendingVibrations[key].isTracking)
			{
				pendingVibrations[key].isTracking = false;
			}
		}
		else
		{
			pendingVibrations.Add(key, new PendingVibration
			{
				key = key,
				timeSinceStart = 0f,
				isTracking = false
			});
		}
		return pendingVibrations[key];
	}

	public PendingVibration SetVibrationTracked(string key, GameObject tracked)
	{
		if (pendingVibrations.ContainsKey(key))
		{
			pendingVibrations[key].timeSinceStart = 0f;
			pendingVibrations[key].isTracking = true;
			pendingVibrations[key].trackedObject = tracked;
		}
		else
		{
			pendingVibrations.Add(key, new PendingVibration
			{
				key = key,
				timeSinceStart = 0f,
				isTracking = true,
				trackedObject = tracked
			});
		}
		return pendingVibrations[key];
	}

	public void StopVibration(string key)
	{
		if (pendingVibrations.ContainsKey(key))
		{
			pendingVibrations.Remove(key);
		}
	}

	public void StopAllVibrations()
	{
		pendingVibrations.Clear();
	}

	private void Update()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, PendingVibration> pendingVibration in pendingVibrations)
		{
			if (pendingVibration.Value.isTracking && (pendingVibration.Value.trackedObject == null || !pendingVibration.Value.trackedObject.activeInHierarchy))
			{
				list.Add(pendingVibration.Key);
			}
			else if (pendingVibration.Value.IsFinished)
			{
				list.Add(pendingVibration.Key);
			}
		}
		foreach (string item in list)
		{
			pendingVibrations.Remove(item);
		}
		float num = 0f;
		foreach (KeyValuePair<string, PendingVibration> pendingVibration2 in pendingVibrations)
		{
			if (pendingVibration2.Value.Intensity > num)
			{
				num = pendingVibration2.Value.Intensity;
			}
		}
		num *= MonoSingleton<PrefsManager>.Instance.GetFloat("totalRumbleIntensity");
		if ((bool)MonoSingleton<OptionsManager>.Instance && MonoSingleton<OptionsManager>.Instance.paused)
		{
			num = 0f;
		}
		currentIntensity = num;
		if (Gamepad.current != null)
		{
			Gamepad.current.SetMotorSpeeds(num, num);
		}
	}

	private void OnDisable()
	{
		if (Gamepad.current != null)
		{
			Gamepad.current.SetMotorSpeeds(0f, 0f);
		}
	}

	public float ResolveDuration(string key)
	{
		if (MonoSingleton<PrefsManager>.Instance.HasKey(key + ".duration"))
		{
			return MonoSingleton<PrefsManager>.Instance.GetFloat(key + ".duration");
		}
		return ResolveDefaultDuration(key);
	}

	public float ResolveDefaultDuration(string key)
	{
		switch (key)
		{
		case "rumble.slide":
		case "rumble.whiplash.throw":
		case "rumble.whiplash.pull":
		case "rumble.gun.railcannon_idle":
		case "rumble.gun.shotgun_charge":
		case "rumble.gun.nailgun_fire":
		case "rumble.gun.revolver_charge":
			return float.PositiveInfinity;
		case "rumble.fall_impact":
		case "rumble.fall_impact_heave":
			return 0.5f;
		case "rumble.jump":
		case "rumble.dash":
		case "rumble.punch":
		case "rumble.gun.sawblade":
			return 0.2f;
		case "rumble.gun.fire":
			return 0.4f;
		case "rumble.gun.super_saw":
		case "rumble.gun.fire_strong":
			return 0.7f;
		case "rumble.gun.fire_projectiles":
			return 0.8f;
		case "rumble.parry_flash":
		case "rumble.coin_toss":
		case "rumble.magnet_released":
			return 0.1f;
		case "rumble.weapon_wheel_tick":
			return 0.025f;
		default:
			Debug.LogError("No duration found for key: " + key);
			return 0.5f;
		}
	}

	public float ResolveIntensity(string key)
	{
		if (MonoSingleton<PrefsManager>.Instance.HasKey(key + ".intensity"))
		{
			return MonoSingleton<PrefsManager>.Instance.GetFloat(key + ".intensity");
		}
		return ResolveDefaultIntensity(key);
	}

	public float ResolveDefaultIntensity(string key)
	{
		switch (key)
		{
		case "rumble.slide":
			return 0.1f;
		case "rumble.dash":
		case "rumble.fall_impact":
			return 0.2f;
		case "rumble.jump":
			return 0.1f;
		case "rumble.fall_impact_heave":
			return 0.5f;
		case "rumble.whiplash.throw":
			return 0.2f;
		case "rumble.whiplash.pull":
			return 0.35f;
		case "rumble.gun.fire":
			return 0.8f;
		case "rumble.gun.fire_strong":
			return 1f;
		case "rumble.gun.fire_projectiles":
			return 0.7f;
		case "rumble.gun.railcannon_idle":
		case "rumble.gun.nailgun_fire":
			return 0.2f;
		case "rumble.gun.super_saw":
		case "rumble.gun.shotgun_charge":
			return 0.7f;
		case "rumble.gun.sawblade":
		case "rumble.gun.revolver_charge":
			return 0.5f;
		case "rumble.punch":
		case "rumble.parry_flash":
		case "rumble.magnet_released":
			return 0.2f;
		case "rumble.coin_toss":
			return 0.1f;
		case "rumble.weapon_wheel_tick":
			return 0.05f;
		default:
			Debug.LogError("No intensity found for key: " + key);
			return 0.5f;
		}
	}

	public string ResolveFullName(string key)
	{
		return key switch
		{
			"rumble.slide" => "Sliding", 
			"rumble.dash" => "Dashing", 
			"rumble.fall_impact" => "Fall Impact", 
			"rumble.jump" => "Jumping", 
			"rumble.fall_impact_heave" => "Heavy Fall Impact", 
			"rumble.whiplash.throw" => "Whiplash Throw", 
			"rumble.whiplash.pull" => "Whiplash Pull", 
			"rumble.gun.fire" => "Gun Fire", 
			"rumble.gun.fire_strong" => "Stronger Gun Fire", 
			"rumble.gun.fire_projectiles" => "Gun Fire (projectiles)", 
			"rumble.gun.railcannon_idle" => "Railcannon Idle", 
			"rumble.gun.nailgun_fire" => "Nailgun Fire", 
			"rumble.gun.sawblade" => "Sawblade", 
			"rumble.gun.super_saw" => "Super Saw", 
			"rumble.magnet_released" => "Magnet", 
			"rumble.gun.shotgun_charge" => "Shotgun Charge", 
			"rumble.gun.revolver_charge" => "Revolver Charge", 
			"rumble.parry_flash" => "Parry Flash", 
			"rumble.coin_toss" => "Coin Toss", 
			"rumble.punch" => "Punching", 
			"rumble.weapon_wheel_tick" => "Weapon Wheel Tick", 
			_ => key, 
		};
	}
}
