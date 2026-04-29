using System.Xml.Linq;

namespace DarkKitchen.ArchitectureTests;

public sealed class ProjectReferenceArchitectureTests
{
    [Fact]
    public void ServiceApiProjects_ReferenceServiceDefaultsOnlyFromSharedInfrastructure()
    {
        var root = RepositoryPaths.FindRoot();
        var serviceProjects = Directory
            .EnumerateFiles(Path.Combine(root, "src", "Services"), "*.csproj", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(serviceProjects);

        foreach (var projectPath in serviceProjects)
        {
            var references = ReadProjectReferences(projectPath);

            Assert.Contains(
                references,
                reference => reference.EndsWith(Path.Combine("DarkKitchen.ServiceDefaults", "DarkKitchen.ServiceDefaults.csproj"), StringComparison.OrdinalIgnoreCase));

            Assert.DoesNotContain(
                references,
                reference => reference.Contains(Path.Combine("src", "Services"), StringComparison.OrdinalIgnoreCase));
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
            .Select(include => Path.GetFullPath(Path.Combine(projectDirectory, include!.Replace("\\", "/"))))
            .Select(path => path.Replace("\\", "/"))
            .ToArray();
    }
}
