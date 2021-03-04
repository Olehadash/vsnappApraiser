using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
#if (UNITY_2018_3_OR_NEWER)
using UnityEngine.Android;
#endif

public class JoinLeaveScript : MonoBehaviour
{

	#region Singleton
	private static JoinLeaveScript instance;
	private void Awake()
	{
		if (instance == null)
			instance = this;
	}

	public static JoinLeaveScript GetInstance
	{
		get
		{
			return instance;
		}
	}

	#endregion

	// Use this for initialization
	private ArrayList permissionList = new ArrayList();

	public GameObject msg;
	public GameObject waytforConnect;
	public GameObject recall;

	void Start()
	{
#if (UNITY_2018_3_OR_NEWER)
		permissionList.Add(Permission.Microphone);
		permissionList.Add(Permission.Camera);
#endif
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	private void CheckPermission()
	{
#if (UNITY_2018_3_OR_NEWER)
		foreach (string permission in permissionList)
		{
			if (Permission.HasUserAuthorizedPermission(permission))
			{

			}
			else
			{
				Permission.RequestUserPermission(permission);
			}
		}
#endif
	}

	// Update is called once per frame
	void Update()
	{
#if (UNITY_2018_3_OR_NEWER)
		CheckPermission();
#endif
	}

	static UnityVideo app = null;

	private void onJoinButtonClicked()
	{
		Debug.Log("agora_: onJoinButtonClicked");
		// get parameters (channel name, channel profile, etc.)
		//GameObject go = GameObject.Find ("ChannelName");
		//InputField field = go.GetComponent<InputField>();

		// create app if nonexistent
		if (ReferenceEquals(app, null))
		{
			app = new UnityVideo(); // create app
			                    //app = new GameObject().AddComponent<TestHelloUnityVideo>();
			app.loadEngine(""); // load engine
			Debug.Log("agora_: ApkLoaded");
		}

		// join channel and jump to next scene
		app.join("test");
		//VideoTimer.isStarted = true;
	}

	private void onLeaveButtonClicked()
	{
		if (!ReferenceEquals(app, null))
		{
			app.leave(); // leave channel
			app.unloadEngine(); // delete engine
			app = null; // delete app
			//VideoTimer.timer = 0f;
			//VideoTimer.isStarted = false;
		}
	}

	public void SwitchCamera()
	{
		app.switchCamera();
	}

	public void onButtonClickedJoin()
	{
		// which GameObject?
		//if (name.CompareTo ("JoinButton") == 0) {
		//	onJoinButtonClicked ();
		//}
		//else if(name.CompareTo ("LeaveButton") == 0) {
		//	onLeaveButtonClicked ();
		//}

		onJoinButtonClicked();
	}

	public void onButtonClickedLeave()
	{
		// which GameObject?
		//if (name.CompareTo ("JoinButton") == 0) {
		//	onJoinButtonClicked ();
		//} else if (name.CompareTo ("LeaveButton") == 0) {
		//	onLeaveButtonClicked ();
		//}

		onLeaveButtonClicked();
	}



}
