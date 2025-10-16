using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject inventoryUI;
    public GameObject mapUI;
    public GameObject menuUI;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ToggleInventory()
    {
        if (inventoryUI == null) return;
        inventoryUI.SetActive(!inventoryUI.activeSelf);
    }

    public void ToggleMap()
    {
        if (mapUI == null) return;
        mapUI.SetActive(!mapUI.activeSelf);
    }

    public void ToggleMenu()
    {
        if (menuUI == null) return;
        menuUI.SetActive(!menuUI.activeSelf);
    }
}
