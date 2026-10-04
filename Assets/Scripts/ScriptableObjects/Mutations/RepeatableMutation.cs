using System;
using UnityEngine;

[Serializable]
public abstract class RepeatableMutation : BaseMutation
{
    [Header("Repeatable Variables")]
    public bool Repeatable;

    [Tooltip("This will be the total number of executions that can occurr if repeatable")]
    public int TotalExecCount;

    public void Awake()
    {
        CurrentExecCount = 0;
        LastExecution = Time.time;
    }

    public override void ExecuteMutation(GameObject parentGameObject)
    {
        // If the mutation is not cured.
        if (!isCured)
        {
            // If execution has occrred once already and its repeatable
            if (CurrentExecCount > 0 && Repeatable)
            {
                // If we are due to execute
                if ((Time.time - LastExecution) >= ExecutionTime)
                {
                    // If we have a finite execution amount.
                    if (TotalExecCount > 0)
                    {
                        // And we have not reached that limit.
                        if (CurrentExecCount < TotalExecCount)
                        {
                            // Execute the mutation.
                            ExecutionTick(parentGameObject);

                            // Increment the CurrentExecCount
                            CurrentExecCount += 1;
                        }
                    }
                    else
                    {
                        // Always execute the mutation
                        ExecutionTick(parentGameObject);
                    }

                    // Set the execution time.
                    LastExecution = Time.time;
                }
            }
            else
            {
                base.ExecuteMutation(parentGameObject);
            }
        }
    }

    public override bool CanCureAfterExec()
    {
        // If its repeatable
        if (Repeatable)
        {
            // And has a total exec count greater than zero
            if (TotalExecCount > 0)
            {
                // And the current exec count is equal to the total exec count
                return CureAfterExecution && CurrentExecCount == TotalExecCount;
            }
            else
            {
                // Else it will never cure after execution.
                return false;
            }
        }
        else
        {
            return base.CanCureAfterExec();
        }
    }
}