using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntoTheScene : MonoBehaviour
{
    public GameObject talkUI;
    public GameObject player;

    public float radiusCheck;
    public float textSpeed;

    public LayerMask whatIsPlayer;
    public Text textLabel;

    public TextAsset textfile;

    private int index;
    private bool cancelTyping;
    private bool textFinished;

    readonly List<string> textList = new List<string>();

    void Awake()
    {
        GetTextFromFile(textfile);
        index = 0;
        textFinished = true;
    }

    void Update()
    {
        if (talkUI.activeSelf)
        {
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
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
