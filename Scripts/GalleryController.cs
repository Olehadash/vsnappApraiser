using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UPersian.Components;
using UnityEngine.SceneManagement;
using UnityGoogleDrive;
using UnityGoogleDrive.Data;

public class GalleryController : MonoBehaviour
{
    #region Singleton
    private static GalleryController instance;
    public static bool isNullInstance
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

    public static GalleryController GetInstance
    {
        get
        {
            return instance;
        }
    }
    #endregion
    #region Serializable Fields

    [SerializeField]
    private GameObject Gallery;
    [Space(10)]
    [SerializeField]
    private InputField nameText;
    [SerializeField]
    private TextMeshProUGUI carnumberText;
    [SerializeField]
    private TextMeshProUGUI orderNumber;

    [Space(10)]
    [SerializeField]
    private Button callEndButton;
    [SerializeField]
    private Button recallButton;

    [Space(10)]
    [SerializeField]
    private GameObject galleryElement;
    [SerializeField]
    private GameObject container;
    [SerializeField]
    private TextMeshProUGUI timer;

    
    #endregion
    #region Private Fields
    private List<Transform> elements = new List<Transform>();
    #endregion
    #region Init
    GameObject nullObject;
    public void Init()
    {
        
        Gallery.SetActive(true);
        SwitchCallButtons(UnityVideo.success);

        nameText.text = HomePageControll.MODEL.name;
        carnumberText.text = VideoCallPhotoManager.FolderDate;
        orderNumber.text = VideoCallPhotoManager.FolderName;

        Transform element;

        Dictionary<string, Sprite> dict = VideoCallPhotoManager.GetPhotoSprites();

        foreach (var link in dict)
        {
            element = Instantiate(galleryElement.transform, new Vector3(0, 0, 0), Quaternion.identity);
            element.SetParent(container.transform);
            element.localScale = new Vector3(1, 1, 1);
            //element.gameObject.GetComponent<GalleryElement>().SetSprite = link.Value;
            element.gameObject.GetComponent<GalleryElement>().SetMiniSprite = SpriteLoader.GetSpriteFromFile(link.Key + "mini");
            element.gameObject.GetComponent<GalleryElement>().Link = link.Key;
            element.gameObject.SetActive(true);
            elements.Add(element);
        }

        element = Instantiate(galleryElement.transform, new Vector3(0, 0, 0), Quaternion.identity);
        element.SetParent(container.transform);
        element.localScale = new Vector3(1, 1, 1);
        element.gameObject.SetActive(true);
        nullObject = element.gameObject;
        elements.Add(element);

    }

    #endregion
    #region Call / Recall buttons Wayt
    public void onCallButtonPress()
    {
        StartCoroutine("WaitCorutine");
    }
    IEnumerator WaitCorutine()
    {
        int time = 5;
        timer.gameObject.SetActive(true);
        SwitchCallButtons(true);
        timer.text = time.ToString();
        while(time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            timer.text = time.ToString();
        }

        SwitchCallButtons(false);
        timer.gameObject.SetActive(false);
        JoinLeaveScript.GetInstance.onButtonClickedLeave();
        

    }

    public void onEndCallButtonPress()
    {
        SwitchCallButtons(false);
        timer.gameObject.SetActive(false);
        StopCoroutine("WaitCorutine");
    }
    #endregion

    #region Button Heandlers
    public void ExitFromGallery()
    {
        for (int i = 0; i < elements.Count; i++)
        {
            
            if (elements[i] != null)
            {
                elements[i].gameObject.GetComponent<GalleryElement>().SetSpriteAsNull();
                Destroy(elements[i].gameObject);
            }
            
        }
        Destroy(nullObject);
        nullObject = null;
        elements.Clear();
        Gallery.SetActive(false);
    }

    public void NeSimpleCamSession()
    {
        
        ExitFromGallery();
        VideoCallPhotoManager.DestroyAllPhotosList();
    }
    public void SwitchCallButtons(bool succsessConnection)
    {
        callEndButton.interactable = succsessConnection;
        recallButton.interactable = !succsessConnection;
    }
    public void GoToLastPage()
    {
        SceneManager.LoadScene(2);
    }

    public void PopElemet()
    {
        if(isNullInstance) return;
        elements.RemoveAt(elements.Count - 1);
       /* if(elements.Count == 1)
        {
            SavePhotoSessionController.RemoveFolder(VideoCallPhotoManager.FolderName);
        }*/
    }

    public void RemoveSession()
    {
        SavePhotoSessionController.RemoveFolder(VideoCallPhotoManager.FolderName);
    }
    #endregion
    private void OnDestroy()
    {
        instance = null;
    }

    public void onGoogleDrivaLoad()
    {
        SavePhotoSessionController.onGoogleDriveLoad();
    }
}
