using System.Diagnostics;
using System.Drawing;
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

        [TestMethod]
        public void RectangleShotTest()
        {
            var image = Screenshoter.Shot(Rectangle.FromLTRB(4509, 808, 5278, 1426));
            var path = $"{Path.GetTempFileName()}.bmp";

            image.Save(path);

            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true,
            });
        }
    }
}
