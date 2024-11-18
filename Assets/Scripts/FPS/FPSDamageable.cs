using UnityEngine;

public class FPSDamageable : MonoBehaviour
{

    public void TakeDamage(float damage)
    {
        print(name + " took " + damage + " damage.");
    }
}
