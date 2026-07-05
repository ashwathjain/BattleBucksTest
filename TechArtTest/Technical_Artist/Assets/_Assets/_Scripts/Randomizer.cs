using System.Collections.Generic;
using UnityEngine;

public class Randomizer : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        [Range(0.1f, 10f)] public float localScaleMultiplyMin = 1f;
        [Range(0.1f, 10f)] public float localScaleMultiplyMax = 1f;
        [Range(-360f, 360f)] public float localRotationMultMin = 0f;
        [Range(-360f, 360f)] public float localRotationMultMax = 0f;
    }

    [Header("Spawn Settings")]
    [SerializeField] private List<SpawnEntry> spawnEntries = new List<SpawnEntry>();
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private int spawnCount = 1;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private float positionOffsetRadius = 0f;

    [Header("Road Safety")]
    [SerializeField] private bool avoidRoad = true;
    [SerializeField] private float roadSafeOffset = 2f;
    [SerializeField] private float roadMinX = -2f;
    [SerializeField] private float roadMaxX = 2f;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnRandomObjects();
        }
    }

    public void SpawnRandomObjects()
    {
        if (spawnEntries == null || spawnEntries.Count == 0)
        {
            Debug.LogWarning("No spawn entries assigned to Randomizer.", this);
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEntry entry = spawnEntries[Random.Range(0, spawnEntries.Count)];

            if (entry.prefab == null)
            {
                continue;
            }

            Vector3 spawnPosition = transform.position;
            if (positionOffsetRadius > 0f)
            {
                spawnPosition += Random.insideUnitSphere * positionOffsetRadius;
                spawnPosition.y = transform.position.y;
            }

            if (avoidRoad)
            {
                spawnPosition.x = Mathf.Clamp(spawnPosition.x, roadMinX, roadMaxX);
                if (Mathf.Abs(spawnPosition.x) < roadSafeOffset)
                {
                    spawnPosition.x = spawnPosition.x > 0f ? roadSafeOffset : -roadSafeOffset;
                }
            }

            GameObject instance = Instantiate(entry.prefab, spawnPosition, transform.rotation, parentTransform);

            float scaleMultiplier = Random.Range(entry.localScaleMultiplyMin, entry.localScaleMultiplyMax);
            instance.transform.localScale = Vector3.one * scaleMultiplier;

            float rotationAmount = Random.Range(entry.localRotationMultMin, entry.localRotationMultMax);
            instance.transform.localRotation *= Quaternion.Euler(0f, rotationAmount, 0f);
        }
    }
}
