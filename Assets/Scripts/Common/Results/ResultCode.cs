namespace Geidai.Common.Results
{
    /// <summary>
    /// 失敗理由コード（nfr-design-patterns §1）。UI へ生の例外を漏らさず、
    /// このコードで分岐して平易な文言を提示する（BR-16/19）。
    /// </summary>
    public enum ResultCode
    {
        Ok = 0,
        NotFound = 1,
        Corrupted = 2,
        IOError = 3,
        ValidationError = 4,
        Unknown = 5,
        NotImplemented = 6
    }
}
