using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionPointMovement : MonoBehaviour
{
    [SerializeField] private float _speed;

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0, vertical) * _speed * Time.deltaTime;

        this.transform.Translate(direction);
    }
}
