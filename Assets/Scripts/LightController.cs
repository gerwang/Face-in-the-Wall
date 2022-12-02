using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public GameObject spotLight1;
    public GameObject spotLight2;

    public float intensity1;
    public float intensity2;
    // Start is called before the first frame update
    private Light light1, light2;
    void Awake()
    {
        light1 = spotLight1.GetComponent<Light>();
        light2 = spotLight2.GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TurnLightOn()
    {
        StartCoroutine(LightBlink());
    }
    
    IEnumerator LightBlink()
    {
        LightOn(0.6f);
        yield return new WaitForSeconds(0.1f);
        LightOff();
        yield return new WaitForSeconds(1.0f);

        LightOn(0.8f);
        yield return new WaitForSeconds(0.1f);
        LightOff();
        yield return new WaitForSeconds(1.3f);

        for(int i = 0; i < 4; i++)
        {
            LightOn(0.4f);
            yield return new WaitForSeconds(0.04f);
            LightOff();
            yield return new WaitForSeconds(0.04f);
        }


        LightOn(1.0f);


    }

    void LightOn(float intensityScale)
    {
        Debug.Log("light on");
        light1.intensity = intensity1*intensityScale;
        light2.intensity = intensity2*intensityScale;
    }

    void LightOff()
    {
        Debug.Log("light off");
        light1.intensity = 0;
        light2.intensity = 0;
    }
}
