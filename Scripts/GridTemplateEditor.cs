#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class GridTemplateEditor : EditorWindow
{
    private Color currentColor = Color.red;
    private GridTemplate currentTemplate;
    private int gridSize = 4;
    private bool isDrawingMode = false;
    private GridTemplatesData templatesData;
    private List<Vector2Int> selectedPositions = new List<Vector2Int>();

    [MenuItem("Tools/Grid Template Editor")]
    public static void ShowWindow()
    {
        GetWindow<GridTemplateEditor>("Grid Template Editor");
    }

    private void OnEnable()
    {
        LoadOrCreateTemplateData();
        InitializeNewTemplate();
    }

    private void OnGUI()
    {
        GUILayout.Label("Редактор шаблона сетки", EditorStyles.boldLabel);

        gridSize = EditorGUILayout.IntField("Размер сетки:", gridSize);
        gridSize = Mathf.Clamp(gridSize, 4, 10);

        if (currentTemplate == null || currentTemplate.gridSize != gridSize)
        {
            LoadOrCreateTemplateData();
            InitializeNewTemplate();
        }

        currentColor = EditorGUILayout.ColorField("Текущий цвет:", currentColor);

        if (GUILayout.Button(isDrawingMode ? "Завершить выбор" : "Выбрать ячейки"))
        {
            if (isDrawingMode)
            {
                FinalizeShape(); // Сохраняем фигуру, когда завершаем выбор
            }
            isDrawingMode = !isDrawingMode;
        }

        DisplayGrid();

        GUILayout.Space(10);

        if (GUILayout.Button("Сохранить шаблон"))
        {
            if (templatesData != null)
            {
                templatesData.templates.Add(currentTemplate);
                EditorUtility.SetDirty(templatesData);
                AssetDatabase.SaveAssets();
                Debug.Log($"Шаблон для {gridSize}x{gridSize} сохранен в {templatesData.name}.");
                InitializeNewTemplate();
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Новый шаблон"))
        {
            InitializeNewTemplate();
        }
    }

    private void DisplayGrid()
    {
        GUILayout.BeginVertical();
        for (int y = 0; y < gridSize; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < gridSize; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Color originalColor = GUI.backgroundColor;

                // Проверка, есть ли эта ячейка в сохранённых фигурах
                bool isInSavedShape = false;
                foreach (var shape in currentTemplate.shapes)
                {
                    if (shape.positions.Contains(pos))
                    {
                        isInSavedShape = true;
                        GUI.backgroundColor = shape.shapeColor;
                        break;
                    }
                }

                // Проверка, находится ли эта ячейка в текущем выборе
                if (selectedPositions.Contains(pos))
                {
                    GUI.backgroundColor = currentColor;
                }

                if (GUILayout.Button("", GUILayout.Width(30), GUILayout.Height(30)) && isDrawingMode)
                {
                    if (selectedPositions.Contains(pos))
                    {
                        selectedPositions.Remove(pos);
                    }
                    else
                    {
                        selectedPositions.Add(pos);
                    }
                }

                GUI.backgroundColor = originalColor;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
  
    private void FinalizeShape()
    {
        if (selectedPositions.Count > 0)
        {
            // Создаем новый ScriptableObject для фигуры
            GridShape newShape = ScriptableObject.CreateInstance<GridShape>();

            // Уникальное имя для фигуры
            string baseName = $"CustomShape_{currentTemplate.shapes.Count + 1}";
            string shapeName = baseName;
            int counter = 1;

            // Проверяем, существует ли уже объект с таким именем
            while (AssetDatabase.LoadAssetAtPath<GridShape>($"Assets/{shapeName}.asset") != null)
            {
                shapeName = $"{baseName}_{counter}";
                counter++;
            }

            // Устанавливаем уникальное имя для фигуры
            newShape.shapeName = shapeName;
            newShape.shapeColor = currentColor;
            newShape.positions = new List<Vector2Int>(selectedPositions);

            // Добавляем фигуру в текущий шаблон
            currentTemplate.shapes.Add(newShape);

            // Сохраняем фигуру как отдельный ScriptableObject
            string shapePath = $"Assets/{shapeName}.asset";
            AssetDatabase.CreateAsset(newShape, shapePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Создана новая фигура: {newShape.shapeName} с {selectedPositions.Count} ячейками.");

            // Очищаем выбранные позиции для следующей фигуры
            selectedPositions.Clear();
        }
    }

    private void LoadOrCreateTemplateData()
    {
        string assetPath = $"Assets/GridTemplatesData_{gridSize}x{gridSize}.asset";
        templatesData = AssetDatabase.LoadAssetAtPath<GridTemplatesData>(assetPath);

        if (templatesData == null)
        {
            templatesData = ScriptableObject.CreateInstance<GridTemplatesData>();
            AssetDatabase.CreateAsset(templatesData, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Создан новый ScriptableObject для сетки {gridSize}x{gridSize}.");
        }
    }

    private void InitializeNewTemplate()
    {
        int templateNumber = (templatesData != null) ? templatesData.templates.Count + 1 : 1;
        string templateName = $"Шаблон {templateNumber}";
        currentTemplate = new GridTemplate(templateName, gridSize);
        Debug.Log($"Создан новый шаблон: {templateName} для сетки {gridSize}x{gridSize}.");
    }
}
#endif
















/*
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class GridTemplateEditor : EditorWindow
{
    private Color currentColor = Color.red;
    private GridTemplate currentTemplate;
    private int gridSize = 4;
    private bool isDrawingMode = false;
    private GridTemplatesData templatesData;
    private ShapeType selectedShape = ShapeType.Single;

    [MenuItem("Tools/Grid Template Editor")]
    public static void ShowWindow()
    {
        GetWindow<GridTemplateEditor>("Grid Template Editor");
    }

    private void OnEnable()
    {
        LoadOrCreateTemplateData();
        InitializeNewTemplate();
    }

    private void OnGUI()
    {
        GUILayout.Label("Редактор шаблона сетки", EditorStyles.boldLabel);

        gridSize = EditorGUILayout.IntField("Размер сетки:", gridSize);
        gridSize = Mathf.Clamp(gridSize, 4, 10);

        if (currentTemplate == null || currentTemplate.gridSize != gridSize)
        {
            LoadOrCreateTemplateData();
            InitializeNewTemplate();
        }

        currentColor = EditorGUILayout.ColorField("Текущий цвет:", currentColor);

        selectedShape = (ShapeType)EditorGUILayout.EnumPopup("Тип фигуры:", selectedShape);

        if (GUILayout.Button(isDrawingMode ? "Завершить выбор" : "Выбрать ячейки"))
        {
            isDrawingMode = !isDrawingMode;
        }

        DisplayGrid();

        GUILayout.Space(10);

        if (GUILayout.Button("Сохранить шаблон"))
        {
            if (templatesData != null)
            {
                templatesData.templates.Add(currentTemplate);
                EditorUtility.SetDirty(templatesData);
                AssetDatabase.SaveAssets();
                Debug.Log($"Шаблон для {gridSize}x{gridSize} сохранен в {templatesData.name}.");
                InitializeNewTemplate();
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Новый шаблон"))
        {
            InitializeNewTemplate();
        }
    }

    private void DisplayGrid()
    {
        GUILayout.BeginVertical();
        for (int y = 0; y < gridSize; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < gridSize; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Color originalColor = GUI.backgroundColor;

                // Проверка, есть ли эта ячейка в фигурах
                GridShape containingShape = null;
                foreach (var shape in currentTemplate.shapes)
                {
                    if (shape.positions.Contains(pos))
                    {
                        containingShape = shape;
                        break;
                    }
                }

                if (containingShape != null)
                {
                    GUI.backgroundColor = containingShape.shapeColor;
                }

                if (GUILayout.Button("", GUILayout.Width(30), GUILayout.Height(30)) && isDrawingMode)
                {
                    CreateAndSaveShape(pos);
                }

                GUI.backgroundColor = originalColor;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    private void CreateAndSaveShape(Vector2Int startPos)
    {
        List<Vector2Int> shapePositions = GetShapePositions(startPos);
        GridShape newShape = ScriptableObject.CreateInstance<GridShape>();

        newShape.shapeName = $"{selectedShape} ({startPos.x}, {startPos.y})";
        newShape.shapeColor = currentColor;
        newShape.positions.AddRange(shapePositions);

        currentTemplate.shapes.Add(newShape);

        string shapePath = $"Assets/GridShape_{selectedShape}_{startPos.x}_{startPos.y}.asset";
        AssetDatabase.CreateAsset(newShape, shapePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Создана новая фигура: {newShape.shapeName}.");
    }

    private List<Vector2Int> GetShapePositions(Vector2Int startPos)
    {
        List<Vector2Int> shapePositions = new List<Vector2Int> { startPos };

        switch (selectedShape)
        {
            case ShapeType.Single:
                break;
            case ShapeType.L:
                shapePositions.Add(new Vector2Int(startPos.x + 1, startPos.y));
                shapePositions.Add(new Vector2Int(startPos.x, startPos.y + 1));
                shapePositions.Add(new Vector2Int(startPos.x, startPos.y + 2));
                break;
            case ShapeType.T:
                shapePositions.Add(new Vector2Int(startPos.x - 1, startPos.y));
                shapePositions.Add(new Vector2Int(startPos.x + 1, startPos.y));
                shapePositions.Add(new Vector2Int(startPos.x, startPos.y + 1));
                break;
            case ShapeType.P:
                shapePositions.Add(new Vector2Int(startPos.x, startPos.y + 1));
                shapePositions.Add(new Vector2Int(startPos.x, startPos.y + 2));
                shapePositions.Add(new Vector2Int(startPos.x + 1, startPos.y + 1));
                break;
            case ShapeType.G:
                shapePositions.Add(new Vector2Int(startPos.x + 1, startPos.y));
                shapePositions.Add(new Vector2Int(startPos.x + 2, startPos.y));
                shapePositions.Add(new Vector2Int(startPos.x, startPos.y + 1));
                shapePositions.Add(new Vector2Int(startPos.x, startPos.y + 2));
                break;
        }

        return shapePositions;
    }

    private void LoadOrCreateTemplateData()
    {
        string assetPath = $"Assets/GridTemplatesData_{gridSize}x{gridSize}.asset";
        templatesData = AssetDatabase.LoadAssetAtPath<GridTemplatesData>(assetPath);

        if (templatesData == null)
        {
            templatesData = ScriptableObject.CreateInstance<GridTemplatesData>();
            AssetDatabase.CreateAsset(templatesData, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Создан новый ScriptableObject для сетки {gridSize}x{gridSize}.");
        }
    }

    private void InitializeNewTemplate()
    {
        int templateNumber = (templatesData != null) ? templatesData.templates.Count + 1 : 1;
        string templateName = $"Шаблон {templateNumber}";
        currentTemplate = new GridTemplate(templateName, gridSize);
        Debug.Log($"Создан новый шаблон: {templateName} для сетки {gridSize}x{gridSize}.");
    }

    private enum ShapeType
    {
        Single,
        L,
        T,
        P,
        G
    }
}
#endif
*/