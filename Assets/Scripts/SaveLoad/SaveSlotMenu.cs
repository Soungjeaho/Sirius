using UnityEngine;
using UnityEngine.UI;

public class SaveSlotMenu : MonoBehaviour
{
    public Button[] saveButtons;
    public Button[] loadButtons;
    private PlayerController2D player;

    void Start()
    {
        player = FindObjectOfType<PlayerController2D>();

        for (int i = 0; i < saveButtons.Length; i++)
        {
            int slot = i + 1;
            saveButtons[i].onClick.AddListener(() => SaveSlot(slot));
            loadButtons[i].onClick.AddListener(() => LoadSlot(slot));
        }
    }

    void SaveSlot(int slot)
    {
        SaveLoadManager.Instance.SavePlayer(player, slot);
    }

    void LoadSlot(int slot)
    {
        SaveLoadManager.Instance.LoadPlayer(player, slot);
    }
}
