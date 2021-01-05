/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Game Area game object
using System.Collections.Generic;
using UnityEngine;

/*
 * Defines the position and boundaries of the GameArea
 * Defines a 2D grid for the GameArea with functions to convert between grid and Unity coordinates
 * The 2D grid describes only the Main Floor gameobject, meaining it does not include the cave nor the entrance
 */
public class GameArea : MonoBehaviour
{
    // The x, y, and z positions of the GameArea
    public const float X = 0f;
    public const float Y = 0f;
    public const float Z = 0f;

    // The boundaries of the Main Floor game object, in Unity units
    // This is the floor described by the grid, where rocks, crates, and mice are spawned
    public const float MAIN_FLOOR_MIN_X = X - 30f;
    public const float MAIN_FLOOR_MAX_X = X + 50f;
    public const float MAIN_FLOOR_MIN_Z = Z - 30f;
    public const float MAIN_FLOOR_MAX_Z = Z + 30f;

    // The boundaries of the EntranceFloor game object
    public const float ENTRANCE_FLOOR_MAX_X = X + 30f;
    public const float ENTRANCE_FLOOR_MIN_X = X + 10f;
    public const float ENTRANCE_FLOOR_MAX_Z = Z - 30f;
    public const float ENTRANCE_FLOOR_MIN_Z = Z - 50f;

    // The mid-position of the cave (not part of the grid)
    public static readonly Vector3 CAVE_MID_POINT = new Vector3(-40f, 0f, 0f);

    // The length of a gird unit in Unity units
    public const float GRID_UNIT_LENGTH = 5f;

    /*
     * The matrix "grid" is a grid of coordinates which correspond to Unity positions on the ManFloor game object
     * The functions ConvertUnityToGrid and ConvertGridToUnity are used to convert between the grid coordinates
     * and the Unity coordinates. 
     * 
     * grid[x, z] stores a integer enum CoordinateType value that corresponds to what is at that coordinate
     */
    public static int[] MAX_GRID_COORDINATES; // The maximum x and z grid coordinates. The minimum x and z coordinates are (0, 0).
    public enum CoordinateType
    {
        FLOOR,
        CRATE,
        ROCK
    }
    public static CoordinateType[,] grid;

    // Awake is called before any other script's Start() method
    // This Awake method is set to execute first in the project settings
    void Awake()
    {
        transform.position = new Vector3(X, Y, Z); // Set the position of the GameArea

        // Initialize the grid
        GridInit();
    }

    /*
     * Converts (x, z) grid coordinates to its corresponding Unity coordinates as a Vector3
     * This scales up the grid coordinates by GRID_UNIT_LENGTH and converts them from
     * a scale of 0...(MAIN_FLOOR_MAX_X(or Z) - MAIN_FLOOR_MIN_X(or Z)) to a scale of
     * MAIN_FLOOR_MIN_X(or Z)...MAIN_FLOOR_MAX_X(or Z)
     */
    public static Vector3 ConvertGridToUnity(int x, int z)
    {
        float UnityX = x * GRID_UNIT_LENGTH; // Scale up x
        float UnityZ = z * GRID_UNIT_LENGTH; // Scale up z
        UnityX = UnityX + MAIN_FLOOR_MIN_X; // Change the scale of x
        UnityZ = UnityZ + MAIN_FLOOR_MIN_Z; // Change the scale of z

        return new Vector3(UnityX, 0f, UnityZ);
    }

    /*
     * Converts (x, z) Unity coordinates to its corresponding grid coordinates
     * This converts (x, z) from a scale of MAIN_FLOOR_MIN_X(or Z)...MAIN_FLOOR_MAX_X(or Z) to 
     * a scale of 0...(MAIN_FLOOR_MAX_X(or Z) - MAIN_FLOOR_MIN_X(or Z))
     * and then scales them down by GRID_UNIT_LENGTH
     */
    public static int[] ConvertUnityToGrid(float x, float z)
    {
        float gridX = x - MAIN_FLOOR_MIN_X; // Change the scale of x
        float gridZ = z - MAIN_FLOOR_MIN_Z; // Change the scale of z
        gridX /= GRID_UNIT_LENGTH; // Scale down x
        gridZ /= GRID_UNIT_LENGTH; // Scale down z

        return new int[] { (int)gridX, (int)gridZ };
    }

    // Initializes the grid
    private static void GridInit()
    {
        MAX_GRID_COORDINATES = ConvertUnityToGrid(MAIN_FLOOR_MAX_X, MAIN_FLOOR_MAX_Z);
        grid = new CoordinateType[MAX_GRID_COORDINATES[0] + 1, MAX_GRID_COORDINATES[1] + 1];

        // Sets all cells of the grid to type FLOOR
        for (int x = 0; x <= MAX_GRID_COORDINATES[0]; x++)
        {
            for (int z = 0; z <= MAX_GRID_COORDINATES[1]; z++)
            {
                grid[x, z] = CoordinateType.FLOOR;
            }
        }
    }

    // Returns a random element from the list l and removes it from the list
    public static int[] PickRandomFromListAndRemove(List<int[]> l, System.Random random)
    {
        if (l.Count == 0)
        { // If l is empty
            return null;
        }

        int i = random.Next(0, l.Count); // Pick a random index
        int[] val = l[i]; // Get the value at that index
        l.RemoveAt(i); // Remove the element chosen
        return val;
    }

    // Returns a random element from the list l
    public static int[] PickRandomFromList(List<int[]> l, System.Random random)
    {
        if (l.Count == 0)
        { // If l is empty
            return null;
        }

        int i = random.Next(0, l.Count); // Pick a random index
        int[] val = l[i]; // Get the value at that index
        return val;
    }

    // Returns a list of all grid coordinates that are type FLOOR, excluding the edges
    public static List<int[]> GetAllAvailableGridCoordinates()
    {
        List<int[]> availableGridCoordinates = new List<int[]>(); // The list to be returned

        // For each (x, z) coordinates in the grid, excluding the edges
        for (int x = 1; x < GameArea.grid.GetLength(0) - 1; x++)
        {
            for (int z = 1; z < GameArea.grid.GetLength(1) - 1; z++)
            {
                // The (x, z) coordinate is a FLOOR
                if (GameArea.grid[x, z] == GameArea.CoordinateType.FLOOR)
                {
                    availableGridCoordinates.Add(new int[] { x, z }); // Add (x, z) to the list
                }
            }
        }

        return availableGridCoordinates;
    }
}
