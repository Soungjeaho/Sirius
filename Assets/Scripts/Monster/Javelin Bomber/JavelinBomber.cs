using UnityEngine;

public class JavelinBomber : BaseMonster
{
    public GameObject bombPrefab;

    protected override void Attack()
    {
        Debug.Log("Á¦ºí¸° ºÕ¹ö°¡ ÆøÅºÀ» ¶³¾î¶ß¸³´Ï´Ù!");
        if (bombPrefab != null)
        {
            Instantiate(bombPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        }
    }
}
