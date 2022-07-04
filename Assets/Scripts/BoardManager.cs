using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class BoardManager : MonoBehaviour
{
    // duration 1（今は16分音符）あたりにおける x の値
    private const float DurationOneX = 4;

    private const float Bpm = 57;
    // 1秒で進んで欲しいxの値
    private const float SpeedXPerSec = (DurationOneX * 4) * Bpm / 60;
    private GameObject notePrefab;
    private GameObject circleNotePrefab;
    private GameObject scoreBoard;

    private PlayRecorder playRecorder;

    private VideoPlayer videoPlayer;
    private ColorSettings colorSettings;

    private SmfLite.MidiTrackSequencer midiTrackSequencer;

    void Start()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        this.colorSettings = ColorSettings.LoadColorSettingsByMaterials();
        this.notePrefab = Resources.Load<GameObject>("Prefabs/NotePrefab");
        this.circleNotePrefab = Resources.Load<GameObject>("Prefabs/CircleNotePrefab");
        this.scoreBoard = GameObject.Find("ScoreBoard");
        GameObject.Find("TestObj").SetActive(false);

        var file = Resources.Load<TextAsset>("BWV846P");
        var scoreRender = new ScoreRender(
            this.colorSettings,
            this.notePrefab,
            this.circleNotePrefab,
            DurationOneX
        );
        var notesSortedByScore = scoreRender.Render(this.scoreBoard, file.text);
        this.playRecorder = new PlayRecorder(notesSortedByScore);
        this.videoPlayer = GameObject.Find("PianoVideoPlayer").GetComponent<VideoPlayer>();

        LoadMidiFile();
    }

    void Update()
    {
        if (this.midiTrackSequencer != null
            && !this.midiTrackSequencer.Playing
            && this.videoPlayer
            && this.videoPlayer.time > 0
            )
        {
            // MIDIファイル再生位置の調整
            this.DispatchEvents(this.midiTrackSequencer.Start(0.2f));
        }
        MoveBoard();
        this.DispatchEvents(this.midiTrackSequencer.Advance(UnityEngine.Time.deltaTime));
    }

    private void LoadMidiFile()
    {
        var smfAsset = Resources.Load<TextAsset>("BWV846P_MIDI.mid");
        var song = SmfLite.MidiFileLoader.Load(smfAsset.bytes);
        // この midi file のテンポは倍速になってしまったので Bpm * 2 を指定
        this.midiTrackSequencer = new SmfLite.MidiTrackSequencer(song.tracks[0], song.division, Bpm * 2);
    }

    void MoveBoard()
    {
        var addX = SpeedXPerSec * UnityEngine.Time.deltaTime;
        var currentBoardPosition = this.scoreBoard.transform.position;
        this.scoreBoard.transform.position -= this.scoreBoard.transform.right * addX;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change != InputDeviceChange.Added)
        {
            return;
        }

        var midiDevice = device as Minis.MidiDevice;
        if (midiDevice == null) return;

        midiDevice.onWillNoteOn += OnWillNoteOn;
    }

    private void OnWillNoteOn(Minis.MidiNoteControl note, float velocity)
    {
        DispatchNoteOnEvent(note.noteNumber);
    }

    private void DispatchEvents(List<SmfLite.MidiEvent> events)
    {
        if (events == null)
        {
            return;
        }

        foreach (var e in events)
        {
            if ((e.status & 0xf0) == 0x90)
            {
                this.DispatchNoteOnEvent(e.data1);
            }
        }
    }

    private void DispatchNoteOnEvent(int noteNumber)
    {
        var pitch = Pitch.GetPitchByMidiNoteNumber(noteNumber);
        this.PlayNote(pitch);
    }

    void PlayNote(Pitch pitch)
    {
        this.playRecorder.Played(pitch, DurationOneX, Bpm, SpeedXPerSec);
    }
}
