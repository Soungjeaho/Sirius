using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject inventoryUI;
    public GameObject mapUI;
    public GameObject menuUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ToggleInventory()
    {
        if (inventoryUI != null)
            inventoryUI.SetActive(!inventoryUI.activeSelf);
    }

    public void ToggleMap()
    {
        if (mapUI != null)
            mapUI.SetActive(!mapUI.activeSelf);
    }

    public void ToggleMenu()
    {
        if (menuUI != null)
            menuUI.SetActive(!menuUI.activeSelf);
    }
}
