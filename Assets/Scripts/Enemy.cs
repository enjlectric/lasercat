using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    public int score;

    private float despawnTimer = -1;

    public enum EnemyType
    {
        Default,
        Train,
        Fly,
        Airplane,
        Fridge,
        Item,
        HWave,
        VWave,
        HUTurn,
        VUTurn,
        Quadcopter,
        RoomRoom,
        Balloon,
        LaserPointer,
        Trainpiece,
    }

    private Dictionary<EnemyType, Action> _enemyUpdateBehaviours;
    private Dictionary<EnemyType, Action> _enemyDeathBehaviours;

    public EnemyType type;
    public Vector2 facingDirection = Vector2.down;
    public bool shootsProjectiles;
    public bool facesPlayer;
    public float shotInterval = 1;
    internal int _numericState = 0;

    internal float shotTimer = 0;

    private AudioSource _persistentSound;

    internal Vector2 _aiVector1;
    internal float _aiFloat1;
    internal Animator _animator;

    public override void Awake()
    {
        base.Awake();

        _enemyUpdateBehaviours = new Dictionary<EnemyType, Action>()
        {
            [EnemyType.Fly] = AI_Fly,
            [EnemyType.Airplane] = AI_Plane,
            [EnemyType.HWave] = AI_HWave,
            [EnemyType.VWave] = AI_VWave,
            [EnemyType.HUTurn] = AI_HUTurn,
            [EnemyType.VUTurn] = AI_VUTurn,
            [EnemyType.Quadcopter] = AI_Quadcopter,
            [EnemyType.RoomRoom] = AI_RoomRoom,
            [EnemyType.LaserPointer] = AI_LaserPointer,
        };

        _enemyDeathBehaviours = new Dictionary<EnemyType, Action>()
        {
            [EnemyType.Fridge] = Death_Fridge,
            [EnemyType.Train] = Death_Train,
            [EnemyType.Trainpiece] = Death_Train,
            [EnemyType.Balloon] = Death_Balloon,
        };
        despawnTimer = 0;
    }

    public void OverrideType(EnemyType e)
    {
        type = e;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        _animator = GetComponent<Animator>();
        base.Start();
        _vulnerableToLayers = LayerMask.GetMask("Player");

        switch (type)
        {
            case EnemyType.Train:
                int childCount = 0;
                Transform rootTransform = transform;
                foreach (char c in customData)
                {
                    childCount++;
                    Entity e = Manager.instance.refs.GetTrainPiece(c);
                    if (e != null)
                    {
                        Entity eInstance = Instantiate(e, transform.position + Vector3.left * 1.28f * Mathf.Sign(rb.velocity.x) * childCount, Quaternion.identity, rootTransform);
                        eInstance.SetSpeed(rb.velocity);
                        rootTransform = eInstance.transform;
                        var ev = eInstance.GetComponent<OnscreenTriggerable>().OnScreenExitEvents;
                        ev.AddListener(cam => InitiateDespawn());
                    }
                }
                break;

            case EnemyType.HWave:
                _aiVector1 = new Vector2(UnityEngine.Random.value * 4, rb.velocity.y);
                break;

            case EnemyType.VWave:
                _aiVector1 = new Vector2(UnityEngine.Random.value * 4, rb.velocity.x);
                break;

            case EnemyType.HUTurn:
                _aiFloat1 = Mathf.Sign(rb.velocity.x);
                break;

            case EnemyType.VUTurn:
                _aiFloat1 = Mathf.Sign(rb.velocity.y);
                break;

            case EnemyType.RoomRoom:
                AudioManager.PlaySFX(SFX.RoomCry);
                break;

            case EnemyType.LaserPointer:
                AudioManager.PlaySFX(SFX.LaserCry);
                break;

            case EnemyType.Fly:
                _persistentSound = AudioManager.PlaySFX(SFX.FlyBuzz);
                break;

            case EnemyType.Quadcopter:
                if (customData.Length > 0)
                {
                    _aiFloat1 = 1;
                }
                else
                {
                    _aiFloat1 = -1;
                }
                AudioManager.PlaySFX(SFX.Quadcopter);
                break;

            default:
                break;
        }
    }

    public override void Update()
    {
        base.Update();

        if (facesPlayer)
        {
            facingDirection = Manager.instance.playerInstance.transform.position - transform.position;
            facingDirection.Normalize();
        }

        if (_enemyUpdateBehaviours.ContainsKey(type))
        {
            _enemyUpdateBehaviours[type]();
        }

        if (despawnTimer >= 0)
        {
            despawnTimer += Manager.deltaTime;
            if (despawnTimer >= 6)
            {
                Despawn();
            }
        }
        else
        {
            if (shootsProjectiles)
            {
                shotTimer = shotTimer + Manager.deltaTime;
                if (shotTimer > shotInterval)
                {
                    shotTimer = shotTimer - shotInterval;
                    Shoot();
                }
            }
        }
    }

    public virtual void Shoot()
    {
        Shoot(ProjectileType.E_Small, facingDirection);
    }

    public override void Kill()
    {
        GetComponent<OnscreenTriggerable>().Unregister();
        if (_enemyDeathBehaviours.ContainsKey(type))
        {
            _enemyDeathBehaviours[type]();
        }
        else
        {
            base.Kill();
        }

        Manager.instance.SpawnEffect(EffectType.EnemyDeath, transform.position);
        Manager.instance.AddScore(score, transform.position);

        Manager.instance.NotifyEnemyKilled(transform);
    }

    public void Death_Fridge()
    {
        SetSpeed(Vector2.up * 0.001f);
        StartCoroutine(SpawnBonusThenDie(2));
    }

    public void Death_Balloon()
    {
        for (int i = 45; i < 360; i += 90)
        {
            facingDirection = Vector2.up.Rotate(i) * 0.5f;
            Shoot();
        }
        base.Kill();
    }

    public void Death_Train()
    {
        foreach (Transform t in transform)
        {
            if (t.GetComponent<Enemy>() is Enemy e)
            {
                e.Kill();
            }
        }

        base.Kill();
    }

    private IEnumerator SpawnBonusThenDie(int bonusToSpawn)
    {
        for (int i = 0; i < bonusToSpawn; i++)
        {
            Collectible bonus = Instantiate(Manager.instance.refs.collectibles.GetRandom());
            bonus.SetSpeed(new Vector2(UnityEngine.Random.Range(-4, 4), 4));
            bonus.transform.position = transform.position;
            yield return new WaitForSeconds(0.2f);
        }
        yield return gameObject.DespawnFlicker(mainRenderer);
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_persistentSound != null)
        {
            _persistentSound.Stop();
        }
    }

    public void InitiateDespawn()
    {
        despawnTimer = 4;
    }

    public void DeinitiateDespawn()
    {
        despawnTimer = -1;
    }

    public override Vector2 GetFacingDirection()
    {
        if (facesPlayer)
        {
            return facingDirection;
        }
        else
        {
            return base.GetFacingDirection();
        }
    }

    public void SetState(int state)
    {
        _numericState = state;
    }

    public void AI_Fly()
    {
        Vector2 playerLocation = Manager.instance.playerInstance.transform.position;
        Vector2 distance = (Vector2)transform.position - playerLocation;

        if (_numericState == 0)
        {
            SetSpeed(distance.normalized * -8);

            if (distance.magnitude < 3)
            {
                SetState(1);
                _aiVector1 = distance.normalized * 3;
                _aiFloat1 = 0;
                SetSpeed(Vector2.zero);
            }
        }
        else
        {
            _aiFloat1 += Manager.deltaTime;
            transform.position = playerLocation + _aiVector1.Rotate(_aiFloat1 * 180);
        }
    }

    public void AI_Plane()
    {
        Vector2 triggerLocation = Manager.instance.playerInstance.transform.position + 6 * Vector3.up;
        Vector2 distance = (Vector2)transform.position - triggerLocation;

        if (_numericState == 0)
        {
            SetSpeed(rb.velocity * 1.001f);

            if (transform.position.y <= triggerLocation.y)
            {
                SetState(1);
                _aiFloat1 = 1 * Mathf.Sign(distance.x);
                _aiVector1 = distance;
            }
        }
        else
        {
            _aiFloat1 += Manager.deltaTime * Mathf.Sign(_aiFloat1);
            if (Mathf.Abs(_aiFloat1) < 1.5f)
            {
                Vector2 currentVector = _aiVector1;
                _aiVector1 = _aiVector1.Rotate(_aiFloat1 * -60 * Manager.deltaTime);
                SetSpeed((_aiVector1 - currentVector).normalized * 5f);
            }
        }
    }

    public void AI_HWave()
    {
        SetSpeed(new Vector2(rb.velocity.x, Mathf.Sin(_aiVector1.x + Time.time * 5) * _aiVector1.y));
    }

    public void AI_VWave()
    {
        SetSpeed(new Vector2(Mathf.Sin(_aiVector1.x + Time.time * 5) * _aiVector1.y, rb.velocity.y));
    }

    // They... don't actually u-turn, but the effect is cool, so I'm keeping it.
    public void AI_HUTurn()
    {
        SetSpeed(rb.velocity - Vector2.left * _aiFloat1 * Manager.deltaTime);
    }

    public void AI_VUTurn()
    {
        SetSpeed(rb.velocity - Vector2.down * _aiFloat1 * Manager.deltaTime);
    }

    public void AI_Quadcopter()
    {
        float value = Mathf.Abs(_aiFloat1) + 180f * Manager.deltaTime;

        _aiFloat1 = value * Mathf.Sign(_aiFloat1);

        facingDirection = Vector2.down.Rotate(_aiFloat1);

        if (rb.velocity.magnitude > 2)
        {
            shotTimer = 0;
        }
    }

    public void AI_RoomRoom()
    {
        Vector2 playerLocation = Manager.instance.playerInstance.transform.position;
        Vector2 distance = (Vector2)transform.position - playerLocation;
        Vector2 distance2 = (Vector2)transform.position + distance.normalized * -4 * Manager.deltaTime - playerLocation;

        Rect cameraBounds = Manager.instance.mainCam.GetCameraBounds();
        if (transform.position.x < cameraBounds.xMin || transform.position.x > cameraBounds.xMax)
        {
            transform.position = transform.position - Vector3.right * Mathf.Sign(transform.position.x - cameraBounds.center.x) * 0.2f;
            SetSpeed(new Vector2(-rb.velocity.x, rb.velocity.y));
            AudioManager.PlaySFX(SFX.RoomBonk);
        }
        if (transform.position.y < cameraBounds.yMin || transform.position.y > cameraBounds.yMax)
        {
            transform.position = transform.position - Vector3.up * Mathf.Sign(transform.position.y - cameraBounds.center.y) * 0.2f;
            SetSpeed(new Vector2(rb.velocity.x, -rb.velocity.y));
            AudioManager.PlaySFX(SFX.RoomBonk);
        }

        Vector2 newSpeed;

        float mult = 1 + 1 - (hp / hpMax);

        transform.localScale = Vector3.one * mult;

        if (distance2.magnitude > distance.magnitude)
        {
            newSpeed = rb.velocity * 0.5f * Manager.deltaTime + distance.normalized * -12 * Manager.deltaTime;
        }
        else
        {
            newSpeed = rb.velocity + distance.normalized * -6 * Manager.deltaTime;
        }

        SetSpeed(newSpeed);
    }

    public void AI_LaserPointer()
    {
        _aiFloat1 = _aiFloat1 + Manager.deltaTime * (1 + 1 - (hp / hpMax));

        if (_numericState == 0)
        {
            facingDirection = Vector2.up;
            if (_aiFloat1 >= 2)
            {
                SetState(1);
                _animator.SetBool("charging", true);
                AudioManager.PlaySFX(SFX.LaserWindup);
                _aiFloat1 = 0;
            }
        }
        else if (_numericState == 1)
        {
            facingDirection = Vector2.up;
            if (_aiFloat1 >= 1)
            {
                SetState(2);
                _aiFloat1 = 0;
                _persistentSound = AudioManager.PlaySFX(SFX.LaserShoot);
            }
        }
        else
        {
            facingDirection = Vector2.up.Rotate(Mathf.Sin(_aiFloat1 * 0.5f) * 30f);
        }
    }
}