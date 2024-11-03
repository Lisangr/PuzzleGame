using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasButtons : MonoBehaviour
{
    public GameObject helpPanel;
    public GameObject settingsPanel;
    public GameObject menuPanel;
    public GameObject leadersPanel;
    public GameObject defeatPanel;
    public GameObject winPanel;

    private void Start()
    {
        OnCloseButtonClick();
    }
    public void OnCloseButtonClick()
    {
        if (helpPanel != null)
        {
            helpPanel.SetActive(false);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        if (leadersPanel != null)
        {
            leadersPanel.SetActive(false);
        }
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }
    public void ShowWinPanel()
    {
        winPanel.SetActive(true);
    }
    public void ShowDefeatPanel()
    {
        defeatPanel.SetActive(true);
    }
    public void OnHelpButtonClick()
    {
        helpPanel.SetActive(true);
    }
    public void OnSettingsButtonClick()
    {
        settingsPanel.SetActive(true);
    }
    public void OnLeadersButtonClick()
    {
        leadersPanel.SetActive(true);
    }
    public void OnStartButtonClick(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
    public void OnPauseMenuClick()
    {
        menuPanel.SetActive(true);
    }
    public void RestartCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}
