using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PineBT;
using PineBT.Composites;
using PineBT.Tasks;

public class Player : MonoBehaviour
{
    public float speed;
    
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform point4;
    
    BehaviourTree tree;
    // Start is called before the first frame update
    void Start()
    {
        CreateBehaviourTree();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateBehaviourTree()
    {
        tree = new BehaviourTree("ExampleTree");
        RandomSelector root = new RandomSelector("Root");
        Action move1 = new Action("Move1", () => Move(point1.position));
        Action move2 = new Action("Move2", () => Move(point2.position));
        Action move3 = new Action("Move3", () => Move(point3.position));
        Action move4 = new Action("Move4", () => Move(point4.position));

        tree.SetRoot(root);
        root.AddChild(move1);
        root.AddChild(move2);
        root.AddChild(move3);
        root.AddChild(move4);

        PineTreeUnityContext.GetInstance().TreeManager.RegisterTree(tree);
    }

    State Move(Vector3 point)
    {
        if (Vector3.Distance(transform.position, point) > 1f)
        {
            Vector3 direction = point-transform.position;
            direction.y = 0;
            float step = speed * Time.deltaTime;
            transform.position += step * direction.normalized;
            return State.RUNNING;
        }
        return State.SUCCESS;
    }
}
