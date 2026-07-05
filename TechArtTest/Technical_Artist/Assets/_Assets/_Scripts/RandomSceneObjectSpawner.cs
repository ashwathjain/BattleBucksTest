using System.Collections.Generic;
using UnityEngine;

public class RandomSceneObjectSpawner : MonoBehaviour
{
    [Header("Prefab Selection")]
    [Tooltip("Prefabs that can be spawned. The script will pick randomly from this list.")]
    public GameObject[] objectPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("How many objects to create in the scene.")]
    public int spawnCount = 10;

    [Tooltip("Radius around the spawner position where objects can appear on the Z axis.")]
    public float spawnRadius = 10f;

    [Tooltip("Y position for spawned objects.")]
    public float spawnY = 0f;

    [Tooltip("Optional parent transform for spawned objects.")]
    public Transform parentTransform;

    [Header("Lane Settings")]
    [Tooltip("Limit spawned objects inside lane boundaries on the X axis.")]
    public bool restrictToLanes = true;

    [Tooltip("Minimum X position for lane placement.")]
    public float laneMinX = -2f;

    [Tooltip("Maximum X position for lane placement.")]
    public float laneMaxX = 2f;

    [Header("Random Transform")]
    [Tooltip("Minimum local scale for spawned objects.")]
    public Vector3 scaleMin = Vector3.one;

    [Tooltip("Maximum local scale for spawned objects.")]
    public Vector3 scaleMax = Vector3.one;

    [Tooltip("If enabled, objects will be randomly rotated on spawn.")]
    public bool randomRotation = true;

    [Tooltip("If enabled, spawned objects are kept in a list for later use.")]
    public bool storeSpawnedObjects = true;

    [HideInInspector]
    public List<GameObject> spawnedObjects = new List<GameObject>();

    private void Start()
    {
        GenerateRandomObjects();
    }

    public void GenerateRandomObjects()
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0)
        {
            Debug.LogWarning("RandomSceneObjectSpawner: No objectPrefabs assigned.", this);
            return;
        }

        if (spawnCount <= 0)
        {
            Debug.LogWarning("RandomSceneObjectSpawner: spawnCount must be greater than zero.", this);
            return;
        }

        if (storeSpawnedObjects)
        {
            spawnedObjects.Clear();
        }

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
            if (prefab == null)
                continue;

            Vector3 spawnPosition = transform.position;
            spawnPosition.z += Random.Range(-spawnRadius, spawnRadius);
            spawnPosition.y = spawnY;

            if (restrictToLanes)
            {
                spawnPosition.x = Random.Range(laneMinX, laneMaxX);
            }
            else
            {
                spawnPosition.x += Random.Range(-spawnRadius, spawnRadius);
            }

            Quaternion spawnRotation = randomRotation ? Random.rotation : Quaternion.identity;
            GameObject instance = Instantiate(prefab, spawnPosition, spawnRotation, parentTransform != null ? parentTransform : transform);

            Vector3 randomScale = new Vector3(
                Random.Range(scaleMin.x, scaleMax.x),
                Random.Range(scaleMin.y, scaleMax.y),
                Random.Range(scaleMin.z, scaleMax.z)
            );
            instance.transform.localScale = randomScale;

            if (storeSpawnedObjects)
            {
                spawnedObjects.Add(instance);
            }
        }
    }

    public void ClearSpawnedObjects()
    {
        if (spawnedObjects == null || spawnedObjects.Count == 0)
            return;

        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                DestroyImmediate(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();
    }
}
