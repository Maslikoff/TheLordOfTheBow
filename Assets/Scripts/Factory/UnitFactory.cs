using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitSpawnData
{
    public GameObject UnirPrefab;
}

[Serializable]
public class RaceUnitConfig
{
    public Race Race;
    public UnitSpawnData[] Units;
}

[Serializable]
public class StarterUnitEntry
{
    public Race Race;
    public Vector3 Position;
}

[Serializable]
public class StarterArmyConfig
{
    public StarterUnitEntry[] StarterUnits;
}

public class UnitFactory : MonoBehaviour
{
    [SerializeField] private RaceUnitConfig[] _raceUnitsConfigs;
    [SerializeField] private PoolsHandler _poolsHandler;

    private Dictionary<Race, GameObject> _unitPrefabs;
    private Dictionary<Race, IObjectPool<GameObject>> _unitPools;

    private void Awake()
    {
        Initialize();
    }

    public IUnit CreateUnit(Race race, Vector3 position)
    {
        _unitPrefabs.TryGetValue(race, out GameObject unit);

        GameObject prefab = Instantiate(unit, position, Quaternion.identity);

        return prefab.GetComponent<IUnit>();
    }

    private void Initialize()
    {
        _unitPrefabs = new Dictionary<Race, GameObject>();

        foreach (var race in _raceUnitsConfigs)
        {
            foreach (var unit in race.Units)
            {
                _unitPrefabs[(race.Race)] = unit.UnirPrefab;
            }
        }
    }
}