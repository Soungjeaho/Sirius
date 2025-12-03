using System.Collections;
using UnityEngine;

namespace Project.NPC
{
    public enum EnemyState { Idle, Chase, Attack, Skill, Stagger, Dead }

    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Stats")] public int maxHP = 3; public float moveSpeed = 2f; public float gravityScale = 2f;
        [Header("Ranges")] public float detectRange = 8f; public float loseRange = 12f; public float meleeRange = 1.1f;
        [Header("Combat")] public float attackDelay = 1.6f; public int contactDamage = 1; public LayerMask playerLayers;
        [Header("Refs")] public Animator anim; public Rigidbody2D rb; public Transform gfxRoot; public Transform attackOrigin; public AnimParams animParams;

        protected int _hp; protected EnemyState _state; protected Transform _player; protected bool _facingRight = true; protected bool _busy = false; protected float _lastAttack = -999f;
        public bool IsDead => _state == EnemyState.Dead;

        protected virtual void Awake()
        {
            if (!rb) rb = GetComponent<Rigidbody2D>();
            if (!anim) anim = GetComponentInChildren<Animator>();
        }

        protected virtual void OnEnable()
        {
            _hp = maxHP; _state = EnemyState.Idle; rb.gravityScale = gravityScale;
        }

        protected virtual void Update()
        {
            if (IsDead) return;
            AcquirePlayer();
            UpdateFacing();
            StateUpdate();
        }

        protected virtual void FixedUpdate()
        {
            if (_state == EnemyState.Chase && _player) MoveTowards(_player.position);
            else HaltX();
        }

        protected void AcquirePlayer()
        {
            if (_player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p) _player = p.transform;
            }
        }

        protected void UpdateFacing()
        {
            if (!_player || !gfxRoot) return;
            bool right = _player.position.x >= transform.position.x;
            if (right != _facingRight)
            {
                _facingRight = right;
                var s = gfxRoot.localScale; s.x = Mathf.Abs(s.x) * (_facingRight ? 1f : -1f); gfxRoot.localScale = s;
            }
        }

        protected virtual void StateUpdate()
        {
            float d = _player ? Vector2.Distance(transform.position, _player.position) : 999f;
            switch (_state)
            {
                case EnemyState.Idle:
                    if (d <= detectRange) ChangeState(EnemyState.Chase);
                    break;
                case EnemyState.Chase:
                    if (_busy) return;
                    if (d > loseRange) ChangeState(EnemyState.Idle);
                    else if (d <= meleeRange && Time.time >= _lastAttack + attackDelay) TryMelee();
                    else TrySkill(d);
                    break;
            }
        }

        protected void ChangeState(EnemyState s)
        {
            _state = s; animParams.SetMove(anim, s == EnemyState.Chase);
        }

        protected IEnumerator Busy(float sec)
        {
            _busy = true; yield return new WaitForSeconds(sec); _busy = false;
        }

        public virtual void TakeDamage(int dmg = 1)
        {
            if (IsDead) return;
            _hp -= Mathf.Max(1, dmg);
            if (!string.IsNullOrEmpty(animParams.hitTrig)) animParams.Fire(anim, animParams.hitTrig);
            if (_hp <= 0) Die();
        }

        protected virtual void Die()
        {
            _state = EnemyState.Dead;
            animParams.Fire(anim, animParams.dieTrig);
            rb.velocity = Vector2.zero;
            enabled = false;
        }

        protected void MoveTowards(Vector2 target)
        {
            var v = rb.velocity; v.x = Mathf.Sign(target.x - transform.position.x) * moveSpeed; rb.velocity = v;
        }

        protected void HaltX() { rb.velocity = new Vector2(0, rb.velocity.y); }

        protected int DealAoE(Vector2 center, float radius, int damage)
        {
            int hits = 0;
            var arr = Physics2D.OverlapCircleAll(center, radius, playerLayers);
            foreach (var c in arr)
            {
                var d = c.GetComponentInParent<Project.Combat.IDamageable>();
                if (d != null && !d.IsDead)
                {
                    Vector2 normal = Vector2.right * (_facingRight ? 1 : -1);
                    d.ApplyDamage(damage, center, normal, this);
                    hits++;
                }
            }
            return hits;
        }

        protected abstract void TryMelee();
        protected abstract void TrySkill(float distToPlayer);
    }
}
