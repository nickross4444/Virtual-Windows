using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(OVRManager.GetPassthroughCapabilities());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
