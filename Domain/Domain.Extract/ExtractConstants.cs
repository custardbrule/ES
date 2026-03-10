namespace Domain.Extract;

public static class ExtractConstants
{
    public const string ESIndex = "es_extracted";
    public static string GetId(Guid sectionId, int chunkIndex) => $"{sectionId:N}_{chunkIndex}";
}
