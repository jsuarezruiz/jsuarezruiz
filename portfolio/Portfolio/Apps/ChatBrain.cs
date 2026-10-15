using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Portfolio.Content;

namespace Portfolio.Apps;

/// <summary>
/// What the assistant says back, and optionally an app the visitor can open from it.
/// </summary>
public sealed record ChatReply(string Text, string? ActionLabel = null, string? AppId = null, string? Url = null);

/// <summary>
/// The assistant's "model": no language model at all, only topics matched by keyword.
/// Facts come from content.json, so they stay current; the jokes are written by hand.
/// </summary>
public sealed class ChatBrain
{
    private sealed record Topic(string[] Keys, Func<ChatReply> English, Func<ChatReply>? Spanish = null);

    private static readonly string[] s_spanishHints =
    {
        "hola", "quien", "que", "donde", "como", "cual", "cuales", "trabaja", "proyectos", "gracias", "puedo",
        "tiene", "sobre", "cuentame", "perro", "chiste", "eres", "vive", "hace", "sus", "contratar", "experiencia",
    };

    private readonly PortfolioContent _content = PortfolioContent.Current;
    private readonly List<Topic> _topics;
    private readonly string[] _fallbacks;
    private readonly string[] _fallbacksEs;
    private readonly string[] _jokes;
    private int _fallback;
    private int _joke;

    public ChatBrain()
    {
        var p = _content.Profile;
        var r = _content.Resume;
        var now = r.Experience.FirstOrDefault();
        var top = _content.Projects.OrderByDescending(x => x.Stars).Take(3).ToList();

        _jokes = new[]
        {
            "Why do UI engineers hate nature? Too many trees to measure and arrange.",
            "There are 2 hard problems in computer science: naming things, caching, and off-by-one errors.",
            "A layout pass walks into a bar. Then walks in again. Then again. That's a layout cycle.",
            "It works on my machine. So we shipped my machine.",
        };

        _fallbacks = new[]
        {
            "I only know about one subject, and he sits at this desk. Try: projects, experience, awards, or contact.",
            "That's beyond my 40 if-statements. Ask me about Javier's work, his libraries, or the dog.",
            "No idea. A real AI would make something up here. I'd rather not. Try \"what has he built?\"",
        };

        _fallbacksEs = new[]
        {
            "Solo sé de un tema, y está sentado en este escritorio. Prueba: proyectos, experiencia, premios o contacto.",
            "Eso se escapa de mis 40 if. Pregúntame por el trabajo de Javier, sus librerías o el perro.",
        };

        _topics = new List<Topic>
        {
            new(new[] { "hello", "hi", "hey", "hola", "buenas" },
                () => new($"Hi. I'm AskGPT. I answer questions about {p.Name}. Ask who he is, what he built, or whether you can hire him."),
                () => new($"Hola. Soy AskGPT. Respondo preguntas sobre {p.Name}. Pregunta quién es, qué ha hecho o si se le puede contratar.")),

            new(new[] { "who", "about", "javier", "himself", "quien", "sobre" },
                () => new($"{p.Name} is a {r.Title.ToLowerInvariant()} in {p.Location}. {p.Bio}"),
                () => new($"{p.Name} es ingeniero de software en {p.Location}. Trabaja en Avalonia UI y antes estuvo en Microsoft con Xamarin.Forms y .NET MAUI.")),

            new(new[] { "work", "works", "job", "company", "employer", "now", "current", "trabaja", "trabajo", "empresa" },
                () => new(now is null ? "He builds cross-platform UI." : $"{now.Role} at {now.Company}. {now.Points.FirstOrDefault()}"),
                () => new("Trabaja en Avalonia UI como ingeniero de software, creando interfaces multiplataforma para .NET.")),

            new(new[] { "experience", "career", "history", "before", "past", "worked", "experiencia", "carrera", "antes" },
                () => new("In order: " + string.Join(", then ", r.Experience.AsEnumerable().Reverse().Select(j => j.Company)) + ". The full story is in his resume.", "OPEN RESUME", "resume"),
                () => new("Por orden: " + string.Join(", luego ", r.Experience.AsEnumerable().Reverse().Select(j => j.Company)) + ". Todo está en su CV.", "ABRIR CV", "resume")),

            new(new[] { "microsoft" },
                () => new(Job("Microsoft"))),

            new(new[] { "plain", "concepts" },
                () => new(Job("Plain Concepts"))),

            new(new[] { "avalonia" },
                () => new("He works on Avalonia, the cross-platform .NET UI framework. This whole desktop is drawn with it, running as WebAssembly in your browser.")),

            new(new[] { "xamarin", "forms" },
                () => new("He spent years on Xamarin.Forms: gradients, shapes, RefreshView, and the first Linux backend with GtkSharp. His good-looking UI samples are still his most starred work.")),

            new(new[] { "maui" },
                () => new("On .NET MAUI he built core APIs: lifecycle events, shadows and a cross-platform drawing API. He also maintains awesome-dotnet-maui.")),

            new(new[] { "projects", "built", "build", "made", "repos", "repositories", "libraries", "open source", "proyectos", "hecho", "librerias" },
                () => new($"His most starred: {string.Join(", ", top.Select(x => $"{x.Name} ({x.Stars})"))}. There are {p.Repos} public repositories in total.", "OPEN PROJECTS", "projects"),
                () => new($"Los más populares: {string.Join(", ", top.Select(x => $"{x.Name} ({x.Stars})"))}. En total tiene {p.Repos} repositorios públicos.", "ABRIR PROYECTOS", "projects")),

            new(new[] { "stars", "followers", "stats", "numbers", "popular", "estrellas", "seguidores" },
                () => new($"{p.Stars} stars across {p.Repos} repositories, and {p.Followers} followers on GitHub. The numbers are live.", "OPEN GITHUB LIST", "browser")),

            new(new[] { "awards", "award", "mvp", "achievements", "premios", "logros" },
                () => new(string.Join(". ", r.Achievements) + "."),
                () => new("Microsoft MVP de 2014 a 2019, Xamarin MVP, Xamarin Certified Mobile Developer y Microsoft Active Professional 2014.")),

            new(new[] { "talks", "talk", "speaker", "speaking", "conference", "conferences", "charlas", "ponente" },
                () => new("He speaks at .NET conferences and meetups, and helps organise SevillaDotNet and MonkeyConf. Avalonia Conf Online is on December 2: check the wall calendar.")),

            new(new[] { "community", "sevilladotnet", "monkeyconf", "meetup", "comunidad" },
                () => new(string.Join(" ", r.Communities))),

            new(new[] { "education", "study", "studied", "degree", "school", "estudios", "estudio" },
                () => new(string.Join(" ", r.Education))),

            new(new[] { "skills", "stack", "tech", "technologies", "tools", "habilidades", "tecnologias" },
                () => new("Mostly " + string.Join(", ", r.Skills) + ". Speaks " + string.Join(" and ", r.Languages) + ".")),

            new(new[] { "live", "lives", "location", "based", "city", "country", "sevilla", "seville", "vive", "ciudad" },
                () => new($"He is based in {p.Location}, Spain."),
                () => new($"Vive en {p.Location}.")),

            new(new[] { "contact", "email", "mail", "reach", "message", "contacto", "correo", "escribir" },
                () => new("The quickest way is email. I can open the mail app for you.", "OPEN MAIL", "mail"),
                () => new("Lo más rápido es el correo. Te abro la aplicación.", "ABRIR CORREO", "mail")),

            new(new[] { "hire", "hiring", "available", "recruit", "freelance", "offer", "opportunity", "contratar", "disponible", "oferta" },
                () => new("He's happily employed at Avalonia. But he reads his email, and he likes a good conversation.", "OPEN MAIL", "mail"),
                () => new("Está muy contento en Avalonia. Pero lee el correo y le gusta una buena conversación.", "ABRIR CORREO", "mail")),

            new(new[] { "cv", "resume", "curriculum" },
                () => new("Here you go.", "OPEN RESUME", "resume"),
                () => new("Aquí lo tienes.", "ABRIR CV", "resume")),

            new(new[] { "game", "play", "fun", "bored", "juego", "jugar" },
                () => new("Bug Hunt. You catch bugs with a net. Some of them have names you'll recognise.", "PLAY BUG HUNT", "game"),
                () => new("Bug Hunt. Cazas bugs con una red. Algunos nombres te sonarán.", "JUGAR", "game")),

            new(new[] { "github" },
                () => new($"He's {p.Login} on GitHub.", "OPEN GITHUB LIST", "browser")),

            new(new[] { "dog", "bulldog", "pet", "perro" },
                () => new("Under the desk. Asleep. As always. Click him if you want to be ignored."),
                () => new("Debajo de la mesa. Dormido. Como siempre.")),

            new(new[] { "duck", "rubber", "pato" },
                () => new("The duck is the senior engineer here. Explain your bug to it, line by line.")),

            new(new[] { "coffee", "cafe" },
                () => new("Coffee is the real runtime. Everything else is just a framework on top.")),

            new(new[] { "ai", "llm", "gpt", "chatgpt", "real", "model", "robot", "bot", "claude", "inteligencia" },
                () => new("I'm not a real AI. I am 40 if-statements in a trench coat. Model: if-else-4o-mini. Context window: one sentence."),
                () => new("No soy una IA de verdad. Soy 40 if con gabardina. Modelo: if-else-4o-mini.")),

            new(new[] { "joke", "funny", "laugh", "chiste" },
                () => new(_jokes[_joke++ % _jokes.Length])),

            new(new[] { "site", "website", "portfolio", "this", "made with", "how", "built with" },
                () => new("This is a 3D room with a laptop and a phone in it. The desktop you're using, the phone's home screen and Bug Hunt are all Avalonia, running as WebAssembly.")),

            new(new[] { "flutter", "react", "swiftui", "compose", "electron" },
                () => new("Fine frameworks. He has even ported Flutter's Vignettes to Avalonia, to prove a point.")),

            new(new[] { "bug", "bugs", "error", "crash", "exception" },
                () => new("Bugs live in Bug Hunt, where they belong. If you want a real crash, there is an icon called DONT_CLICK.EXE.")),

            new(new[] { "age", "old", "born", "edad" },
                () => new("Old enough to have shipped Silverlight. Young enough to still be excited about a new layout panel.")),

            new(new[] { "meaning", "life", "42" },
                () => new("42. Also: measure once, arrange once.")),

            new(new[] { "thanks", "thank", "gracias" },
                () => new("Any time. That's what my if-statements are for."),
                () => new("De nada. Para eso están mis if.")),

            new(new[] { "bye", "goodbye", "adios" },
                () => new("Bye. Say hi to the dog on your way out."),
                () => new("Hasta luego. Saluda al perro al salir.")),

            new(new[] { "help", "what can you", "can you do", "commands", "ayuda" },
                () => new("Ask about Javier: who he is, where he works, what he built, his awards, how to contact him. I can also open apps: try \"show me the CV\" or \"play a game\".")),
        };
    }

    /// <summary>
    /// Questions offered as buttons, so nobody has to guess what it knows.
    /// </summary>
    public static readonly string[] Suggestions =
    {
        "Who is Javier?", "What has he built?", "Can I hire him?", "Are you a real AI?", "Tell me a joke",
    };

    public ChatReply Answer(string message)
    {
        var text = " " + Normalize(message) + " ";
        var spanish = s_spanishHints.Any(h => text.Contains(" " + h + " "));

        // A project mentioned by name wins over everything else.
        var project = _content.Projects
            .OrderByDescending(x => x.Name.Length)
            .FirstOrDefault(x => text.Contains(" " + Normalize(x.Name) + " ") || Aliases(x.Name).Any(a => text.Contains(" " + a + " ")));
        if (project is not null)
            return new($"{project.Name}: {project.Description} It has {project.Stars} stars.", "OPEN ON GITHUB", Url: project.Url);

        Topic? best = null;
        var bestScore = 0;
        foreach (var topic in _topics)
        {
            var score = topic.Keys.Where(k => text.Contains(" " + k + " ")).Sum(k => k.Length);
            if (score > bestScore)
            {
                best = topic;
                bestScore = score;
            }
        }

        if (best is null)
        {
            var lines = spanish ? _fallbacksEs : _fallbacks;
            return new(lines[_fallback++ % lines.Length]);
        }

        return spanish && best.Spanish is not null ? best.Spanish() : best.English();
    }

    private string Job(string company)
    {
        var job = _content.Resume.Experience.FirstOrDefault(j => j.Company == company);
        return job is null ? "He worked there." : $"{job.Role} at {job.Company}, {job.Period.ToLowerInvariant()}. {string.Join(" ", job.Points.Take(2))}";
    }

    // "Nova.Avalonia.UI" should also answer to "nova".
    private static IEnumerable<string> Aliases(string name)
    {
        var first = Normalize(name).Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (first is { Length: > 3 } && first is not ("awesome" or "avalonia" or "xamarin" or "netmaui" or "dotnet"))
            yield return first;
    }

    /// <summary>
    /// Lower case, no accents, no punctuation: "¿Quién es?" becomes "quien es".
    /// </summary>
    private static string Normalize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value.ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (char.IsLetterOrDigit(ch))
                builder.Append(ch);
            else if (char.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark)
                builder.Append(' ');
        }
        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
