using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class PlayerController : MonoBehaviour
{
    private Vector3 destination;
    private Camera cam;
    private bool isMove = false;

    [SerializeField] private GameObject walkTile;

    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMove)
        {
            float desX = (int)cam.ScreenToWorldPoint(Input.mousePosition).x;
            float desY = (int)cam.ScreenToWorldPoint(Input.mousePosition).y;
            
            desX = (desX < 0) ? desX - 0.5f : desX + 0.5f;
            desY = (desY < 0) ? desY - 0.5f : desY + 0.5f;

            if (desX == 0.5f && cam.ScreenToWorldPoint(Input.mousePosition).x < 0)
                desX = -desX;
            
            if (desY == 0.5f && cam.ScreenToWorldPoint(Input.mousePosition).y < 0)
                desY = -desY;
            
            destination = new Vector3(desX, desY, 0);

            if ((destination.x >= -5.5f && destination.x <= 5.5f) && (destination.y >= -2.5f && destination.y <= 2.5f))
            {
                isMove = true;
                Astar(transform.position, destination);
            }
        }
    }
    
    float getDistanceFormula(Vector3 xy1, Vector3 xy2)
    {
        float x1 = xy1.x < xy2.x ? xy1.x : xy2.x;
        float x2 = xy1.x > xy2.x ? xy1.x : xy2.x;

        float y1 = xy1.y < xy2.y ? xy1.y : xy2.y;
        float y2 = xy1.y > xy2.y ? xy1.y : xy2.y;
        
        return (float)Math.Sqrt(Math.Pow(x2-x1, 2) + Math.Pow(y2-y1, 2));
    } // 두 점 사이의 거리 구하는 함수
    
    Node find(List<Node> list, Vector3 p)
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
        List<Vector3> path = new List<Vector3>();
        closeList.Reverse();
        Node node = closeList[0];

        while (node.parent != null)
        {
            path.Add(new Vector3(node.pos.x, node.pos.y, 0));
            node = node.parent;
        }
        
        GameObject walk = Instantiate(walkTile, transform.parent);
        walk.transform.position = transform.position;
        path.Reverse();
        foreach (Vector3 pos in path)
        {
            transform.Translate(pos - transform.position);
            walk = Instantiate(walkTile, transform.parent);
            walk.transform.position = pos;

        }
    } // 도착지에 도착 후, 어떤 경로로 도착했는지 부모 노드를 쫓아간다.

    void Astar(Vector3 startPos, Vector3 endPos)
    {
        Debug.Log("Astar start");
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
                if (list.f < smallF)
                {
                    smallNode = list;
                    smallF = list.f;
                    minIndex = index;
                }

                index++;
            }

            current = smallNode;
            openList.RemoveAt(minIndex);
            closeList.Add(current);
            
            if (current.pos.x == endPos.x && current.pos.y == endPos.y) // 만약 현재 그리드 좌표가 도착지 좌표라면 종료
                break;

            foreach (List<int> dir in wasd)
            {
                Vector3 nextPosition =
                    new Vector3(current.pos.x + dir[0], current.pos.y + dir[1], 0);

                if ((nextPosition.x >= -5.5f && nextPosition.x <= 5.5f) &&
                    (nextPosition.y >= -2.5f && nextPosition.y <= 2.5f))
                {

                    Node nextNode = new Node(current, null, nextPosition);
                    current.nextNode = nextNode;

                    nextNode.g = current.g;
                    if (dir[1] != 0 && dir[0] != 0)
                        nextNode.g += 1.4f;
                    else
                        nextNode.g += 1f;

                    nextNode.h = getDistanceFormula(nextPosition, endPos);
                    nextNode.f = nextNode.g + nextNode.h;
                    // g, h, f 값을 초기화

                    if (find(closeList, nextPosition) == null)
                    {
                        // closelist에 nextPosition에 해당하는 좌표가 있는지, 즉 해당 좌표의 정보를 가지고 있는지 판단한다. 있으면, 두 번 방문 x
                        Node n = find(openList, nextPosition); // 없다면, openlist에 있는지 확인한다.
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
                                continue;
                            }
                        }
                        openList.Add(nextNode); // 해당 사항이 없으면 추가
                    }
                }
            }
        }
        
        makePath(closeList);
        isMove = false;
    }
}
