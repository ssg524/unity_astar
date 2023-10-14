using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    public Vector3Int playerStartPos;
    public Vector3Int endPos;
    
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
                    Vector3Int wallPos = new Vector3Int(x, y, 0);

                    if (wallPos != playerStartPos && wallPos != endPos)
                        this.GetComponent<Tilemap>().SetTile(wallPos, wallTile);
                } 
            }
        }    
    }

    void Start()
    {
        int playerStartPosX = Random.Range(-6, -1);
        int playerStartPosY = Random.Range(-4, 4);
        int endPosX = Random.Range(1, 6);
        int endPosY = Random.Range(-4, 4);

        playerStartPos = new Vector3Int(playerStartPosX, playerStartPosY, 0);
        player = Instantiate(player, new Vector3(playerStartPosX + 0.5f, playerStartPosY + 0.5f, 0),
            UnityEngine.Quaternion.identity);
        endPos = new Vector3Int(endPosX, endPosY, 0);
        this.GetComponent<Tilemap>().SetTile(endPos, goalTile);

        makeWall();
    }
}
