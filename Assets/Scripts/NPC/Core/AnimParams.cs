using UnityEngine;

namespace Project.NPC
{
    [System.Serializable]
    public struct AnimParams
    {
        public string moveBool;
        public string attackTrig;
        public string skillATrig;
        public string skillBTrig;
        public string hitTrig;
        public string dieTrig;

        public void SetMove(Animator a, bool v)
        {
            if (a && !string.IsNullOrEmpty(moveBool)) a.SetBool(moveBool, v);
        }
        public void Fire(Animator a, string trig)
        {
            if (a && !string.IsNullOrEmpty(trig)) a.SetTrigger(trig);
        }
    }
}
