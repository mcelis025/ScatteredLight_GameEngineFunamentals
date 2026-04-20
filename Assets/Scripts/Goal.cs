using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Goal : MonoBehaviour
{
    private const string MainMenuSceneName = "Main_Menu";
    private const string MainMenuScenePath = "Assets/Scenes/Main_Menu.unity";
    private const string Level2SceneName = "Level_2";
    private const string Level3SceneName = "Level_3";
    private static GameObject winMenuUI;
    private static Sprite defaultUISprite;
    private static Font defaultUIFont;
    private bool hasTriggered;

    void Awake()
    {
        Collider2D goalCollider = GetComponent<Collider2D>();

        if (goalCollider != null)
        {
            goalCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (hasTriggered || !col.CompareTag("Player"))
        {
            return;
        }

        hasTriggered = true;

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "Main")
        {
            SceneManager.LoadScene(Level2SceneName);
            return;
        }

        if (currentSceneName == "Level_2")
        {
            SceneManager.LoadScene(Level3SceneName);
            return;
        }

        if (currentSceneName == "Level_3")
        {
            ShowWinMenu(col.GetComponent<PlayerController>());
            return;
        }

        Debug.Log("Goal was reached, but this scene is not set up in Goal.cs yet.");
    }

    void ShowWinMenu(PlayerController playerController)
    {
        if (winMenuUI != null)
        {
            winMenuUI.SetActive(true);
            Time.timeScale = 0f;
            return;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        Time.timeScale = 0f;
        EnsureEventSystemExists();

        GameObject canvasObject = new GameObject("WinMenuCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = new GameObject("WinPanel");
        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 300f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.sprite = GetDefaultUISprite();
        panelImage.color = new Color(0f, 0f, 0f, 0.84f);
        panelImage.type = Image.Type.Simple;

        CreateText(panelObject.transform, "WinText", "YOU WIN", new Vector2(0f, 70f), 42, Color.white);
        CreateButton(panelObject.transform, "MainMenuButton", "MAIN MENU", new Vector2(0f, -10f), LoadMainMenu);

        winMenuUI = canvasObject;
    }

    void CreateText(Transform parent, string objectName, string textValue, Vector2 anchoredPosition, int fontSize, Color textColor)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(360f, 80f);
        textRect.anchoredPosition = anchoredPosition;

        Text text = textObject.AddComponent<Text>();
        text.text = textValue;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = GetDefaultFont();
        text.fontSize = fontSize;
        text.color = textColor;
    }

    void CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction clickAction)
    {
        GameObject buttonObject = new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(260f, 52f);
        rectTransform.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GetDefaultUISprite();
        image.color = new Color(0.92f, 0.92f, 0.92f, 1f);
        image.type = Image.Type.Simple;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(clickAction);

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
        text.color = Color.black;
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

    void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (winMenuUI != null)
        {
            Destroy(winMenuUI);
            winMenuUI = null;
        }

        int mainMenuBuildIndex = SceneUtility.GetBuildIndexByScenePath(MainMenuScenePath);

        if (mainMenuBuildIndex >= 0)
        {
            SceneManager.LoadScene(mainMenuBuildIndex);
            return;
        }

        SceneManager.LoadScene(MainMenuSceneName);
    }

    void EnsureEventSystemExists()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }
        else if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }
}
