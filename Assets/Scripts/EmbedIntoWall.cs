using UnityEngine;

public class EmbedIntoWall : MonoBehaviour
{
    public GameObject objectToThrowPrefab;
    private Rigidbody rb;
    void Start()
    {     
         rb = gameObject.GetComponent<Rigidbody>();      
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer==LayerMask.NameToLayer("Wall")) 
        {
            ContactPoint contact = collision.contacts[0];
            ReplaceWall(collision.gameObject,contact);
           
        }
    }

    void ReplaceWall(GameObject wall,ContactPoint hit)
    {
        Vector3 wallPosition = wall.transform.position;
        Quaternion wallRotation = wall.transform.rotation;
        
        transform.rotation = Quaternion.Euler(0, 0, 0);
        //transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        transform.localScale = Vector3.one;
        Destroy(wall); 
    }
}
