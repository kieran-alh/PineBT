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

    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;

    private Color prevColor = Color.white;
    private Color curColor;

    private Vector3 prevPosition;
    private Vector3 curPosition;
    private Vector3 nextPosition;
    private MeshRenderer meshRenderer;

    BehaviourTree tree;
    Blackboard blackboard;
    // Start is called before the first frame update
    void Start()
    {
        prevPosition = Vector3.zero;
        meshRenderer = GetComponent<MeshRenderer>();
        CreateBehaviourTree();
    }

    void CreateBehaviourTree()
    {
        // Create tree
        tree = new BehaviourTree("ExampleTree");
        blackboard = new Blackboard();

        Service exampleService = new Service("ExampleBB", 2f, 0.25f, ExampleBlackboardService, false, true);

        Parallel parallel1 = new Parallel("P1", Parallel.Policy.SEQUENCE, Parallel.Executor.REMAINING,
            new Action("Move1", () => Move(point1.position)),
            new Action("Color1", () => ColorAction(color1))
        );
        Parallel parallel2 = new Parallel("P2",
            new Action("Move1", () => Move(point2.position)),
            new Action("Color1", () => ColorAction(color2))
        );
        Parallel parallel3 = new Parallel("P3",
            new Action("Move1", () => Move(point3.position)),
            new Action("Color1", () => ColorAction(color3))
        );
        Parallel parallel4 = new Parallel("P4",
            new Action(() => Move(point4.position)),
            new Action(() => ColorAction(color4))
        );

        Cooldown limiter = new Cooldown("Limiter", 5.0f, 0.1f, true, new Action(LimitExample));
        Sequence moveParent = new Sequence("MoveParent", parallel1, parallel2, parallel3, parallel4);

        Sequence sequence = new Sequence("Sequence", moveParent, limiter);

        tree.SetRoot(exampleService);
            exampleService.AddChild(sequence);

        tree.Enable();
        // blackboard.Enable();
        //
        // blackboard.RegisterListener("test_vector3", BlackboardListener);
        // blackboard["test_vector3"] = new Vector3(1, 2, 3);
    }

    State Move(Vector3 point)
    {
        nextPosition = point;
        if (Vector3.Distance(transform.position, point) > 1f)
        {
            Vector3 direction = point-transform.position;
            direction.y = 0;
            float step = speed * Time.deltaTime;
            transform.position += step * direction.normalized;
            curPosition = transform.position;
            return State.RUNNING;
        }
        prevPosition = curPosition;
        prevColor = curColor;
        return State.SUCCESS;
    }

    State ColorAction(Color color)
    {
        curColor = color;
        float dist = (curPosition - prevPosition).sqrMagnitude / (nextPosition - prevPosition).sqrMagnitude;
        Color next = Color.Lerp(prevColor, curColor, dist);
        meshRenderer.material.color = next;
        return State.SUCCESS;
    }

    void ExampleBlackboardService()
    {
        // Debug.Log($"Timer Count {tree.TreeManager.TotalTimerCount()} Updating Blackboard : {Time.time}");
        // blackboard["test_vector3"] = new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10));
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
