using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] float speed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(transform.position);
        Debug.Log(gameObject.transform.position);
        Debug.Log(gameObject.name);
        //Debug.Log(GameObject);

    }

    // Update is called once per frame
    void Update()
    {
        //transform.position += new Vector3(inputManager.moveInput.x * speed * Time.deltaTime,
        //inputManager.moveInput.y * speed * Time.deltaTime,
        //0f);

        transform.Translate(inputManager.moveInput.x * speed * Time.deltaTime,
        inputManager.moveInput.y * speed * Time.deltaTime,
        0f);
    }
}
