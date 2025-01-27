using UnityEngine;

public class EnvironmentSpawner : MonoBehaviour
{
    public enum EnvironmentType
    {
        Forest,
        Space,
        Water
    }
    public struct SpawnZone
    {
        public Vector3 position;
        public Vector3 size;
        public Quaternion rotation;
    }
    public static EnvironmentSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnEnvironment(EnvironmentType environmentType, SpawnZone spawnZone)
    {
        // TODO: Implement environment spawning
    }
}