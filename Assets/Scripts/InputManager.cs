using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Vector2 moveInput;
    [SerializeField] private float horizontal;
    [SerializeField] private float vertical;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        moveInput = new Vector2(horizontal, vertical);
    }
}
