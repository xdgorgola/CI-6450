using System.Collections;
using UnityEngine;

public class RegeneratingResource : MonoBehaviour
{
    private GameObject _owner = null;

    [SerializeField]
    private WorldResource _type = WorldResource.Gold;
    [Min(0)]
    [SerializeField]
    private int _amount = 0;
    [Min(1)]
    [SerializeField]
    private int _maxAmount = 10;

    [Min(0.1f)]
    [SerializeField]
    private float _regenerationTime = 1.0f;
    [Min(1)]
    [SerializeField]
    private int _regenerationAmount = 1;

    private Coroutine _regenerationRoutine = null;

    public int Amount
    {
        get { return _amount; }
    }

    public WorldResource ResourceType
    {
        get { return _type; }
    }

    public bool Available
    {
        get { return _owner is null; }
    }

    public int ConsumeResource(int toConsume)
    {
        int consumed = toConsume;
        if (toConsume > _amount)
            consumed = _amount;

        _amount = Mathf.Max(_amount - toConsume, 0);
        if (_regenerationRoutine is null)
            StartRegeneration();

        return consumed;
    }


    private void StartRegeneration()
    {
        if (_regenerationRoutine is not null)
        {
            Debug.LogWarning("[RESOURCE] Trying to start regenerating an already regenerating resource");
            return;
        }

        _regenerationRoutine = StartCoroutine(RegenerationCo());
    }


    private IEnumerator RegenerationCo()
    {
        while (_amount != _maxAmount)
        {
            yield return new WaitForSeconds(_regenerationTime);
            _amount = Mathf.Min(_amount + _regenerationAmount, _maxAmount);
        }

        _regenerationRoutine = null;
    }


    public bool Occupy(GameObject owner)
    {
        if (_owner is not null)
        {
            Debug.LogWarning("[RESOURCE] Trying to occupy an already occupied resource");
            return false;
        }

        _owner = owner;
        return true;
    }


    public void DeOccupy()
    {
        _owner = null;
    }
}
