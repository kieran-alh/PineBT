using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PineBT;
using PineBT.Tasks;
using PineBT.Decorators;

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
        Service service = new Service("Example", .5f, ExampleService, false, true);
        RandomSelector moveSelector = new RandomSelector("MoveSelector");
        Action move1 = new Action("Move1", () => Move(point1.position));
        Action move2 = new Action("Move2", () => Move(point2.position));
        Action move3 = new Action("Move3", () => Move(point3.position));
        Action move4 = new Action("Move4", () => Move(point4.position));

        tree.SetRoot(service);
            service.AddChild(moveSelector);
                moveSelector.AddChild(move1);
                moveSelector.AddChild(move2);
                moveSelector.AddChild(move3);
                moveSelector.AddChild(move4);

        tree.Enable();
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

    void ExampleService()
    {
        Debug.Log($"Count {tree.TreeManager.TotalTimerCount()} : {Time.time}");
    }

    void OnDisable()
    {
        tree.Disable();
    }
}
