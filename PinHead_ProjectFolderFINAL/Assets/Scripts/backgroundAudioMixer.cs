using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backgroundAudioMixer : MonoBehaviour {

    public AudioSource pinheadTheme;
    public AudioSource spindleTheme;
    public AudioSource rebutiaTheme;
    public AudioSource claydoughTheme;
    AudioSource currentSource;

    private void Start()
    {
        pinheadTheme.volume = 1f;
        currentSource = pinheadTheme;
        spindleTheme.volume = 0f;
        rebutiaTheme.volume = 0f;
        claydoughTheme.volume = 0f;

    }
    // Update is called once per frame
    void Update () {
		
	}
    public void SetTrack(FormScript.Form form)
    {
        currentSource.volume = 0f;
        switch (form)
        {
            case FormScript.Form.Regular:
                break;
            case FormScript.Form.Yarn:
                currentSource = spindleTheme;
                break;
            case FormScript.Form.Pin:
                currentSource = pinheadTheme;
                break;
            case FormScript.Form.Roll:
                currentSource = rebutiaTheme;
                break;
            case FormScript.Form.Heavy:
                currentSource = claydoughTheme;
                break;
        }
        currentSource.volume = 1f;
    }
}