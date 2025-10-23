using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public List<string> acquiredAbilities = new List<string>();
    public List<string> inventoryItems = new List<string>();

    public float health = 100f;
    public float stamina = 50f;
    public float mana = 30f;

    public string currentMapName = "DarkVillage";
}
