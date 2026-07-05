using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RandomStackMaterials : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] prefabs;

    [Header("Spawn Settings")]
    public int spawnCount = 10;
    public bool spawnOnStart = true;
    // If true, the spawner will also run when the component is enabled/loaded.
    public bool spawnOnEnable = true;
    public bool clearExistingChildren = false;
    public bool useColliderBounds = false;
    public Vector3 areaSize = Vector3.one;
    public bool useLocalSpace = true;

    [Header("Transform Options")]
    public bool applyRotation = true;
    public bool applyUniformRotation = false;
    public Vector3 rotationMin = Vector3.zero;
    public Vector3 rotationMax = Vector3.zero;

    public bool applyUniformScale = false;
    public Vector3 scaleMin = Vector3.one;
    public Vector3 scaleMax = Vector3.one;

    [Header("Other")]
    public int randomSeed = 0; // 0 = no seed

    bool hasSpawned = false;

    void Start()
    {
        if (spawnOnStart && !hasSpawned)
            Spawn();
    }

    void Awake()
    {
        // ensure spawn on load if requested
        if (spawnOnStart && !hasSpawned)
            Spawn();
    }

    void OnEnable()
    {
        if (spawnOnEnable && !hasSpawned)
            Spawn();
    }

    [ContextMenu("Spawn Random Objects")]
    public void Spawn()
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("RandomStackMaterials: no prefabs assigned.");
            return;
        }

        if (randomSeed != 0)
            Random.InitState(randomSeed);

        if (clearExistingChildren)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        for (int n = 0; n < spawnCount; n++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null)
                continue;

            GameObject go = Instantiate(prefab, transform);

            // Position
            if (useColliderBounds && TryGetComponent<Collider>(out var col))
            {
                var b = col.bounds;
                float x = Random.Range(b.min.x, b.max.x);
                float y = Random.Range(b.min.y, b.max.y);
                float z = Random.Range(b.min.z, b.max.z);
                go.transform.position = new Vector3(x, y, z);
            }
            else
            {
                Vector3 half = Vector3.Scale(areaSize, useLocalSpace ? transform.localScale : Vector3.one) * 0.5f;
                Vector3 localPos = new Vector3(
                    Random.Range(-half.x, half.x),
                    Random.Range(-half.y, half.y),
                    Random.Range(-half.z, half.z)
                );

                if (useLocalSpace)
                    go.transform.localPosition = localPos;
                else
                    go.transform.position = transform.position + localPos;
            }

            // Rotation
            if (applyRotation)
            {
                if (applyUniformRotation)
                {
                    float r = Random.Range(rotationMin.x, rotationMax.x);
                    go.transform.localRotation = Quaternion.Euler(r, r, r);
                }
                else
                {
                    float rx = Random.Range(rotationMin.x, rotationMax.x);
                    float ry = Random.Range(rotationMin.y, rotationMax.y);
                    float rz = Random.Range(rotationMin.z, rotationMax.z);
                    go.transform.localRotation = Quaternion.Euler(rx, ry, rz);
                }
            }

            // Scale
            if (applyUniformScale)
            {
                float s = Random.Range(scaleMin.x, scaleMax.x);
                go.transform.localScale = new Vector3(s, s, s);
            }
            else
            {
                float sx = Random.Range(scaleMin.x, scaleMax.x);
                float sy = Random.Range(scaleMin.y, scaleMax.y);
                float sz = Random.Range(scaleMin.z, scaleMax.z);
                go.transform.localScale = new Vector3(sx, sy, sz);
            }
        }
        hasSpawned = true;
    }
}
