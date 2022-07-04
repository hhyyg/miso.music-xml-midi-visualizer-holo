using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public interface IPlayRecorder
{
    void Played(Pitch playedPitch, float durationOneX, float bpm, float speedXPerSec);
}

public class PlayRecorder : IPlayRecorder
{
    public PlayRecorder(IEnumerable<NoteView> notes)
    {
        this._notesSorted = notes.OrderBy(note => note.X).ToList();
    }
    private List<NoteView> _notesSorted;

    private List<int> _playedPointers = new List<int>();

    private const int _searchBuffer = 30;

    public void Played(Pitch playedPitch, float durationOneX, float bpm, float speedXPerSec)
    {
        var lastPointer = this._playedPointers.LastOrDefault();

        for (int i = Math.Max(lastPointer - _searchBuffer, 0);
            i < Math.Min(this._notesSorted.Count(), lastPointer + _searchBuffer);
            i++)
        {
            if (
                !this._playedPointers.Contains(i)
                && this._notesSorted[i].Pitch.GetMidiNoteNumber() == playedPitch.GetMidiNoteNumber())
            {
                this._notesSorted[i].GameObject.GetComponent<NoteController>().StartAnimation(
                    durationOneX,
                    bpm,
                    speedXPerSec
                );
                this._playedPointers.Add(i);
                break;
            }
        }
    }
}