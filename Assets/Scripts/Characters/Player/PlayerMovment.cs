using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovment : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float _speed = 5f;

    private void OnValidate()
    {
        _characterController ??= GetComponent<CharacterController>();
    }

    public void Move(Vector2 direction)
    {
        Vector3 move = new Vector3(direction.x, 0, direction.y);
        _characterController.Move(move * _speed * Time.deltaTime);
    }
}