using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    Transform eyes;
    [SerializeField]
    Transform player;
    [SerializeField]
    Vector2Int viewGrid;
    [SerializeField]
    float fovScale;
    Vector3[] offsets;
    // Start is called before the first frame update
    void Start()
    {
        eyes = transform.Find("Eyes");
        offsets = new Vector3[viewGrid.x*viewGrid.y];
        Vector2 botLeft = (Vector2)(-viewGrid+Vector2Int.one) * 0.5f*fovScale;

        for(int x = 0; x < viewGrid.x; x++)
        {
            for(int y = 0; y < viewGrid.y; y++)
            {
                offsets[x * viewGrid.y + y] = fovScale*(new Vector2(x,y)) + botLeft;
            }
        }
    }

    void Update()
    {
        LookForPlayer();
    }

    int LookForPlayer()
    {
        int hits = 0;
        Vector3 toPlayer = player.position - eyes.position;
        foreach(Vector3 o in offsets)
        {
            Ray look = new Ray(eyes.position, (Quaternion.LookRotation(toPlayer.normalized) * o / toPlayer.magnitude + toPlayer.normalized) * toPlayer.magnitude * 1.2f);
            if (Physics.Raycast(look, out RaycastHit hit, toPlayer.magnitude*1.1f))
            {
                if (hit.collider.transform.Equals(player))
                {
                    hits++;
                }
            }
        }
        if(hits > 0)
        {
            print(hits);
        }
        

        return hits;
    }
    private void OnDrawGizmos()
    {
        if(eyes == null || player == null)
        {
            return;
        }
        Vector3 toPlayer = player.position - eyes.position;
        foreach(Vector3 o in offsets)
        {
            Gizmos.DrawRay(eyes.position, (Quaternion.LookRotation(toPlayer.normalized)*o/toPlayer.magnitude+toPlayer.normalized)*toPlayer.magnitude*1.2f);
        }
       
    }
}
