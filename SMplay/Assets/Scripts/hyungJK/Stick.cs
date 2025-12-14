using UnityEngine;

public class Stick : MonoBehaviour
{
    SpriteRenderer sr;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    public void Throwing(bool face)
    {
        sr.flipX = face;
    }

}
