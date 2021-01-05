/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Mouse game object prefab
using UnityEngine;

/*
 * Handles the behaviour of a mouse. A mouse avoids obstacles using steering behaviour only (they do not use navigation meshes).
 * 
 * The mice detect obstacles using raycasts, which is why obstacles including rocks, crates, and other mice contain an extra collider
 * with isTrigger set to true, which are slightly larger than their actual shape. The purpose of these isTrigger colliders is for
 * the mice to see the obstacles as being slightly larger than they actually are in order to make them stay slightly further away
 * from the obstacles while steering away from them. The isTrigger is set to true so that the CharacterController components of the
 * First Person Controller and Cave Monster do not use them to detect a collision.
 */
public class Mouse : MonoBehaviour
{
    public const float Y = GameArea.Y + 0.5f; // The y-position of a mouse
    public const float WIDTH = 0.25f; // The diameter of a mouse along the x-axis
    public const float LENGTH = 1.5f; // The diameter of a mouse along the z-axis
    public const float HEIGHT = 1f; // The diameter of a mouse along the y-axis
    public const float SPEED = 0.04f; // The speed (magnitude of the velocity) of the mouse when there are no steering forces
    public const float MAX_TIME_TO_STEER = 4f; // The maximum number of seconds it takes for a mouse to steer to a new velocity
    public const float MIN_STEERING_FORCE_MAGNITUDE = SPEED / (MAX_TIME_TO_STEER / 2f * GameTime.RATE); // The minimum magnitude of the steering force
    public const float MAX_STEERING_FORCE_MAGNITUDE = 50f * MIN_STEERING_FORCE_MAGNITUDE; // The maximum magnitude of the steering force

    private static System.Random random = new System.Random(); // An instance of the Random class

    private int mouseId = -1; // The id of the mouse, which will be initialized by the MouseGenerator class

    private Vector3 velocity; // The mouse's current velocity (the distance the mouse moves per GameTime.INTERVAL seconds)
    private Vector3 steeringForce; // The mouse's current steering force (assuming unit mass, this is the change in velocity per GameTime.INTERVAL seconds)
    private Vector3 velocityToAchieve; // After encountering a wall, a mouse will continue to steer until it achieves this velocity
    private bool alreadyEncounteredHorizontalWall = false; // True if the mouse just encountered a horizontal wall and is seering towards velocityToAchieve
    private bool alreadyEncounteredVerticalWall = false; // True if the mouse just encountered a horizontal wall and is seering towards velocityToAchieve
    private float currentSteeringForceMagnitude = MIN_STEERING_FORCE_MAGNITUDE; // The current steering force magnitude, inversely proportional to the distance from the mouse to the obstacle

    // The types of obstacles that the mouse will avoid with the steering force
    enum ObstacleType
    {
        NONE,
        HORIZONTAL_WALL,
        VERTICAL_WALL,
        OTHER
    }

    private ObstacleType obstacleType = ObstacleType.NONE; // The current obstacle that is in front of the mouse

    private bool paused = false; // True if the mouse is in its paused state
    private bool justUnpaused = true; // True if the mouse just exited its paused state
    private float pauseTime = 4f; // The amount of seconds that a mouse stays in its paused state
    private int minTimeToPause = 10; // The minimum amount of seconds before a mouse can enter its paused state
    private int maxTimeToPause = 30; // The maximum amount of seconds before a mouse must enter its paused state

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(WIDTH, HEIGHT, LENGTH); // Set the localScale of the mouse

        // Rotate the mouse randomly
        int yRotation = random.Next(0, 360);
        transform.eulerAngles = new Vector3(0f, yRotation, 0f);

        // Set the mouse's initial steeringForce
        steeringForce = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // If the mouse is not in its paused state, walk around while steering away from obstacles
        if (!paused)
        {
            AdjustVelocity(); // Adjust the velocity and rotation of the mouse according to the steering force
            Move(); // Make the mouse move according to its velocity
            obstacleType = CheckObstacle(); // Checks if there is an obstacle in front of the mouse
            AdjustSteeringForce(obstacleType); // Adjust the steering force based on which obstacle was detected
        }

        // Puts the mouse in its paused state after a random delay if it was just recently unpaused
        if (!paused && justUnpaused)
        {
            justUnpaused = false;
            int delay = random.Next(minTimeToPause, maxTimeToPause + 1);
            Invoke("PauseMouse", delay);
        }
    }

    // Adjusts the mouse's velocity and rotation according to the steering force
    private void AdjustVelocity()
    {
        // If there is no steering force
        if (steeringForce.Equals(Vector3.zero))
        {
            velocity = transform.forward * SPEED; // The velocity points in the direction the mouse is facing and has magnitude SPEED
        }
        else
        { // There is a steering force
            // Adjust the velocity of the mouse according to the steering force, assuming the mouse has a mass of 1
            // By multiplying by GameTime.TimeFactor(), the velocity changes by the value of steeringForce every GameTime.INTERVAL seconds
            velocity += steeringForce * GameTime.TimeFactor();

            // Change the forward direction of the mouse to be the direction of the adjusted velocity
            if (velocity != Vector3.zero)
            {
                transform.forward = velocity;
            }
        }
    }

    // Makes the mouse move according to its velocity
    private void Move()
    {
        // By multiplying by GameTime.TimeFactor(), the position changes by the value of velocity every GameTime.INTERVAL seconds
        transform.position += velocity * GameTime.TimeFactor();
    }

    // Uses raycasting to check if there is an obstacle in front of the mouse
    // Returns the type of obstacle that was found in front of the mouse
    // It also sets the currentSteeringForceMagnitude
    private ObstacleType CheckObstacle()
    {
        float rayLength = 5f; // The length of the ray that will be cast

        // Cast a ray of length rayLength from the center of the mouse in the direction it is facing
        // and store the information in the variable hitInfo
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(transform.position, transform.forward, out hitInfo, rayLength);

        if (!hit)
        { // If no obstacle was detected in front of the mouse
            currentSteeringForceMagnitude = MIN_STEERING_FORCE_MAGNITUDE;
            return ObstacleType.NONE;
        }
        else
        {
            // Set the currentSteeringForceMagnitude inversely proportional to the distance between the mouse and the object detected
            currentSteeringForceMagnitude = MIN_STEERING_FORCE_MAGNITUDE * rayLength / hitInfo.distance; // The lower the distance, the higher the currentSteeringForceMagnitude
            if (currentSteeringForceMagnitude > MAX_STEERING_FORCE_MAGNITUDE)
            {
                currentSteeringForceMagnitude = MAX_STEERING_FORCE_MAGNITUDE;
            }

            if (hitInfo.collider.CompareTag("Horizontal Mouse Wall"))
            { // If a horizontal mouse wall was detected
                return ObstacleType.HORIZONTAL_WALL;
            }
            else if (hitInfo.collider.CompareTag("Vertical Mouse Wall"))
            { // If a vertical mouse wall was detected
                return ObstacleType.VERTICAL_WALL;
            }
            else
            { // If any other obstacle was detected
                return ObstacleType.OTHER;
            }
        }
    }

    // Adjust the steering force based on which obstacle was detected in front of the mouse or
    // if no obstacle was detected.
    private void AdjustSteeringForce(ObstacleType obstacleType)
    {
        // Check if alreadyEncounteredHorizontalWall or alreadyEncounteredVerticalWall need to be reset
        if (alreadyEncounteredVerticalWall || alreadyEncounteredHorizontalWall)
        {
            // Check if the velocityToAchieve has been achieved
            if (ApproximatelySameDirection(velocity, velocityToAchieve))
            {
                // If so, reset the variables
                alreadyEncounteredHorizontalWall = false;
                alreadyEncounteredVerticalWall = false;
            }
            else if (obstacleType != ObstacleType.NONE && obstacleType != ObstacleType.HORIZONTAL_WALL && alreadyEncounteredHorizontalWall)
            { // If the mouse was avoiding a horizontal wall but has encountered another obstacle
                // Reset the variables
                alreadyEncounteredHorizontalWall = false;
                alreadyEncounteredVerticalWall = false;
            }
            else if (obstacleType != ObstacleType.NONE && obstacleType != ObstacleType.VERTICAL_WALL && alreadyEncounteredVerticalWall)
            { // If the mouse was avoiding a vertical wall but has encountered another obstacle
                // Reset the variables
                alreadyEncounteredHorizontalWall = false;
                alreadyEncounteredVerticalWall = false;
            }
        }

        // This condition is useful for when a mouse is stuck at a corner
        // It causes the mouse to reverse directions when the steering force is very high,
        // meaning that the distance from the mouse to an obstacle is very low
        if (currentSteeringForceMagnitude == MAX_STEERING_FORCE_MAGNITUDE)
        {
            velocity *= -1;
            transform.forward *= -1;
            return;
        }

        // If no obstacle was detected in front of the mouse
        if (obstacleType == ObstacleType.NONE)
        { 
            if (!alreadyEncounteredHorizontalWall && !alreadyEncounteredVerticalWall)
            {
                steeringForce = Vector3.zero; // Set the steering force to 0 because the mouse does not need to avoid an obstacle and is not avoiding a wall
            }
            else
            { // Mouse is still avoiding a wall
                // Set the steeringForce in the direction of the difference between the mouse's velocity and velocityToAchieve
                steeringForce = (velocityToAchieve - velocity).normalized * currentSteeringForceMagnitude;
            }
        }
        else if (obstacleType == ObstacleType.HORIZONTAL_WALL)
        { // If a horizontal mouse wall was detected
            // Set the velocityToAchieve if the mouse did not already encounter the wall
            if (! alreadyEncounteredHorizontalWall)
            {
                alreadyEncounteredHorizontalWall = true;
                Vector3 direction = new Vector3(SignOf(transform.forward.x), 0f, -SignOf(transform.forward.z)); // Set the direction of the velocityToAchieve
                direction = direction.normalized; // Normalize the direction
                velocityToAchieve = direction * SPEED; // Set the velocityToAchieve
            }

            // Set the steeringForce in the direction of the difference between the mouse's velocity and velocityToAchieve
            steeringForce = (velocityToAchieve - velocity).normalized * currentSteeringForceMagnitude;
        } 
        else if (obstacleType == ObstacleType.VERTICAL_WALL)
        { // If a vertical mouse wall was detected
          // Set the velocityToAchieve if the mouse did not already encounter the wall
            if (! alreadyEncounteredVerticalWall)
            {
                alreadyEncounteredVerticalWall = true;
                Vector3 direction = new Vector3(-SignOf(transform.forward.x), 0f, SignOf(transform.forward.z)); // Set the direction of the velocityToAchieve
                direction = direction.normalized; // Normalize the direction
                velocityToAchieve = direction * SPEED; // Set the velocityToAchieve
            }

            // Set the steeringForce in the direction of the difference between the mouse's velocity and velocityToAchieve
            // Set the steeringForce in the direction of the difference between the mouse's velocity and velocityToAchieve
            steeringForce = (velocityToAchieve - velocity).normalized * currentSteeringForceMagnitude;
        }
        else
        { // If any other obstacle was detected (crate, rock, player, cave monster, or other mouse)

            Vector3 direction;
            
            // To find a vector perpendicular to the mouse's forward direction, you must swap the mouse's forward direction
            // vector's x and z components and invert the sign of one of them, so there are two possible method.
            // The method that is used depends on the mouseId
            if (mouseId % 2 == 0)
            {
                // Make the direction of the steeringForce perpendicular to the mouse's forward direction by inverting the sig of the z-component
                direction = new Vector3(transform.forward.z, 0f, -transform.forward.x);
            }
            else
            {
                // Make the direction of the steeringForce perpendicular to the mouse's forward direction by inverting the sig of the x-component
                direction = new Vector3(-transform.forward.z, 0f, transform.forward.x);
            }

            direction = direction.normalized; // Normalize the direction of the steeringForce

            steeringForce = direction * currentSteeringForceMagnitude; // Set the steeringForce
        }
    }

    // Returns true if two velocity vectors are approximately in the same direction
    private bool ApproximatelySameDirection(Vector3 v1, Vector3 v2)
    {
        float margin = MIN_STEERING_FORCE_MAGNITUDE / 2f; // The margin of error

        if (System.Math.Abs(v1.normalized[0] - v2.normalized[0]) <= margin && System.Math.Abs(v1.normalized[1] - v2.normalized[1]) <= margin)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Returns -1 or 1 to indicate the sign of a number
    private float SignOf(float num)
    {
        if (num >= 0)
        {
            return 1f;
        }
        else
        {
            return -1f;
        }
    }

    // Puts the mouse in its paused state
    // and unpauses it after some time
    private void PauseMouse()
    {
        paused = true;
        Invoke("UnpauseMouse", pauseTime);
    }

    // Exits the mouse from its paused state
    private void UnpauseMouse()
    {
        paused = false;
        justUnpaused = true;
    }

    // Getter method for mouseId
    public int getMouseId()
    {
        return mouseId;
    }

    public void setMouseId(int id)
    {
        mouseId = id;
    }
}
