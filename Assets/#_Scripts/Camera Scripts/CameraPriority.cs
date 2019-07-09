using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraPriority : MonoBehaviour
{

    //[SerializeField]
    //public Camera MainCamera;

    [SerializeField]
    public CinemachineVirtualCamera CameraToSwitchTo;


    // Start is called before the first frame update
    void Start()
    {

    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            CameraToSwitchTo.GetComponent<CinemachineVirtualCamera>().Priority = 100;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraToSwitchTo.GetComponent<CinemachineVirtualCamera>().Priority = 0;
        }
    }

}
