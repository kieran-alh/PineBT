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

    void CreateBehaviourTree()
    {
        // Create tree
        tree = new BehaviourTree("ExampleTree");
        blackboard = new Blackboard();

        Service exampleService = new Service("ExampleBB", 2f, ExampleBlackboardService, false, true);

        Action move1 = new Action("Move1", () => Move(point1.position));
        Action move2 = new Action("Move2", () => Move(point2.position));
        Action move3 = new Action("Move3", () => Move(point3.position));
        Action move4 = new Action("Move4", () => Move(point4.position));

        Limiter limiter = new Limiter("Limiter", 5.0f, true, new Action(LimitExample));
        RandomSelector moveSelector = new RandomSelector("MoveSelector", move1, move2, move3, move4);

        Sequence sequence = new Sequence("Sequence", moveSelector, limiter);

        tree.SetRoot(exampleService);
            exampleService.AddChild(sequence);

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

    State LimitExample()
    {
        Debug.Log($"Limiter: {Time.time}");
        return State.SUCCESS;
    }

    void OnDisable()
    {
        tree.Disable();
        blackboard.Disable();
    }
}
