using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridShape", menuName = "Grid/Shape")]
public class GridShape : ScriptableObject
{
    public string shapeName;
    public List<Vector2Int> positions = new List<Vector2Int>();
    public Color shapeColor;
}
