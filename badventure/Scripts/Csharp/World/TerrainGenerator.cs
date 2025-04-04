using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using Badventure.Scripts.Csharp.Solution.Dependencies.PerlinNoise;

public partial class TerrainGenerator : Node3D
{
    /// <summary>
    /// Width of the map to be generated.
    /// </summary>
    [Export] public int Width = 100;

    /// <summary>
    /// Height of the map to be generated.
    /// </summary>
    [Export] public int Height = 100;

    /// <summary>
    /// Controls the scale of the noise features. Higher values result in smaller, more detailed patterns, while lower values create larger, smoother features.
    /// </summary>
    [Export] public float GeneratorFrequency = 0.05f;

    /// <summary>
    /// Controls the intensity or height of the noise. Higher values produce more pronounced variations, while lower values result in flatter noise.
    /// </summary>
    [Export] public float GeneratorAmplitude = 1f;

    /// <summary>
    /// Controls how much each octave contributes to the final noise. Lower values reduce the influence of higher octaves, creating smoother noise.
    /// </summary>
    [Export] public float GeneratorPersistence = 0.5f;

    /// <summary>
    /// The number of layers of noise added together. Each octave adds finer details at a higher frequency, increasing the complexity of the noise.
    /// </summary>
    [Export] public int GeneratorOctaves = 6;

    /// <summary>
    /// The seed value for the random number generator. Ensures that the same seed produces the same noise map, allowing for reproducibility.
    /// </summary>
    [Export] public int GeneratorSeed = 12345;

    /// <summary>
    /// A value applied to the noise to modify its distribution. Values greater than 1 reduce low values and emphasize high values, while values less than 1 do the opposite. Useful for adjusting the balance between low and high areas.
    /// </summary>
    [Export] public float GeneratorDistributionPower = 1f;

    /// <summary>  
    /// Height scaling factor for terrain generation. Sets the multiplication factor for the noise height. The larger the value, the taller the mountains.  
    /// </summary>  
    [Export] public float HeightScale = 10.0f;

    /// <summary>  
    /// Falloff strength for smooth height reduction at the edges of the map. Indicates how sharply the height decreases at the edges. Values greater than 1.0 will make the edges less steep, creating a smoother transition. Values less than 1.0 will make the edges steeper.  
    /// </summary>  
    [Export] public float CoastFalloffStrength = 0.5f;


    [Export] public Texture2D Texture;

    private MeshInstance3D TerrainMesh;
    private CollisionShape3D terrainCollision;

    public override void _Ready()
    {
        InitializeScene();
        Generate3DNoiseMap();
    }

    private void InitializeScene()
    {
        TerrainMesh = GetNode<MeshInstance3D>("Terrain");
        terrainCollision = GetNode<CollisionShape3D>("Terrain/StaticBody3D/TerrainCollision");
        AddDirectionalLight();
    }

    private void AddDirectionalLight()
    {
        var light = new DirectionalLight3D
        {
            ShadowEnabled = true,
            ShadowBias = 0.05F,
            ShadowBlur = 1.0F,
            LightColor = new Color(1, 1, 1)
        };
        AddChild(light);
    }

    private void Generate3DNoiseMap()
    {
        float[,] noiseMatrix = GenerateNoiseMatrix();
        float[,] oceanFloorNoise = GenerateOceanFloorNoise(noiseMatrix);

        var (vertices, waterVertices) = GenerateVertices(noiseMatrix, oceanFloorNoise);

        var (terrainMesh, waterMesh) = CreateMeshes(vertices, waterVertices);

        ConfigureTerrainMesh(terrainMesh);

        var waterMeshInstance = CreateWaterMesh(waterMesh);

        SetupTerrainCollision(terrainMesh);

        PositionTerrainAndWater(waterMeshInstance);
    }

    private float[,] GenerateNoiseMatrix()
    {
        float[,] baseNoiseMatrix = PerlinNoise.GetNoiseMatrix(
            Width, Height,
            GeneratorFrequency, GeneratorAmplitude,
            GeneratorPersistence, GeneratorOctaves,
            GeneratorSeed, GeneratorDistributionPower
        );

        return ApplyCoastFalloff(baseNoiseMatrix);
    }

    private float[,] ApplyCoastFalloff(float[,] noiseMatrix)
    {
        float[,] falloffMatrix = new float[Width, Height];

        float centerX = Width / 2f;
        float centerZ = Height / 2f;

        float maxRadius = MathF.Sqrt(centerX * centerX + centerZ * centerZ);

        Parallel.For(0, Width, x =>
        {
            for (int z = 0; z < Height; z++)
            {
                float distanceX = Math.Abs(x - centerX);
                float distanceZ = Math.Abs(z - centerZ);
                float distance = MathF.Sqrt(distanceX * distanceX + distanceZ * distanceZ);

                float normalizedDistance = distance / maxRadius;

                float falloff = MathF.Pow(normalizedDistance, CoastFalloffStrength);

                float originalHeight = noiseMatrix[x, z];
                float adjustedHeight = originalHeight * (1 - falloff);

                falloffMatrix[x, z] = Math.Max(adjustedHeight, 0.1f);
            }
        });

        return falloffMatrix;
    }

    private float[,] GenerateOceanFloorNoise(float[,] coastFalloffMatrix)
    {
        float[,] oceanFloorNoise = PerlinNoise.GetNoiseMatrix(
            Width, Height,
            GeneratorFrequency * 2f,
            HeightScale * 0.5f,
            GeneratorPersistence,
            GeneratorOctaves,
            GeneratorSeed + 1000
        );

        float[,] finalOceanFloor = new float[Width, Height];

        Parallel.For(0, Width, x =>
        {
            for (int z = 0; z < Height; z++)
            {
                float falloffFactor = 1 - coastFalloffMatrix[x, z];
                float oceanFloorHeight = oceanFloorNoise[x, z] * falloffFactor * 5f;
                finalOceanFloor[x, z] = Math.Max(oceanFloorHeight, 0);
            }
        });

        return finalOceanFloor;
    }

    private (ConcurrentBag<(Vector3 vertex, Vector2 uv)> vertices, ConcurrentBag<(Vector3 vertex, Vector2 uv)> waterVertices) GenerateVertices(float[,] noiseMatrix, float[,] oceanFloorNoise)
    {
        var vertices = new ConcurrentBag<(Vector3 vertex, Vector2 uv)>();
        var waterVertices = new ConcurrentBag<(Vector3 vertex, Vector2 uv)>();

        Parallel.For(0, Width, x =>
        {
            for (int z = 0; z < Height; z++)
            {
                float u = (float)x / (Width - 1);
                float v = (float)z / (Height - 1);
                float y = (noiseMatrix[x, z] * HeightScale);

                Vector3 vertex = new Vector3(x, y, z);
                Vector2 uv = new Vector2(u, v);

                vertices.Add((vertex, uv));

                Vector3 waterVertex = new Vector3(x, 20f - oceanFloorNoise[x, z], z);
                waterVertices.Add((waterVertex, uv));
            }
        });

        return (vertices, waterVertices);
    }

    private (ConcurrentBag<(Vector3 vertex, Vector2 uv)> vertices, ConcurrentBag<(Vector3 vertex, Vector2 uv)> waterVertices) GenerateVertices(float[,] noiseMatrix)
    {
        var vertices = new ConcurrentBag<(Vector3 vertex, Vector2 uv)>();
        var waterVertices = new ConcurrentBag<(Vector3 vertex, Vector2 uv)>();

        Parallel.For(0, Width, x =>
        {
            for (int z = 0; z < Height; z++)
            {
                float u = (float)x / (Width - 1);
                float v = (float)z / (Height - 1);
                float y = (noiseMatrix[x, z] * HeightScale);

                Vector3 vertex = new Vector3(x, y, z);
                Vector2 uv = new Vector2(u, v);

                vertices.Add((vertex, uv));

                if (y < 30f)
                {
                    Vector3 waterVertex = new Vector3(x, 30f, z);
                    waterVertices.Add((waterVertex, uv));
                }
            }
        });

        return (vertices, waterVertices);
    }

    private (ArrayMesh terrainMesh, ArrayMesh waterMesh) CreateMeshes(ConcurrentBag<(Vector3 vertex, Vector2 uv)> vertices, ConcurrentBag<(Vector3 vertex, Vector2 uv)> waterVertices)
    {
        var surfaceTool = new SurfaceTool();
        var waterSurfaceTool = new SurfaceTool();

        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        waterSurfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        var vertexArray = vertices.OrderBy(v => v.vertex.X * Height + v.vertex.Z).ToArray();
        var waterVertexArray = waterVertices.OrderBy(v => v.vertex.X * Height + v.vertex.Z).ToArray();

        AddVerticesToSurfaceTool(surfaceTool, vertexArray);
        AddVerticesToSurfaceTool(waterSurfaceTool, waterVertexArray);

        AddIndices(surfaceTool, waterSurfaceTool);

        surfaceTool.GenerateNormals();
        surfaceTool.GenerateTangents();

        return (
            surfaceTool.Commit(),
            waterSurfaceTool.Commit()
        );
    }

    private void AddVerticesToSurfaceTool(
        SurfaceTool surfaceTool,
        (Vector3 vertex, Vector2 uv)[] vertexArray)
    {
        foreach (var (vertex, uv) in vertexArray)
        {
            surfaceTool.SetUV(uv);
            surfaceTool.AddVertex(vertex);
        }
    }

    private void AddIndices(SurfaceTool surfaceTool, SurfaceTool waterSurfaceTool)
    {
        for (int x = 0; x < Width - 1; x++)
        {
            for (int z = 0; z < Height - 1; z++)
            {
                int topLeft = x * Height + z;
                int topRight = (x + 1) * Height + z;
                int bottomLeft = x * Height + (z + 1);
                int bottomRight = (x + 1) * Height + (z + 1);

                AddTriangleIndices(surfaceTool, topLeft, topRight, bottomLeft, bottomRight);
                AddTriangleIndices(waterSurfaceTool, topLeft, topRight, bottomLeft, bottomRight);
            }
        }
    }
    private void AddTriangleIndices(
        SurfaceTool surfaceTool,
        int topLeft, int topRight,
        int bottomLeft, int bottomRight)
    {
        surfaceTool.AddIndex(topLeft);
        surfaceTool.AddIndex(topRight);
        surfaceTool.AddIndex(bottomLeft);

        surfaceTool.AddIndex(topRight);
        surfaceTool.AddIndex(bottomRight);
        surfaceTool.AddIndex(bottomLeft);
    }

    private void ConfigureTerrainMesh(ArrayMesh terrainMesh)
    {
        TerrainMesh.Mesh = terrainMesh;
    }

    private MeshInstance3D CreateWaterMesh(ArrayMesh waterMesh)
    {
        var waterMeshInstance = new MeshInstance3D
        {
            Mesh = waterMesh,
            MaterialOverride = CreateWaterMaterial()
        };

        AddChild(waterMeshInstance);
        return waterMeshInstance;
    }

    private ShaderMaterial CreateWaterMaterial()
    {
        var waterShader = GD.Load<Shader>("res://Shaders/water.gdshader");
        var waterMaterial = new ShaderMaterial
        {
            Shader = waterShader
        };

        ConfigureWaterMaterialTextures(waterMaterial);
        return waterMaterial;
    }

    private void ConfigureWaterMaterialTextures(ShaderMaterial waterMaterial)
    {
        var waterNoise1 = GD.Load<Texture2D>("res://Assets/Textures/noise_texture.png");
        var waterNoise2 = GD.Load<Texture2D>("res://Assets/Textures/noise_texture.png");
        var causticsNoise = GD.Load<Texture2D>("res://Assets/Textures/foam_texture.png");
        var foamNoise = GD.Load<Texture2D>("res://Assets/Textures/foam_texture.png");

        waterMaterial.SetShaderParameter("water_noise_1", waterNoise1);
        waterMaterial.SetShaderParameter("water_noise_2", waterNoise2);
        waterMaterial.SetShaderParameter("caustics_noise", causticsNoise);
        waterMaterial.SetShaderParameter("foam_noise", foamNoise);
        waterMaterial.SetShaderParameter("water_color", new Vector3(0.0f, 0.3f, 0.6f));
    }

    private void SetupTerrainCollision(ArrayMesh terrainMesh)
    {
        var collisionShape = new ConcavePolygonShape3D();
        collisionShape.SetFaces(terrainMesh.GetFaces());
        terrainCollision.Shape = collisionShape;
    }

    private void PositionTerrainAndWater(MeshInstance3D waterMeshInstance)
    {
        TerrainMesh.Position = new Vector3(-(Width / 2), 0, -(Height / 2));
        terrainCollision.Position = new Vector3(terrainCollision.Position.X, 0, terrainCollision.Position.Z);

        waterMeshInstance.Name = "Water";

        var water = GetNode<MeshInstance3D>("Water");
        water.Position = new Vector3(TerrainMesh.Position.X, -10, TerrainMesh.Position.Z);
    }

    public bool IsWithinMapBounds(Vector3 position)
    {
        float centerX = Width / 2f;
        float centerZ = Height / 2f;

        float distanceX = Math.Abs(position.X - centerX);
        float distanceZ = Math.Abs(position.Z - centerZ);
        float distance = MathF.Sqrt(distanceX * distanceX + distanceZ * distanceZ);

        return distance <= (Math.Min(Width, Height) / 2f - 20f);
    }
}
