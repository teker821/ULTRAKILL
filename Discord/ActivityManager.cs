using System;
using System.Runtime.InteropServices;

namespace Discord;

public class ActivityManager
{
	internal struct FFIEvents
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ActivityJoinHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ActivitySpectateHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ActivityJoinRequestHandler(IntPtr ptr, ref User user);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ActivityInviteHandler(IntPtr ptr, ActivityActionType type, ref User user, ref Activity activity);

		internal ActivityJoinHandler OnActivityJoin;

		internal ActivitySpectateHandler OnActivitySpectate;

		internal ActivityJoinRequestHandler OnActivityJoinRequest;

		internal ActivityInviteHandler OnActivityInvite;
	}

	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result RegisterCommandMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string command);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result RegisterSteamMethod(IntPtr methodsPtr, uint steamId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void UpdateActivityCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void UpdateActivityMethod(IntPtr methodsPtr, ref Activity activity, IntPtr callbackData, UpdateActivityCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ClearActivityCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ClearActivityMethod(IntPtr methodsPtr, IntPtr callbackData, ClearActivityCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SendRequestReplyCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SendRequestReplyMethod(IntPtr methodsPtr, long userId, ActivityJoinRequestReply reply, IntPtr callbackData, SendRequestReplyCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SendInviteCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SendInviteMethod(IntPtr methodsPtr, long userId, ActivityActionType type, [MarshalAs(UnmanagedType.LPStr)] string content, IntPtr callbackData, SendInviteCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void AcceptInviteCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void AcceptInviteMethod(IntPtr methodsPtr, long userId, IntPtr callbackData, AcceptInviteCallback callback);

		internal RegisterCommandMethod RegisterCommand;

		internal RegisterSteamMethod RegisterSteam;

		internal UpdateActivityMethod UpdateActivity;

		internal ClearActivityMethod ClearActivity;

		internal SendRequestReplyMethod SendRequestReply;

		internal SendInviteMethod SendInvite;

		internal AcceptInviteMethod AcceptInvite;
	}

	public delegate void UpdateActivityHandler(Result result);

	public delegate void ClearActivityHandler(Result result);

	public delegate void SendRequestReplyHandler(Result result);

	public delegate void SendInviteHandler(Result result);

	public delegate void AcceptInviteHandler(Result result);

	public delegate void ActivityJoinHandler(string secret);

	public delegate void ActivitySpectateHandler(string secret);

	public delegate void ActivityJoinRequestHandler(ref User user);

	public delegate void ActivityInviteHandler(ActivityActionType type, ref User user, ref Activity activity);

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

	public event ActivityJoinHandler OnActivityJoin;

	public event ActivitySpectateHandler OnActivitySpectate;

	public event ActivityJoinRequestHandler OnActivityJoinRequest;

	public event ActivityInviteHandler OnActivityInvite;

	public void RegisterCommand()
	{
		RegisterCommand(null);
	}

	internal ActivityManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
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
		events.OnActivityJoin = OnActivityJoinImpl;
		events.OnActivitySpectate = OnActivitySpectateImpl;
		events.OnActivityJoinRequest = OnActivityJoinRequestImpl;
		events.OnActivityInvite = OnActivityInviteImpl;
		Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
	}

	public void RegisterCommand(string command)
	{
		Result result = Methods.RegisterCommand(MethodsPtr, command);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	public void RegisterSteam(uint steamId)
	{
		Result result = Methods.RegisterSteam(MethodsPtr, steamId);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	[MonoPInvokeCallback]
	private static void UpdateActivityCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		UpdateActivityHandler obj = (UpdateActivityHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void UpdateActivity(Activity activity, UpdateActivityHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.UpdateActivity(MethodsPtr, ref activity, GCHandle.ToIntPtr(value), UpdateActivityCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void ClearActivityCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		ClearActivityHandler obj = (ClearActivityHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void ClearActivity(ClearActivityHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.ClearActivity(MethodsPtr, GCHandle.ToIntPtr(value), ClearActivityCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void SendRequestReplyCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		SendRequestReplyHandler obj = (SendRequestReplyHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void SendRequestReply(long userId, ActivityJoinRequestReply reply, SendRequestReplyHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.SendRequestReply(MethodsPtr, userId, reply, GCHandle.ToIntPtr(value), SendRequestReplyCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void SendInviteCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		SendInviteHandler obj = (SendInviteHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void SendInvite(long userId, ActivityActionType type, string content, SendInviteHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.SendInvite(MethodsPtr, userId, type, content, GCHandle.ToIntPtr(value), SendInviteCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void AcceptInviteCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		AcceptInviteHandler obj = (AcceptInviteHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void AcceptInvite(long userId, AcceptInviteHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.AcceptInvite(MethodsPtr, userId, GCHandle.ToIntPtr(value), AcceptInviteCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void OnActivityJoinImpl(IntPtr ptr, string secret)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.ActivityManagerInstance.OnActivityJoin != null)
		{
			discord.ActivityManagerInstance.OnActivityJoin(secret);
		}
	}

	[MonoPInvokeCallback]
	private static void OnActivitySpectateImpl(IntPtr ptr, string secret)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.ActivityManagerInstance.OnActivitySpectate != null)
		{
			discord.ActivityManagerInstance.OnActivitySpectate(secret);
		}
	}

	[MonoPInvokeCallback]
	private static void OnActivityJoinRequestImpl(IntPtr ptr, ref User user)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.ActivityManagerInstance.OnActivityJoinRequest != null)
		{
			discord.ActivityManagerInstance.OnActivityJoinRequest(ref user);
		}
	}

	[MonoPInvokeCallback]
	private static void OnActivityInviteImpl(IntPtr ptr, ActivityActionType type, ref User user, ref Activity activity)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.ActivityManagerInstance.OnActivityInvite != null)
		{
			discord.ActivityManagerInstance.OnActivityInvite(type, ref user, ref activity);
		}
	}
}
