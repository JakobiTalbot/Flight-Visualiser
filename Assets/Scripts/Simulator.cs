using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class Simulator : MonoBehaviour
{    
    [SerializeField]
    private string fileName;
    [SerializeField]
    private int numbersPerData = 7;

    private Vector3 velocity = Vector3.zero;
    private Vector3 currentAccel = Vector3.zero;
    private Vector3 currentGyro = Vector3.zero;
    private Vector3 gyroAccel = Vector3.zero;
    private float lastTime;

    private TestData[] testData;

    public struct TestData
    {
        public float time;
        public Vector3 gyro;
        public Vector3 accel;
    }

    private void Start()
    {
        LoadData();
        StartCoroutine(Simulate());
    }

    private void Update()
    {
        // rotation
        currentGyro += gyroAccel * Time.deltaTime;
        transform.Rotate(currentGyro * Time.deltaTime);

        // movement
        velocity += currentAccel * Time.deltaTime;
        //transform.Translate(velocity * Time.deltaTime, Space.Self);
        Debug.DrawLine(transform.position, transform.position + transform.InverseTransformDirection(velocity), Color.red);
    }

    private void LoadData()
    {
        string raw = Resources.Load(fileName).ToString();
        // replace line breaks with commas
        raw = Regex.Replace(raw, "\r\n", ",");
        string[] data = raw.Split(',');
        testData = new TestData[data.Length / numbersPerData];

        // try to negate some drag
        Vector3 accelOffset;
        accelOffset.x = float.Parse(data[4]);
        accelOffset.z = float.Parse(data[5]);
        accelOffset.y = float.Parse(data[6]);

        for (int i = 0; i < testData.Length; ++i)
        {
            int index = i * numbersPerData;

            // parse read values into data, z and y are flipped
            testData[i].time = float.Parse(data[index]);
            testData[i].gyro.x = float.Parse(data[index + 1]);
            testData[i].gyro.z = float.Parse(data[index + 2]);
            testData[i].gyro.y = float.Parse(data[index + 3]);
            testData[i].accel.x = float.Parse(data[index + 4]) - accelOffset.x;
            testData[i].accel.z = float.Parse(data[index + 5]) - accelOffset.z;
            testData[i].accel.y = -float.Parse(data[index + 6]) + accelOffset.y;
        }
        // get length in seconds of total simulation
        float simTime = (testData[testData.Length - 1].time - testData[0].time) / 1000f;
        Debug.Log(simTime);
    }

    private IEnumerator Simulate()
    {
        float timeDelta;

        for (int i = 0; i < testData.Length - 1; ++i)
        {
            // load values to simulate at current time
            currentAccel = testData[i].accel;
            gyroAccel = testData[i].gyro;

            // wait until next data entry to simulate
            timeDelta = (testData[i + 1].time - lastTime) / 1000f;
            yield return new WaitForSeconds(timeDelta);

            lastTime = testData[i].time;
        }

        // stop simulation
        currentAccel = Vector3.zero;
        currentGyro = Vector3.zero;
        gyroAccel = Vector3.zero;
        velocity = Vector3.zero;
        Debug.Log("Simulation Complete.");
    }
}