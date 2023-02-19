using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Entity : MonoBehaviour, IFacingDirection
{
    public float hp;

    internal float _iframes = 0;
    internal float hpMax;
    public float iFramesMax;

    internal bool _blockDamageDuringIFrames = false;

    public Gradient hurtTintGradient;
    public SpriteRenderer mainRenderer;
    public float hurtTintModulus = 0.5f;

    public UnityEvent<Entity> OnDestroyEvent = new UnityEvent<Entity>();

    internal LayerMask _vulnerableToLayers;

    public SFX harmSound;
    public SFX killSound;
    public SFX shootSound;

    public string customData;

    [HideInInspector] public Rigidbody2D rb;

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hpMax = hp;
    }

    public virtual void Start()
    {
    }

    public virtual Projectile Shoot(ProjectileType type, Vector2 speed)
    {
        Projectile p = Manager.instance.refs.GetProjectile(type);
        Projectile pInstance = Instantiate(p);
        pInstance.transform.position = transform.position;
        pInstance.SetSpeed(speed);
        AudioManager.PlaySFX(shootSound);
        return pInstance;
    }

    public virtual void Update()
    {
        if (_iframes > 0)
        {
            _iframes = _iframes - Manager.deltaTime;

            if (_iframes > 0)
            {
                mainRenderer.color = hurtTintGradient.Evaluate((_iframes / hurtTintModulus) % 1);
            }
            else
            {
                mainRenderer.color = Color.white;
            }
        }
    }

    public virtual void SetSpeed(Vector2 speed)
    {
        rb.velocity = speed;
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (Manager.instance.GetIsPaused())
        {
            return;
        }
        if (hp <= 0)
        {
            return;
        }

        if (collision.GetComponent<Projectile>() is Projectile p)
        {
            if (_vulnerableToLayers.Contains(collision.gameObject.layer))
            {
                p.Impact(true);
                if (_iframes > 0 && _blockDamageDuringIFrames)
                {
                    return;
                }
                // iframes are damage block
                float damage = Mathf.Clamp(p.damage * (1 - (Mathf.Max(_iframes, 0) / iFramesMax) * 0.5f), 0, p.damage);
                _iframes = iFramesMax;

                hp = hp - damage;

                if (hp <= 0)
                {
                    Kill();
                }
                else
                {
                    AudioManager.PlaySFX(harmSound);
                }
            }
        }
    }

    public virtual void Kill()
    {
        Destroy(gameObject);
        AudioManager.PlaySFX(killSound);
    }

    public virtual Vector2 GetFacingDirection()
    {
        return rb.velocity;
    }

    public virtual void OnDestroy()
    {
        if (!Manager.instance.GetIsPaused())
        {
            if (OnDestroyEvent != null)
            {
                OnDestroyEvent.Invoke(this);
            }
        }
    }
}