using UnityEngine;

public class FishingLine : MonoBehaviour
{
    public Transform rodTip;  // The tip of the fishing rod
    public Transform lineEnd; // The end of the fishing line (attached to hook or bait)
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;  // Set two points for the line (start and end)
    }
    

    void Update()
    {
        // Update the position of the line renderer to simulate the fishing line
        lineRenderer.SetPosition(0, rodTip.position);  // Start of line (fishing rod tip)
        lineRenderer.SetPosition(1, lineEnd.position); // End of line (attached to hook/bait)
    }
}
