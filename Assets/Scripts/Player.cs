using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : Entity
{
    public float moveSpeed;

    public Collider2D boundingCollider;

    public Joystick androidInput;
    public EventTrigger androidShoot;

    private float _fireDelay = 0;
    private float _fireDelayMax = 0.05f;
    private bool _isShooting;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        _blockDamageDuringIFrames = true;
        _vulnerableToLayers = LayerMask.GetMask("Enemy");
        Manager.instance.playerInstance = this;

        EventTrigger.TriggerEvent e1 = new EventTrigger.TriggerEvent();
        EventTrigger.TriggerEvent e2 = new EventTrigger.TriggerEvent();

        e1.AddListener((e) => _isShooting = true);
        e2.AddListener((e) => _isShooting = false);

        androidShoot.triggers.Add(new EventTrigger.Entry() { callback = e1, eventID = EventTriggerType.PointerDown });
        androidShoot.triggers.Add(new EventTrigger.Entry() { callback = e2, eventID = EventTriggerType.PointerUp });
    }

    // Update is called once per frame
    public override void Update()
    {
        if (Manager.instance.GetIsPaused())
        {
            return;
        }
        base.Update();
        _fireDelay = _fireDelay - Time.deltaTime;
        if (_fireDelay <= 0 && (Input.GetButton("Fire1") || _isShooting))
        {
            AudioManager.PlaySFX(SFX.PlayerShoot);

            _fireDelay = _fireDelayMax;
            switch (Manager.instance.FrontShot)
            {
                case PowerupStage.Ultra:
                    Shoot(ProjectileType.P_Thin, Vector2.up).transform.position += Vector3.left * 0.2f;
                    Shoot(ProjectileType.P_Thin, Vector2.up).transform.position += Vector3.right * 0.2f;
                    Shoot(ProjectileType.P_Long, Vector2.up).transform.position += Vector3.left * 0.35f;
                    Shoot(ProjectileType.P_Long, Vector2.up).transform.position += Vector3.right * 0.35f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.1f * Vector2.left).transform.position += Vector3.left * 0.6f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.1f * Vector2.right).transform.position += Vector3.right * 0.6f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.3f * Vector2.left).transform.position += Vector3.left * 0.8f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.3f * Vector2.right).transform.position += Vector3.right * 0.8f;
                    break;

                case PowerupStage.High:
                    Shoot(ProjectileType.P_Thin, Vector2.up).transform.position += Vector3.left * 0.25f;
                    Shoot(ProjectileType.P_Thin, Vector2.up).transform.position += Vector3.right * 0.25f;
                    Shoot(ProjectileType.P_Thin, Vector2.up);
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.2f * Vector2.left).transform.position += Vector3.left * 0.6f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.2f * Vector2.right).transform.position += Vector3.right * 0.6f;

                    break;

                case PowerupStage.Mid:
                    Shoot(ProjectileType.P_Thin, Vector2.up).transform.position += Vector3.left * 0.15f;
                    Shoot(ProjectileType.P_Thin, Vector2.up).transform.position += Vector3.right * 0.15f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.1f * Vector2.left).transform.position += Vector3.left * 0.5f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.1f * Vector2.right).transform.position += Vector3.right * 0.5f;
                    break;

                case PowerupStage.Low:
                    Shoot(ProjectileType.P_Long, Vector2.up).transform.position += Vector3.left * 0.3f;
                    Shoot(ProjectileType.P_Long, Vector2.up).transform.position += Vector3.right * 0.3f;
                    Shoot(ProjectileType.P_Thin, Vector2.up);
                    break;

                case PowerupStage.Off:
                    Shoot(ProjectileType.P_Thin, Vector2.up);
                    break;
            }
            switch (Manager.instance.SideShot)
            {
                case PowerupStage.Ultra:
                    Shoot(ProjectileType.P_Thin, Vector2.left + 0.1f * Vector2.up);
                    Shoot(ProjectileType.P_Thin, Vector2.right + 0.1f * Vector2.up);
                    Shoot(ProjectileType.P_Thin, Vector2.left + 0.1f * Vector2.down);
                    Shoot(ProjectileType.P_Thin, Vector2.right + 0.1f * Vector2.down);
                    Shoot(ProjectileType.P_Thin, Vector2.left + 0.3f * Vector2.up);
                    Shoot(ProjectileType.P_Thin, Vector2.right + 0.3f * Vector2.up);
                    Shoot(ProjectileType.P_Thin, Vector2.left + 0.3f * Vector2.down);
                    Shoot(ProjectileType.P_Thin, Vector2.right + 0.3f * Vector2.down);
                    break;

                case PowerupStage.High:
                    Shoot(ProjectileType.P_Thin, Vector2.left + 0.2f * Vector2.up);
                    Shoot(ProjectileType.P_Thin, Vector2.right + 0.2f * Vector2.up);
                    Shoot(ProjectileType.P_Thin, Vector2.left + 0.2f * Vector2.down);
                    Shoot(ProjectileType.P_Thin, Vector2.right + 0.2f * Vector2.down);
                    Shoot(ProjectileType.P_Long, Vector2.left);
                    Shoot(ProjectileType.P_Long, Vector2.right);

                    break;

                case PowerupStage.Mid:
                    Shoot(ProjectileType.P_Thin, Vector2.left + 0.1f * Vector2.up);
                    Shoot(ProjectileType.P_Thin, Vector2.right + 0.1f * Vector2.up);
                    Shoot(ProjectileType.P_Thin, Vector2.left + 0.1f * Vector2.down);
                    Shoot(ProjectileType.P_Thin, Vector2.right + 0.1f * Vector2.down);
                    break;

                case PowerupStage.Low:
                    Shoot(ProjectileType.P_Thin, Vector2.left);
                    Shoot(ProjectileType.P_Thin, Vector2.right);
                    break;

                case PowerupStage.Off:
                    break;
            }
            switch (Manager.instance.BackShot)
            {
                case PowerupStage.Ultra:
                    Shoot(ProjectileType.P_Thin, Vector2.up).transform.position += Vector3.left * 0.35f;
                    Shoot(ProjectileType.P_Thin, Vector2.up).transform.position += Vector3.right * 0.35f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.2f * Vector2.left).transform.position += Vector3.left * 0.6f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.2f * Vector2.right).transform.position += Vector3.right * 0.6f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.4f * Vector2.left).transform.position += Vector3.left * 0.8f;
                    Shoot(ProjectileType.P_Long, Vector2.up + 0.4f * Vector2.right).transform.position += Vector3.right * 0.8f;
                    break;

                case PowerupStage.High:
                    Shoot(ProjectileType.P_Thin, Vector2.down).transform.position += Vector3.left * 0.25f;
                    Shoot(ProjectileType.P_Thin, Vector2.down).transform.position += Vector3.right * 0.25f;
                    Shoot(ProjectileType.P_Long, Vector2.down + 0.2f * Vector2.left).transform.position += Vector3.left * 0.6f;
                    Shoot(ProjectileType.P_Long, Vector2.down + 0.2f * Vector2.right).transform.position += Vector3.right * 0.6f;

                    break;

                case PowerupStage.Mid:
                    Shoot(ProjectileType.P_Thin, Vector2.down);
                    Shoot(ProjectileType.P_Long, Vector2.down + 0.1f * Vector2.left).transform.position += Vector3.left * 0.5f;
                    Shoot(ProjectileType.P_Long, Vector2.down + 0.1f * Vector2.right).transform.position += Vector3.right * 0.5f;
                    break;

                case PowerupStage.Low:
                    Shoot(ProjectileType.P_Thin, Vector2.down).transform.position += Vector3.left * 0.3f;
                    Shoot(ProjectileType.P_Thin, Vector2.down).transform.position += Vector3.right * 0.3f;
                    break;

                case PowerupStage.Off:
                    break;
            }
        }

        rb.velocity = (new Vector2(Input.GetAxis("Horizontal") + Mathf.Clamp(androidInput.Horizontal * 4, -1, 1), Input.GetAxis("Vertical") + Mathf.Clamp(androidInput.Vertical * 4, -1, 1))) * moveSpeed;
        boundingCollider.KeepOnScreen(Manager.instance.mainCam);
    }

    public override void Kill()
    {
        int lives = Manager.instance.SubtractLife();

        if (lives > 0)
        {
            _iframes = iFramesMax;
            Manager.instance.OpenSkillUI();
            AudioManager.PlaySFX(SFX.PlayerKill);
        }
        else
        {
            mainRenderer.enabled = false;
            AudioManager.PlaySFX(SFX.PlayerKillForever);
            Manager.instance.DoGameOver();
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (Manager.instance.GetIsPaused())
        {
            return;
        }
        base.OnTriggerEnter2D(collision);

        if (collision.GetComponent<Entity>() is Entity e)
        {
            if (_vulnerableToLayers.Contains(collision.gameObject.layer) && _iframes <= 0)
            {
                _iframes = iFramesMax;
                hp = hp - 1 / ((float)Manager.instance.Shields + 1);

                if (hp <= 0)
                {
                    Kill();
                }
                else
                {
                    AudioManager.PlaySFX(SFX.PlayerHarm);
                }
            }
        }
    }

    public override Projectile Shoot(ProjectileType type, Vector2 speed)
    {
        if (Manager.instance.Zany == PowerupStage.Low)
        {
            speed = Quaternion.Euler(0, 0, UnityEngine.Random.value * 25 - 12.5f) * speed;
        }
        return base.Shoot(type, speed);
    }
}