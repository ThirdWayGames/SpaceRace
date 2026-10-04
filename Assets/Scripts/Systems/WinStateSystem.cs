using Assets.Scripts.Components;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    public class WinStateSystem : ComponentSystem
    { 
        protected struct Filter
        {
            public Transform Transform;
            public StaticWinConditionComponent WinConditionComponent;
        }

        protected override void OnUpdate()
        {
            List<StaticWinConditionComponent> winConditions = new List<StaticWinConditionComponent>();

            foreach (var entity in GetEntities<Filter>())
            {
                winConditions.Add(entity.WinConditionComponent);
            }

            if (winConditions.Any())
            {
                if (winConditions.All(x => x.ConditionMet))
                {
                    foreach (var winCondition in winConditions)
                    {
                        var gameManager = GameManager3D.instance;
                        if (!gameManager.GameEndConditions.Contains(winCondition))
                        {
                            gameManager.GameEndConditions.Add(winCondition);
                        }

                    }
                }
            }
        }
    }
}