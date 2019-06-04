using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timePerformanceTest : MonoBehaviour
{
    public int testerNumber;

    List<TimeTester> list = new List<TimeTester>();
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < testerNumber; i++)
        {
            list.Add(new TimeTester());
        }
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < testerNumber; i++)
        {
            list[i].UpdateTime(deltaTime);
        }
    }
}

public class TimeTester
{
    float i;

    public TimeTester()
    {
        i = 0;
    }

    public void UpdateTime(float deltaTime)
    {
        i += deltaTime;
    }
}
