using UnityEngine;
using System.Collections.Generic;

namespace ProjectConductor
{
    public class WirePathManager : MonoBehaviour
    {
        public List<WireSegment> segments = new List<WireSegment>();

        [ContextMenu("Refresh Path")]
        public void RefreshPath()
        {
            segments.Clear();
            foreach (Transform child in transform)
            {
                WireSegment s = child.GetComponent<WireSegment>();
                if (s != null) segments.Add(s);
            }
        }

        private void OnValidate() { RefreshPath(); }

        private void OnDrawGizmos()
        {
            if (transform.childCount < 2) return;
            Gizmos.color = Color.white;
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
            }
        }
    }
}