using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public interface IMazeGenerator
{
    public Vector3Int MazeSize { get; }
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
    [field: SerializeField] public Vector3Int MazeSize { get; private set; }

    [field: SerializeField] int MaxSection { get; set; }

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

        for (int i = 0; i < MazeSize.y; i++)
        {
            var floor = new Floor(this, new RectInt(0, 0, MazeSize.x, MazeSize.z), MaxSection);
            floors.Add(floor);
        }

        ConnectAllSection();

        List<SectionsDistance> sectionsDistances = new();
        FindFarthestSections(sectionsDistances);
        foreach(var sectionPair in sectionsDistances.OrderByDescending(sectionPair=>sectionPair.Distance))
        {
            if(sectionPair.From.UnuseOneWayCells.Count > 0 && sectionPair.To.UnuseOneWayCells.Count > 0)
            {
                var startPos = sectionPair.From.UnuseOneWayCells[Random.Range(0, sectionPair.From.UnuseOneWayCells.Count)];
                var endPos = sectionPair.To.UnuseOneWayCells[Random.Range(0, sectionPair.To.UnuseOneWayCells.Count)];

                StartAt = new PositionRef(sectionPair.From.Floor,startPos);
                EndAt = new PositionRef(sectionPair.To.Floor, endPos);


                Debug.Log($"start at F:{StartAt.Floor.FloorIndex} ,{StartAt.CellPos}");
                Debug.Log($"end at F:{EndAt.Floor.FloorIndex} ,{EndAt.CellPos}");
                Debug.Log($"distance is : {sectionPair.Distance}");
                break;
            }
        }

    }

    void ConnectAllSection(int MaxLoop = 10)
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
                        queued.Add(new SectionsDistance(startFrom, other, _sectionDistance.Distance + 1));
                    }
                    else if (isInCompleted)
                    {
                        var completedSD = completed.Find(sd => sd.To.ConnectedSections.Contains(other));
                        if (completedSD.Distance > _sectionDistance.Distance + 1)
                        {
                            completedSD.Distance = _sectionDistance.Distance + 1;
                        }
                    }
                }
                queued.Remove(_sectionDistance);
                completed.Add(_sectionDistance);
            }

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
