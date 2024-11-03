using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    private GameObject cloneShape;
    private RectTransform cloneRectTransform;
    private bool isDropped = false;
    private bool isLocked = false;
    public GridShape shapeData;
    private GridSpawner gridSpawner;
    private GridCellReceiver currentHoveredCell = null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();

        gridSpawner = FindObjectOfType<GridSpawner>();
        if (gridSpawner == null)
        {
            Debug.LogError("GridSpawner �� ������! ���������, ��� �� ������������ �� �����.");
        }
    }// ������ ��������������
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // ������� ���� �������
        cloneShape = Instantiate(gameObject, parentCanvas.transform);
        cloneRectTransform = cloneShape.GetComponent<RectTransform>();

        // ��������� ������ �������������� � �����
        DraggableShape cloneDraggable = cloneShape.GetComponent<DraggableShape>();
        cloneDraggable.enabled = false;

        // CanvasGroup ��� ����� � ����� �� �� ���������� ������ ��������
        cloneShape.GetComponent<CanvasGroup>().blocksRaycasts = false;
        cloneRectTransform.SetAsLastSibling(); // ������������� ���� ������ ��������� ���������

        // ������������� ������ ����� ����� ��, ��� � ���������
        cloneRectTransform.sizeDelta = rectTransform.sizeDelta;

        // �������� ��������� ���������� �����
        cloneRectTransform.localPosition = Vector3.zero;

        // ���������� ������������� ������� ����� �� ����� ��� ��������
        SetClonePosition(eventData);
    }

    // ��������������
    public void OnDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // ��������� ������� ����� � ����� �������
        SetClonePosition(eventData);
    }

    // ��������� ��������������
    public void OnEndDrag(PointerEventData eventData)
    {
        // ������� ����, ���� �������������� ���������
        if (cloneShape != null)
        {
            Destroy(cloneShape);
        }

        if (isDropped || isLocked) return;

        // ���������, ������ �� ������ �� ����� � ���� ������ �� ������
        if (currentHoveredCell != null && !currentHoveredCell.IsOccupied)
        {
            Debug.Log("������ ������� �������� �� ������.");

            // ���������� ������� ������ ��� ����� �������� � ����������� ����� ������
            Vector2Int baseCellPosition = currentHoveredCell.GridPosition;

            // ��������� ����������� �������� ���� ������ ����� ���, ��� � ����������
            if (CanAttachShapePartsToGrid(baseCellPosition))
            {
                // ���� �������� �������, ����������� ������ � ������� ��������
                AttachShapePartsToGrid(baseCellPosition);
                DestroyShapesAfterAttach();
                gridSpawner.CheckWinCondition();
                isDropped = true;  // ������ ����� ������� ���������� ������ ����� �������� ��������
            }
            else
            {
                RevertShapeToOriginal();  // ���� �� ������� ���������, ���������� �������� ������
            }
        }
        else
        {
            Debug.Log("������ �� ������ �� �����.");
            RevertShapeToOriginal();  // ���������� ������ � �������� ��������� ��� �������
        }

        currentHoveredCell = null;
    }// ������������� ������� ����� ��� ��������
    private void SetClonePosition(PointerEventData eventData)
    {
        // ����������� �������� ���������� � ������� ���������� � ������ ������ Canvas'�
        Vector3 worldMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,  // ��������� ������, ����������� � �������
            out worldMousePos);

        // ������������� ���� � ������� ������� ��� ��������
        cloneRectTransform.position = worldMousePos;
    }
    // �������� ������ ������ � ������� ����� � ������ ��������
    private void AttachShapePartsToGrid(Vector2Int baseCellPosition)
    {
        Vector2Int originalFirstPartPosition = shapeData.positions[0];
        Vector2Int offset = baseCellPosition - originalFirstPartPosition;

        // ����������� ������ ����� ������ � �����
        foreach (Transform shapeInstance in transform)
        {
            int index = shapeInstance.GetSiblingIndex();
            Vector2Int partOriginalPosition = shapeData.positions[index];
            Vector2Int adjustedPosition = partOriginalPosition + offset;

            GridCellReceiver targetCell = gridSpawner.FindGridCellByPosition(adjustedPosition);

            if (targetCell != null)
            {
                // ���������� ������ ����� � ���� ����� ������
                Image shapeImage = shapeInstance.GetComponent<Image>();
                if (shapeImage != null)
                {
                    Image cellImage = targetCell.GetComponent<Image>() ?? targetCell.gameObject.AddComponent<Image>();
                    cellImage.color = shapeImage.color;
                }

                // ������� ��������� ����������� ������
                if (index == 0) // ���� ��� ������ (�����������) ������
                {
                    RemoveHighlightFromCell(shapeInstance.gameObject);
                }

                // �������� ������ ��� �������
                targetCell.SetOccupied(true);
            }
        }
    }

    // ����� ��� �������� ��������� ����������� ������ ����� � ������
    private void RemoveHighlightFromCell(GameObject cell)
    {
        Image cellImage = cell.GetComponent<Image>();

        // ��������������� ������������ ����
        if (cellImage != null)
        {
            cellImage.color = shapeData.shapeColor;  // ���������� �������� ����
        }

        // ������� ������ "X", ���� �� ��� ��������
        Transform symbol = cell.transform.Find("CentralSymbol");
        if (symbol != null)
        {
            Destroy(symbol.gameObject);
        }
    }

    private bool CanAttachShapePartsToGrid(Vector2Int baseCellPosition)
    {
        Vector2Int originalFirstPartPosition = shapeData.positions[0];
        Vector2Int offset = baseCellPosition - originalFirstPartPosition;

        // ��������� ������ ����� ������, ����� �� � ����������
        foreach (Vector2Int partOriginalPosition in shapeData.positions)
        {
            Vector2Int adjustedPosition = partOriginalPosition + offset;
            GridCellReceiver targetCell = gridSpawner.FindGridCellByPosition(adjustedPosition);

            // ���� ������ ������ ��� �����������, �������� ����������
            if (targetCell == null || targetCell.IsOccupied)
            {
                return false;
            }
        }

        return true; // ��� ����� ����� ���������
    }

    // �������� ������ ����� �������� � �����
    private void DestroyShapesAfterAttach()
    {
        Destroy(gameObject);  // ������� ������������ ������
    }

    // ������� ������ �� ������������ �������
    // ������� ������ �� �������� ������� � �������������� ����������� � ��������������
    private void RevertShapeToOriginal()
    {
        Debug.Log("������ �� ��������� � �����. ���������� � � �������� ���������.");

        // ��������� ����, ����� ��������� ���������� �����������
        isDropped = false;

        // ������ ����� �������� ��� ��������������
        canvasGroup.blocksRaycasts = true;

        // ������� ���� ������, ���� �� ����������
        if (cloneShape != null)
        {
            Destroy(cloneShape);
        }
    }


    // ���������� ������
    public void LockShape()
    {
        Debug.Log("������ �������������.");
        isLocked = true;
        canvasGroup.blocksRaycasts = false;
    }

    // ��������, ������������� �� ������
    public bool IsLocked()
    {
        return isLocked;
    }

    // ��������� ������, ��� ������� ��������� ������
    public void SetHoveredCell(GridCellReceiver cell)
    {
        currentHoveredCell = cell;
    }

    // �������� ��� ���������� ������� ������
    public bool IsDropped { get; set; } = false;
}




















/*
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    private GameObject cloneShape;
    private RectTransform cloneRectTransform;
    private bool isDropped = false;
    private bool isLocked = false;
    public GridShape shapeData;
    private GridSpawner gridSpawner;
    private GridCellReceiver currentHoveredCell = null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();

        gridSpawner = FindObjectOfType<GridSpawner>();
        if (gridSpawner == null)
        {
            Debug.LogError("GridSpawner �� ������! ���������, ��� �� ������������ �� �����.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // ������� ���� �������
        cloneShape = Instantiate(gameObject, parentCanvas.transform);
        cloneRectTransform = cloneShape.GetComponent<RectTransform>();

        // ��������� ������ �������������� � �����
        DraggableShape cloneDraggable = cloneShape.GetComponent<DraggableShape>();
        cloneDraggable.enabled = false;

        cloneShape.GetComponent<CanvasGroup>().blocksRaycasts = false;

        // ������������� ���� � ���������� (0, 0)
        cloneRectTransform.localPosition = Vector3.zero;

        // ���������� ��������� ������� ����� � ����� ��� ��������
        SetClonePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // ��������� ������� ����� � ����� �������
        SetClonePosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // ������� ����, ����� ������������� ��������������
        if (cloneShape != null)
        {
            Destroy(cloneShape);
        }

        if (isDropped || isLocked) return;

        if (currentHoveredCell != null && !currentHoveredCell.IsOccupied)
        {
            Debug.Log("������ ������� �������� �� ������.");
            currentHoveredCell.SetOccupied(true);
            cloneRectTransform.position = currentHoveredCell.transform.position;
            isDropped = true;

            if (AttachShapePartsToGrid())
            {
                DestroyShapesAfterAttach();
                gridSpawner.CheckWinCondition();
            }
            else
            {
                RevertShapeToOriginal();
            }
        }
        else
        {
            Debug.Log("������ �� ������ �� �����.");
            isDropped = false;
        }
    }

    private void SetClonePosition(PointerEventData eventData)
    {
        // ����������� �������� ���������� ������� � ��������� ���������� �������
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,  // ��������� ������, ����������� � �������
            out var localMousePos);

        // ������������� ������� ����� � ��������� ����������, ��������������� ��������� �������
        cloneRectTransform.localPosition = localMousePos;
    }

    private bool AttachShapePartsToGrid()
    {
        bool allAttached = true;

        foreach (Transform shapeInstance in transform)
        {
            int index = shapeInstance.GetSiblingIndex();
            Vector2Int position = shapeData.positions[index];
            GridCellReceiver targetCell = gridSpawner.FindGridCellByPosition(position);

            if (targetCell != null)
            {
                Image shapeImage = shapeInstance.GetComponent<Image>();
                if (shapeImage != null)
                {
                    Image cellImage = targetCell.GetComponent<Image>() ?? targetCell.gameObject.AddComponent<Image>();
                    cellImage.color = shapeImage.color;
                }

                targetCell.SetOccupied(true);
            }
            else
            {
                Debug.LogWarning($"�� ������� ����� ������ ��� ����� ������ �� ������� {position}.");
                allAttached = false;
            }
        }

        return allAttached;
    }

    private void DestroyShapesAfterAttach()
    {
        Destroy(cloneShape);
        Destroy(gameObject);
    }

    private void RevertShapeToOriginal()
    {
        Debug.Log("�� ��� ����� ������ ���� ������� ���������. �������� ��������.");
        Destroy(cloneShape);
    }

    public void LockShape()
    {
        Debug.Log("������ �������������.");
        isLocked = true;
        canvasGroup.blocksRaycasts = false;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    public bool IsDropped { get; set; } = false;

    public void SetHoveredCell(GridCellReceiver cell)
    {
        currentHoveredCell = cell;
    }
}
*/








/*using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    private GameObject cloneShape;
    private RectTransform cloneRectTransform;
    private bool isDropped = false;
    private bool isLocked = false;
    //private float snapRadius = 20f; // ������ ��� ���������������
    public GridShape shapeData; // ������ �� ������ �����
    private GridSpawner gridSpawner; // ������ �� GridSpawner
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();

        // ������� GridSpawner ���� ��� ��� �������
        gridSpawner = FindObjectOfType<GridSpawner>();
        if (gridSpawner == null)
        {
            Debug.LogError("GridSpawner �� ������! ���������, ��� �� ������������ �� �����.");
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // ������� ���� ������ � ����������� ���
        cloneShape = Instantiate(gameObject, parentCanvas.transform);
        cloneRectTransform = cloneShape.GetComponent<RectTransform>();
        cloneRectTransform.sizeDelta = rectTransform.sizeDelta;

        DraggableShape cloneDraggable = cloneShape.GetComponent<DraggableShape>();
        cloneDraggable.enabled = false; // ��������� �������������� �� �����

        cloneShape.GetComponent<CanvasGroup>().blocksRaycasts = false;
        cloneRectTransform.SetAsLastSibling();

    }
    public void OnDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // ���������� ���� ������ �� �������� � ��������� ��������� �����
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out var globalMousePos);

        // ��������� �������� �� ��� Y
        float yOffset = 50f; // �������� ����� �� 50 ��������
        cloneRectTransform.position = globalMousePos + new Vector3(0, yOffset, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (cloneShape == null)
        {
            Debug.Log("������: ���� ������ �� ������.");
            return;
        }

        if (TrySnapToGrid(cloneRectTransform.position))
        {
            Debug.Log("������ ������� �������� �� �����.");
            isDropped = true;

            if (AttachShapePartsToGrid())
            {
                DestroyShapesAfterAttach();
                gridSpawner.CheckWinCondition();
            }
            else
            {
                RevertShapeToOriginal();
            }
        }
        else
        {
            Debug.Log("������ �� ������ �� �����. ���������� ����.");
            Destroy(cloneShape);
            isDropped = false;
        }
    }
   
    private bool TrySnapToGrid(Vector3 dropPosition)
    {
        GridCellReceiver[] gridCells = FindObjectsOfType<GridCellReceiver>();
        List<GridCellReceiver> matchingCells = new List<GridCellReceiver>();

        foreach (var position in shapeData.positions)
        {
            foreach (var cell in gridCells)
            {
                if (cell.GridPosition == position && !cell.IsOccupied)
                {
                    matchingCells.Add(cell);
                    break;
                }
            }
        }

        if (matchingCells.Count == shapeData.positions.Count)
        {
            foreach (var cell in matchingCells)
            {
                cell.SetOccupied(true);
            }

            // ��������� ������� �����, ������ ��������� ��������
            cloneRectTransform.position = dropPosition;
            return true;
        }
        return false;
    }
    
    // ����� ��� �������� ������ ������ � ������� �����
    private bool AttachShapePartsToGrid()
    {
        bool allAttached = true;

        foreach (Transform shapeInstance in transform)
        {
            int index = shapeInstance.GetSiblingIndex();
            Vector2Int position = shapeData.positions[index];
            GridCellReceiver targetCell = gridSpawner.FindGridCellByPosition(position);

            if (targetCell != null)
            {
                Image shapeImage = shapeInstance.GetComponent<Image>();
                if (shapeImage != null)
                {
                    Image cellImage = targetCell.GetComponent<Image>() ?? targetCell.gameObject.AddComponent<Image>();
                    cellImage.color = shapeImage.color;
                }

                targetCell.SetOccupied(true);
            }
            else
            {
                Debug.LogWarning($"�� ������� ����� ������ ��� ����� ������ �� ������� {position}.");
                allAttached = false;
            }
        }

        return allAttached;
    }

    // ����� ��� �������� ����� � �������� ������ ����� ��������� ����������
    private void DestroyShapesAfterAttach()
    {
        Destroy(cloneShape);
        Destroy(gameObject);
    }

    // ����� ��� �������� ������ � �������� ��������� ��� ��������� ��������
    private void RevertShapeToOriginal()
    {
        Debug.Log("�� ��� ����� ������ ���� ������� ���������. �������� ��������.");

        foreach (Transform shapeInstance in transform)
        {
            shapeInstance.SetParent(this.transform);
        }

        Destroy(cloneShape);
    }
    public void LockShape()
    {
        Debug.Log("������ ����� LockShape. ������ �������������.");
        isLocked = true;
        canvasGroup.blocksRaycasts = false;
    }

    public bool IsLocked()
    {
        return isLocked;
    }
    public bool IsDropped { get; set; } = false; // ������ ��������� ��������

}*/