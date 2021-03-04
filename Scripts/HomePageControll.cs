using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class HomePageControll : MonoBehaviour
{
    public static UserModel MODEL = new UserModel();
    public static bool isCalling = true;
    public static bool onGaraje = false;
    [SerializeField]
    private GameObject GarageList;
    [SerializeField]
    private GameObject HomePage;

    private void Start()
    {
        if(onGaraje && GarageList != null)
        {
            GarageList.SetActive(true);
        }
    }

    public void GotoGarageList(bool iscall = true)
    {
        isCalling = iscall;
        HomePage.SetActive(false);
        GarageList.SetActive(true);
    }

    public void BackToHome()
    {
        HomePage.SetActive(true);
        GarageList.SetActive(false);
    }

    public void GoToVidoeCall()
    {
        onGaraje = false;
        SceneManager.LoadScene(1);
    }

    public void ReloadVidoeCallers()
    {
        VideoCallPhotoManager.FolderName = "";
        VideoCallPhotoManager.FolderDate = null;
        GlobalParameters.isSameSession = false;
        SceneManager.sceneLoaded += AfterReload;
        SceneManager.LoadScene(1);
    }

    private void AfterReload(Scene scene, LoadSceneMode mode)
    {
        TestHome.GetInstance.ScreenReload();
        SceneManager.sceneLoaded -= AfterReload;
    }

    public void GoToHomaPage()
    {
        
        //Debug.Log("agors_: GoToHomaPage");
        onGaraje = false;
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        SceneManager.LoadScene(0,LoadSceneMode.Single);
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        TestHome.GetInstance.onLeaveButtonClicked(false);
        VideoCallPhotoManager.FolderName = "";
        VideoCallPhotoManager.FolderDate = null;
        GlobalParameters.isSameSession = false;
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    public void GoToGaragePage()
    {
        //Debug.Log("agors_: GoToHomaPage");
        VideoCallPhotoManager.FolderName = "";
        VideoCallPhotoManager.FolderDate = "";
        GlobalParameters.isSameSession = false;
        onGaraje = true;
        SceneManager.LoadScene(0);
    }

    public void GoToLastPage()
    {
        //Debug.Log("agors_: GoToHomaPage");
        SceneManager.LoadScene(2);
    }

    public void GotoSessions()
    {
        SceneManager.LoadScene("Sesions");
    }

    public void ReloadVideoCall()
    {
        SceneManager.LoadScene(1);
    }

    public void GoToSimpleCamera()
    {
        SceneManager.LoadScene("SimpleCamera");
    }
}
