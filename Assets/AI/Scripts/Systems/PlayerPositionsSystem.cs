using Assets.Scripts;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class PlayerPositionsSystem : ComponentSystem {

    private struct Group
    {
        public Transform Transform;
        public EnemyTargetComponent Targeting;
        public PlayerPositionsComponent PlayerPositions;
    }

    protected override void OnUpdate()
    {
        // If master client or is not in room
        if (PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom)
        {
            foreach (var entity in GetEntities<Group>())
            {
                foreach (var playerFound in Physics.OverlapSphere(entity.Transform.position, entity.Targeting.LookRadius).Select(x => x.transform.root.gameObject).Where(x => x.GetComponent<PlayerController3D>() != null).ToList())
                {
                    if (!entity.PlayerPositions.PlayerPositions.Contains(playerFound))
                    {
                        entity.PlayerPositions.PlayerPositions.Add(playerFound);
                    }
                }

                // Tidy up the list of player positions where the GameObject is no longer valid (poss killed)
                var nullPlayerPos = entity.PlayerPositions.PlayerPositions.Where(x => x == null).ToArray();
                for(int i = 0; i < nullPlayerPos.Length; i++)
                {
                    entity.PlayerPositions.PlayerPositions.Remove(nullPlayerPos[i]);
                }
                
            }
        }
    }
}
