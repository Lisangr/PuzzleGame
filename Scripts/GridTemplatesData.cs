using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridTemplatesData", menuName = "Grid/Template Data")]
public class GridTemplatesData : ScriptableObject
{
    public List<GridTemplate> templates = new List<GridTemplate>();
}
