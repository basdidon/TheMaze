using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    public static Grid Grid;
    SpriteRenderer spriteRenderer;

    public PortalData PortalData;

    [SerializeField] public Sprite diffFloorPortalSprite;
    [SerializeField] public Sprite sameFloorPortalSprite;

    private void Awake()
    {
        Grid = FindObjectOfType<Grid>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void SetDestination(PortalData portalData)
    {
        PortalData = portalData;
        if (portalData.FromFloor == portalData.ToFloor)
        {
            spriteRenderer.sprite = sameFloorPortalSprite;
        }
        else
        {
            spriteRenderer.sprite = diffFloorPortalSprite;
        }
    }

    public void Teleport(Transform user)
    {
        if(MazeRenderer.Instance.RenderMode == MazeRenderer.RenderModes.SINGLE)
        {
            Debug.Log($"{PortalData.FromFloor.GetFloorIndex()} -> {PortalData.ToFloor.GetFloorIndex()} pos: {(Vector3Int)PortalData.ToLocalPos}");
            user.transform.position = Grid.GetCellCenterWorld((Vector3Int)PortalData.ToLocalPos);

            ///** Grid.GetCellCenterWorld((Vector3Int)PortalData.ToLocalPos) after below line got unexpected result. so i teleport player first, then swap floor, let's fix it later.
            MazeRenderer.Instance.RenderFloor(PortalData.ToFloor.GetFloorIndex());
            Debug.Log($"-> {Grid.GetCellCenterWorld((Vector3Int)PortalData.ToLocalPos)}");
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }
}

[System.Serializable]
public struct PortalData
{
    [field:SerializeField] public Section FromSection { get; private set; }
    public Floor FromFloor => FromSection.Floor;
    [field: SerializeField] public Vector2Int FromLocalPos { get; private set; }
    public Vector2Int FromWorldPos => FromFloor.LocalToWorldPos(FromLocalPos);

    [field: SerializeField] public Section ToSection { get; private set; }
    public Floor ToFloor => ToSection.Floor;
    [field: SerializeField] public Vector2Int ToLocalPos { get; private set; }
    public Vector2Int ToWorldPos => ToFloor.LocalToWorldPos(ToLocalPos);

    public PortalData(Section fromSection, Section toSection, Vector2Int fromLocalPos, Vector2Int toLocalPos)
    {
        FromSection = fromSection;
        ToSection = toSection;
        FromLocalPos = fromLocalPos;
        ToLocalPos = toLocalPos;
    }
}