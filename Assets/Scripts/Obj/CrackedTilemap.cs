using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class CrackedTilemap : MonoBehaviour
{
    private Tilemap tilemap;
    private Dictionary<Vector3Int, int> crackStages = new Dictionary<Vector3Int, int>();

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    // HeavyFloatProjectile이 호출할 함수
    public void OnHeavyHit(Vector2 hitPoint)
    {
        Vector3Int cellPos = tilemap.WorldToCell(hitPoint);
        TileBase baseTile = tilemap.GetTile(cellPos);
        if (baseTile is CrackedTile crackedTile)
        {
            // 현재 깨진 정도 추적
            if (!crackStages.ContainsKey(cellPos))
                crackStages[cellPos] = 0;

            int stage = crackStages[cellPos];
            stage++;

            // 🔹 최대 단계 초과 시 타일 제거
            if (stage >= crackedTile.crackStages.Length)
            {
                tilemap.SetTile(cellPos, null);
                crackStages.Remove(cellPos);
                return;
            }

            // 🔹 새 스프라이트로 교체 (DrawTile 효과)
            CrackedTile newTile = ScriptableObject.Instantiate(crackedTile);
            newTile.sprite = crackedTile.crackStages[stage];
            crackStages[cellPos] = stage;

            tilemap.SetTile(cellPos, newTile); // 실제로 다시 그리기
            tilemap.RefreshTile(cellPos);      // 즉시 반영
        }
    }
}
