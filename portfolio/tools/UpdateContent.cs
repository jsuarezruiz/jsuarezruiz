// Regenerates content.json from the public GitHub API.
//
//   dotnet run tools/UpdateContent.cs
//
// Set GITHUB_TOKEN to raise the API rate limit; it is not required.

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

const string User = "jsuarezruiz";
const int PerEra = 8;

var root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ThisFile())!, ".."));
string[] targets =
[
    Path.Combine(root, "Portfolio", "Assets", "content.json"),
    Path.Combine(root, "Portfolio.Browser", "wwwroot", "content.json"),
];

using var http = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("portfolio-content", "1.0"));
http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
if (Environment.GetEnvironmentVariable("GITHUB_TOKEN") is { Length: > 0 } token)
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

var profile = await Get($"users/{User}");

var repos = new List<JsonNode>();
for (var page = 1; ; page++)
{
    var batch = (await Get($"users/{User}/repos?per_page=100&page={page}")).AsArray();
    repos.AddRange(batch.Where(r => !r!["fork"]!.GetValue<bool>())!);
    if (batch.Count < 100)
        break;
}

// GitHub only returns the email to authenticated calls. Without a token, keep the one already saved.
var mailto = Text(profile, "email") is { Length: > 0 } email ? "mailto:" + email : SavedMailto(targets[0]);

var projects = new JsonArray();
foreach (var era in new[] { "xamarin", "maui", "avalonia" })
{
    foreach (var repo in repos.Where(r => EraOf(r) == era).OrderByDescending(Stars).Take(PerEra))
    {
        projects.Add((JsonNode)new JsonObject
        {
            ["name"] = Text(repo, "name"),
            ["era"] = era,
            ["stars"] = Stars(repo),
            ["description"] = Text(repo, "description").Trim(),
            ["url"] = Text(repo, "html_url"),
        });
    }
}

var content = new JsonObject
{
    ["profile"] = new JsonObject
    {
        ["name"] = Text(profile, "name"),
        ["login"] = User,
        ["bio"] = Text(profile, "bio"),
        ["location"] = Text(profile, "location"),
        ["followers"] = profile["followers"]!.GetValue<int>(),
        ["repos"] = repos.Count,
        ["stars"] = repos.Sum(Stars),
        ["url"] = Text(profile, "html_url"),
    },
    ["eras"] = Array(
        Era("xamarin", "Xamarin.Forms", "2014-2021"),
        Era("maui", ".NET MAUI", "2021-2025"),
        Era("avalonia", "Avalonia", "2026-")
    ),
    ["links"] = Array(
        Link("Email me", mailto),
        Link("GitHub", Text(profile, "html_url")),
        Link("LinkedIn", "https://linkedin.com/in/jsuarezruiz"),
        Link("YouTube", "https://www.youtube.com/javiersuarezruiz"),
        Link("Blog", "https://javiersuarezruiz.wordpress.com")
    ),
    // Live WebAssembly builds that open on the phone in the room.
    ["apps"] = Array(
        App("Nova.Avalonia.UI", "https://jsuarezruiz.github.io/Nova.Avalonia.UI/gallery/"),
        App("Cupertino.Avalonia", "https://jsuarezruiz.github.io/Cupertino.Avalonia/gallery/"),
        App("Nova.Avalonia.AIKit", "https://jsuarezruiz.github.io/Nova.Avalonia.AIKit/gallery/"),
        App("Nova.Avalonia.Animations", "https://jsuarezruiz.github.io/Nova.Avalonia.Animations/gallery/"),
        App("avalonia_vignettes", "https://jsuarezruiz.github.io/avalonia_vignettes/")
    ),
    // Files live in Portfolio/Assets/Photos. A missing file shows a drawn placeholder.
    ["photos"] = Array(
        Photo("monkeyconf.jpg", "MonkeyConf"),
        Photo("sevilladotnet.jpg", "SevillaDotNet meetup"),
        Photo("dotnetconf.jpg", ".NET Conf: Focus on Xamarin"),
        Photo("avaloniaconf.jpg", "Avalonia Conf Online")
    ),
    // The CV: taken from "Javier Suarez Resume.pdf" in the GitHub profile repository, plus the
    // current role and recent work from the public GitHub and LinkedIn profiles.
    ["resume"] = new JsonObject
    {
        ["title"] = "Software Engineer",
        ["summary"] = "I want to learn something new every day, and I enjoy sharing ideas through technology that make life easier.",
        ["pdf"] = "https://github.com/jsuarezruiz/jsuarezruiz/blob/master/Javier%20Suarez%20Resume.pdf",
        ["experience"] = Array(
            Job("Avalonia UI", "Software Engineer", "Now",
                "Building cross-platform UI for .NET.",
                "Cupertino.Avalonia: an iOS design system for Avalonia, with control themes, motion and Liquid Glass.",
                "Nova.Avalonia.UI: a suite of controls and layout panels.",
                "Avalonia.Vignettes, and dual-screen and foldable samples with a TwoPaneView control."),
            Job("Microsoft", "Senior Software Engineer", "From Dec 2018",
                "Led the UI APIs in Xamarin.Forms: gradients, shapes and view clipping, which raised satisfaction in customer surveys.",
                "Created new controls such as RefreshView (pull to refresh).",
                "Built core .NET MAUI APIs: lifecycle events, shadows and a new cross-platform drawing API.",
                "Led an experimental project for drawn controls in Cupertino, Material and Fluent styles."),
            Job("Plain Concepts", "Software Engineer", "Aug 2015 to Dec 2018",
                "Managed the mobile development team.",
                "Built Microsoft's Xamarin.Forms demos for Build and Connect, including BikeSharing and SmartHotel360.",
                "Led the first external Xamarin.Forms backend, adding Linux support with GtkSharp.",
                "Helped add WPF support to Xamarin.Forms, and built Visual Studio extensions with that team.",
                "Wrote a small UI framework for a game engine, rendering with OpenGL and DirectX."),
            Job("Bravent", "Mobile Team Lead", "Nov 2014 to Aug 2015",
                "Created and managed the mobile development team.",
                "Earned several Xamarin certifications and a Microsoft partnership within months.",
                "Shipped cross-platform apps for banks, and the official Real Madrid app for Microsoft."),
            Job("Icinetic TIC", "Software Engineer", "Dec 2012 to Oct 2014",
                "Built Windows Phone and Windows Store apps for radio stations, clothing stores and others.",
                "Helped create Windows Phone App Studio, Microsoft's drag-and-drop app builder that also generated the code."),
            Job("iDesarrolla", "Software Engineer", "Jun 2009 to Dec 2012",
                "Worked mainly with WPF, Silverlight and Windows Phone, plus some Objective-C for iOS.")
        ),
        ["openSource"] = Strings(
            "Xamarin.Forms GoodLooking UI: samples that recreate real apps and designs.",
            "TemplateUI: a Xamarin.Forms controls toolkit.",
            "GraphicsControls: drawn controls for .NET MAUI.",
            "awesome-dotnet-maui and awesome-xamarin-forms: curated lists."),
        ["achievements"] = Strings(
            "Microsoft MVP, 2014 to 2019",
            "Xamarin MVP",
            "Xamarin Certified Mobile Developer",
            "Microsoft Active Professional 2014"),
        ["communities"] = Strings(
            "SevillaDotNet: .NET user group in Seville (organiser).",
            "SXDG: Xamarin developers group in Seville (organiser).",
            "Author of the CampusMVP course on mobile development with C# and Xamarin."),
        ["education"] = Strings(
            "Superior Technician in Management of Computer Systems. Centro Educativo Altair, Sevilla, 2007 to 2009."),
        ["skills"] = Strings(".NET", "Avalonia", ".NET MAUI", "Xamarin", "WinUI", "WPF", "UWP", "Azure", "Flutter"),
        ["languages"] = Strings("Spanish", "English"),
    },
    ["projects"] = projects,
};

var json = content.ToJsonString(new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
});

foreach (var target in targets)
{
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
    await File.WriteAllTextAsync(target, json + "\n");
    Console.WriteLine("wrote " + Path.GetRelativePath(root, target));
}

async Task<JsonNode> Get(string path)
{
    using var response = await http.GetAsync(path);
    response.EnsureSuccessStatusCode();
    return JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
}

static string Text(JsonNode node, string key) => node[key]?.GetValue<string>() ?? "";

static int Stars(JsonNode repo) => repo["stargazers_count"]!.GetValue<int>();

// Which stretch of the career a repository belongs to, judged by its name and last push.
static string? EraOf(JsonNode repo)
{
    var name = Text(repo, "name").ToLowerInvariant();
    if (name.Contains("avalonia") || name.Contains("evergine"))
        return "avalonia";
    if (name.Contains("maui") || name.Contains("alohakit") || name.Contains("mcp"))
        return "maui";
    string[] xamarin = ["xamarin", "forms", "xamanimation", "templateui", "xampane"];
    if (xamarin.Any(name.Contains) || string.CompareOrdinal(Text(repo, "pushed_at"), "2021") < 0)
        return "xamarin";
    return null;
}

static JsonObject Era(string id, string name, string years) => new() { ["id"] = id, ["name"] = name, ["years"] = years };

static JsonObject Link(string label, string url) => new() { ["label"] = label, ["url"] = url };

static JsonObject App(string name, string url) => new() { ["name"] = name, ["url"] = url };

static JsonObject Photo(string file, string caption) => new() { ["file"] = file, ["caption"] = caption };

static JsonObject Job(string company, string role, string period, params string[] points)
    => new() { ["company"] = company, ["role"] = role, ["period"] = period, ["points"] = Strings(points) };

static JsonArray Strings(params string[] items) => Array(items.Select(i => (JsonNode)i!).ToArray());

static string SavedMailto(string path)
{
    if (!File.Exists(path))
        return "mailto:";
    var links = JsonNode.Parse(File.ReadAllText(path))?["links"]?.AsArray();
    return links?.Select(l => Text(l!, "url")).FirstOrDefault(u => u.StartsWith("mailto:")) ?? "mailto:";
}

static JsonArray Array(params JsonNode[] items) => new(items);

static string ThisFile([CallerFilePath] string path = "") => path;
