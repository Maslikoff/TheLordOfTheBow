using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Spawners
{
    public class SpawnGrid : MonoBehaviour
    {
        private readonly HashSet<int> _occupiedCells = new();
        
        [SerializeField] private Transform _enemySpawnPoint;
    
        [Header("Settings")]
        [SerializeField][Min(0)] private int _gridWidth = 5;
        [SerializeField][Min(0)] private int _gridHeight = 3;
        [SerializeField] private Vector3 _gridOffset = Vector3.zero;
        [SerializeField] private bool _centerGrid = true;
    
        [Header("Distance between entity")]
        [SerializeField] private float _horizontalSpacing = 2f;
        [SerializeField] private float _verticalSpacing = 2f;
        
        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;
        public int OccupiedCount => _occupiedCells.Count;
        public int TotalCells => _gridWidth * _gridHeight;
        public bool HasFreeCell => _occupiedCells.Count < TotalCells;
    
        public event Action<Vector3> SpawnEnemyAtPosition;
    
        public void ResetOccupancy() => _occupiedCells.Clear();
        public bool IsOccupied(int x, int y) => IsValidPosition(x, y) && _occupiedCells.Contains(ToIndex(x, y));
        
        public void Release(int x, int y)
        {
            if (IsValidPosition(x, y))
                _occupiedCells.Remove(ToIndex(x, y));
        }
        
        public bool TryGetRandomFreeCell(out int x, out int y)
        {
            x = 0;
            y = 0;
            if (HasFreeCell == false)
                return false;
            
            int freeIndex = UnityEngine.Random.Range(0, TotalCells - _occupiedCells.Count);
            
            for (int i = 0; i < TotalCells; i++)
            {
                if (_occupiedCells.Contains(i))
                    continue;
                
                if (freeIndex == 0)
                {
                    x = i % _gridWidth;
                    y = i / _gridWidth;
                    return TryOccupy(x, y);
                }
                freeIndex--;
            }
            return false;
        }
        
        public bool TryGetNextFreeCell(int startX, int startY, out int x, out int y)
        {
            for (int cy = startY; cy < _gridHeight; cy++)
            {
                int fromX = cy == startY ? startX : 0;
                
                for (int cx = fromX; cx < _gridWidth; cx++)
                {
                    if (TryOccupy(cx, cy))
                    {
                        x = cx;
                        y = cy;
                        return true;
                    }
                }
            }
            x = 0;
            y = 0;
            return false;
        }
        
        public bool TryOccupy(int x, int y)
        {
            if (IsValidPosition(x, y) == false || IsOccupied(x, y))
                return false;
            
            _occupiedCells.Add(ToIndex(x, y));
            
            return true;
        }
        
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
        
        public Vector3 GetSpawnPosition(int x, int y)
        {
            if (IsValidPosition(x, y) == false)
                return Vector3.zero;
                
            Vector3 offset = CalculateOffset();
            
            return CalculateSpawnPosition(x, y, offset);
        }
        
        public Vector3 GetRandomSpawnPosition()
        {
            int x = UnityEngine.Random.Range(0, _gridWidth);
            int y = UnityEngine.Random.Range(0, _gridHeight);
            
            return GetSpawnPosition(x, y);
        }

        private bool IsValidPosition(int x, int y) => x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight;
        
        private int ToIndex(int x, int y) => y * _gridWidth + x;
    
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
