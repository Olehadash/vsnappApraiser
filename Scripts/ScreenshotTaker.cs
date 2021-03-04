using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotTaker : MonoBehaviour
{
    #region Singleton
    private static ScreenshotTaker instance;
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
                Debug.LogError("agora_" + scriptName + " instance not found at line " + lineNumber + " !");
#else
                Debug.LogError("agora_" + "ScreenshotTaker instance not found!");
#endif
                return true;
            }
            return false;
        }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //Debug.Log("agora_: ScreenShoot Activate");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    #region Serializable Fields
    [SerializeField]
    private GameObject UIobj;
    [SerializeField]
    private GameObject Localscreen;
    [SerializeField]
    private AudioSource audio;
    #endregion

    #region Private Fields
    private Camera mainCamera;

    private System.Action<string, string> OnScreenshotSaved;

    private string screenshotName;

    private bool takeScreenshotOnNextFrame;
    #endregion

    #region Setup
    private void Start()
    {
        mainCamera = GetComponent<Camera>();
    }
    #endregion

    #region Capture Screen Part
    public static void CaptureScreenshot(string screenshotName, System.Action<string, string> OnScreenshotSaved)
    {
        if (isNullInstance)
            return;
        //Debug.Log("agora_:  CaptureScreenshot");
        instance.screenshotName = screenshotName;
        instance.UIobj.SetActive(false);
        if(instance.Localscreen != null)
            instance.Localscreen.SetActive(false);
        MenuController.HideMenuButton();
        instance.takeScreenshotOnNextFrame = true;
        instance.mainCamera.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
    }
    
    private void OnPostRender()
    {
        if (takeScreenshotOnNextFrame)
        {
            takeScreenshotOnNextFrame = false;
            Debug.Log("agora_: OnPostRender");


            RenderTexture renderTexture = mainCamera.targetTexture;

            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);
            
            screenshotName += screenshotName
                + "_" + HomePageControll.MODEL.id.ToString() + "_" + Models.user.id.ToString()
                + System.DateTime.Now.Day.ToString()+"."
               + System.DateTime.Now.Month.ToString()+"."
               + System.DateTime.Now.Year.ToString() + "-"
               + System.DateTime.Now.Hour.ToString() + "-"
               + System.DateTime.Now.Minute.ToString() + "-"
               + System.DateTime.Now.Second.ToString() + ".jpg";
            audio.Play();
            SaveScreenshot(renderResult.EncodeToJPG());

            UIobj.SetActive(true);
            if(Localscreen != null)Localscreen.SetActive(true);
            MenuController.ShowMenuButton();
            RenderTexture.ReleaseTemporary(renderTexture);
            mainCamera.targetTexture = null;
        }
    }

    private void SaveScreenshot(byte[] photoBytes)
    {
        
        string pathToFile = Path.Combine(GlobalSettings.cloudStorageUploadPath, screenshotName);
        File.WriteAllBytes(pathToFile, photoBytes);
        VideoCallPhotoManager.GetInstance.ShowSpriteFromPhoto(pathToFile, screenshotName);

    }
    #endregion
}
