using UnityEngine;

public class WaterNormalOffsetMover : MonoBehaviour
{
    public float speed = 0.01f;
    private Material material;

    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        float offset = Time.time * speed;
        material.mainTextureOffset = new Vector2(material.mainTextureOffset.x, offset );
    }
}