/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Treasure game object
using UnityEngine.UI;
using UnityEngine;

/*
 * Implements the behaviour of the treasure
 */
public class Treasure : MonoBehaviour
{
    public Text treasureAcquiredText; // The text box to use to display that the treasure has been acquired
    private static bool treasureAcquired = false; // True if the treasure has been acquired

    // Start is called before the first frame update
    void Start()
    {
        treasureAcquiredText.text = "";
    }

    // If an object collided with this game object
    private void OnTriggerEnter(Collider other)
    {
        // The other object that collided with it is the First Person Controller
        if (!treasureAcquired && other.CompareTag("First Person Controller"))
        {
            // Indicate that the treasure has been acquired
            treasureAcquired = true;

            // Destroy the treasure box and spot light
            Destroy(transform.GetChild(0).gameObject);
            Destroy(transform.GetChild(1).gameObject);

            // Set the text box
            treasureAcquiredText.text = "Treasure Acquired!";
        }
    }

    // Returns the value of treasureAcquired
    public static bool IsTreasureAcquired()
    {
        return treasureAcquired;
    }
}
