using System;
using System.Threading;
using System.Threading.Tasks;
using H.Tests.Extensions;
using Timer = System.Timers.Timer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Tests.UnitTests;

[TestClass]
public class Tests
{
    [TestMethod]
    public async Task WithEventLoggingTest()
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = cancellationTokenSource.Token;

        using var timer = new Timer(1000).WithEventLogging();
        timer.Start();

        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
    }
}