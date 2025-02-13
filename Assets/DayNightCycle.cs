using UnityEngine;
using System;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField]
    bool useRealTime = false;
    [SerializeField]
    Light sun;
    [SerializeField, Range(0, 24)] float timeOfDay;

    [SerializeField]
    float sunRotationSpeed;

    [Header("LightingPreset")]
    [SerializeField] Gradient skyColor;
    [SerializeField] Gradient equatorColor;
    [SerializeField] Gradient sunColor;



    private void Update()
    {
        if (useRealTime)    
        {
            DateTime now = DateTime.Now;
            timeOfDay = now.Hour + now.Minute / 60f + now.Second / 3600f;
        }
        else
        {
            timeOfDay += Time.deltaTime * sunRotationSpeed;
            if (timeOfDay > 24)
                timeOfDay = 0;
        }
   
        UpdateSunRotation();
        UpdateLighting();
    }


    private void OnValidate()
    {
        UpdateSunRotation();
        UpdateLighting();
    }
    //Update sun rotation
    void UpdateSunRotation() 
    {
        float sunRotation = Mathf.Lerp(-90, 270, timeOfDay / 24);
        sun.transform.rotation = Quaternion.Euler(sunRotation, sun.transform.rotation.y, sun.transform.rotation.z);
    }


    private void UpdateLighting() 
    {
        float timeFraction = timeOfDay / 24;
        RenderSettings.ambientEquatorColor = equatorColor.Evaluate(timeFraction);
        RenderSettings.ambientSkyColor = skyColor.Evaluate(timeFraction);
        sun.color = sunColor.Evaluate(timeFraction);
    }
}
