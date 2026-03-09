public class EncryptionSettings
{
    public const string SectionName = "Encryption";

    public string Key { get; init; } = string.Empty;
    public string IV { get; init; } = string.Empty;
}