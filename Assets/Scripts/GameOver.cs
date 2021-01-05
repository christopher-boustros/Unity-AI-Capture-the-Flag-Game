/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Game Over game object
using UnityEngine.UI;
using UnityEngine;

/*
 * Ends the game when the player has been hit twice or has returned to the entrance with the treasure
 */
public class GameOver : MonoBehaviour
{
    public Text gameOverText;
    public Transform controller; // The transform component of the First Person Controller game object
    private static bool gameOver = false; // True when the game is over
    private static bool case1 = false; // The player has been hit twice
    private static bool case2 = false; // The player has returned to the entrance with the treasure

    // Start is called before the first frame update
    void Start()
    {
        gameOverText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        case1 = Shield.GetHealthValue() == 0; // Compute case1
        case2 = Treasure.IsTreasureAcquired() && controller.position.z < (4 * GameArea.ENTRANCE_FLOOR_MAX_Z + GameArea.ENTRANCE_FLOOR_MIN_Z) / 5f; // Compute case2

        if (case1 || case2)
        {
            if (case1)
            {
                gameOverText.text = "Game Over!\nYou have been hit twice.";
            }
            else
            {
                gameOverText.text = "Game Over!\nYou have successfully returned to the entrance with the treasure.";
            }

            EndGame();
        }
    }

    // Returns the value of gameOver
    public static bool IsGameOver()
    {
        return gameOver;
    }

    // Deactivates all active scripts in the game, except for this game object's and the Game Over Text game object's scripts
    private void EndGame()
    {
        MonoBehaviour[] allActiveScripts = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(); // Get all active scripts in the game

        // For all active scripts in the game
        foreach (MonoBehaviour script in allActiveScripts)
        {
            if (script.gameObject.name != gameOverText.name && script.gameObject.name != gameObject.name)
            { // If the script is not from the Game Over Text game object and is not from this game object
                script.enabled = false; // Deactivate the script
            }
        }
    }
}
