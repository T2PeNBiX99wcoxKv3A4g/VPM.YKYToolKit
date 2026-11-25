using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Editor.Discord
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