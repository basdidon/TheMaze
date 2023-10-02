using System;
using UnityEngine;

public struct CellData
{
    public int? depth;
    public int? SectionId { get; set; }
    public TileConnection connection;

    public CellData SetDepth(int newDepth)
    {
        if (depth == null || newDepth < depth)
        {
            depth = newDepth;
        }
        return this;
    }

    public CellData SetConnectByDir(Vector2Int dir, bool value = true)
    {
        if (dir == Vector2Int.up)
        {
            connection.IsConnectN = value;
        }
        else if (dir == Vector2Int.down)
        {
            connection.IsConnectS = value;
        }
        else if (dir == Vector2Int.left)
        {
            connection.IsConnectW = value;
        }
        else if (dir == Vector2Int.right)
        {
            connection.IsConnectE = value;
        }
        else
        {
            throw new Exception("Unexpected value");
        }

        return this;
    }

    public CellData SetConnectByDir(Vector3Int dir, bool value = true)
    {
        return SetConnectByDir((Vector2Int)dir, value);
    }

    public bool IsConnectTo(Vector2Int dir)
    {
        if (dir == Vector2Int.up)
        {
            return connection.IsConnectN;
        }
        else if (dir == Vector2Int.down)
        {
            return connection.IsConnectS;
        }
        else if (dir == Vector2Int.left)
        {
            return connection.IsConnectW;
        }
        else if (dir == Vector2Int.right)
        {
            return connection.IsConnectE;
        }
        else
        {
            throw new Exception("Unexpected value");
        }
    }

    internal CellData SetDepth(int? v)
    {
        throw new NotImplementedException();
    }

    public bool IsOneWayCell
    {
        get
        {
            int wayCount = 0;

            if (connection.IsConnectN) wayCount++;
            if (connection.IsConnectS) wayCount++;
            if (connection.IsConnectW) wayCount++;
            if (connection.IsConnectE) wayCount++;

            return wayCount == 1;
        }
    }
}
