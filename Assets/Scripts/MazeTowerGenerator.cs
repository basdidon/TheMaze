using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using BasDidon;
using Random = UnityEngine.Random;

public class MazeTowerGenerator : MonoBehaviour, IMazePath
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
    [field: SerializeField] public PathTile PathTile { get; private set; }

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

    private void Start()
    {
        Grid = FindFirstObjectByType<Grid>();
        PathTile.Initialize(this);
    }

    // Implement IMazePath
    public bool TryGetTileConection(Vector3Int cellPos,out TileConnection connection)
    {
        connection = new();

        foreach (var floor in floors)
        {
            // inside rect handle
            if(floor.TryGetCellData((Vector2Int)cellPos,out CellData cellData))
            {
                connection = cellData.connection;
                return true;
            }
        }

        return false;
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
                    PathTileMap.SetTile(cellPos, PathTile);
                }

            }
        }

        // mark onewaycells
        foreach(var floor in Floors)
        {
            foreach(var section in floor.Sections)
            {
                foreach(var oneWayCell in section.OneWayCells)
                {
                    Instantiate(stairPrefab, Grid.GetCellCenterWorld((Vector3Int)oneWayCell), Quaternion.identity, stairParent);
                }
            }
        }
        /*
        foreach(var oneWayCell in Floors.SelectMany(floor => floor.Sections.SelectMany(section => section.OneWayCells)))
        {
            Instantiate(stairPrefab, Grid.GetCellCenterWorld(((Vector3Int)section.Floor.LocalToWorldPos(stair.LocalCellPos))), Quaternion.identity, stairParent);
        }

        /*
        // place stair for each section
        foreach(var section in Floors.SelectMany(floor => floor.Sections))
        {
            section.ConnectUnconnectSection();
        }

        foreach(var section in Floors.SelectMany(floor => floor.Sections))
        {
            // render stairs
            foreach(var stair in section.Stairs)
            {
                Instantiate(stairPrefab, Grid.GetCellCenterWorld(((Vector3Int)section.Floor.LocalToWorldPos(stair.LocalCellPos))), Quaternion.identity, stairParent);
            }
        }
        */
        //ConnectAllSections();

        // section
        foreach (var floor in Floors)
        {
            foreach(var rectPos in floor.FloorRect.allPositionsWithin)
            {
                Color tileColor = Color.black;
                Section section = floor.GetSection(rectPos);
                /*
                Debug.Log(section);
                if (section.IsConnentMainWay)
                {
                    tileColor = Color.white;
                }
                else if (!section.IsUnconnected)
                {

                }*/

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

    /*
    // unused
    void ConnectAllSections()
    {
        var startSection = floors[0].Sections[0];

        List<Section> UnConnectedSections = floors.SelectMany(floor => floor.Sections).ToList();
        List<Section> QueuedSections = new() { startSection };
        List<Section> ConnectedSections = new() { };

        UnConnectedSections.Remove(startSection);

        while(QueuedSections.Count > 0)
        {
            var section = QueuedSections.Last();
            QueuedSections.Remove(section);
            section.IsConnentMainWay = true;
            foreach(var otherSection in section.Stairs.Select(stair => stair.TargetSection))
            {
                if (UnConnectedSections.Contains(otherSection))
                {
                    UnConnectedSections.Remove(otherSection);
                    QueuedSections.Add(otherSection);
                }
            }
        }

        
    }
*/
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

   
    /*
    void ConnectSections()
    {
        if (TowerFloor <= 1)
            return;

        var sections = Floors.Select(floor => floor.GetSectionByLocalPos(Vector2Int.zero)).ToArray();
        if(TowerFloor == 2)
        {
            sections[0].AddConnectableSection(sections[1]);
            sections[1].AddConnectableSection(sections[0]);
        }
    }*/
    /*
public void RandomFloorSectionOrder()
{
   if (SectionCount < TowerFloor)
       SectionCount = TowerFloor;

   FloorSectionOrder = new int?[SectionCount];

   for (int i = 0; i < TowerFloor; i++)
   {
       int rand;

       do
       {
           rand = Random.Range(0, SectionCount);
       } while (FloorSectionOrder[rand] != null);

       FloorSectionOrder[rand] = i;
   }

   for (int i = 0; i < SectionCount; i++)
   {
       if (FloorSectionOrder[i] != null)
           continue;

       FloorSectionOrder[i] = Random.Range(0, TowerFloor);
   }

   Debug.Log("--------------");
   foreach (var value in FloorSectionOrder)
   {
       Debug.Log(value);
   }
}
public void RandomCreateStair(int floorIndex, int otherFloorIndex)
{
   int lowerFloorIndex;
   int upperFloorIndex;

   if (floorIndex > otherFloorIndex)
   {
       lowerFloorIndex = otherFloorIndex;
       upperFloorIndex = floorIndex;
   }
   else if (floorIndex < otherFloorIndex)
   {
       lowerFloorIndex = floorIndex;
       upperFloorIndex = otherFloorIndex;
   }
   else  // equals
   {
       return;
   }

   Vector3Int randCellPos;
   do
   {
       randCellPos = new(UnityEngine.Random.Range(MapRect.xMin, MapRect.xMax), Random.Range(MapRect.yMin, MapRect.yMax), 0);
   }
   while (randCellPos == StartAt);

   for (int i = lowerFloorIndex; i <= upperFloorIndex - lowerFloorIndex; i++)
   {
       if (i == upperFloorIndex || i == lowerFloorIndex)
       {
           GroundTileMap.SetTile(GetWorldPos(randCellPos, i), StairTile);
       }
       else
       {
           GroundTileMap.SetTile(GetWorldPos(randCellPos, i), null);
       }
   }
}
*/
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
        }
        /*
        if (GUILayout.Button("RamdomSectionOrder"))
        {
            //towerMazeGenerator.RandomFloorSectionOrder();
        }

        if (GUILayout.Button("RandomStair"))
        {
            //towerMazeGenerator.RandomCreateStair(0,2);
        }

        if (GUILayout.Button("Generate Maze"))
        {
            //towerMazeGenerator.CreateMaze();
        }*/
    }
}
#endif
