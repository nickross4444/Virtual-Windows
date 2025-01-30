using System;
using UnityEngine;

public class HandPointer : MonoBehaviour
{
    public OVRHand rightHand;
    public GameObject currentTarget {get; private set;}
    public LayerMask targetLayer;

    [SerializeField] private bool showRaycast = true;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private LineRenderer lineRenderer;

    private Color _originalColor;
    private Renderer _currentRenderer;

    private void Update()
    {
        CheckHandPointer(rightHand);
    }

    private void CheckHandPointer(OVRHand hand)
    {
        if (Physics.Raycast(hand.PointerPose.position, hand.PointerPose.forward, out RaycastHit hit, Mathf.Infinity,
                targetLayer))
        {
            if (currentTarget != hit.collider.gameObject)
            {
                currentTarget = hit.collider.gameObject;
            }
            
            UpdateVisualization(hand.PointerPose.position, hit.point, true);
        }
        else
        {
            if (_currentRenderer != null)
            {
                currentTarget = null;
            }
            
            UpdateVisualization(hand.PointerPose.position, hand.PointerPose.position + hand.PointerPose.forward * 1000, false);
        }
    }

    private void UpdateVisualization(Vector3 startPosition, Vector3 endPosition, bool hitSomething)
    {
        if (showRaycast && lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
            //lineRenderer.material.color = hitSomething ? Color.green : Color.red;
        }
        else if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
