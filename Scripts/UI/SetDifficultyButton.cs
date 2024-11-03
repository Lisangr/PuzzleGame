using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SetDifficultyButton : MonoBehaviour
{
    public int difficultyLevel; // ������� ���������, ������� ����� ������������
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SetDifficulty);
    }

    // ��������� ������� ��������� � PlayerPrefs � ��������� �����
    private void SetDifficulty()
    {
        // ��������� ������� ���������
        PlayerPrefs.SetInt("DifficultyLevel", difficultyLevel);
        PlayerPrefs.Save();

        // ��������� ����� PCscene
        SceneManager.LoadScene("PCscene");
    }
}
