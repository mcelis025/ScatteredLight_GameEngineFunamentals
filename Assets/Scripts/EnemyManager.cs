using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    void Awake()
    {
        RepairSceneObjects();
    }

    void RepairSceneObjects()
    {
        GameObject[] sceneObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject sceneObject in sceneObjects)
        {
            if (sceneObject == null)
            {
                continue;
            }

            if (sceneObject.name.StartsWith("DeathTile"))
            {
                SetUpDeathTile(sceneObject);
            }
            else if (sceneObject.name == "Goal_Next_Level")
            {
                SetUpGoal(sceneObject);
            }
            else if (sceneObject.name.StartsWith("45_Left"))
            {
                SetUpSlideTile(sceneObject, -1f);
            }
            else if (sceneObject.name.StartsWith("45_Right"))
            {
                SetUpSlideTile(sceneObject, 1f);
            }
            else if (sceneObject.name.StartsWith("Horizontal_Limit"))
            {
                SetUpLimitTile(sceneObject);
            }
            else if (sceneObject.name.StartsWith("Vertical_Limit"))
            {
                SetUpLimitTile(sceneObject);
            }
            else if (sceneObject.name.StartsWith("VertTile"))
            {
                SetUpWallTile(sceneObject);
            }
            else if (sceneObject.name == "Scattered_Light")
            {
                SetUpScatteredLight(sceneObject);
            }
        }
    }

    void SetUpDeathTile(GameObject tileObject)
    {
        BoxCollider2D tileCollider = tileObject.GetComponent<BoxCollider2D>();

        if (tileCollider == null)
        {
            tileCollider = tileObject.AddComponent<BoxCollider2D>();
        }

        tileCollider.enabled = true;
        tileCollider.isTrigger = true;

        if (tileObject.GetComponent<DeathTile>() == null)
        {
            tileObject.AddComponent<DeathTile>();
        }
    }

    void SetUpGoal(GameObject goalObject)
    {
        Collider2D goalCollider = goalObject.GetComponent<Collider2D>();

        if (goalCollider == null)
        {
            goalCollider = goalObject.AddComponent<BoxCollider2D>();
        }

        goalCollider.isTrigger = true;

        if (goalObject.GetComponent<Goal>() == null)
        {
            goalObject.AddComponent<Goal>();
        }
    }

    void SetUpSlideTile(GameObject tileObject, float slideDirection)
    {
        SlideSurfaceTile slideTile = tileObject.GetComponent<SlideSurfaceTile>();

        if (slideTile == null)
        {
            slideTile = tileObject.AddComponent<SlideSurfaceTile>();
        }

        slideTile.slideDirection = slideDirection;
    }

    void SetUpScatteredLight(GameObject lightObject)
    {
        CircleCollider2D lightCollider = lightObject.GetComponent<CircleCollider2D>();

        if (lightCollider == null)
        {
            lightCollider = lightObject.AddComponent<CircleCollider2D>();
        }

        lightCollider.isTrigger = true;
        lightCollider.radius = 0.8f;

        if (lightObject.GetComponent<ScatteredLightCollectible>() == null)
        {
            lightObject.AddComponent<ScatteredLightCollectible>();
        }
    }

    void SetUpLimitTile(GameObject limitObject)
    {
        SetUpBlockingTile(limitObject, true);
    }

    void SetUpWallTile(GameObject wallObject)
    {
        SetUpBlockingTile(wallObject, false);
    }

    void SetUpBlockingTile(GameObject tileObject, bool hideVisual)
    {
        BoxCollider2D tileCollider = tileObject.GetComponent<BoxCollider2D>();

        if (tileCollider == null)
        {
            tileCollider = tileObject.AddComponent<BoxCollider2D>();
        }

        tileCollider.enabled = true;
        tileCollider.isTrigger = false;

        LimitTile limitTile = tileObject.GetComponent<LimitTile>();

        if (limitTile == null)
        {
            limitTile = tileObject.AddComponent<LimitTile>();
        }

        limitTile.hideVisual = hideVisual;
        limitTile.ApplySettings();
    }
}
