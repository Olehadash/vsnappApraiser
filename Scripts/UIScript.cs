using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

public class UIScript : MonoBehaviour {
    
	// Use this for initialization
	public void AudioCall () {
		SceneManager.LoadScene (3);
	}
	
	public void VideoCall () {
		SceneManager.LoadScene (1);
	}
}
