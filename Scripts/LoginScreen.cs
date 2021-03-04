using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityGoogleDrive;
using UnityGoogleDrive.Data;

public class LoginScreen : MonoBehaviour
{
    #region Serializable fields Fields
    [SerializeField]
    private GameObject radioButon;
    [SerializeField]
    private GameObject radioButon1;
    [SerializeField]
    private Button logInButton;
    [SerializeField]
    private Image logInButtonImg;
    [SerializeField]
    private TextMeshProUGUI logInButtonText;
    [SerializeField]
    private TextMeshProUGUI logininline;

    [Space(10)]
    [SerializeField]
    private InputField loginInputField;
    [SerializeField]
    private InputField passwordInputField;

    [Space(10)]
    [SerializeField]
    private GameObject LoginPage;
    #endregion

    private void Start()
    {
        if(GlobalParameters.isLogined)
        {
            LoginPage.SetActive(false);
        }
        else
        {
            if (SaveModelController.LoadUserData())
                LogIn(false);
        }

    }

    #region ChangeInput Field

    public void SetLogin()
    {
        
        Models.user.user = loginInputField.text;
        logininline.text = loginInputField.text;
        //Debug.Log(Models.user.name);

    }

    public void SetPassword()
    {
        Models.user.password = passwordInputField.text;
    }

    #endregion

    #region Private Fields
    private bool isSaveble = false, isSaveble1 = false;
    #endregion

    #region LogIn
    public void LogIn(bool checkTxt)
    {
        if (checkTxt)
        {
            if (string.IsNullOrEmpty(loginInputField.text))
            {
                StartCoroutine(HighlighInputField(loginInputField.image));
                return;
            }
        }
        /*if (string.IsNullOrEmpty(passwordInputField.text))
        {
            StartCoroutine(HighlighInputField(passwordInputField.image));
            return;
        }*/

        /*if(loginInputField.text.Equals("test") && passwordInputField.text.Equals("test"))
        {
            LoginPage.SetActive(false);
            GlobalParameters.isLogined = true;
        }*/
        ServerController.onSuccessHandler += SuccesLogin;
        WWWForm form = new WWWForm();
        form.AddField("login", Models.user.user);
        form.AddField("password", Models.user.password);

        ServerController.PostREquest("login_garage_app", form, false);
    }

    private void SuccesLogin(WWW www)
    {
        ServerController.onSuccessHandler -= SuccesLogin;
        ServerController.onSuccessHandler = null;
        Models.user = JsonUtility.FromJson<Garage>(www.text);
        Debug.Log(www.text);
        Debug.Log(Models.user.user);
        LoginPage.SetActive(false);
        GlobalParameters.isLogined = true;
        if (isSaveble)
            SaveModelController.SaweUserData();
        if(!PlayerPrefs.HasKey("google"))
        {
            PlayerPrefs.SetInt("google", 0);
            var auth = new UnityGoogleDrive.Data.File() { Name = "authointification"};
            GoogleDriveFiles.Create(auth).Send();
        }
        CallMessage cmsg = new CallMessage();
        cmsg.from = Models.user.user;
        cmsg.to = SystemInfo.deviceUniqueIdentifier;
        cmsg.comand = "login";

        WEbSocketController.GetInstance.SendMessage(JsonUtility.ToJson(cmsg));
    }

    #endregion
    #region Highlight InputField
    private const float alphaHighlighSpeed = 10f;
    private IEnumerator HighlighInputField(Image inputFieldImg)
    {
        while (inputFieldImg.color.a < 1f)
        {
            inputFieldImg.color = new Color(inputFieldImg.color.r,
                 inputFieldImg.color.g, inputFieldImg.color.b,
                 Mathf.MoveTowards(inputFieldImg.color.a, 1f, alphaHighlighSpeed * Time.deltaTime));
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);

        while (inputFieldImg.color.a > 0f)
        {
            inputFieldImg.color = new Color(inputFieldImg.color.r,
                 inputFieldImg.color.g, inputFieldImg.color.b,
                 Mathf.MoveTowards(inputFieldImg.color.a, 0f, alphaHighlighSpeed * Time.deltaTime));
            yield return null;
        }
    }
    #endregion

    #region Buttons Events
    private void SetLoginButtonActive()
    {
        if(isSaveble1)
        {
            logInButtonImg.color = GlobalParameters.selectedButtonColor;
            logInButtonText.color = GlobalParameters.selectedTextColor;
            logInButton.interactable = true;
        }
        else
        {
            logInButtonImg.color = GlobalParameters.unselectedButtonColor;
            logInButtonText.color = GlobalParameters.unselectedTextColor;
            logInButton.interactable = false;
        }
    }

    public void AutologinButton()
    {
        isSaveble = !isSaveble;
        radioButon.SetActive(isSaveble);
        //SetLoginButtonActive();
    }

    public void ACheckMarkButton()
    {
        isSaveble1 = !isSaveble1;
        radioButon1.SetActive(isSaveble1);
        SetLoginButtonActive();
    }
    #endregion

    
}
