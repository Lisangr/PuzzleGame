using System.Collections.Generic;

[System.Serializable]
public class GridTemplate
{
    public string name;
    public List<GridShape> shapes = new List<GridShape>(); // Добавили список фигур
    public int gridSize;

    public GridTemplate(string name, int gridSize)
    {
        this.name = name;
        this.gridSize = gridSize;
    }
}