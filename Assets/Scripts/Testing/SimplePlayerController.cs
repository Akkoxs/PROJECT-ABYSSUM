using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public SpriteRenderer sprite; 

    [SerializeField] Transform lightContainer;

    void Start(){
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float horizontal = 0f;

        if (Keyboard.current.aKey.isPressed){
            horizontal = -1f;
            sprite.flipX = true;
            FlipLight(lightContainer, true);
            
        }
            
        else if (Keyboard.current.dKey.isPressed){
            horizontal = 1f;
            sprite.flipX = false;
            FlipLight(lightContainer, false);

        }

        transform.Translate(Vector2.right * horizontal * moveSpeed * Time.deltaTime);
    }

    Transform FlipLight(Transform container, bool command){
        Vector3 scale = container.localScale;

        if (command)
            scale.x = -1;
        else
            scale.x = 1;

        container.localScale = scale;
        return container; 
    }

}