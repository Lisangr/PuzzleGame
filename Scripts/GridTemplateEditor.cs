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
        GUILayout.Label("�������� ������� �����", EditorStyles.boldLabel);

        gridSize = EditorGUILayout.IntField("������ �����:", gridSize);
        gridSize = Mathf.Clamp(gridSize, 4, 10);

        if (currentTemplate == null || currentTemplate.gridSize != gridSize)
        {
            LoadOrCreateTemplateData();
            InitializeNewTemplate();
        }

        currentColor = EditorGUILayout.ColorField("������� ����:", currentColor);

        if (GUILayout.Button(isDrawingMode ? "��������� �����" : "������� ������"))
        {
            if (isDrawingMode)
            {
                FinalizeShape(); // ��������� ������, ����� ��������� �����
            }
            isDrawingMode = !isDrawingMode;
        }

        DisplayGrid();

        GUILayout.Space(10);

        if (GUILayout.Button("��������� ������"))
        {
            if (templatesData != null)
            {
                templatesData.templates.Add(currentTemplate);
                EditorUtility.SetDirty(templatesData);
                AssetDatabase.SaveAssets();
                Debug.Log($"������ ��� {gridSize}x{gridSize} �������� � {templatesData.name}.");
                InitializeNewTemplate();
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("����� ������"))
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

                // ��������, ���� �� ��� ������ � ���������� �������
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

                // ��������, ��������� �� ��� ������ � ������� ������
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
            // ������� ����� ScriptableObject ��� ������
            GridShape newShape = ScriptableObject.CreateInstance<GridShape>();

            // ���������� ��� ��� ������
            string baseName = $"CustomShape_{currentTemplate.shapes.Count + 1}";
            string shapeName = baseName;
            int counter = 1;

            // ���������, ���������� �� ��� ������ � ����� ������
            while (AssetDatabase.LoadAssetAtPath<GridShape>($"Assets/{shapeName}.asset") != null)
            {
                shapeName = $"{baseName}_{counter}";
                counter++;
            }

            // ������������� ���������� ��� ��� ������
            newShape.shapeName = shapeName;
            newShape.shapeColor = currentColor;
            newShape.positions = new List<Vector2Int>(selectedPositions);

            // ��������� ������ � ������� ������
            currentTemplate.shapes.Add(newShape);

            // ��������� ������ ��� ��������� ScriptableObject
            string shapePath = $"Assets/{shapeName}.asset";
            AssetDatabase.CreateAsset(newShape, shapePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"������� ����� ������: {newShape.shapeName} � {selectedPositions.Count} ��������.");

            // ������� ��������� ������� ��� ��������� ������
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
            Debug.Log($"������ ����� ScriptableObject ��� ����� {gridSize}x{gridSize}.");
        }
    }

    private void InitializeNewTemplate()
    {
        int templateNumber = (templatesData != null) ? templatesData.templates.Count + 1 : 1;
        string templateName = $"������ {templateNumber}";
        currentTemplate = new GridTemplate(templateName, gridSize);
        Debug.Log($"������ ����� ������: {templateName} ��� ����� {gridSize}x{gridSize}.");
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
        GUILayout.Label("�������� ������� �����", EditorStyles.boldLabel);

        gridSize = EditorGUILayout.IntField("������ �����:", gridSize);
        gridSize = Mathf.Clamp(gridSize, 4, 10);

        if (currentTemplate == null || currentTemplate.gridSize != gridSize)
        {
            LoadOrCreateTemplateData();
            InitializeNewTemplate();
        }

        currentColor = EditorGUILayout.ColorField("������� ����:", currentColor);

        selectedShape = (ShapeType)EditorGUILayout.EnumPopup("��� ������:", selectedShape);

        if (GUILayout.Button(isDrawingMode ? "��������� �����" : "������� ������"))
        {
            isDrawingMode = !isDrawingMode;
        }

        DisplayGrid();

        GUILayout.Space(10);

        if (GUILayout.Button("��������� ������"))
        {
            if (templatesData != null)
            {
                templatesData.templates.Add(currentTemplate);
                EditorUtility.SetDirty(templatesData);
                AssetDatabase.SaveAssets();
                Debug.Log($"������ ��� {gridSize}x{gridSize} �������� � {templatesData.name}.");
                InitializeNewTemplate();
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("����� ������"))
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

                // ��������, ���� �� ��� ������ � �������
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
        Debug.Log($"������� ����� ������: {newShape.shapeName}.");
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
            Debug.Log($"������ ����� ScriptableObject ��� ����� {gridSize}x{gridSize}.");
        }
    }

    private void InitializeNewTemplate()
    {
        int templateNumber = (templatesData != null) ? templatesData.templates.Count + 1 : 1;
        string templateName = $"������ {templateNumber}";
        currentTemplate = new GridTemplate(templateName, gridSize);
        Debug.Log($"������ ����� ������: {templateName} ��� ����� {gridSize}x{gridSize}.");
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