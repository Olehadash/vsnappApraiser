using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using agora_gaming_rtc;

using AndroidGoodiesExamples;
#if UNITY_ANDROID
using DeadMosquito.AndroidGoodies.Internal;
using DeadMosquito.AndroidGoodies;
#endif
using JetBrains.Annotations;

public class WebCamPhotoManager : MonoBehaviour
{
    #region Singleton
    private static WebCamPhotoManager instance;
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
                Debug.LogError("WebCamPhotoManager instance not found!");
#endif
                return true;
            }
            return false;
        }
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public static WebCamPhotoManager GetInstance
    {
        get
        {
            return instance;
        }
    }

    void OnDestroy()
    {
        instance = null;
        //FL_Stop();
    }
    #endregion

    #region Serializable Fields
    public Image background;

    public RectTransform webCamTextureRect;

    public OtherGoodiesTest goodies;

    public VideoSurface surface;

    [SerializeField]
    private Sprite lightOff;
    [SerializeField]
    private Sprite lightOn;
    [SerializeField]
    private Image button;
    #endregion

    #region Private Fields
    private WebCamTexture webCamTexture;

    private System.Action<string> OnScreenshotSaved;

    private string screenshotName;

    private bool takeScreenshotOnNextFrame;

    private bool light = false;
    private AndroidJavaObject camera1;
    #endregion
    #region WebCamera Controller
    private void Start()
    {
        if (SceneManager.GetActiveScene().name.Equals("SimpleCamera"))
        {
            TestHome.GetInstance.JustOn();
            surface.SetEnable(true);
        }
    }

    public void leave()
    {
        TestHome.GetInstance.LieaveOn();
    }


    public static void StartWebCam()
    {
        if (isNullInstance)
            return;

        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            StopWebCam();
            return;
        }

        instance.webCamTexture = new WebCamTexture(devices[0].name, Screen.width, Screen.height /*768, 1024*/);

        instance.webCamTexture.Play();
        instance.background.material.mainTexture = instance.webCamTexture;
    }

    public void CamStop()
    {
        instance.webCamTexture.Stop();
    }

    public void CamPlay()
    {
        //instance.webCamTexture.Play();
    }

    public static void CaptureWebCam(string screenshotName, System.Action<string> OnScreenshotSaved)
    {
        if (isNullInstance)
            return;

        instance.takeScreenshotOnNextFrame = true;
        instance.screenshotName = screenshotName;
        instance.OnScreenshotSaved = OnScreenshotSaved;
    }

    private void OnPostRender()
    {
        if (takeScreenshotOnNextFrame)
        {
            takeScreenshotOnNextFrame = false;

            Texture2D captureTex = new Texture2D(webCamTexture.width, webCamTexture.height);
            captureTex.SetPixels(webCamTexture.GetPixels());
            captureTex.Apply();
            captureTex = TextureRotator.RotateTexture(captureTex, true);

            byte[] encodedBytes = captureTex.EncodeToJPG();
            SaveScreenshot(encodedBytes);
        }
    }

    private void SaveScreenshot(byte[] encodedBytes)
    {
        screenshotName = screenshotName + "-" +
            DateStringConverter.GetMDHMSMDate() + ".jpg";
        string pathToFile = Path.Combine(GlobalSettings.cloudStorageUploadPath, screenshotName);
        File.WriteAllBytes(pathToFile, encodedBytes);
        OnScreenshotSaved(pathToFile);
    }

    public static void StopWebCam()
    {
        if (isNullInstance)
            return;

        if (instance.webCamTexture != null)
            instance.webCamTexture.Stop();
    }

    private void Update()
    {
        /*if (instance.webCamTexture == null)
            return;

        float scaleY = webCamTexture.videoVerticallyMirrored ? -1f : 1f;
        webCamTextureRect.localScale = new Vector3(1f, 1, 1f);

        var orient = -webCamTexture.videoRotationAngle;
        webCamTextureRect.rotation = Quaternion.Euler(new Vector3(0f, 0f, orient));*/
    }
    #endregion
    #region Flash Light Controller
    public void OnOffLight()
    {
        if (light)
        {
            button.sprite = lightOff;
            goodies.OnFlashlightOff();
            light = false;
            
        }
        else
        {
            button.sprite = lightOn;
            goodies.OnFlashlightOn();
            light = true;
        }
    }

    public void ofLight()
    {
        /*if (light) return;
        button.sprite = lightOff;
        goodies.OnFlashlightOff();
        light = false;*/
    }
    #endregion
}
