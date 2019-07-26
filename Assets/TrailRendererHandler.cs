using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailRendererHandler : MonoBehaviour
{
    public GameObject swappingTrail;

    public void TrailRendererOn()
    {
        swappingTrail.SetActive(true);
    }

    public void TrailRendererOff()
    {
        swappingTrail.SetActive(false);
    }

}
