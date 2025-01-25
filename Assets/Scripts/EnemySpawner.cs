using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemySpawner : SerializedMonoBehaviour
{
    [SerializeField] private Vector3 enemyInitSpawnPosition;
    [OdinSerialize, SerializeField]
    private Dictionary<EnemyType, Enemy> _enemies = new Dictionary<EnemyType, Enemy>();
    
    private List<GameObject> _spawnedEnemies = new List<GameObject>();
    private EnemySpawnCombinations _combinations;
    
    private void Awake()
    {
        _combinations = Resources.Load<EnemySpawnCombinations>("Enemy Spawn Combinations");

        Task.Run(StartLevelSpawning);
    }

    public async UniTaskVoid StartLevelSpawning()
    {
        //Started
        await UniTask.Delay(TimeSpan.FromSeconds(3));
        await UniTask.SwitchToMainThread();
        foreach (var level in _combinations.Levels)
        {
            foreach (var turn in level.Turns)
            {
                foreach (var enemy in turn.Enemies)
                {
                    Enemy enemyObj = enemy.EnemyType switch
                    {
                        EnemyType.Mini => Instantiate(_enemies[EnemyType.Mini], enemyInitSpawnPosition,
                            Quaternion.identity),
                        EnemyType.Mid => Instantiate(_enemies[EnemyType.Mid], enemyInitSpawnPosition,
                            Quaternion.identity),
                        EnemyType.Big => Instantiate(_enemies[EnemyType.Big], enemyInitSpawnPosition,
                            Quaternion.identity),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    enemyObj.Initialize(enemy);
                    _spawnedEnemies.Add(enemyObj.gameObject);
                }
                
                await UniTask.WaitUntil(() => _spawnedEnemies.Count == 0);
                
                _spawnedEnemies.Clear();
            }
        }
    }
}