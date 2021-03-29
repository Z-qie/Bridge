using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstBungee : MonoBehaviour
{
    public GameObject talkUI;
    public GameObject player;

    public float maxHeightTrigger;
    public float textSpeed;

    public Text textLabel;

    public TextAsset textfile;

    private int index;

    private float lastHeight;
    private float currentHeight;
    private float jumpHeight;
    private bool cancelTyping;
    private bool textFinished;
    private bool isTriggered;
    private bool isFirstLine = true;


    readonly List<string> textList = new List<string>();

    void Awake()
    {
        GetTextFromFile(textfile);
        index = 0;
        textFinished = true;
        currentHeight = Mathf.Abs(player.GetComponent<Rigidbody2D>().position.y);
    }

     
    void Update()
    {
        if (talkUI.activeSelf)
        {
            if(isFirstLine == true)
            {
                isFirstLine = false;
                StartCoroutine(SetTextUI());
            }

            if (Input.GetKeyDown(KeyCode.E) && index == textList.Count)
            {
                talkUI.SetActive(false);
                gameObject.SetActive(false);
                player.GetComponent<PlayerController>().enabled = true;
                index = 0;
                return;
            }

            if (Input.GetKeyDown(KeyCode.E))
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
        lastHeight = Mathf.Abs(currentHeight);
        currentHeight = Mathf.Abs(player.GetComponent<Rigidbody2D>().position.y);

        if (player.GetComponent<Rigidbody2D>().velocity.y <= 0f && !player.GetComponent<PlayerController>().isGrounded)
        {
            jumpHeight += Mathf.Abs(lastHeight - currentHeight);
            if (jumpHeight > maxHeightTrigger)
                isTriggered = true;
            
        }
        else
        {
            jumpHeight = 0;
        }

        if (isTriggered && Mathf.Abs(player.GetComponent<Rigidbody2D>().velocity.y) < 0.1f)
        {
            talkUI.SetActive(true);
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            player.GetComponent<PlayerController>().enabled = false;
        }

        if (talkUI.activeSelf)
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
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
}
