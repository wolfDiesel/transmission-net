using System.Net;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;

namespace TransmissonNET.App.Avalonia.Providers;

/// <summary>
/// Reads cookies from a WebKitGTK <see cref="NativeWebDialog"/> view via native
/// libsoup/glib calls. Moved from the RuTracker provider into the UI host so the
/// plugin no longer depends on Avalonia/WebKit types.
/// </summary>
internal static class RuTrackerGtkCookies
{
    public static async Task<IReadOnlyList<Cookie>> TryGetAllAsync(
        NativeWebDialog dialog,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsLinux())
            return [];

        var handle = dialog.TryGetWebViewPlatformHandle() as IGtkWebViewPlatformHandle;
        if (handle is null || handle.WebKitWebView == IntPtr.Zero)
        {
            RuTrackerLogInfo("GTK cookie read: no WebKitWebView handle yet");
            return [];
        }

        if (!NativeLibrary.TryLoad("libwebkit2gtk-4.1.so.0", out var webkit)
            && !NativeLibrary.TryLoad("libwebkit2gtk-4.0.so.0", out webkit))
        {
            RuTrackerLogError("GTK cookie read: libwebkit2gtk not found");
            return [];
        }

        if (!NativeLibrary.TryLoad("libsoup-3.0.so.0", out var soup)
            && !NativeLibrary.TryLoad("libsoup-2.4.so.0", out soup))
        {
            NativeLibrary.Free(webkit);
            RuTrackerLogError("GTK cookie read: libsoup not found");
            return [];
        }

        if (!NativeLibrary.TryLoad("libglib-2.0.so.0", out var glib))
        {
            NativeLibrary.Free(soup);
            NativeLibrary.Free(webkit);
            RuTrackerLogError("GTK cookie read: libglib not found");
            return [];
        }

        try
        {
            var getWebsiteData = Get<GetPtr>(webkit, "webkit_web_view_get_website_data_manager");
            var getCookieMgrFromData = Get<GetPtr>(webkit, "webkit_website_data_manager_get_cookie_manager");
            var getContext = Get<GetPtr>(webkit, "webkit_web_view_get_context");
            var getCookieMgrFromContext = Get<GetPtr>(webkit, "webkit_web_context_get_cookie_manager");
            var getAll = Get<GetAllCookies>(webkit, "webkit_cookie_manager_get_all_cookies");
            var getAllFinish = Get<GetAllCookiesFinish>(webkit, "webkit_cookie_manager_get_all_cookies_finish");

            var getName = Get<GetStr>(soup, "soup_cookie_get_name");
            var getValue = Get<GetStr>(soup, "soup_cookie_get_value");
            var getDomain = Get<GetStr>(soup, "soup_cookie_get_domain");
            var getPath = Get<GetStr>(soup, "soup_cookie_get_path");
            var getSecure = Get<GetBool>(soup, "soup_cookie_get_secure");
            var getHttpOnly = Get<GetBool>(soup, "soup_cookie_get_http_only");
            var getExpires = Get<GetPtr>(soup, "soup_cookie_get_expires");
            var freeCookie = Get<FreePtr>(soup, "soup_cookie_free");

            var listFreeFull = Get<ListFreeFull>(glib, "g_list_free_full");
            var dateToUnix = Get<DateToUnix>(glib, "g_date_time_to_unix");

            var webView = handle.WebKitWebView;
            var manager = IntPtr.Zero;
            var dataManager = getWebsiteData(webView);
            if (dataManager != IntPtr.Zero)
                manager = getCookieMgrFromData(dataManager);
            if (manager == IntPtr.Zero)
            {
                var context = getContext(webView);
                if (context != IntPtr.Zero)
                    manager = getCookieMgrFromContext(context);
            }

            if (manager == IntPtr.Zero)
            {
                RuTrackerLogError("GTK cookie read: cookie manager is null");
                return [];
            }

            var tcs = new TaskCompletionSource<IReadOnlyList<Cookie>>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            GAsyncReadyCallback callback = (sourceObject, result, _) =>
            {
                try
                {
                    var err = IntPtr.Zero;
                    var list = getAllFinish(sourceObject, result, ref err);
                    if (err != IntPtr.Zero || list == IntPtr.Zero)
                    {
                        tcs.TrySetResult([]);
                        return;
                    }

                    var cookies = new List<Cookie>();
                    for (var node = list; node != IntPtr.Zero; node = Marshal.ReadIntPtr(node, IntPtr.Size))
                    {
                        var soupCookie = Marshal.ReadIntPtr(node);
                        if (soupCookie == IntPtr.Zero)
                            continue;

                        var name = PtrToUtf8(getName(soupCookie));
                        var value = PtrToUtf8(getValue(soupCookie));
                        if (string.IsNullOrEmpty(name))
                            continue;

                        var domain = PtrToUtf8(getDomain(soupCookie)) ?? string.Empty;
                        var path = PtrToUtf8(getPath(soupCookie)) ?? "/";
                        var cookie = new Cookie(name, value ?? string.Empty)
                        {
                            Domain = domain.TrimStart('.'),
                            Path = path,
                            Secure = getSecure(soupCookie),
                            HttpOnly = getHttpOnly(soupCookie),
                        };

                        var expires = getExpires(soupCookie);
                        if (expires != IntPtr.Zero)
                        {
                            var unix = dateToUnix(expires);
                            if (unix > 0)
                                cookie.Expires = DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
                        }

                        cookies.Add(cookie);
                    }

                    listFreeFull(list, Marshal.GetFunctionPointerForDelegate(freeCookie));
                    RuTrackerLogInfo(
                        $"GTK cookie read: {cookies.Count} cookie(s): "
                        + string.Join(", ", cookies.Select(c => c.Name)));
                    tcs.TrySetResult(cookies);
                }
                catch (Exception ex)
                {
                    RuTrackerLogError("GTK cookie read finish failed: " + ex.Message);
                    tcs.TrySetResult([]);
                }
            };

            var callbackHandle = GCHandle.Alloc(callback);
            try
            {
                getAll(manager, IntPtr.Zero, callback, IntPtr.Zero);
                while (!tcs.Task.IsCompleted)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    PumpMainLoop();
                    await Task.Delay(20, cancellationToken);
                }

                return await tcs.Task;
            }
            finally
            {
                if (callbackHandle.IsAllocated)
                    callbackHandle.Free();
            }
        }
        finally
        {
            NativeLibrary.Free(glib);
            NativeLibrary.Free(soup);
            NativeLibrary.Free(webkit);
        }
    }

    private static void RuTrackerLogInfo(string message) =>
        System.Diagnostics.Debug.WriteLine($"[RuTracker] {message}");

    private static void RuTrackerLogError(string message) =>
        System.Diagnostics.Debug.WriteLine($"[RuTracker] {message}");

    private static void PumpMainLoop()
    {
        if (!NativeLibrary.TryLoad("libglib-2.0.so.0", out var glib))
            return;

        try
        {
            var contextDefault = Get<GetPtr0>(glib, "g_main_context_default");
            var pending = Get<ContextPending>(glib, "g_main_context_pending");
            var iteration = Get<ContextIteration>(glib, "g_main_context_iteration");
            var ctx = contextDefault();
            for (var i = 0; i < 32 && pending(ctx) != 0; i++)
                iteration(ctx, 0);
        }
        catch
        {
        }
        finally
        {
            NativeLibrary.Free(glib);
        }

        if (!NativeLibrary.TryLoad("libgtk-3.so.0", out var gtk))
            return;

        try
        {
            var eventsPending = Get<GetInt0>(gtk, "gtk_events_pending");
            var mainIteration = Get<Action0>(gtk, "gtk_main_iteration");
            for (var i = 0; i < 16 && eventsPending() != 0; i++)
                mainIteration();
        }
        catch
        {
        }
        finally
        {
            NativeLibrary.Free(gtk);
        }
    }

    private static string? PtrToUtf8(IntPtr ptr) =>
        ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);

    private static T Get<T>(IntPtr lib, string name) where T : Delegate =>
        Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(lib, name));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GAsyncReadyCallback(IntPtr sourceObject, IntPtr result, IntPtr userData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetPtr(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetPtr0();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetInt0();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Action0();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GetAllCookies(IntPtr manager, IntPtr cancellable, GAsyncReadyCallback callback, IntPtr userData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetAllCookiesFinish(IntPtr manager, IntPtr result, ref IntPtr error);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetStr(IntPtr cookie);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool GetBool(IntPtr cookie);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void FreePtr(IntPtr ptr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ListFreeFull(IntPtr list, IntPtr freeFunc);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate long DateToUnix(IntPtr dateTime);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ContextPending(IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ContextIteration(IntPtr context, int mayBlock);
}