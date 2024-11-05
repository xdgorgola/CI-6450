using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterEnergy : MonoBehaviour
{
    [Min(0f)]
    [SerializeField]
    private float _energy = 10f;

    [Min(1f)]
    [SerializeField]
    private float _maxEnergy = 10f;

    [Min(0.1f)]
    [SerializeField]
    private float _regenerationTime = 1.0f;
    [Min(0.1f)]
    [SerializeField]
    private float _regenerationAmount = 1;

    [Min(0.1f)]
    [SerializeField]
    private float _lossTime = 1.0f;
    [Min(0.1f)]
    [SerializeField]
    private float _lossAmount = 1;

    private Coroutine _regenerationRoutine = null;
    private Coroutine _lossRoutine = null;

    public UnityEvent OnTired { get; private set; } = new UnityEvent();
    public UnityEvent OnRested { get; private set; } = new UnityEvent();
    public CharacterBed OccupiedBed { get; private set; } = null;

    public float Energy
    {
        get { return _energy; }
    }

    public bool IsRegenerating() =>
        _regenerationRoutine is not null;


    public bool IsRested() =>
        _energy == _maxEnergy;


    private void Start()
    {
        _lossRoutine = StartCoroutine(LossCo());
    }

    public bool OccupyBed(CharacterBed bed)
    {
        if (OccupiedBed is not null)
        {
            Debug.LogWarning("[ENERGY] Trying to occupy a bed while already occupying one");
            return false;
        }

        if (!bed.OccupyBed(this))
            return false;

        OccupiedBed = bed;
        Debug.Log("[ENERGY] Starting energy regeneration");
        if (_regenerationRoutine is not null)
            Debug.LogWarning("[ENERGY] Trying to start regenerating an already regenerating resource");

        if (_lossRoutine is not null)
        {
            StopCoroutine(_lossRoutine);
            _lossRoutine = null;
        }

        _regenerationRoutine = StartCoroutine(RegenerationCo());
        return true;
    }

    public void DeOccupyBed()
    {
        if (OccupiedBed is null)
        {
            Debug.LogWarning("[ENERGY] Trying to deocuppy a bed but the character owns no bed");
            return;
        }

        Debug.Log("[ENERGY] Stopping energy regeneration");
        if (_regenerationRoutine is null)
            Debug.LogWarning("[ENERGY] Trying to stop regenerating, but the resource isnt!");
        else
            StopCoroutine(_regenerationRoutine);

        _regenerationRoutine = null;
        _lossRoutine = StartCoroutine(LossCo());
        OccupiedBed.DeOccupy();
        OccupiedBed = null;
    }

    public float ConsumeEnergy(float toConsume)
    {
        float consumed = toConsume;
        if (toConsume > _energy)
            consumed = _energy;

        _energy = Mathf.Max(_energy - toConsume, 0);

        if (_energy == 0)
            OnTired?.Invoke();

        return consumed;
    }


    private IEnumerator RegenerationCo()
    {
        while (_energy != _maxEnergy)
        {
            yield return new WaitForSeconds(_regenerationTime);
            _energy = Mathf.Min(_energy + _regenerationAmount, _maxEnergy);
        }

        OnRested?.Invoke();
        _regenerationRoutine = null;
    }

    
    private IEnumerator LossCo()
    {
        while (_energy != 0)
        {
            yield return new WaitForSeconds(_lossTime);
            _energy = Mathf.Max(_energy - _lossAmount, 0f);
        }

        OnRested?.Invoke();
        _lossRoutine = null;
    }
}
