using UnityEngine;
using UnityEngine.Events;

public class CharacterBed : MonoBehaviour
{
    private CharacterEnergy _owner = null;

    public bool Available
    { 
        get { return _owner is null; } 
    }

    public bool OccupyBed(CharacterEnergy occupier)
    {
        if (_owner is not null)
        {
            Debug.LogWarning("[ENERGY] Trying to occupy an already occupied bed");
            return false;
        }

        _owner = occupier;
        return true;
    }


    public void DeOccupy()
    {
        _owner = null;
    }
}
