using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSensor : MonoBehaviour
{
    public Light light;

    public void Start()
    {
        light = GetComponent<Light>();
    }
    public void OnTriggerEnter(Collider other)
    {
        light.enabled = true;
    }
    public void OnTriggerExit(Collider other)
    {
        StartCoroutine(DelayBeforeTurnOff());
    }
    IEnumerator DelayBeforeTurnOff()
    {
        yield return new WaitForSeconds(60);
        light.enabled = false;
    }
}
