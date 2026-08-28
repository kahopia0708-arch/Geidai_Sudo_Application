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
        public const int MinBirthYear = 1925;
        public const int MinAge = 3;
        public const int MinNicknameLength = 1;
        public const int MaxNicknameLength = 8;

        /// <summary>生年下限（1925）から算出する最大年齢。</summary>
        public static int MaxAge => DateTime.Now.Year - MinBirthYear;

        /// <summary>生年（1925〜現在年）。</summary>
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

        /// <summary>年齢（3〜MaxAge さい）。</summary>
        public static Result ValidateAge(int age)
        {
            if (age < MinAge || age > MaxAge)
            {
                return Result.Fail(ResultCode.ValidationError,
                    $"なんさいか {MinAge}〜{MaxAge} さいの なかから えらんでね。");
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
