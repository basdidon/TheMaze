using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeGenerator : MonoBehaviour
{
    [field: SerializeField] public Tilemap GroundTileMap { get; private set; }
    [field: SerializeField] public Tilemap PathTileMap { get; private set; }

    [field: SerializeField] public Tile GroundTile {get;private set;}
    [field: SerializeField] public Tile PathTile {get;private set;}

    [field: SerializeField] public Vector2Int MapSize { get; private set; }

    void Start()
    {        
        for(int x=0; x < MapSize.x; x++)
        {
            for(int y=0; y< MapSize.y; y++)
            {
                var cellPos = new Vector3Int(x, y);
                GroundTileMap.SetTile(cellPos,GroundTile);
                PathTileMap.SetTile(cellPos, PathTile);
            }
        }
    }
}
