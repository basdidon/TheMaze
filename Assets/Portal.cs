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
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Grid = FindObjectOfType<Grid>();
    }

    public void SetDestination(PortalData portalData)
    {
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
        Debug.Log($"teleport to {Grid.GetCellCenterWorld((Vector3Int)PortalData.ToWorldPos)}");
        user.position = Grid.GetCellCenterWorld((Vector3Int)PortalData.ToWorldPos);
    }
}

public struct PortalData
{
    public Section FromSection { get; }
    public Floor FromFloor => FromSection.Floor;
    public Vector2Int FromLocalPos { get; }
    public Vector2Int FromWorldPos => FromFloor.LocalToWorldPos(FromLocalPos);

    public Section ToSection { get; }
    public Floor ToFloor => ToSection.Floor;
    public Vector2Int ToLocalPos { get; }
    public Vector2Int ToWorldPos => ToFloor.LocalToWorldPos(ToLocalPos);

    public PortalData(Section fromSection, Section toSection, Vector2Int fromLocalPos, Vector2Int toLocalPos)
    {
        FromSection = fromSection;
        ToSection = toSection;
        FromLocalPos = fromLocalPos;
        ToLocalPos = toLocalPos;
    }
}