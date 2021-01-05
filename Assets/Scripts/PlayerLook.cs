/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
/*
 * The implementation of camera movement with the cursor is based on these two scripts, which were released under the Unlicense License: 
 * - https://github.com/Brackeys/MultiplayerFPS-Tutorial/blob/702e31cb1f9a7f480b4f9673551804db2436488d/MultiplayerFPS/Assets/Scripts/PlayerController.cs
 * - https://github.com/Brackeys/MultiplayerFPS-Tutorial/blob/702e31cb1f9a7f480b4f9673551804db2436488d/MultiplayerFPS/Assets/Scripts/PlayerMotor.cs
 */
// This script is linked to the First Person Controller object
using UnityEngine;

/*
 * Makes the First Person Controller camera and player rotate with the cursor position
 */
public class PlayerLook : MonoBehaviour
{
    public Transform cam; // The Transform component of the Main Camera game object
    private float currentCameraRotationX = 0f; // The current rotation of the camera about the x-axis (up/down)
    private float xRotation = 0f; // The change in rotation of the camera about the x-axis (up/down)
    private float yRotation = 0f; // The change in rotation of the camera about the y-axis (left/right)
    private const float CURSOR_SENSITIVITY = 4f; // The higher the sensitivity, the faster the camera moves with the cursor

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
    }

    // Update is called once per frame
    void Update()
    {
        yRotation = Input.GetAxisRaw("Mouse X") * CURSOR_SENSITIVITY * GameTime.TimeFactor(); // Get the y rotation, adjusted for the frame rate and cursor sensitivity
        transform.Rotate(Vector3.up * yRotation); // Rotate the player about the y-axis

        xRotation = Input.GetAxisRaw("Mouse Y") * CURSOR_SENSITIVITY * GameTime.TimeFactor(); // Get the x rotation, adjusted for the frame rate and cursor sensitivity
        currentCameraRotationX -= xRotation; // Update currentCameraRotationX
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX - xRotation, -90f, 90f); // Clamp currentCameraRotationX between -90 and 90 degrees        
        cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f); // Rotate the camera about the x-axis to currentCameraRotationX
    }
}
