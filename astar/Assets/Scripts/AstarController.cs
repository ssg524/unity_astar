using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

class Node
{
    public Node nextNode;
    public Node parent;
    public Vector3 pos;

    public float f;
    public float g;
    public float h;

    public Node(Node p, Node n, Vector3 pos)
    {
        nextNode = n;
        parent = p;
        this.pos = pos;

        f = 0;
        g = 0;
        h = 0;
    }
}

public class AstarController : MonoBehaviour
{
    private Vector3 destinationPos;
    private Vector3 startPos;

    private float tileOffset = 0.5f;

    private GameObject player;

    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase historyTile;
    [SerializeField] private TileBase goalTile;
    [SerializeField] private Tilemap tilemap;
    

    void Start()
    {
        TilemapController tileController;
        tileController = tilemap.GetComponent<TilemapController>();
        
        player = GameObject.Find("Player(Clone)");
        startPos = tileController.playerStartPos;
        destinationPos = tileController.endPos;
    }

    public void onClick()
    {
        Start();
        Astar(startPos, destinationPos);
    }
    
    float getDistanceFormula(Vector3 xy1, Vector3 xy2)
    {
        float x1 = xy1.x < xy2.x ? xy1.x : xy2.x;
        float x2 = xy1.x > xy2.x ? xy1.x : xy2.x;

        float y1 = xy1.y < xy2.y ? xy1.y : xy2.y;
        float y2 = xy1.y > xy2.y ? xy1.y : xy2.y;
        
        return (float)Math.Sqrt(Math.Pow(x2-x1, 2) + Math.Pow(y2-y1, 2));
    } // 두 점 사이의 거리 구하는 함수
    
    Node isPosInNode(List<Node> list, Vector3 p)
    {
        foreach (Node lt in list)
        {
            if (lt.pos.x == p.x && lt.pos.y == p.y)
                return lt;
        }

        return null;
    }
    
    void makePath(List<Node> closeList)
    {
        Debug.Log("path make start");
        
        List<Vector3> path = new List<Vector3>();
        Node node = closeList[closeList.Count - 1];

        if (node.pos.x != destinationPos.x && node.pos.y != destinationPos.y)
        {
            Debug.Log("갈 수 없는 목적지입니다.");
            return;
        }
    
        while (node != null)
        {
            path.Add(new Vector3(node.pos.x, node.pos.y, 0));
            node = node.parent;
        }
        
        path.Reverse();
        foreach (Vector3 pos in path)
        {
            tilemap.SetTile(new Vector3Int((int)(pos.x - tileOffset), (int)(pos.y - tileOffset), 0), historyTile);
            player.transform.Translate(pos - player.transform.position);
        }
    } // 도착지에 도착 후, 어떤 경로로 도착했는지 부모 노드를 쫓아간다.

    void Astar(Vector3 startPos, Vector3 endPos)
    {
        List<List<int>> wasd = new List<List<int>>()
        {
            new List<int> { 0, 1 }, //상
            new List<int> { 0, -1 },//하
            new List<int> {-1, 0},  //좌
            new List<int> {1, 0},   //우
            new List<int> {-1, 1},  //왼쪽 위
            new List<int> {1, 1},   //오른쪽 위
            new List<int> {-1, -1}, //왼쪽 아래
            new List<int> {1, -1}   //오른쪽 아래
        }; // 상하좌우 대각선을 검사해주기 위한 리스트
        
        List<Node> openList = new List<Node>();
        List<Node> closeList = new List<Node>();
        
        Node startNode = new Node(null, null, startPos);
        openList.Add(startNode);
        
        Node current;

        while (openList.Count != 0)
        {
            float smallF = float.MaxValue;
            Node smallNode = null;

            int index = 0, minIndex = 0;
            foreach (Node list in openList)
            {
                if (list.f <= smallF)
                {
                    smallNode = list;
                    smallF = list.f;
                    minIndex = index;
                }

                index++;
            }

            current = smallNode;
            //tilemap.SetTile(new Vector3Int((int)(current.pos.x - tileOffset), (int)(current.pos.y - tileOffset), 0), historyTile);
            //Debug.Log("(" + current.pos.x + ", " + current.pos.y + "), f=" + current.f);
            openList.RemoveAt(minIndex);
            closeList.Add(current);
            
            if (current.pos.x == endPos.x && current.pos.y == endPos.y) // 만약 현재 그리드 좌표가 도착지 좌표라면 종료
                break;

            foreach (List<int> dir in wasd)
            {
                Vector3 nextPosition =
                    new Vector3(current.pos.x + dir[0], current.pos.y + dir[1], 0);
                
                if ((nextPosition.x >= -5.5f && nextPosition.x <= 5.5f) &&
                    (nextPosition.y >= -3.5f && nextPosition.y <= 3.5f))
                {
                    if (tilemap.GetTile(new Vector3Int((int)(nextPosition.x - tileOffset), (int)(nextPosition.y - tileOffset), 0)) != wallTile) {
                        Node nextNode = new Node(current, null, nextPosition);
                        current.nextNode = nextNode;
                        
                        if (tilemap.GetTile(new Vector3Int((int)(nextPosition.x - tileOffset),
                                (int)(nextPosition.y - tileOffset), 0)) == goalTile)
                        {
                            nextNode.f = nextNode.g = nextNode.h = 0;
                            openList.Add(nextNode);
                            break;
                        }

                        nextNode.g = current.g;
                        if (dir[1] != 0 && dir[0] != 0)
                            nextNode.g += 1.4f;
                        else
                            nextNode.g += 1f;
                        nextNode.h = getDistanceFormula(nextNode.pos, endPos);
                        nextNode.f = nextNode.g + nextNode.h;
                        // g, h, f 값을 초기화

                        if (isPosInNode(closeList, nextNode.pos) == null)
                        {
                            // closelist에 nextPosition에 해당하는 좌표가 있는지, 즉 해당 좌표의 정보를 가지고 있는지 판단한다. 있으면, 두 번 방문 x
                            Node n = isPosInNode(openList, nextNode.pos); // 없다면, openlist에 있는지 확인한다
                            if (n != null)
                            {
                                if (n.f > nextNode.f)
                                {
                                    // 새로 구한 그리드가 openlist에 있을 경우에 f 값을 보고 더 작은 그리드 정보로 바꾸어준다. 그래야 해당 부모 경로를 거치는 최단경로가 만들어질 수 있다.
                                    n.parent = current;
                                    n.nextNode = null;
                                    n.f = nextNode.f;
                                    n.g = nextNode.g;
                                    n.h = nextNode.h;
                                    current.nextNode = n;
                                }
                            }
                            else
                            {
                                openList.Add(nextNode); // 해당 사항이 없으면 추가
                            }
                        }
                    }
                }
            }
        }
        
        makePath(closeList);
    }
}
