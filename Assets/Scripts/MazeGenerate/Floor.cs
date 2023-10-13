using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;

[Serializable]
public class Floor
{
    public MazeTowerGenerator Maze { get; }
    public RectInt FloorRect { get; }

    // Sections on this floor
    readonly List<Section> sections;
    public IReadOnlyList<Section> Sections => sections;

    // CellDataMap on this floor
    readonly Dictionary<Vector2Int, CellData> cellDataMap;
    public IReadOnlyDictionary<Vector2Int, CellData> CellDataMap => cellDataMap;

    public Floor(MazeTowerGenerator maze, RectInt floorRect, int numSection)
    {
        Maze = maze;
        FloorRect = floorRect;
        var _numSection = numSection > 0 ? numSection : 1;

        sections = new();
        for (int i = 0; i < _numSection; i++)
        {
            sections.Add(new Section(this));
        }

        cellDataMap = new();

        CreateMazeFloor();
    }

    public int FloorIndex => Maze.GetFloorIndex(this);
    public int GetSectionIdx(Section section) => sections.IndexOf(section);
    public int GetSectionIdx(Vector2Int cellPos) => sections.FindIndex(section => section.IsContain(cellPos));

    public Section GetSection(Vector2Int worldPos) => sections.FirstOrDefault(section => section.IsContain(worldPos));
    public IEnumerable<Vector2Int> OneWayCells => CellDataMap.Where(pair => pair.Value.IsOneWayCell).Select(pair => pair.Key);

    void CreateMazeFloor()
    {
        Vector2Int[] StartAts = new Vector2Int[Sections.Count];
        for (int i = 0; i < Sections.Count; i++)
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
        List<Vector2Int> visitedList = new(StartAts);
        List<Vector2Int>  finishList = new();

        int LoopLimiter = 4 * FloorRect.width * FloorRect.height;
        var loopCount = 0;

        while (visitedList.Count > 0 && loopCount < LoopLimiter)
        {
            loopCount++;
            Vector2Int toSearchCell;

            if (Sections.Count > 1)
            {
                var randSection = Random.Range(0, Sections.Count);
                // search by last cell of random section
                toSearchCell = visitedList.LastOrDefault(cell => sections[randSection].IsContain(cell));
            }
            else
            {
                toSearchCell = visitedList.Last();
            }

            var nodes = neighborDirs.Select(dir => dir + toSearchCell)
                .Where(pos => !IsOutOfBound(pos) && !visitedList.Contains(pos) && !finishList.Contains(pos))
                .ToArray();

            if (nodes.Length > 0)
            {
                var result = nodes[Random.Range(0, nodes.Length)];
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


    bool IsOutOfBound(Vector2Int pos)
    {
        if (pos.x < FloorRect.xMin || pos.x >= FloorRect.xMax)
            return true;
        if (pos.y < FloorRect.yMin || pos.y >= FloorRect.yMax)
            return true;

        return false;
    }
}

