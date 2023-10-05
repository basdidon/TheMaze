using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class MazeRenderer : MonoBehaviour,IMazePath
{
    // Maze Properties
    [field: SerializeField] RectInt MazeRect { get; set; }
    [field: SerializeField] int FloorCount { get; set; }

    RandomizeDFSMazeGenerator3D Generator { get; set; }
    IReadOnlyDictionary<Vector3Int, int> MazeData => Generator.MazeData;

    Vector3Int MazeDataPosToWorldCell(Vector3Int mazeDataPos) => new(mazeDataPos.x + (mazeDataPos.y * (FloorOffset + MazeRect.width)), mazeDataPos.z);
    Vector3Int WorldCellToMazeDataPos(Vector3Int worldCell) => new(worldCell.x % (FloorOffset + MazeRect.width),worldCell.x / (FloorOffset + MazeRect.width), worldCell.y);

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

    [field: SerializeField] public PathTile PathTile { get; private set; }

    // Floor Properties
    [field: SerializeField] int FloorOffset { get; set; } = 1;

    private void Start()
    {
        PathTile.Initialize(this);

        Generator = new RandomizeDFSMazeGenerator3D(new Vector3Int(MazeRect.width,FloorCount,MazeRect.height));
        Generator.CreateMaze();
        PlaceGround();
        RenderPath();
    }

    public void PlaceGround()
    {
        foreach(var mazeDataPos in MazeData.Keys)
        {
            GroundTileMap.SetTile(MazeDataPosToWorldCell(mazeDataPos), GroundTile);
        }
    }

    public void RenderPath()
    {
        foreach (var mazeDataPos in MazeData.Keys)
        {
            Tile pathTile = PathTile;
            PathTileMap.SetTile(MazeDataPosToWorldCell(mazeDataPos), pathTile);
        }
    }

    public bool TryGetTileConection(Vector3Int cellPos, out TileConnection connection)
    {
        connection = new();
        var mazeDataPos = WorldCellToMazeDataPos(cellPos);

        if (MazeData.TryGetValue(mazeDataPos,out int code))
        {
            //Debug.Log($"{mazeDataPos} : {Convert.ToString(code,2)}");
            connection = new(code);
            return true;
        }

        return false;
    }
}
