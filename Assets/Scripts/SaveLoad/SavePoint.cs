using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public int slot = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                SaveLoadManager.Instance.SavePlayer(player, slot);
                Debug.Log($"���̺� ����Ʈ({slot})���� ���� �Ϸ�!");
            }
        }
    }
}
