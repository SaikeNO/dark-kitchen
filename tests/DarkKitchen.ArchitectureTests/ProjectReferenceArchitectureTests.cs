using System.Xml.Linq;

namespace DarkKitchen.ArchitectureTests;

public sealed class ProjectReferenceArchitectureTests
{
    [Fact]
    public void ServiceApiProjects_ReferenceServiceDefaultsAndDoNotReferenceOtherBoundedContexts()
    {
        var root = RepositoryPaths.FindRoot();
        var servicesRootPath = NormalizePath(Path.Combine(root, "src", "Services")) + "/";
        var serviceDefaultsProjectPath = NormalizePath(Path.Combine(root, "src", "DarkKitchen.ServiceDefaults", "DarkKitchen.ServiceDefaults.csproj"));
        var serviceProjects = GetServiceApiProjects(root);

        Assert.NotEmpty(serviceProjects);

        foreach (var projectPath in serviceProjects)
        {
            var boundedContextRootPath = GetBoundedContextRootPath(root, projectPath);
            var references = ReadProjectReferences(projectPath);

            Assert.Contains(serviceDefaultsProjectPath, references);

            Assert.DoesNotContain(
                references,
                reference => reference.StartsWith(servicesRootPath, StringComparison.OrdinalIgnoreCase)
                    && !reference.StartsWith(boundedContextRootPath, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void NonApiServiceProjects_DoNotReferenceServiceDefaults()
    {
        var root = RepositoryPaths.FindRoot();
        var serviceDefaultsProjectPath = NormalizePath(Path.Combine(root, "src", "DarkKitchen.ServiceDefaults", "DarkKitchen.ServiceDefaults.csproj"));
        var nonApiServiceProjects = GetServiceProjects(root)
            .Except(GetServiceApiProjects(root), StringComparer.OrdinalIgnoreCase);

        foreach (var projectPath in nonApiServiceProjects)
        {
            Assert.DoesNotContain(serviceDefaultsProjectPath, ReadProjectReferences(projectPath));
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

        var serviceProjectNames = GetServiceApiProjects(root)
            .Select(Path.GetFileName)
            .ToArray();

        Assert.All(serviceProjectNames, projectName => Assert.Contains(projectName, appHostReferences));
    }

    [Fact]
    public void AppHost_ReferencesOnlyServiceApiProjectsFromServicesFolder()
    {
        var root = RepositoryPaths.FindRoot();
        var servicesRootPath = NormalizePath(Path.Combine(root, "src", "Services")) + "/";
        var appHostProject = Path.Combine(root, "src", "DarkKitchen.AppHost", "DarkKitchen.AppHost.csproj");
        var appHostServiceReferences = ReadProjectReferences(appHostProject)
            .Where(reference => reference.StartsWith(servicesRootPath, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.NotEmpty(appHostServiceReferences);
        Assert.All(
            appHostServiceReferences,
            reference => Assert.EndsWith(".Api.csproj", reference, StringComparison.OrdinalIgnoreCase));
    }

    private static string[] GetServiceProjects(string root)
    {
        return Directory
            .EnumerateFiles(Path.Combine(root, "src", "Services"), "*.csproj", SearchOption.AllDirectories)
            .Select(NormalizePath)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string[] GetServiceApiProjects(string root)
    {
        return GetServiceProjects(root)
            .Where(projectPath => Path.GetFileNameWithoutExtension(projectPath).EndsWith(".Api", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static string GetBoundedContextRootPath(string root, string projectPath)
    {
        var servicesRoot = Path.Combine(root, "src", "Services");
        var relativePath = Path.GetRelativePath(servicesRoot, projectPath);
        var contextDirectory = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];

        return NormalizePath(Path.Combine(servicesRoot, contextDirectory)) + "/";
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
