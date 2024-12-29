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
        // ����������� �������� ���������� � ������� ����������
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out Vector2 localPoint))
        {
            // ������������� ������� ����� ������������ Canvas
            cloneRectTransform.localPosition = localPoint;
        }
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