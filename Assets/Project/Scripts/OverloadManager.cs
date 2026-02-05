using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro를 쓰기 위해 이 줄을 추가해야 합니다!

namespace ProjectConductor
{
    public class OverloadManager : MonoBehaviour
    {
        public static OverloadManager Instance;

        [Header("설정")]
        public float graceTime = 1.5f;
        public TextMeshProUGUI warningText; // Text를 TextMeshProUGUI로 변경

        private Dictionary<string, Coroutine> overloadCoroutines = new Dictionary<string, Coroutine>();

        void Awake()
        {
            if (Instance == null) Instance = this;
            if (warningText != null) warningText.gameObject.SetActive(false);
        }

        public void UpdateZoneCount(string zoneID)
        {
            if (string.IsNullOrEmpty(zoneID)) return;

            int count = 0;
            WireSegment[] allSegments = FindObjectsOfType<WireSegment>();
            foreach (var s in allSegments)
            {
                if (s.zoneID == zoneID) count += s.currentList.Count;
            }

            if (count >= 3)
            {
                if (!overloadCoroutines.ContainsKey(zoneID))
                {
                    overloadCoroutines[zoneID] = StartCoroutine(OverloadCountdown(zoneID));
                    ShowWarning(true, zoneID);
                }
            }
            else
            {
                if (overloadCoroutines.ContainsKey(zoneID))
                {
                    StopCoroutine(overloadCoroutines[zoneID]);
                    overloadCoroutines.Remove(zoneID);
                    ShowWarning(false, "");
                }
            }
        }

        private void ShowWarning(bool show, string zoneID)
        {
            if (warningText == null) return;
            warningText.gameObject.SetActive(show);
            if (show) warningText.text = $"WARNING: OVERLOAD IN {zoneID}!";
        }

        IEnumerator OverloadCountdown(string zoneID)
        {
            yield return new WaitForSeconds(graceTime);
            RestartStage();
        }

        public void RestartStage()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}