using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcquirePinEffect : MonoBehaviour
{
    public Renderer meshRenderer;
    public Material instancedMaterial;

    private float time = 0.0F;
    public float speedOfEffect = 2.0f;
    private float startIntensity = 0.0f;
    private float maxIntensity = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = gameObject.GetComponent<Renderer>();
        meshRenderer.material.SetFloat("_FresnelPowerSlider", startIntensity);

    }
    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(StartFresnel());

          //  StartCoroutine(Reverse());
        }
    }

    public IEnumerator StartFresnel()
    {
        time = 0;
        while (instancedMaterial.GetFloat("_FresnelPowerSlider") != maxIntensity)
        {
            time += (Time.deltaTime / speedOfEffect);
          //  meshRenderer.material.SetFloat("_FresnelPowerSlider", Mathf.Lerp(startIntensity, maxIntensity, time));
            meshRenderer.material.SetFloat("_FresnelPowerSlider", Mathf.PingPong(time, maxIntensity));
            //yield return new WaitForSeconds(1f);
            yield return null;
        }
    }
    //public IEnumerator Reverse()
    //{
    //    yield return new WaitForSeconds(1f);
    //    time = 0;
    //    while (instancedMaterial.GetFloat("_FresnelPowerSlider") != startIntensity)
    //    {
    //        time += (Time.deltaTime / speedOfEffect);
    //        meshRenderer.material.SetFloat("_FresnelPowerSlider", Mathf.Lerp(maxIntensity, startIntensity, time));
    //        yield return null;
    //    }

    //}
}
