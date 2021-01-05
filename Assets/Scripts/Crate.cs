/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Crate game object prefab
using UnityEngine;

/*
 * Defines the size and position of a crate
 */
public class Crate : MonoBehaviour
{
    public const float Y = GameArea.Y + 2.5f; // The  y-position of a crate
    public const float WIDTH = 5f; // The diameter of a crate along the x-axis
    public const float LENGTH = 5f; // The diameter of a crate along the z-axis
    public const float HEIGHT = 5f; // The diameter of a crate along the y-axis

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(WIDTH, HEIGHT, LENGTH); // Set the localScale of the rock
    }
}
