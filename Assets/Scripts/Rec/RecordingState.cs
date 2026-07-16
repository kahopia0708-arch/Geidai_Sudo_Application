namespace Geidai.Rec
{
    /// <summary>
    /// Rec 画面の録音セッション状態（domain-entities §2.1）。
    /// UI の活性/表示と操作可否を規定する。
    /// </summary>
    public enum RecordingState
    {
        Idle,       // 初期／録音前
        NoMic,      // マイク不在・権限拒否（録音不可）
        Recording,  // 録音中（3秒カウントダウン）
        Recorded,   // 録音済み・プレビュー可
        Playing,    // プレビュー再生中
        Saving,     // 保存処理中
        Saved       // 保存完了
    }
}
