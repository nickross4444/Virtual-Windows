using UnityEngine;

public class EnvironmentSpawner : MonoBehaviour
{
    public enum EnvironmentType
    {
        Forest,
        Space,
        Water
    }
    [System.Serializable]
    public class EnvironmentPrefabMapping
    {
        public EnvironmentType type;
        public GameObject prefab;
    }

    [SerializeField] private EnvironmentPrefabMapping[] environmentMappings;
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
        var mapping = System.Array.Find(environmentMappings, m => m.type == environmentType);
        if (mapping == null || mapping.prefab == null)
        {
            Debug.LogError($"No prefab found for environment type: {environmentType}");
            return;
        }
        Vector3 rotation = spawnZone.rotation.eulerAngles;
        rotation.y += 180f;     //face toward the player, not away
        spawnZone.rotation = Quaternion.Euler(rotation);
        Instantiate(mapping.prefab, spawnZone.position, spawnZone.rotation);
    }
}