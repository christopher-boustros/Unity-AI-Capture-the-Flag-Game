/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
/*
 * The implementation of character movement with the W/A/S/D keys is based on these two scripts, which were released under the Unlicense License: 
 * - https://github.com/Brackeys/MultiplayerFPS-Tutorial/blob/702e31cb1f9a7f480b4f9673551804db2436488d/MultiplayerFPS/Assets/Scripts/PlayerController.cs
 * - https://github.com/Brackeys/MultiplayerFPS-Tutorial/blob/702e31cb1f9a7f480b4f9673551804db2436488d/MultiplayerFPS/Assets/Scripts/PlayerMotor.cs
 */
// This script is linked to the First Person Controller game object
using UnityEngine;

/*
 * Makes the First Person Controller move based on W/A/S/D keyboard input
 * 
 * With the CharacterController component, the player avoids obstacles with colliders that are not IsTrigger.
 */
public class PlayerMovement : MonoBehaviour
{
    private const float SPEED = 0.06f; // The distance that the player moves every GameTime.INTERVAL seconds
    private const float Y = 1f; // The y-position of the player
    private static CharacterController controller; // The CharacterController component of the First Person Controller game object
    private static bool playerEnteredArea = false; // True once the player has entered the main floor for the first time

    // Start is called before the first frame update
    private void Start()
    {
        controller = GetComponent<CharacterController>(); // Initialize controller
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal"); // Get the integer associated with the A/D keyboard keys (-1, 0, or 1)
        float z = Input.GetAxisRaw("Vertical"); // Get the integer associated with the W/S keyboard keys (-1, 0, or 1)

        Vector3 direction = (transform.right * x + transform.forward * z).normalized; // The vector pointing in the direction that the controller will move

        // Make the controller move along the direction vector by a distance of SPEED every GameTime.INTERVAL seconds
        // The CharacterController Move function prevents the player from moving through colliders
        controller.Move(direction * SPEED * GameTime.TimeFactor());

        if (!playerEnteredArea && transform.position.z >= GameArea.ENTRANCE_FLOOR_MAX_Z)
        { // Player has entered the main floor for the first time
            playerEnteredArea = true;
        }

        // Make sure the player has a constant y-position
        if (controller.transform.position.y > 1.05f * Y || controller.transform.position.y < 0.95f * Y)
        {
            controller.transform.position = new Vector3(controller.transform.position.x, Y, controller.transform.position.z);
        }
    }

    // Getter method for playerEnteredArea
    public static bool IsPlayerEnteredArea()
    {
        return playerEnteredArea;
    }

    // Returns true if the player is within a threshold distance of the center of the cave
    public static bool IsPlayerNearCave()
    {
        float thresholdDistance = 25f; // The threshold distance for determining whether the player is near the cave
        float actualDistance = (GetPlayerPosition() - GameArea.CAVE_MID_POINT).magnitude; // The distance from the player to the mid-point of the cave

        if (actualDistance <= thresholdDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Gets the position of the player
    public static Vector3 GetPlayerPosition()
    {
        return controller.transform.position;
    }
}
