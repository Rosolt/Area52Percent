/*  ResetButton.cs

    Placed on a UI button to call ResetGame.
*/

using UnityEngine;

public class ResetButton : MonoBehaviour
{
    private GameManager gameManager;
    
    // Awake is called after all objects are initialized
    void Awake()
    {
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }

    // Call reset game from game manager
    public void ResetGame()
    {
        StartCoroutine(gameManager.ResetGame());
    }
}
