using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    public static Grid Grid;
    public Vector2Int destinationCell;

    private void Start()
    {
        Grid = FindObjectOfType<Grid>();
    }

    public void SetDestination(Vector2Int des)
    {
        destinationCell = des;
    }

    public void Teleport(Transform user)
    {
        Debug.Log($"teleport to {Grid.GetCellCenterWorld((Vector3Int)destinationCell)}");
        user.position = Grid.GetCellCenterWorld((Vector3Int)destinationCell);
    }
}
