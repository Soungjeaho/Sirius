using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float positionX;
    public float positionY;

    public PlayerData(PlayerController2D player)
    {
        positionX = player.transform.position.x;
        positionY = player.transform.position.y;
    }
}
