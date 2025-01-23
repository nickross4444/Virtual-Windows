using UnityEngine;

public class ColorLUT : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private Texture2D _2dColorLUT;
    public Texture2D ColorLUT2D => _2dColorLUT;
    private OVRPassthroughColorLut ovrpcl;
    private OVRPassthroughLayer passthroughLayer;

    void Start()
    {
        OVRCameraRig ovrCameraRig = FindObjectOfType<OVRCameraRig>();
        if (ovrCameraRig == null)
        {
            Debug.LogError("Scene does not contain an OVRCameraRig");
            return;
        }

        passthroughLayer = ovrCameraRig.GetComponent<OVRPassthroughLayer>();
        if (passthroughLayer == null)
        {
            Debug.LogError("OVRCameraRig does not contain an OVRPassthroughLayer component");
            return;
        }

        ovrpcl = new OVRPassthroughColorLut(_2dColorLUT, flipY: false);
        passthroughLayer.SetColorLut(ovrpcl, weight: 1);
    }
}
