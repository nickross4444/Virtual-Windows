using Oculus.Interaction.HandGrab;
using UnityEngine;

public class JiggleFollow : MonoBehaviour
{
    [SerializeField][Tooltip("The transform this object will follow")] Transform target;
    [SerializeField] float followSpeed = 30f;
    [SerializeField] float damping = 6f;
    [SerializeField] float rotationSpeed = 10f;

    private Vector3 velocity = Vector3.zero;

    HandGrabInteractable handGrabInteractable;

    void Start()
    {
        handGrabInteractable = GetComponent<HandGrabInteractable>();
    }
    void FixedUpdate()
    {
        if (target && (!handGrabInteractable || handGrabInteractable.Interactors.Count == 0))        //only move if not grabbed and is grabbable
        {
            // Position physics with momentum
            Vector3 direction = target.position - transform.position;
            float distance = direction.magnitude;
            Vector3 acceleration = direction.normalized * (distance * followSpeed);
            velocity += acceleration * Time.fixedDeltaTime;
            transform.position += velocity * Time.fixedDeltaTime;
            velocity = Vector3.Lerp(velocity, Vector3.zero, damping * Time.fixedDeltaTime);

            // Simple rotation following
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
