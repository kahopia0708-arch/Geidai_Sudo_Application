namespace Geidai.Rec
{
    /// <summary>
    /// マイク権限の状態（domain-entities §2.3 / SECURITY-15）。
    /// フェイルセーフ判定に用いる。
    /// </summary>
    public enum MicPermissionStatus
    {
        Unknown,   // 未確認（初期）
        Granted,   // 許可済み（録音可）
        Denied,    // 拒否（録音不可・案内）
        NoDevice   // マイクデバイス無し（録音不可・案内）
    }
}
