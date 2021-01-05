/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Shield game object
using UnityEngine.UI;
using UnityEngine;

/*
 * Implements the player's shield
 */
public class Shield : MonoBehaviour
{
    public Text shieldActiveText; // The text box to indicate whether the shield is active
    public Text shieldValueText; // The text box to display the shield value
    public Text healthText; // The text box to display the health

    private static bool shieldActive = false; // True if the shield is active, false otherwise
    private static float shieldValue = 10f; // The shield value
    private const float SHIELD_RATE = 1f; // The amount by which the shield decreases per 1 second when activated

    private static int healthValue = 2; // The remaining number of times that the player can be hit by the cave monster before losing

    private bool keyJustPressed = false; // True if the space key was recently pressed
    private float shieldTogglePeriod = 1f; // The amount of seconds to wait before the shield can be toggled again

    // Update is called once per frame
    void Update()
    {
        // Set the shieldActiveText
        if (shieldValue == 0)
        {
            shieldActiveText.text = "Shield depleted";
        }
        else if (shieldActive)
        {
            shieldActiveText.text = "SHIELD ACTIVE - Press space to deactivate";
        }
        else
        {
            shieldActiveText.text = "Press space to activate shield";
        }

        // Set the shieldValueText
        shieldValueText.text = "Shield: " + Mathf.Ceil(shieldValue);

        // Check if the the shield is being toggled
        if (shieldValue != 0 && !keyJustPressed && Input.GetKeyDown("space"))
        { // The space key was pressed after at least shieldTogglePeriod seconds after the last shield toggle
            keyJustPressed = true;
            Invoke("ResetKeyJustPressed", shieldTogglePeriod); // Reset the keyJustPressed variable after shieldTogglePeriod seconds
            shieldActive = !shieldActive; // Toggle the shield
        }

        // Reduce the shield value when active
        if (shieldActive)
        {
            float decrease = SHIELD_RATE * Time.deltaTime; // The amount by which to reduce the shieldvalue, which is a value of SHIELD_RATE for every second that has elapsed between frames

            // Decrease the shield value to no less than 0
            if (shieldValue - decrease <= 0)
            {
                shieldValue = 0;
                shieldActive = false;
            }
            else
            {
                shieldValue -= decrease;
            }
        }

        // Set the health text
        healthText.text = "Health: " + healthValue;
    }

    // Resets the keyJustPressed variable
    private void ResetKeyJustPressed()
    {
        keyJustPressed = false;
    }

    // Returns the value of shieldActive
    public static bool IsShieldActive()
    {
        return shieldActive;
    }

    // Reduce the health by 1 if the shield is not active
    public static void ReduceHealth()
    {
        if (healthValue > 0 && !shieldActive)
        {
            healthValue -= 1;
        }
    }

    // Getter method for the healthValue
    public static int GetHealthValue()
    {
        return healthValue;
    }
}
