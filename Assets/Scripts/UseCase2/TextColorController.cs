using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextColorController : MonoBehaviour
{
    public TMP_Text text1;
    public TMP_Text text2;

    public Color color1 = Color.red;
    public Color color2 = Color.green;

    private int count = 0;

    public void ActivateText1()
    {
        if (text1 != null)
            text1.color = color1;
    }

    public void ActivateText2()
    {
        if (text2 != null)
            text2.color = color2;
    }

    public void Counter()
    {
        ++count;
        if(count == 1)
        {
            text2.color = color1;    
        }    
        if(count == 2)
        {
            text2.color = color2;    
        }
    }
}
