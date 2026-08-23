using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class Bowling : MonoBehaviour
{
    public Rigidbody rb;
    [SerializeField] private int forcePower;
    [SerializeField] private float resetZPosition = 20f;

    [Header("UI Settings")]
    public GameObject resetTextObject;

    private Vector3 ballStartPosition;
    private bool isShot = false;

    private List<Transform> pinTransforms = new List<Transform>();
    private List<Vector3> pinStartPositions = new List<Vector3>();
    private List<Quaternion> pinStartRotations = new List<Quaternion>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ballStartPosition = transform.position;

        if (resetTextObject != null)
        {
            resetTextObject.SetActive(false);
        }

        GameObject[] pins = GameObject.FindGameObjectsWithTag("Pin");
        foreach (GameObject pin in pins)
        {
            pinTransforms.Add(pin.transform);
            pinStartPositions.Add(pin.transform.position);
            pinStartRotations.Add(pin.transform.rotation);
        }
    }

    void Update()
    {
        if (!isShot && (Keyboard.current.spaceKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame))
        {
            ShootBall();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetGame();
        }

        if (!isShot)
        {
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                MoveRight();
            }
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                MoveLeft();
            }
        }
    }

    private void ShootBall()
    {
        isShot = true;
        rb.AddForce(Vector3.forward * forcePower, ForceMode.Impulse);

        Invoke("ShowResetText", 1.0f);
    }

    private void ShowResetText()
    {
        if (resetTextObject != null)
        {
            resetTextObject.SetActive(true);
        }
    }

    private void MoveRight()
    {
        transform.position += new Vector3(0.5f, 0f, 0f) * Time.deltaTime;
    }

    private void MoveLeft()
    {
        transform.position += new Vector3(-0.5f, 0f, 0f) * Time.deltaTime;
    }

    private void ResetGame()
    {
        isShot = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = ballStartPosition;

        for (int i = 0; i < pinTransforms.Count; i++)
        {
            Rigidbody pinRb = pinTransforms[i].GetComponent<Rigidbody>();
            if (pinRb != null)
            {
                pinRb.linearVelocity = Vector3.zero;
                pinRb.angularVelocity = Vector3.zero;
            }
            pinTransforms[i].position = pinStartPositions[i];
            pinTransforms[i].rotation = pinStartRotations[i];
        }

        if (resetTextObject != null)
        {
            resetTextObject.SetActive(false);
        }
    }
}