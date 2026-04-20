using UnityEngine;

public class QuitManager : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();

        Debug.Log("Game is exiting...");
    }
}