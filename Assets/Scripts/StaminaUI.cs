using UnityEngine;

public class StaminaUI : MonoBehaviour
{
    private PlayerController player;

    public static void EnsureExists(PlayerController player)
    {
        StaminaUI existingUI = FindFirstObjectByType<StaminaUI>();

        if (existingUI != null)
        {
            existingUI.player = player;
            return;
        }

        GameObject uiObject = new GameObject("Stamina UI");
        StaminaUI newUI = uiObject.AddComponent<StaminaUI>();
        newUI.player = player;
    }

    void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
    }

    void OnGUI()
    {
        if (player == null)
        {
            return;
        }

        DrawStaminaBar();
        DrawHealthHearts();
    }

    void DrawStaminaBar()
    {
        float staminaPercent = player.GetStaminaPercent();

        Rect backgroundRect = new Rect(20f, 20f, 200f, 20f);
        Rect fillRect = new Rect(20f, 20f, 200f * staminaPercent, 20f);

        GUI.color = Color.black;
        GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);

        GUI.color = Color.yellow;
        GUI.DrawTexture(fillRect, Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.Label(new Rect(20f, 42f, 200f, 25f), "Stamina");
    }

    void DrawHealthHearts()
    {
        int currentHealth = player.GetCurrentHealth();
        int maxHealth = player.GetMaxHealth();

        GUI.color = Color.white;
        GUI.Label(new Rect(20f, 70f, 200f, 25f), "Health");

        for (int i = 0; i < maxHealth; i++)
        {
            Rect heartRect = new Rect(20f + i * 36f, 95f, 28f, 28f);

            if (i < currentHealth)
            {
                GUI.color = Color.red;
            }
            else
            {
                GUI.color = Color.black;
            }

            GUI.DrawTexture(heartRect, Texture2D.whiteTexture);
        }

        GUI.color = Color.white;
    }
}
