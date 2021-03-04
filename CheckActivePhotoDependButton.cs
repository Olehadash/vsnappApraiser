using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckActivePhotoDependButton : MonoBehaviour
{
    public Button folderbut;
    public Button FinishSeesionButton;
    public Button NewSessionButton;
    public HomePageControll homepage;
    public GalleryController gallery;

    // Update is called once per frame
    void Update()
    {
        folderbut.interactable = !string.IsNullOrEmpty(VideoCallPhotoManager.FolderName);
        FinishSeesionButton.interactable = VideoCallPhotoManager.photoList.Count > 0;
        NewSessionButton.interactable = VideoCallPhotoManager.photoList.Count > 0;
    }

    public void onBackButtonPres()
    {
        if(string.IsNullOrEmpty(VideoCallPhotoManager.FolderName))
        {
            homepage.GoToGaragePage();
            TestHome.GetInstance.LieaveOn();
        }
        else
        {
            gallery.Init();
        }
    }
}
