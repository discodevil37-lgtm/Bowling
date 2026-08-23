using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class Bowling : MonoBehaviour
{
    public Rigidbody rb;
    [SerializeField] private float maxForcePower = 50f;  // พลังโยนสูงสุดที่เป็นไปได้
    [SerializeField] private float chargeSpeed = 30f;    // ความเร็วในการสะสมพลังงานตอนกดค้าง
    [SerializeField] private float resetZPosition = 20f;

    [Header("UI Settings")]
    public GameObject resetTextObject;
    public TextMeshProUGUI scoreText;

    private Vector3 ballStartPosition;
    private bool isShot = false;

    // ระบบสะสมพลังภายใน (ไม่มี UI มาเกี่ยว)
    private float currentPower = 0f;
    private bool isCharging = false;
    private bool isIncreasing = true;

    // ระบบนับคะแนนและรีเซ็ตพิน
    private int currentScore = 0;
    private List<Transform> pinTransforms = new List<Transform>();
    private List<Vector3> pinStartPositions = new List<Vector3>();
    private List<Quaternion> pinStartRotations = new List<Quaternion>();
    private List<bool> pinKnockedStatus = new List<bool>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ballStartPosition = transform.position;

        if (resetTextObject != null) resetTextObject.SetActive(false);

        // ดึงพินทั้งหมดในด่านมารวมในระบบเพื่อเตรียมเช็กล้ม
        GameObject[] pins = GameObject.FindGameObjectsWithTag("Pin");
        foreach (GameObject pin in pins)
        {
            pinTransforms.Add(pin.transform);
            pinStartPositions.Add(pin.transform.position);
            pinStartRotations.Add(pin.transform.rotation);
            pinKnockedStatus.Add(false);
        }

        UpdateScoreUI();
    }

    void Update()
    {
        if (isShot)
        {
            CheckKnockedPins();
        }

        // === ระบบกดปุ่มค้างเพื่อสะสมพลัง (ยิ่งนานยิ่งแรง) ===
        if (!isShot)
        {
            // ตรวจสอบเฟรมต่อเฟรมว่าปุ่ม Spacebar กำลังโดนกดค้างอยู่จริงหรือไม่
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
            {
                isCharging = true;
                CalculatePower(); // แอบคำนวณพลังงานสะสมวิ่งขึ้นลงในระบบเงียบๆ
            }
            else
            {
                // เมื่อผู้เล่นถอนนิ้วออกจากปุ่ม Spacebar ให้สั่งยิงลูกบอลออกไปทันทีด้วยแรงที่สะสมไว้
                if (isCharging)
                {
                    ShootBall();
                }
            }
        }

        // === ระบบรีเซ็ตเกม (กดปุ่ม R) ===
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetGame();
        }

        // === ระบบเลื่อนตำแหน่งบอลซ้าย-ขวาก่อนโยน ===
        if (!isShot && !isCharging)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) MoveRight();
                if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) MoveLeft();
            }
        }
    }

    private void CalculatePower()
    {
        // ลูปค่าตัวเลขพลังงานให้วิ่งสะสมขึ้น-ลงในโค้ดตามเวลาจริง
        if (isIncreasing)
        {
            currentPower += chargeSpeed * Time.deltaTime;
            if (currentPower >= maxForcePower)
            {
                currentPower = maxForcePower;
                isIncreasing = false;
            }
        }
        else
        {
            currentPower -= chargeSpeed * Time.deltaTime;
            if (currentPower <= 0f)
            {
                currentPower = 0f;
                isIncreasing = true;
            }
        }
    }

    private void ShootBall()
    {
        isCharging = false;
        isShot = true;

        // ผลักบอลพุ่งไปข้างหน้า (แกน Z) ด้วยพละกำลัง currentPower ที่ผู้เล่นกดกะจังหวะค้างเอาไว้
        rb.AddForce(Vector3.forward * currentPower, ForceMode.Impulse);

        Invoke("ShowResetText", 1.0f);
    }

    private void CheckKnockedPins()
    {
        for (int i = 0; i < pinTransforms.Count; i++)
        {
            if (pinKnockedStatus[i]) continue;

            // ตรวจสอบมุมองศาพิน ถ้ายอดพินเอียงเกิน 30 องศาถือว่าล้มและได้แต้ม
            if (Vector3.Angle(pinTransforms[i].up, Vector3.up) > 30f)
            {
                pinKnockedStatus[i] = true;
                currentScore++;
                UpdateScoreUI();
            }
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    private void ShowResetText()
    {
        if (resetTextObject != null) resetTextObject.SetActive(true);
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
        isCharging = false;
        currentPower = 0f;
        isIncreasing = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = ballStartPosition;

        currentScore = 0;
        UpdateScoreUI();

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
            pinKnockedStatus[i] = false;
        }

        if (resetTextObject != null) resetTextObject.SetActive(false);
    }
}