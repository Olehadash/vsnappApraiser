using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
#if(UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif
using System.Collections;

/// <summary>
///    TestHome serves a game controller object for this application.
/// </summary>
public class TestHome : MonoBehaviour
{
    #region Singleton
    private static TestHome instance;
    private static bool isNullInstance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_EDITOR
                System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackTrace(true).GetFrame(1);
                string scriptName = stackFrame.GetFileName();
                int lineNumber = stackFrame.GetFileLineNumber();
                Debug.LogError(scriptName + " instance not found at line " + lineNumber + " !");
#else
                Debug.LogError("CallingProccessManager instance not found!");
#endif
                return true;
            }
            return false;
        }
    }
    private void Awake()
    {
         instance = this;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
            permissionList.Add(Permission.Microphone);
            permissionList.Add(Permission.Camera);
#endif
    }

    public static TestHome GetInstance
    {
        get { return instance; }
    }
    #endregion
    // Use this for initialization
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList();
#endif
    static UnityVideo app = null;

    private string HomeSceneName = "LoginScene";

    private string PlaySceneName = "VideoChatGArage";

    // PLEASE KEEP THIS App ID IN SAFE PLACE
    // Get your own App ID at https://dashboard.agora.io/
    [SerializeField]
    private string AppID = "d69f0e3df2d94fb0b88a13a4d1340961";

    void Update()
    {
        CheckPermissions();
    }


    /// <summary>
    ///   Checks for platform dependent permissions.
    /// </summary>
    private void CheckPermissions()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach(string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {                 
				Permission.RequestUserPermission(permission);
			}
        }
#endif
    }

    public void JustOn()
    {
        // create app if nonexistent
        if (ReferenceEquals(app, null))
        {
            app = new UnityVideo(); // create app
            app.loadEngine(AppID); // load engine
        }
        app.join(HomePageControll.MODEL.user+"0");
        app.switchCamera();
    }

    public void LieaveOn()
    {
        if (!ReferenceEquals(app, null))
        {
            app.leave();
            app.unloadEngine();
            app = null;
        }
    }

        public void onJoinButtonClicked(bool IsLoadNewScene = true)
    {
        
        Debug.Log("agora_: onJoinButtonClicked");
        // create app if nonexistent
        if (ReferenceEquals(app, null))
        {
            app = new UnityVideo(); // create app
            app.loadEngine(AppID); // load engine
            Debug.Log("agora_: Engine Initialized To chanel " + HomePageControll.MODEL.user);
        }

        // join channel and jump to next scene
        app.join(HomePageControll.MODEL.user);
        if (IsLoadNewScene) {
            
            SceneManager.sceneLoaded += OnLevelFinishedLoading; // configure GameObject after scene is loaded
            SceneManager.LoadScene(PlaySceneName, LoadSceneMode.Single);
        }
    }

    public void ScreenReload()
    {
        app.ScreenReload();
    }

    public void onLeaveButtonClicked(bool UnloadScene = true)
    {
        //Debug.Log("agora_: onLeaveButtonClicked");
        if (!ReferenceEquals(app, null))
        {
            app.leave();       
            app.unloadEngine();
            app = null;
            if (UnloadScene)
                SceneManager.LoadScene(HomeSceneName, LoadSceneMode.Single);
            /*VideoCallPhotoManager.FolderName = "";
            VideoCallPhotoManager.FolderDate = null;
            GlobalParameters.isSameSession = false;*/
            Models.cmsg.comand = "stop";
            string json = JsonUtility.ToJson(Models.cmsg);
            WEbSocketController.GetInstance.SendMessage(json);
            //CallingProccessManager.EndCalling();
        }
        
    }

    public void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == PlaySceneName)
        {
            //RecallController.GetInstanse.SetWaytDialog();
            Debug.Log("agora_: OnLevelFinishedLoading");
            SetScreen();
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
    }

    private void SetScreen()
    {
        
        if (!ReferenceEquals(app, null))
        {
            Debug.Log("agora_: SetScreen");
            app.onSceneHelloVideoLoaded(); // call this after scene is loaded
        }
    }

    void OnApplicationPause(bool paused)
    {
        if (!ReferenceEquals(app, null))
        {
            app.EnableVideo(paused);
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        
    }

    void OnApplicationQuit()
    {
        if (!ReferenceEquals(app, null))
        {
            app.unloadEngine();
        }
    }
}
