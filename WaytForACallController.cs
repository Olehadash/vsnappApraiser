using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaytForACallController : MonoBehaviour
{
    #region Singlton
    private static WaytForACallController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool isNullreference
    {
        get
        {
            return instance == null;
        }
    }

    public static WaytForACallController GetInstance
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

    #region SErializable Fields
    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private Text text;
    [SerializeField]
    private GameObject poup;
    [SerializeField]
    private GameObject isBusypopup;
    #endregion

    private CallMessage cmsg;


    public void setCallMessage(CallMessage _cmsg)
    {
        this.cmsg = _cmsg;
    }
    public void Activate()
    {
        parent.SetActive(true);
        poup.SetActive(false);
        text.gameObject.SetActive(true);

        string jline = JsonUtility.ToJson(this.cmsg);

        WEbSocketController.GetInstance.SendMessage(jline);
        WEbSocketController.GetInstance.SendMessage(jline);

        StartCoroutine("WaytCorutine");
    }

    public void DisActive()
    {
        parent.SetActive(false);
        SoundController.GetInstance.StopAll();
        StopCoroutine("WaytCorutine");
        CallMessage msg = new CallMessage();
        msg = cmsg;
        msg.comand = "stop";
        string jline = JsonUtility.ToJson(msg);

        WEbSocketController.GetInstance.SendMessage(jline);
    }

    IEnumerator WaytCorutine()
    {
        int time = 30;
        SoundController.GetInstance.playStartSound();
        while (time > 0)
        {
            text.text = time.ToString();
            yield return new WaitForSeconds(1);
            time--;
        }
        SoundController.GetInstance.StopAll();

        CallMessage msg = new CallMessage();
        msg = cmsg;
        msg.comand = "stop";
        string jline = JsonUtility.ToJson(msg);

        WEbSocketController.GetInstance.SendMessage(jline);

        poup.SetActive(true);
        text.gameObject.SetActive(false);
    }

    public void SetBusy()
    {
        if (isNullreference) return;
        StartCoroutine("WaytBeforeDrop");
        StopCoroutine("WaytCorutine");
    }
    IEnumerator WaytBeforeDrop()
    {
        SoundController.GetInstance.StopAll();
        SoundController.GetInstance.playBisy();
        parent.SetActive(true);
        isBusypopup.SetActive(true);
        yield return new WaitForSeconds(1);
        
        text.gameObject.SetActive(true);
        
    }
}
