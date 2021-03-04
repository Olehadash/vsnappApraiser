using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO;
using UnityEngine.SceneManagement;

public delegate void AftergetTrueSocketMessage();

public struct CallMessage
{
    public string from;
    public string to;
    public string comand;
}

public class WEbSocketController : MonoBehaviour
{
    #region Singlton
    private static WEbSocketController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private static bool isNullreference
    {
        get
        {
            return instance == null;
        }
    }

    public static WEbSocketController GetInstance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    SocketManager Manager;
    string address = "https://vsnapp.pp.ua/socket.io/";
    public bool isConnect = false;
    //public WaytForACallController wayt;

    Dictionary<string, object> data;

    // Start is called before the first frame update
    void Start()
    {
        var options = new SocketOptions();
        //options.ConnectWith = BestHTTP.SocketIO.Transports.TransportTypes.Polling;
        Manager = new SocketManager(new System.Uri(address), options);
        if(!isConnect)
        {
            Connect();
        }
    }

    public void Connect()
    {
        //webSocket.Open();
        Manager.Socket.On(SocketIOEventTypes.Connect, (s, p, a) =>
        {
            Debug.Log("socketio Connteceted");
            SendMessage("");
            isConnect = true;
        });

        Manager.Socket.On(SocketIOEventTypes.Error, (socket, packet, args) =>
        {
            Debug.Log(string.Format("socketio Error: {0}", args[0].ToString()));
        });

        Manager.Socket.On("message", OnNewMessage);

        Manager.Socket.On(SocketIOEventTypes.Disconnect, (s, p, a) =>
        {
            Debug.Log("socketio DisConnteceted");
        });


    }
    public void Disconnect()
    {
        Manager.Socket.Disconnect();
    }

    void OnNewMessage(Socket socket, Packet packet, params object[] args)
    {
        //addChatMessage();
        //data = args[0] as Dictionary<string, object>;
        string msg = args[0] as string;
        Debug.Log("socketio Geted : " + msg);
        if (string.IsNullOrEmpty(msg)) return;

        CallMessage cmsg = JsonUtility.FromJson<CallMessage>(msg);
        if (cmsg.from.Equals(Models.user.user))
        {
            if (cmsg.comand.Equals("login"))
            {
                //Debug.Log(SystemInfo.deviceUniqueIdentifier + "  :  " + cmsg.to);
                if (!SystemInfo.deviceUniqueIdentifier.Equals(cmsg.to))
                {
                    cmsg.comand = "outlog";

                    SendMessage(JsonUtility.ToJson(cmsg));
                }
            }

            if (cmsg.comand.Equals("outlog"))
            {
                if (SystemInfo.deviceUniqueIdentifier.Equals(cmsg.to))
                {
                    GlobalParameters.isLogined = false;
                    PlayerPrefs.DeleteKey("user");
                    GlobalParameters.IsUserLogined = true;
                    SceneManager.LoadScene(0);
                }
            }
        }
        if (cmsg.to.Equals(Models.user.user))
        {
            

            if (cmsg.comand.Equals("busy"))
            {
                SoundController.GetInstance.playBisy();
                WaytForACallController.GetInstance.SetBusy();
            }
            if (cmsg.comand.Equals("accept"))
            {
                SoundController.GetInstance.StopAll();
                TestHome.GetInstance.onJoinButtonClicked(true);
            }
            if(cmsg.comand.Equals("reject"))
            {
                SoundController.GetInstance.playBisy();
                WaytForACallController.GetInstance.SetBusy();
            }

        }
        
    }

    public void SendMessage(string message)
    {
        if (Manager == null) return;
        //Debug.Log("id = " + message);
        Manager.Socket.Emit("message", message);
    }

    private void OnApplicationQuit()
    {
        Models.cmsg.comand = "stop";
        string json = JsonUtility.ToJson(Models.cmsg);
        SendMessage(json);
        Disconnect();
    }

    private void OnApplicationPause(bool pause)
    {
        if (Manager == null) return;
        if(!pause)
            Connect();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (Manager == null) return;
        if (focus)
            Connect();
    }
}
