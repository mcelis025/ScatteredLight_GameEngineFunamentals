using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static bool hasRegisteredSceneCallback;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterSceneCallback()
    {
        if (hasRegisteredSceneCallback)
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        hasRegisteredSceneCallback = true;
    }

    static void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    {
        if (activeScene.name != "Main_Menu")
        {
            return;
        }

        SetUpMainMenu();
    }

    static void SetUpMainMenu()
    {
        MainMenuManager existingManager = Object.FindFirstObjectByType<MainMenuManager>();

        if (existingManager != null)
        {
            existingManager.ConfigureButtons();
            return;
        }

        GameObject managerObject = new GameObject("MainMenuManager");
        MainMenuManager manager = managerObject.AddComponent<MainMenuManager>();
        manager.ConfigureButtons();
    }

    void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Main_Menu")
        {
            ConfigureButtons();
        }
    }

    void ConfigureButtons()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            if (button == null)
            {
                continue;
            }

            string buttonName = button.gameObject.name.Trim();

            if (buttonName == "Start_Button")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(StartGame);
                button.interactable = true;
            }
            else if (buttonName == "Quit_Button")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(QuitGame);
                button.interactable = true;
            }
            else if (buttonName == "Settings_Button")
            {
                button.onClick.RemoveAllListeners();
                button.interactable = false;
            }
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("Game is exiting...");
    }
}
