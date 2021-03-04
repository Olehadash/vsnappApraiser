using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
#if (UNITY_2018_3_OR_NEWER)
using UnityEngine.Android;
#endif

public class RecallController : MonoBehaviour
{
    #region Singlton
    private static RecallController instanse = null;
    private static bool isNullInstance
    {
        get
        {
            return (instanse == null) ;

        }
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
        instanse = null;
    }
    private void Awake()
    {
        if (instanse == null)
        {
            instanse = this;
        }
    }

    public static RecallController GetInstanse
    {
        get
        {
            return instanse;
        }
    }
    #endregion
    #region Serialize Field
    [SerializeField]
    private GameObject recall;
    [SerializeField]
    private GameObject msg;
    [SerializeField]
    private GameObject waytforConnect;
    [SerializeField]
    private GameObject RemoteObject;
    [SerializeField]
    private GameObject LocalObject;
    #endregion
    private int timeForCalling = 5;

    public void Recall()
    {
        if (isNullInstance) return; 
        recall.SetActive(false);
        StartCoroutine("WaitForCalling");
    }

    public void SetWaytDialog()
    {
        RemoteObject.SetActive(false);
        waytforConnect.SetActive(true);
        msg.GetComponent<Text>().text = timeForCalling.ToString();
        StartCoroutine("WaitForCalling");

    }

    public void StopWaytingDialog()
    {
        if (isNullInstance) return;
        waytforConnect.SetActive(false);
        RemoteObject.SetActive(true);
        StopCoroutine("WaitForCalling");
    }


    IEnumerator WaitForCalling()
    {
        timeForCalling = 5;

        msg.GetComponent<Text>().text = "Waiting for connect... " + timeForCalling.ToString();
        while (timeForCalling > 0)
        {
            yield return new WaitForSeconds(1);
            timeForCalling--;
            msg.GetComponent<Text>().text = "Waiting for connect... " + timeForCalling.ToString();

        }
        recall.SetActive(true);

        TestHome.GetInstance.onLeaveButtonClicked(false);

    }
    #region Getters
    public GameObject GetRemote
    {
        get
        {
            return RemoteObject;
        }
    }

    public GameObject GetLocal
    {
        get{ return LocalObject; }
    }
    #endregion
}
