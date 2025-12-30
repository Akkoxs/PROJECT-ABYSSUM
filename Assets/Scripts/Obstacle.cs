// using UnityEngine;

// public class Obstacle : MonoBehaviour
// {
//     Rigidbody2D rb;
//     private Camera mainC;
//     private float screenHalfWidth;
    
//     void Start()
//     {
//         // float randomSize = Random.Range(minSize, maxSize);
//         // transform.localScale = new Vector3(randomSize, randomSize, 1);
//         rb = GetComponent<Rigidbody2D>();
        
//         // Get camera reference and calculate screen bounds
//         mainC = Camera.main;
//         screenHalfWidth = mainC.orthographicSize * mainC.aspect;
        
//         // Lock vertical movement
//         rb.constraints = RigidbodyConstraints2D.FreezePositionY;
        
//         float randomSpeed = Random.Range(minSpeed, maxSpeed);
//         Vector2 right = Vector2.right;
//         rb.AddForce(right * 100); 
        
//         float randomTorque = Random.Range(-maxSpinSpeed, maxSpinSpeed);
//         rb.AddTorque(randomTorque);
//     }
    
//     void OnCollisionEnter2D(Collision2D collision)
//     {
//         // Reverse horizontal velocity on any collision
//         rb.linearVelocity = new Vector2(-rb.linearVelocity.x, 0);
        
//         // Move object to left side of screen
//         float leftEdge = mainC.transform.position.x - screenHalfWidth;
//         transform.position = new Vector3(leftEdge, transform.position.y, transform.position.z);
//     }
    
//     void Update()
//     {
            
//     }   
// }