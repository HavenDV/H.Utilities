using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Utilities.IntegrationTests
{
    [TestClass]
    public class SimpleTests
    {
        [TestMethod]
        public void ShotTest()
        {
            var image = Screenshoter.Shot();
            var path = $"{Path.GetTempFileName()}.bmp";

            image.Save(path);

            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true,
            });
        }
    }
}
