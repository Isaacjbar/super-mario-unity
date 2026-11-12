using System.Collections;
using UnityEngine;

public enum BlockKind { Brick, Question, Used }

/// <summary>Brick / question block. Hit from below by the player.</summary>
public class Block : MonoBehaviour
{
    public BlockKind Kind;

    bool bumping;

    public void Hit()
    {
        if (Kind == BlockKind.Used) return;

        if (Kind == BlockKind.Question)
        {
            Kind = BlockKind.Used;
            GetComponent<SpriteRenderer>().sprite = SpriteFactory.UsedBlock;
            GameManager.Instance.AddCoin();
            StartCoroutine(CoinPopup());
        }

        if (!bumping) StartCoroutine(Bump());
    }

    IEnumerator Bump()
    {
        bumping = true;
        Vector3 start = transform.position;
        const float duration = 0.12f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float k = Mathf.Sin(t / duration * Mathf.PI);
            transform.position = start + Vector3.up * (0.25f * k);
            yield return null;
        }
        transform.position = start;
        bumping = false;
    }

    IEnumerator CoinPopup()
    {
        var go = new GameObject("CoinPopup");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CoinSprite;
        sr.sortingOrder = 20;
        Vector3 start = transform.position + Vector3.up;
        const float duration = 0.55f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            go.transform.position = start + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 2f);
            go.transform.localScale = new Vector3(Mathf.Abs(Mathf.Cos(t * 18f)), 1f, 1f);
            yield return null;
        }
        Destroy(go);
    }
}
