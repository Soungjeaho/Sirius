using System.Collections;
using UnityEngine;

namespace Project.NPC
{
    public class JavelinBomber : EnemyBase
    {
        public Bomb bombPrefab;
        public float throwInterval = 2.4f;
        public float throwSpeed = 6f;
        public float minThrowDist = 2.5f;
        private float _lastThrow;

        protected override void TryMelee()
        {
            // No melee
        }

        protected override void TrySkill(float d)
        {
            if (Time.time >= _lastThrow + throwInterval && d >= minThrowDist)
                StartCoroutine(ThrowBomb());
        }

        IEnumerator ThrowBomb()
        {
            _lastThrow = Time.time; _lastAttack = Time.time; _state = EnemyState.Skill; _busy = true;
            animParams.Fire(anim, animParams.skillATrig);
            yield return new WaitForSeconds(0.25f);
            if (bombPrefab)
            {
                var b = GameObject.Instantiate(bombPrefab);
                Vector2 dir = _player ? (Vector2)(_player.position - transform.position) : Vector2.right * (_facingRight ? 1 : -1);
                dir = (dir + Vector2.up * 0.2f).normalized;
                b.hitLayers = playerLayers; b.damage = 2; b.speed = throwSpeed;
                b.Fire(attackOrigin ? (Vector2)attackOrigin.position : (Vector2)transform.position, dir);
            }
            yield return new WaitForSeconds(0.35f);
            _busy = false; ChangeState(EnemyState.Chase);
        }
    }
}
