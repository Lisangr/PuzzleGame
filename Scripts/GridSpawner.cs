using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
//using YG;

public class GridSpawner : MonoBehaviour
{
    public GridTemplatesData[] templatesDataArray; // ������ �������� ������ �����
    public Transform gridParent;  // ������������ ������ ��� �����
    public Transform shapesParent;  // ������������ ������ ��� �����
    public GameObject gridCellPrefab; // ������ ������ ��� �����
    public GameObject shapeCellPrefab; // ������ ������ ��� �����
    public int cellSize = 30; // ������ ������ �� �������
    public Button restartButton; // ������ ����������� ������
    private int currentDifficultyLevel = 1;
    public GameObject winPanel; // ������ ��������                               
    private GridCellReceiver[] gridCells; // ���� ��� �������� �����
    private void Start()
    {
        // ��������� ������� ��������� �� PlayerPrefs
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

    // ����� ��� ������������� ����� �� ������ �������� ������ ���������
    public void InitializeGrid(int difficultyLevel)
    {
        if (templatesDataArray == null || templatesDataArray.Length == 0)
        {
            Debug.LogError("������ templatesDataArray ���� ��� �� ���������.");
            return;
        }

        GridTemplate selectedTemplate = GetRandomTemplateForLevel(difficultyLevel);
        if (selectedTemplate != null)
        {
            SpawnGrid(selectedTemplate);
            SpawnShapes(selectedTemplate);

            // ������� ��� ������ ����� �������� �����
            gridCells = gridParent.GetComponentsInChildren<GridCellReceiver>();
        }
        else
        {
            Debug.LogError("������ ��� ���������� ������ ��������� �� ������.");
        }
    }
    // ����� ��� �������� ������������� �����
    public void CheckWinCondition()
    {
        foreach (var cell in gridCells)
        {
            if (!cell.IsOccupied)
            {
                Debug.Log("������� ��������� ������. ���������� ����.");
                return; // ���� ���� ���� �� ���� ��������� ������, ���������� ����
            }
        }

        // ��� ������ ������, ���������� WinPanel
        Debug.Log("��� ������ ������. ���������� ������ ��������.");
        winPanel.SetActive(true);
    }
    // ����� ��� ��������� ���������� ������� ����������� ������� �� �������
    private GridTemplate GetRandomTemplateForLevel(int difficultyLevel)
    {
        int expectedGridSize = difficultyLevel + 3;

        foreach (var templatesData in templatesDataArray)
        {
            List<GridTemplate> filteredTemplates = templatesData.templates.FindAll(template => template.gridSize == expectedGridSize);

            if (filteredTemplates.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, filteredTemplates.Count);
                Debug.Log($"������ ������: {filteredTemplates[randomIndex].name} ��� ����� {expectedGridSize}x{expectedGridSize}.");
                return filteredTemplates[randomIndex];
            }
        }

        Debug.LogWarning($"��� ���������� �������� ��� ������ ��������� {difficultyLevel}. �������� ������ �����: {expectedGridSize}x{expectedGridSize}.");
        return null;
    }
    private void SpawnGrid(GridTemplate template)
    {
        HashSet<Vector2Int> addedPositions = new HashSet<Vector2Int>();

        // ���������� ������� �����, ����� ��������� ����������� �����
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

        // ��������� ����������� ������� �����
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;

        // ��������� �������� ��� ������������� ����� � ������������ �������
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

                    // ������������� ���������� �� ������
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
            draggableShape.shapeData = shape; // ������������� ������ �����

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

                // ������������� ��������� ������� � shapesParent
                shapeTransform.localPosition = new Vector2(position.x * cellSize, position.y * cellSize);

                // ��������� ���������� ��������� ��� ������ ������ (����������� �����)
                if (position == shape.positions[0]) // ������ ������ ������
                {
                    HighlightCentralCell(shapeInstance);
                }
            }
        }
    }
    // ����� ��� ��������� ������ ������ (����������� ����� ������)
    private void HighlightCentralCell(GameObject cell)
    {
        Image cellImage = cell.GetComponent<Image>();

        // �������� ���� �� ����� ����� ��� ��������� �����
        if (cellImage != null)
        {
            cellImage.color = Color.white;  // �������� ����������� ������ ������� ������
        }

        // ��������� ������ "X" � ������ ������, ����� ��������� ���������� �
        GameObject symbol = new GameObject("CentralSymbol");
        symbol.transform.SetParent(cell.transform, false);

        Text symbolText = symbol.AddComponent<Text>();
        symbolText.text = "X";
        symbolText.alignment = TextAnchor.MiddleCenter;
        symbolText.fontSize = 20;
        symbolText.color = Color.white;

        // ����������� RectTransform ��� �������
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
    // ����� ��� ��������� ������ ��������� � ������ ������������� �����
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
    public GridTemplatesData[] templatesDataArray; // ������ �������� ������ �����
    public Transform gridParent;  // ������������ ������ ��� �����
    public Transform shapesParent;  // ������������ ������ ��� �����
    public GameObject gridCellPrefab; // ������ ������ ��� �����
    public GameObject shapeCellPrefab; // ������ ������ ��� �����
    public int cellSize = 30; // ������ ������ �� �������
    public Button restartButton; // ������ ����������� ������
    private int currentDifficultyLevel = 1;
    public GameObject winPanel; // ������ ��������                               
    private GridCellReceiver[] gridCells; // ���� ��� �������� �����
    private void Start()
    {
        // ��������� ������� ��������� �� PlayerPrefs
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

    // ����� ��� ������������� ����� �� ������ �������� ������ ���������
    public void InitializeGrid(int difficultyLevel)
    {
        if (templatesDataArray == null || templatesDataArray.Length == 0)
        {
            Debug.LogError("������ templatesDataArray ���� ��� �� ���������.");
            return;
        }

        GridTemplate selectedTemplate = GetRandomTemplateForLevel(difficultyLevel);
        if (selectedTemplate != null)
        {
            SpawnGrid(selectedTemplate);
            SpawnShapes(selectedTemplate);

            // ������� ��� ������ ����� �������� �����
            gridCells = gridParent.GetComponentsInChildren<GridCellReceiver>();
        }
        else
        {
            Debug.LogError("������ ��� ���������� ������ ��������� �� ������.");
        }
    }
    // ����� ��� �������� ������������� �����
    public void CheckWinCondition()
    {
        foreach (var cell in gridCells)
        {
            if (!cell.IsOccupied)
            {
                Debug.Log("������� ��������� ������. ���������� ����.");
                return; // ���� ���� ���� �� ���� ��������� ������, ���������� ����
            }
        }

        // ��� ������ ������, ���������� WinPanel
        Debug.Log("��� ������ ������. ���������� ������ ��������.");
        winPanel.SetActive(true);
    }
    // ����� ��� ��������� ���������� ������� ����������� ������� �� �������
    private GridTemplate GetRandomTemplateForLevel(int difficultyLevel)
    {
        int expectedGridSize = difficultyLevel + 3;

        foreach (var templatesData in templatesDataArray)
        {
            List<GridTemplate> filteredTemplates = templatesData.templates.FindAll(template => template.gridSize == expectedGridSize);

            if (filteredTemplates.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, filteredTemplates.Count);
                Debug.Log($"������ ������: {filteredTemplates[randomIndex].name} ��� ����� {expectedGridSize}x{expectedGridSize}.");
                return filteredTemplates[randomIndex];
            }
        }

        Debug.LogWarning($"��� ���������� �������� ��� ������ ��������� {difficultyLevel}. �������� ������ �����: {expectedGridSize}x{expectedGridSize}.");
        return null;
    }
    private void SpawnGrid(GridTemplate template)
    {
        HashSet<Vector2Int> addedPositions = new HashSet<Vector2Int>();

        // ���������� ������� �����, ����� ��������� ����������� �����
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

        // ��������� ����������� ������� �����
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;

        // ��������� �������� ��� ������������� ����� � ������������ �������
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

                    // ������������� ���������� �� ������
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
            draggableShape.shapeData = shape; // ������������� ������ �����

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

                // ������������� ��������� ������� � shapesParent
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
    // ����� ��� ��������� ������ ��������� � ������ ������������� �����
    public void SetDifficultyLevel(int difficultyLevel)
    {
        currentDifficultyLevel = difficultyLevel;
        InitializeGrid(currentDifficultyLevel);
    }
}
 */