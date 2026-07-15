using System;
using Geidai.Common.Results;

namespace Geidai.Common.Utils
{
    /// <summary>
    /// 入力検証を集約するユーティリティ（BR-01〜03 / SECURITY-05）。
    /// 各画面はこれを呼び、失敗は Result(ValidationError) で受ける。
    /// </summary>
    public static class ValidationUtil
    {
        public const int MinBirthYear = 1900;
        public const int MinNicknameLength = 1;
        public const int MaxNicknameLength = 8;

        /// <summary>生年（1900〜現在年）。</summary>
        public static Result ValidateBirthYear(int birthYear)
        {
            int currentYear = DateTime.Now.Year;
            if (birthYear < MinBirthYear || birthYear > currentYear)
            {
                return Result.Fail(ResultCode.ValidationError,
                    $"生年は {MinBirthYear}〜{currentYear} の範囲で入力してください。");
            }
            return Result.Ok();
        }

        /// <summary>ニックネーム（前後空白を除き 1〜8 文字）。</summary>
        public static Result ValidateNickname(string nickname)
        {
            if (nickname == null)
            {
                return Result.Fail(ResultCode.ValidationError, "ニックネームを入力してください。");
            }

            string trimmed = nickname.Trim();
            if (trimmed.Length < MinNicknameLength || trimmed.Length > MaxNicknameLength)
            {
                return Result.Fail(ResultCode.ValidationError,
                    $"ニックネームは {MinNicknameLength}〜{MaxNicknameLength} 文字で入力してください。");
            }
            return Result.Ok();
        }
    }
}
