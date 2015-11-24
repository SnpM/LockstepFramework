using UnityEngine;

public class DemoMain : MonoBehaviour
{
    public GameObject MainCamera;

    private RtsCamera _rtsCamera;

    public GameObject WalkerPrefab;
    public GameObject SpawnPoofPrefab;

    private int _currWalkerIndex = 0;

    private void Start()
    {
        _rtsCamera = MainCamera.GetComponent<RtsCamera>();
        if (_rtsCamera == null)
            return; // can't really do anything, so we'll bail... avoid lots of null ref exceptions

        _rtsCamera.OnBeginFollow = t =>
                                {
                                    EnableWalkerSelection(t, true);
                                };
        _rtsCamera.OnEndFollow = t =>
                              {
                                  EnableWalkerSelection(t, false);
                              };

        // _rtsCamera.GetTerrainHeight = MyGetTerrainHeightFunction;

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
        if (_rtsCamera == null)
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

        CheckWalkerClick();
    }

    private void CheckWalkerClick()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                var tag = hit.collider.gameObject.tag;
                if (tag == "Walker")
                {
                    _rtsCamera.Follow(hit.collider.gameObject);
                }
            }
        }
    }

    public void CenterCamera()
    {
        if (_rtsCamera == null)
            return; // no camera, bail!

        //_rtsCamera.JumpTo(new Vector3(0, 0, 0));
        _rtsCamera.ResetToInitialValues(true);
    }

    public void SpawnWorker()
    {
        if (_rtsCamera == null)
            return; // no camera, bail!

        var walker = SpawnWalker();
        if (walker != null)
        {
            _rtsCamera.Follow(walker);
        }
    }

    public void TargetRandomWorker()
    {
        if (_rtsCamera == null)
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
                _rtsCamera.Follow(walker);
            }
        }
    }

    //
    // PRIVATE
    //

    private void EnableWalkerSelection(Transform t, bool enable)
    {
        t.FindChild("Selection Projector").GetComponent<Projector>().enabled = enable;
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
