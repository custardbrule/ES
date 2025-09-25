using System.Text.Json;

namespace Utilities
{
    public static class ObjectHelper
    {
        public static string Serialize(this Guid id, string format = "N") => id.ToString(format);
        public static string Serialize(this object obj) => JsonSerializer.Serialize(obj);
    }
}
