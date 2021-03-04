using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class HardwareButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKeyDown(KeyCode.Escape))
        {
			if(SceneManager.GetActiveScene().buildIndex == 1)
			{
				SceneManager.LoadScene (0);
			}
            
        }
	}
}
