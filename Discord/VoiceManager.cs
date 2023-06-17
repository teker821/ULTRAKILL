using System;
using System.Runtime.InteropServices;

namespace Discord;

public class VoiceManager
{
	internal struct FFIEvents
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SettingsUpdateHandler(IntPtr ptr);

		internal SettingsUpdateHandler OnSettingsUpdate;
	}

	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetInputModeMethod(IntPtr methodsPtr, ref InputMode inputMode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SetInputModeCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SetInputModeMethod(IntPtr methodsPtr, InputMode inputMode, IntPtr callbackData, SetInputModeCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result IsSelfMuteMethod(IntPtr methodsPtr, ref bool mute);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SetSelfMuteMethod(IntPtr methodsPtr, bool mute);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result IsSelfDeafMethod(IntPtr methodsPtr, ref bool deaf);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SetSelfDeafMethod(IntPtr methodsPtr, bool deaf);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result IsLocalMuteMethod(IntPtr methodsPtr, long userId, ref bool mute);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SetLocalMuteMethod(IntPtr methodsPtr, long userId, bool mute);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetLocalVolumeMethod(IntPtr methodsPtr, long userId, ref byte volume);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SetLocalVolumeMethod(IntPtr methodsPtr, long userId, byte volume);

		internal GetInputModeMethod GetInputMode;

		internal SetInputModeMethod SetInputMode;

		internal IsSelfMuteMethod IsSelfMute;

		internal SetSelfMuteMethod SetSelfMute;

		internal IsSelfDeafMethod IsSelfDeaf;

		internal SetSelfDeafMethod SetSelfDeaf;

		internal IsLocalMuteMethod IsLocalMute;

		internal SetLocalMuteMethod SetLocalMute;

		internal GetLocalVolumeMethod GetLocalVolume;

		internal SetLocalVolumeMethod SetLocalVolume;
	}

	public delegate void SetInputModeHandler(Result result);

	public delegate void SettingsUpdateHandler();

	private IntPtr MethodsPtr;

	private object MethodsStructure;

	private FFIMethods Methods
	{
		get
		{
			if (MethodsStructure == null)
			{
				MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
			}
			return (FFIMethods)MethodsStructure;
		}
	}

	public event SettingsUpdateHandler OnSettingsUpdate;

	internal VoiceManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
	{
		if (eventsPtr == IntPtr.Zero)
		{
			throw new ResultException(Result.InternalError);
		}
		InitEvents(eventsPtr, ref events);
		MethodsPtr = ptr;
		if (MethodsPtr == IntPtr.Zero)
		{
			throw new ResultException(Result.InternalError);
		}
	}

	private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
	{
		events.OnSettingsUpdate = OnSettingsUpdateImpl;
		Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
	}

	public InputMode GetInputMode()
	{
		InputMode inputMode = default(InputMode);
		Result result = Methods.GetInputMode(MethodsPtr, ref inputMode);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return inputMode;
	}

	[MonoPInvokeCallback]
	private static void SetInputModeCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		SetInputModeHandler obj = (SetInputModeHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void SetInputMode(InputMode inputMode, SetInputModeHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.SetInputMode(MethodsPtr, inputMode, GCHandle.ToIntPtr(value), SetInputModeCallbackImpl);
	}

	public bool IsSelfMute()
	{
		bool mute = false;
		Result result = Methods.IsSelfMute(MethodsPtr, ref mute);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return mute;
	}

	public void SetSelfMute(bool mute)
	{
		Result result = Methods.SetSelfMute(MethodsPtr, mute);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	public bool IsSelfDeaf()
	{
		bool deaf = false;
		Result result = Methods.IsSelfDeaf(MethodsPtr, ref deaf);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return deaf;
	}

	public void SetSelfDeaf(bool deaf)
	{
		Result result = Methods.SetSelfDeaf(MethodsPtr, deaf);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	public bool IsLocalMute(long userId)
	{
		bool mute = false;
		Result result = Methods.IsLocalMute(MethodsPtr, userId, ref mute);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return mute;
	}

	public void SetLocalMute(long userId, bool mute)
	{
		Result result = Methods.SetLocalMute(MethodsPtr, userId, mute);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	public byte GetLocalVolume(long userId)
	{
		byte volume = 0;
		Result result = Methods.GetLocalVolume(MethodsPtr, userId, ref volume);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return volume;
	}

	public void SetLocalVolume(long userId, byte volume)
	{
		Result result = Methods.SetLocalVolume(MethodsPtr, userId, volume);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	[MonoPInvokeCallback]
	private static void OnSettingsUpdateImpl(IntPtr ptr)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.VoiceManagerInstance.OnSettingsUpdate != null)
		{
			discord.VoiceManagerInstance.OnSettingsUpdate();
		}
	}
}
