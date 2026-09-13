using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    PlanetMeshGenerator meshGenerator;

    private void Awake()
    {
        meshGenerator = GetComponent<PlanetMeshGenerator>();

    }

    private void Start()
    {
        //GeneratePlanet();
    }

    private void GeneratePlanet()
    {
        //generate planet mesh
        meshGenerator.GeneratePlanetMesh();
        //populate planet e.g resources, foliage, rocks
    }

}
