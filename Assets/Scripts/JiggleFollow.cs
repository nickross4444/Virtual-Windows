using Oculus.Interaction.HandGrab;
using UnityEngine;

public class JiggleFollow : MonoBehaviour
{
    [SerializeField][Tooltip("The transform this object will follow")] Transform target;
    HandGrabInteractable handGrabInteractable;
    void Start()
    {
        handGrabInteractable = GetComponent<HandGrabInteractable>();
    }
    void Update()
    {
        if (target && handGrabInteractable && handGrabInteractable.Interactors.Count == 0)        //only move if not grabbed and is grabbable
        {
            //Move this object towards the target
            Vector3 direction = target.position - transform.position;
            float distance = direction.magnitude;

            // Apply force proportional to distance, with some damping
            Vector3 velocity = direction.normalized * (distance * 5f);
            transform.position = Vector3.Lerp(transform.position, transform.position + velocity * Time.deltaTime, 0.8f);
        }
    }
}
