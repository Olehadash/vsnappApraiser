using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
public class VideoCallPhotoManager : MonoBehaviour
{
    #region Singleton
    private static VideoCallPhotoManager instance;
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
                Debug.LogError("VideoCallPhotoManager instance not found!");
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

    public static VideoCallPhotoManager GetInstance
    {
        get
        {
            return instance;
        }
    }
    #endregion


    [SerializeField]
    private GameObject photoCaptireImag;
    [SerializeField]
    private GameObject errorImag;

    public static string FolderName = "";
    public static string FolderDate = "";
    #region Private Fields
    private Dictionary<string, Sprite> photoSprites =
    new Dictionary<string, Sprite>();
    private Sprite photo;
    private List<string> photoNames = new List<string>();
    private string fileName = "";
    #endregion

    public void VideoScreenshotCapture()
    {
        //Debug.Log("agors_: VideoScreenshotCapture");
        string photoName = "";
        ScreenshotTaker.CaptureScreenshot(photoName, ShowSpriteFromPhoto);
    }

    public void ShowSpriteFromPhoto(string pathToPhoto, string fname)
    {
        Debug.Log("agora_: ShowSpriteFromPhoto");
        fileName = fname;
        
        StartCoroutine(WaitForFile(pathToPhoto));
    }

    private IEnumerator WaitForFile(string pathToPhoto)
    {
        //Debug.Log("agora_: WaitForFile");
        yield return null;
        Sprite photoSprite = SpriteLoader.GetSpriteFromFile(pathToPhoto);
        
        photoNames.Add(pathToPhoto);

        CheckingTextType checkingTextType = CheckingTextType.CarNumber;
        photo = photoSprite;
        if (string.IsNullOrEmpty(VideoCallPhotoManager.FolderName))
        {
            PhotoChecking.OpenPhotoCheck(photoSprite, FolderName, checkingTextType, true);
            PhotoChecking.AddManualCloseEventListiner(RemoveLastPhoto);
        }
        else
        {
            StartCoroutine(PhotoMakeAnim());
            SaveInFolder();
        }
        


    }
    IEnumerator PhotoMakeAnim()
    {
        yield return null;
        photoCaptireImag.SetActive(true);
        yield return null;
        photoCaptireImag.SetActive(false);

    }

    public void RemoveLastPhoto()
    {
        if (photoNames.Count == 0) return;
        string lastPhotoPath = photoNames[photoNames.Count - 1];
        photo = null;
        photoNames.Remove(lastPhotoPath);
        DeleteFile(lastPhotoPath);

    }
    public void RemovePhoto(string lastPhotoPath)
    {
        if (GalleryController.GetInstance != null) GalleryController.GetInstance.PopElemet();
        photoSprites.Remove(lastPhotoPath);
        photoNames.Remove(lastPhotoPath);
        DeleteFile(lastPhotoPath);
    }

    public static void DestroyAllPhotosList()
    {
        if (isNullInstance)
            return;

        instance.photoSprites.Clear();
        instance.photoNames.Clear();
    }

    #region Move Img and Delete IMG Files
    public void SaveInFolder()
    {
        
        MoveFileToFolder(fileName);
    }

    IEnumerator ErrorShow()
    {
        yield return null;
        errorImag.SetActive(true);
        yield return null;
        errorImag.SetActive(false);
        yield return null;
        errorImag.SetActive(true);
        yield return null;
        errorImag.SetActive(false);
        yield return null;
        errorImag.SetActive(true);
        yield return null;
        errorImag.SetActive(false);
    }

    private void MoveFileToFolder(string screenshotName)
    {
        if (string.IsNullOrEmpty(VideoCallPhotoManager.FolderDate))
        {
            VideoCallPhotoManager.FolderDate = System.DateTime.Now.Year.ToString() + "-"
               + System.DateTime.Now.Month.ToString() + "-"
               + System.DateTime.Now.Day.ToString() + "_"
               + System.DateTime.Now.Hour + "-" + System.DateTime.Now.Minute + "-" + System.DateTime.Now.Second;
        }

        string oldPathToFile = photoNames[photoNames.Count - 1];
        string newPathToFile = Path.Combine(Application.persistentDataPath, PhotoChecking.GetInstance.GetFolderName);
        
        if (!GlobalParameters.isSameSession)
        {
            Directory.CreateDirectory(newPathToFile);
        }
        SavePhotoSessionController.AddFile(FolderName, screenshotName);
        SavePhotoSessionController.Sawe();
        
        try { 
            File.Move(oldPathToFile, Path.Combine(newPathToFile, screenshotName));
            Debug.Log("agora_: " + screenshotName + " move To: " + newPathToFile);
            photoNames[photoNames.Count - 1] = newPathToFile;
            photoSprites.Add(Path.Combine(newPathToFile, screenshotName), null);

        }catch (IOException e)
        {
            Debug.Log("File did not saved: " + e);
            Debug.Log(newPathToFile);
            return;
        }
        SpriteLoader.SaweSpriteMini(Path.Combine(newPathToFile, screenshotName));


    }

    private void DeleteFile(string path)
    {
        File.Delete(path);
    }

    #endregion
    #region Getters
    public static Dictionary<string, Sprite> GetPhotoSprites()
    {
        if (isNullInstance)
            return null;

        return instance.photoSprites;
    }

    public static Dictionary<string, Sprite> photoList
    {
        get
        {
            return instance.photoSprites;
        }
    }

    public static int GetPhotosCount()
    {
        if (isNullInstance)
            return -1;

        return instance.photoSprites.Count;
    }
    #endregion
}
