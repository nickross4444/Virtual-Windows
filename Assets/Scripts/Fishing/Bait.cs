using UnityEditorInternal.VersionControl;
using UnityEngine;

public class Bait : MonoBehaviour
{
    public ParticleSystem ripple;
    public ParticleSystem congrats;
    public ParticleSystem failure;
    [SerializeField] private GameObject hook;
    public GameObject[] fishes;
    [HideInInspector] public bool inWater = false;
    [HideInInspector] public bool baitedFish = false;
    [HideInInspector] public bool caughtFish = false;
    private Transform rippleEffectPosition;

    private float timer = 0f;
    private float rippleTimer = 0f;
    private float timeToCatchFish;

    Rigidbody rb;
    private int randomFishSpawn;
    void Start()
    {
        rb=GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(baitedFish && !caughtFish)
        {
            if (rb != null)
            {
                Debug.Log("Fish Bait Velocity is Zero");
                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            Debug.Log("Baited Fish");
            rippleTimer += Time.deltaTime;
            if (rippleTimer > 1.5f)
            {
                rippleTimer = 0f;
                ripple.Play();
            }
            timer += Time.deltaTime;
            //Debug.Log("Fish Timer"+timer);
            if(timer >timeToCatchFish)
            {
                Debug.Log("Caught Fish");
                timer = 0f;
                caughtFish=true;
                if (caughtFish)
                {
                    Debug.Log("Fish Caught Congrats");
                    GameObject spawnedFish = Instantiate(fishes[randomFishSpawn]);
                    spawnedFish.transform.parent = hook.transform;
                    spawnedFish.transform.position=hook.transform.position;
                    Destroy(spawnedFish, 3f);
                    congrats.Play();
                }
                
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            Debug.Log(" Fish Water Layer Entered");
            rippleEffectPosition = gameObject.transform;
            ripple.gameObject.transform.position = rippleEffectPosition.position;
            ripple.Play();
            inWater = true;
            
            timeToCatchFish = Random.Range(2,5.5f);
            baitedFish = true;
            randomFishSpawn=Random.Range(0,fishes.Length);
            Debug.Log("timeToCatchFish is:" + timeToCatchFish);
        }
        
    }
   
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            rippleEffectPosition = null;
            //ripple.Play();
            if (!caughtFish && timer < timeToCatchFish)
            {
                Debug.Log("Fishing Rod is Taken at:" + timer);
                Debug.Log("Fishing Failed");
                failure.Play();
            }
            rb.constraints = RigidbodyConstraints.None;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            timer = 0f;
           rippleTimer = 0f;
            
            inWater = false;
            baitedFish=false;
            caughtFish = false;
        }
    }
}
