using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalleryElement : MonoBehaviour
{
    #region Serialize Fild
    [SerializeField]
    private GameObject icon;
    [SerializeField]
    private GameObject text;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Button button;
    #endregion
    #region Private Fields
    private string link;
    private Sprite img;
    private Sprite miniimg;

    private bool isSeted = false;
    #endregion
    #region Seters
    public Sprite SetSprite
    {
        set {
            icon.SetActive(false);
            text.SetActive(false);
            img = value;
            image.color = Color.white;

        }
    }

    public Sprite SetMiniSprite
    {
        set
        {
            icon.SetActive(false);
            text.SetActive(false);
            image.sprite = value;
            image.color = Color.white;
            isSeted = true;
        }
    }

    public string Link
    {
        set
        {
            link = value;
        }
        get
        {
            return link;
        }
    }
    
    #endregion
    private void Start()
    {
        //button.onClick.AddListener(OnButtonClick);
    }
    public void  OnButtonClick()
    {
        if (string.IsNullOrEmpty(link))
        {
            if(!GalleryController.isNullInstance)
                GalleryController.GetInstance.ExitFromGallery();
            else if(!GallryViewCotroller.isNullInstance)
                GallryViewCotroller.GetInstance.ExitFromGallery();
            Destroy(this.gameObject);
        }
        else
        {
            FullViewImageScreen.SetElement(this);
            FullViewImageScreen.ShowFullViewImageScreen(SpriteLoader.GetSpriteFromFile(Link), link);
        }
    }

    public void SetSpriteAsNull()
    {
        if (!isSeted) return;
        Destroy(image.sprite);
        image.sprite = null;
    }

}
