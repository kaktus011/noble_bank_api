namespace NobleBank.Infrastructure.Settings
{
    /// <summary>
    /// Configuration for seeding the default admin user.
    /// This should be provided via secure configuration (user-secrets, environment variables, or Key Vault),
    /// NOT hardcoded in the application.
    /// </summary>
    public class AdminSeederSettings
    {
        public const string SectionName = "AdminSeeder";

        /// <summary>
        /// The email address of the default admin account.
        /// Leave empty or set to null to skip admin seeding.
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// The password for the default admin account.
        /// Should be a strong password following Identity requirements.
        /// Leave empty or set to null to skip admin seeding.
        /// </summary>
        public string? Password { get; init; }

        /// <summary>
        /// If true, skip seeding admin user even if credentials are provided.
        /// Useful for preventing accidental seeding in production.
        /// </summary>
        public bool Disabled { get; init; }

        /// <summary>
        /// Validation: both Email and Password must be provided to seed.
        /// </summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !Disabled;
    }
}
