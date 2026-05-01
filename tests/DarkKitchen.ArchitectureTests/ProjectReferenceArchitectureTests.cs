using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DarkKitchen.ArchitectureTests;

public sealed partial class ProjectReferenceArchitectureTests
{
    [Fact]
    public void ServiceBoundedContexts_ExposeCleanArchitectureClassLibraries()
    {
        var root = RepositoryPaths.FindRoot();
        var contextDirectories = GetServiceContextDirectories(root);

        Assert.NotEmpty(contextDirectories);

        foreach (var contextDirectory in contextDirectories)
        {
            Assert.True(File.Exists(GetContextProjectPath(contextDirectory, "Api")), $"{contextDirectory} must contain an Api project.");
            Assert.True(File.Exists(GetContextProjectPath(contextDirectory, "Domain")), $"{contextDirectory} must contain a Domain class library.");
            Assert.True(File.Exists(GetContextProjectPath(contextDirectory, "Features")), $"{contextDirectory} must contain a Features class library.");
            Assert.True(File.Exists(GetContextProjectPath(contextDirectory, "Infrastructure")), $"{contextDirectory} must contain an Infrastructure class library.");
        }
    }

    [Fact]
    public void ServiceProjects_FollowCleanArchitectureReferences()
    {
        var root = RepositoryPaths.FindRoot();
        var serviceDefaultsProjectPath = NormalizePath(Path.Combine(root, "src", "DarkKitchen.ServiceDefaults", "DarkKitchen.ServiceDefaults.csproj"));
        var contextDirectories = GetServiceContextDirectories(root);

        foreach (var contextDirectory in contextDirectories)
        {
            var apiProject = NormalizePath(GetContextProjectPath(contextDirectory, "Api"));
            var domainProject = NormalizePath(GetContextProjectPath(contextDirectory, "Domain"));
            var featuresProject = NormalizePath(GetContextProjectPath(contextDirectory, "Features"));
            var infrastructureProject = NormalizePath(GetContextProjectPath(contextDirectory, "Infrastructure"));

            var apiReferences = ReadProjectReferences(apiProject);
            Assert.Contains(serviceDefaultsProjectPath, apiReferences);
            Assert.Contains(featuresProject, apiReferences);
            Assert.Contains(infrastructureProject, apiReferences);
            Assert.DoesNotContain(domainProject, apiReferences);

            Assert.Empty(ReadProjectReferences(domainProject));

            var featuresReferences = ReadProjectReferences(featuresProject);
            Assert.Contains(domainProject, featuresReferences);
            Assert.DoesNotContain(infrastructureProject, featuresReferences);
            Assert.DoesNotContain(apiProject, featuresReferences);
            Assert.DoesNotContain(serviceDefaultsProjectPath, featuresReferences);

            var infrastructureReferences = ReadProjectReferences(infrastructureProject);
            Assert.Contains(domainProject, infrastructureReferences);
            Assert.DoesNotContain(apiProject, infrastructureReferences);
            Assert.DoesNotContain(serviceDefaultsProjectPath, infrastructureReferences);
        }
    }

    [Fact]
    public void ApiProjects_KeepOnlyCompositionRootSource()
    {
        var root = RepositoryPaths.FindRoot();
        var allowedRootFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Program.cs"
        };

        foreach (var projectPath in GetServiceApiProjects(root))
        {
            var projectDirectory = Path.GetDirectoryName(projectPath)!;
            var sourceFiles = GetProjectSourceFiles(projectDirectory)
                .Select(path => Path.GetRelativePath(projectDirectory, path))
                .ToArray();

            Assert.All(sourceFiles, sourceFile => Assert.Contains(sourceFile, allowedRootFiles));
        }
    }

    [Fact]
    public void FeatureProjects_ContainVerticalSlicesFolder()
    {
        var root = RepositoryPaths.FindRoot();

        foreach (var contextDirectory in GetServiceContextDirectories(root))
        {
            var featuresProject = GetContextProjectPath(contextDirectory, "Features");
            var featuresDirectory = Path.GetDirectoryName(featuresProject)!;

            Assert.True(
                Directory.Exists(Path.Combine(featuresDirectory, "Features")),
                $"{featuresProject} must contain a Features folder for vertical slices.");
        }
    }

    [Fact]
    public void DomainProjects_KeepOnePublicDomainTypePerFile()
    {
        var root = RepositoryPaths.FindRoot();

        foreach (var contextDirectory in GetServiceContextDirectories(root))
        {
            var domainProject = GetContextProjectPath(contextDirectory, "Domain");
            var domainDirectory = Path.GetDirectoryName(domainProject)!;

            foreach (var sourceFile in GetProjectSourceFiles(domainDirectory))
            {
                var publicTypes = PublicTypePattern()
                    .Matches(File.ReadAllText(sourceFile))
                    .Select(match => match.Groups["name"].Value)
                    .ToArray();

                if (publicTypes.Length == 0)
                {
                    continue;
                }

                Assert.Single(publicTypes);
                Assert.Equal(Path.GetFileNameWithoutExtension(sourceFile), publicTypes[0]);
            }
        }
    }

    [GeneratedRegex(@"public\s+(?:sealed\s+|abstract\s+)?(?:class|record|struct|enum)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)")]
    private static partial Regex PublicTypePattern();

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

    private static string[] GetServiceContextDirectories(string root)
    {
        return Directory
            .EnumerateDirectories(Path.Combine(root, "src", "Services"))
            .Select(NormalizePath)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string GetContextProjectPath(string contextDirectory, string layer)
    {
        var contextName = Path.GetFileName(contextDirectory);

        return Path.Combine(
            contextDirectory,
            $"DarkKitchen.{contextName}.{layer}",
            $"DarkKitchen.{contextName}.{layer}.csproj");
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

    private static string[] GetProjectSourceFiles(string projectDirectory)
    {
        return Directory
            .EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedBuildOutput(projectDirectory, path))
            .Select(NormalizePath)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsGeneratedBuildOutput(string projectDirectory, string path)
    {
        var relativePath = Path.GetRelativePath(projectDirectory, path);
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return segments.Contains("bin", StringComparer.OrdinalIgnoreCase)
            || segments.Contains("obj", StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path).Replace("\\", "/");
    }
}
