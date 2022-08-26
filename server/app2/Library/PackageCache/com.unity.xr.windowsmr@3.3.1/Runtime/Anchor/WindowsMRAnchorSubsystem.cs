using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.WindowsMR
{
    /// <summary>
    /// The WindowsMR implementation of the <c>XRAnchorSubsystem</c>. Do not create this directly.
    /// Use <c>XRAnchorSubsystemDescriptor.Create()</c> instead.
    /// </summary>
    [Preserve]
    public sealed class WindowsMRAnchorSubsystem : XRAnchorSubsystem
    {
        protected override Provider CreateProvider()
        {
            return new WindowsMRProvider();
        }

        class WindowsMRProvider : Provider
        {
            public override void Start()
            {
                NativeApi.UnityWindowsMR_refPoints_start();
            }

            public override void Stop()
            {
                NativeApi.UnityWindowsMR_refPoints_stop();
            }

            public override void Destroy()
            {
                NativeApi.UnityWindowsMR_refPoints_onDestroy();
            }

            public override unsafe TrackableChanges<XRAnchor> GetChanges(
                XRAnchor defaultAnchor,
                Allocator allocator)
            {
                int addedCount, updatedCount, removedCount, elementSize;
                void* addedPtr, updatedPtr, removedPtr;
                var context = NativeApi.UnityWindowsMR_refPoints_acquireChanges(
                    out addedPtr, out addedCount,
                    out updatedPtr, out updatedCount,
                    out removedPtr, out removedCount,
                    out elementSize);

                try
                {
                    return new TrackableChanges<XRAnchor>(
                        addedPtr, addedCount,
                        updatedPtr, updatedCount,
                        removedPtr, removedCount,
                        defaultAnchor, elementSize,
                        allocator);
                }
                finally
                {
                    NativeApi.UnityWindowsMR_refPoints_releaseChanges(context);
                }
            }

            public override bool TryAddAnchor(
                Pose pose,
                out XRAnchor anchor)
            {
                return NativeApi.UnityWindowsMR_refPoints_tryAdd(pose, out anchor);
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                return NativeApi.UnityWindowsMR_refPoints_tryRemove(anchorId);
            }

            static class NativeApi
            {
#if UNITY_EDITOR
                [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
                [DllImport("WindowsMRXRSDK.dll")]
#else
                [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
                public static extern void UnityWindowsMR_refPoints_start();

#if UNITY_EDITOR
                [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
                [DllImport("WindowsMRXRSDK.dll")]
#else
                [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
                public static extern void UnityWindowsMR_refPoints_stop();

#if UNITY_EDITOR
                [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
                [DllImport("WindowsMRXRSDK.dll")]
#else
                [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
                public static extern void UnityWindowsMR_refPoints_onDestroy();

#if UNITY_EDITOR
                [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
                [DllImport("WindowsMRXRSDK.dll")]
#else
                [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
                public static extern unsafe void* UnityWindowsMR_refPoints_acquireChanges(
                    out void* addedPtr, out int addedCount,
                    out void* updatedPtr, out int updatedCount,
                    out void* removedPtr, out int removedCount,
                    out int elementSize);

#if UNITY_EDITOR
                [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
                [DllImport("WindowsMRXRSDK.dll")]
#else
                [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
                public static extern unsafe void UnityWindowsMR_refPoints_releaseChanges(
                    void* changes);

#if UNITY_EDITOR
                [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
                [DllImport("WindowsMRXRSDK.dll")]
#else
                [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
                public static extern bool UnityWindowsMR_refPoints_tryAdd(
                    Pose pose,
                    out XRAnchor anchor);

#if UNITY_EDITOR
                [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
                [DllImport("WindowsMRXRSDK.dll")]
#else
                [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
                public static extern bool UnityWindowsMR_refPoints_tryRemove(TrackableId anchorId);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRAnchorSubsystemDescriptor.Create(new XRAnchorSubsystemDescriptor.Cinfo
            {
                id = "Windows Mixed Reality Anchor",
                subsystemImplementationType = typeof(WindowsMRAnchorSubsystem),
                supportsTrackableAttachments = false
            });
        }
    }
}
