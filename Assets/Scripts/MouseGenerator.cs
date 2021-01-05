/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Mice game object
using System.Collections.Generic;
using UnityEngine;

/*
 * Generates and spawns 5 mice on the Main Floor after the obstacles have been spawned
 */
public class MouseGenerator : MonoBehaviour
{
    public GameObject mouse; // The mouse game object prefab

    private static List<GameObject> mice = new List<GameObject>(); // A list of all mice generated
    private const int NUMBER_OF_MICE = 100; // The number of mice to generate. It is possible that fewer mice are generated if there are not enough available grid coordinates to spawn them.
    private static List<int[]> availableGridCoordinates = new List<int[]>(); // A list of all grid coordinates available to spawn a mouse
    private static System.Random random = new System.Random(); // An instance of the Random class


    // Awake is called before any other script's Start() method
    // This Awake method is set to execute third in the project settings
    // so that it is executed after the ObstacleGenerator Awake method
    void Awake()
    {
        availableGridCoordinates = GameArea.GetAllAvailableGridCoordinates(); // Initialize availableGridCoordinates

        // Generate the mice at random available grid positions
        for (int i = 0; i < NUMBER_OF_MICE; i++)
        {
            if (availableGridCoordinates.Count == 0)
            {
                return; // Stop generating mice if there are no more available grid coordinates
            }

            int[] chosenCoordinates = GameArea.PickRandomFromListAndRemove(availableGridCoordinates, random); // Choose coordinates to spawn a mouse and remove them from the list
            InstantiateMouse(chosenCoordinates[0], chosenCoordinates[1], i); // Instantiate a mouse at the chosenCoordinates
        }
    }

    // Instantiate a mouse at grid coordinates (x, z) with an integer id
    private void InstantiateMouse(int x, int z, int id)
    {
        Vector3 position = GameArea.ConvertGridToUnity(x, z); // The position of the mouse that corresponds to the gid coordinates
        position.y = Mouse.Y; // Set the y-position of the mouse

        GameObject newMouse = Instantiate(mouse, position, transform.rotation) as GameObject; // Instantiate a mouse at the position
        newMouse.name = "Mouse " + id; // The the mouse's name
        newMouse.transform.parent = transform; // Set the parent of the mouse to this game object
        newMouse.GetComponent<Mouse>().setMouseId(id); // Set the mouse's id

        mice.Add(newMouse); // Add the mouse to the list of all generated mice
    }
}
