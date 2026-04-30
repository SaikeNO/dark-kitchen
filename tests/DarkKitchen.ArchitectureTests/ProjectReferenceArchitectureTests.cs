using System.Xml.Linq;

namespace DarkKitchen.ArchitectureTests;

public sealed class ProjectReferenceArchitectureTests
{
    [Fact]
    public void ServiceApiProjects_ReferenceServiceDefaultsOnlyFromSharedInfrastructure()
    {
        var root = RepositoryPaths.FindRoot();
        var serviceDefaultsProjectPath = NormalizePath(Path.Combine(root, "src", "DarkKitchen.ServiceDefaults", "DarkKitchen.ServiceDefaults.csproj"));
        var servicesRootPath = NormalizePath(Path.Combine(root, "src", "Services")) + "/";
        var serviceProjects = Directory
            .EnumerateFiles(Path.Combine(root, "src", "Services"), "*.csproj", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(serviceProjects);

        foreach (var projectPath in serviceProjects)
        {
            var references = ReadProjectReferences(projectPath);

            Assert.Contains(serviceDefaultsProjectPath, references);

            Assert.DoesNotContain(
                references,
                reference => reference.StartsWith(servicesRootPath, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void AppHost_ReferencesEveryServiceApiProject()
    {
        var root = RepositoryPaths.FindRoot();
        var appHostProject = Path.Combine(root, "src", "DarkKitchen.AppHost", "DarkKitchen.AppHost.csproj");
        var appHostReferences = ReadProjectReferences(appHostProject)
            .Select(Path.GetFileName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var serviceProjectNames = Directory
            .EnumerateFiles(Path.Combine(root, "src", "Services"), "*.csproj", SearchOption.AllDirectories)
            .Select(Path.GetFileName)
            .ToArray();

        Assert.All(serviceProjectNames, projectName => Assert.Contains(projectName, appHostReferences));
    }

    private static string[] ReadProjectReferences(string projectPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        var document = XDocument.Load(projectPath);

        return document
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => NormalizePath(Path.GetFullPath(Path.Combine(projectDirectory, include!.Replace("\\", "/")))))
            .ToArray();
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path).Replace("\\", "/");
    }
}
