using UnityEngine;

public class ExplodeableDestroy : MonoBehaviour

// Explodeable simple link
{
    public void Explode()
    {
        Destroy(gameObject);
    }
}