using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator1 : MonoBehaviour
{
    [Header("Enemy Generate")]
    [SerializeField] GameObject[] enemies;

    [Header("Enemy First Generate")]
    [SerializeField] float genDelay = 2f;

    [Header("Enemy Second Generate")]
    [SerializeField] float genInterval = 1.5f;

    [Header("White")]
    [SerializeField] float genMarginY = 1f;

    Vector2 genMin, genMax;
    // Start is called before the first frame update
    void Start()
    {
        genMin = Camera.main.ViewportToWorldPoint(Vector2.zero);
        genMax = Camera.main.ViewportToWorldPoint(Vector2.one);

        InvokeRepeating("EnemyGenerate", genDelay, genInterval);

    }

    void EnemyGenerate()
    {
        Instantiate(
            enemies[Random.Range(0, enemies.Length)],
            new Vector2(transform.position.x, Random.Range(genMin.y + genMarginY, genMax.y - genMarginY)),
            Quaternion.identity);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
