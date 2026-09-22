using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PlanetMeshGenerator : MonoBehaviour
{
    #region Class References

    PlanetManager manager;
    [Header("Planet Data")]
    [Tooltip("Optional saved recipe for this planet. When assigned, its values are applied before generation.")]
    [SerializeField] private bool usePlanetDataOnGenerate = true;
    #endregion

    #region Private Fields

    public bool IsInitialized { get; private set; }
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

    [Header("Mountain / Terrain Shape")]
    [SerializeField] private bool useRidgedNoise = true;
    [Range(1f, 4f)][SerializeField] private float elevationExponent = 2.0f;

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
    [SerializeField] private Vector2 seaLevelRange = new Vector2(-0.6f, 0.2f);
    [SerializeField] private bool randomizeOceanColour;
    [SerializeField] private bool randomizeSeaLevelOnGenerate = false;
    [SerializeField] private Color oceanColour;
    
    private GameObject oceanObject;

    private float minSurfaceRadius;
    private float maxSurfaceRadius;
    [SerializeField] private Gradient planetGradient;

    private float averageSurfaceHeight;
    #endregion

    #region Properties
    public PlanetManager Manager
    {
        get
        {
            if (manager == null)
            {
                manager = GetComponent<PlanetManager>();
            }
            return manager;
        }
    }
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

    public float AverageSurfaceHeight => averageSurfaceHeight;

    public bool HasPlanetData => manager != null && manager.PlanetData != null;
    #endregion



    #region Update Methods
    public void OnUpdate()
    {
        if (chunkIDMap == null)
            return;

        UpdateDirtyChunks();
    }
    #endregion

    #region Generation Methods
    public void GeneratePlanetMesh()
    {
        IsInitialized = false;

        if (usePlanetDataOnGenerate && Manager.PlanetData != null)
        {
            ApplyPlanetData();
        }

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


        IsInitialized = true;
    }

    /// <summary>
    /// Copies the assigned PlanetData asset to this generator. Keep this public
    /// so the custom Inspector can apply a data asset without generating yet.
    /// </summary>
    public void ApplyPlanetData()
    {
        PlanetData planetData = manager.PlanetData;
        if (planetData == null)
        {
            Debug.LogWarning("PlanetMeshGenerator: No PlanetData asset is assigned.", this);
            return;
        }

        width = planetData.Width;
        height = planetData.Height;
        depth = planetData.Depth;
        voxelSize = planetData.VoxelSize;
        isoLevel = planetData.IsoLevel;
        planetRadius = planetData.PlanetRadius;
        chunkSize = planetData.ChunkSize;
        planetChunkPrefab = planetData.PlanetChunkPrefab;
        planetChunkMaterial = planetData.PlanetChunkMaterial;
        noiseScale = planetData.NoiseScale;
        noiseHeight = planetData.NoiseHeight;
        octaves = planetData.Octaves;
        persistence = planetData.Persistence;
        lacunarity = planetData.Lacunarity;
        useRidgedNoise = planetData.UseRidgedNoise;
        elevationExponent = planetData.ElevationExponent;
        seed = planetData.Seed;
        createOcean = planetData.CreateOcean;
        seaLevelOffset = planetData.SeaLevelOffset;
        seaLevelRange = planetData.SeaLevelRange;
        oceanColour = planetData.OceanColour;
        planetGradient = CloneGradient(planetData.PlanetGradient);
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

        double totalSurfaceRadiusSum = 0;
        int surfaceSampleCount = 0;

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


                        totalSurfaceRadiusSum += distanceFromCentre;
                        surfaceSampleCount++;
                    }
                }
            }
        }
        minSurfaceRadius = dynamicMin;
        maxSurfaceRadius = dynamicMax;

        if (surfaceSampleCount > 0)
        {
            averageSurfaceHeight = (float)(totalSurfaceRadiusSum / surfaceSampleCount);
        }
        else
        {
            averageSurfaceHeight = planetRadius; // Fallback
        }
    }

    private static Gradient CloneGradient(Gradient source)
    {
        if (source == null)
            return new Gradient();

        Gradient copy = new Gradient();
        copy.SetKeys(source.colorKeys, source.alphaKeys);
        copy.mode = source.mode;
        return copy;
    }

    private float GetFractalNoise3D(Vector3 position)
    {
        float totalNoise = 0f;
        float frequency = noiseScale;
        float amplitude = 1f;
        float maxAmplitude = 0f;

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

            // Average sampling planes (0.0 to 1.0 range)
            float currentOctaveNoise = (xy + yz + zx + yx + zy + xz) / 6f;

            // Map to -1.0 to 1.0 range
            currentOctaveNoise = (currentOctaveNoise * 2f) - 1f;

            if (useRidgedNoise)
            {
                // Invert the absolute value to create sharp ridges
                currentOctaveNoise = 1f - Mathf.Abs(currentOctaveNoise);
                // Square the ridge to sharpen the peaks and widen the valleys
                currentOctaveNoise *= currentOctaveNoise;
            }

            totalNoise += currentOctaveNoise * amplitude;
            maxAmplitude += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        float normalizedNoise = totalNoise / maxAmplitude;

        // Apply exponent to flatten lowlands and exaggerate high peaks
        // We preserve the sign so deep ocean trenches remain deep instead of flipping upwards
        float sign = Mathf.Sign(normalizedNoise);
        return sign * Mathf.Pow(Mathf.Abs(normalizedNoise), elevationExponent);
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
        
        if (oceanObject == null)
        {
            GenerateOcean();
            return;
        }
        oceanObject.GetComponent<MeshRenderer>().sharedMaterial.color = oceanColour;
    }

    public void RandomizeSeaLevel()
    {
        // Sensible bounds relative to mountain height:
        // -0.6f: Deep oceans, leaving mostly landmasses and lakes
        //  0.2f: High water level, creating island archipelagos
       
        if (randomizeSeaLevelOnGenerate)
            seaLevelOffset = Random.Range(seaLevelRange.x, seaLevelRange.y);
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
        RandomizeSeaLevel();
        RandomizeOceanColour();
    }
    #endregion

    #region Digging

    private void UpdateDirtyChunks()
    {
        foreach (PlanetChunk chunk in chunkIDMap.Values)
        {
            if (!chunk.IsDirty)
                continue;

            chunk.RegenerateMesh();
            chunk.SetIsDirty(false);
        }
    }
    public void ModifyDensity(Vector3Int gridPos, float amount)
    {
        if (!IsInitialized)
            return;

        if (densityGrid == null)
            return;

        if (!IsInBounds(gridPos.x, gridPos.y, gridPos.z))
            return;

        densityGrid[
            gridPos.x,
            gridPos.y,
            gridPos.z
        ] += amount;
    }

    public void MarkChunkDirty(Vector3Int gridPos)
    {
        if (!IsInitialized)
            return;

        if (chunkIDMap == null)
            return;

        if (densityGrid == null)
            return;
        int baseX = Mathf.FloorToInt((float)gridPos.x / chunkSize);
        int baseY = Mathf.FloorToInt((float)gridPos.y / chunkSize);
        int baseZ = Mathf.FloorToInt((float)gridPos.z / chunkSize);

        bool onXBoundary = gridPos.x % chunkSize == 0;
        bool onYBoundary = gridPos.y % chunkSize == 0;
        bool onZBoundary = gridPos.z % chunkSize == 0;

        int xMin = onXBoundary ? -1 : 0;
        int yMin = onYBoundary ? -1 : 0;
        int zMin = onZBoundary ? -1 : 0;

        for (int x = xMin; x <= 0; x++)
        {
            for (int y = yMin; y <= 0; y++)
            {
                for (int z = zMin; z <= 0; z++)
                {
                    Vector3Int chunkCoords = new Vector3Int(
                        baseX + x,
                        baseY + y,
                        baseZ + z
                    );

                    if (chunkIDMap.TryGetValue(chunkCoords, out PlanetChunk chunk))
                    {
                        chunk.SetIsDirty(true);
                    }
                }
            }
        }
    }
    #endregion

    #region Helper Methods
    public Vector3Int WorldPointToGridPoint(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // Convert from world/local units into voxel/grid units
        Vector3 gridPos = (localPos - gridOriginOffset) / voxelSize;

        return Vector3Int.RoundToInt(gridPos);
    }
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
