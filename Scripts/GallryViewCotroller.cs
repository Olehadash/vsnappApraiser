using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UPersian.Components;
using TMPro;

public class GallryViewCotroller : MonoBehaviour
{
    public static PhotoSession.PhotoFolder folder;
    #region Singleton
    private static GallryViewCotroller instance;
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

    public static GallryViewCotroller GetInstance
    {
        get
        {
            return instance;
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }
    #endregion
    #region Serialized Field
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI carnumberText;
    [SerializeField]
    private TextMeshProUGUI orderNumber;
    [Space(10)]
    [SerializeField]
    private GameObject galleryElement;
    [SerializeField]
    private GameObject container;

    [Space(10)]
    [SerializeField]
    private TestHome agora;
    #endregion
    #region Private Fields
    private List<Transform> elements = new List<Transform>();
    private static readonly SpriteMeshType spriteType = SpriteMeshType.Tight;
    private const float PixelsPerUnit = 100.0f;
    #endregion
    #region Init

    private void Start()
    {
        Init();
    }
    public void Init()
    {
        nameText.text = folder.garageName;
        carnumberText.text = folder.folderName;
        orderNumber.text = folder.data;

        Transform element;

        for (int i = 0; i< folder.files.Count; i++)
        {
            element = Instantiate(galleryElement.transform, new Vector3(0, 0, 0), Quaternion.identity);
            element.SetParent(container.transform);
            element.localScale = new Vector3(1, 1, 1);
            string p = Path.Combine(Application.persistentDataPath, folder.folderName +"_"+ folder.data
                + "_" + folder.modelid + "_" +folder.userid);
            string path = Path.Combine(p, folder.files[i].name);
            //Debug.Log(path);
            //element.gameObject.GetComponent<GalleryElement>().SetSprite = SpriteLoader.GetSpriteFromFile(path+"mini");
            element.gameObject.GetComponent<GalleryElement>().SetMiniSprite = SpriteLoader.GetSpriteFromFile(path + "mini");
            element.gameObject.GetComponent<GalleryElement>().Link = path;
            element.gameObject.SetActive(true);
            elements.Add(element);
        }


    }

    IEnumerator LoadFile(Transform element, string path)
    {
        yield return null;
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D photoTexture = LoadTextureByBytes(bytes);
            
        }catch{}

    }

    private static Texture2D LoadTextureByBytes(byte[] FileData)
    {
        Texture2D Tex2D = null;

        Tex2D = new Texture2D(2, 2);
        if (FileData.Length > 0)
        {
            Tex2D.LoadImage(FileData);
            //TextureScaler.Bilinear(Tex2D, 256, 256);
        }
        return Tex2D;
    }
    #endregion


    public void ExitFromGallery()
    {
        agora.onJoinButtonClicked();
    }

    public void onBackButtonPress()
    {
        SceneManager.LoadScene("Folder");
    }

    public void onGoToLastPage()
    {
        SceneManager.LoadScene("Sesions");
    }

    public void onGotoLogin()
    {
        SceneManager.LoadScene("LoginScene");
    }
}
