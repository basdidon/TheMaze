using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
[CreateAssetMenu(menuName = "PathTile")]
public class PathTile : Tile
{
    [Header("NoConnect")]
    [SerializeField] Sprite noConnectTileSprite;
    [Header("SingleWayConnect")]
    [SerializeField] Sprite test;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        if((position.x + position.y) % 2 == 0)
        {
            tileData.sprite = noConnectTileSprite;
        }
        else
        {
            tileData.sprite = test;
        }
    }


}*/