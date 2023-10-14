using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    public Vector3 playerStartPos;
    public Vector3 endPos;
    
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase goalTile;
    [SerializeField] private GameObject player;

    void makeWall()
    {
        for (int y = -4; y < 4; y++)
        {
            for (int x = -6; x < 6; x++)
            {
                if (Random.Range(0, 3) == 0)
                {
                    Vector3 wallPos = new Vector3(x + 0.5f, y + 0.5f, 0);

                    if (wallPos != playerStartPos && wallPos != endPos)
                        this.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), wallTile);
                } 
            }
        }    
    }

    void Start()
    {
        float playerStartPosX = Random.Range(-6, -1) + 0.5f;
        float playerStartPosY = Random.Range(-4, 4) + 0.5f;
        float endPosX = Random.Range(1, 6) + 0.5f;
        float endPosY = Random.Range(-4, 4) + 0.5f;

        playerStartPos = new Vector3(playerStartPosX, playerStartPosY, 0);
        player = Instantiate(player, new Vector3(playerStartPosX, playerStartPosY, 0),
            UnityEngine.Quaternion.identity);
        endPos = new Vector3(endPosX, endPosY, 0);
        this.GetComponent<Tilemap>().SetTile(new Vector3Int((int)(endPos.x - 0.5f), (int)(endPos.y - 0.5f), 0), goalTile);

        makeWall();
    }
}
