using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerLetter : MonoBehaviour
{
    public float textSpeed;

    public GameObject talkUI;
    public GameObject player;

    public Text textLabel;
    public TextAsset textfile;

    private bool cancelTyping;
    private bool textFinished;


    private string letter;

    void Awake()
    {
        GetTextFromFile(textfile);
        textFinished = true;
    }

    void Update()
    {

        if (talkUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (textFinished && !cancelTyping)
                {
                    talkUI.SetActive(false);
                    gameObject.SetActive(false);
                    player.GetComponent<PlayerController>().enabled = true;
                    return;
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
        letter = file.text;
    }

    IEnumerator SetTextUI()
    {
        textFinished = false;
        textLabel.text = "";

        int letter_count = 0;
        while (!cancelTyping && letter_count < letter.Length - 1)
        {
            textLabel.text += letter[letter_count];
            letter_count++;
            yield return new WaitForSeconds(textSpeed);
        }
        textLabel.text = letter;
        cancelTyping = false;
        textFinished = true;
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

}
