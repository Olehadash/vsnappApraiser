using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GarajeElementView : MonoBehaviour
{
    public TextMeshProUGUI name;
    public TextMeshProUGUI phone;
    public TextMeshProUGUI ofise;
    public TextMeshProUGUI insur;

    public WaytForACallController wayt;

    UserModel model;

    // Start is called before the first frame update
    void Start()
    {
        /*HomePageControll.MODEL.name = this.name.text;
        HomePageControll.MODEL.telephone = this.insur.text;
        HomePageControll.MODEL.ofisephone = this.ofise.text;
        HomePageControll.MODEL.insurName = this.phone.text;*/ 
    }

    public void SetModel(UserModel model)
    {
        this.model = model;
        this.name.text = model.name;
        this.insur.text = model.mobile.ToString();
        this.phone.text = model.login;
        this.ofise.text = model.phone.ToString();
    }

    public void onButtonClick()
    {
        HomePageControll.MODEL = this.model;

        if (HomePageControll.isCalling)
        {
            CallMessage cmsg = new CallMessage();
            cmsg.from = Models.user.user;
            cmsg.to = model.user;
            cmsg.comand = "call";

            Models.cmsg = cmsg;

            wayt.setCallMessage(cmsg);
            wayt.Activate();
        }
        else
        {
            SceneManager.LoadScene("SimpleCamera");
        }
    }

    private void onSucces(WWW www)
    {
        Debug.Log(www.text);
    }

}
