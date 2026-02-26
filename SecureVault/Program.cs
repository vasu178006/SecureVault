// ============================================
// SecureVault - Application Entry Point
// ============================================

using SecureVault.UI.Forms;

namespace SecureVault
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the SecureVault application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
    }
}