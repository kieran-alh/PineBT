using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PineBT;

public class Player : MonoBehaviour
{
    public float speed;
    
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform point4;
    
    BehaviourTree tree;
    Blackboard blackboard;
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
        // Create tree
        tree = new BehaviourTree("ExampleTree");
        blackboard = new Blackboard();

        Service exampleService = new Service("ExampleBB", 2f, ExampleBlackboardService, false, true);
        Service debugService = new Service("Debug", 5f, DebuggerService, false, true);

        RandomSelector moveSelector = new RandomSelector("MoveSelector");
        Action move1 = new Action("Move1", () => Move(point1.position));
        Action move2 = new Action("Move2", () => Move(point2.position));
        Action move3 = new Action("Move3", () => Move(point3.position));
        Action move4 = new Action("Move4", () => Move(point4.position));

        tree.SetRoot(debugService);
            debugService.AddChild(exampleService);
                exampleService.AddChild(moveSelector);
                    moveSelector.AddChild(move1);
                    moveSelector.AddChild(move2);
                    moveSelector.AddChild(move3);
                    moveSelector.AddChild(move4);

        tree.Enable();
        blackboard.Enable();

        blackboard.RegisterListener("test_vector3", BlackboardListener);
        blackboard["test_vector3"] = new Vector3(1, 2, 3);
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

    void ExampleBlackboardService()
    {
        Debug.Log($"Timer Count {tree.TreeManager.TotalTimerCount()} Updating Blackboard : {Time.time}");
        blackboard["test_vector3"] = new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10));
    }

    void BlackboardListener(Blackboard.Type type, object value)
    {
        Vector3 val = (Vector3)value;
        // Debug.Log($"TYPE: {type} : {value.ToString()}");
    }

    void DebuggerService()
    {
        // tree.TreeManager.DebugTimers();
    }

    void OnDisable()
    {
        // TODO: Does there need to be clean up?
        // tree.Disable();
        // blackboard.Disable();
    }
}
