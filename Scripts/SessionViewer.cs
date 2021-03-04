using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionViewer : MonoBehaviour
{
    #region Serializable Feld
    [SerializeField]
    private TextMeshProUGUI numberLabel, dataLabel, garageLabel;
    #endregion

    private PhotoSession.PhotoFolder folder;

    public void Setdata(PhotoSession.PhotoFolder f)
    {
        folder = f;
        numberLabel.text = f.folderName;
        dataLabel.text = f.data;
        garageLabel.text = f.garageName;
    }

    public void onChouse()
    {
        GallryViewCotroller.folder = this.folder;
        SceneManager.LoadScene("Folder");
    }
}
