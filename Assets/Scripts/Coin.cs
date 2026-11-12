using UnityEngine;

/// <summary>Collectible coin (trigger). Spins by squashing its width.</summary>
public class Coin : MonoBehaviour
{
    float seed;

    void Start() => seed = transform.position.x * 0.37f;

    void Update()
    {
        float w = Mathf.Abs(Mathf.Cos(Time.time * 6f + seed));
        transform.localScale = new Vector3(Mathf.Max(0.15f, w), 1f, 1f);
    }

    public void Collect()
    {
        GameManager.Instance.AddCoin();
        Destroy(gameObject);
    }
}
