/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Cave Monster game object
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * Implements the behaviour of the Cave Monster game object.
 * 
 * The Cave Monster avoids obstacles with the NavMeshAgent component. A static navigation mesh was baked using the Navigation tab, so
 * the NavMeshAgent component walks on the walkable area of the navigation mesh, avoids obstacles that have the NavMeshObstacle component, 
 * which include the Crate, Rock, and Player game objects. The Crate, Rock, and Player objects dynamically carve holes in the navigation mesh
 */
public class CaveMonster : MonoBehaviour
{
    // The maximum speed of the cave mosnter when following a path as the displacement per second
    // This value should be greater than the player's speed (per second), which is PlayerMovement.SPEED / GameTime.INTERVAL
    private const float MAX_SPEED = 15f;
    private const float RADIUS = 3f; // The radius of the Cave Monster
    private const float HEIGHT = 13f; // The height of the Cave Monster
    private const float OBSTACLE_HEIGHT = HEIGHT + 10f; // The height of an obstacle picked up by the cave monster

    private static NavMeshAgent agent; // The NavMeshAgent component of the Cave Monster
    private static System.Random random = new System.Random();
    private static bool monsterHoldingCrate = false; // True if the Cave Monster is holding a crate
    private static bool monsterHoldingRock = false; // True if the Cave Monster is holding a rock
    private static bool mosnterJustThrewObstacle = false; // True if the monster just threw an obstacle
    private GameObject obstacleHeld; // The obstacle that the Cave Monster is holding

    private List<int[]> availableGridCoordinates; // A list of all grid coordinates that do not contain rocks or crates

    private Queue<string> tasksQueue = new Queue<string>(); // A queue of tasks to perform
    private string currentTask = null; // The task that the Cave Monster is currently performing
    private bool justPickedTask = false; // True if the current task was just picked

    public Transform caveMonsterHead; // The Cave Monster's head

    // Variables for "Walk around"
    private Vector3 currentTarget; // The position that the mosnter is currently moving towards

    private bool waiting = false; // Used when a task needs to wait a bit before finishing




    // Variables for "Throw rock" and "Throw crate"
    private Vector3 directionToPlayer; // The direction from the obstacle to the player when the monster just started the task

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Initialize agent
        agent.speed = MAX_SPEED; // Set the agent's maximum speed 
        availableGridCoordinates = GameArea.GetAllAvailableGridCoordinates(); // Initialize availableGridCoordinates
        ResetCurrentTarget();
    }

    // Update is called once per frame
    void Update()
    {
        // If the queue is empty
        if (tasksQueue.Count == 0)
        {
            // Search the HTN tree for a new plan and enqueue the tasks in the plan
            HierarchicalTaskNetworkTree.GetNextPlan().ForEach(t => tasksQueue.Enqueue(t));
        }

        // If the Cave Monster just finished its last task
        if (currentTask == null)
        {
            currentTask = tasksQueue.Dequeue(); // Dequeue a new task
            justPickedTask = true; // Indicate that the currentTask was just picked
            mosnterJustThrewObstacle = false; // Indicates that monster did not just throw an obstacle
        }

        // Perform the current task
        if (currentTask == "Inactive")
        {
            /* 
             * Do nothing for one frame
             */

            StopCurrentTask();
        }
        else if (currentTask == "Pause")
        {
            /* 
             * Do nothing for 1.25 seconds
             */

            SetTaskDuration(1.25f);
        }
        else if (currentTask == "Walk around")
        {
            /*
             * Walk to random locations using the navigation mesh for 3 seconds
             */

            SetTaskDuration(3f);

            agent.isStopped = false;
            agent.SetDestination(currentTarget); // Set the target that the agent will try to move to

            if (agent.velocity.Equals(Vector3.zero)) // If the agent is not able to move
            {
                ResetCurrentTarget(); // Pick a new target
            }

        }
        else if (currentTask == "Move to cave")
        {
            /*
             * Attempt to move to the cave for 2.5 seconds
             */

            SetTaskDuration(2.5f);

            agent.isStopped = false;
            agent.SetDestination(GameArea.CAVE_MID_POINT); // Set the target that the agent will try to move to
        }
        else if (currentTask == "Spin head horizontally")
        {
            /*
             * Spin head horizontally for 1.25 seconds
             */
            SetTaskDuration(1.25f);

            Vector3 rotationVector = new Vector3(0f, 20f, 0f);
            caveMonsterHead.Rotate(rotationVector * GameTime.TimeFactor());

        }
        else if (currentTask == "Spin head vertically")
        {
            /*
             * Spin head vertically for 1.25 seconds
             */
            SetTaskDuration(1.25f);

            Vector3 rotationVector = new Vector3(20f, 0f, 0f);
            caveMonsterHead.Rotate(rotationVector * GameTime.TimeFactor());
        }
        else if (currentTask == "Pick up a rock" || currentTask == "Pick up a crate")
        {
            /*
             * Pick up the nearest rock and wait 0.75 seconds
             */
            if (justPickedTask)
            {
                if (currentTask == "Pick up a rock")
                {
                    PickUpObstacle(GameArea.CoordinateType.ROCK);
                }
                else
                {
                    PickUpObstacle(GameArea.CoordinateType.CRATE);
                }

                justPickedTask = false;
                waiting = false;
            }
            else
            {
                if (!waiting)
                {
                    Invoke("StopCurrentTask", 0.75f);
                    waiting = true;
                }
            }
        }
        else if (currentTask == "Throw rock" || currentTask == "Throw crate")
        {
            /*
             * Rotate towards the player and throw the obstacle held
             */

            if (justPickedTask)
            {
                // Stop the task if an obstacle cannot be thrown
                if (obstacleHeld == null || currentTask == "Throw rock" && !monsterHoldingRock || currentTask == "Throw crate" && !monsterHoldingCrate)
                {
                    StopCurrentTask();
                }

                directionToPlayer = (PlayerMovement.GetPlayerPosition() - obstacleHeld.transform.position).normalized;

                justPickedTask = false;
                mosnterJustThrewObstacle = true;
                monsterHoldingRock = false;
                monsterHoldingCrate = false;
            }
            else
            {
                // Move obstacle
                obstacleHeld.transform.position += directionToPlayer * 2f * GameTime.TimeFactor();

                // First check for collision with an obstacle and make the obstacle stop on collision
                RaycastHit obstacleWallFloorHitInfo;
                bool hit = Physics.SphereCast(obstacleHeld.transform.position, 1f, directionToPlayer, out obstacleWallFloorHitInfo, 1.5f); // Cast a sphere with radius 1f from the obstacle's position in the direction directionToPlayer with a maximum distance of 1.5f
                if (hit)
                {
                    if (obstacleWallFloorHitInfo.collider.CompareTag("Crate") || obstacleWallFloorHitInfo.collider.CompareTag("Rock"))
                    { // An obstacle was hit
                        StopCurrentTask();
                        return;
                    }
                }

                // Then check for a collision with mice, and destroy the mice on collision
                RaycastHit[] miceHitInfo = Physics.SphereCastAll(obstacleHeld.transform.position, 4f, directionToPlayer, 1f);
                if (miceHitInfo != null)
                {
                    foreach (RaycastHit h in miceHitInfo)
                    {
                        // If a mouse was hit
                        if (h.collider.CompareTag("Mouse"))
                        {
                            Destroy(h.collider.gameObject); // Destroy the mouse
                        }
                    }
                }

                // Then check for collision with the player and make the obstacle stop on collisions
                if ((obstacleHeld.transform.position - PlayerMovement.GetPlayerPosition()).magnitude <= 6f)
                { // The player was hit
                    currentTask = "Throw crate"; // Assume obstcacle thrown was a crate
                    Shield.ReduceHealth();
                    StopCurrentTask();
                    return;
                }

                // Then check for collision with the floors and walls
                if (hit)
                {
                    if (obstacleWallFloorHitInfo.collider.CompareTag("Floor") || obstacleWallFloorHitInfo.collider.CompareTag("Wall"))
                    { // A floor or wall was hit
                        StopCurrentTask();
                        return;
                    }
                }

                // Finally, check if the obstacle is at or below the floor
                if (obstacleHeld.transform.position.y <= 0)
                { // The obstacle is at or below the floor
                    StopCurrentTask();
                    return;
                }
            }
        }
        else
        {
            // Incorrect task
        }
    }

    // This is called when the Cave Monster collides with an obstacle
    private void OnTriggerEnter(Collider other)
    {
        // If the obstacle is a mouse and the collider is not isTrigger
        if (other.gameObject.CompareTag("Mouse") && other.isTrigger == false)
        {
            Destroy(other.gameObject); // Destroy the mouse
        }
    }

    // Sets currentTarget to a new position
    private void ResetCurrentTarget()
    {
        int[] c = GameArea.PickRandomFromList(availableGridCoordinates, random); // Pick grid coordinates that are available (do not contain a rock or crate)
        currentTarget = GameArea.ConvertGridToUnity(c[0], c[1]); // Set the currentTarget to the corresponding position
    }

    // Returns true if the Cave Monster is near a rock (within a particular threshold distance)
    public static bool IsMonsterNearRock()
    {
        float thresholdDistance = RADIUS + 2f * Rock.WIDTH;

        foreach (GameObject rock in GameObject.FindGameObjectsWithTag("Rock"))
        {
            if ((rock.transform.position - agent.transform.position).magnitude <= thresholdDistance)
            {
                return true;
            }
        }

        return false;
    }

    // Returns true if the Cave Monster is near a crate (within a particular threshold distance)
    public static bool IsMonsterNearCrate()
    {
        float thresholdDistance = RADIUS + 4f * Crate.WIDTH;

        foreach (GameObject crate in GameObject.FindGameObjectsWithTag("Crate"))
        {
            if ((crate.transform.position - agent.transform.position).magnitude <= thresholdDistance)
            {
                return true;
            }
        }

        return false;
    }

    // Returns true if the Cave monster is near the player (within a particular threshold distance)
    public static bool IsMonsterNearPlayer()
    {
        float thresholdDistance = RADIUS + 15f;

        if ((PlayerMovement.GetPlayerPosition() - agent.transform.position).magnitude <= thresholdDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Getter method for monsterHoldingCrate
    public static bool IsMonsterHoldingCrate()
    {
        return monsterHoldingCrate;
    }

    // Getter method for monsterHoldingRock
    public static bool IsMonsterHoldingRock()
    {
        return monsterHoldingRock;
    }

    // Stops the current task and indicates that the agent has finished the current task by setting currentTask to null
    private void StopCurrentTask()
    {
        if (currentTask == "Walk around")
        {
            agent.isStopped = true; // Stop the agent from moving
        }
        else if (currentTask == "Spin head horizontally" || currentTask == "Spin head vertically")
        {
            caveMonsterHead.localRotation = Quaternion.Euler(0f, agent.transform.localRotation.y, 0f); // Reset the rotation of the head
        }
        else if (currentTask == "Throw rock" || currentTask == "Throw crate")
        {
            monsterHoldingRock = false;
            monsterHoldingCrate = false;


            if (currentTask == "Throw crate")
            { // Destroy the obstacle if it is a crate
                Destroy(obstacleHeld);
            }
            else
            { // Change back the parent of the obstacle if it is a rock
                obstacleHeld.transform.parent = GameObject.FindGameObjectWithTag("Rocks").transform;
            }

            obstacleHeld = null;
        }

        currentTask = null;
    }

    // Sets the amount of time until the current task will stop, in seconds
    private void SetTaskDuration(float time)
    {
        if (justPickedTask)
        {
            Invoke("StopCurrentTask", time);
            justPickedTask = false;
        }
    }

    // Finds the nearest rock or crate to the Cave Monster and picks it up
    private void PickUpObstacle(GameArea.CoordinateType type)
    {
        GameObject nearestObstacle;
        GameObject[] obstacles;

        if (type == GameArea.CoordinateType.ROCK)
        {
            obstacles = GameObject.FindGameObjectsWithTag("Rock"); // The list of active rocks
            monsterHoldingRock = true;
        }
        else if (type == GameArea.CoordinateType.CRATE)
        {
            obstacles = GameObject.FindGameObjectsWithTag("Crate"); // The list of active crates
            monsterHoldingCrate = true;
        }
        else
        {
            return;
        }

        float shortestDistance = float.MaxValue;
        int shortestDistanceIndex = 0;

        // Find the shortest distance to a rock/crate

        // For each available rock/crate
        for (int i = 0; i < obstacles.Length; i++)
        {
            float distanceToObstacle = (obstacles[i].transform.position - agent.transform.position).magnitude;

            if (distanceToObstacle < shortestDistance)
            {
                shortestDistance = distanceToObstacle;
                shortestDistanceIndex = i;
            }
        }

        nearestObstacle = obstacles[shortestDistanceIndex]; // The rock/crate that will be picked up

        // Pick up the rock/crate
        nearestObstacle.transform.position = agent.transform.position + (OBSTACLE_HEIGHT) * agent.transform.up;
        nearestObstacle.transform.parent = agent.transform;
        obstacleHeld = nearestObstacle;
    }


    // Getter method for monsterJustThrewObstacle
    public static bool IsMonsterJustThrewObstacle()
    {
        return mosnterJustThrewObstacle;
    }
}
