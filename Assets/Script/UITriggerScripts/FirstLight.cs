using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;


public class FirstLight : MonoBehaviour
{
    public float zoomOutSize;
    public float zoomOutSpeed;
    public float zoomOutX;
    public float zoomInSpeed;
    public float radiusCheck;
    public float textSpeed;
    public CinemachineVirtualCamera cinemachineVirtualCamera;


    public GameObject talkUI;
    public GameObject player;

    public Text textLabel;
    public TextAsset textfile;

    private int index;

    private bool cancelTyping;
    private bool textFinished;
    private bool isZoomingOut;
    private bool isZoomingIn;

    private float originalOrthographicSize;
    private float originalCameraPostionX;

    readonly List<string> textList = new List<string>();

    void Awake()
    {
        GetTextFromFile(textfile);
        index = 0;
        textFinished = true;

    }

    private void Start()
    {
        originalOrthographicSize = cinemachineVirtualCamera.m_Lens.OrthographicSize;
        originalCameraPostionX = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX;
    }

    void Update()
    {
        if (isZoomingOut && cinemachineVirtualCamera.m_Lens.OrthographicSize < zoomOutSize)
            cinemachineVirtualCamera.m_Lens.OrthographicSize += zoomOutSize * Time.deltaTime * zoomOutSpeed;

        if (isZoomingOut && cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX > zoomOutX)
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX -= Time.deltaTime * zoomOutSpeed * 2;


        if (talkUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E) && index == textList.Count)
            {
                isZoomingOut = false;
                isZoomingIn = true;
            }

            if (isZoomingIn)
                StartCoroutine(ZoomIn());

            if (Input.GetKeyDown(KeyCode.E) && !isZoomingIn)
            {
                if (textFinished && !cancelTyping)
                {
                    StartCoroutine(SetTextUI());
                }
                else if (!textFinished && !cancelTyping)
                {
                    cancelTyping = true;
                }
            }

        }
    }


    private void FixedUpdate()
    {
        if (talkUI.activeSelf)
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }

    IEnumerator ZoomIn()
    {
        if(cinemachineVirtualCamera.m_Lens.OrthographicSize > originalOrthographicSize
            ||
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX < originalCameraPostionX)
        {
            if (cinemachineVirtualCamera.m_Lens.OrthographicSize > originalOrthographicSize)
                cinemachineVirtualCamera.m_Lens.OrthographicSize -= zoomOutSize * Time.deltaTime * zoomInSpeed;

            if (cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX < originalCameraPostionX)
                cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX += Time.deltaTime * zoomInSpeed;
        }
        else
        {
            talkUI.SetActive(false);
            player.GetComponent<PlayerController>().enabled = true;
            index = 0;
            gameObject.SetActive(false);
        }
        yield return null;
    }

    void GetTextFromFile(TextAsset file)
    {
        textList.Clear();
        index = 0;
        var lineData = file.text.Split('\n');
        foreach (var line in lineData)
        {
            textList.Add(line);
        }
    }

    IEnumerator SetTextUI()
    {
        textFinished = false;
        textLabel.text = "";

        int letter = 0;
        while (!cancelTyping && letter < textList[index].Length - 1)
        {
            textLabel.text += textList[index][letter];
            letter++;
            yield return new WaitForSeconds(textSpeed);
        }
        textLabel.text = textList[index];
        cancelTyping = false;
        textFinished = true;
        index++;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            isZoomingOut = true;
            talkUI.SetActive(true);
            StartCoroutine(SetTextUI());
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            player.GetComponent<PlayerController>().enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusCheck);
    }
}
