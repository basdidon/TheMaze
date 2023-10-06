using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Stair
{
    public Vector2Int LocalCellPos { get; }

    public Vector2Int TargetWorldPos => TargetFloor.LocalToWorldPos(LocalCellPos);
    public Section TargetSection => TargetFloor.GetSection(TargetWorldPos);
    public Floor TargetFloor { get; }

    public Stair(Vector2Int localPos, Floor floor)
    {
        LocalCellPos = localPos;
        TargetFloor = floor;
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