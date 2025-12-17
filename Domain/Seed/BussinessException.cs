namespace Seed
{
    public class BussinessException : Exception
    {
        public string Code { get; set; }
        public int HttpCode { get; set; }
        public BussinessException(string code, int httpCode, string message) : base(message)
        {
            Code = code;
            HttpCode = httpCode;
        }
    }
}
