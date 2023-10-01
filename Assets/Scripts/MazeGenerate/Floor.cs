using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;

[Serializable]
public class Section
{
    Floor Floor { get; }
    MazeTowerGenerator Maze => Floor.Maze;
    List<Vector2Int> sectionCells;
    public IReadOnlyList<Vector2Int> SectionCells => sectionCells;

    public Section(Floor floor)
    {
        Floor = floor;
        sectionCells = new();
    }

    public void AddCell(Vector2Int cellPos) => sectionCells.Add(cellPos);
    public bool IsContain(Vector2Int cellPos) => sectionCells.Contains(cellPos);

    public HashSet<Section> GetConnectableSection()
    {
        //var set = OthersFloor.Where(floor=>floor.GetSectionByLocalPos())
        var set = sectionCells.Select(cellPos => Maze.GetSectionsByLocalPos(Floor.WorldToLocalPos(cellPos)))
            .Aggregate(new HashSet<Section>(), (acc, set) => { 
                acc.UnionWith(set); 
                return acc;
            });

        set.Remove(this);

        return set;
    }

    public IEnumerable<Floor> OthersFloor => Maze.Floors.Where(floor => floor != Floor);
    public IEnumerable<Section> AllSectionsInOtherFloor => OthersFloor.SelectMany(floor => floor.Sections);

    public IEnumerable<Vector2Int> OneWayCells => Floor.OneWayCells.Where(cellPos=>IsContain(cellPos));

    public override string ToString() => $"F: {Floor.GetFloorIndex()} ,S: {Floor.GetSectionIndex(this)}";
}

[Serializable]
public class Floor
{
    public MazeTowerGenerator Maze { get; }
    public RectInt FloorRect { get; }

    [field: SerializeField] public int NumSection { get; private set; }

    readonly List<Section> sections;
    public IReadOnlyList<Section> Sections => sections;
    Dictionary<Vector2Int, CellData> CellDataMap { get; }
    public KeyValuePair<Vector2Int, CellData> MostDepthCell => CellDataMap.OrderByDescending(pair => pair.Value.depth).First();

    int LoopLimiter => 4 * FloorRect.width * FloorRect.height;

    public Floor(MazeTowerGenerator maze, RectInt floorRect, int numSection)
    {
        Maze = maze;
        FloorRect = floorRect;
        NumSection = numSection > 0 ? numSection : 1;

        sections = new();
        for (int i = 0; i < NumSection; i++)
        {
            sections.Add(new Section(this));
        }

        CellDataMap = new();

        PlaceSectionRoot();
    }


    public int GetFloorIndex() => Maze.GetFloorIndex(this);
    public int GetSectionIndex(Section section) => sections.IndexOf(section);

    Section GetSection(Vector2Int cellPos) => sections.FirstOrDefault(section => section.IsContain(cellPos));
    public Section GetSectionByLocalPos(Vector2Int localPos) => GetSection(LocalToWorldPos(localPos));

    /// <returns> index of <c>Section</c> on <c>Floor</c>,or -1 if the element is not found.</returns>
    public int GetLocalSectionIdx(Vector2Int cellPos) => sections.FindIndex(section => section.IsContain(cellPos));

    // Convertor
    public Vector2Int LocalToWorldPos(Vector2Int localPos) => localPos + FloorRect.position;
    public Vector2Int WorldToLocalPos(Vector2Int worldPos) => worldPos - FloorRect.position;


    List<Vector2Int> visitedList;
    List<Vector2Int> finishList;
    void PlaceSectionRoot()
    {
        Vector2Int[] StartAts = new Vector2Int[NumSection];
        for (int i = 0; i < NumSection; i++)
        {
            Vector2Int randPos;
            do
            {
                randPos = new(Random.Range(FloorRect.xMin, FloorRect.xMax), Random.Range(FloorRect.yMin, FloorRect.yMax));
            } while (GetSection(randPos) != null);

            sections[i].AddCell(randPos);
            StartAts[i] = randPos;
        }

        // start create
        visitedList = new(StartAts);
        finishList = new();

        var loopCount = 0;

        while (visitedList.Count > 0 && loopCount < LoopLimiter)
        {
            loopCount++;
            Vector2Int toSearchCell;

            if (NumSection > 1)
            {
                var randSection = Random.Range(0, NumSection);
                // search by last cell of random section
                toSearchCell = visitedList.LastOrDefault(cell => sections[randSection].IsContain(cell));
            }
            else
            {
                toSearchCell = visitedList.Last();
            }

            if (RandomUnvisitNode(toSearchCell, out Vector2Int result))
            {
                Connect(toSearchCell, result);
                visitedList.Add(result);
            }
            else
            {
                visitedList.Remove(toSearchCell);
                finishList.Add(toSearchCell);
            }
        }
    }

    public bool TryGetCellData(Vector2Int cellPos, out CellData cellData)
    {
        if (CellDataMap == null)
            throw new Exception("CellDataMap Empty");
        return CellDataMap.TryGetValue(cellPos, out cellData);
    }

    public void Connect(Vector2Int from, Vector2Int to)
    {
        var dir = to - from;
        if (Mathf.Abs(dir.x) + Mathf.Abs(dir.y) != 1)
            return;

        if (GetSection(from) is Section section)
        {
            section.AddCell(to);
        }

        if (!CellDataMap.TryGetValue(from, out CellData fromCellData))
        {
            fromCellData = new();
            CellDataMap.Add(from, fromCellData);
        }

        if (!CellDataMap.TryGetValue(to, out CellData toCellData))
        {
            toCellData = new();
            CellDataMap.Add(to, toCellData);
        }

        CellDataMap[from] = fromCellData.SetConnectByDir(to - from);
        fromCellData.depth ??= 1;
        if (fromCellData.depth is int fromDepth)
        {
            CellDataMap[to] = toCellData.SetConnectByDir(from - to).SetDepth(fromDepth + 1);
        }
    }

    readonly Vector2Int[] neighborDirs = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
    };

    bool RandomUnvisitNode(Vector2Int pos, out Vector2Int result)
    {
        result = Vector2Int.zero;
        var nodes = neighborDirs.Select(dir => dir + pos)
            .Where(pos => !IsOutOfBound(pos) && !visitedList.Contains(pos) && !finishList.Contains(pos))
            .ToArray();

        if (nodes.Length == 0)
            return false;

        result = nodes[Random.Range(0, nodes.Length)];
        return true;
    }

    bool IsOutOfBound(Vector2Int pos)
    {
        if (pos.x < FloorRect.xMin || pos.x >= FloorRect.xMax)
            return true;
        if (pos.y < FloorRect.yMin || pos.y >= FloorRect.yMax)
            return true;

        return false;
    }

    public IEnumerable<Vector2Int> OneWayCells => CellDataMap.Where(pair => pair.Value.IsOneWayCell).Select(pair => pair.Key);
    public bool IsOneWayCell(Vector2Int localPos) {
        if (!CellDataMap.TryGetValue(LocalToWorldPos(localPos), out CellData cellData))
            return false;

        return cellData.IsOneWayCell;
    } 
}
