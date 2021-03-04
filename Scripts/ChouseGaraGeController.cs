using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChouseGaraGeController : MonoBehaviour
{
    #region Serializable Fields
    [SerializeField]
    private GameObject container;
    [SerializeField]
    private GameObject element;
    #endregion

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        PhotoSession session = SavePhotoSessionController.GetSession;
        if (session.folders == null) return;
        for (int i = 0; i < session.folders.Count; i++)
        {
            Transform el = Instantiate(element.transform, new Vector3(0, 0, 0), Quaternion.identity);
            el.SetParent(container.transform);
            el.localScale = new Vector3(1, 1, 1);
            el.gameObject.SetActive(true);
            el.gameObject.GetComponent<SessionViewer>().Setdata(session.folders[i]);

        }
    }

    public void BackButtonPress()
    {
        SceneManager.LoadScene("LoginScene");
    }
}
