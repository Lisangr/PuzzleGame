using UnityEngine;
using UnityEngine.EventSystems;

public class GridCellReceiver : MonoBehaviour, IDropHandler//, IPointerEnterHandler, IPointerExitHandler, 
{
    public Vector2Int GridPosition { get; set; }

    [SerializeField]
    private bool isOccupied = false;

    public bool IsOccupied
    {
        get { return isOccupied; }
        private set { isOccupied = value; }
    }

    public void SetOccupied(bool occupied)
    {
        IsOccupied = occupied;
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableShape draggable = eventData.pointerDrag.GetComponent<DraggableShape>();
        if (draggable != null && !draggable.IsLocked())
        {
            draggable.SetHoveredCell(this);
            draggable.IsDropped = true;

            GridSpawner spawner = FindObjectOfType<GridSpawner>();
            spawner.CheckWinCondition();
        }
    }
}



/*using UnityEngine.EventSystems;
using UnityEngine;

public class GridCellReceiver : MonoBehaviour, IDropHandler
{
    public Vector2Int GridPosition { get; set; }

    [SerializeField]
    private bool isOccupied = false;

    public bool IsOccupied
    {
        get { return isOccupied; }
        private set { isOccupied = value; }
    }

    public void SetOccupied(bool occupied)
    {
        IsOccupied = occupied;
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        DraggableShape draggable = eventData.pointerDrag.GetComponent<DraggableShape>();
        if (draggable != null && !draggable.IsLocked())
        {
            // Проверяем попадание на ячейку, не перемещая её в дочерний
            IsOccupied = true;
            draggable.IsDropped = true;

            // Проверяем условие выигрыша
            GridSpawner spawner = FindObjectOfType<GridSpawner>();
            spawner.CheckWinCondition();
        }
    }
    
}*/