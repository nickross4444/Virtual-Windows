using Oculus.Interaction.HandGrab;
using UnityEngine;

public class JiggleFollow : MonoBehaviour
{
    [SerializeField][Tooltip("The transform this object will follow")] Transform target;
    [SerializeField] float followSpeed = 10f;
    [SerializeField] float damping = 0.8f;
    [SerializeField] float torqueSpeed = 10f;
    [SerializeField] float rotationDamping = 0.8f;

    private Vector3 velocity = Vector3.zero;
    private Quaternion angularVelocity = Quaternion.identity;

    HandGrabInteractable handGrabInteractable;

    void Start()
    {
        handGrabInteractable = GetComponent<HandGrabInteractable>();
    }
    void FixedUpdate()
    {
        if (target && handGrabInteractable && handGrabInteractable.Interactors.Count == 0)        //only move if not grabbed and is grabbable
        {
            // Position physics with momentum
            Vector3 direction = target.position - transform.position;
            float distance = direction.magnitude;
            Vector3 acceleration = direction.normalized * (distance * followSpeed);
            velocity += acceleration * Time.fixedDeltaTime;
            transform.position += velocity * Time.fixedDeltaTime;
            velocity = Vector3.Lerp(velocity, Vector3.zero, damping * Time.fixedDeltaTime);

            // Rotation physics
            Quaternion rotationDifference = target.rotation * Quaternion.Inverse(transform.rotation);
            float angle;
            Vector3 axis;
            rotationDifference.ToAngleAxis(out angle, out axis);

            // Avoid super large angles
            if (angle > 180) angle -= 360;

            // Apply torque based on angle difference
            if (Mathf.Abs(angle) > 0.01f)
            {
                angularVelocity = Quaternion.AngleAxis(angle * torqueSpeed * Time.fixedDeltaTime, axis) * angularVelocity;
            }

            // Apply angular velocity with damping
            transform.rotation = transform.rotation * angularVelocity;
            angularVelocity = Quaternion.Slerp(angularVelocity, Quaternion.identity, rotationDamping * Time.fixedDeltaTime);
        }
    }
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
