using UnityEngine;

public class ShovelProjectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float lifetime = 3f;      
    public int damage = 1;           
    public LayerMask hitLayer;       

    private void Start()
    {
        
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (((1 << collision.gameObject.layer) & hitLayer) != 0)
        {
            Debug.Log($"투사체가 {collision.gameObject.name}에게 명중!");

            Destroy(gameObject); 
        }
    }
}
