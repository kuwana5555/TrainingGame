using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGController : MonoBehaviour
{
    [Header("Scroll Speed")]
    [SerializeField] float scrollSpeed = 0.1f;
    [Header("x move")]
    [SerializeField] int moveX = 1;
    [Header("y move")]
    [SerializeField] int moveY = 1;
    [Header("Player BG")]
    [SerializeField] bool playerFollow = false;
    [Header("Player Move")]
    [SerializeField] float playerMove = 0.01f;
    [Header("Follow Object")]
    [SerializeField] GameObject player;

    float scrollMax = 1f;
    Vector2 offset;
    Renderer material;

    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<Renderer>();

        if (playerFollow && player == null)
        {
            player = GameObject.FindWithTag("Player").GetComponent<GameObject>();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerFollow)
        {
            playerMove = 0;
        }

        if (playerFollow && player == null)
        {
            return;
        }

        float scrollX = Mathf.Repeat(Time.time * scrollSpeed * moveX, scrollMax),
              scrollY = Mathf.Repeat(Time.time * scrollSpeed * moveY, scrollMax);

        float movePointX = player.transform.position.x,
              movePointY = player.transform.position.y;

        offset = new Vector2(scrollX + movePointX * playerMove,
                             scrollY + movePointY * playerMove);

        material.sharedMaterial.SetTextureOffset("_MainTex", offset);

    }
}
