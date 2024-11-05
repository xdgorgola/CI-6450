using System;
using System.Collections;
using UnityEngine;

public class CharacterDrunkness : MonoBehaviour
{
    [SerializeField]
    private CharacterInventory _inv = null;
    [SerializeField]
    private ResourceDeposit _ownedDep = null;


    [Min(0)]
    [SerializeField]
    private int _drunknessLevel = 0;
    [Min(1)]
    [SerializeField]
    private int _maxDrunkness = 10;
    [Min(0)]
    [SerializeField]
    private int _drunknessPerBeer = 2;
    [Min(0)]
    [SerializeField]
    private float _drinkSpeed = 1;
    [Min(0)]
    [SerializeField]
    private int _soberPerSecond = 1;

    private Coroutine _drinkingRoutine = null;
    private Coroutine _drunkRoutine = null;

    public int DrunknessAmount
    {
        get { return _drunknessLevel; }
    }

    public bool IsDrinking() =>
        _drinkingRoutine is not null;

    public bool IsDrunk() =>
        _drunkRoutine is not null;

    private void Awake()
    {
        if (_inv is null)
            throw new NullReferenceException(nameof(_inv));
    }

    
    public bool OccupyBeerDeposit(ResourceDeposit src)
    {
        if (src.ResourceType != WorldResource.Beer )
        {
            Debug.LogWarning("[DRUNKNESS] Trying to occupy deposit with non beer resource.");
            return false;
        }

        if (src.HasConsumer())
        {
            Debug.LogWarning("[DRUNKNESS] Trying to occupy beer deposit that already has an owner.");
            return false;
        }

        if (!src.Occupy(gameObject))
            return false;

        _ownedDep = src;
        _drinkingRoutine = StartCoroutine(DrinkCo());
        return true;
    }

    public void DeOccupyBeerDeposit()
    {
        if (_ownedDep is null)
        {
            Debug.LogWarning("[DRUNKNESS] Trying to deocupy a deposit but the drinker owns no deposit");
            return;
        }

        if (IsDrinking())
        {
            StopCoroutine(_drinkingRoutine);
            _drinkingRoutine = null;
        }

        _ownedDep.DeOccupy();
        _ownedDep = null;
    }

    public bool TakeBeer(ResourceDeposit src, int amount)
    {
        if (src.ResourceType != WorldResource.Beer)
        {
            Debug.LogError($"[DRUNKNESS] Trying to take beer from deposit {src} of type {src.ResourceType}");
            return false;
        }

        if (src.HasConsumer() != gameObject)
        {
            Debug.LogWarning("[DRUNKNESS] Trying to take beer from deposit that already has an owner.");
            return false;
        }
        int taken = src.TakeResource(amount);
        return taken > 0;
    }

    public bool DrinkBeer()
    {
        int consumed = _inv.ConsumeResource(WorldResource.Beer, 1);
        if (consumed == 0)
            return false;

        _drunknessLevel = Mathf.Min(_drunknessLevel + _drunknessPerBeer, _maxDrunkness);
        if (_drunknessLevel == _maxDrunkness && _drunkRoutine is null)
            _drunkRoutine = StartCoroutine(DrunkCo());

        return true;
    }

    private IEnumerator DrinkCo()
    {
        while (_drunknessLevel < _maxDrunkness)
        {
            yield return new WaitUntil(() => _ownedDep.Amount > 0);
            yield return new WaitForSeconds(_drinkSpeed);
            TakeBeer(_ownedDep, 1);
            _inv.AddResource(WorldResource.Beer, 1);
            DrinkBeer();
        }

        _drinkingRoutine = null;
    }

    private IEnumerator DrunkCo()
    {
        while (_drunknessLevel > 0)
        {
            yield return new WaitForSeconds(1);
            _drunknessLevel = Mathf.Max(_drunknessLevel - _soberPerSecond, 0);
        }

        _drunkRoutine = null;
    }
}
