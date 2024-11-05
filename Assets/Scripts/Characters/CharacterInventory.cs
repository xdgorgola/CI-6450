using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    private Dictionary<WorldResource, int> _inventory = new();

    private void Awake()
    {
        foreach (WorldResource res in Enum.GetValues(typeof(WorldResource)))
            _inventory.Add(res, 0);
    }

    public int GetResourceAmount(WorldResource res) =>
        _inventory[res];

    public void AddResource(WorldResource res, int amount) =>
        _inventory[res] += amount;

    public int ConsumeResource(WorldResource res, int toConsume)
    {
        int consumed = toConsume;
        int amount = _inventory[res];
        if (toConsume > amount)
            consumed = amount;

        _inventory[res] = Mathf.Max(amount - toConsume, 0);
        return consumed;
    }
}
