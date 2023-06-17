using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Discord;

public class LobbyManager
{
	internal struct FFIEvents
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void LobbyUpdateHandler(IntPtr ptr, long lobbyId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void LobbyDeleteHandler(IntPtr ptr, long lobbyId, uint reason);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void MemberConnectHandler(IntPtr ptr, long lobbyId, long userId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void MemberUpdateHandler(IntPtr ptr, long lobbyId, long userId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void MemberDisconnectHandler(IntPtr ptr, long lobbyId, long userId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void LobbyMessageHandler(IntPtr ptr, long lobbyId, long userId, IntPtr dataPtr, int dataLen);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SpeakingHandler(IntPtr ptr, long lobbyId, long userId, bool speaking);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void NetworkMessageHandler(IntPtr ptr, long lobbyId, long userId, byte channelId, IntPtr dataPtr, int dataLen);

		internal LobbyUpdateHandler OnLobbyUpdate;

		internal LobbyDeleteHandler OnLobbyDelete;

		internal MemberConnectHandler OnMemberConnect;

		internal MemberUpdateHandler OnMemberUpdate;

		internal MemberDisconnectHandler OnMemberDisconnect;

		internal LobbyMessageHandler OnLobbyMessage;

		internal SpeakingHandler OnSpeaking;

		internal NetworkMessageHandler OnNetworkMessage;
	}

	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetLobbyCreateTransactionMethod(IntPtr methodsPtr, ref IntPtr transaction);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetLobbyUpdateTransactionMethod(IntPtr methodsPtr, long lobbyId, ref IntPtr transaction);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetMemberUpdateTransactionMethod(IntPtr methodsPtr, long lobbyId, long userId, ref IntPtr transaction);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void CreateLobbyCallback(IntPtr ptr, Result result, ref Lobby lobby);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void CreateLobbyMethod(IntPtr methodsPtr, IntPtr transaction, IntPtr callbackData, CreateLobbyCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void UpdateLobbyCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void UpdateLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr transaction, IntPtr callbackData, UpdateLobbyCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void DeleteLobbyCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void DeleteLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, DeleteLobbyCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ConnectLobbyCallback(IntPtr ptr, Result result, ref Lobby lobby);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ConnectLobbyMethod(IntPtr methodsPtr, long lobbyId, [MarshalAs(UnmanagedType.LPStr)] string secret, IntPtr callbackData, ConnectLobbyCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ConnectLobbyWithActivitySecretCallback(IntPtr ptr, Result result, ref Lobby lobby);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ConnectLobbyWithActivitySecretMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string activitySecret, IntPtr callbackData, ConnectLobbyWithActivitySecretCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void DisconnectLobbyCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void DisconnectLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, DisconnectLobbyCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetLobbyMethod(IntPtr methodsPtr, long lobbyId, ref Lobby lobby);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetLobbyActivitySecretMethod(IntPtr methodsPtr, long lobbyId, StringBuilder secret);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetLobbyMetadataValueMethod(IntPtr methodsPtr, long lobbyId, [MarshalAs(UnmanagedType.LPStr)] string key, StringBuilder value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetLobbyMetadataKeyMethod(IntPtr methodsPtr, long lobbyId, int index, StringBuilder key);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result LobbyMetadataCountMethod(IntPtr methodsPtr, long lobbyId, ref int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result MemberCountMethod(IntPtr methodsPtr, long lobbyId, ref int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetMemberUserIdMethod(IntPtr methodsPtr, long lobbyId, int index, ref long userId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetMemberUserMethod(IntPtr methodsPtr, long lobbyId, long userId, ref User user);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetMemberMetadataValueMethod(IntPtr methodsPtr, long lobbyId, long userId, [MarshalAs(UnmanagedType.LPStr)] string key, StringBuilder value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetMemberMetadataKeyMethod(IntPtr methodsPtr, long lobbyId, long userId, int index, StringBuilder key);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result MemberMetadataCountMethod(IntPtr methodsPtr, long lobbyId, long userId, ref int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void UpdateMemberCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void UpdateMemberMethod(IntPtr methodsPtr, long lobbyId, long userId, IntPtr transaction, IntPtr callbackData, UpdateMemberCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SendLobbyMessageCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SendLobbyMessageMethod(IntPtr methodsPtr, long lobbyId, byte[] data, int dataLen, IntPtr callbackData, SendLobbyMessageCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetSearchQueryMethod(IntPtr methodsPtr, ref IntPtr query);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SearchCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void SearchMethod(IntPtr methodsPtr, IntPtr query, IntPtr callbackData, SearchCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void LobbyCountMethod(IntPtr methodsPtr, ref int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetLobbyIdMethod(IntPtr methodsPtr, int index, ref long lobbyId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ConnectVoiceCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ConnectVoiceMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, ConnectVoiceCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void DisconnectVoiceCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void DisconnectVoiceMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, DisconnectVoiceCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result ConnectNetworkMethod(IntPtr methodsPtr, long lobbyId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result DisconnectNetworkMethod(IntPtr methodsPtr, long lobbyId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result FlushNetworkMethod(IntPtr methodsPtr);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result OpenNetworkChannelMethod(IntPtr methodsPtr, long lobbyId, byte channelId, bool reliable);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SendNetworkMessageMethod(IntPtr methodsPtr, long lobbyId, long userId, byte channelId, byte[] data, int dataLen);

		internal GetLobbyCreateTransactionMethod GetLobbyCreateTransaction;

		internal GetLobbyUpdateTransactionMethod GetLobbyUpdateTransaction;

		internal GetMemberUpdateTransactionMethod GetMemberUpdateTransaction;

		internal CreateLobbyMethod CreateLobby;

		internal UpdateLobbyMethod UpdateLobby;

		internal DeleteLobbyMethod DeleteLobby;

		internal ConnectLobbyMethod ConnectLobby;

		internal ConnectLobbyWithActivitySecretMethod ConnectLobbyWithActivitySecret;

		internal DisconnectLobbyMethod DisconnectLobby;

		internal GetLobbyMethod GetLobby;

		internal GetLobbyActivitySecretMethod GetLobbyActivitySecret;

		internal GetLobbyMetadataValueMethod GetLobbyMetadataValue;

		internal GetLobbyMetadataKeyMethod GetLobbyMetadataKey;

		internal LobbyMetadataCountMethod LobbyMetadataCount;

		internal MemberCountMethod MemberCount;

		internal GetMemberUserIdMethod GetMemberUserId;

		internal GetMemberUserMethod GetMemberUser;

		internal GetMemberMetadataValueMethod GetMemberMetadataValue;

		internal GetMemberMetadataKeyMethod GetMemberMetadataKey;

		internal MemberMetadataCountMethod MemberMetadataCount;

		internal UpdateMemberMethod UpdateMember;

		internal SendLobbyMessageMethod SendLobbyMessage;

		internal GetSearchQueryMethod GetSearchQuery;

		internal SearchMethod Search;

		internal LobbyCountMethod LobbyCount;

		internal GetLobbyIdMethod GetLobbyId;

		internal ConnectVoiceMethod ConnectVoice;

		internal DisconnectVoiceMethod DisconnectVoice;

		internal ConnectNetworkMethod ConnectNetwork;

		internal DisconnectNetworkMethod DisconnectNetwork;

		internal FlushNetworkMethod FlushNetwork;

		internal OpenNetworkChannelMethod OpenNetworkChannel;

		internal SendNetworkMessageMethod SendNetworkMessage;
	}

	public delegate void CreateLobbyHandler(Result result, ref Lobby lobby);

	public delegate void UpdateLobbyHandler(Result result);

	public delegate void DeleteLobbyHandler(Result result);

	public delegate void ConnectLobbyHandler(Result result, ref Lobby lobby);

	public delegate void ConnectLobbyWithActivitySecretHandler(Result result, ref Lobby lobby);

	public delegate void DisconnectLobbyHandler(Result result);

	public delegate void UpdateMemberHandler(Result result);

	public delegate void SendLobbyMessageHandler(Result result);

	public delegate void SearchHandler(Result result);

	public delegate void ConnectVoiceHandler(Result result);

	public delegate void DisconnectVoiceHandler(Result result);

	public delegate void LobbyUpdateHandler(long lobbyId);

	public delegate void LobbyDeleteHandler(long lobbyId, uint reason);

	public delegate void MemberConnectHandler(long lobbyId, long userId);

	public delegate void MemberUpdateHandler(long lobbyId, long userId);

	public delegate void MemberDisconnectHandler(long lobbyId, long userId);

	public delegate void LobbyMessageHandler(long lobbyId, long userId, byte[] data);

	public delegate void SpeakingHandler(long lobbyId, long userId, bool speaking);

	public delegate void NetworkMessageHandler(long lobbyId, long userId, byte channelId, byte[] data);

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

	public event LobbyUpdateHandler OnLobbyUpdate;

	public event LobbyDeleteHandler OnLobbyDelete;

	public event MemberConnectHandler OnMemberConnect;

	public event MemberUpdateHandler OnMemberUpdate;

	public event MemberDisconnectHandler OnMemberDisconnect;

	public event LobbyMessageHandler OnLobbyMessage;

	public event SpeakingHandler OnSpeaking;

	public event NetworkMessageHandler OnNetworkMessage;

	internal LobbyManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
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
		events.OnLobbyUpdate = OnLobbyUpdateImpl;
		events.OnLobbyDelete = OnLobbyDeleteImpl;
		events.OnMemberConnect = OnMemberConnectImpl;
		events.OnMemberUpdate = OnMemberUpdateImpl;
		events.OnMemberDisconnect = OnMemberDisconnectImpl;
		events.OnLobbyMessage = OnLobbyMessageImpl;
		events.OnSpeaking = OnSpeakingImpl;
		events.OnNetworkMessage = OnNetworkMessageImpl;
		Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
	}

	public LobbyTransaction GetLobbyCreateTransaction()
	{
		LobbyTransaction result = default(LobbyTransaction);
		Result result2 = Methods.GetLobbyCreateTransaction(MethodsPtr, ref result.MethodsPtr);
		if (result2 != 0)
		{
			throw new ResultException(result2);
		}
		return result;
	}

	public LobbyTransaction GetLobbyUpdateTransaction(long lobbyId)
	{
		LobbyTransaction result = default(LobbyTransaction);
		Result result2 = Methods.GetLobbyUpdateTransaction(MethodsPtr, lobbyId, ref result.MethodsPtr);
		if (result2 != 0)
		{
			throw new ResultException(result2);
		}
		return result;
	}

	public LobbyMemberTransaction GetMemberUpdateTransaction(long lobbyId, long userId)
	{
		LobbyMemberTransaction result = default(LobbyMemberTransaction);
		Result result2 = Methods.GetMemberUpdateTransaction(MethodsPtr, lobbyId, userId, ref result.MethodsPtr);
		if (result2 != 0)
		{
			throw new ResultException(result2);
		}
		return result;
	}

	[MonoPInvokeCallback]
	private static void CreateLobbyCallbackImpl(IntPtr ptr, Result result, ref Lobby lobby)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		CreateLobbyHandler obj = (CreateLobbyHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result, ref lobby);
	}

	public void CreateLobby(LobbyTransaction transaction, CreateLobbyHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.CreateLobby(MethodsPtr, transaction.MethodsPtr, GCHandle.ToIntPtr(value), CreateLobbyCallbackImpl);
		transaction.MethodsPtr = IntPtr.Zero;
	}

	[MonoPInvokeCallback]
	private static void UpdateLobbyCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		UpdateLobbyHandler obj = (UpdateLobbyHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void UpdateLobby(long lobbyId, LobbyTransaction transaction, UpdateLobbyHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.UpdateLobby(MethodsPtr, lobbyId, transaction.MethodsPtr, GCHandle.ToIntPtr(value), UpdateLobbyCallbackImpl);
		transaction.MethodsPtr = IntPtr.Zero;
	}

	[MonoPInvokeCallback]
	private static void DeleteLobbyCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		DeleteLobbyHandler obj = (DeleteLobbyHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void DeleteLobby(long lobbyId, DeleteLobbyHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.DeleteLobby(MethodsPtr, lobbyId, GCHandle.ToIntPtr(value), DeleteLobbyCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void ConnectLobbyCallbackImpl(IntPtr ptr, Result result, ref Lobby lobby)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		ConnectLobbyHandler obj = (ConnectLobbyHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result, ref lobby);
	}

	public void ConnectLobby(long lobbyId, string secret, ConnectLobbyHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.ConnectLobby(MethodsPtr, lobbyId, secret, GCHandle.ToIntPtr(value), ConnectLobbyCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void ConnectLobbyWithActivitySecretCallbackImpl(IntPtr ptr, Result result, ref Lobby lobby)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		ConnectLobbyWithActivitySecretHandler obj = (ConnectLobbyWithActivitySecretHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result, ref lobby);
	}

	public void ConnectLobbyWithActivitySecret(string activitySecret, ConnectLobbyWithActivitySecretHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.ConnectLobbyWithActivitySecret(MethodsPtr, activitySecret, GCHandle.ToIntPtr(value), ConnectLobbyWithActivitySecretCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void DisconnectLobbyCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		DisconnectLobbyHandler obj = (DisconnectLobbyHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void DisconnectLobby(long lobbyId, DisconnectLobbyHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.DisconnectLobby(MethodsPtr, lobbyId, GCHandle.ToIntPtr(value), DisconnectLobbyCallbackImpl);
	}

	public Lobby GetLobby(long lobbyId)
	{
		Lobby lobby = default(Lobby);
		Result result = Methods.GetLobby(MethodsPtr, lobbyId, ref lobby);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return lobby;
	}

	public string GetLobbyActivitySecret(long lobbyId)
	{
		StringBuilder stringBuilder = new StringBuilder(128);
		Result result = Methods.GetLobbyActivitySecret(MethodsPtr, lobbyId, stringBuilder);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return stringBuilder.ToString();
	}

	public string GetLobbyMetadataValue(long lobbyId, string key)
	{
		StringBuilder stringBuilder = new StringBuilder(4096);
		Result result = Methods.GetLobbyMetadataValue(MethodsPtr, lobbyId, key, stringBuilder);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return stringBuilder.ToString();
	}

	public string GetLobbyMetadataKey(long lobbyId, int index)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		Result result = Methods.GetLobbyMetadataKey(MethodsPtr, lobbyId, index, stringBuilder);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return stringBuilder.ToString();
	}

	public int LobbyMetadataCount(long lobbyId)
	{
		int count = 0;
		Result result = Methods.LobbyMetadataCount(MethodsPtr, lobbyId, ref count);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return count;
	}

	public int MemberCount(long lobbyId)
	{
		int count = 0;
		Result result = Methods.MemberCount(MethodsPtr, lobbyId, ref count);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return count;
	}

	public long GetMemberUserId(long lobbyId, int index)
	{
		long userId = 0L;
		Result result = Methods.GetMemberUserId(MethodsPtr, lobbyId, index, ref userId);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return userId;
	}

	public User GetMemberUser(long lobbyId, long userId)
	{
		User user = default(User);
		Result result = Methods.GetMemberUser(MethodsPtr, lobbyId, userId, ref user);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return user;
	}

	public string GetMemberMetadataValue(long lobbyId, long userId, string key)
	{
		StringBuilder stringBuilder = new StringBuilder(4096);
		Result result = Methods.GetMemberMetadataValue(MethodsPtr, lobbyId, userId, key, stringBuilder);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return stringBuilder.ToString();
	}

	public string GetMemberMetadataKey(long lobbyId, long userId, int index)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		Result result = Methods.GetMemberMetadataKey(MethodsPtr, lobbyId, userId, index, stringBuilder);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return stringBuilder.ToString();
	}

	public int MemberMetadataCount(long lobbyId, long userId)
	{
		int count = 0;
		Result result = Methods.MemberMetadataCount(MethodsPtr, lobbyId, userId, ref count);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return count;
	}

	[MonoPInvokeCallback]
	private static void UpdateMemberCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		UpdateMemberHandler obj = (UpdateMemberHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void UpdateMember(long lobbyId, long userId, LobbyMemberTransaction transaction, UpdateMemberHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.UpdateMember(MethodsPtr, lobbyId, userId, transaction.MethodsPtr, GCHandle.ToIntPtr(value), UpdateMemberCallbackImpl);
		transaction.MethodsPtr = IntPtr.Zero;
	}

	[MonoPInvokeCallback]
	private static void SendLobbyMessageCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		SendLobbyMessageHandler obj = (SendLobbyMessageHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void SendLobbyMessage(long lobbyId, byte[] data, SendLobbyMessageHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.SendLobbyMessage(MethodsPtr, lobbyId, data, data.Length, GCHandle.ToIntPtr(value), SendLobbyMessageCallbackImpl);
	}

	public LobbySearchQuery GetSearchQuery()
	{
		LobbySearchQuery result = default(LobbySearchQuery);
		Result result2 = Methods.GetSearchQuery(MethodsPtr, ref result.MethodsPtr);
		if (result2 != 0)
		{
			throw new ResultException(result2);
		}
		return result;
	}

	[MonoPInvokeCallback]
	private static void SearchCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		SearchHandler obj = (SearchHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void Search(LobbySearchQuery query, SearchHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.Search(MethodsPtr, query.MethodsPtr, GCHandle.ToIntPtr(value), SearchCallbackImpl);
		query.MethodsPtr = IntPtr.Zero;
	}

	public int LobbyCount()
	{
		int count = 0;
		Methods.LobbyCount(MethodsPtr, ref count);
		return count;
	}

	public long GetLobbyId(int index)
	{
		long lobbyId = 0L;
		Result result = Methods.GetLobbyId(MethodsPtr, index, ref lobbyId);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return lobbyId;
	}

	[MonoPInvokeCallback]
	private static void ConnectVoiceCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		ConnectVoiceHandler obj = (ConnectVoiceHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void ConnectVoice(long lobbyId, ConnectVoiceHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.ConnectVoice(MethodsPtr, lobbyId, GCHandle.ToIntPtr(value), ConnectVoiceCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void DisconnectVoiceCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		DisconnectVoiceHandler obj = (DisconnectVoiceHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void DisconnectVoice(long lobbyId, DisconnectVoiceHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.DisconnectVoice(MethodsPtr, lobbyId, GCHandle.ToIntPtr(value), DisconnectVoiceCallbackImpl);
	}

	public void ConnectNetwork(long lobbyId)
	{
		Result result = Methods.ConnectNetwork(MethodsPtr, lobbyId);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	public void DisconnectNetwork(long lobbyId)
	{
		Result result = Methods.DisconnectNetwork(MethodsPtr, lobbyId);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	public void FlushNetwork()
	{
		Result result = Methods.FlushNetwork(MethodsPtr);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	public void OpenNetworkChannel(long lobbyId, byte channelId, bool reliable)
	{
		Result result = Methods.OpenNetworkChannel(MethodsPtr, lobbyId, channelId, reliable);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	public void SendNetworkMessage(long lobbyId, long userId, byte channelId, byte[] data)
	{
		Result result = Methods.SendNetworkMessage(MethodsPtr, lobbyId, userId, channelId, data, data.Length);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	[MonoPInvokeCallback]
	private static void OnLobbyUpdateImpl(IntPtr ptr, long lobbyId)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.LobbyManagerInstance.OnLobbyUpdate != null)
		{
			discord.LobbyManagerInstance.OnLobbyUpdate(lobbyId);
		}
	}

	[MonoPInvokeCallback]
	private static void OnLobbyDeleteImpl(IntPtr ptr, long lobbyId, uint reason)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.LobbyManagerInstance.OnLobbyDelete != null)
		{
			discord.LobbyManagerInstance.OnLobbyDelete(lobbyId, reason);
		}
	}

	[MonoPInvokeCallback]
	private static void OnMemberConnectImpl(IntPtr ptr, long lobbyId, long userId)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.LobbyManagerInstance.OnMemberConnect != null)
		{
			discord.LobbyManagerInstance.OnMemberConnect(lobbyId, userId);
		}
	}

	[MonoPInvokeCallback]
	private static void OnMemberUpdateImpl(IntPtr ptr, long lobbyId, long userId)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.LobbyManagerInstance.OnMemberUpdate != null)
		{
			discord.LobbyManagerInstance.OnMemberUpdate(lobbyId, userId);
		}
	}

	[MonoPInvokeCallback]
	private static void OnMemberDisconnectImpl(IntPtr ptr, long lobbyId, long userId)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.LobbyManagerInstance.OnMemberDisconnect != null)
		{
			discord.LobbyManagerInstance.OnMemberDisconnect(lobbyId, userId);
		}
	}

	[MonoPInvokeCallback]
	private static void OnLobbyMessageImpl(IntPtr ptr, long lobbyId, long userId, IntPtr dataPtr, int dataLen)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.LobbyManagerInstance.OnLobbyMessage != null)
		{
			byte[] array = new byte[dataLen];
			Marshal.Copy(dataPtr, array, 0, dataLen);
			discord.LobbyManagerInstance.OnLobbyMessage(lobbyId, userId, array);
		}
	}

	[MonoPInvokeCallback]
	private static void OnSpeakingImpl(IntPtr ptr, long lobbyId, long userId, bool speaking)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.LobbyManagerInstance.OnSpeaking != null)
		{
			discord.LobbyManagerInstance.OnSpeaking(lobbyId, userId, speaking);
		}
	}

	[MonoPInvokeCallback]
	private static void OnNetworkMessageImpl(IntPtr ptr, long lobbyId, long userId, byte channelId, IntPtr dataPtr, int dataLen)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.LobbyManagerInstance.OnNetworkMessage != null)
		{
			byte[] array = new byte[dataLen];
			Marshal.Copy(dataPtr, array, 0, dataLen);
			discord.LobbyManagerInstance.OnNetworkMessage(lobbyId, userId, channelId, array);
		}
	}

	public IEnumerable<User> GetMemberUsers(long lobbyID)
	{
		int num = MemberCount(lobbyID);
		List<User> list = new List<User>();
		for (int i = 0; i < num; i++)
		{
			list.Add(GetMemberUser(lobbyID, GetMemberUserId(lobbyID, i)));
		}
		return list;
	}

	public void SendLobbyMessage(long lobbyID, string data, SendLobbyMessageHandler handler)
	{
		SendLobbyMessage(lobbyID, Encoding.UTF8.GetBytes(data), handler);
	}
}
