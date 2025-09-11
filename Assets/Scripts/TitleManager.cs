using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    public GameObject startButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            ChangeScene cs = startButton.GetComponent<ChangeScene>();
            if(cs != null)
            {
                cs.Load();
            }
        }
    }
}
