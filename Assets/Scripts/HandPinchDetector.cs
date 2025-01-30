using UnityEngine;

public class HandPinchDetector : MonoBehaviour
{
    [SerializeField] private HandPointer handPointer;
    [SerializeField] private GameObject ghostToSpawn; 
    [SerializeField] private GameObject windowPrefab; 
    public Material stencilMaterial;
    public EnvironmentSpawner.EnvironmentType activeEnvironmentType;
    
    private GameObject _spawnedGhost; 
    private GameObject effectMeshWall;
    private Vector3 _initialScale;
    private Vector3 _initialHitPoint;

    private bool _hasPinched;
    private bool _isIndexFingerPinching;
    private OVRHand.TrackingConfidence _confidence;

    private void Update()
    {
        CheckPinch(handPointer.pointingHand);
    }

    void CheckPinch(OVRHand hand)
    {
        _isIndexFingerPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        _confidence = hand.GetFingerConfidence(OVRHand.HandFinger.Index);

        // Begin pinching
        if (!_hasPinched && _isIndexFingerPinching && _confidence == OVRHand.TrackingConfidence.High && handPointer.currentTarget)
        {
            _hasPinched = true;
            
            if (Physics.Raycast(handPointer.pointingHand.PointerPose.position, handPointer.pointingHand.PointerPose.forward, out RaycastHit hit, Mathf.Infinity, handPointer.targetLayer))
            {
                _initialHitPoint = hit.point; 
                effectMeshWall = hit.collider.gameObject;
                _spawnedGhost = Instantiate(ghostToSpawn, _initialHitPoint, Quaternion.LookRotation(hit.normal));
                
                _initialScale = _spawnedGhost.transform.localScale;
            }
        }
        // Adjust scaling while pinching
        else if (_hasPinched && _isIndexFingerPinching)
        {
            if (_spawnedGhost != null && Physics.Raycast(handPointer.pointingHand.PointerPose.position, handPointer.pointingHand.PointerPose.forward, out RaycastHit hit, Mathf.Infinity, handPointer.targetLayer))
            {
                Vector3 currentHitPoint = hit.point;
                Vector3 distance = currentHitPoint - _initialHitPoint;
                float horizontalDistance = new Vector3(distance.x, 0, distance.z).magnitude;
                float verticalDistance = distance.y;
                
                _spawnedGhost.transform.position = (_initialHitPoint + currentHitPoint) / 2f;   //midpoint
                _spawnedGhost.transform.localScale = new Vector3(horizontalDistance, verticalDistance, _initialScale.z);
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
        MeshRenderer meshRenderer = replacementObject.GetComponentInChildren<MeshRenderer>();
        meshRenderer.material = stencilMaterial;
        EnvironmentSpawner.Instance.SpawnEnvironment(activeEnvironmentType, effectMeshWall.transform, replacementObject);
        Destroy(_spawnedGhost);
        
        _spawnedGhost = null;
    }
}