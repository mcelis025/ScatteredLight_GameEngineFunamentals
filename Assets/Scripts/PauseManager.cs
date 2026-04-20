using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    private static PauseManager instance;
    private static Sprite defaultUISprite;
    private static Font defaultUIFont;
    private static bool hasRegisteredSceneCallback;

    private GameObject pauseMenuUI;
    private GameObject settingsButtonObject;
    private bool isPaused;

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
        if (activeScene.name == "Main_Menu")
        {
            if (instance != null)
            {
                instance.Resume();
                Destroy(instance.gameObject);
                instance = null;
            }

            return;
        }

        if (instance != null)
        {
            instance.Resume();
            instance.EnsurePauseMenuExists();
            instance.EnsureEventSystemExists();
            return;
        }

        GameObject managerObject = new GameObject("PauseManager");
        instance = managerObject.AddComponent<PauseManager>();
        DontDestroyOnLoad(managerObject);
        instance.EnsurePauseMenuExists();
        instance.EnsureEventSystemExists();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Main_Menu")
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        EnsurePauseMenuExists();

        if (settingsButtonObject != null)
        {
            Button settingsButton = settingsButtonObject.GetComponent<Button>();

            if (settingsButton != null)
            {
                settingsButton.interactable = false;
            }
        }

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
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

    void EnsurePauseMenuExists()
    {
        if (pauseMenuUI == null)
        {
            CreatePauseMenu();
        }
    }

    void CreatePauseMenu()
    {
        Canvas canvas = CreateCanvas();
        pauseMenuUI = canvas.gameObject;
        pauseMenuUI.SetActive(false);

        GameObject panel = CreatePanel(canvas.transform);
        CreateTitle(panel.transform, "Paused", new Vector2(0f, 120f));
        CreateButton(panel.transform, "Resume_Button", "RESUME", new Vector2(0f, 35f), Resume, true);
        settingsButtonObject = CreateButton(panel.transform, "Settings_Button", "SETTINGS", new Vector2(0f, -20f), null, false);
        CreateButton(panel.transform, "Quit_Button", "QUIT", new Vector2(0f, -75f), QuitGame, true);
    }

    Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("PauseMenuCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    GameObject CreatePanel(Transform parent)
    {
        GameObject panelObject = new GameObject("PausePanel");
        panelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(420f, 320f);
        rectTransform.anchoredPosition = Vector2.zero;

        Image image = panelObject.AddComponent<Image>();
        image.sprite = GetDefaultUISprite();
        image.color = new Color(0f, 0f, 0f, 0.82f);
        image.type = Image.Type.Simple;

        return panelObject;
    }

    void CreateTitle(Transform parent, string textValue, Vector2 anchoredPosition)
    {
        GameObject textObject = new GameObject("PauseTitle");
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(320f, 60f);
        rectTransform.anchoredPosition = anchoredPosition;

        Text text = textObject.AddComponent<Text>();
        text.text = textValue;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = GetDefaultFont();
        text.fontSize = 34;
        text.color = Color.white;
    }

    GameObject CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction clickAction, bool interactable)
    {
        GameObject buttonObject = new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(250f, 44f);
        rectTransform.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GetDefaultUISprite();
        image.color = interactable ? new Color(0.92f, 0.92f, 0.92f, 1f) : new Color(0.42f, 0.42f, 0.42f, 0.9f);
        image.type = Image.Type.Simple;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.interactable = interactable;

        if (clickAction != null)
        {
            button.onClick.AddListener(clickAction);
        }

        GameObject textObject = new GameObject("Label");
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObject.AddComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = GetDefaultFont();
        text.fontSize = 24;
        text.color = interactable ? Color.black : new Color(0.08f, 0.08f, 0.08f, 0.9f);

        return buttonObject;
    }

    Sprite GetDefaultUISprite()
    {
        if (defaultUISprite == null)
        {
            Rect spriteRect = new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height);
            defaultUISprite = Sprite.Create(Texture2D.whiteTexture, spriteRect, new Vector2(0.5f, 0.5f));
        }

        return defaultUISprite;
    }

    Font GetDefaultFont()
    {
        if (defaultUIFont == null)
        {
            defaultUIFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return defaultUIFont;
    }

    void EnsureEventSystemExists()
    {
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();

        if (existingEventSystem != null)
        {
            if (existingEventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                existingEventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
        DontDestroyOnLoad(eventSystemObject);
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            Time.timeScale = 1f;
            instance = null;
        }
    }
}
