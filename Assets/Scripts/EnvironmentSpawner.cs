using System.Collections.Generic;
using UnityEngine;

public class  EnvironmentSpawner : MonoBehaviour
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
    private List<EnvironmentType> activeEnvironments = new List<EnvironmentType>();
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
    Dictionary<Transform, List<GameObject>> WindowsOnWalls = new Dictionary<Transform, List<GameObject>>();
    public void SpawnEnvironment(EnvironmentType environmentType, Transform spawnTransform, GameObject window = null)
    {
        // Destroy residual windows if stenciling the whole wall:
        if(window != null)
        {
            if (!WindowsOnWalls.ContainsKey(spawnTransform))
            {
                WindowsOnWalls[spawnTransform] = new List<GameObject>();        //create list if it doesn't exist
            }
            WindowsOnWalls[spawnTransform].Add(window);
        }
        else if (WindowsOnWalls.ContainsKey(spawnTransform))
        {
            foreach(GameObject existingWindow in WindowsOnWalls[spawnTransform])
            {
                Destroy(existingWindow);
            }
            WindowsOnWalls.Remove(spawnTransform);
        }
        // Spawn the environment if necessary:
        if(activeEnvironments.Contains(environmentType)) return;
        var mapping = System.Array.Find(environmentMappings, m => m.type == environmentType);
        if (mapping == null || mapping.prefab == null)
        {
            Debug.LogError($"No prefab found for environment type: {environmentType}");
            return;
        }
        Instantiate(mapping.prefab, spawnTransform.position, spawnTransform.rotation);
        activeEnvironments.Add(environmentType);
    }
}