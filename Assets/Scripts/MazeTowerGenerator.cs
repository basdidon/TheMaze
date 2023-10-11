using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public interface IMazeGenerator
{
    public void CreateMaze();
    public PositionRef StartAt { get; }
    public PositionRef EndAt { get; }
    public IReadOnlyList<Floor> Floors { get; }
}

public struct PositionRef
{
    public Floor Floor { get; }
    public Vector2Int CellPos { get; }

    public PositionRef(Floor floor, Vector2Int cellPos)
    {
        Floor = floor;
        CellPos = cellPos;
    }
}

public class MazeTowerGenerator : MonoBehaviour, IMazeGenerator
{
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

    // Query
    IEnumerable<Section> AllSections => Floors.SelectMany(floor => floor.Sections);

    public PositionRef StartAt { get; private set; }
    public PositionRef EndAt { get; private set; }

    public void CreateMaze()
    {
        floors.Clear();

        for (int i = 0; i < TowerFloor; i++)
        {
            var floor = new Floor(this, new RectInt(i * (FloorWidth + FloorOffset), 0, FloorWidth, FloorHeight), MaxSection);
            floors.Add(floor);
        }

        RDFSsection();

        List<SectionsDistance> sectionsDistances = new();
        FindFarthestSections(sectionsDistances);
        foreach(var sectionPair in sectionsDistances)
        {
            if(sectionPair.From.UnuseOneWayCells.Count > 0 && sectionPair.To.UnuseOneWayCells.Count > 0)
            {
                Debug.Log("Found ");
                var startPos = sectionPair.From.UnuseOneWayCells[Random.Range(0, sectionPair.From.UnuseOneWayCells.Count)];
                var endPos = sectionPair.To.UnuseOneWayCells[Random.Range(0, sectionPair.To.UnuseOneWayCells.Count)];

                StartAt = new PositionRef(sectionPair.From.Floor,startPos);
                EndAt = new PositionRef(sectionPair.To.Floor, endPos);
                break;
            }
        }

        Debug.Log($"start at F:{StartAt.Floor.GetFloorIndex()} ,{StartAt.CellPos}");
        Debug.Log($"end at F:{EndAt.Floor.GetFloorIndex()} ,{EndAt.CellPos}");
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

    void RDFSsection(int MaxLoop = 10)
    {
        //random pick one section
        var allSection = AllSections.ToList();
        var startSection = allSection[Random.Range(0,allSection.Count)];
        var connected = new HashSet<Section>() { startSection };

        var loopCount = 0;
        while (loopCount < MaxLoop)
        {
            loopCount++;

            var count = 0;
            while (allSection.Count != connected.Count && count < Mathf.Pow(allSection.Count, 2))
            {
                //Debug.Log($"{connected.Count} / {allSection.Count}");
                count++;

                var randSections = connected.Where(section => section.UnuseOneWayCells.Count > 0).ToList();
                var otherSections = allSection.Where(section => section.UnuseOneWayCells.Count > 0).ToList();

                if (randSections.Count == 0)
                {
                    Debug.Log($"Can't connect anymore reset ({loopCount})");
                    break;
                }
                var randSection = randSections[Random.Range(0, randSections.Count)];
                var otherSection = otherSections[Random.Range(0, otherSections.Count)];
                if (randSection != otherSection)
                {
                    var randCell = randSection.UnuseOneWayCells[Random.Range(0, randSection.UnuseOneWayCells.Count)];
                    var otherCell = otherSection.UnuseOneWayCells[Random.Range(0, otherSection.UnuseOneWayCells.Count)];

                    randSection.AddPortal(randCell, otherCell, otherSection);
                    otherSection.AddPortal(otherCell, randCell, randSection);

                    connected.Add(otherSection);
                }
            }

            if(allSection.Count == connected.Count)
            {
                return;
            }
        }
        //Debug.Log($"count : {count}");

    }

    public void FindFarthestSections(List<SectionsDistance> sectionsDistances)
    {
        List<SectionsDistance> queued = new();
        List<SectionsDistance> completed = new();

        foreach (var startFrom in AllSections)
        {
            queued.Clear();
            completed.Clear();

            foreach (var other in startFrom.ConnectedSections)
            {
                Debug.Log($"{startFrom} -> {other} : 1 added.");
                queued.Add(new SectionsDistance(startFrom, other));
            }

            var loopLimiter = 0;
            while (queued.Count > 0 && loopLimiter < 100)
            {
                loopLimiter++;
                var _sectionDistance = queued.Last();

                foreach (var other in _sectionDistance.To.ConnectedSections)
                {
                    var isInQueued = queued.Exists(sd => sd.To == other || sd.From == other);
                    var isInCompleted = completed.Exists(sd => sd.To == other || sd.From == other);

                    if (!isInQueued && !isInCompleted)
                    {
                        Debug.Log($"{startFrom} to {other} : {_sectionDistance.Distance + 1} added.");
                        queued.Add(new SectionsDistance(startFrom, other, _sectionDistance.Distance + 1));
                    }
                    else if (isInCompleted)
                    {
                        var completedSD = completed.Find(sd => sd.To.ConnectedSections.Contains(other));
                        if (completedSD.Distance > _sectionDistance.Distance + 1)
                        {
                            Debug.Log($"{completedSD.To} update distance from {completedSD.Distance} to {_sectionDistance.Distance + 1} ");
                            completedSD.Distance = _sectionDistance.Distance + 1;
                        }
                    }
                }
                queued.Remove(_sectionDistance);
                completed.Add(_sectionDistance);
            }

            Debug.Log($"-> {completed.Count}");
            sectionsDistances.AddRange(completed);
        }
    }

    public struct SectionsDistance
    {
        public Section From { get;}
        public Section To { get;}
        public int Distance { get; set; }

        public SectionsDistance(Section from, Section to, int distance = 1)
        {
            From = from;
            To = to;
            Distance = distance;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MazeTowerGenerator))]
public class MazeTowerGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var towerMazeGenerator = (MazeTowerGenerator)target;

        if (towerMazeGenerator == null) return;

        DrawDefaultInspector();

        if(Application.isPlaying)
        {
            if (GUILayout.Button("CreateMaze"))
            {
                towerMazeGenerator.CreateMaze();
            }

            if(towerMazeGenerator.Floors.Count > 0)
            {
                if (GUILayout.Button("FarthestSections"))
                {
                    List<MazeTowerGenerator.SectionsDistance> sectionsDistances = new();
                    towerMazeGenerator.FindFarthestSections(sectionsDistances);
                    var result = sectionsDistances.OrderByDescending(sd => sd.Distance).FirstOrDefault();
                    Debug.Log($"{sectionsDistances.Count} | {result.From} -> {result.To} [{result.Distance}]");
                }
            }
        }
    }
}
#endif
