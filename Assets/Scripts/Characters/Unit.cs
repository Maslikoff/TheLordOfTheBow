using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour, IUnit
{
    private void OnEnable()
    {
        Attack();
    }

    public void Attack()
    {
        Debug.Log("Attac");
    }
}