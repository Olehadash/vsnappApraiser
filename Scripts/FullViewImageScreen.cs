using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class FullViewImageScreen : MonoBehaviour
{
    #region Singleton
    private static FullViewImageScreen instance;
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
                Debug.LogError("FullViewImageScreen instance not found!");
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
    #endregion
    #region Serializable Fields
    [SerializeField]
	private GameObject fullViewScreen;
    [SerializeField]
    private Image photoImage;
    [SerializeField]
    private GameObject sure;
    #endregion
    private bool isVisible = false;
    private string link = "";
    #region Show/Hide Card Creation Screen
    public static void ShowFullViewImageScreen(Sprite img, string l)
    {
        if (isNullInstance)
            return;
        instance.link = l;
        instance.isVisible = true;
        instance.fullViewScreen.SetActive(true);
        instance.photoImage.sprite = img;
    }
    public void HideFullViewImageScreen()
    {
        Destroy(instance.photoImage.sprite);
        instance.isVisible = false;
        instance.photoImage.sprite = null;
        photoImage.transform.localScale = new Vector3(1, 1, 1);
        photoImage.transform.localPosition = new Vector3(0, 0, 0);
        fullViewScreen.SetActive(false);
    }
    #endregion
    #region Delete GallaryElement
    private GalleryElement element;
    public static void SetElement(GalleryElement el)
    {
        if (isNullInstance)
            return;
        instance.element = el;
    }

    public static void DeleteElement()
    {
        if (isNullInstance)
            return;
        Destroy(instance.element.gameObject);
        VideoCallPhotoManager.GetInstance.RemovePhoto(instance.element.Link);
        string filename = Path.GetFileName(instance.element.Link);
        SavePhotoSessionController.GetInstance.RemoveFile(filename);
        if (VideoCallPhotoManager.photoList.ContainsKey(instance.link))
            VideoCallPhotoManager.photoList.Remove(instance.link);
        instance.HideFullViewImageScreen();
    }

    public void SetPic(bool active)
    {
        sure.SetActive(active);
    }
    #endregion
    #region Scaling
    private void LateUpdate()
    {
        if (isVisible)
        {
            Lean.LeanTouch.MoveObject(photoImage.transform, Lean.LeanTouch.DragDelta);
            Lean.LeanTouch.ScaleObject(photoImage.transform, Lean.LeanTouch.PinchScale);
        }
    }
    #endregion
}
