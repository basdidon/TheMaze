using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

[Serializable]
public class Section
{
    public Floor Floor { get; }

    // cells
    readonly List<Vector2Int> sectionCells;
    public IReadOnlyList<Vector2Int> SectionCells => sectionCells;

    // portal
    readonly List<PortalData> portals;
    public IReadOnlyList<PortalData> Portals => portals;

    public Section(Floor floor)
    {
        Floor = floor;
        sectionCells = new();
        portals = new();
    }

    // Modify
    public void AddCell(Vector2Int cellPos) => sectionCells.Add(cellPos);
    public void AddPortal(Vector2Int fromLocalPos, Vector2Int toLocalPos, Section otherSection)
    {
        Debug.Log($"AddCell portal {this} to {otherSection}");
        portals.Add(new(this, otherSection, fromLocalPos, toLocalPos));
    }

    // Query
    public bool IsContain(Vector2Int cellPos) => sectionCells.Contains(cellPos);
    public IEnumerable<Vector2Int> OneWayCells => Floor.OneWayCells.Where(cellPos => IsContain(cellPos));
    public List<Vector2Int> UnuseOneWayCells => OneWayCells.Where(cellPos => !Portals.Any(portal => portal.FromPos == cellPos)).ToList();
    public HashSet<Section> ConnectedSections => portals.Select(portal => portal.ToSection).ToHashSet();

    // toString
    public override string ToString() => $"[{Floor.FloorIndex}:{Floor.GetSectionIdx(this)}]";
}
