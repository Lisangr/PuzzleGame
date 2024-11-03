using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
//using YG;

public class GridSpawner : MonoBehaviour
{
    public GridTemplatesData[] templatesDataArray; // Массив шаблонов данных сетки
    public Transform gridParent;  // Родительский объект для сетки
    public Transform shapesParent;  // Родительский объект для фигур
    public GameObject gridCellPrefab; // Префаб ячейки для сетки
    public GameObject shapeCellPrefab; // Префаб ячейки для фигур
    public int cellSize = 30; // Размер ячейки на канвасе
    public Button restartButton; // Кнопка перезапуска уровня
    private int currentDifficultyLevel = 1;
    public GameObject winPanel; // Панель выигрыша                               
    private GridCellReceiver[] gridCells; // Поле для хранения ячеек
    private void Start()
    {
        // Загружаем уровень сложности из PlayerPrefs
        if (PlayerPrefs.HasKey("DifficultyLevel"))
        {
            currentDifficultyLevel = PlayerPrefs.GetInt("DifficultyLevel");
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }

        gridCells = gridParent.GetComponentsInChildren<GridCellReceiver>();
        InitializeGrid(currentDifficultyLevel);
    }

    // Метод для инициализации сетки на основе текущего уровня сложности
    public void InitializeGrid(int difficultyLevel)
    {
        if (templatesDataArray == null || templatesDataArray.Length == 0)
        {
            Debug.LogError("Массив templatesDataArray пуст или не определен.");
            return;
        }

        GridTemplate selectedTemplate = GetRandomTemplateForLevel(difficultyLevel);
        if (selectedTemplate != null)
        {
            SpawnGrid(selectedTemplate);
            SpawnShapes(selectedTemplate);

            // Находим все ячейки после создания сетки
            gridCells = gridParent.GetComponentsInChildren<GridCellReceiver>();
        }
        else
        {
            Debug.LogError("Шаблон для указанного уровня сложности не найден.");
        }
    }
    // Метод для проверки заполненности ячеек
    public void CheckWinCondition()
    {
        foreach (var cell in gridCells)
        {
            if (!cell.IsOccupied)
            {
                Debug.Log("Найдена свободная ячейка. Продолжаем игру.");
                return; // Если есть хотя бы одна свободная ячейка, продолжаем игру
            }
        }

        // Все ячейки заняты, отображаем WinPanel
        Debug.Log("Все ячейки заняты. Отображаем панель выигрыша.");
        winPanel.SetActive(true);
    }
    // Метод для получения случайного шаблона подходящего размера из массива
    private GridTemplate GetRandomTemplateForLevel(int difficultyLevel)
    {
        int expectedGridSize = difficultyLevel + 3;

        foreach (var templatesData in templatesDataArray)
        {
            List<GridTemplate> filteredTemplates = templatesData.templates.FindAll(template => template.gridSize == expectedGridSize);

            if (filteredTemplates.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, filteredTemplates.Count);
                Debug.Log($"Выбран шаблон: {filteredTemplates[randomIndex].name} для сетки {expectedGridSize}x{expectedGridSize}.");
                return filteredTemplates[randomIndex];
            }
        }

        Debug.LogWarning($"Нет подходящих шаблонов для уровня сложности {difficultyLevel}. Ожидался размер сетки: {expectedGridSize}x{expectedGridSize}.");
        return null;
    }
    private void SpawnGrid(GridTemplate template)
    {
        HashSet<Vector2Int> addedPositions = new HashSet<Vector2Int>();

        // Определяем границы сетки, чтобы вычислить центральную точку
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var shape in template.shapes)
        {
            foreach (var position in shape.positions)
            {
                if (position.x < minX) minX = position.x;
                if (position.y < minY) minY = position.y;
                if (position.x > maxX) maxX = position.x;
                if (position.y > maxY) maxY = position.y;
            }
        }

        // Вычисляем центральную позицию сетки
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;

        // Вычисляем смещение для центрирования сетки в родительском объекте
        Vector2 gridOffset = new Vector2(-centerX * cellSize, -centerY * cellSize);

        foreach (var shape in template.shapes)
        {
            foreach (var position in shape.positions)
            {
                if (!addedPositions.Contains(position))
                {
                    addedPositions.Add(position);

                    GameObject gridCellInstance = Instantiate(gridCellPrefab, gridParent);
                    RectTransform cellTransform = gridCellInstance.GetComponent<RectTransform>();
                    cellTransform.anchoredPosition = new Vector2(position.x * cellSize, position.y * cellSize) + gridOffset;
                    cellTransform.sizeDelta = new Vector2(cellSize, cellSize);

                    Image cellImage = gridCellInstance.GetComponent<Image>();
                    if (cellImage != null)
                    {
                        cellImage.color = Color.gray;
                    }

                    // Устанавливаем координаты на ячейке
                    GridCellReceiver cellReceiver = gridCellInstance.GetComponent<GridCellReceiver>();
                    if (cellReceiver != null)
                    {
                        cellReceiver.GridPosition = position;
                    }
                }
            }
        }
    }

    private void SpawnShapes(GridTemplate template)
    {
        foreach (var shape in template.shapes)
        {
            GameObject shapeContainer = new GameObject(shape.shapeName);
            shapeContainer.transform.SetParent(shapesParent, false);

            RectTransform containerTransform = shapeContainer.AddComponent<RectTransform>();
            containerTransform.anchoredPosition = new Vector2(shape.positions[0].x * cellSize, shape.positions[0].y * cellSize);

            CanvasGroup canvasGroup = shapeContainer.AddComponent<CanvasGroup>();
            DraggableShape draggableShape = shapeContainer.AddComponent<DraggableShape>();
            draggableShape.shapeData = shape; // Устанавливаем данные формы

            foreach (var position in shape.positions)
            {
                GameObject shapeInstance = Instantiate(shapeCellPrefab, shapeContainer.transform);

                Image shapeImage = shapeInstance.GetComponent<Image>();
                if (shapeImage != null)
                {
                    shapeImage.color = shape.shapeColor;
                }

                RectTransform shapeTransform = shapeInstance.GetComponent<RectTransform>();
                shapeTransform.anchoredPosition = new Vector2((position.x - shape.positions[0].x) * cellSize, (position.y - shape.positions[0].y) * cellSize);
                shapeTransform.sizeDelta = new Vector2(cellSize, cellSize);

                // Устанавливаем начальные позиции в shapesParent
                shapeTransform.localPosition = new Vector2(position.x * cellSize, position.y * cellSize);

                // Добавляем визуальное выделение для первой ячейки (центральная точка)
                if (position == shape.positions[0]) // Первая ячейка фигуры
                {
                    HighlightCentralCell(shapeInstance);
                }
            }
        }
    }
    // Метод для выделения первой ячейки (центральной точки фигуры)
    private void HighlightCentralCell(GameObject cell)
    {
        Image cellImage = cell.GetComponent<Image>();

        // Изменяем цвет на более яркий или добавляем рамку
        if (cellImage != null)
        {
            cellImage.color = Color.white;  // Выделяем центральную ячейку красным цветом
        }

        // Добавляем символ "X" в первую ячейку, чтобы визуально обозначить её
        GameObject symbol = new GameObject("CentralSymbol");
        symbol.transform.SetParent(cell.transform, false);

        Text symbolText = symbol.AddComponent<Text>();
        symbolText.text = "X";
        symbolText.alignment = TextAnchor.MiddleCenter;
        symbolText.fontSize = 20;
        symbolText.color = Color.white;

        // Настраиваем RectTransform для символа
        RectTransform symbolTransform = symbol.GetComponent<RectTransform>();
        symbolTransform.sizeDelta = cell.GetComponent<RectTransform>().sizeDelta;
    }

    public GridCellReceiver FindGridCellByPosition(Vector2Int position)
    {
        foreach (Transform cell in gridParent)
        {
            GridCellReceiver cellReceiver = cell.GetComponent<GridCellReceiver>();
            if (cellReceiver != null && cellReceiver.GridPosition == position)
            {
                return cellReceiver;
            }
        }
        return null;
    }

    public void RestartLevel()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in shapesParent)
        {
            Destroy(child.gameObject);
        }

        InitializeGrid(currentDifficultyLevel);
    }

    public void NextLevel()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in shapesParent)
        {
            Destroy(child.gameObject);
        }

        InitializeGrid(currentDifficultyLevel);
        winPanel.SetActive(false);

        int lvl = PlayerPrefs.GetInt("Levels",0);
        PlayerPrefs.SetInt("Levels", lvl + 1);
        PlayerPrefs.Save();

        //YandexGame.savesData.levels = lvl;
        //YandexGame.SaveProgress();

        //YandexGame.NewLeaderboardScores("Levels", lvl);

    }
    // Метод для установки уровня сложности и вызова инициализации сетки
    public void SetDifficultyLevel(int difficultyLevel)
    {
        currentDifficultyLevel = difficultyLevel;
        InitializeGrid(currentDifficultyLevel);
    }
}


/*
 using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
//using YG;

public class GridSpawner : MonoBehaviour
{
    public GridTemplatesData[] templatesDataArray; // Массив шаблонов данных сетки
    public Transform gridParent;  // Родительский объект для сетки
    public Transform shapesParent;  // Родительский объект для фигур
    public GameObject gridCellPrefab; // Префаб ячейки для сетки
    public GameObject shapeCellPrefab; // Префаб ячейки для фигур
    public int cellSize = 30; // Размер ячейки на канвасе
    public Button restartButton; // Кнопка перезапуска уровня
    private int currentDifficultyLevel = 1;
    public GameObject winPanel; // Панель выигрыша                               
    private GridCellReceiver[] gridCells; // Поле для хранения ячеек
    private void Start()
    {
        // Загружаем уровень сложности из PlayerPrefs
        if (PlayerPrefs.HasKey("DifficultyLevel"))
        {
            currentDifficultyLevel = PlayerPrefs.GetInt("DifficultyLevel");
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }

        gridCells = gridParent.GetComponentsInChildren<GridCellReceiver>();
        InitializeGrid(currentDifficultyLevel);
    }

    // Метод для инициализации сетки на основе текущего уровня сложности
    public void InitializeGrid(int difficultyLevel)
    {
        if (templatesDataArray == null || templatesDataArray.Length == 0)
        {
            Debug.LogError("Массив templatesDataArray пуст или не определен.");
            return;
        }

        GridTemplate selectedTemplate = GetRandomTemplateForLevel(difficultyLevel);
        if (selectedTemplate != null)
        {
            SpawnGrid(selectedTemplate);
            SpawnShapes(selectedTemplate);

            // Находим все ячейки после создания сетки
            gridCells = gridParent.GetComponentsInChildren<GridCellReceiver>();
        }
        else
        {
            Debug.LogError("Шаблон для указанного уровня сложности не найден.");
        }
    }
    // Метод для проверки заполненности ячеек
    public void CheckWinCondition()
    {
        foreach (var cell in gridCells)
        {
            if (!cell.IsOccupied)
            {
                Debug.Log("Найдена свободная ячейка. Продолжаем игру.");
                return; // Если есть хотя бы одна свободная ячейка, продолжаем игру
            }
        }

        // Все ячейки заняты, отображаем WinPanel
        Debug.Log("Все ячейки заняты. Отображаем панель выигрыша.");
        winPanel.SetActive(true);
    }
    // Метод для получения случайного шаблона подходящего размера из массива
    private GridTemplate GetRandomTemplateForLevel(int difficultyLevel)
    {
        int expectedGridSize = difficultyLevel + 3;

        foreach (var templatesData in templatesDataArray)
        {
            List<GridTemplate> filteredTemplates = templatesData.templates.FindAll(template => template.gridSize == expectedGridSize);

            if (filteredTemplates.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, filteredTemplates.Count);
                Debug.Log($"Выбран шаблон: {filteredTemplates[randomIndex].name} для сетки {expectedGridSize}x{expectedGridSize}.");
                return filteredTemplates[randomIndex];
            }
        }

        Debug.LogWarning($"Нет подходящих шаблонов для уровня сложности {difficultyLevel}. Ожидался размер сетки: {expectedGridSize}x{expectedGridSize}.");
        return null;
    }
    private void SpawnGrid(GridTemplate template)
    {
        HashSet<Vector2Int> addedPositions = new HashSet<Vector2Int>();

        // Определяем границы сетки, чтобы вычислить центральную точку
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var shape in template.shapes)
        {
            foreach (var position in shape.positions)
            {
                if (position.x < minX) minX = position.x;
                if (position.y < minY) minY = position.y;
                if (position.x > maxX) maxX = position.x;
                if (position.y > maxY) maxY = position.y;
            }
        }

        // Вычисляем центральную позицию сетки
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;

        // Вычисляем смещение для центрирования сетки в родительском объекте
        Vector2 gridOffset = new Vector2(-centerX * cellSize, -centerY * cellSize);

        foreach (var shape in template.shapes)
        {
            foreach (var position in shape.positions)
            {
                if (!addedPositions.Contains(position))
                {
                    addedPositions.Add(position);

                    GameObject gridCellInstance = Instantiate(gridCellPrefab, gridParent);
                    RectTransform cellTransform = gridCellInstance.GetComponent<RectTransform>();
                    cellTransform.anchoredPosition = new Vector2(position.x * cellSize, position.y * cellSize) + gridOffset;
                    cellTransform.sizeDelta = new Vector2(cellSize, cellSize);

                    Image cellImage = gridCellInstance.GetComponent<Image>();
                    if (cellImage != null)
                    {
                        cellImage.color = Color.gray;
                    }

                    // Устанавливаем координаты на ячейке
                    GridCellReceiver cellReceiver = gridCellInstance.GetComponent<GridCellReceiver>();
                    if (cellReceiver != null)
                    {
                        cellReceiver.GridPosition = position;
                    }
                }
            }
        }
    }

    private void SpawnShapes(GridTemplate template)
    {
        foreach (var shape in template.shapes)
        {
            GameObject shapeContainer = new GameObject(shape.shapeName);
            shapeContainer.transform.SetParent(shapesParent, false);

            RectTransform containerTransform = shapeContainer.AddComponent<RectTransform>();
            containerTransform.anchoredPosition = new Vector2(shape.positions[0].x * cellSize, shape.positions[0].y * cellSize);

            CanvasGroup canvasGroup = shapeContainer.AddComponent<CanvasGroup>();
            DraggableShape draggableShape = shapeContainer.AddComponent<DraggableShape>();
            draggableShape.shapeData = shape; // Устанавливаем данные формы

            foreach (var position in shape.positions)
            {
                GameObject shapeInstance = Instantiate(shapeCellPrefab, shapeContainer.transform);

                Image shapeImage = shapeInstance.GetComponent<Image>();
                if (shapeImage != null)
                {
                    shapeImage.color = shape.shapeColor;
                }

                RectTransform shapeTransform = shapeInstance.GetComponent<RectTransform>();
                shapeTransform.anchoredPosition = new Vector2((position.x - shape.positions[0].x) * cellSize, (position.y - shape.positions[0].y) * cellSize);
                shapeTransform.sizeDelta = new Vector2(cellSize, cellSize);

                // Устанавливаем начальные позиции в shapesParent
                shapeTransform.localPosition = new Vector2(position.x * cellSize, position.y * cellSize);
            }
        }
    }

    public GridCellReceiver FindGridCellByPosition(Vector2Int position)
    {
        foreach (Transform cell in gridParent)
        {
            GridCellReceiver cellReceiver = cell.GetComponent<GridCellReceiver>();
            if (cellReceiver != null && cellReceiver.GridPosition == position)
            {
                return cellReceiver;
            }
        }
        return null;
    }

    public void RestartLevel()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in shapesParent)
        {
            Destroy(child.gameObject);
        }

        InitializeGrid(currentDifficultyLevel);
    }

    public void NextLevel()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in shapesParent)
        {
            Destroy(child.gameObject);
        }

        InitializeGrid(currentDifficultyLevel);
        winPanel.SetActive(false);

        int lvl = PlayerPrefs.GetInt("Levels",0);
        PlayerPrefs.SetInt("Levels", lvl + 1);
        PlayerPrefs.Save();

        //YandexGame.savesData.levels = lvl;
        //YandexGame.SaveProgress();

        //YandexGame.NewLeaderboardScores("Levels", lvl);

    }
    // Метод для установки уровня сложности и вызова инициализации сетки
    public void SetDifficultyLevel(int difficultyLevel)
    {
        currentDifficultyLevel = difficultyLevel;
        InitializeGrid(currentDifficultyLevel);
    }
}
 */