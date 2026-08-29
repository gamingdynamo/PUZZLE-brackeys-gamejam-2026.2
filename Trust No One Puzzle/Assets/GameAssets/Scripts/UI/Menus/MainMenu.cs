using UnityEngine;
using UnityEngine.SceneManagement; // Required for switching scenes

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;


    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    public void CloseOptions()
    {

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            mainMenuPanel.SetActive(true);
            settingsPanel.SetActive(false);
        }
        else
        {
            SceneManager.LoadScene(0);
        }

    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
