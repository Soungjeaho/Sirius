using UnityEngine;
using System.Collections.Generic;

namespace ProjectConductor
{
    public class CurrentSpawner : MonoBehaviour
    {
        [Header("연결 설정")]
        public GameObject currentPrefab;
        public WirePathManager pathManager;

        [Header("스폰 설정")]
        public int spawnCount = 3;

        void Start()
        {
            if (currentPrefab == null || pathManager == null)
            {
                Debug.LogError("Spawner: 프리팹이나 PathManager가 비어있습니다!");
                return;
            }

            SpawnCurrents();
        }

        void SpawnCurrents()
        {
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject obj = Instantiate(currentPrefab, transform.position, Quaternion.identity);
                ElectricCurrent currentScript = obj.GetComponent<ElectricCurrent>();

                if (currentScript != null)
                {
                    ElectricCurrent.CurrentType randomType = (ElectricCurrent.CurrentType)(i % 3);

                    // 핵심 수정: pathManager.waypoints를 pathManager.segments로 변경
                    currentScript.Initialize(randomType, pathManager.segments);
                }
            }
        }
    }
}