using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageListController : MonoBehaviour
{
    [SerializeField]
    private GameObject container;
    [SerializeField]
    private GameObject element;

    List<Transform> elements = new List<Transform>();

    private void Start()
    {
        if (Models.garagesmodel.data != null)
        {
            SetChecks();
        }
        else
        {
            if(PlayerPrefs.HasKey("garage"))
            {
                Models.garagesmodel = JsonUtility.FromJson<GaragesModel>(PlayerPrefs.GetString("garage"));
                SetChecks();
            }
            StartCoroutine("WaytToLoad");
            Debug.Log("garage_data_ start get");

        }
    }

    private IEnumerator WaytToLoad()
    {
        yield return new WaitForSeconds(3);
        ServerController.onSuccessHandler2 += onSuccess;
        ServerController.PostREquest("get_apriser_app", null, false);
    }

    public void onSuccess(WWW www)
    {
        Models.garagesmodel = JsonUtility.FromJson<GaragesModel>(www.text);
        
        Debug.Log("garage_data_"+www.text);
        ServerController.onSuccessHandler2 -= onSuccess;
        if (PlayerPrefs.HasKey("garage"))
            ClaenAll();
        SetChecks();
        
        PlayerPrefs.SetString("garage", www.text);
    }


    private void  SetChecks()
    {
        for (int i = 0; i < Models.garagesmodel.data.Length; i++)
        {
            Transform el = Instantiate(element.transform, new Vector3(0, 0, 0), Quaternion.identity);
            el.SetParent(container.transform);
            el.localScale = new Vector3(1, 1, 1);
            el.gameObject.SetActive(true);
            el.gameObject.GetComponent<GarajeElementView>().SetModel(Models.garagesmodel.data[i]);
            elements.Add(el);
        }
    }

    void ClaenAll()
    {
        for (int i = 0; i < elements.Count; i++)
        {
            Destroy(elements[i]);
        }
    }
}
