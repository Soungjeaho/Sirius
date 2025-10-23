using UnityEngine;

public class HookProjectile : MonoBehaviour
{
    private void Update()
    {
        transform.right = GetComponent<Rigidbody2D>().velocity; 
    }

}
