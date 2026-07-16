namespace Geidai.Rec
{
    /// <summary>
    /// 3秒固定録音の経過計測（nfr-design §2 / NFR-03）。
    /// MonoBehaviour 非依存の POCO（Tick に deltaTime を与える）でテスト容易。
    /// </summary>
    public class RecordingClock
    {
        public const float DurationSeconds = 3f;

        public float Elapsed { get; private set; }
        public bool IsRunning { get; private set; }

        /// <summary>残り秒（0〜Duration）。表示用。</summary>
        public float RemainingSeconds
        {
            get
            {
                float remaining = DurationSeconds - Elapsed;
                return remaining < 0f ? 0f : remaining;
            }
        }

        /// <summary>3秒到達で true。</summary>
        public bool IsDone => Elapsed >= DurationSeconds;

        public void Start()
        {
            Elapsed = 0f;
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Reset()
        {
            Elapsed = 0f;
            IsRunning = false;
        }

        /// <summary>経過を加算し、到達したら true を返す（到達後は Elapsed を Duration に丸める）。</summary>
        public bool Tick(float deltaTime)
        {
            if (!IsRunning) return IsDone;

            Elapsed += deltaTime;
            if (Elapsed >= DurationSeconds)
            {
                Elapsed = DurationSeconds;
                IsRunning = false;
                return true;
            }
            return false;
        }
    }
}
