/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Obstacles game object
using System.Collections.Generic;
using UnityEngine;

/*
 * Generates and spawns rocks and crates on the Main Floor
 */
public class ObstacleGenerator : MonoBehaviour
{
    public GameObject rock; // The Rock game object prefab
    public GameObject crate; // The Crate game object prefab

    private List<int[]> availableCoordinates = new List<int[]>(); // A list of all grid coordinates on the Main Floor available to put rocks and crates
    private System.Random random = new System.Random(); // Create an instance of the Random class 

    /* 
     * And obstacle buffer zone is number of grid units surrounding the center of a generated obstacle to 
     * mark as a zone where the center of an obstacle will not be generated. By placing a buffer zone around
     * an obstacle, it prevents another obstacle from being generated exactly adjacent to it, causing
     * the two obstacles to intersect. 
     * The buffer zone must be at least cover the distance of the maximum radius of an obstacle.
     * 
     * The value of 1 was chosen for a crate because the maximum obstacle radius is the width of a rock,
     * which is nearly 1 grid unit
     * 
     * The value of 2 was chosen for a rock because it is one grid unit greater than that of a crate,
     * which is necessary to account for the fact that a rock has a greater radius than a crate
     */
    public const int CRATE_BUFFER_ZONE = 1;
    public const int ROCK_BUFFER_ZONE = 2;
    public const int WALL_BUFFER_ZONE = 1;

    private const int MAX_OBSTACLES_TO_GENERATE = int.MaxValue; // The maximum number of obstacles to generate

    // Awake is called before any other script's Start() method
    // This Awake method is set to execute second in the project settings
    // so that it is executed after the GameArea Awake method
    void Awake()
    {
        AvailableCoordinatesInit(); // Initialize the list of available grid coordinates

        // For each obstacle that needs to be generated
        for (int i = 0; i < MAX_OBSTACLES_TO_GENERATE; i++)
        {
            // Stop generating obstacles if there is no more room to place obstacles
            if (availableCoordinates.Count == 0)
            {
                return;
            }

            int[] obstacleCoordinates; // The grid coordinates where the obstacle will be generated

            // Pick a random (x, z)-coordinate from the list of available coordinates
            obstacleCoordinates = GameArea.PickRandomFromListAndRemove(availableCoordinates, random);

            int xBuffer, yBuffer; // The amount of buffer zone around the generated obstacle to use when removing available coordinates from the list
            int yRotation; // The y-rotation of the obstacle

            // Randomly determine the y-rotation of the obstacle
            yRotation = random.Next(0, 90);

            // Instantiate either a crate or a rock (at a ratio of 1 rock per 2 crates)
            if (i % 3 >= 1)
            { // Instantiate a crate
                InstantiateObstacle(obstacleCoordinates[0], obstacleCoordinates[1], yRotation, GameArea.CoordinateType.CRATE);
                xBuffer = CRATE_BUFFER_ZONE;
                yBuffer = CRATE_BUFFER_ZONE;
            }
            else
            { // Instantiate a rock
                InstantiateObstacle(obstacleCoordinates[0], obstacleCoordinates[1], yRotation, GameArea.CoordinateType.ROCK);

                if (yRotation >= 70) // If the yRotation is within 20 degrees of 90 degrees
                {
                    xBuffer = CRATE_BUFFER_ZONE;
                    yBuffer = ROCK_BUFFER_ZONE;
                }
                else if (yRotation <= 20) // If the yRotation is within 20 degrees of 0 degrees
                {
                    xBuffer = ROCK_BUFFER_ZONE;
                    yBuffer = CRATE_BUFFER_ZONE;
                }
                else
                {
                    xBuffer = ROCK_BUFFER_ZONE;
                    yBuffer = ROCK_BUFFER_ZONE;
                }
            }

            // Remove the coordinates on and surrounding the generated obstacle
            // This is done so that the next obstacle is not placed on adjacent to the current obstacle
            for (int j = -xBuffer; j <= xBuffer; j++)
            {
                for (int k = -yBuffer; k <= yBuffer; k++)
                {
                    if (j == 0 && k == 0)
                    {
                        continue; // The coordinate at j=0 and k=0 has already been removed
                    }

                    int[] coordinatesToRemove = new int[] { obstacleCoordinates[0] + j, obstacleCoordinates[1] + k };
                    availableCoordinates.RemoveAll(c => c[0] == coordinatesToRemove[0] && c[1] == coordinatesToRemove[1]); // Remove coordinatesToRemove from the list of available coordinates
                }
            }
        }
    }

    // Initializes the list of available coordinates
    private void AvailableCoordinatesInit()
    {
        // For each (x, z) coordinates in the grid where an obstacle could be placed, excluding a buffer zone around the edges of the grid (the walls)
        for (int x = WALL_BUFFER_ZONE; x < GameArea.grid.GetLength(0) - WALL_BUFFER_ZONE; x++)
        {
            for (int z = WALL_BUFFER_ZONE; z < GameArea.grid.GetLength(1) - WALL_BUFFER_ZONE; z++)
            {
                availableCoordinates.Add(new int[] { x, z }); // Add the grid coordinates to the list
            }
        }
    }

    // Instantiate a crate or rock at grid coordinates (x, z) with y-rotation yRotation and update the grid
    private void InstantiateObstacle(int x, int z, int yRotation, GameArea.CoordinateType type)
    {
        // Parameters for the type of obstacle being instantiated
        float y; // The Unity y-position of the obstacle
        GameObject obstacle; // The obstalce game object
        string name; // The name to use for the obstacle
        int parentIndex; // The index of the obstacle's parent game object

        // Set the parameters
        if (type == GameArea.CoordinateType.CRATE)
        {
            y = Crate.Y;
            obstacle = crate;
            name = "Crate ";
            parentIndex = 0;
        }
        else if (type == GameArea.CoordinateType.ROCK)
        {
            y = Rock.Y;
            obstacle = rock;
            name = "Rock ";
            parentIndex = 1;
        }
        else
        {
            return;
        }

        Vector3 position = GameArea.ConvertGridToUnity(x, z); // The position of the obstacle that corresponds to the grid coordinates
        position.y = y; // Set the y-position of the obstacle
        GameObject newObstacle = Instantiate(obstacle, position, transform.rotation) as GameObject; // Instantiate the obstacle
        newObstacle.name = name + "(" + position[0] + ", " + position[1] + ")"; // Set the crate's name
        newObstacle.transform.parent = transform.GetChild(parentIndex); // Set the parent of the obstacle

        // Set the rotation of the obstacle
        newObstacle.transform.eulerAngles = new Vector3(0f, yRotation, 0f);

        // Update the grid
        GameArea.grid[x, z] = type;
    }
}
