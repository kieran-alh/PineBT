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

        // SERVICE
        Service exampleService = new Service("ExampleBB", 2f, 0.25f, ExampleBlackboardService, false, true);

        // MOVES
        Action move1 = new Action("Move1", () => Move(point1.position));
        Action move2 = new Action("Move2", () => Move(point2.position));
        Action move3 = new Action("Move3", () => Move(point3.position));
        Action move4 = new Action("Move4", () => Move(point4.position));
        
        // PARALLELS
        Parallel parallel1 = new Parallel("P1", Parallel.Policy.SEQUENCE, Parallel.Executor.ENTIRE,
            move1,
            new Succeeder(
                new Failer(new Action("Color1", () => ColorAction(color1)))
            )
        );
        Parallel parallel2 = new Parallel("P2", Parallel.Policy.SEQUENCE_CONTINUE, Parallel.Executor.ENTIRE,
            move2,
            new Action("Color1", () => ColorAction(color2))
        );
        Parallel parallel3 = new Parallel("P3",
            move3,
            new Action("Color1", () => ColorAction(color3))
        );
        Parallel parallel4 = new Parallel("P4", Parallel.Policy.SELECTOR_CONTINUE, Parallel.Executor.ENTIRE,
            move4,
            new Action(() => ColorAction(color4))
        );

        // COOLDOWN
        Cooldown limiter = new Cooldown("Limiter", 5.0f, 0.1f, true, new Action(LimitExample));
        // PARALLEL MOVE
        Sequence parallelMove = new Sequence("ParallelMove", parallel1, parallel2, parallel3, parallel4);
        
        // SELECTOR MOVE
        Selector selectorMove = new Selector("SelectMove",
            new Failer(true, move1), move2, move3, move4
        );

        Sequence sequence = new Sequence("Sequence", parallelMove, limiter);

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
