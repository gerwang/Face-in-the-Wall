using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private List<GameObject> camerasPos;
    [SerializeField] private GameObject facesPos;
    [SerializeField] private GameObject globalCameraPos;
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject facePrefab;
    private int _currentPieceIndex = 0;
    [SerializeField] public GameObject _mainCamera;
    [SerializeField] private FaceCaptureSDK faceCapturor;

    private List<GameObject> faceFathers = new List<GameObject>();
    
    private List<float> durations = new List<float>();
    private List<bool> havingFaceFrames = new List<bool>(30);
    private bool haveFace = false;

    public CameraLook cameraLook;
    
    // Start is called before the first frame update
    
    IEnumerator Start()
    {
        cameraLook = _mainCamera.GetComponent<CameraLook>();
        Cursor.visible = false;
        while (true)
        {
            yield return StartNewWall();
            yield return ReplayWall();
        }
    }

    // Update is called once per frame
    void Update()
    {
        havingFaceFrames.Add(faceCapturor.haveFace);
        if (havingFaceFrames.Count > 30)
        {
            havingFaceFrames.RemoveAt(0);
        }
        int havingFaceCount = 0;
        foreach (var b in havingFaceFrames)
        {
            havingFaceCount += b ? 1 : 0;
        }

        if (havingFaceCount > 10)
        {
            haveFace = true;
        }
        else
        {
            haveFace = false;
        }
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }
    
    
    private IEnumerator StartNewWall()
    {
        yield return MainCamMovesTo(globalCameraPos, 2);
        
        for (int i = 0; i < camerasPos.Count; i++)
        {
            // Wait for players go out
            while (haveFace)
            {
                yield return null;
            }
            // Wait for players come in
            while (!haveFace)
            {
                yield return null;
            }


            
            yield return MainCamMovesTo(camerasPos[i], 2);
            Debug.Log("moved to face" + i);
            
            
            GameObject faceFather = StartFace(i);
            faceFathers.Add(faceFather);
            faceFather.GetComponentInChildren<LightController>().TurnLightOn();
            yield return new WaitForSeconds(.5f);
            yield return GameObjectMovesTo(faceFather, faceFather.transform.position + new Vector3(0, 0, -2.2f), 3f);
            
            List<DroppableRig> faceScripts = new List<DroppableRig>(faceFather.GetComponentsInChildren<DroppableRig>());
            foreach (DroppableRig fs in faceScripts)
            {
                fs.StartSynchronizing();
                fs.StartRecord();
            }
            
            cameraLook.enableCameraLook(camerasPos[i].transform);

            DroppableRig faceScript = faceScripts[0];
            float startTime = Time.time;
            
            int initCount = faceScript.liveCount;
            while (initCount - faceScript.liveCount < 20 )
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    break;
                }
                yield return null;
            }
        
            durations.Add(Time.time - startTime);
            
            //breakWall(index);
            wall.GetComponent<DroppableWall>().DropGroup(i);
            
            //deleteFace();
            faceScript.DropAll();
            faceScript.StopSynchronizing();
            faceScript.StopRecord();
            cameraLook.disableCameraLook();
            yield return new WaitForSeconds(1);
            StartCoroutine(GameObjectMovesTo(faceFather.transform.GetChild(1).gameObject, faceFather.transform.position + new Vector3(0, 0, 3f), 2f));
            yield return new WaitForSeconds(3);

            faceFather.GetComponent<FaceFather>().spotLight.SetActive(false);
            Debug.Log("move back" + i);
            StartCoroutine(MainCamMovesTo(globalCameraPos, 2));
            yield return GameObjectMovesTo(faceFather.transform.GetChild(1).gameObject, faceFather.transform.position + new Vector3(0, 0, 3f), 2f);
            Destroy(faceFather.transform.GetChild(1).gameObject);
        }
        //Destroy(wall.gameObject);
    }

    private GameObject StartFace(int index)
    {
        // instantiate new face at facesPos[i]; enable moion;
        Vector3 offset = new Vector3(1.2f, 0, 1.7f);
        Vector3 facePos = new Vector3(_mainCamera.transform.position.x + 0.2f, _mainCamera.transform.position.y,
            wall.transform.position.z + 3.8f);
        GameObject face = Instantiate(facePrefab, facePos, _mainCamera.transform.rotation);
        face.transform.Rotate(0, 180, 0);
        return face;
    }

    private IEnumerator ReplayWall()
    {
        Debug.Log("replayWall");
        wall.GetComponent<DroppableWall>().RewindAll(1);
        for (int i = 0; i < faceFathers.Count; i++)
        {
            faceFathers[i].GetComponentInChildren<DroppableRig>().StartRewind(durations[i]);
        }
        yield return new WaitForSeconds(8);

        foreach (GameObject faceFather in faceFathers)
        {
            StartCoroutine(GameObjectMovesTo(faceFather, faceFather.transform.position + new Vector3(0, 0, 3f), 1f));
        }
        yield return new WaitForSeconds(1);
        foreach (GameObject faceFather in faceFathers)
        {   
            Destroy(faceFather);
        }
        faceFathers.Clear();
        yield return null;
    }

    private IEnumerator MainCamMovesTo(GameObject dest, float time)
    {
        AnimationCurve xCurve = AnimationCurve.EaseInOut(0, _mainCamera.transform.position.x, time, dest.transform.position.x);
        xCurve.postWrapMode = WrapMode.Once;

        AnimationCurve yCurve = AnimationCurve.EaseInOut(0, _mainCamera.transform.position.y, time, dest.transform.position.y);
        yCurve.postWrapMode = WrapMode.Once;
        
        AnimationCurve zCurve = AnimationCurve.EaseInOut(0, _mainCamera.transform.position.z, time, dest.transform.position.z);
        zCurve.postWrapMode = WrapMode.Once;

        float timeCount = 0;
        while (timeCount <= time)
        {
            timeCount += Time.deltaTime;
            _mainCamera.transform.position = new Vector3(xCurve.Evaluate(timeCount), yCurve.Evaluate(timeCount), zCurve.Evaluate(timeCount));
            yield return null;
        }
    }
    
    private IEnumerator GameObjectMovesTo(GameObject src, Vector3 dest, float time)
    {
        AnimationCurve xCurve = AnimationCurve.EaseInOut(0, src.transform.position.x, time, dest.x);
        xCurve.postWrapMode = WrapMode.Once;

        AnimationCurve yCurve = AnimationCurve.EaseInOut(0, src.transform.position.y, time, dest.y);
        yCurve.postWrapMode = WrapMode.Once;
        
        AnimationCurve zCurve = AnimationCurve.EaseInOut(0, src.transform.position.z, time, dest.z);
        zCurve.postWrapMode = WrapMode.Once;

        float timeCount = 0;
        while (timeCount <= time)
        {
            timeCount += Time.deltaTime;
            src.transform.position = new Vector3(xCurve.Evaluate(timeCount), yCurve.Evaluate(timeCount), zCurve.Evaluate(timeCount));
            yield return null;
        }
    }
}
