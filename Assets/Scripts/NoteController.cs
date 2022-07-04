using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    Transform tableObj;
    Transform playNoteObj;

    public float X { get; set; } = 0;

    public float Width { get; set; } = 0;

    public ScoreNote Note { get; set; }
    public ColorSettings ColorSettings { get; set; }

    public static float ScaleFor16th = 6;


    void Start()
    {
        this.tableObj = this.gameObject.transform.Find("TablePrefab");
        this.playNoteObj = this.gameObject.transform.Find("PlayNotePrefab");

        if (this.Note.Type == "16th")
        {
            this.tableObj.transform.localScale = new Vector3(ScaleFor16th, ScaleFor16th, ScaleFor16th);
            this.playNoteObj.transform.localScale = new Vector3(ScaleFor16th, ScaleFor16th, ScaleFor16th);
        }
        else
        {
            this.transform.localPosition = new Vector3(this.X + (this.Width / 2),
                this.transform.localPosition.y,
                this.transform.localPosition.z);

            this.tableObj.transform.localScale = new Vector3(this.Width,
                this.tableObj.transform.localScale.y,
                this.tableObj.transform.localScale.z);

            this.playNoteObj.transform.localScale = new Vector3(this.Width,
                this.playNoteObj.transform.localScale.y,
                this.playNoteObj.transform.localScale.z);
        }

        this.SetColor();
    }

    void SetColor()
    {
        switch (this.Note.Pitch?.Step)
        {
            case "C":
                this.SetColor(this.ColorSettings.C);
                break;
            case "D":
                this.SetColor(this.ColorSettings.D);
                break;
            case "E":
                this.SetColor(this.ColorSettings.E);
                break;
            case "F":
                this.SetColor(this.ColorSettings.F);
                break;
            case "G":
                this.SetColor(this.ColorSettings.G);
                break;
            case "A":
                this.SetColor(this.ColorSettings.A);
                break;
            case "B":
                this.SetColor(this.ColorSettings.B);
                break;
            default:
                break;
        }
    }

    private void SetColor(NoteColor noteColor)
    {
        var renderer = this.playNoteObj.GetComponent<Renderer>();
        renderer.material.SetColor("_Color", noteColor.MainColor);
    }

    public void AddWidth(float width)
    {
        this.Width += width;
    }

    public void StartAnimation(float divisionOneX, float tempo, float speedXPerSec)
    {
        if (Note.Type == "16th")
        {
            StartCoroutine(this.AnimatedBarFor16th(divisionOneX, tempo, speedXPerSec));
        }
        else
        {
            StartCoroutine(this.AnimatedBar(divisionOneX, tempo, speedXPerSec));
        }
    }

    float GetThisObjTime(float durationOneX, float bpm)
    {
        var objDuration = this.tableObj.transform.localScale.x / durationOneX;
        // このオブジェクトがもつ時間（ /4 は16部音符換算にしてから、durationをかける）
        //   ※1 duration=16部音符の前提
        return 60 / bpm / 4 * objDuration;
    }

    IEnumerator AnimatedBar(float durationOneX, float bpm, float speedXPerSec)
    {
        var objTime = GetThisObjTime(durationOneX, bpm);
        var restTime = objTime;
        var originalWidth = this.playNoteObj.transform.localScale.x;
        var originalHeight = this.playNoteObj.transform.localScale.y;

        while (restTime > 0)
        {
            var width = restTime / objTime * originalWidth;

            var currentScale = this.playNoteObj.transform.localScale;
            this.playNoteObj.transform.localScale = new Vector3(width, originalHeight, currentScale.z);

            restTime -= UnityEngine.Time.deltaTime;
            yield return null;
        }
        Destroy(this.playNoteObj.gameObject);
    }

    IEnumerator AnimatedBarFor16th(float durationOneX, float bpm, float speedXPerSec)
    {
        var objTime = GetThisObjTime(durationOneX, bpm);
        var restTime = objTime;
        while (restTime > 0)
        {
            var size = restTime / objTime * ScaleFor16th;

            var currentScale = this.playNoteObj.transform.localScale;
            this.playNoteObj.transform.localScale = new Vector3(size, size, size);

            restTime -= UnityEngine.Time.deltaTime;
            yield return null;
        }
        Destroy(this.playNoteObj.gameObject);
    }
}
