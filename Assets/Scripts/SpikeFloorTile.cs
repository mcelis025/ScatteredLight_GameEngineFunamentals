using UnityEngine;
using System.Collections.Generic;

public class SpikeFloorTile : MonoBehaviour
{
    public float spikeDelay = 2f;
    public float spikeHeight = 0.5f;
    public Color safeColor = Color.gray;
    public Color dangerColor = Color.red;
    public string customSpikeChildName = "Spikes";

    private bool playerIsStanding;
    private bool spikesAreOut;
    private float standTimer;

    private List<GameObject> spikeObjects = new List<GameObject>();
    private SpriteRenderer tileRenderer;
    private PlayerController playerController;

    void Start()
    {
        tileRenderer = GetComponent<SpriteRenderer>();
        FindOrCreateSpikes();
        SetSpikes(false);
    }

    void Update()
    {
        if (!playerIsStanding || spikesAreOut)
        {
            return;
        }

        standTimer += Time.deltaTime;

        if (standTimer >= spikeDelay)
        {
            SetSpikes(true);
            HurtPlayerIfNeeded();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerIsStanding = true;
            playerController = collision.gameObject.GetComponent<PlayerController>();

            if (spikesAreOut)
            {
                HurtPlayerIfNeeded();
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerIsStanding = false;
            playerController = null;

            if (!spikesAreOut)
            {
                standTimer = 0f;
            }
        }
    }

    void FindOrCreateSpikes()
    {
        spikeObjects.Clear();
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < allChildren.Length; i++)
        {
            Transform child = allChildren[i];

            if (child == transform)
            {
                continue;
            }

            if (child.name.StartsWith(customSpikeChildName))
            {
                spikeObjects.Add(child.gameObject);
            }
        }

        if (spikeObjects.Count > 0)
        {
            return;
        }

        CreateSimpleSpikes();
    }

    void CreateSimpleSpikes()
    {
        GameObject spikeObject = new GameObject("Spikes");
        spikeObject.transform.SetParent(transform);
        spikeObject.transform.localPosition = new Vector3(0f, 0.5f + spikeHeight * 0.5f, 0f);
        spikeObject.transform.localScale = new Vector3(0.8f, spikeHeight, 1f);

        SpriteRenderer spikeRenderer = spikeObject.AddComponent<SpriteRenderer>();
        spikeRenderer.color = dangerColor;

        if (tileRenderer != null)
        {
            spikeRenderer.sprite = tileRenderer.sprite;
            spikeRenderer.sortingOrder = tileRenderer.sortingOrder + 1;
        }

        spikeObjects.Add(spikeObject);
    }

    void SetSpikes(bool active)
    {
        spikesAreOut = active;

        for (int i = 0; i < spikeObjects.Count; i++)
        {
            if (spikeObjects[i] != null)
            {
                spikeObjects[i].SetActive(active);
            }
        }

        if (tileRenderer != null)
        {
            if (active)
            {
                tileRenderer.color = dangerColor;
            }
            else
            {
                tileRenderer.color = safeColor;
            }
        }
    }

    void HurtPlayerIfNeeded()
    {
        if (playerController != null)
        {
            playerController.TakeDamage(1);
        }
    }
}
