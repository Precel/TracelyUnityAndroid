// Tracely.io Android Unity Plugin
// Version 0.12
// All Rights Reserved

using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class TracelyManager : MonoBehaviour {

	public string APIKey = "";

	#pragma warning disable

	private string PluginClassName = "io.rwilinski.tracely.TracelyManager";

	//Android Classes 	
	private static AndroidJavaClass ExceptionHandlerPlugin;
	private AndroidJavaObject ExceptionHandlerInstance;

	private AndroidJavaClass unityPlayer;
	private AndroidJavaObject activity;
	private AndroidJavaObject context;

	#pragma warning restore

	//Constructor
	public TracelyManager() {

	}

	#region Singleton
	private static TracelyManager _instance;
	public static TracelyManager Instance {
		get {
			if(_instance == null) CreateGameObject();
			return _instance;
		}
	}

	private static void CreateGameObject() {
		GameObject go = new GameObject();
		go.name = "Tracely";
		go.AddComponent<TracelyManager>();
	}

	private void Awake() {
		if(TracelyManager.Instance != null) {
			UnityEngine.Debug.LogWarning("Found duplicate Tracely instance, destroying.");
			Destroy(this.gameObject);
		}

		_instance = this;
		DontDestroyOnLoad(this.gameObject);
	}
	#endregion

	#region Helpers
	private void Log(string msg) {
		UnityEngine.Debug.Log("[TracelyManager] "+msg);
	}
	#endregion


	#region Internal Functions

	private void OnEnable() {
		Log("Starting Tracely...");

		System.AppDomain.CurrentDomain.UnhandledException += _OnUnresolvedExceptionHandler;
		Application.logMessageReceived += _OnDebugLogCallbackHandler;
		
		RegisterExceptionHandler();
	}

	private void OnDisable() {
		Log("Unsubscribing delegates");

		System.AppDomain.CurrentDomain.UnhandledException -= _OnUnresolvedExceptionHandler;
		Application.logMessageReceived -= _OnDebugLogCallbackHandler;
	}

	//Android Layer Exception Handler and communication module
	public void RegisterExceptionHandler() {
		#if UNITY_ANDROID && !UNITY_EDITOR
		try {
			unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			context = activity.Call<AndroidJavaObject>("getApplicationContext");

			ExceptionHandlerPlugin = new AndroidJavaClass(PluginClassName);
			ExceptionHandlerPlugin.CallStatic("SetApiKey", this.APIKey);
			ExceptionHandlerPlugin.CallStatic<bool>("RegisterExceptionHandler", context);

			Log("Exception Handler registering success!");
		}
		catch(System.Exception e) {
			Log("Failed to register exception handler, details: "+e.Message+" | "+e.StackTrace);
		}
		#else
		Log("Tracely is not compatible yet with that platform yet :(");
		#endif
	}

	static private void _OnUnresolvedExceptionHandler (object sender, System.UnhandledExceptionEventArgs args)
	{
		if (args == null || args.ExceptionObject == null) {
				return;
		}

		if (args.ExceptionObject.GetType () != typeof(System.Exception)) {
				return;
		}

		TracelyManager.SendUnhandledException((System.Exception)args.ExceptionObject);
	}

	static private void _OnDebugLogCallbackHandler (string name, string stack, LogType type)
	{
		if (LogType.Assert != type && LogType.Exception != type && LogType.Error != type) {
			TracelyManager.ExceptionHandlerPlugin.CallStatic("AddToUserLog", type.ToString(), name);
			return;
		}

		if(TracelyManager.ExceptionHandlerPlugin != null) {
			try {
				if(stack == null) { // Unfortunately development builds don't pass stack parameter
					StackTrace trace = new StackTrace(true);
					stack = trace.ToString();
				}

				TracelyManager.ExceptionHandlerPlugin.CallStatic ("RegisterUnhandledException", GetName(name), GetCause(name), stack);
			} 
			catch (System.Exception e) {
				UnityEngine.Debug.Log("Unable to write exception to tracely.io plugin. "+GetName(name)+" - "+GetCause(name)+", because of "+e.Message);
			}
		}
	}

	private static string GetCause(string msg) {
		int colonIndex = msg.IndexOf(":");
		string cause = "";
		if(colonIndex > 0) {
			cause = msg.Substring(colonIndex+2, msg.Length-colonIndex-2);
			// TracelyManager.Instance.cause = cause;
		} else {
			cause = "Unity Engine Exception";
		}
		return cause;
	}

	private static string GetName(string msg) {
		int colonIndex = msg.IndexOf(":");
		string name = "Exception";
		if(colonIndex > 0) {
			name = msg.Substring(0, colonIndex);
		} else {
			name = "Unity Engine Exception";
		}
		return name;
	}

	#endregion




	#region Public Functions

	public static void SendHandledException(System.Exception e) {
		ExceptionHandlerPlugin.CallStatic("RegisterHandledException", GetName(e.Message), GetCause(e.Message), e.StackTrace);
	}

	public static void SendUnhandledException(System.Exception e) {
		ExceptionHandlerPlugin.CallStatic("RegisterHandledException", GetName(e.Message), GetCause(e.Message), e.StackTrace);
	}

	public void GetDeviceID() {
		Log("Device ID: "+SystemInfo.deviceUniqueIdentifier);
	}

	#endregion
}
