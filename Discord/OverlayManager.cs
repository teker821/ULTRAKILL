using System;
using System.Runtime.InteropServices;

namespace Discord;

public class OverlayManager
{
	internal struct FFIEvents
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ToggleHandler(IntPtr ptr, bool locked);

		internal ToggleHandler OnToggle;
	}

	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void IsEnabledMethod(IntPtr methodsPtr, ref bool enabled);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void IsLockedMethod(IntPtr methodsPtr, ref bool locked);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SetLockedCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SetLockedMethod(IntPtr methodsPtr, bool locked, IntPtr callbackData, SetLockedCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void OpenActivityInviteCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void OpenActivityInviteMethod(IntPtr methodsPtr, ActivityActionType type, IntPtr callbackData, OpenActivityInviteCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void OpenGuildInviteCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void OpenGuildInviteMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string code, IntPtr callbackData, OpenGuildInviteCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void OpenVoiceSettingsCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void OpenVoiceSettingsMethod(IntPtr methodsPtr, IntPtr callbackData, OpenVoiceSettingsCallback callback);

		internal IsEnabledMethod IsEnabled;

		internal IsLockedMethod IsLocked;

		internal SetLockedMethod SetLocked;

		internal OpenActivityInviteMethod OpenActivityInvite;

		internal OpenGuildInviteMethod OpenGuildInvite;

		internal OpenVoiceSettingsMethod OpenVoiceSettings;
	}

	public delegate void SetLockedHandler(Result result);

	public delegate void OpenActivityInviteHandler(Result result);

	public delegate void OpenGuildInviteHandler(Result result);

	public delegate void OpenVoiceSettingsHandler(Result result);

	public delegate void ToggleHandler(bool locked);

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

	public event ToggleHandler OnToggle;

	internal OverlayManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
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
		events.OnToggle = OnToggleImpl;
		Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
	}

	public bool IsEnabled()
	{
		bool enabled = false;
		Methods.IsEnabled(MethodsPtr, ref enabled);
		return enabled;
	}

	public bool IsLocked()
	{
		bool locked = false;
		Methods.IsLocked(MethodsPtr, ref locked);
		return locked;
	}

	[MonoPInvokeCallback]
	private static void SetLockedCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		SetLockedHandler obj = (SetLockedHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void SetLocked(bool locked, SetLockedHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.SetLocked(MethodsPtr, locked, GCHandle.ToIntPtr(value), SetLockedCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void OpenActivityInviteCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		OpenActivityInviteHandler obj = (OpenActivityInviteHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void OpenActivityInvite(ActivityActionType type, OpenActivityInviteHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.OpenActivityInvite(MethodsPtr, type, GCHandle.ToIntPtr(value), OpenActivityInviteCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void OpenGuildInviteCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		OpenGuildInviteHandler obj = (OpenGuildInviteHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void OpenGuildInvite(string code, OpenGuildInviteHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.OpenGuildInvite(MethodsPtr, code, GCHandle.ToIntPtr(value), OpenGuildInviteCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void OpenVoiceSettingsCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		OpenVoiceSettingsHandler obj = (OpenVoiceSettingsHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void OpenVoiceSettings(OpenVoiceSettingsHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.OpenVoiceSettings(MethodsPtr, GCHandle.ToIntPtr(value), OpenVoiceSettingsCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void OnToggleImpl(IntPtr ptr, bool locked)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.OverlayManagerInstance.OnToggle != null)
		{
			discord.OverlayManagerInstance.OnToggle(locked);
		}
	}
}
