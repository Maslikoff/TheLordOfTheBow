using System;
using System.Collections;
using UnityEngine;

public class ShootEntity : MonoBehaviour
{
    [SerializeField] private float _cooldownTime = 1f;
    [SerializeField] private int _maxShotsPerBurst = 3;

    private int _currentShotsInBurst;
    private bool _isReloading;
    private bool _canShoot = true;

    public event Action<float> ReloadProgressUpdated;
    public event Action ShotFired;

    private void Start()
    {
        InvokeRepeating(nameof(TryShoot), 0f, 1f);
    }

    public void TryShoot()
    {
        if (_canShoot == false || _isReloading) 
            return;

        if (_currentShotsInBurst < _maxShotsPerBurst)
        {
            _currentShotsInBurst++;

            ShotFired?.Invoke();

            if (_currentShotsInBurst >= _maxShotsPerBurst)
                StartReload();
        }
    }

    private void StartReload()
    {
        _isReloading = true;
        _canShoot = false;
        _currentShotsInBurst = 0;

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _cooldownTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / _cooldownTime;
            
            ReloadProgressUpdated?.Invoke(progress);

            yield return null;
        }

        _isReloading = false;
        _canShoot = true;

        ReloadProgressUpdated?.Invoke(1f);
    }
}