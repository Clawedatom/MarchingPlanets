using UnityEngine;

/// <summary>
/// Saved authoring data for one procedural planet. This is the repeatable
/// recipe for a planet, separate from the scene object that generates it.
/// </summary>
[CreateAssetMenu(menuName = "Marching Planets/Planet Data")]
public class PlanetData : ScriptableObject
{
    [Header("Voxel Grid")]
    [SerializeField, Min(1)] private int width = 64;
    [SerializeField, Min(1)] private int height = 64;
    [SerializeField, Min(1)] private int depth = 64;
    [SerializeField, Min(0.01f)] private float voxelSize = 1f;
    [SerializeField] private float isoLevel = 1f;

    [Header("Planet Shape")]
    [SerializeField, Min(1)] private int planetRadius = 10;
    [SerializeField] private bool useRidgedNoise = true;
    [SerializeField, Min(0.01f)] private float elevationExponent = 2f;

    [Header("Chunk Rendering")]
    [SerializeField, Min(1)] private int chunkSize = 16;
    [SerializeField] private GameObject planetChunkPrefab;
    [SerializeField] private Material planetChunkMaterial;

    [Header("Noise")]
    [SerializeField, Min(0.0001f)] private float noiseScale = 0.05f;
    [SerializeField, Min(0f)] private float noiseHeight = 5f;
    [SerializeField, Min(1)] private int octaves = 4;
    [SerializeField, Range(0f, 1f)] private float persistence = 0.5f;
    [SerializeField, Min(0.01f)] private float lacunarity = 2f;

    [Header("Seed")]
    [SerializeField] private int seed = 1337;

    [Header("Ocean")]
    [SerializeField] private bool createOcean = true;
    [SerializeField] private float seaLevelOffset;
    [SerializeField] private Vector2 seaLevelRange = new(-0.6f, 0.2f);
    [SerializeField] private Color oceanColour = Color.blue;

    [Header("Surface Colours")]
    [SerializeField] private Gradient planetGradient = new();

    [Header("Planet Physical Properties")]
    [Header("Planet Gravity")]
    [SerializeField] private float gravityStrength;
    public float GravityStrength => gravityStrength;    

    public int Width => width;
    public int Height => height;
    public int Depth => depth;
    public float VoxelSize => voxelSize;
    public float IsoLevel => isoLevel;
    public int PlanetRadius => planetRadius;
    public bool UseRidgedNoise => useRidgedNoise;
    public float ElevationExponent => elevationExponent;
    public int ChunkSize => chunkSize;
    public GameObject PlanetChunkPrefab => planetChunkPrefab;
    public Material PlanetChunkMaterial => planetChunkMaterial;
    public float NoiseScale => noiseScale;
    public float NoiseHeight => noiseHeight;
    public int Octaves => octaves;
    public float Persistence => persistence;
    public float Lacunarity => lacunarity;
    public int Seed => seed;
    public bool CreateOcean => createOcean;
    public float SeaLevelOffset => seaLevelOffset;
    public Vector2 SeaLevelRange => seaLevelRange;
    public Color OceanColour => oceanColour;
    public Gradient PlanetGradient => planetGradient;

    private void OnValidate()
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        depth = Mathf.Max(1, depth);
        chunkSize = Mathf.Max(1, chunkSize);
        voxelSize = Mathf.Max(0.01f, voxelSize);
        planetRadius = Mathf.Max(1, planetRadius);
        noiseScale = Mathf.Max(0.0001f, noiseScale);
        noiseHeight = Mathf.Max(0f, noiseHeight);
        octaves = Mathf.Max(1, octaves);
        lacunarity = Mathf.Max(0.01f, lacunarity);
        elevationExponent = Mathf.Max(0.01f, elevationExponent);
    }
}
