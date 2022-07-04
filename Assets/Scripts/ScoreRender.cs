using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreRender
{
    public ScoreRender(
        ColorSettings colorSettings,
        GameObject notePrefab,
        GameObject circleNotePrefab,
        float durationOneX
    )
    {
        this._colorSettings = colorSettings;
        this._notePrefab = notePrefab;
        this._circleNotePrefab = circleNotePrefab;
        this._durationOneX = durationOneX;
    }

    private static int OneNoteY = 2;
    private static int YOfC4 = OneNoteY * Pitch.NoteList.Count * 5;
    private float _durationOneX;

    private ColorSettings _colorSettings;
    private GameObject _notePrefab;
    private GameObject _circleNotePrefab;

    public IList<NoteView> Render(GameObject scoreBoardObj, string musicXMLText)
    {
        var score = MusicXMLParser.GetScorePartwise(musicXMLText);
        var notesSortedByScore = new List<NoteView>();

        int currentDivisions = 4;
        bool noNeedToDisplayForTie = false;
        float xCursor = 0;
        foreach (var part in score.ScoreParts)
        {
            foreach (var measure in part.MeasureList)
            {
                if (measure.Attribute?.Divisions != null)
                {
                    currentDivisions = measure.Attribute.Value.Divisions.Value;
                }

                foreach (IMeasureChild child in measure.Children)
                {
                    if (child is ScoreNote)
                    {
                        var note = (ScoreNote)child;

                        float y = 0;
                        // y
                        if (note.Pitch != null)
                        {
                            y = note.Pitch.Value.GetMidiNoteNumber() * OneNoteY - YOfC4;
                        }

                        // x
                        float willConsumedTimeUnit = note.Duration * this._durationOneX;

                        if (note.IsChord)
                        {
                            // 開始位置は、前の音と同じ位置
                            xCursor = notesSortedByScore[Math.Max(notesSortedByScore.Count - 1, 0)].X;
                        }

                        // instantiate
                        if (note.Pitch != null && !noNeedToDisplayForTie)
                        {
                            var noteObj = InstantiateNote(scoreBoardObj, xCursor, y, willConsumedTimeUnit, note);
                            var noteView = new NoteView() { GameObject = noteObj, X = xCursor, Pitch = note.Pitch.Value };
                            notesSortedByScore.Add(noteView);
                        }
                        if (note.Pitch != null && noNeedToDisplayForTie)
                        {
                            // 1つ前のNoteに長さを加える
                            var noteController = notesSortedByScore[notesSortedByScore.Count - 1].GameObject.GetComponent<NoteController>();
                            noteController.AddWidth(willConsumedTimeUnit);
                        }

                        xCursor += willConsumedTimeUnit;

                        // If both start and stop are present, noNeedToDisplayForTie should be true.
                        if (note.TieList != null && note.TieList.Exists(x => x.Type == "stop"))
                        {
                            noNeedToDisplayForTie = false;
                        }
                        if (note.TieList != null && note.TieList.Exists(x => x.Type == "start"))
                        {
                            noNeedToDisplayForTie = true;
                        }
                    }
                    else if (child is Backup)
                    {
                        // backup は、x のCursorを元に戻す
                        var backup = (Backup)child;
                        // どのくらい x を戻るか
                        float backupUnit = backup.Duration * this._durationOneX;

                        xCursor -= backupUnit;
                    }
                }
            }
        }

        return notesSortedByScore;
    }

    GameObject InstantiateNote(
        GameObject scoreBoardObj,
        float positionX,
        float positionY,
        float willConsumedTimeUnit,
        ScoreNote note)
    {
        GameObject noteObj = UnityEngine.Object.Instantiate<GameObject>(
            this.GetPrafabByNote(note),
            new Vector3(positionX, positionY, 0),
            Quaternion.identity,
            scoreBoardObj.transform);
        noteObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
        noteObj.transform.localPosition = new Vector3(positionX, positionY, 0);
        var noteController = noteObj.GetComponent<NoteController>();
        noteController.X = positionX;
        noteController.Width = willConsumedTimeUnit;
        noteController.Note = note;
        noteController.ColorSettings = this._colorSettings;

        noteObj.name = $"{note.Pitch?.Octave}{note.Pitch?.Step}";
        return noteObj;
    }

    GameObject GetPrafabByNote(ScoreNote note)
    {
        if (note.Type == "16th")
        {
            return this._circleNotePrefab;
        }

        return this._notePrefab;
    }
}
