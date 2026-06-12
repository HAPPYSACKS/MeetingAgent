using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using FluentAssertions;

namespace MeetingAgent.UnitTests;

public class TeamsAppPackageTests
{
    [Fact]
    public void NewTeamsAppPackage_CreatesExpectedMeetingManifestAndZipContents()
    {
        var repoRoot = FindRepoRoot();
        var outputDirectory = Path.Combine(Path.GetTempPath(), "meetingagent-teams-package-" + Guid.NewGuid().ToString("N"));
        var scriptPath = Path.Combine(repoRoot, "scripts", "New-TeamsAppPackage.ps1");
        var teamsAppId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var entraClientId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        try
        {
            RunPowerShellScript(
                scriptPath,
                "-BaseUrl", "https://meetingagent-web.example.devtunnels.ms",
                "-TeamsAppId", teamsAppId.ToString(),
                "-EntraClientId", entraClientId.ToString(),
                "-OutputDirectory", outputDirectory);

            var packagePath = Path.Combine(outputDirectory, "MeetingAgent.TeamsApp.zip");
            File.Exists(packagePath).Should().BeTrue();

            using var archive = ZipFile.OpenRead(packagePath);
            archive.Entries.Select(entry => entry.FullName).Should().BeEquivalentTo([
                "manifest.json",
                "color.png",
                "outline.png"
            ]);

            var manifestEntry = archive.GetEntry("manifest.json");
            manifestEntry.Should().NotBeNull();

            using var manifestStream = manifestEntry!.Open();
            using var manifest = JsonDocument.Parse(manifestStream);
            var root = manifest.RootElement;

            root.GetProperty("$schema").GetString().Should().Be("https://developer.microsoft.com/json-schemas/teams/v1.28/MicrosoftTeams.schema.json");
            root.GetProperty("manifestVersion").GetString().Should().Be("1.28");
            root.GetProperty("id").GetString().Should().Be(teamsAppId.ToString());

            var configurableTab = root.GetProperty("configurableTabs").EnumerateArray().Should().ContainSingle().Subject;
            configurableTab.GetProperty("configurationUrl").GetString().Should().Be("https://meetingagent-web.example.devtunnels.ms/Teams/Configure");
            configurableTab.GetProperty("scopes").EnumerateArray().Select(scope => scope.GetString()).Should().Equal("groupchat");
            configurableTab.GetProperty("context").EnumerateArray().Select(context => context.GetString()).Should().Equal(
                "meetingDetailsTab",
                "meetingSidePanel");

            root.TryGetProperty("bots", out _).Should().BeFalse();
            root.TryGetProperty("composeExtensions", out _).Should().BeFalse();
            root.TryGetProperty("staticTabs", out _).Should().BeFalse();

            var permissions = root
                .GetProperty("authorization")
                .GetProperty("permissions")
                .GetProperty("resourceSpecific")
                .EnumerateArray()
                .Select(permission => new
                {
                    Name = permission.GetProperty("name").GetString(),
                    Type = permission.GetProperty("type").GetString()
                })
                .ToArray();

            permissions.Should().BeEquivalentTo([
                new { Name = "OnlineMeeting.ReadBasic.Chat", Type = "Application" },
                new { Name = "OnlineMeetingTranscript.Read.Chat", Type = "Application" }
            ]);

            root.GetProperty("webApplicationInfo").GetProperty("id").GetString().Should().Be(entraClientId.ToString());
            root.GetProperty("webApplicationInfo").GetProperty("resource").GetString().Should().Be($"api://meetingagent-web.example.devtunnels.ms/{entraClientId}");
            root.GetProperty("validDomains").EnumerateArray().Select(domain => domain.GetString()).Should().Equal(
                "meetingagent-web.example.devtunnels.ms",
                "res.cdn.office.net");
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }

    private static void RunPowerShellScript(string scriptPath, params string[] arguments)
    {
        var executable = OperatingSystem.IsWindows() ? "powershell.exe" : "pwsh";
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo);
        process.Should().NotBeNull();

        var standardOutput = process!.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();

        process.WaitForExit();
        process.ExitCode.Should().Be(0, "stdout: {0}{1}stderr: {2}", standardOutput, Environment.NewLine, standardError);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MeetingAgent.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find repository root.");
    }
}
