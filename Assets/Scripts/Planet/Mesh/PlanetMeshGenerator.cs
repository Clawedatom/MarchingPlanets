using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PlanetMeshGenerator : MonoBehaviour
{
    #region Class References

    #endregion

    #region Private Fields
    //chunk fields
    private Dictionary<Vector3Int, PlanetChunk> chunkIDMap;

    [Header("Generation Fields")]
    [Header("Density Grid Fields")]
    [SerializeField] private int width = 64;
    [SerializeField] private int height = 64;
    [SerializeField] private int depth = 64;
    private Vector3 gridOriginOffset;
    private float[,,] densityGrid;
    [SerializeField] private float isoLevel = 1f;

    [Header("Planet Fields")]
    [SerializeField] private int planetRadius = 10;

    [Header("Chunk Fields")]
    [SerializeField] private GameObject planetChunkPrefab;
    [SerializeField] private Material planetChunkMaterial;

    [SerializeField] private int chunkSize = 16;
    [SerializeField] private float voxelSize = 1f;
    private int planetChunksX;
    private int planetChunksY;
    private int planetChunksZ;

    [Header("Noise / Terrain Settings")]
    [Range(0.015f, 0.08f)][SerializeField] private float noiseScale = 0.05f;
    [Range(3f, 12f)][SerializeField] private float noiseHeight = 5f;
    [Range(3, 6)][SerializeField] private int octaves = 4;
    [Range(0.35f, 0.65f)][SerializeField] private float persistence = 0.5f;
    [Range(1.8f, 2.5f)][SerializeField] private float lacunarity = 2.0f;

    [Header("Seed Settings")]
    [SerializeField] private int seed = 1337;
    [SerializeField] private bool randomizeSeedOnGenerate = false;

    [Header("Randomization Settings")]
    [SerializeField] private bool randomizeNoiseOnGenerate = false;
    [SerializeField] private bool randomizeGradientOnGenerate = false;

    private Vector3 seedOffset;

    [Header("Ocean Settings")]

    [SerializeField] private bool createOcean = true;
    
    [SerializeField] private float seaLevelOffset = 0f;
    [SerializeField] private bool randomizeOceanColour;

    [SerializeField] private Color oceanColour;
    
    private GameObject oceanObject;

    private float minSurfaceRadius;
    private float maxSurfaceRadius;
    [SerializeField] private Gradient planetGradient;

    #endregion

    #region Properties
    public int ChunkSize => chunkSize;
    public float IsoLevel => isoLevel;
    public Material PlanetMat => planetChunkMaterial;
    public Gradient PlanetGradient => planetGradient;
    public float MinSR => minSurfaceRadius;
    public float MaxSR => maxSurfaceRadius;

    public float VoxelSize => voxelSize;
    public float Width => width;
    public float Height => height;
    public float Depth => depth;
    #endregion

    

    

    #region Generation Methods
    public void GeneratePlanetMesh()
    {

        if (randomizeSeedOnGenerate)
        {
            RandomizeSeed();
        }

        if (randomizeNoiseOnGenerate)
        {
            RandomizeNoise();
        }

        if (randomizeGradientOnGenerate)
        {
            RandomizeGradient();
        }

        // Generate pseudo-random offset vectors from seed
        System.Random prng = new System.Random(seed);
        seedOffset = new Vector3(
            prng.Next(-10000, 10000),
            prng.Next(-10000, 10000),
            prng.Next(-10000, 10000)
        );

        ClearExistingChunks();

        //initalize chunk id map 
        chunkIDMap = new Dictionary<Vector3Int, PlanetChunk>();



        //calculate total number of chunks in xyz
        planetChunksX = width / chunkSize;
        planetChunksY = height / chunkSize;
        planetChunksZ = depth / chunkSize;


        //calculate grid origin offset
        gridOriginOffset = new Vector3(-width / 2f, -height / 2f, -depth / 2f);

        // set min and max surface based on noise height offset
        minSurfaceRadius = planetRadius - noiseHeight;
        maxSurfaceRadius = planetRadius + noiseHeight;

        InitalizeDensityGrid();

        GenerateChunks();
        GenerateOcean();
    }

    public void ClearExistingChunks()  // clears ocean too
    {
        int childCount = transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    private void GenerateOcean()
    {
        if (!createOcean) return;

        // Create a smooth standard primitive sphere
        oceanObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        oceanObject.name = "Planet_Ocean";
        oceanObject.transform.SetParent(this.transform);
        oceanObject.transform.localPosition = Vector3.zero;

        // Unity's primitive sphere has a base diameter of 1 unit (radius 0.5)
        // Scale by (planetRadius + seaLevelOffset) * 2 to set true world radius
        float oceanRadius = planetRadius + seaLevelOffset;
        oceanObject.transform.localScale = Vector3.one * (oceanRadius * 2f);



        // Assign blue/water material
        if (randomizeOceanColour)
        {
            RandomizeOceanColour();
        }
        oceanObject.GetComponent<MeshRenderer>().sharedMaterial.color = oceanColour;
    }
    private void InitalizeDensityGrid()
    {
        densityGrid = new float[width + 1, height + 1, depth + 1];
        Vector3 centerOffset = new Vector3(width, height, depth) * 0.5f * voxelSize;

        float dynamicMin = float.MaxValue;
        float dynamicMax = float.MinValue;

        for (int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                for (int z = 0; z <= depth; z++)
                {
                    // World-space sampling position
                    Vector3 localPos = new Vector3(x * voxelSize, y * voxelSize, z * voxelSize) - centerOffset;
                    float distanceFromCentre = localPos.magnitude;

                    // Base spherical shape (0 at surface, positive inside, negative outside)
                    float baseDensity = planetRadius - distanceFromCentre;

                    // Only calculate detail noise near the planet surface to save overhead
                    if (Mathf.Abs(baseDensity) < noiseHeight * 2f)
                    {
                        float noiseValue = GetFractalNoise3D(localPos);
                        baseDensity += noiseValue * noiseHeight;
                    }

                    densityGrid[x, y, z] = baseDensity;

                    if (Mathf.Abs(baseDensity) < isoLevel * 1.5f)
                    {
                        if (distanceFromCentre < dynamicMin) dynamicMin = distanceFromCentre;
                        if (distanceFromCentre > dynamicMax) dynamicMax = distanceFromCentre;

                    }
                }
            }
        }
        minSurfaceRadius = dynamicMin;
        maxSurfaceRadius = dynamicMax;
    }

    private float GetFractalNoise3D(Vector3 position)
    {
        float totalNoise = 0f;
        float frequency = noiseScale;
        float amplitude = 1f;
        float maxAmplitude = 0f; // Used for normalization

        for (int i = 0; i < octaves; i++)
        {
            Vector3 sample = (position + seedOffset) * frequency;

            // Sample 3D noise using three offset 2D Perlin planes
            float xy = Mathf.PerlinNoise(sample.x, sample.y);
            float yz = Mathf.PerlinNoise(sample.y, sample.z);
            float zx = Mathf.PerlinNoise(sample.z, sample.x);

            float yx = Mathf.PerlinNoise(sample.y, sample.x);
            float zy = Mathf.PerlinNoise(sample.z, sample.y);
            float xz = Mathf.PerlinNoise(sample.x, sample.z);

            // Average sampling planes (maps from 0..1 to -1..1 range)
            float currentOctaveNoise = (xy + yz + zx + yx + zy + xz) / 6f;
            currentOctaveNoise = (currentOctaveNoise * 2f) - 1f;

            totalNoise += currentOctaveNoise * amplitude;
            maxAmplitude += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return totalNoise / maxAmplitude; // Returns normalized value between -1 and 1
    }
    #endregion

    #region Chunk Methods
    private void GenerateChunks()
    {
        for (int x = 0; x < planetChunksX; x++)
        {
            for (int y = 0; y < planetChunksY; y++)
            {
                for (int z = 0; z < planetChunksZ; z++)
                {
                    CreateChunk(x, y, z);
                }

            }

        }
    }
    private void CreateChunk(int x, int y, int z)
    {
        if (!planetChunkPrefab)
        {
            Debug.LogError("PlanetMeshGenerator: Planet Chunk Prefab has not been assigned");
            return;
        }

        //Set Up Chunk Values

        Vector3 chunkPos = gridOriginOffset + new Vector3(
            x * chunkSize * voxelSize,
            y * chunkSize * voxelSize,
            z * chunkSize * voxelSize);

        Vector3Int chunkCoords = new Vector3Int(x, y, z);

        //create chunk game object
        GameObject planetChunk = Instantiate(planetChunkPrefab, chunkPos, Quaternion.identity, this.transform);
        planetChunk.name = $"PlanetChunk_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}";
        PlanetChunk chunk = planetChunk.GetComponent<PlanetChunk>();

        chunk.Initialize(this, chunkCoords);
        //continue chunk setup
        RegisterChunk(chunkCoords, chunk);
    }

    private void RegisterChunk(Vector3Int chunkCoord, PlanetChunk chunk)
    {
        chunkIDMap.Add(chunkCoord, chunk);
    }
    #endregion

    #region Randomisation Methods
    private void RandomizeSeed()
    {
        seed = Random.Range(0, int.MaxValue);
    }
    public void RandomizeNoise()
    {
        noiseScale = Random.Range(0.015f, 0.08f);
        noiseHeight = Random.Range(3f, 12f);
        octaves = Random.Range(3, 6);
        persistence = Random.Range(0.35f, 0.65f);
        lacunarity = Random.Range(1.8f, 2.5f);
    }

    public void RandomizeGradient()
    {
        planetGradient = new Gradient();

        // Randomize 4 color stops along terrain height
        GradientColorKey[] colorKeys = new GradientColorKey[4];

        // Deep Valleys / Coast
        colorKeys[0] = new GradientColorKey(Color.HSVToRGB(Random.value, 0.6f, 0.4f), 0.0f);
        // Mid Terrain / Vegetation
        colorKeys[1] = new GradientColorKey(Color.HSVToRGB(Random.value, 0.7f, 0.6f), 0.35f);
        // High Hills / Mountain Rock
        colorKeys[2] = new GradientColorKey(Color.HSVToRGB(Random.value, 0.5f, 0.3f), 0.75f);
        // Mountain Peaks
        colorKeys[3] = new GradientColorKey(Color.HSVToRGB(Random.value, 0.2f, 0.9f), 1.0f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);

        planetGradient.SetKeys(colorKeys, alphaKeys);
    }

    public void RandomizeOceanColour()
    {
        oceanColour = GetVibrantAnyColor();
        oceanObject.GetComponent<MeshRenderer>().sharedMaterial.color = oceanColour;
    }
    Color GetVibrantAnyColor()
    {
        // Hue: 0.0 to 1.0 allows the full spectrum (Red, Orange, Yellow, Green, Cyan, Blue, Purple, Pink)
        float minHue = 0.0f;
        float maxHue = 1.0f;

        // Saturation: 0.75 to 1.0 strictly locks out washed-out pastels, whites, and dull greys
        float minSat = 0.75f;
        float maxSat = 1.0f;

        // Value/Brightness: 0.6 to 0.95 ensures the color stays rich, bright, and never muddy or pitch black
        float minVal = 0.6f;
        float maxVal = 0.95f;

        return Random.ColorHSV(minHue, maxHue, minSat, maxSat, minVal, maxVal);
    }
    public void RandomizeAll()
    {
        RandomizeSeed();
        RandomizeNoise();
        RandomizeGradient();
        RandomizeOceanColour();
    }
    #endregion

    #region Helper Methods
    public float GetDensity(int x, int y, int z)
    {
        if (IsInBounds(x, y, z))
        {
            return densityGrid[x, y, z];
        }
        return 0f;
    }
    public bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x <= width &&
        y >= 0 && y <= height &&
        z >= 0 && z <= depth;

    }
    #endregion
}
