using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEditor;

[RequireComponent(typeof(IMazeGenerator))]
public class MazeRenderer : MonoBehaviour
{
    // Instance
    public static MazeRenderer Instance { get; private set; }

    // Mode
    public enum RenderModes { SINGLE, ALL }
    [SerializeField] RenderModes renderMode = RenderModes.SINGLE;
    public RenderModes RenderMode
    {
        get => renderMode;
        set => renderMode = value;
    }
    // Generator
    public IMazeGenerator Generator { get; private set; }
    public IReadOnlyList<Floor> Floors => Generator.Floors;

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
        Color.green,
        Color.cyan,
        Color.yellow
    };

    [field: SerializeField] public MazeTile MazeTile { get; private set; }

    // scene objects
    [SerializeField] public List<GameObject> portalClones;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        Generator = GetComponent<MazeTowerGenerator>();
    }

    private void Start()
    {
        Grid = FindFirstObjectByType<Grid>();

        portalClones = new();
    }

    void ClearTileMaps()
    {
        GroundTileMap.ClearAllTiles();
        SectionTileMap.ClearAllTiles();
        PathTileMap.ClearAllTiles();
    }

    void ClearPortals()
    {
        foreach (var portalClone in portalClones)
        {
            portalClone.SetActive(false);
        }
        portalClones.Clear();
    }


    // Render Maze
    public void RenderAllFloors()
    {
        ClearPortals();
        ClearTileMaps();

        foreach (var floor in Floors)
        {
            foreach (var rectPos in floor.FloorRect.allPositionsWithin)
            {
                Vector3Int cellPos = (Vector3Int)rectPos;

                // Ground
                GroundTileMap.SetTile(cellPos, GroundTile);

                // Path
                if (floor.TryGetCellData(rectPos, out CellData cellData))
                {
                    PathTileMap.SetTile(cellPos, MazeTile.GetTile(cellData.connection));
                }

                // section
                Color tileColor = Color.black;

                var sectionIdx = floor.GetLocalSectionIdx(rectPos);
                if (sectionIdx != -1)
                {
                    if (sectionIdx < SectionColors.Length)
                    {
                        tileColor = SectionColors[sectionIdx];
                    }
                }

                SectionTileMap.SetTile(cellPos, SectionTile);
                SectionTileMap.SetColor(cellPos, tileColor);
            }

            foreach (var section in floor.Sections)
            {
                foreach (var portalData in section.Portals)
                {
                    var clone = PortalObjectPool.Instance.GetObject(Grid.GetCellCenterWorld((Vector3Int)floor.LocalToWorldPos(portalData.FromLocalPos)));
                    if (clone == null)
                        return;

                    portalClones.Add(clone);
                    if (clone.TryGetComponent(out Portal portal))
                    {
                        portal.SetDestination(portalData);
                    }

                }
            }
        }
    }

    public void RenderFloor(int floorIdx)
    {
        if (floorIdx < 0 || floorIdx >= Floors.Count)
            return;

        ClearTileMaps();
        ClearPortals();

        var floor = Floors[floorIdx];

        foreach (var rectPos in floor.FloorRect.allPositionsWithin)
        {
            Vector2Int localRectPos = rectPos - floor.FloorRect.position;
            Vector3Int localCellPos = (Vector3Int)localRectPos;

            // Ground
            GroundTileMap.SetTile(localCellPos, GroundTile);

            // Path
            if (floor.TryGetCellData(rectPos, out CellData cellData))
            {
                PathTileMap.SetTile(localCellPos, MazeTile.GetTile(cellData.connection));
            }

            // section
            Color tileColor = Color.black;

            var sectionIdx = floor.GetLocalSectionIdx(rectPos);
            if (sectionIdx != -1)
            {
                if (sectionIdx < SectionColors.Length)
                {
                    tileColor = SectionColors[sectionIdx];
                }
            }

            SectionTileMap.SetTile(localCellPos, SectionTile);
            SectionTileMap.SetColor(localCellPos, tileColor);
        }

        foreach (var section in floor.Sections)
        {
            foreach (var portalData in section.Portals)
            {
                var clone = PortalObjectPool.Instance.GetObject(Grid.GetCellCenterWorld((Vector3Int)floor.LocalToWorldPos(portalData.FromLocalPos)));
                if (clone == null)
                    return;

                portalClones.Add(clone);
                if (clone.TryGetComponent(out Portal portal))
                {
                    portal.SetDestination(portalData);
                }

            }
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(MazeRenderer))]
public class MazeRendererEditor : Editor
{
    int floorIdx = 0;

    public override void OnInspectorGUI()
    {
        var renderer = (MazeRenderer)target;

        if (renderer == null) return;

        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            if (renderer.Floors.Count > 0)
            {
                if (GUILayout.Button("RenderAllFloor"))
                {
                    renderer.RenderMode = MazeRenderer.RenderModes.ALL;
                    renderer.RenderAllFloors();
                }

                GUILayout.BeginHorizontal();

                floorIdx = EditorGUILayout.IntField(floorIdx);
                if (GUILayout.Button("RenderFloor"))
                {
                    Debug.Log(floorIdx);
                    renderer.RenderFloor(floorIdx);
                    renderer.RenderMode = MazeRenderer.RenderModes.SINGLE;
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}

#endif