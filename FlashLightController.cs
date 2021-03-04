using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashLightController : MonoBehaviour
{
    

    [SerializeField]
    private Sprite lightOff;
    [SerializeField]
    private Sprite lightOn;
    [SerializeField]
    private Image button;

    private bool light = false;
    private AndroidJavaObject camera1;

    
    void OnDestroy()
    {
        FL_Stop();
    }

    public void OnOffLight()
    {
        if(light)
        {
            button.sprite = lightOff;
            FL_Stop();
        }
        else
        {
            button.sprite = lightOn;
            FL_Start();
        }
        //WebCamPhotoManager.GetInstance.CamPlay();
    }

    public void SwitchOffLight()
    {
        light = false;
        button.sprite = lightOff;
        FL_Stop();


    }

    void FL_Start()
    {
        AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");

        int camID = 0;
        camera1 = cameraClass.CallStatic<AndroidJavaObject>("open", camID);

        if (camera1 != null)
        {
            AndroidJavaObject cameraParameters = camera1.Call<AndroidJavaObject>("getParameters");
            cameraParameters.Call("setFlashMode", "torch");
            camera1.Call("setParameters", cameraParameters);
            camera1.Call("release");
            camera1.Call("startPreview");
            light = true;
        }
        else
        {
            Debug.LogError("[CameraParametersAndroid] Camera not available");
        }

    }


    void FL_Stop()
    {

        if (camera1 != null)
        {
            camera1.Call("stopPreview");
            camera1.Call("release");
            light = false;
        }
        else
        {
            Debug.LogError("[CameraParametersAndroid] Camera not available");
        }

    }
}
