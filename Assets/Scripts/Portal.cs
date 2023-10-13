using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    public static Grid Grid;
    SpriteRenderer spriteRenderer;

    public PortalData PortalData;

    [ColorUsage(false, true)]
    public Color diffFloorColor;
    [SerializeField] public Sprite diffFloorPortalSprite;
    [ColorUsage(false, true)]
    public Color sameFloorColor;
    [SerializeField] public Sprite sameFloorPortalSprite;

    private void Awake()
    {
        Grid = FindObjectOfType<Grid>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        DOTween.SetTweensCapacity(500, 50);
    }
    
    public void SetDestination(PortalData portalData)
    {
        PortalData = portalData;
        if (portalData.FromFloor == portalData.ToFloor)
        {
            spriteRenderer.sprite = sameFloorPortalSprite;
            spriteRenderer.material.SetColor("_EmissionColor", diffFloorColor);
        }
        else
        {
            spriteRenderer.sprite = diffFloorPortalSprite;
            spriteRenderer.material.SetColor("_EmissionColor", sameFloorColor);

        }
    }

    public void Teleport(Transform user)
    {
        if(MazeRenderer.Instance.RenderMode == MazeRenderer.RenderModes.SINGLE)
        {
            Debug.Log($"{PortalData.FromFloor.FloorIndex} -> {PortalData.ToFloor.FloorIndex} pos: {(Vector3Int)PortalData.ToPos}");
            user.transform.position = Grid.GetCellCenterWorld((Vector3Int)PortalData.ToPos);

            ///** Grid.GetCellCenterWorld((Vector3Int)PortalData.ToLocalPos) after below line got unexpected result. so i teleport player first, then swap floor, let's fix it later.
            MazeRenderer.Instance.RenderFloor(PortalData.ToFloor.FloorIndex);
            Debug.Log($"-> {Grid.GetCellCenterWorld((Vector3Int)PortalData.ToPos)}");
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }

    private void OnEnable()
    {
        Rotate();
    }

    void Rotate()
    {
        transform.DOLocalRotate(new Vector3(0, 0, 360), 30, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetLoops(-1,LoopType.Restart)
            .SetEase(Ease.Linear);
    }
}

[System.Serializable]
public struct PortalData
{
    [field:SerializeField] public Section FromSection { get; private set; }
    public Floor FromFloor => FromSection.Floor;
    [field: SerializeField] public Vector2Int FromPos { get; private set; }

    [field: SerializeField] public Section ToSection { get; private set; }
    public Floor ToFloor => ToSection.Floor;
    [field: SerializeField] public Vector2Int ToPos { get; private set; }

    public PortalData(Section fromSection, Section toSection, Vector2Int fromPos, Vector2Int toPos)
    {
        FromSection = fromSection;
        ToSection = toSection;
        FromPos = fromPos;
        ToPos = toPos;
    }
}