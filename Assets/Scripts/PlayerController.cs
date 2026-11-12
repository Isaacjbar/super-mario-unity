using UnityEngine;

/// <summary>
/// Platformer player: run, variable-height jump, coyote time, jump buffering,
/// stomping enemies, hitting blocks from below, collecting coins and reaching the goal.
/// Controls: A/D or arrows to move, Space/W/Up to jump, Left Shift to run.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Current { get; private set; }

    const float WalkSpeed = 6f;
    const float RunSpeed = 9f;
    const float Acceleration = 55f;
    const float JumpVelocity = 15.5f;
    const float CoyoteTime = 0.1f;
    const float JumpBuffer = 0.12f;
    const float FallY = -6f;

    Rigidbody2D rb;
    SpriteRenderer sr;
    CapsuleCollider2D col;

    float input;
    bool running;
    bool jumpHeld;
    float coyote;
    float buffer;
    bool grounded;
    bool dead;
    float animTimer;
    int walkFrame;

    void Awake()
    {
        Current = this;

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.PlayerIdle;
        sr.sortingOrder = 10;

        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        col = gameObject.AddComponent<CapsuleCollider2D>();
        col.direction = CapsuleDirection2D.Vertical;
        col.size = new Vector2(0.7f, 0.95f);
        col.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    bool CanPlay => !dead && GameManager.Instance.State == GameState.Playing;

    void Update()
    {
        if (!CanPlay)
        {
            if (!dead) input = 0f;
            return;
        }

        input = Input.GetAxisRaw("Horizontal");
        running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        bool jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        jumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

        grounded = CheckGround();
        coyote = grounded ? CoyoteTime : coyote - Time.deltaTime;
        buffer = jumpPressed ? JumpBuffer : buffer - Time.deltaTime;

        if (buffer > 0f && coyote > 0f)
        {
            var v = rb.linearVelocity;
            v.y = JumpVelocity;
            rb.linearVelocity = v;
            buffer = 0f;
            coyote = 0f;
            SoundFx.Play(SoundFx.Jump);
        }

        // Variable jump height + snappier fall
        float vy = rb.linearVelocity.y;
        rb.gravityScale = vy < 0f ? 4.8f : (jumpHeld ? 3f : 6f);

        if (input != 0f) sr.flipX = input < 0f;
        Animate();

        if (transform.position.y < FallY) Die();
    }

    void FixedUpdate()
    {
        if (dead) return;

        float target = input * (running ? RunSpeed : WalkSpeed);
        var v = rb.linearVelocity;
        v.x = Mathf.MoveTowards(v.x, target, Acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = v;

        // Can't walk back off the left edge of the screen
        float minX = CameraFollow.LeftEdge + 0.5f;
        if (rb.position.x < minX)
        {
            rb.position = new Vector2(minX, rb.position.y);
            if (rb.linearVelocity.x < 0f) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    bool CheckGround()
    {
        Vector2 center = (Vector2)transform.position + Vector2.down * 0.5f;
        var hits = Physics2D.OverlapBoxAll(center, new Vector2(0.55f, 0.12f), 0f);
        foreach (var h in hits)
        {
            if (h == col || h.isTrigger) continue;
            if (h.GetComponent<EnemyPatrol>() != null) continue;
            return true;
        }
        return false;
    }

    void Animate()
    {
        if (!grounded)
        {
            sr.sprite = SpriteFactory.PlayerJump;
            return;
        }
        if (Mathf.Abs(rb.linearVelocity.x) > 0.3f && input != 0f)
        {
            animTimer += Time.deltaTime * (running ? 1.5f : 1f);
            if (animTimer > 0.1f)
            {
                animTimer = 0f;
                walkFrame = 1 - walkFrame;
            }
            sr.sprite = walkFrame == 0 ? SpriteFactory.PlayerWalk1 : SpriteFactory.PlayerWalk2;
        }
        else
        {
            sr.sprite = SpriteFactory.PlayerIdle;
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (dead) return;

        Vector2 delta = c.collider.transform.position - transform.position;

        var enemy = c.collider.GetComponent<EnemyPatrol>();
        if (enemy != null && !enemy.Dead)
        {
            if (delta.y < -0.6f)
            {
                // Stomp
                enemy.Stomp();
                var v = rb.linearVelocity;
                v.y = jumpHeld ? 14f : 10f;
                rb.linearVelocity = v;
            }
            else
            {
                Die();
            }
            return;
        }

        // Head hits a block from below
        var block = c.collider.GetComponent<Block>();
        if (block != null && delta.y > 0.7f && Mathf.Abs(delta.x) < 0.75f)
            block.Hit();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (dead) return;

        var coin = other.GetComponent<Coin>();
        if (coin != null)
        {
            coin.Collect();
            return;
        }

        if (other.GetComponent<Goal>() != null && GameManager.Instance.State == GameState.Playing)
        {
            input = 0f;
            rb.linearVelocity = Vector2.zero;
            GameManager.Instance.Win();
        }
    }

    void Die()
    {
        if (dead) return;
        dead = true;
        input = 0f;
        col.enabled = false;
        rb.gravityScale = 3f;
        rb.linearVelocity = new Vector2(0f, 12f);
        sr.sprite = SpriteFactory.PlayerJump;
        sr.sortingOrder = 50;
        SoundFx.Play(SoundFx.Die);
        Invoke(nameof(NotifyDeath), 1.6f);
    }

    void NotifyDeath() => GameManager.Instance.PlayerDied();
}
