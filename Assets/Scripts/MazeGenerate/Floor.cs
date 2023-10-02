using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;
using BasDidon.PathFinder2D;

[Serializable]
public class Section
{
    public Floor Floor { get; }
    MazeTowerGenerator Maze => Floor.Maze;

    readonly List<Vector2Int> sectionCells;
    public IReadOnlyList<Vector2Int> SectionCells => sectionCells;

    readonly List<Stair> stairs;
    public IReadOnlyList<Stair> Stairs => stairs;

    public IReadOnlyList<Vector2Int> LocalSectionCells => SectionCells.Select(cell => Floor.WorldToLocalPos(cell)).ToList();
    public IEnumerable<Vector2Int> OneWayCells => Floor.OneWayCells.Where(cellPos=>IsContain(cellPos));
    public IEnumerable<Vector2Int> LocalOneWayCells => OneWayCells.Select(cell => Floor.WorldToLocalPos(cell));

    public Section(Floor floor)
    {
        Floor = floor;
        sectionCells = new();
    }

    // Modify
    public void AddCell(Vector2Int cellPos) => sectionCells.Add(cellPos);
    public bool IsContain(Vector2Int cellPos) => sectionCells.Contains(cellPos);

    // ** Connect on OneWayCells
    public void ConnectUnconnectSection()
    {
        // Get unconnect sections
        var unconnectList = OneWayConnectableSection.Where(section => !stairs.Any(stair => stair.TargetSection == section)).ToList();
        // pick one of them by random
        var randSection = unconnectList[Random.Range(0,unconnectList.Count)];
        // Get possible connect cells
        var cells = GetOneWayConnectableCells(randSection).ToList();
        // pick one of cells by random
        var randCell = cells[Random.Range(0, cells.Count)];

        // add stair to both section
        AddStair(randCell, randSection.Floor);
        randSection.AddStair(randCell, Floor);
    }

    public void AddStair(Vector2Int localPos, Floor targetFloor)
    {
        stairs.Add(new(localPos, targetFloor));
    }

    // Query
    public IEnumerable<Section> ConnectableSection => OtherFloorSections.Where(section => LocalSectionCells.Intersect(section.LocalSectionCells).Any());
    public IEnumerable<Section> OneWayConnectableSection => OtherFloorSections.Where(section => GetOneWayConnectableCells(section).Any());

    public IEnumerable<Vector2Int> GetOneWayConnectableCells(Section section) => LocalOneWayCells.Intersect(section.LocalOneWayCells);

    public IEnumerable<Floor> OthersFloor => Maze.Floors.Where(floor => floor != Floor);
    public IEnumerable<Section> OtherFloorSections => OthersFloor.SelectMany(floor => floor.Sections);

    public List<Vector2Int> FindPathFarthestOneWayCells()
    {
        var cellPosPairs = (
            from fromCellPos in OneWayCells.Select((cellPos, index) => new { CellPos = cellPos, Index = index })
            from toCellPos in OneWayCells.Skip(fromCellPos.Index + 1)
            select new { FromCellPos = fromCellPos.CellPos, ToCellPos = toCellPos }
        ).ToList();

        List<Vector2Int> result = null;
        cellPosPairs.ForEach(cellPair =>
        {
            if(PathFinder.TryFindPath(Floor,cellPair.FromCellPos,cellPair.ToCellPos,Floor.neighborDirs.ToList(),out List<Vector2Int> resultPath))
            {
                if (result == null || result.Count < resultPath.Count)
                {
                    result = resultPath;
                }
            }
        });

        return result;
    }
    public override string ToString() => $"F: {Floor.GetFloorIndex()} ,S: {Floor.GetSectionIndex(this)}";
}

[Serializable]
public class Floor:IPathMap
{
    public MazeTowerGenerator Maze { get; }
    public RectInt FloorRect { get; }

    [field: SerializeField] public int NumSection { get; private set; }

    readonly List<Section> sections;
    public IReadOnlyList<Section> Sections => sections;

    readonly Dictionary<Vector2Int, CellData> cellDataMap;
    public IReadOnlyDictionary<Vector2Int, CellData> CellDataMap => cellDataMap;
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

        cellDataMap = new();

        PlaceSectionRoot();
    }

    public int GetFloorIndex() => Maze.GetFloorIndex(this);
    public int GetSectionIndex(Section section) => sections.IndexOf(section);

    public Section GetSection(Vector2Int worldPos) => sections.FirstOrDefault(section => section.IsContain(worldPos));

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
            cellDataMap.Add(from, fromCellData);
        }

        if (!CellDataMap.TryGetValue(to, out CellData toCellData))
        {
            toCellData = new();
            cellDataMap.Add(to, toCellData);
        }

        cellDataMap[from] = fromCellData.SetConnectByDir(to - from);
        fromCellData.depth ??= 1;
        if (fromCellData.depth is int fromDepth)
        {
            cellDataMap[to] = toCellData.SetConnectByDir(from - to).SetDepth(fromDepth + 1);
        }
    }

    public readonly Vector2Int[] neighborDirs = new Vector2Int[]
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

    public bool CanMoveTo(Vector2Int from, Vector2Int to)
    {
        if (!cellDataMap.TryGetValue(from, out CellData fromCell))
            return false;
        if (!cellDataMap.TryGetValue(to, out CellData toCell))
            return false;

        var dir = to - from;
        return fromCell.IsConnectTo(dir);
    }

    public IEnumerable<Vector2Int> OneWayCells => CellDataMap.Where(pair => pair.Value.IsOneWayCell).Select(pair => pair.Key);
}

