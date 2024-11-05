using UnityEngine;

public interface IResourceOwner
{
    protected bool Occupy(RegeneratingResource resource);
    protected void DeOccupy(); 
}