using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SetDifficultyButton : MonoBehaviour
{
    public int difficultyLevel; // Уровень сложности, который будет передаваться
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SetDifficulty);
    }

    // Сохраняем уровень сложности в PlayerPrefs и загружаем сцену
    private void SetDifficulty()
    {
        // Сохраняем уровень сложности
        PlayerPrefs.SetInt("DifficultyLevel", difficultyLevel);
        PlayerPrefs.Save();

        // Загружаем сцену PCscene
        SceneManager.LoadScene("PCscene");
    }
}
