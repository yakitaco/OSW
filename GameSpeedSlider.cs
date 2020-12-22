using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameSpeedSlider : MonoBehaviour
{

	Slider slider;

	// Use this for initialization
	void Start () {
		slider = GetComponent<Slider>();
	}

    // Start is called before the first frame update
    public void changeGameSpeed(){
    	Time.timeScale = slider.value;
    }

}
