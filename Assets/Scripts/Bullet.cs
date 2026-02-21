using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField][Min(0)] private float _speed;
    [SerializeField] private Rigidbody _rigidbody;

    public event Action<IPoolable> Released;

    private void OnValidate()
    {
        _rigidbody ??= GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        _rigidbody.velocity = transform.forward * _speed;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent(out Enemy _) == false && other.gameObject.TryGetComponent(out Wall _) == false)
            return;

        if (other.gameObject.TryGetComponent(out Enemy _enemy))
            _enemy.Release();

        Release();
    }

    public void Release()
    {
        Released?.Invoke(this);

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }
}