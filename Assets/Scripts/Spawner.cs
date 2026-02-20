using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private UnitFactory _unitFactory;
    [SerializeField] private Race _playerRace;
    [SerializeField] private StarterArmyConfig _starterArmy;

    private void Start()
    {
        BuildStartedArmy();
    }

    private void BuildStartedArmy()
    {
        foreach(var unit in _starterArmy.StarterUnits)
        {
            _unitFactory.CreateUnit(_playerRace, unit.Position);
        }
    }
}