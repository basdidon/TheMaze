using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    Grid Grid { get; set; }
    [field :SerializeField] Player Player { get; set; }
    MazeRenderer MazeRenderer => MazeRenderer.Instance;
    [field :SerializeField] MazeTowerGenerator MazeTowerGenerator { get; set; }
    //MazeRenderer MazeRenderer { get; set; }

    private void Awake()
    {
        Grid = FindFirstObjectByType<Grid>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("LM started");

        MazeTowerGenerator.CreateMaze();
        MazeRenderer.RenderFloor(MazeTowerGenerator.StartAt.Floor.FloorIndex);
        Player.transform.position = Grid.GetCellCenterWorld((Vector3Int) MazeTowerGenerator.StartAt.CellPos);

        // move main camera to center of maze
        Camera.main.transform.position = (Vector3)MazeTowerGenerator.Floors[0].FloorRect.center + (Vector3.back * 10);
        Camera.main.orthographicSize = MazeTowerGenerator.MazeSize.z / 2;
    }
}
