using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RandomizeDFSMazeGenerator3D
{
    public Vector3Int MazeSize { get; private set; }
    Dictionary<Vector3Int, int> mazeData;
    public IReadOnlyDictionary<Vector3Int, int> MazeData => mazeData;

    // 6 dirs
    readonly List<Vector3Int> dirs = new(){
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.right,
    };
    Vector3Int[] verticalDirs => dirs.Take(2).ToArray();
    Vector3Int[] horizonDirs => dirs.TakeLast(4).ToArray();

    public RandomizeDFSMazeGenerator3D(Vector3Int mazeSize)
    {
        MazeSize = mazeSize;
    }

    public void ResetMaze()
    {
        mazeData = new();
        for (int z = 0; z < MazeSize.z; z++)
        {
            for (int y = 0; y < MazeSize.y; y++)
            {
                for (int x = 0; x < MazeSize.x; x++)
                {
                    mazeData.Add(new Vector3Int(x, y, z), 0b111111);
                }
            }
        }
    }

    // temp list for CreateMaze
    List<Vector3Int> visitedList;
    List<Vector3Int> completedList;

    public void CreateMaze()
    {
        ResetMaze();

        visitedList = new();
        completedList = new();

        Vector3Int startNode = new(Random.Range(0, MazeSize.x), Random.Range(0, MazeSize.y), Random.Range(0, MazeSize.z));

        visitedList.Add(startNode);
        Debug.Log($"Start at : {startNode}");

        while(visitedList.Count > 0)
        {
            // random one node inside todo
            var rand = Random.Range(0, visitedList.Count);
            var randCell = visitedList[rand];
            
            // remove from todo
            visitedList.RemoveAt(rand);

            if (RandomNextPos(randCell) is Vector3Int nextPos)
            {
                int d = dirs.IndexOf(nextPos-randCell);

                Debug.Log($"d = {d}");

                if(d > -1)
                {
                    // connect them
                    mazeData[randCell] &= ~(1 << d);
                    completedList.Add(randCell);
                    mazeData[nextPos] &= ~(1 << (d ^ 1));
                    visitedList.Add(nextPos);
                    Debug.Log($"connect {randCell} to {nextPos} ({mazeData[randCell]} => {mazeData[nextPos]})");
                }
            }
        }
    }



    Vector3Int? RandomNextPos(Vector3Int cellPos)
    {
        var nextPos = dirs.Select(dir => dir + cellPos);

        Debug.Log("ho");
        foreach(var dir in horizonDirs)
        {
            Debug.Log(dir);
        }

        Debug.Log("ve");
        foreach (var dir in verticalDirs)
        {
            Debug.Log(dir);
        }

        var horizonPos = horizonDirs
            .Select(dir => dir + cellPos)
            .Where(pos => IsInsideBound(pos) && !visitedList.Contains(pos) && !completedList.Contains(pos))
            .ToList();

        var verticalPos = verticalDirs
            .Select(dir => dir + cellPos)
            .Where(pos => IsInsideBound(pos) && !visitedList.Contains(pos) && !completedList.Contains(pos))
            .ToList();
        
        if(horizonPos.Count > 0 && verticalPos.Count > 0)
        {
            var randAxis = Random.Range(0, 1f);

            if (randAxis < 0.9f) // horizontal case
            {
                return horizonPos[Random.Range(0, horizonPos.Count)];
            }
            else // vertical case
            {
                return verticalPos[Random.Range(0,verticalPos.Count)];
            }
        }
        else if(horizonPos.Count > 0 && verticalPos.Count == 0)
        {
            return horizonPos[Random.Range(0, horizonPos.Count)];
        }
        else if(horizonPos.Count == 0 && verticalPos.Count > 0)
        {
            return verticalPos[Random.Range(0, verticalPos.Count)];
        }
        else
        {
            return null;
        }
    }

    bool IsInsideBound(Vector3Int cellPos)
    {
        if (cellPos.x < 0 || cellPos.x >= MazeSize.x)
            return false;
        if (cellPos.y < 0 || cellPos.y >= MazeSize.y)
            return false;
        if (cellPos.z < 0 || cellPos.z >= MazeSize.z)
            return false;
        return true;
    }

}
