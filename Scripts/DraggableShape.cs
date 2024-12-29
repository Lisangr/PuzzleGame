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
            Debug.LogError("GridSpawner не найден! Убедитесь, что он присутствует на сцене.");
        }
    }// Начало перетаскивания
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // Создаем клон объекта
        cloneShape = Instantiate(gameObject, parentCanvas.transform);
        cloneRectTransform = cloneShape.GetComponent<RectTransform>();

        // Отключаем логику перетаскивания у клона
        DraggableShape cloneDraggable = cloneShape.GetComponent<DraggableShape>();
        cloneDraggable.enabled = false;

        // CanvasGroup для клона — чтобы он не блокировал другие элементы
        cloneShape.GetComponent<CanvasGroup>().blocksRaycasts = false;
        cloneRectTransform.SetAsLastSibling(); // Устанавливаем клон поверх остальных элементов

        // Устанавливаем размер клона таким же, как у оригинала
        cloneRectTransform.sizeDelta = rectTransform.sizeDelta;

        // Обнуляем локальные координаты клона
        cloneRectTransform.localPosition = Vector3.zero;

        // Немедленно устанавливаем позицию клона на место под курсором
        SetClonePosition(eventData);
    }

    // Перетаскивание
    public void OnDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // Обновляем позицию клона в точке курсора
        SetClonePosition(eventData);
    }

    // Окончание перетаскивания
    public void OnEndDrag(PointerEventData eventData)
    {
        // Удаляем клон, если перетаскивание завершено
        if (cloneShape != null)
        {
            Destroy(cloneShape);
        }

        if (isDropped || isLocked) return;

        // Проверяем, попала ли фигура на сетку и если ячейка не занята
        if (currentHoveredCell != null && !currentHoveredCell.IsOccupied)
        {
            Debug.Log("Фигура успешно сброшена на ячейку.");

            // Определяем базовую клетку как точку привязки — центральную точку фигуры
            Vector2Int baseCellPosition = currentHoveredCell.GridPosition;

            // Проверяем возможность привязки всей фигуры перед тем, как её разместить
            if (CanAttachShapePartsToGrid(baseCellPosition))
            {
                // Если привязка успешна, привязываем фигуру и удаляем исходник
                AttachShapePartsToGrid(baseCellPosition);
                DestroyShapesAfterAttach();
                gridSpawner.CheckWinCondition();
                isDropped = true;  // Фигуру можно считать сброшенной только после успешной привязки
            }
            else
            {
                RevertShapeToOriginal();  // Если не удалось привязать, возвращаем исходную фигуру
            }
        }
        else
        {
            Debug.Log("Фигура не попала на сетку.");
            RevertShapeToOriginal();  // Возвращаем фигуру в исходное состояние при неудаче
        }

        currentHoveredCell = null;
    }// Устанавливаем позицию клона под курсором
    private void SetClonePosition(PointerEventData eventData)
    {
        // Преобразуем экранные координаты в мировые координаты
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out Vector2 localPoint))
        {
            // Устанавливаем позицию клона относительно Canvas
            cloneRectTransform.localPosition = localPoint;
        }
    }

    // Привязка частей фигуры к ячейкам сетки с учётом смещения
    private void AttachShapePartsToGrid(Vector2Int baseCellPosition)
    {
        Vector2Int originalFirstPartPosition = shapeData.positions[0];
        Vector2Int offset = baseCellPosition - originalFirstPartPosition;

        // Привязываем каждую часть фигуры к сетке
        foreach (Transform shapeInstance in transform)
        {
            int index = shapeInstance.GetSiblingIndex();
            Vector2Int partOriginalPosition = shapeData.positions[index];
            Vector2Int adjustedPosition = partOriginalPosition + offset;

            GridCellReceiver targetCell = gridSpawner.FindGridCellByPosition(adjustedPosition);

            if (targetCell != null)
            {
                // Окрашиваем ячейку сетки в цвет части фигуры
                Image shapeImage = shapeInstance.GetComponent<Image>();
                if (shapeImage != null)
                {
                    Image cellImage = targetCell.GetComponent<Image>() ?? targetCell.gameObject.AddComponent<Image>();
                    cellImage.color = shapeImage.color;
                }

                // Убираем выделение центральной ячейки
                if (index == 0) // Если это первая (центральная) ячейка
                {
                    RemoveHighlightFromCell(shapeInstance.gameObject);
                }

                // Отмечаем ячейку как занятую
                targetCell.SetOccupied(true);
            }
        }
    }

    // Метод для удаления выделения центральной ячейки после её сброса
    private void RemoveHighlightFromCell(GameObject cell)
    {
        Image cellImage = cell.GetComponent<Image>();

        // Восстанавливаем оригинальный цвет
        if (cellImage != null)
        {
            cellImage.color = shapeData.shapeColor;  // Возвращаем исходный цвет
        }

        // Удаляем символ "X", если он был добавлен
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

        // Проверяем каждую часть фигуры, можно ли её разместить
        foreach (Vector2Int partOriginalPosition in shapeData.positions)
        {
            Vector2Int adjustedPosition = partOriginalPosition + offset;
            GridCellReceiver targetCell = gridSpawner.FindGridCellByPosition(adjustedPosition);

            // Если ячейка занята или отсутствует, привязка невозможна
            if (targetCell == null || targetCell.IsOccupied)
            {
                return false;
            }
        }

        return true; // Все части можно привязать
    }

    // Удаление фигуры после привязки к сетке
    private void DestroyShapesAfterAttach()
    {
        Destroy(gameObject);  // Удаляем оригинальную фигуру
    }

    // Возврат фигуры на оригинальную позицию
    // Возврат фигуры на исходную позицию и восстановление возможности её перетаскивания
    private void RevertShapeToOriginal()
    {
        Debug.Log("Фигура не привязана к сетке. Возвращаем её в исходное состояние.");

        // Обновляем флаг, чтобы разрешить дальнейшее перемещение
        isDropped = false;

        // Фигура снова доступна для перетаскивания
        canvasGroup.blocksRaycasts = true;

        // Удаляем клон фигуры, если он существует
        if (cloneShape != null)
        {
            Destroy(cloneShape);
        }
    }


    // Блокировка фигуры
    public void LockShape()
    {
        Debug.Log("Фигура заблокирована.");
        isLocked = true;
        canvasGroup.blocksRaycasts = false;
    }

    // Проверка, заблокирована ли фигура
    public bool IsLocked()
    {
        return isLocked;
    }

    // Установка ячейки, над которой находится фигура
    public void SetHoveredCell(GridCellReceiver cell)
    {
        currentHoveredCell = cell;
    }

    // Свойство для управления сбросом фигуры
    public bool IsDropped { get; set; } = false;
}