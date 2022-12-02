using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.IO.Pipes;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;

public class FaceCaptureSDK : MonoBehaviour
{
    private const int blendShapeOffset = 12;
    public int blendShapeCount = 50;
    // public string inputPath = @"C:\Users\Gerwa\Desktop\params.csv";
    public TextAsset textAsset;
    private const float coolDown = 0.07f; // 15 fps
    private float timer = 0.0f;
    List<DroppableRig> listeningRigs = new List<DroppableRig>();
    private int currentFrame;
    private StreamReader csvReader;
    // public float referenceZ = 0.003f;
    // public float radiusX = 8.0f;
    // public float radiusY = 4.5f;
    // public float cameraWidth = 640.0f;
    // public float cameraHeight = 480.0f;
    // public bool constrainXY = false, constrainZ = true;
    public bool usePython = true;
    public string pythonFileName = "/home/gerw/anaconda3/envs/cvae/bin/python";
    public string pythonArgs = "main_process_file.py --mode camera";
    public string pythonWorkingDir = "/home/gerw/Documents/git-task/2DASL/test_codes";
    private Process pythonProcess;
    private bool inUpdate = false;
    public bool useMouse = false;
    public float mouseSpeed = 5.0f;
    public bool haveFace = false;
    private bool usingCSV = false;
    Vector3 prevEuler = new Vector3();
    float deltaEulerX = 0, deltaEulerY = 0;
    private void NotifyHaveFace(bool haveFace)
    {
        this.haveFace = haveFace;
    }

    private void Awake()
    {
        if (usePython)
        {
            pythonProcess = new Process();
            pythonProcess.StartInfo = new ProcessStartInfo
            {
                Arguments = pythonArgs,
                FileName = pythonFileName,
                UseShellExecute = false,
                WorkingDirectory = pythonWorkingDir,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            pythonProcess.Start();
        }
        else if (useMouse)
        {
            // do nothing
            byte[] byteArray = Encoding.ASCII.GetBytes(textAsset.text);
            MemoryStream stream = new MemoryStream(byteArray);
            csvReader = new StreamReader(stream);
        }
        else
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(textAsset.text);
            MemoryStream stream = new MemoryStream(byteArray);
            csvReader = new StreamReader(stream);
        }
    }
    // Start is called before the first frame update
    private void Start()
    {
        currentFrame = 0;
    }

    private void OnDestroy()
    {
        if (usePython)
        {
            if (pythonProcess != null)
            {
                pythonProcess.Kill();
            }
        }
        else if (useMouse)
        {
            // do nothing
        }
        else
        {
            if (csvReader != null)
            {
                csvReader.Close();
            }
        }
    }

    public void registerListener(DroppableRig rig)
    {
        listeningRigs.Add(rig);
    }

    public void unregisterListener(DroppableRig rig)
    {
        listeningRigs.Remove(rig);
    }

    // private float startTime;
    // Update is called once per frame

    private async void FixedUpdate()
    {
        if (inUpdate)
        {
            return;
        }
        inUpdate = true;

        if (Input.GetKeyDown(KeyCode.X))
        {
            useMouse = !useMouse;
        }
        if (usePython)
        {
            var line = await pythonProcess.StandardOutput.ReadLineAsync();
            /*
            if (currentFrame == 0)
            {
                startTime = Time.time;
            }
            else
            {
                Debug.Log(currentFrame / (Time.time - startTime));
            }
            */

            if (line == null)
            {
                Debug.Log("Didn't read from python");
                // do nothing
            }
            else if (line.StartsWith("Warning") || line.StartsWith("success"))
            {
                // empty frame
                NotifyHaveFace(false);
            }
            else
            {
                NotifyHaveFace(true);
                string[] stringValues = line.Split(',');
                float[] values = new float[blendShapeOffset + blendShapeCount];
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = float.Parse(stringValues[i]);
                }
                UpdateModelFromValues(values);
            }
        }
        else if (useMouse)
        {
            var dx = Input.GetAxis("Mouse X");
            var dy = Input.GetAxis("Mouse Y");
            var spacePressed = Input.GetKeyDown("space");
            if (spacePressed)
            {
                NotifyHaveFace(!haveFace);
            }
            if (dx != 0 || dy != 0)
            {
                if (!haveFace)
                {
                    Debug.Log("Press space to enable face");
                }
                else if (listeningRigs.Count > 0)
                {
                    var exampleRig = listeningRigs[0];
                    Quaternion rotation = Quaternion.Euler(exampleRig.transform.localEulerAngles.x - dy * mouseSpeed,
                     exampleRig.transform.localEulerAngles.y - dx * mouseSpeed,
                      exampleRig.transform.localEulerAngles.z);
                    Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
                    float[] values = new float[blendShapeOffset + blendShapeCount];
                    for (var i = 0; i < 3; i++)
                    {
                        for (var j = 0; j < 4; j++)
                        {
                            values[i * 4 + j] = m[i, j];
                        }
                    }
                    UpdateModelFromValues(values);
                }
            }
        }
        else
        {
            if (Input.GetKeyDown("space"))
            {
                NotifyHaveFace(!haveFace);
            }
            /*if (Input.GetKeyDown(KeyCode.Z))
            {
                usingCSV = !usingCSV;
            }*/
            //if (usingCSV)
            {
                timer += Time.deltaTime;
                if (timer >= coolDown)
                {
                    timer -= coolDown;
                    if (!csvReader.EndOfStream)
                    {
                        var line = await csvReader.ReadLineAsync();
                        if (currentFrame == 0)
                        {
                            line = await csvReader.ReadLineAsync(); // skip title
                        }
                        NotifyHaveFace(true);
                        var stringValues = line.Split(',');
                        float[] values = new float[blendShapeOffset + blendShapeCount];
                        for (var i = 0; i < values.Length; i++)
                        {
                            values[i] = float.Parse(stringValues[i + 1]);
                        }
                        UpdateModelFromValues(values);
                    }
                    else
                    {
                        NotifyHaveFace(false);
                    }
                }
            }
        }
        inUpdate = false;
    }

    private void UpdateModelFromValues(float[] values)
    {
        var mat = new Matrix4x4();
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                mat[i, j] = values[i * 4 + j];
            }
        }
        var localRotation = DroppableRig.ExtractRotationFromMatrix(ref mat);
        deltaEulerX = localRotation.eulerAngles.y - prevEuler.y;
        deltaEulerY = localRotation.eulerAngles.x - prevEuler.x;
        prevEuler = localRotation.eulerAngles;
        foreach (var rig in listeningRigs)
        {
            rig.NotifyNewInput(values);
        }
        currentFrame++;
    }

    public float GetMockedInputAxisX()
    {
        if (useMouse)
        {
            return Input.GetAxis("Mouse X");
        }
        else
        {
            return deltaEulerX / mouseSpeed;
        }
    }

    public float GetMockedInputAxisY()
    {
        if (useMouse)
        {
            return Input.GetAxis("Mouse Y");
        }
        else
        {
            return deltaEulerY / mouseSpeed;
        }
    }
}
