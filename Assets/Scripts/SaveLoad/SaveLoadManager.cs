using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;
    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Application.persistentDataPath + "/SaveSlot";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SavePlayer(PlayerController2D player, int slot)
    {
        PlayerData data = new PlayerData(player);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath + slot + ".json", json);
        Debug.Log($"세이브 완료 (슬롯 {slot}) - {savePath + slot}.json");
    }

    public void LoadPlayer(PlayerController2D player, int slot)
    {
        string path = savePath + slot + ".json";
        if (!File.Exists(path))
        {
            Debug.LogWarning("저장 파일이 없습니다.");
            return;
        }

        string json = File.ReadAllText(path);
        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

        Vector2 loadPos = new Vector2(data.positionX, data.positionY);
        player.transform.position = loadPos;

        Debug.Log($"로드 완료 (슬롯 {slot}) - 위치 복원: {loadPos}");
    }

    public bool HasSave(int slot)
    {
        return File.Exists(savePath + slot + ".json");
    }
}
