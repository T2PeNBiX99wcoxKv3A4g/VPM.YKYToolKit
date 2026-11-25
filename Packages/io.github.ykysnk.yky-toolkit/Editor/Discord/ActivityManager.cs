using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Discord
{
    [PublicAPI]
    public partial class ActivityManager
    {
        public void RegisterCommand()
        {
            RegisterCommand(null);
        }
    }
}