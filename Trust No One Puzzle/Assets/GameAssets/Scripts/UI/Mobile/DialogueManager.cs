using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    
    public GameObject dialogueBox, textBlock;
    
    [SerializeField]
    List<Message> messageList = new List<Message>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && GameObject.Find("MobileManager").activeInHierarchy)
        {
            SendChatMessage("Space was Pressed!!");
        }
    }

    public void SendChatMessage(string text)
    {
        Message message = new Message();
        message.text = text;

        GameObject newTextBlock = Instantiate(textBlock, dialogueBox.transform);
        message.textBlock = newTextBlock.GetComponent<TextMeshProUGUI>();
        message.textBlock.text = message.text;
        
        messageList.Add(message);
    }
}

[System.Serializable]
public class Message
{
    public string text;
    public TextMeshProUGUI textBlock;
}
