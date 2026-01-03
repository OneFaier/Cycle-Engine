using UnityEngine;
using Fusion;

namespace Starter.Shooter
{
    public sealed class GameManager : NetworkBehaviour
    {
        public PlayerFPS PlayerPrefab; // <- ici le vrai type

        [Networked]
        public PlayerRef BestHunter { get; set; }
        public PlayerFPS LocalPlayer { get; private set; } // <- aussi PlayerFPS

        private SpawnPoint[] _spawnPoints;

        public Vector3 GetSpawnPosition()
        {
            var spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            var randomPositionOffset = Random.insideUnitCircle * spawnPoint.Radius;
            return spawnPoint.transform.position + new Vector3(randomPositionOffset.x, 0f, randomPositionOffset.y);
        }

        public override void Spawned()
        {
            _spawnPoints = FindObjectsOfType<SpawnPoint>();

            LocalPlayer = Runner.Spawn(PlayerPrefab, GetSpawnPosition(), Quaternion.identity, Runner.LocalPlayer);
            Runner.SetPlayerObject(Runner.LocalPlayer, LocalPlayer.Object);
        }

        public override void FixedUpdateNetwork()
        {
            BestHunter = PlayerRef.None;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            LocalPlayer = null;
        }
    }
}