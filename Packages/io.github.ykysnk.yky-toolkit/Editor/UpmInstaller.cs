using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.ykyToolkit.Editor.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    // TODO: Add new window for package install
    public class UpmInstaller
    {
        private readonly string[] _packages;

        public UpmInstaller(string[] packages) => _packages = packages;

        public async Task InstallAsync() => await InstallAsync(CancellationToken.None);

        [PublicAPI]
        public async Task InstallAsync(CancellationToken token)
        {
            var progressId = Progress.Start(
                "Installing UPM Packages",
                "Preparing...",
                Progress.Options.Sticky | Progress.Options.Indefinite
            );

            Progress.RegisterCancelCallback(progressId, () =>
            {
                Utils.Log(nameof(UpmInstaller), "Cancel requested by user.");
                token.ThrowIfCancellationRequested();
                return true;
            });

            try
            {
                Progress.Report(progressId, 0f, "Checking installed packages...");

                var listRequest = await Client.List().AsTask(token);
                var installed = listRequest.Result.Select(p => p.name).ToHashSet();

                var total = _packages.Length;
                var index = 0;

                foreach (var pkg in _packages)
                {
                    index++;
                    var progress = (float)index / total;

                    if (installed.Contains(pkg))
                    {
                        Utils.Log(nameof(UpmInstaller), $"Skip (already installed): {pkg}");
                        Progress.Report(progressId, progress, $"Skip (already installed): {pkg}");
                        continue;
                    }

                    Utils.Log(nameof(UpmInstaller), $"Installing: {pkg}");
                    Progress.Report(progressId, progress, $"Installing: {pkg}");

                    var addRequest = await Client.Add(pkg).AsTask(token);
                    if (addRequest.Status == StatusCode.Success)
                        Utils.Log(nameof(UpmInstaller), $"Installed: {addRequest.Result.packageId}");
                    else
                        Utils.LogError(nameof(UpmInstaller), $"Failed: {addRequest.Error.message}");
                }

                Progress.Finish(progressId);
                Utils.Log(nameof(UpmInstaller), "All packages processed.");
            }
            catch (OperationCanceledException)
            {
                Progress.Finish(progressId, Progress.Status.Canceled);
                Utils.LogWarning(nameof(UpmInstaller), "Installation cancelled.");
            }
            catch (Exception ex)
            {
                Progress.Finish(progressId, Progress.Status.Failed);
                Utils.LogError(nameof(UpmInstaller), $"Installation Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}