using System;

public class VoiceObject {
    private string text;
    private bool roboticVoice;

    public VoiceObject(string text, bool roboticVoice) {
        this.text = text;
        this.roboticVoice = roboticVoice;
    }

    public string GetText() {
        return this.text;
    }
    public bool GetRoboticVoice() {
        return this.roboticVoice;
    }
}