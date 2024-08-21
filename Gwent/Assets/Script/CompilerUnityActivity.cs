using CardCompiler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompilerUnityActivity : MonoBehaviour
{
    public TMP_InputField Console, Output;

    public void Build()
    {
        SintaxBack process = new();
        process.CleanSintax();
        process.VerifcateSintax(Console.text);
        Output.text = "";
        if (process.error.Count <= 0)
        {
            foreach (string s in process.TextImpress)
            {
                Output.text += s + "\n";
            }
            Output.text += "Build Succed";
           
        }

        else
        {
            foreach (ErrorBack lines in process.error)
            {
                Output.text += "Line: " + lines.Line + ": *" + lines.typeError + "*" + "\n";
            }
        }
        
    }
}
