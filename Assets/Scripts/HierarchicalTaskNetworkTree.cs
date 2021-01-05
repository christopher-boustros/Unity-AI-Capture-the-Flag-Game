/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is not linked to a game object
using System.Collections.Generic;

/*
 * This class implements a Hierarchical Task Network tree (HTN tree), along with a simple forward planner
 * that searches the tree to construct a plan.
 * 
 * Each possible plan is a behaviour that the Cave Monster can perform.
 */
public class HierarchicalTaskNetworkTree
{
    /*
     * CLASSES
     */

    // A world state vector, which defines the state of the game at a given time
    private class WorldStateVector
    {
        public Dictionary<string, bool> states = new Dictionary<string, bool>(); // A dictionary containing the boolean values of each state in the vector

        // Constructor 1
        public WorldStateVector(List<bool> values)
        {
            states.Add("Player entered area", values[0]);
            states.Add("Player near cave", values[1]);
            states.Add("Monster near rock", values[2]);
            states.Add("Monster near crate", values[3]);
            states.Add("Monster near player", values[4]);
            states.Add("Monster holding crate", values[5]);
            states.Add("Monster holding rock", values[6]);
            states.Add("Monster just threw obstacle", values[7]);
        }

        // Constructor 2 (no parameter)
        public WorldStateVector()
        {
            states.Add("Player entered area", true);
            states.Add("Player near cave", true);
            states.Add("Monster near rock", true);
            states.Add("Monster near crate", true);
            states.Add("Monster near player", true);
            states.Add("Monster holding crate", true);
            states.Add("Monster holding rock", true);
            states.Add("Monster just threw obstacle", true);
        }
    }

    // Describes what the boolean value of an element of a world state vector must be to satisfy a condition
    private enum ConditionType
    {
        CAN_BE_TRUE_OR_FALSE,
        MUST_BE_FALSE,
        MUST_BE_TRUE,
        UNDEFINED // Undefined means the element value must be both true and false, which means the condition can never be satisfied
    }

    // Conditions on a world state vector 
    private class WorldStateVectorConditions
    {
        // A dictionary containing ConditionType values to describe what the value of each element of a world state vector must be to satisfy the condition
        public Dictionary<string, ConditionType> states = new Dictionary<string, ConditionType>();

        // Constructor 1
        public WorldStateVectorConditions(List<ConditionType> values)
        {
            states.Add("Player entered area", values[0]);
            states.Add("Player near cave", values[1]);
            states.Add("Monster near rock", values[2]);
            states.Add("Monster near crate", values[3]);
            states.Add("Monster near player", values[4]);
            states.Add("Monster holding crate", values[5]);
            states.Add("Monster holding rock", values[6]);
            states.Add("Monster just threw obstacle", values[7]);
        }

        // Constructor 2 (no parameter)
        public WorldStateVectorConditions()
        {
            states.Add("Player entered area", ConditionType.CAN_BE_TRUE_OR_FALSE);
            states.Add("Player near cave", ConditionType.CAN_BE_TRUE_OR_FALSE);
            states.Add("Monster near rock", ConditionType.CAN_BE_TRUE_OR_FALSE);
            states.Add("Monster near crate", ConditionType.CAN_BE_TRUE_OR_FALSE);
            states.Add("Monster near player", ConditionType.CAN_BE_TRUE_OR_FALSE);
            states.Add("Monster holding crate", ConditionType.CAN_BE_TRUE_OR_FALSE);
            states.Add("Monster holding rock", ConditionType.CAN_BE_TRUE_OR_FALSE);
            states.Add("Monster just threw obstacle", ConditionType.CAN_BE_TRUE_OR_FALSE);
        }

        // Returns true if a particular boolean value satisfies a condition type
        public static bool IsConditionSatisfied(bool b, ConditionType type)
        {
            if (type == ConditionType.UNDEFINED)
            {
                return false;
            }

            if (b == true && type == ConditionType.MUST_BE_FALSE)
            {
                return false;
            }

            if (b == false && type == ConditionType.MUST_BE_TRUE)
            {
                return false;
            }

            return true;
        }

        // Returns the boolean value that would result from a postcondition type given the current boolean value of a world state vector
        public static bool ResultOfPostcondition(bool b, ConditionType type)
        {
            if (type == ConditionType.CAN_BE_TRUE_OR_FALSE)
            {
                return b;
            }
            else if (type == ConditionType.MUST_BE_FALSE)
            {
                return false;
            }
            else if (type == ConditionType.MUST_BE_TRUE)
            {
                return true;
            }
            else // The type is UNDERFINED
            {
                return false;
            }
        }
    }

    // A node of the HTN tree, which can be a TaskNode or a MethodNode
    private abstract class HTNNode
    {
        public string description;

        // Constructor
        public HTNNode(string description)
        {
            this.description = description;
        }

        // Returns true if all its child nodes' preconditions (or its preconditions if it's a primitive task) are satisfied by a given world state vector
        // This function must be implemented by all subclasses
        public abstract bool SatisfiesPreconditions(WorldStateVector vector);
    }

    // A node of the HTN tree that is either a primitive task or a compound task
    private abstract class TaskNode : HTNNode
    {
        // Constructor
        public TaskNode(string description) : base(description) { }
    }

    // A method node of the HTN tree, which is a possible list of tasks which a compound task can be made up of
    // For this HTN tree, all tasks in a method node must be performed in a specific order
    private class MethodNode : HTNNode
    {
        public List<TaskNode> tasks; // The child nodes of a method

        // Constructor 1
        public MethodNode(string description, List<TaskNode> tasks) : base(description)
        {
            this.tasks = tasks;
        }

        // Returns true if all its child nodes' preconditions are satisfied by a given world state vector
        override
        public bool SatisfiesPreconditions(WorldStateVector vector)
        {
            // Check if the preconditions of each child is satisfied
            foreach (TaskNode child in tasks)
            {
                if (!child.SatisfiesPreconditions(vector))
                {
                    return false; // The preconditions of the child are not satisfied
                }

                // If the child is a primitive task
                if (child.GetType().Equals(typeof(PrimitiveTaskNode)))
                {
                    vector = ((PrimitiveTaskNode)child).ApplyTask(vector); // Get the world state vector after the task's postconditions are applied
                }
            }

            return true; // All preconditions are satisfied
        }
    }

    // A task that is not composed of subtasks
    private class PrimitiveTaskNode : TaskNode
    {
        public WorldStateVectorConditions preconditions; // The preconditions of the primitive task on a world state vector
        public WorldStateVectorConditions postconditions; // The postconditions of the primitive task on a world state vector

        // Constructor 1
        public PrimitiveTaskNode(string description, WorldStateVectorConditions preconditions, WorldStateVectorConditions postconditions) : base(description)
        {
            this.preconditions = preconditions;
            this.postconditions = postconditions;
        }

        // Constructor 2 (no preconditions and no postconditions)
        public PrimitiveTaskNode(string description) : base(description)
        {
            preconditions = new WorldStateVectorConditions();
            postconditions = new WorldStateVectorConditions();
        }

        // Returns what a given world state vector will become after the task is applied
        public WorldStateVector ApplyTask(WorldStateVector vector)
        {
            WorldStateVector finalVector = new WorldStateVector();

            // For each key in the postconditions
            foreach (string key in postconditions.states.Keys)
            {
                // Set the value at that key for the finalVector
                finalVector.states[key] = WorldStateVectorConditions.ResultOfPostcondition(vector.states[key], postconditions.states[key]);
            }

            return finalVector;
        }

        // Returns true if the node's preconditions are satisfied by a given world state vector
        override
        public bool SatisfiesPreconditions(WorldStateVector vector)
        {
            // For each key in the preconditions
            foreach (string key in preconditions.states.Keys)
            {
                if (!WorldStateVectorConditions.IsConditionSatisfied(vector.states[key], preconditions.states[key]))
                {
                    return false; // This node's preconditions are not satisfied
                }
            }

            return true; // This node's preconditions are satisfied
        }
    }

    // A task composed of subtasks
    // Its children are methods
    private class CompoundTaskNode : TaskNode
    {
        public List<MethodNode> methods; // The child nodes of a compound task
        private static System.Random random = new System.Random(); // An instance of the Random class

        // Constructor 1
        public CompoundTaskNode(string description, List<MethodNode> methods) : base(description)
        {
            this.methods = methods;
        }

        // Finds a method that is feasible for a given world state vector
        // A feasible method is one that satisfies its children's preconditions
        public MethodNode FindMethod(WorldStateVector vector)
        {
            // Helper function that returns a random method node from a list of methods
            MethodNode PickRandomMethod(List<MethodNode> methods)
            {
                if (methods == null || methods.Count == 0)
                {
                    return null;
                }

                int randomIndex = random.Next(0, methods.Count);
                return methods[randomIndex];
            }

            List<MethodNode> feasibleMethods = new List<MethodNode>();

            // For each method of the node
            foreach (MethodNode method in methods)
            {
                // If the vector satisfies the method's and its children's preconditions
                if (method.SatisfiesPreconditions(vector))
                {
                    feasibleMethods.Add(method); // Add it to the list of feasible methods
                }
            }

            return PickRandomMethod(feasibleMethods);
        }

        // Returns true if any of its child nodes are satisfied by a given world state vector
        override
        public bool SatisfiesPreconditions(WorldStateVector vector)
        {
            // Check if the preconditions of a child are satisfied
            foreach (MethodNode child in methods)
            {
                if (child.SatisfiesPreconditions(vector))
                {
                    return true; // The preconditions of the child are satisfied
                }
            }

            return false; // None of the children are satisfied by the vector
        }
    }

    // This class is used in the SimpleForwardPLanner function when searching the tree
    // The purpose of this class is to store the state of a search
    private class SearchState
    {
        // Fields
        public TaskNode task;
        public List<PrimitiveTaskNode> plan;
        public WorldStateVector state;

        // Constructor
        public SearchState(TaskNode task, List<PrimitiveTaskNode> plan, WorldStateVector state)
        {
            this.task = task;
            this.plan = plan;
            this.state = state;
        }
    }

    /*
     * FIELDS
     */
    private static TaskNode htnRoot; // The root node of the HTN tree

    /*
     * CONSTRUCTOR
     */
    static HierarchicalTaskNetworkTree()
    {
        TreeInit();
    }

    /*
     * FUNCTIONS
     */
    // Initialize the HTN tree
    // For simplicity, preconditions are given only for method nodes and compound task nodes.
    private static void TreeInit()
    {
        // Instantiate primitive task nodes and set their preconditions and postconditions
        PrimitiveTaskNode inactive = new PrimitiveTaskNode("Inactive"); // Do nothing indefinetly
        inactive.preconditions.states["Player entered area"] = ConditionType.MUST_BE_FALSE;
        inactive.postconditions.states["Monster just threw obstacle"] = ConditionType.MUST_BE_FALSE;

        PrimitiveTaskNode pause = new PrimitiveTaskNode("Pause"); // Pause for a fixed amount of time
        pause.preconditions.states["Player entered area"] = ConditionType.MUST_BE_TRUE;
        pause.preconditions.states["Monster holding crate"] = ConditionType.MUST_BE_FALSE;
        pause.preconditions.states["Monster holding rock"] = ConditionType.MUST_BE_FALSE;
        pause.postconditions.states["Monster just threw obstacle"] = ConditionType.MUST_BE_FALSE;

        PrimitiveTaskNode walkAround = new PrimitiveTaskNode("Walk around"); // Walk around randomly for a fixed amount of time
        walkAround.preconditions.states["Player entered area"] = ConditionType.MUST_BE_TRUE;
        walkAround.preconditions.states["Monster holding crate"] = ConditionType.MUST_BE_FALSE;
        walkAround.preconditions.states["Monster holding rock"] = ConditionType.MUST_BE_FALSE;
        walkAround.postconditions.states["Monster just threw obstacle"] = ConditionType.MUST_BE_FALSE;

        PrimitiveTaskNode moveToCave = new PrimitiveTaskNode("Move to cave"); // Attempt to move to the cave and stop once inside the cave or unable to move further
        moveToCave.preconditions.states["Player entered area"] = ConditionType.MUST_BE_TRUE;
        moveToCave.preconditions.states["Player near cave"] = ConditionType.MUST_BE_FALSE;
        moveToCave.preconditions.states["Monster near player"] = ConditionType.MUST_BE_TRUE;
        moveToCave.postconditions.states["Monster near player"] = ConditionType.MUST_BE_FALSE;

        PrimitiveTaskNode spinHeadHorizontally = new PrimitiveTaskNode("Spin head horizontally"); // Spin the head horizontally a few times
        spinHeadHorizontally.preconditions.states["Player entered area"] = ConditionType.MUST_BE_TRUE;
        spinHeadHorizontally.preconditions.states["Monster holding crate"] = ConditionType.MUST_BE_FALSE;
        spinHeadHorizontally.preconditions.states["Monster holding rock"] = ConditionType.MUST_BE_FALSE;

        PrimitiveTaskNode spinHeadVertically = new PrimitiveTaskNode("Spin head vertically"); // Spin the head vertically a few times
        spinHeadVertically.preconditions.states["Player entered area"] = ConditionType.MUST_BE_TRUE;
        spinHeadVertically.preconditions.states["Monster holding crate"] = ConditionType.MUST_BE_FALSE;
        spinHeadVertically.preconditions.states["Monster holding rock"] = ConditionType.MUST_BE_FALSE;

        PrimitiveTaskNode PickUpRock = new PrimitiveTaskNode("Pick up a rock"); // Pick up the nearest rock
        PickUpRock.preconditions.states["Player entered area"] = ConditionType.MUST_BE_TRUE;
        PickUpRock.preconditions.states["Monster holding crate"] = ConditionType.MUST_BE_FALSE;
        PickUpRock.preconditions.states["Monster holding rock"] = ConditionType.MUST_BE_FALSE;
        PickUpRock.preconditions.states["Monster near rock"] = ConditionType.MUST_BE_TRUE;
        PickUpRock.preconditions.states["Monster just threw obstacle"] = ConditionType.MUST_BE_FALSE;
        PickUpRock.postconditions.states["Monster holding rock"] = ConditionType.MUST_BE_TRUE;
        PickUpRock.postconditions.states["Monster holding crate"] = ConditionType.MUST_BE_FALSE;


        PrimitiveTaskNode PickUpCrate = new PrimitiveTaskNode("Pick up a crate"); // Pick up the nearest crate
        PickUpCrate.preconditions.states["Player entered area"] = ConditionType.MUST_BE_TRUE;
        PickUpCrate.preconditions.states["Monster holding crate"] = ConditionType.MUST_BE_FALSE;
        PickUpCrate.preconditions.states["Monster holding rock"] = ConditionType.MUST_BE_FALSE;
        PickUpCrate.preconditions.states["Monster near crate"] = ConditionType.MUST_BE_TRUE;
        PickUpCrate.preconditions.states["Monster just threw obstacle"] = ConditionType.MUST_BE_FALSE;
        PickUpCrate.postconditions.states["Monster holding rock"] = ConditionType.MUST_BE_FALSE;
        PickUpCrate.postconditions.states["Monster holding crate"] = ConditionType.MUST_BE_TRUE;

        PrimitiveTaskNode ThrowRock = new PrimitiveTaskNode("Throw rock"); // Throw the rock towards the player
        ThrowRock.preconditions.states["Player entered area"] = ConditionType.MUST_BE_TRUE;
        ThrowRock.preconditions.states["Monster holding rock"] = ConditionType.MUST_BE_TRUE;
        ThrowRock.preconditions.states["Monster holding crate"] = ConditionType.MUST_BE_FALSE;
        ThrowRock.preconditions.states["Monster just threw obstacle"] = ConditionType.MUST_BE_FALSE;
        ThrowRock.preconditions.states["Monster near player"] = ConditionType.MUST_BE_FALSE;
        ThrowRock.postconditions.states["Monster holding rock"] = ConditionType.MUST_BE_FALSE;
        ThrowRock.postconditions.states["Monster holding crate"] = ConditionType.MUST_BE_FALSE;

        PrimitiveTaskNode ThrowCrate = new PrimitiveTaskNode("Throw crate"); // Throw the crate towards the player
        ThrowCrate.preconditions.states["Player entered area"] = ConditionType.MUST_BE_TRUE;
        ThrowCrate.preconditions.states["Monster holding crate"] = ConditionType.MUST_BE_TRUE;
        ThrowCrate.preconditions.states["Monster holding rock"] = ConditionType.MUST_BE_FALSE;
        ThrowCrate.preconditions.states["Monster just threw obstacle"] = ConditionType.MUST_BE_FALSE;
        ThrowCrate.preconditions.states["Monster near player"] = ConditionType.MUST_BE_FALSE;
        ThrowCrate.postconditions.states["Monster holding crate"] = ConditionType.MUST_BE_FALSE;
        ThrowCrate.postconditions.states["Monster holding rock"] = ConditionType.MUST_BE_FALSE;

        // Instantiate compound task nodes
        CompoundTaskNode beAMonster = new CompoundTaskNode("Be a monster", new List<MethodNode>()); // The behaviour of a monster (the root node)
        htnRoot = beAMonster;
        CompoundTaskNode rockAttack = new CompoundTaskNode("Rock attack", new List<MethodNode>()); // A rock attack
        CompoundTaskNode crateAttack = new CompoundTaskNode("Crate attack", new List<MethodNode>()); // A crate attack

        // Instantiate method nodes

        // Be a monster methods
        beAMonster.methods.Add(new MethodNode("Do a rock attack", new List<TaskNode>() { spinHeadHorizontally, rockAttack })); // The rock attack behaiour
        beAMonster.methods.Add(new MethodNode("Do a crate attack", new List<TaskNode>() { spinHeadVertically, crateAttack })); // The crate attack behaiour
        beAMonster.methods.Add(new MethodNode("Be idle", new List<TaskNode>() { walkAround, pause })); // The idle behaviour
        beAMonster.methods.Add(new MethodNode("Be inactive", new List<TaskNode>() { inactive })); // The inactive behaviour

        // Rock attack methods
        rockAttack.methods.Add(new MethodNode("Do a fast rock attack", new List<TaskNode>() { PickUpRock, ThrowRock })); // The rock attack performed only when the player is not near the cave monster
        rockAttack.methods.Add(new MethodNode("Do a slow rock attack", new List<TaskNode>() { PickUpRock, moveToCave, ThrowRock })); // The rock attack performed only when the player is not near the cave and is near the cave monster

        // Crate attack methods
        crateAttack.methods.Add(new MethodNode("Do a fast crate attack", new List<TaskNode>() { PickUpCrate, ThrowCrate })); // The crate attack performed only when the player is not near the cave monster
        crateAttack.methods.Add(new MethodNode("Do a slow crate attack", new List<TaskNode>() { PickUpCrate, moveToCave, ThrowCrate })); // The crate attack performed when the player is not near the cave and is near the cave monster
    }

    // A simple forward planner, which performs a depth-first search of the HTN tree to construct a plan
    private static List<PrimitiveTaskNode> SimpleForwardPlanner()
    {
        Stack<SearchState> searchStates = new Stack<SearchState>(); // A stack of search states used for the helper functions SaveSate and RestoreSavedState

        List<PrimitiveTaskNode> plan = new List<PrimitiveTaskNode>(); // The plan that will be constructed
        WorldStateVector state = GetCurrentWorldStateVector(); // The current sate of the game
        LinkedList<TaskNode> tasks = new LinkedList<TaskNode>(); // A queue of TaskNode objects, implemented as a linked list
        tasks.AddFirst(htnRoot); // Enqueue the root of the tree

        // Depth-first search of the HTN tree
        while (!(tasks.Count == 0)) // While the queue is not empty
        {
            TaskNode task = tasks.First.Value; // Peek the task at the front of the queue
            tasks.RemoveFirst(); // Dequeue that task

            if (task.GetType().Equals(typeof(CompoundTaskNode))) // If the task is a compound task
            {
                MethodNode method = ((CompoundTaskNode)task).FindMethod(state); // Find a method of the compound task that satisfies the current world state vector

                if (method != null) // If a method was found
                {
                    SaveState(task, plan, state); // Save the state of the search so we can come back to it later

                    // Enqueue all tasks of the method
                    foreach (TaskNode t in method.tasks)
                    {
                        tasks.AddLast(t);
                    }
                }
                else
                {
                    RestoreSavedState(); // Restore a previous state of the search to try another method
                }
            }
            else // If the task is a primitive task
            {
                PrimitiveTaskNode primitiveTask = (PrimitiveTaskNode)task; // Downcast to a PrimitiveTaskNode

                if (task.SatisfiesPreconditions(state)) // If the current world state vector satisfies the task's preconditions
                {
                    state = primitiveTask.ApplyTask(state); // Update the world state vector with what it would be after the postconditions of the task are applied
                    plan.Add(primitiveTask); // Add the primitive task to the plan
                }
                else
                {
                    RestoreSavedState(); // Restore a previous state of the search to try another method
                }
            }
        }

        return plan; // Return the plan

        // Helper functions

        // Crates a SearchState object that represents the state of the search at the current loop iteration
        // and pushes it to the searchStates stack
        void SaveState(TaskNode currentTask, List<PrimitiveTaskNode> currentPLan, WorldStateVector currentWorldStateVector)
        {
            SearchState ss = new SearchState(currentTask, currentPLan, currentWorldStateVector);
            searchStates.Push(ss);
        }

        // Pops a search state from the searchStates stack and restores the function variables to what they were at that search state
        void RestoreSavedState()
        {
            if (searchStates.Count == 0)
            {
                return;
            }

            SearchState ss = searchStates.Pop(); // Get the last saved search state
            plan = ss.plan; // Replace the plan with the saved plan
            tasks.AddFirst(ss.task); // Add the saved task to the front of the queue
        }
    }

    // Constructs the current world state vector
    private static WorldStateVector GetCurrentWorldStateVector()
    {
        WorldStateVector currentVector = new WorldStateVector();
        currentVector.states["Player entered area"] = PlayerMovement.IsPlayerEnteredArea();
        currentVector.states["Player near cave"] = PlayerMovement.IsPlayerNearCave();
        currentVector.states["Monster near rock"] = CaveMonster.IsMonsterNearRock();
        currentVector.states["Monster near crate"] = CaveMonster.IsMonsterNearCrate();
        currentVector.states["Monster near player"] = CaveMonster.IsMonsterNearPlayer();
        currentVector.states["Monster holding crate"] = CaveMonster.IsMonsterHoldingCrate();
        currentVector.states["Monster holding rock"] = CaveMonster.IsMonsterHoldingRock();
        currentVector.states["Monster just threw obstacle"] = CaveMonster.IsMonsterJustThrewObstacle();

        return currentVector;
    }

    // Uses the SimpleForwardPlanner method to get a plan from the HTN tree and return it as a list of strings
    public static List<string> GetNextPlan()
    {
        List<PrimitiveTaskNode> plan = SimpleForwardPlanner();
        List<string> stringPlan = new List<string>();
        plan.ForEach(t => stringPlan.Add(t.description));
        return stringPlan;
    }
}
