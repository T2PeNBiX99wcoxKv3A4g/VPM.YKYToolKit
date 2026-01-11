using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager.Requests;

namespace io.github.ykysnk.ykyToolkit.Editor.Extensions
{
    [PublicAPI]
    public static class UpmRequestExtensions
    {
        public static Task<AddRequest> AsTask(this AddRequest request)
        {
            var tcs = new TaskCompletionSource<AddRequest>();

            EditorApplication.update += Check;

            return tcs.Task;

            void Check()
            {
                if (!request.IsCompleted) return;
                EditorApplication.update -= Check;
                tcs.SetResult(request);
            }
        }

        public static Task<ListRequest> AsTask(this ListRequest request)
        {
            var tcs = new TaskCompletionSource<ListRequest>();

            EditorApplication.update += Check;

            return tcs.Task;

            void Check()
            {
                if (!request.IsCompleted) return;
                EditorApplication.update -= Check;
                tcs.SetResult(request);
            }
        }

        public static Task<AddRequest> AsTask(this AddRequest request, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<AddRequest>();

            EditorApplication.update += Check;

            return tcs.Task;

            void Check()
            {
                if (token.IsCancellationRequested)
                {
                    EditorApplication.update -= Check;
                    tcs.TrySetCanceled();
                    return;
                }

                if (!request.IsCompleted) return;
                EditorApplication.update -= Check;
                tcs.TrySetResult(request);
            }
        }

        public static Task<ListRequest> AsTask(this ListRequest request, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<ListRequest>();

            EditorApplication.update += Check;

            return tcs.Task;

            void Check()
            {
                if (token.IsCancellationRequested)
                {
                    EditorApplication.update -= Check;
                    tcs.TrySetCanceled();
                    return;
                }

                if (!request.IsCompleted) return;
                EditorApplication.update -= Check;
                tcs.TrySetResult(request);
            }
        }
    }
}