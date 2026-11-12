using System.Collections;
using UnityEngine;

/// <summary>Goomba: walks in one direction, turns around on walls, dies when stomped.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    const float Speed = 2f;
    const float WakeDistance = 16f;

    public bool Dead { get; private set; }

    Rigidbody2D rb;
    SpriteRenderer sr;
    BoxCollider2D box;
    float direction = -1f;
    float animTimer;

    void Awake()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Goomba;
        sr.sortingOrder = 8;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.simulated = false; // sleeps until the player gets close

        box = gameObject.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.9f, 0.9f);
        box.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
    }

    void Update()
    {
        if (Dead) return;

        var player = PlayerController.Current;
        if (player != null && !rb.simulated && Mathf.Abs(player.transform.position.x - transform.position.x) < WakeDistance)
            rb.simulated = true;

        animTimer += Time.deltaTime;
        if (animTimer > 0.22f)
        {
            animTimer = 0f;
            sr.flipX = !sr.flipX;
        }

        if (transform.position.y < -8f) Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (Dead || !rb.simulated) return;
        rb.linearVelocity = new Vector2(direction * Speed, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (Dead) return;
        if (c.collider.GetComponent<PlayerController>() != null) return;

        // Turn around when hitting something on the side
        foreach (var contact in c.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.6f)
            {
                direction = contact.normal.x > 0 ? 1f : -1f;
                break;
            }
        }
    }

    public void Stomp()
    {
        if (Dead) return;
        Dead = true;
        box.enabled = false;
        rb.simulated = false;
        transform.localScale = new Vector3(1f, 0.3f, 1f);
        transform.position += Vector3.down * 0.35f;
        SoundFx.Play(SoundFx.Stomp);
        GameManager.Instance.AddScore(100);
        StartCoroutine(Vanish());
    }

    IEnumerator Vanish()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
