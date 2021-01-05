/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * This code is released under the MIT License
 */
// This script is linked to the Rock game object prefab
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Defines the size and position of a rock
 */
public class Rock : MonoBehaviour
{
    public const float Y = GameArea.Y + 1f; // The y-position of a rock
    public const float WIDTH = 8.5f; // The diameter of a rock along the x-axis
    public const float LENGTH = 5f; // The diameter of a rock along the z-axis
    public const float HEIGHT = 5f; // The diameter of a rock along the y-axis

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(WIDTH, HEIGHT, LENGTH); // Set the localScale of the rock
    }
}
