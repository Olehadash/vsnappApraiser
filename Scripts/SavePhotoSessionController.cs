using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGoogleDrive;
using UnityGoogleDrive.Data;
using System.IO;

#region Model
public class PhotoSession
{
    [System.Serializable]
    public struct PhotoFile
    {
        public int id;
        public string name;
    }

    [System.Serializable]
    public class PhotoFolder
    {
        
        public string folderName;
        public string data;
        public string garageName;
        public string modelid;
        public string userid;
        public List<PhotoFile> files = new List<PhotoFile>();
    }

    public List<PhotoFolder> folders;
}
#endregion
public class SavePhotoSessionController : MonoBehaviour
{
    #region Singleton
    private static SavePhotoSessionController instance;
    private static bool isNullInstance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_EDITOR
                System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackTrace(true).GetFrame(1);
                string scriptName = stackFrame.GetFileName();
                int lineNumber = stackFrame.GetFileLineNumber();
                //Debug.LogError(scriptName + " instance not found at line " + lineNumber + " !");
#else
                Debug.LogError("CallingProccessManager instance not found!");
#endif
                return true;
            }
            return false;
        }
    }
    private void Awake()
    {
        datapath = Application.persistentDataPath;
        if (instance == null)
        {
            instance = this;
            Load();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    #region Private Fields
    private PhotoSession session = new PhotoSession();
    private string datapath;
    #endregion
    #region Add Files
    public static void AddFile(string folderName, string fileName)
    {
        if (isNullInstance) return;
        if (!GlobalParameters.isSameSession)
        {
            GlobalParameters.isSameSession = true;
            PhotoSession.PhotoFolder folder = new PhotoSession.PhotoFolder();
            folder.folderName = folderName;
            folder.data = VideoCallPhotoManager.FolderDate;
            folder.garageName = HomePageControll.MODEL.name;
            folder.modelid = HomePageControll.MODEL.id.ToString();
            folder.userid = Models.user.id.ToString();

            if (instance.session.folders == null)
                instance.session.folders = new List<PhotoSession.PhotoFolder>();

            instance.session.folders.Add(folder);
        }
        instance.getFolder(folderName, VideoCallPhotoManager.FolderDate).files.Add(instance.createFile(fileName, folderName));
        instance.SortFolder();
    }

    void SortFolder()
    {
        if (instance.session.folders.Count < 1) return;
        instance.session.folders.Sort(delegate (PhotoSession.PhotoFolder x, PhotoSession.PhotoFolder y)
        {
            //Debug.Log(x.folderName + " : "+ y.folderName);
            if (x.folderName == null && y.folderName == null) return 0;
            else if (x.folderName == null) return -1;
            else if (y.folderName == null) return 1;
            else return x.folderName.CompareTo(y.folderName);
        });
    }
    //private GoogleDriveFiles.CreateRequest request;
    #endregion
    #region Google Drive
    public static void onGoogleDriveLoad()
    {
        if (string.IsNullOrEmpty(VideoCallPhotoManager.FolderName)) return;
        if (VideoCallPhotoManager.GetPhotosCount() == 0) return;
        instance.folderNameGlobal = VideoCallPhotoManager.FolderName;
        instance.folderDateGlobal = VideoCallPhotoManager.FolderDate;
        GoogleDriveFiles.CreateRequest request;
        var folderr = new UnityGoogleDrive.Data.File() { Name = instance.GenerateName(), MimeType = "application/vnd.google-apps.folder" };
        request = GoogleDriveFiles.Create(folderr);
        request.Send().OnDone += instance.putInFolderCorut;
        VideoCallPhotoManager.FolderName = "";
        VideoCallPhotoManager.FolderDate = null;
        GlobalParameters.isSameSession = false;
    }

    string folderNameGlobal, folderDateGlobal;

    string GenerateName()
    {
        //if (string.IsNullOrEmpty(VideoCallPhotoManager.FolderName)) return "";
        //Debug.Log(instance.folderNameGlobal + " : " + VideoCallPhotoManager.GetPhotosCount());
        PhotoSession.PhotoFolder folder = instance.getFolder(instance.folderNameGlobal, instance.folderDateGlobal);
        
        string gId = "",aid = "";
        if(int.Parse(folder.modelid) <10) gId = "000" + folder.modelid;
        else if(int.Parse(folder.modelid) <100) gId = "00" + folder.modelid;
        else if (int.Parse(folder.modelid) < 1000)  gId = "0" + folder.modelid;
        else gId = folder.userid;

        if (Models.user.id < 10)  aid = "000" + Models.user.id.ToString();
        else if (Models.user.id < 100) aid = "00" + Models.user.id.ToString();
        else if (Models.user.id < 1000) aid = "0" + Models.user.id.ToString();
        else aid = Models.user.id.ToString();

        return folder.folderName + "_" + folder.data+ "_"+ gId+"-"+ aid;
    }
    
    private void putInFolderCorut(UnityGoogleDrive.Data.File file)
    {
        StartCoroutine(putInFolder(file));

    }
    private void putInFolderInThread(UnityGoogleDrive.Data.File file)
    {
        GoogleDriveFiles.CreateRequest request;
        PhotoSession.PhotoFolder folder = instance.getFolder(instance.folderNameGlobal, instance.folderDateGlobal);
        string p = Path.Combine(datapath, folder.folderName + "_" + folder.data
                + "_" + folder.modelid + "_" + folder.userid);

        
        foreach (var link in folder.files)
        {
            string path = Path.Combine(p, link.name);
            Sprite sprite = SpriteLoader.GetSpriteFromFileWithCompression(path);
            var filen = new UnityGoogleDrive.Data.File()
            {
                Name = GenerateName() + GetId(link.id + 1),
                Content = sprite.texture.EncodeToPNG(),
                Parents = new List<string> { file.Id }
            };
            request = GoogleDriveFiles.Create(filen);
            request.Fields = new List<string> { "id", "name", "size", "createdTime" };
            request.Send().OnDone += instance.PrintResult;
        }
        instance.folderNameGlobal = "";
    }

    private IEnumerator putInFolder(UnityGoogleDrive.Data.File file)
    {
        GoogleDriveFiles.CreateRequest request;
        PhotoSession.PhotoFolder folder = instance.getFolder(instance.folderNameGlobal, instance.folderDateGlobal);
        string p = Path.Combine(datapath, folder.folderName + "_" + folder.data
                + "_" + folder.modelid + "_" + folder.userid);

        Debug.Log(instance.folderNameGlobal);
        while (folder.files.Count == 0)
        {
            Load();
            folder = instance.getFolder(instance.folderNameGlobal, instance.folderDateGlobal);
            Debug.Log(folder.files.Count);
            yield return null;
        }
        foreach (var link in folder.files)
        {
            string path = Path.Combine(p, link.name);
            Sprite sprite = SpriteLoader.GetSpriteFromFileWithCompression(path);
            var filen = new UnityGoogleDrive.Data.File() { Name = GenerateName()+ GetId(link.id+1), 
                Content = sprite.texture.EncodeToPNG(),
                Parents = new List<string> { file.Id }
            };
            request = GoogleDriveFiles.Create(filen);
            request.Fields = new List<string> { "id", "name", "size", "createdTime" };
            request.Send().OnDone += instance.PrintResult;
        }
        yield return null;
        instance.folderNameGlobal = "";
    }

    private string GetId(int id)
    {
        string gId = "";
        if (id < 10) gId = "000" + id.ToString();
        else if (id < 100) gId = "00" + id.ToString();
        else if (id < 1000) gId = "0" + id.ToString();
        else gId = id.ToString();

        return "_" + gId;
    }




    private string result;
    private void PrintResult(UnityGoogleDrive.Data.File file)
    {
        result = string.Format("Name: {0} Size: {1:0.00}MB Created: {2:dd.MM.yyyy HH:MM:ss}\nID: {3}",
                file.Name,
                file.Size * .000001f,
                file.CreatedTime,
                file.Id);
        Debug.Log("google_ " +  result);
    }
    #endregion
    #region Remove Files
    public void RemoveFile(string fileName)
    {
        if (isNullInstance) return;
        var files = instance.getFolder(VideoCallPhotoManager.FolderName, VideoCallPhotoManager.FolderDate).files;
        for(int i = 0; i< files.Count;i ++)
        {
            if(files[i].name.Equals(fileName))
            {
                files.RemoveAt(i);
                return;
            }
        }
    }

    public static void RemoveFolder(string folderName)
    {
        if (isNullInstance) return;
        if (instance.isFolderExist(folderName))
        {
            instance.session.folders.Remove(instance.getFolder(folderName, VideoCallPhotoManager.FolderDate));
        }
    }
    #endregion
    #region Save Load
    public static void Sawe()
    {
        string json = JsonUtility.ToJson(instance.session);
        Debug.Log("Sawed: "+ json);
        PlayerPrefs.SetString("Session", json);
        
    }

    public static void Load()
    {
        if (!PlayerPrefs.HasKey("Session")) return;
        string json = PlayerPrefs.GetString("Session");
        instance.session = JsonUtility.FromJson<PhotoSession>(json);

    }
    #endregion
    #region Service Methods
    PhotoSession.PhotoFile createFile(string fileName, string folderName)
    {
        PhotoSession.PhotoFile file = new PhotoSession.PhotoFile();
        file.name = fileName;
        file.id = instance.getFolder(folderName, VideoCallPhotoManager.FolderDate).files.Count;
        return file;
    }

    public bool isFolderExist(string folderName)
    {
        if (instance.session.folders == null)
        {
            instance.session.folders = new List<PhotoSession.PhotoFolder>();
            return false;
        }
        for (int i = 0; i < instance.session.folders.Count; i++)
            if (instance.session.folders[i].folderName.Equals(folderName))
                return true;
       return false;
    }

    public PhotoSession.PhotoFolder getFolder(string folderName, string date)
    {
        PhotoSession.PhotoFolder folder = new PhotoSession.PhotoFolder();
        for (int i = 0; i < instance.session.folders.Count; i++)
        {
            if (instance.session.folders[i].folderName.Equals(folderName)
                && instance.session.folders[i].data.Equals(date))
            {
                folder =  instance.session.folders[i];
                break;
            }
        }
        return folder;
    }
    #endregion
    #region Getters
    public static PhotoSession GetSession
    {
        get
        {
            return instance.session;
        }
    }

    public static SavePhotoSessionController GetInstance
    {
        get
        {
            return instance;
        }
    }
    #endregion
}
