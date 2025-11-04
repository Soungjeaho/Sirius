using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        Debug.Log(gameObject.name + "이(가) " + dmg + " 피해를 입음!");

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
