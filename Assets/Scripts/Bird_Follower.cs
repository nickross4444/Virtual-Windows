using UnityEngine;

public class Bird_Follower : MonoBehaviour
{
    [SerializeField] private Transform spawn_Pos;
    private bool canSpawn = false;
    private lb_BirdController birdController;
    [SerializeField] private float birdSpawnTimer=10f;

    private float timer = 0f;
    void Start()
    {
        birdController = FindFirstObjectByType<lb_BirdController>();
    }
    public void Bird_Perch()
    {
        gameObject.transform.position = spawn_Pos.position;
        //gameObject.transform.rotation = spawn_Pos.rotation;
        
    }
    public void spawnCondition()
    {
        canSpawn = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(canSpawn)
        {
            birdController.enabled = true;
            timer += Time.deltaTime;
            if(timer>birdSpawnTimer)
            {
                timer = 0f;
                birdController.SpawnAmount(1);
            }
        }
        else
        {
            birdController.enabled = false;
        }
    }
}
