using UnityEngine;

public class Walker : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Transform _constructor;
    private Animation _anim;

    // Use this for initialization
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _constructor = transform.FindChild("Constructor");
        _anim = _constructor.GetComponent<Animation>();

        SetRandomDestination();

        _anim.Play("walk");
    }

    private void SetRandomDestination()
    {
        var maxTries = 64;

        while (true)
        {
            var x = transform.position.x + Random.Range(-32f, 32f);
            var z = transform.position.z + Random.Range(-32f, 32f);

            x = Mathf.Clamp(x, -100, 100);
            z = Mathf.Clamp(z, -100, 100);

            RaycastHit hitInfo;
            if (Physics.Raycast(new Vector3(x, 16, z), Vector3.down, out hitInfo, 20f))
            {
                var point = hitInfo.point;

                _agent.SetDestination(point);
                break;
            }

            maxTries--;
            if (maxTries <= 0)
            {
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_agent.velocity.magnitude < 0.1f)
        {
            _anim.Play("idle");
        }
        else
        {
            _anim.Play("walk");
        }

        if (!_agent.hasPath || _agent.remainingDistance < 0.1f)
        {
            SetRandomDestination();
        }
    }
}
