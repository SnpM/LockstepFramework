using UnityEngine;

public class DemoKeys : MonoBehaviour
{
    public GameObject MainCamera;

    private RtsCamera _scCamera;

    public GameObject WalkerPrefab;
    public GameObject SpawnPoofPrefab;

    private int _currWalkerIndex = 0;

    private void Start()
    {
        _scCamera = MainCamera.GetComponent<RtsCamera>();
        if (_scCamera == null)
            return; // can't really do anything, so we'll bail... avoid lots of null ref exceptions

        _scCamera.OnBeginFollow = t =>
                                {
                                    EnableWalkerSelection(t, true);
                                };
        _scCamera.OnEndFollow = t =>
                              {
                                  EnableWalkerSelection(t, false);
                              };

        // _scCamera.GetTerrainHeight = MyGetTerrainHeightFunction;

        //
        // give each walker a unique name (number)
        //

        var walkers = GameObject.FindGameObjectsWithTag("Walker");
        if (walkers != null && walkers.Length > 0)
        {
            for (var i = 0; i < walkers.Length; i++)
            {
                var walkerName = "Worker " + (i + 1);
                SetWalkerName(walkers[i], walkerName);
            }
        }
    }

    private static void SetWalkerName(GameObject walker, string walkerName)
    {
        if (walker == null)
            return;

        walker.gameObject.name = walkerName;
        walker.transform.FindChild("NameTag").GetComponent<GUIText>().text = walkerName;
    }

    private float MyGetTerrainHeightFunction(float x, float z)
    {
        return 0;
    }

    protected void Update()
    {
        if (_scCamera == null)
            return; // no camera, bail!

        if (Input.GetKeyDown(KeyCode.T))
        {
            CenterCamera();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TargetRandomWorker();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            SpawnWorker();
        }
    }

    public void CenterCamera()
    {
        if (_scCamera == null)
            return; // no camera, bail!

        _scCamera.JumpTo(new Vector3(0, 0, 0));
    }

    public void SpawnWorker()
    {
        if (_scCamera == null)
            return; // no camera, bail!

        var walker = SpawnWalker();
        if (walker != null)
        {
            _scCamera.Follow(walker);
        }
    }

    public void TargetRandomWorker()
    {
        if (_scCamera == null)
            return; // no camera, bail!

        var walkers = GameObject.FindGameObjectsWithTag("Walker");
        if (walkers != null && walkers.Length > 0)
        {
            _currWalkerIndex++;
            if (_currWalkerIndex >= walkers.Length)
                _currWalkerIndex = 0;

            var walker = walkers[_currWalkerIndex];
            if (walker != null)
            {
                _scCamera.Follow(walker);
            }
        }
    }

    //
    // PRIVATE
    //

    private void EnableWalkerSelection(Transform t, bool enable)
    {
        t.FindChild("Blob Shadow Projector").GetComponent<Projector>().enabled = enable;
        t.FindChild("NameTag").GetComponent<GUIText>().enabled = enable;
    }

    private GameObject SpawnWalker()
    {
        if (WalkerPrefab == null)
            return null;

        var x = Random.Range(-100, 100);
        var z = Random.Range(-100, 100);

        var y = GetHeightAt(x, z);

        var pos = new Vector3(x, y, z);
        var walker = (GameObject)Instantiate(WalkerPrefab, pos, Quaternion.identity);

        var walkers = GameObject.FindGameObjectsWithTag("Walker");
        SetWalkerName(walker, "Worker " + walkers.Length);

        var parent = GameObject.Find("Walkers");
        walker.transform.parent = parent.transform;

        AddSpawnPoof(pos);

        return walker;
    }

    private void AddSpawnPoof(Vector3 position)
    {
        if (SpawnPoofPrefab == null)
            return;

        Instantiate(SpawnPoofPrefab, position, Quaternion.identity);
    }

    private float GetHeightAt(float x, float z)
    {
        const int layerMask = 1 << 8;   // "Walkable" layer

        RaycastHit hitInfo;
        if (Physics.Raycast(new Vector3(x, 20f, z), new Vector3(0, -1, 0), out hitInfo, 30f, layerMask))
        {
            return hitInfo.point.y;
        }

        return 0;
    }
}
