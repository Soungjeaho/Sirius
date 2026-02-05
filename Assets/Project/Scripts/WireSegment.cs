using UnityEngine;
using System.Collections.Generic;

namespace ProjectConductor
{
    public class WireSegment : MonoBehaviour
    {
        [Header("구역 설정")]
        public string zoneID;

        [Header("연결 설정")]
        public bool isRequiredPlayer;
        public bool isConnected = true;

        // 중요: 반드시 new List...로 초기화가 되어 있어야 Null 에러가 안 납니다.
        public List<ElectricCurrent> currentList = new List<ElectricCurrent>();

        public void EnterCurrent(ElectricCurrent current)
        {
            if (currentList == null) currentList = new List<ElectricCurrent>();

            if (!currentList.Contains(current))
            {
                currentList.Add(current);

                // OverloadManager가 씬에 있는지 확인 후 호출
                if (OverloadManager.Instance != null)
                {
                    OverloadManager.Instance.UpdateZoneCount(zoneID);
                }
            }
        }

        public void ExitCurrent(ElectricCurrent current)
        {
            if (currentList == null) return;

            if (currentList.Contains(current))
            {
                currentList.Remove(current);

                if (OverloadManager.Instance != null)
                {
                    OverloadManager.Instance.UpdateZoneCount(zoneID);
                }
            }
        }
    }
}