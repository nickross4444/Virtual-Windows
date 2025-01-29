using UnityEngine;

public class HandPinchDetector : MonoBehaviour
{
    [SerializeField] private HandPointer handPointer;
    [SerializeField] private GameObject ghostToSpawn; 
    [SerializeField] private GameObject windowPrefab; 
    [SerializeField] private float scalingMultiplier = 8f; 
    
    private GameObject _spawnedGhost; 
    private Vector3 _initialHandPosition; 
    private Vector3 _initialScale;

    private bool _hasPinched;
    private bool _isIndexFingerPinching;
    private float _pinchStrength;
    private OVRHand.TrackingConfidence _confidence;

    private void Update()
    {
        CheckPinch(handPointer.rightHand);
    }

    void CheckPinch(OVRHand hand)
    {
        _pinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        _isIndexFingerPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        _confidence = hand.GetFingerConfidence(OVRHand.HandFinger.Index);

        // Begin pinching
        if (!_hasPinched && _isIndexFingerPinching && _confidence == OVRHand.TrackingConfidence.High && handPointer.currentTarget)
        {
            _hasPinched = true;
            
            if (Physics.Raycast(handPointer.rightHand.PointerPose.position, handPointer.rightHand.PointerPose.forward, out RaycastHit hit, Mathf.Infinity, handPointer.targetLayer))
            {
                Vector3 hitPoint = hit.point; 
                Vector3 hitNormal = hit.normal; 
                
                _spawnedGhost = Instantiate(ghostToSpawn, hitPoint, Quaternion.LookRotation(hitNormal));
                
                _initialHandPosition = hand.transform.position;
                _initialScale = _spawnedGhost.transform.localScale;
            }
        }
        // Adjust scaling while pinching
        else if (_hasPinched && _isIndexFingerPinching)
        {
            if (_spawnedGhost != null)
            {
                Vector3 currentHandPosition = hand.transform.position;
                
                float xDifference = (currentHandPosition.x - _initialHandPosition.x) * scalingMultiplier;
                float yDifference = (currentHandPosition.y - _initialHandPosition.y) * scalingMultiplier;
                
                Vector3 localScaleChange = _spawnedGhost.transform.InverseTransformDirection(new Vector3(xDifference, yDifference, 0));
                
                float newScaleX = _initialScale.x + localScaleChange.x;
                float newScaleY = _initialScale.y + localScaleChange.y;
                
                _spawnedGhost.transform.localScale = new Vector3(newScaleX, newScaleY, _initialScale.z);
            }
        }
        // End pinching
        else if (_hasPinched && !_isIndexFingerPinching)
        {
            _hasPinched = false;

            if (_spawnedGhost != null)
            {
                ReplaceObject();
            }
        }
    }

    private void ReplaceObject()
    {
        GameObject replacementObject = Instantiate(
            windowPrefab,
            _spawnedGhost.transform.position,
            _spawnedGhost.transform.rotation
        );
        
        replacementObject.transform.localScale = _spawnedGhost.transform.localScale;

        Destroy(_spawnedGhost);
        
        _spawnedGhost = null;
    }
}