using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace H.Tests;

/// <summary>
/// 
/// </summary>
public sealed class TestWpfApp : IDisposable
{
    #region Static methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TestWpfApp> CreateAsync(
        CancellationToken cancellationToken = default)
    {
        var application = (Application?)null;
        var exception = (Exception?)null;
        var thread = new Thread(() =>
        {
            try
            {
                application = new Application()
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown,
                };
                application.Run();
            }
            catch (Exception e)
            {
                exception = e;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        while (application == null && exception == null)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken)
                .ConfigureAwait(false);
        }

        if (exception != null)
        {
            throw exception;
        }

        if (application == null)
        {
            throw new InvalidOperationException("application is null.");
        }

        return new TestWpfApp(application);
    }

    #endregion

    #region Properties

    /// <summary>
    /// 
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// 
    /// </summary>
    public Dispatcher Dispatcher => Application.Dispatcher;

    #endregion

    #region Constructors

    /// <summary>
    /// 
    /// </summary>
    /// <param name="application"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public TestWpfApp(Application application)
    {
        Application = application ?? throw new ArgumentNullException(nameof(application));
    }

    #endregion

    #region Public methods

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        Dispatcher.InvokeShutdown();

        var field = typeof(Application).GetField(
            "_appCreatedInThisAppDomain",
            BindingFlags.Static | BindingFlags.NonPublic);
        field?.SetValue(null, false);
    }

    #endregion
}