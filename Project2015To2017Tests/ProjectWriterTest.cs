using Microsoft.VisualStudio.TestTools.UnitTesting;
using Project2015To2017.Writing;

namespace Project2015To2017Tests
{
    [TestClass]
    public class ProjectWriterTest
	{
        [TestMethod]
        public void ProgramDatabaseFilesArePackaged()
        {
            var writer = new ProjectWriter();
            var xmlNode = writer.CreateXml(new Project2015To2017.Definition.Project
            {
                AssemblyAttributes = new Project2015To2017.Definition.AssemblyAttributes()
            }, new System.IO.FileInfo("test.cs"));

            var node = xmlNode.Element("PropertyGroup").Element("AllowedOutputExtensionsInPackageBuildOutputFolder");
            Assert.IsNotNull(node);
            Assert.AreEqual("$(AllowedOutputExtensionsInPackageBuildOutputFolder).pdb;", node.Value);
        }

        [TestMethod]
        public void GeneratePackageOnBuild()
        {
            var writer = new ProjectWriter();
            var xmlNode = writer.CreateXml(new Project2015To2017.Definition.Project
            {
                AssemblyAttributes = new Project2015To2017.Definition.AssemblyAttributes()
            }, new System.IO.FileInfo("test.cs"));

            var node = xmlNode.Element("PropertyGroup").Element("GeneratePackageOnBuild");
            Assert.IsNotNull(node);
            Assert.AreEqual("true", node.Value);
        }

        [TestMethod]
        public void Version()
        {
            var writer = new ProjectWriter();
            var xmlNode = writer.CreateXml(new Project2015To2017.Definition.Project
            {
                AssemblyAttributes = new Project2015To2017.Definition.AssemblyAttributes { InformationalVersion = "1.4.0" }
            }, new System.IO.FileInfo("test.cs"));

            var node = xmlNode.Element("PropertyGroup").Element("Version");
            Assert.IsNotNull(node);
            Assert.AreEqual("1.4.0", node.Value);
        }

        [TestMethod]
        public void DocumentationFile()
        {
            var writer = new ProjectWriter();
            var xmlNode = writer.CreateXml(new Project2015To2017.Definition.Project
            {
                AssemblyAttributes = new Project2015To2017.Definition.AssemblyAttributes()
            }, new System.IO.FileInfo("test.cs"));

            var node = xmlNode.Element("PropertyGroup").Element("DocumentationFile");
            Assert.IsNotNull(node);
            Assert.AreEqual(@"bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml", node.Value);
        }
    }
}
