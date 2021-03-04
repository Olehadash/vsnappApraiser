using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using UnityEngine.UI;

// this is an example of using Agora Unity SDK
// It demonstrates:
// How to enable video
// How to join/leave channel
// 
public class UnityVideo : MonoBehaviour {

	// PLEASE KEEP THIS App ID IN SAFE PLACE
	// Get your own App ID at https://dashboard.agora.io/
	//private static string appId = "58de5f787c6848feb866522f1998391e";
	public static bool success = false;
	
	// load agora engine
	public void loadEngine(string appId)
	{
		// start sdk
		Debug.Log ("agora_: initializeEngine");
		if (mRtcEngine != null) {
			Debug.Log ("agora_: Engine exists. Please unload it first!");
			return;
		}

		// init engine
		mRtcEngine = IRtcEngine.getEngine (appId);

		// enable log
		mRtcEngine.SetLogFilter (LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
	}

	public void join(string channel)
	{
		Debug.Log ("agora_:" + "calling join (channel = " + channel + ")");
		//VideoTimer.isStarted = true;

		if (mRtcEngine == null)
			return;

		// set callbacks (optional)
		mRtcEngine.OnJoinChannelSuccess = onJoinChannelSuccess;
		mRtcEngine.OnUserJoined = onUserJoined;
		mRtcEngine.OnUserOffline = onUserOffline;
		mRtcEngine.EnableVideo();
		mRtcEngine.EnableVideoObserver();
		mRtcEngine.JoinChannel(channel, null, 0);

		Debug.Log ("agora_: " + "initializeEngine done");
	}

	public string getSdkVersion () {
		return IRtcEngine.GetSdkVersion ();
	}

	public void leave()
	{
		//Debug.Log ("agora_:" + "calling leave");
		success = false;
		if (mRtcEngine == null)
			return;

		// leave channel
		mRtcEngine.LeaveChannel();
		// deregister video frame observers in native-c code
		mRtcEngine.DisableVideoObserver();
	}

	public void switchCamera()
    {
		mRtcEngine.SwitchCamera();
	}

	// unload agora engine
	public void unloadEngine()
	{
		//Debug.Log ("agora_:" + "calling unloadEngine");

		// delete
		if (mRtcEngine != null) {
			IRtcEngine.Destroy ();
			mRtcEngine = null;
		}
	}

	// accessing GameObject in Scnene1
	// set video transform delegate for statically created GameObject
	public void onSceneHelloVideoLoaded()
	{
		Debug.Log("onSceneHelloVideoLoaded_________________________________");
		GameObject go = RecallController.GetInstanse.GetLocal;
		VideoSurface o = go.AddComponent<VideoSurface> ();

		o.SetEnable(true);
	}

	// instance of agora engine
	public IRtcEngine mRtcEngine;

	// implement engine callbacks

	public uint mRemotePeer = 0; // insignificant. only record one peer

	private void onJoinChannelSuccess (string channelName, uint uid, int elapsed)
	{
		Debug.Log ("agora_:" + "JoinChannelSuccessHandler: uid = " + uid);
		onSceneHelloVideoLoaded();
		/*GameObject textVersionGameObject = GameObject.Find ("msg");
		textVersionGameObject.GetComponent<Text> ().text = "Waiting for receiver...";*/
	}

	// When a remote user joined, this delegate will be called. Typically
	// create a GameObject to render video on itRecallController.GetInstanse
	private void onUserJoined(uint uid, int elapsed)
	{
		success = true;
		Debug.Log ("agora_:"+"onUserJoined: uid = " + uid);
		// this is called in main thread
		RecallController.GetInstanse.StopWaytingDialog();
		// find a game object to render video stream from 'uid'
		GameObject go = RecallController.GetInstanse.GetRemote;
		go.SetActive(true);
		VideoSurface o = go.AddComponent<VideoSurface> ();
		o.SetForUser (uid);
		o.SetEnable (true);
		mRemotePeer = uid;
	}

	public void ScreenReload()
	{
		Debug.Log("ScreenReload");
		onSceneHelloVideoLoaded();
		onUserJoined(mRemotePeer, 0);

	}

	// When remote user is offline, this delegate will be called. Typically
	// delete the GameObject for this user
	private void onUserOffline(uint uid, USER_OFFLINE_REASON reason)
	{
		// remove video stream
		Debug.Log ("agora_:" + "onUserOffline: uid = " + uid);
		// this is called in main thread
		GameObject go = RecallController.GetInstanse.GetRemote;

		if (!ReferenceEquals (go, null)) {
			Destroy (go);
		}
		//VideoTimer.timer = 0f;
		//VideoTimer.isStarted = false;
	}

	// delegate: adjust transfrom for game object 'objName' connected with user 'uid'
	// you could save information for 'uid' (e.g. which GameObject is attached)
	private void onTransformDelegate (uint uid, string objName, ref Transform transform)
	{
		if (uid == 0) {
			transform.position = new Vector3 (0f, 2f, 0f);
			transform.localScale = new Vector3 (2.0f, 2.0f, 1.0f);
			//transform.Rotate (0f, 1f, 0f);
		} else {
			//transform.Rotate (0.0f, 1.0f, 0.0f);
		}
	}

	public void EnableVideo(bool pauseVideo)
	{

		if (mRtcEngine != null)
		{
			if (!pauseVideo)
			{
				mRtcEngine.EnableVideo();
			}
			else
			{
				mRtcEngine.DisableVideo();
			}
		}
	}
}
