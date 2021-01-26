using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{
    public Text textBox;
    public Text answerBox;
    public Text findText;
    string[] textSet = { "Mama", "Yamyam", "FF", "Nishin", "Waiwai", "Kanor" };
    // Start is called before the first frame update
    void Start()
    {
        Text textBoxAdd;
        textBoxAdd = textBox.GetComponent<Text>();
        textBoxAdd.text = textSet[0];

        for (int i = 1; i < textSet.Length; i++)
        {
            textBoxAdd.text += "\n"+textSet[i];
        }
    }

    public void AnswerReturn()
    {
        string inputText = findText.GetComponent<Text>().text;
        foreach (string t in textSet)
        {
            if (inputText == t)
            {
                answerBox.GetComponent<Text>().text = "[ <b>" + inputText + "</b> ] is found";
                return;
            }
        }
        answerBox.GetComponent<Text>().text = "[ <b>" + inputText + "</b> ] is not found";
    }
}
