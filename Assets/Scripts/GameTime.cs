/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is not linked to a game object
using UnityEngine;

/*
 * This class defines the constant timestep interval used for the game's physics computations
 * The lower the time interval, the faster the game's overall physics motions will appear.
 * 
 * All physics quantities (i.e. speed, velocity, acceleration, force) are defined per INTERVAL seconds.
 * For example, a speed of 5 means a displacement of 5 units per INTERVAL seconds.
 */
public static class GameTime
{
    public const float INTERVAL = 0.02f; // 0.02 seconds
    public const float RATE = 1 / INTERVAL; // The framerate equivalent to the interval

    /*
     * This factor determines by how much something will change position based on the current framerate.
     * For example, if a game object is set to move to the right by 1 * TimeFactor() units every frame,
     * then the object will move to the right by 1 unit every INTERVAL amount of seconds
     */
    public static float TimeFactor()
    {
        return Time.deltaTime * RATE;
    }
}
