using UnityEngine;

public class ResourceDeposit : MonoBehaviour
{
    private GameObject _consumer = null;

    [SerializeField]
    private WorldResource _resourceType = WorldResource.Gold;
    [Min(0)]
    [SerializeField]
    private int _amount = 0;
    [SerializeField]
    private bool _isUnlimited = false;

    public WorldResource ResourceType
    {
        get { return _resourceType; }
    }

    public int Amount
    {
        get { return _isUnlimited ? int.MaxValue : _amount; }
    }

    public bool HasConsumer() =>
        _consumer is not null;

    public int TakeResource(int toTake)
    {
        if (_isUnlimited)
            return toTake;

        int taken = toTake;
        if (toTake > _amount)
            taken = _amount;

        _amount = Mathf.Max(_amount - toTake, 0);
        return taken;
    }

    public void DepositResource(int toDeposit)
    {
        if (_isUnlimited)
            return;

        if (toDeposit <= 0)
        {
            Debug.LogWarning("[RESOURCE.DEPOSIT] Trying to deposit 0 or less resources");
            return;
        }

        _amount += toDeposit;
    }

    public bool Occupy(GameObject consumer)
    {
        if (_consumer is not null)
        {
            Debug.LogWarning("[RESOURCE] Trying to occupy an already occupied resource");
            return false;
        }

        _consumer = consumer;
        return true;
    }


    public void DeOccupy()
    {
        _consumer = null;
    }
}
