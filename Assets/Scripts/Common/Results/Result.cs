namespace Geidai.Common.Results
{
    /// <summary>
    /// 成功/失敗＋理由を伝搬する結果型（Q1=A / NFR-07）。
    /// 致命的でない失敗はクラッシュさせず、呼び出し側が分岐して UI 表示する。
    /// </summary>
    public struct Result
    {
        public bool IsSuccess { get; private set; }
        public ResultCode Code { get; private set; }
        public string Message { get; private set; }

        private Result(bool isSuccess, ResultCode code, string message)
        {
            IsSuccess = isSuccess;
            Code = code;
            Message = message;
        }

        public static Result Ok()
        {
            return new Result(true, ResultCode.Ok, string.Empty);
        }

        public static Result Fail(ResultCode code, string message)
        {
            return new Result(false, code, message ?? string.Empty);
        }
    }

    /// <summary>値を伴う結果型。</summary>
    public struct Result<T>
    {
        public bool IsSuccess { get; private set; }
        public ResultCode Code { get; private set; }
        public string Message { get; private set; }
        public T Value { get; private set; }

        private Result(bool isSuccess, ResultCode code, string message, T value)
        {
            IsSuccess = isSuccess;
            Code = code;
            Message = message;
            Value = value;
        }

        public static Result<T> Ok(T value)
        {
            return new Result<T>(true, ResultCode.Ok, string.Empty, value);
        }

        public static Result<T> Fail(ResultCode code, string message)
        {
            return new Result<T>(false, code, message ?? string.Empty, default(T));
        }
    }
}
