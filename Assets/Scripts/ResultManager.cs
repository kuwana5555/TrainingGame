using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    [Header("DisPlay Score")]
    [SerializeField] TMP_Text UI_score;

    // Start is called before the first frame update
    void Start()
    {
        UI_score.SetText(GameManager1.totalscore.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
