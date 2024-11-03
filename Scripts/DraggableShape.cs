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
        // Преобразуем экранные координаты в мировые координаты с учётом камеры Canvas'а
        Vector3 worldMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,  // Учитываем камеру, привязанную к канвасу
            out worldMousePos);

        // Устанавливаем клон в мировую позицию под курсором
        cloneRectTransform.position = worldMousePos;
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
            Debug.LogError("GridSpawner не найден! Убедитесь, что он присутствует на сцене.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // Создаем клон объекта
        cloneShape = Instantiate(gameObject, parentCanvas.transform);
        cloneRectTransform = cloneShape.GetComponent<RectTransform>();

        // Отключаем логику перетаскивания у клона
        DraggableShape cloneDraggable = cloneShape.GetComponent<DraggableShape>();
        cloneDraggable.enabled = false;

        cloneShape.GetComponent<CanvasGroup>().blocksRaycasts = false;

        // Устанавливаем клон в координаты (0, 0)
        cloneRectTransform.localPosition = Vector3.zero;

        // Немедленно обновляем позицию клона в точке под курсором
        SetClonePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // Обновляем позицию клона в точке курсора
        SetClonePosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Удаляем клон, когда заканчивается перетаскивание
        if (cloneShape != null)
        {
            Destroy(cloneShape);
        }

        if (isDropped || isLocked) return;

        if (currentHoveredCell != null && !currentHoveredCell.IsOccupied)
        {
            Debug.Log("Фигура успешно сброшена на ячейку.");
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
            Debug.Log("Фигура не попала на сетку.");
            isDropped = false;
        }
    }

    private void SetClonePosition(PointerEventData eventData)
    {
        // Преобразуем экранные координаты курсора в локальные координаты канваса
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,  // Учитываем камеру, привязанную к канвасу
            out var localMousePos);

        // Устанавливаем позицию клона в локальные координаты, соответствующие положению курсора
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
                Debug.LogWarning($"Не удалось найти ячейку для части фигуры на позиции {position}.");
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
        Debug.Log("Не все части фигуры были успешно привязаны. Отменяем привязку.");
        Destroy(cloneShape);
    }

    public void LockShape()
    {
        Debug.Log("Фигура заблокирована.");
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
    //private float snapRadius = 20f; // Радиус для примагничивания
    public GridShape shapeData; // Ссылка на данные формы
    private GridSpawner gridSpawner; // Ссылка на GridSpawner
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();

        // Находим GridSpawner один раз при запуске
        gridSpawner = FindObjectOfType<GridSpawner>();
        if (gridSpawner == null)
        {
            Debug.LogError("GridSpawner не найден! Убедитесь, что он присутствует на сцене.");
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // Создаем клон фигуры и настраиваем его
        cloneShape = Instantiate(gameObject, parentCanvas.transform);
        cloneRectTransform = cloneShape.GetComponent<RectTransform>();
        cloneRectTransform.sizeDelta = rectTransform.sizeDelta;

        DraggableShape cloneDraggable = cloneShape.GetComponent<DraggableShape>();
        cloneDraggable.enabled = false; // Отключаем перетаскивание на клоне

        cloneShape.GetComponent<CanvasGroup>().blocksRaycasts = false;
        cloneRectTransform.SetAsLastSibling();

    }
    public void OnDrag(PointerEventData eventData)
    {
        if (isDropped || isLocked) return;

        // Перемещаем клон фигуры за курсором с небольшим смещением вверх
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out var globalMousePos);

        // Добавляем смещение по оси Y
        float yOffset = 50f; // Смещение вверх на 50 пикселей
        cloneRectTransform.position = globalMousePos + new Vector3(0, yOffset, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (cloneShape == null)
        {
            Debug.Log("Ошибка: Клон фигуры не найден.");
            return;
        }

        if (TrySnapToGrid(cloneRectTransform.position))
        {
            Debug.Log("Фигура успешно сброшена на сетку.");
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
            Debug.Log("Фигура не попала на сетку. Уничтожаем клон.");
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

            // Обновляем позицию клона, вместо изменения родителя
            cloneRectTransform.position = dropPosition;
            return true;
        }
        return false;
    }
    
    // Метод для привязки частей фигуры к ячейкам сетки
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
                Debug.LogWarning($"Не удалось найти ячейку для части фигуры на позиции {position}.");
                allAttached = false;
            }
        }

        return allAttached;
    }

    // Метод для удаления клона и исходной фигуры после успешного размещения
    private void DestroyShapesAfterAttach()
    {
        Destroy(cloneShape);
        Destroy(gameObject);
    }

    // Метод для возврата фигуры в исходное положение при неудачной привязке
    private void RevertShapeToOriginal()
    {
        Debug.Log("Не все части фигуры были успешно привязаны. Отменяем привязку.");

        foreach (Transform shapeInstance in transform)
        {
            shapeInstance.SetParent(this.transform);
        }

        Destroy(cloneShape);
    }
    public void LockShape()
    {
        Debug.Log("Вызван метод LockShape. Фигура заблокирована.");
        isLocked = true;
        canvasGroup.blocksRaycasts = false;
    }

    public bool IsLocked()
    {
        return isLocked;
    }
    public bool IsDropped { get; set; } = false; // Теперь публичное свойство

}*/