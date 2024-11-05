using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterMining : MonoBehaviour
{
    public RegeneratingResource OwnedVein { get; private set; } = null;

    [Min(1)]
    [SerializeField]
    private int _obtainedPerHit = 2;
    [Min(0.5f)]
    [SerializeField]
    private float _hitTime = 1f;

    [SerializeField]
    private UnityEvent<WorldResource, int> OnMined = new();

    private Coroutine _miningRoutine = null;

    public bool StartMining(RegeneratingResource res)
    {
        if (OwnedVein is not null)
        {
            Debug.LogWarning("[MINER] Trying to start mining a resource while already owning a vein.");
            return false;
        }

        if (!Occupy(res))
            return false;

        _miningRoutine = StartCoroutine(MiningCo());
        return true;
    }


    public void StopMining()
    {
        if (OwnedVein is null)
        {
            Debug.LogWarning("[MINER] Trying to stop mining a resource while not owning a vein.");
            return;
        }

        StopCoroutine(_miningRoutine);
        DeOccupy();
        _miningRoutine = null;
    }


    private IEnumerator MiningCo()
    {
        while (true)
        {
            if (OwnedVein.Amount == 0)
                yield return new WaitUntil(() => OwnedVein.Amount > 0);

            yield return new WaitForSeconds(_hitTime);
            int amountGet = OwnedVein.ConsumeResource(_obtainedPerHit);
            OnMined?.Invoke(OwnedVein.ResourceType, amountGet);
        }
    }

    private bool Occupy(RegeneratingResource resource)
    {
        if (OwnedVein is not null)
        {
            Debug.LogWarning("[MINER] Trying to occupy a resource while already occupying one");
            return false;
        }

        if (!resource.Occupy(gameObject))
            return false;

        OwnedVein = resource;
        return true;
    }


    private void DeOccupy()
    {
        if (OwnedVein is null)
        {
            Debug.LogWarning("[MINER] Trying to deocuppy a resource but the character owns no resource");
            return;
        }

        OwnedVein.DeOccupy();
        OwnedVein = null;
    }
}