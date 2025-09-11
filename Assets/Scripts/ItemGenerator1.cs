using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ItemGenerator1 : MonoBehaviour
{

    [Header("Generate Item Object")]
    [SerializeField] GameObject[] items;
    [Header("First Generate Time")]
    [SerializeField] float genItemDelay = 5f;
    [Header("Generate Pace")]
    [SerializeField] float genItemInterval = 5f;
    [Header("Screen White")]
    [SerializeField] float genItemMarginY = 5f;

    Vector2 genItemMin, genItemMax;
    string genItemMethodName = "ItemGenerate";
    // Start is called before the first frame update
    void Start()
    {
        genItemMin = Camera.main.ViewportToWorldPoint(Vector2.zero);
        genItemMax = Camera.main.ViewportToWorldPoint(Vector2.one);

        InvokeRepeating("ItemGenerate", genItemDelay, genItemInterval);
    }

    void ItemGenerate()
    {
        Instantiate(
            items[Random.Range(0, items.Length)],
            new Vector2(transform.position.x, Random.Range(genItemMin.y + genItemMarginY, genItemMax.y - genItemMarginY)),
            Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
