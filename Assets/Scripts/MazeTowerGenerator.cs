using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class MazeTowerGenerator : MonoBehaviour
{
    // Grid
    Grid Grid { get; set; }
    // TileMaps
    [field: SerializeField] public Tilemap GroundTileMap { get; private set; }
    [field: SerializeField] public Tilemap PathTileMap { get; private set; }
    [field: SerializeField] public Tilemap SectionTileMap { get; private set; }

    // Tiles
    [field: SerializeField] public Tile GroundTile { get; private set; }
    [field: SerializeField] public Tile FinishTile { get; private set; }
    [field: SerializeField] public Tile SectionTile { get; private set; }
    [field: SerializeField]
    public Color[] SectionColors { get; private set; } = new Color[] {
        Color.white,
        Color.blue,
        Color.red,
        Color.gray,
    };
    //[field: SerializeField] public PathTile PathTile { get; private set; }
    [field: SerializeField] public MazeTile MazeTile { get; private set; }
    // Floor Properties
    [field: SerializeField] int TowerFloor { get; set; }
    [field: SerializeField] int MaxSection { get; set; }
    [field: SerializeField] int FloorWidth { get; set; } = 20;
    [field: SerializeField] int FloorHeight { get; set; } = 20;
    [field: SerializeField] int FloorOffset { get; set; } = 1;
    // Floor List
    [field: SerializeField] List<Floor> floors;
    public IReadOnlyList<Floor> Floors => floors;
    public int GetFloorIndex(Floor floor) => floors.IndexOf(floor);

    // prefab
    [SerializeField] public Transform stairParent;
    [SerializeField] public GameObject stairPrefab;

    // portal
    [SerializeField] public Sprite sameFloorPortalSprite;
    private void Start()
    {
        Grid = FindFirstObjectByType<Grid>();
    }
   
    void ClearTileMaps()
    {
        GroundTileMap.ClearAllTiles();
        SectionTileMap.ClearAllTiles();
        PathTileMap.ClearAllTiles();
    }

    public void PlaceGroundFloor()
    {
        ClearTileMaps();
        floors.Clear();

        for (int i = 0; i < TowerFloor; i++)
        {
            var floor = new Floor(this, new RectInt(i * (FloorWidth + FloorOffset), 0, FloorWidth, FloorHeight), MaxSection);
            floors.Add(floor);

            foreach (var rectPos in floor.FloorRect.allPositionsWithin)
            {
                Vector3Int cellPos = (Vector3Int) rectPos;

                // Ground
                GroundTileMap.SetTile(cellPos, GroundTile);

                // Path
                if (floor.TryGetCellData(rectPos, out CellData cellData))
                {
                    PathTileMap.SetTile(cellPos, MazeTile.GetTile(cellData.connection));
                }

            }
        }

        RDFSsection();

        foreach (var floor in Floors)
        {
            foreach (var section in floor.Sections)
            {
                foreach (var portalData in section.Portals)
                {
                    var clone = PortalObjectPool.Instance.GetObject(Grid.GetCellCenterWorld((Vector3Int)floor.LocalToWorldPos(portalData.FromLocalPos)));
                    if (clone == null)
                        return;

                    if(clone.TryGetComponent(out Portal portal))
                    {
                        portal.SetDestination(portalData.ToFloor.LocalToWorldPos(portalData.ToLocalPos));
                        if (portalData.FromFloor == portalData.ToFloor)
                        {
                            if (clone.TryGetComponent(out SpriteRenderer renderer))
                            {
                                renderer.sprite = sameFloorPortalSprite;
                            }
                        }
                    }

                }
            }
        }
       
        // section
        foreach (var floor in Floors)
        {
            foreach(var rectPos in floor.FloorRect.allPositionsWithin)
            {
                Color tileColor = Color.black;

                var sectionIdx = floor.GetLocalSectionIdx(rectPos);
                if (sectionIdx != -1)
                {
                    if (sectionIdx < SectionColors.Length)
                    {
                        tileColor = SectionColors[sectionIdx];
                    }

                }

                Tile tile = SectionTile;
                tile.color = tileColor;
                SectionTileMap.SetTile((Vector3Int)rectPos, tile);
            }
        }

        PathTileMap.RefreshAllTiles();
        Debug.Log("*----*");
        DebugPhysicsShapeCount();
        Debug.Log("*----*");
    }

    private void Update()
    {
        DebugPhysicsShapeCount();
    }

    public void DebugPhysicsShapeCount()
    {
        if (PathTileMap.TryGetComponent(out CompositeCollider2D compositeCollider))
        {
            compositeCollider.GenerateGeometry();
            var n = compositeCollider.shapeCount;
            Debug.Log($"tilemap physicsShape (n) = {n}");
        }
    }

    void GenerateShadowCaster2D()
    {

        if(PathTileMap.TryGetComponent(out ShadowCaster2DCreator creator))
        {
            creator.AltCreate();
        }
        else
        {
            Debug.Log("not found.");
        }
    }

    bool IsAllConnect()
    {
        HashSet<Section> queued = new() { floors[0].Sections[0]};
        HashSet<Section> visited = new();

        while(queued.Count > 0)
        {
            var section = queued.Last();
            queued.Remove(section);
            visited.Add(section);

            // add other section that connect to this section to queued
        }

        if (visited.Count == floors.SelectMany(floor => floor.Sections).Count())
            return true;
        return false;
    }

    void RDFSsection()
    {
        //random pick one section
        var allSection = Floors.SelectMany(floor => floor.Sections).ToList();
        var startSection = allSection[Random.Range(0,allSection.Count)];
        var connected = new HashSet<Section>() { startSection };

        var count = 0;
        while (allSection.Count != connected.Count && count < 500)
        {
            //Debug.Log($"{connected.Count} / {allSection.Count}");
            count++;
            
            var randSections = connected.Where(section => section.UnuseOneWayCells.Count > 0).ToList();
            var otherSections = allSection.Where(section => section.UnuseOneWayCells.Count > 0).ToList();

            var randSection = randSections[Random.Range(0, randSections.Count)];
            var otherSection = otherSections[Random.Range(0, otherSections.Count)];
            if(randSection != otherSection)
            {
                var randCell = randSection.UnuseOneWayCells[Random.Range(0,randSection.UnuseOneWayCells.Count)];
                var otherCell = otherSection.UnuseOneWayCells[Random.Range(0, otherSection.UnuseOneWayCells.Count)];

                randSection.AddPortal(randCell, otherCell, otherSection);
                otherSection.AddPortal(otherCell, randCell, randSection);

                connected.Add(otherSection);
            }
        }
        //Debug.Log($"count : {count}");

    }

    void FindFarthestSections()
    {
       
    }

    public void DebugSectionsConnectable()
    {
        foreach(var floor in floors)
        {
            foreach(var section in floor.Sections)
            {
                Debug.Log($"# {section}");
                foreach (var connectableSection in section.ConnectableSection)
                {
                    Debug.Log($"--- {connectableSection}");
                }
            }
        }
    }

    public void DebugSectionsConnectableOneWay()
    {
        foreach (var floor in floors)
        {
            foreach (var section in floor.Sections)
            {
                Debug.Log($"# {section}");
                foreach (var connectableSection in section.OneWayConnectableSection)
                {
                    Debug.Log($"--- {connectableSection} ({section.GetOneWayConnectableCells(connectableSection).Count()})");
                    
                }

            }
        }
    }

    public void DebugFarthestPos()
    {
        var path = floors[0].Sections[0].FindPathFarthestOneWayCells();
        Debug.Log(path.First());
        Debug.Log(path.Last());
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MazeTowerGenerator))]
public class HirachicalMazeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var towerMazeGenerator = (MazeTowerGenerator)target;

        if (towerMazeGenerator == null) return;

        DrawDefaultInspector();

        if (GUILayout.Button("PlaceGround"))
        {
            towerMazeGenerator.PlaceGroundFloor();
        }

        if(GUILayout.Button("Count Tilemap PhysicsShape"))
        {
            towerMazeGenerator.DebugPhysicsShapeCount();
        }
        /*
        if (GUILayout.Button("debug"))
        {
            towerMazeGenerator.DebugSectionsConnectable();
        }

        if (GUILayout.Button("debugOneWay"))
        {
            towerMazeGenerator.DebugSectionsConnectableOneWay();
        }

        if (GUILayout.Button("debugFarthestCellPos"))
        {
            towerMazeGenerator.DebugFarthestPos();
        }*/
    }
}
#endif
