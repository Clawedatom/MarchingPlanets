using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    private PlanetMeshGenerator meshGenerator;
    private PlanetBody body;

    [SerializeField] private PlanetData planetData;

    public PlanetData PlanetData => planetData;
    public PlanetBody Body => body;

    public bool IsInitialized { get; private set; }

    private void Awake()
    {
        meshGenerator = GetComponent<PlanetMeshGenerator>();
        body = GetComponent<PlanetBody>();

        if (meshGenerator == null)
        {
            Debug.LogError("PlanetManager: PlanetMeshGenerator is missing.", this);
            return;
        }

        if (body == null)
        {
            Debug.LogError("PlanetManager: PlanetBody is missing.", this);
            return;
        }
    }

    private void Start()
    {
        GeneratePlanet();
    }

    private void Update()
    {
        if (!IsInitialized)
            return;

        meshGenerator.OnUpdate();
    }

    private void GeneratePlanet()
    {
        if (meshGenerator == null)
            return;

        meshGenerator.GeneratePlanetMesh();

        IsInitialized = true;
    }
}