using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class WorldLocationContext : MonoBehaviour
{
    [SerializeField]
    private WorldGrid _worldGrid = null;
    [SerializeField]
    private List<CharacterBed> _beds = null;
    [SerializeField]
    private List<RegeneratingResource> _resourceList = null;
    [SerializeField]
    private List<ResourceDeposit> _depositsList = null;
    private Dictionary<WorldResource, LinkedList<RegeneratingResource>> _resources = new();
    private Dictionary<WorldResource, LinkedList<ResourceDeposit>> _deposits = new();
    [SerializeField]
    private List<ResourceDeposit> _beerTables = new();


    public ref readonly WorldGrid WorldGrid
    {
        get { return ref _worldGrid; }
    }

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        if (_resourceList is not null)
        {
            foreach (RegeneratingResource r in _resourceList)
            {
                if (!_resources.ContainsKey(r.ResourceType))
                    _resources.Add(r.ResourceType, new LinkedList<RegeneratingResource>());

                _resources[r.ResourceType].AddLast(r);
            }
        }

        if (_depositsList is not null)
        {
            foreach (ResourceDeposit d in _depositsList)
            {
                if (!_deposits.ContainsKey(d.ResourceType))
                    _deposits.Add(d.ResourceType, new LinkedList<ResourceDeposit>());

                _deposits[d.ResourceType].AddLast(d);
            }
        }

        if (_worldGrid is null)
            throw new NullReferenceException("[PATHFINDING] World grid is null for a location ctx");

    }


    public IEnumerable<RegeneratingResource> GetRegeneratingResources(WorldResource type)
    {
        if (_resources is null || !_resources.ContainsKey(type))
            return Enumerable.Empty<RegeneratingResource>();

        return _resources[type];
    }


    public IEnumerable<RegeneratingResource> GetAvailableRegeneratingResources(WorldResource type)
    {
        if (_resources is null || !_resources.ContainsKey(type))
            return Enumerable.Empty<RegeneratingResource>();

        return _resources[type].Where((r) => r.Available);
    }


    public IEnumerable<CharacterBed> GetAvailableBeds()
    {
        return _beds.Where((b) => b.Available);
    }

    
    public IEnumerable<ResourceDeposit> GetDeposits(WorldResource type)
    {
        if (_deposits is null || !_deposits.ContainsKey(type))
            return Enumerable.Empty<ResourceDeposit>();

        return _deposits[type];
    }

    public IEnumerable<ResourceDeposit> GetAvailableDeposits(WorldResource type)
    {
        if (_deposits is null || !_deposits.ContainsKey(type))
            return Enumerable.Empty<ResourceDeposit>();

        return _deposits[type].Where((d) => !d.HasConsumer());
    }


    public IEnumerable<ResourceDeposit> GetAvailableBeerTables()
    {
        return _beerTables.Where((t) => !t.HasConsumer());
    }


    public IEnumerable<ResourceDeposit> GetEmptyBeerTables()
    {
        return _beerTables.Where((t) => t.Amount == 0);
    }
}
