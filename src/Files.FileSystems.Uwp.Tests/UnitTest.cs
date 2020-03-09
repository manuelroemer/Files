namespace Files.FileSystems.Uwp.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Windows.Storage;

    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public async Task TestMethod1()
        {
            var appDataPaths = AppDataPaths.GetDefault();
            var appData = ApplicationData.Current;
            var tmp = appData.TemporaryFolder.Path;
            var path = AppDataPaths.GetDefault().ProgramData;
            var folder = await StorageFolder.GetFolderFromPathAsync(path);
            var files = await folder.GetFilesAsync();
        }

    }

}
