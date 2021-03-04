using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public enum CheckingTextType { Unknown, CarNumber, CardNumber }
public class PhotoChecking : MonoBehaviour
{
    #region Singleton
    private static PhotoChecking instance;
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
                Debug.LogError("PhotoNaming instance not found!");
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

    public static PhotoChecking GetInstance
    { get
        {
            return instance;
        } 
    }
    #endregion

    #region Serializable Fields
    [SerializeField]
    private GameObject checkingWindowObj;
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private Image photoImage;
    [SerializeField]
    private InputField checkingText;
    [SerializeField]
    private GameObject ErrorMessage;

    [Space(10)]
    [SerializeField]
    private GameObject webCamImageObj;
    [SerializeField]
    private GameObject photoImageObj;
    [SerializeField]
    private GameObject rephotoButtonObj;
    [SerializeField]
    private GameObject photoButtonObj;

    [Space(10)]
    [SerializeField]
    private TextMeshProUGUI headerText;
    [SerializeField]
    private TextMeshProUGUI placeholderText;
    #endregion

    #region Private Fields
    private System.Action OnManualCloseEvent;
    private System.Action OnManualConfirmEvent;

    private Vector2 showPosition;
    private Vector2 hidePosition;

    private CheckingTextType checkingTextType;

    private const string cardNumberCheckText = "בדיקת מספר הזמנת תיקון";
    private const string carNumberCheckText = "בדיקת מספר רכב";

    private const string cardNumberPlaceholderText = "מס'  הזמנת";
    private const string carNumberPlaceholderText = "מספר רכב";

    private const string photosDefaultPrefix = "Vsnaap";

    private string lastCheckingText;

    private const float movingSpeed = 5000f;

    private bool isMoving;
    #endregion

    #region Setup
    private void Start()
    {
        showPosition = Vector2.zero;
        hidePosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y);
        rectTransform.anchoredPosition = hidePosition;
    }
    #endregion

    #region Open/Close Part

    public static void OpenPhotoCheck(Sprite sprite, string checkingText, CheckingTextType checkingTextType, bool isVideoCall)
    {
        if (isNullInstance)
            return;

        if (instance.isMoving)
            return;
        Debug.Log("agora_: OpenPhotoCheck");
        instance.isMoving = true;
        instance.checkingText.interactable = true;
        instance.checkingTextType = checkingTextType;
        instance.checkingText.text = VideoCallPhotoManager.FolderName;
        instance.lastCheckingText = VideoCallPhotoManager.FolderName;
        instance.photoImage.sprite = sprite;

        instance.webCamImageObj.SetActive(false);
        instance.photoImageObj.SetActive(true);

        if (isVideoCall)
        {
            instance.rephotoButtonObj.SetActive(true);
            instance.photoButtonObj.SetActive(true);
        }
        else
        {
            instance.photoButtonObj.SetActive(false);
            instance.rephotoButtonObj.SetActive(false);
        }

        instance.StartCoroutine(instance.ShowWindowCoroutine());

        if (checkingTextType == CheckingTextType.CardNumber)
        {
            instance.headerText.text = cardNumberCheckText;
            instance.placeholderText.text = cardNumberPlaceholderText;
        }
        else
        {
            instance.headerText.text = carNumberCheckText;
            instance.placeholderText.text = carNumberPlaceholderText;
        }
    }

    

    private IEnumerator ShowWindowCoroutine()
    {
        checkingWindowObj.SetActive(true);
        //MenuController.SetCurrentScreenFirst(checkingWindowObj.transform);
        while (rectTransform.anchoredPosition.y != showPosition.y)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition,
                showPosition, movingSpeed * Time.deltaTime);
            yield return null;
        }
        isMoving = false;
    }

    private IEnumerator RedErorAnimation()
    {
        int i = 0;
        Image img = instance.checkingText.gameObject.GetComponent<Image>();
        bool b = false;
        while(i<6)
        {
            yield return new WaitForSeconds(.1f);
            i++;
            b = !b;
            ErrorMessage.SetActive(b);

        }
    }

    public void ClosePhotoCheck(bool isManual)
    {
        if(string.IsNullOrEmpty(instance.checkingText.text) && isManual)
        {
            StartCoroutine("RedErorAnimation");
            return;
        }
        /*if (instance.isMoving)
            return;

        instance.isMoving = true;

        if (isManual)
            OnManualCloseEvent?.Invoke();
        else
            OnManualConfirmEvent?.Invoke();*/

        OnManualCloseEvent = null;
        OnManualConfirmEvent = null;

        if(isManual) VideoCallPhotoManager.GetInstance.SaveInFolder();
        StartCoroutine(HideWindowCoroutine());
    }

    public static void AddManualCloseEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        if (instance.OnManualCloseEvent != null)
            instance.OnManualCloseEvent -= instance.OnManualCloseEvent;
        instance.OnManualCloseEvent += eventListiner;
    }

    public static void AddManualConfirmEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        if (instance.OnManualConfirmEvent != null)
            instance.OnManualConfirmEvent -= instance.OnManualConfirmEvent;
        instance.OnManualConfirmEvent += eventListiner;
    }

    private IEnumerator HideWindowCoroutine()
    {
        while (rectTransform.anchoredPosition.y != hidePosition.y)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition,
                hidePosition, movingSpeed * Time.deltaTime);
            yield return null;
        }
        checkingWindowObj.SetActive(false);
        isMoving = false;
    }
    #endregion

    #region Change Text
    public void ChangeTextFolder()
    {
       /* if (IsDigitsOnly(instance.checkingText.text))
        {*/
            VideoCallPhotoManager.FolderName = instance.checkingText.text;
        /*}
        else
        {
            instance.checkingText.text = VideoCallPhotoManager.FolderName;
        }*/
    }

    bool IsDigitsOnly(string str)
    {
        foreach (char c in str)
        {
            if (c < '0' || c > '9')
                return false;
        }

        return true;
    }
    #endregion

    #region WebCam Part
    public void ActivateWebCam()
    {
        rephotoButtonObj.SetActive(false);
        photoImageObj.SetActive(false);
        webCamImageObj.SetActive(true);
        photoButtonObj.SetActive(true);
    }

    public void MakePhoto()
    {
        photoButtonObj.SetActive(false);
        string photoName = photosDefaultPrefix;
        WebCamPhotoManager.CaptureWebCam(photoName, CaptureWebCamHandle);
    }

    private void CaptureWebCamHandle(string pathToPhoto)
    {
        if (string.IsNullOrEmpty(pathToPhoto))
        {
            Debug.Log("agora_: CaptureWebCamHandle has empty path!");
            return;
        }

        Debug.Log("agora_:CaptureWebCamHandle on path: " + pathToPhoto);
        Sprite photoSprite = SpriteLoader.GetSpriteFromFile(pathToPhoto);
        if (photoSprite == null)
        {
            Debug.LogError("agora_: CaptureWebCam complete, but sprite is null in path:\n" +
                pathToPhoto);
            return;
        }

        photoImage.sprite = photoSprite;

        rephotoButtonObj.SetActive(true);
        photoImageObj.SetActive(true);
        webCamImageObj.SetActive(false);

        //CardAdditionalScreen.WebCamCaptureHandle(pathToPhoto, photoSprite);
    }

    private void CaptureDeviceCamHandle(string pathToPhoto)
    {
        if (string.IsNullOrEmpty(pathToPhoto))
        {
            Debug.Log("CaptureDeviceCamHandle has empty path!");
            return;
        }

        StartCoroutine(LateSaveCapturedPhoto(pathToPhoto));
    }

    private IEnumerator LateSaveCapturedPhoto(string photoPath)
    {
        yield return null;

        Debug.Log("CaptureDeviceCamHandle on path: " + photoPath);
        byte[] bytes = File.ReadAllBytes(photoPath);
        File.Delete(photoPath);

        Sprite photoSprite = SpriteLoader.GetSpriteFromBytes(bytes);
        if (photoSprite == null)
        {
            Debug.LogError("CaptureDeviceCam complete, but sprite is null in path:\n" +
                photoPath);
            yield break;
        }

        Sprite rotatedSprite = TextureRotator.RotateSprite(photoSprite);
        bytes = rotatedSprite.texture.EncodeToJPG();
        string photoName = "Vsm-Appraiser-devicePhoto-" + DateStringConverter.GetMDHMSMDate() + ".jpg";
        photoPath = Path.Combine(GlobalSettings.cloudStorageUploadPath, photoName);
        File.WriteAllBytes(photoPath, bytes);
        Debug.Log("CaptureDeviceCamHandle replaced by new path " + photoPath);

        //CardAdditionalScreen.DeviceCameraCaptureHandle(photoPath, rotatedSprite);
    }
    #endregion

    #region Geters
    public string GetFolderName
    {
        get
        {
            return VideoCallPhotoManager.FolderName+ "_" +VideoCallPhotoManager.FolderDate
                + "_" + HomePageControll.MODEL.id.ToString() + "_" + Models.user.id.ToString();
        }
    }
    #endregion
}
