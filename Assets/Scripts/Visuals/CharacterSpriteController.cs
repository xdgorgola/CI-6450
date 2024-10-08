using IA.Steering;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterSpriteController : MonoBehaviour
{
    [SerializeField]
    private AgentMovement _character;
    [SerializeField]
    private SpriteRenderer _renderer;
    [SerializeField]
    private Animator _animator;
    

    private void Awake()
    {
        if (_character is null)
            throw new System.NullReferenceException();

        if (!TryGetComponent(out _renderer))
            throw new System.NullReferenceException();

        if (_animator is null)
            _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!_character.IsEnabled)
            return;

        // Revisar problema sin el rotation locker. Flipea sprite durisimo
        KinematicMovementData movementData = _character.KinematicData;
        bool isIdle = movementData.Velocity.magnitude <= MathUtils.STOP_EPSILON;
        _animator.SetBool("Idle", isIdle);
        _animator.SetFloat("DirX", Mathf.Sign(movementData.Velocity.x));
    }
}
