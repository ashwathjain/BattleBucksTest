using UnityEngine;

public class CarAISpawner : MonoBehaviour
{
    [Header("Traffic Car")]
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private float spawnInterval = 3.5f;
    [SerializeField] private float spawnDelayVariance = 1.5f;
    [SerializeField] private float carSpeed = 20f;
    [SerializeField] private float spawnAheadDistance = 120f;
    [SerializeField] private float despawnZ = -60f;
    [SerializeField] private float spawnY = 0f;
    [SerializeField] private float spawnSpread = 40f;
    [SerializeField] private Transform parentTransform;

    [Header("Lane Setup")]
    [SerializeField] private bool usePlayerLanes = true;
    [SerializeField] private int laneCount = 3;
    [SerializeField] private float laneMinX = -3f;
    [SerializeField] private float laneMaxX = 3f;

    private CarController playerCarController;
    private float spawnTimer;

    private void Awake()
    {
        playerCarController = FindObjectOfType<CarController>();
        spawnTimer = GetNextSpawnDelay();
    }

    private void Update()
    {
        if (playerCarController != null && playerCarController.CurrentState != CarController.GameState.Playing)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnCar();
            spawnTimer = GetNextSpawnDelay();
        }
    }

    private void SpawnCar()
    {
        if (carPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position;
        spawnPosition.x = GetRandomLaneX();
        spawnPosition.y = spawnY;
        spawnPosition.z = transform.position.z + spawnAheadDistance + Random.Range(-spawnSpread, spawnSpread);

        GameObject car = Instantiate(carPrefab, spawnPosition, Quaternion.Euler(0f, 180f, 0f), parentTransform);
        CarAIMovement aiMovement = car.GetComponent<CarAIMovement>();
        if (aiMovement == null)
        {
            aiMovement = car.AddComponent<CarAIMovement>();
        }

        aiMovement.Initialize(carSpeed, despawnZ);
    }

    private float GetRandomLaneX()
    {
        if (usePlayerLanes && playerCarController != null)
        {
            int playerLaneCount = Mathf.Max(2, Mathf.RoundToInt(playerCarController.HorizontalStepCount) + 1);
            float left = playerCarController.leftXPosition;
            float right = playerCarController.rightXPosition;
            float step = playerLaneCount > 1 ? (right - left) / (playerLaneCount - 1) : 0f;
            int laneIndex = Random.Range(0, playerLaneCount);
            return left + laneIndex * step;
        }

        if (laneCount < 2)
        {
            laneCount = 2;
        }

        float laneStep = (laneMaxX - laneMinX) / (laneCount - 1);
        int fallbackLaneIndex = Random.Range(0, laneCount);
        return laneMinX + fallbackLaneIndex * laneStep;
    }

    private float GetNextSpawnDelay()
    {
        return Mathf.Max(0.2f, spawnInterval + Random.Range(-spawnDelayVariance, spawnDelayVariance));
    }
}

public class CarAIMovement : MonoBehaviour
{
    private float moveSpeed;
    private float despawnZ;
    private CarController playerCarController;
    private bool hasPassedPlayer = false;

    private void Start()
    {
        playerCarController = FindAnyObjectByType<CarController>();
    }

    public void Initialize(float speed, float despawnDepth)
    {
        moveSpeed = speed;
        despawnZ = despawnDepth;
        EnsureCollisionComponents();
    }

    private void Update()
    {
        transform.position += Vector3.back * moveSpeed * Time.deltaTime;

        if (!hasPassedPlayer && playerCarController != null && transform.position.z < playerCarController.transform.position.z)
        {
            hasPassedPlayer = true;
            playerCarController.OnPassedCar(gameObject);
        }

        if (transform.position.z < despawnZ)
        {
            Destroy(gameObject);
        }
    }

    private void EnsureCollisionComponents()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject other)
    {
        CarController player = other.GetComponent<CarController>();
        if (player == null)
        {
            player = other.GetComponentInParent<CarController>();
        }

        if (player != null)
        {
            player.HandleCrash();
        }
    }
}
