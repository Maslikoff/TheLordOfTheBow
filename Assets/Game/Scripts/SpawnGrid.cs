using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class SpawnGrid : MonoBehaviour
    {
        [SerializeField] private Transform _enemySpawnPoint;
    
        [Header("Settings")]
        [SerializeField][Min(0)] private int _gridWidth = 5;
        [SerializeField][Min(0)] private int _gridHeight = 3;
        [SerializeField] private Vector3 _gridOffset = Vector3.zero;
        [SerializeField] private bool _centerGrid = true;
    
        [Header("Distance between entity")]
        [SerializeField] private float _horizontalSpacing = 2f;
        [SerializeField] private float _verticalSpacing = 2f;
    
        public event Action<Vector3> SpawnEnemyAtPosition;
    
        public void SpawnAllEnemies()
        {
            Vector3 offset = CalculateOffset();
    
            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    Vector3 spawnPosition = CalculateSpawnPosition(x, y, offset);
    
                    SpawnEnemyAtPosition?.Invoke(spawnPosition);
                }
            }
        }
    
        public void SpawnEnemyAt(int x, int y)
        {
            if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
                return;
    
            Vector3 offset = CalculateOffset();
            Vector3 spawnPosition = CalculateSpawnPosition(x, y, offset);
    
            SpawnEnemyAtPosition?.Invoke(spawnPosition);
        }
    
        private Vector3 CalculateOffset()
        {
            if (_centerGrid == false)
                return Vector3.zero;
    
            float totalWidth = (_gridWidth - 1) * _horizontalSpacing;
            float totalHeight = (_gridHeight - 1) * _verticalSpacing;
    
            return new Vector3(-totalWidth / 2f, 0, -totalHeight / 2f);
        }
    
        private Vector3 CalculateSpawnPosition(int x, int y, Vector3 offset)
        {
            Vector3 basePosition = _enemySpawnPoint != null ? _enemySpawnPoint.position : transform.position;
    
            return new Vector3(x * _horizontalSpacing, 0, y * _verticalSpacing) + offset + _gridOffset + basePosition;
        }
    }
}
