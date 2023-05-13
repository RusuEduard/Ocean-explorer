using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CsvWriter
{
    public string FileName { get; set; }

    public CsvWriter(string filename)
    {
        this.FileName = filename;
    }

    public void setFileName(string filename)
    {
        this.FileName = filename;
    }
    public void WriteToFile(List<string> data, Boolean append)
    {
        TextWriter tw = new StreamWriter(FileName, append);

        for (int i = 0; i < data.Count; i++)
        {
            tw.Write(data[i]);
            if (i != data.Count - 1)
            {
                tw.Write(',');
            }
        }
        tw.WriteLine();
        tw.Close();
    }
}
