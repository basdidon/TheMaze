using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PrimMaze3D : MonoBehaviour
{
    [field:SerializeField] Vector3Int MazeSize { get; set; }
    Dictionary<Vector3Int, int> maze;
    public IReadOnlyDictionary<Vector3Int, int> Maze=>maze;

    // define mask as bit
    private const int WALL_ABOVE    = 0b00000001;
    private const int WALL_BELOW    = 0b00000010;
    private const int WALL_LEFT     = 0b00000100;
    private const int WALL_RIGHT    = 0b00001000;
    private const int WALL_FRONT    = 0b00010000;
    private const int WALL_BACK     = 0b00100000;

    // 6 dirs
    readonly Vector3Int[] dirs = new[] {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.forward,
        Vector3Int.back,
    };

    private void Start()
    {
        ResetMaze();
    }

    public void ResetMaze()
    {
        maze = new();
        for (int z = 0; z < MazeSize.z; z++)
        {
            for (int y = 0; y < MazeSize.y; y++)
            {
                for (int x = 0; x < MazeSize.x; x++)
                {
                    maze.Add(new Vector3Int(x, y, z), 0b11111111);
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


        Vector3Int nextPos;

        Vector3Int startNode = new(Random.Range(0, MazeSize.x), Random.Range(0, MazeSize.y), Random.Range(0, MazeSize.z));

        visitedList.Add(startNode);

        while(visitedList.Count > 0)
        {
            // random one node inside todo
            var rand = Random.Range(0, visitedList.Count);
            var randCell = visitedList[rand];
            
            // remove from todo
            visitedList.RemoveAt(rand);

            var passBool = false;
            while (passBool == false)
            {
                // ********* maybe it infinite loop
                var d = Random.Range(0, dirs.Length);
                nextPos = randCell + dirs[d];
                if (IsInsideBound(nextPos) && !completedList.Contains(nextPos))
                {
                    passBool = true;

                    // connect them
                    maze[randCell] &= ~(1 << d);
                    completedList.Add(randCell);
                    maze[nextPos] &= ~(1 << (d ^ 1));
                    visitedList.Add(nextPos);
                }
            }
        }
    }

    readonly Vector3Int[] horizonDir = new[] {
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.right,
    };

    readonly Vector3Int[] verticalDir = new[]
    {
        Vector3Int.up,
        Vector3Int.down,
    };

   void RandomNextPos(Vector3Int cellPos)
    {
        
        var result = horizonDir
            .Select(dir => dir + cellPos)
            .Where(pos => IsInsideBound(pos)&&!visitedList.Contains(pos)&&!completedList.Contains(pos))
            .ToList();

        var randAxis = Random.Range(0,1f);
        
        if(randAxis < 0.9f) // horizontal case
        {

        }
        else // vertical case
        {

        }

        if (result.Count() <= 2)
        {
            foreach(var dir in verticalDir)
            {
                var pos = dir + cellPos;
                if(IsInsideBound(pos) && !visitedList.Contains(pos) && !completedList.Contains(pos))
                {
                    result.Add(pos);
                }
            }
        }

        // random one of 
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
