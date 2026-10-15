using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using Portfolio.Pixel;
using Portfolio.Shell;

namespace Portfolio.Apps;

/// <summary>
/// Code Studio: a pocket IDE. Pick a file in the Solution Explorer, edit its XAML, and the
/// designer on the right renders it as you type, using Avalonia's runtime XAML loader.
/// Mistakes land in the Error List; Build produces a build log of the usual kind.
/// </summary>
public class StudioApp : DockPanel
{
    private const string Ns = "xmlns=\"https://github.com/avaloniaui\"";

    private static readonly (string File, string Xaml)[] s_files =
    {
        ("Hello.axaml", $"""
            <StackPanel {Ns} Spacing="12" Margin="20">
              <TextBlock Text="Hello from XAML" FontSize="22" FontWeight="Bold" />
              <TextBlock Text="Edit me. The designer updates as you type." TextWrapping="Wrap" />
              <Button Content="A real button" />
              <Slider Minimum="0" Maximum="100" Value="40" />
            </StackPanel>
            """),
        ("Card.axaml", $"""
            <Border {Ns} Margin="20" Padding="18" CornerRadius="16"
                    Background="#7B4FD6" VerticalAlignment="Top">
              <StackPanel Spacing="6">
                <TextBlock Text="Javier Suárez" FontSize="20" FontWeight="Bold" Foreground="White" />
                <TextBlock Text="Software Engineer at Avalonia" Foreground="#E6DDFB" />
                <ProgressBar Value="72" Maximum="100" Margin="0,10,0,0" />
                <TextBlock Text="Coffee level" FontSize="11" Foreground="#E6DDFB" />
              </StackPanel>
            </Border>
            """),
        ("Form.axaml", $"""
            <StackPanel {Ns} Spacing="10" Margin="20">
              <TextBox PlaceholderText="Your name" />
              <ComboBox SelectedIndex="0" HorizontalAlignment="Stretch">
                <ComboBoxItem Content="Avalonia" />
                <ComboBoxItem Content=".NET MAUI" />
                <ComboBoxItem Content="Xamarin.Forms" />
              </ComboBox>
              <CheckBox Content="It works on my machine" IsChecked="True" />
              <ToggleSwitch OnContent="Dark mode (not really)" OffContent="Light mode" />
            </StackPanel>
            """),
        ("Shapes.axaml", $"""
            <Canvas {Ns} Width="240" Height="200">
              <Rectangle Canvas.Left="20" Canvas.Top="30" Width="90" Height="90"
                         RadiusX="14" RadiusY="14" Fill="#3498DB" />
              <Ellipse Canvas.Left="90" Canvas.Top="70" Width="100" Height="100"
                       Fill="#F2C230" Opacity="0.9" />
              <Path Canvas.Left="130" Canvas.Top="20" Fill="#E5484D"
                    Data="M 0,40 L 40,0 L 80,40 Z" />
            </Canvas>
            """),
        ("Spinner.axaml", $"""
            <Border {Ns} Width="90" Height="90" CornerRadius="18" Background="#1FB5A0">
              <Border.Styles>
                <Style Selector="Border">
                  <Style.Animations>
                    <Animation Duration="0:0:2" IterationCount="INFINITE">
                      <KeyFrame Cue="0%">
                        <Setter Property="RotateTransform.Angle" Value="0" />
                      </KeyFrame>
                      <KeyFrame Cue="100%">
                        <Setter Property="RotateTransform.Angle" Value="360" />
                      </KeyFrame>
                    </Animation>
                  </Style.Animations>
                </Style>
              </Border.Styles>
            </Border>
            """),
        ("Buggy.axaml", $"""
            <StackPanel {Ns} Margin="20">
              <TextBlock Text="Somebody forgot to close this tag">
              <Button Content="Fix me" />
            </StackPanel>
            """),
    };

    private readonly TextEditor _editor;
    private readonly ContentControl _designer = new();
    private readonly StackPanel _files = new();
    private readonly TextBlock _errors = new() { FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(9, 6) };
    private readonly PixelText _status = new() { Foreground = Palette.Brush(Palette.White), VerticalAlignment = VerticalAlignment.Center };
    private readonly DispatcherTimer _typing = new() { Interval = TimeSpan.FromMilliseconds(350) };
    private readonly DispatcherTimer _building = new() { Interval = TimeSpan.FromMilliseconds(260) };
    private readonly Queue<string> _log = new();
    private readonly Dictionary<string, string> _edited = new();
    private string _file = s_files[0].File;
    private bool _ok;

    public StudioApp()
    {
        Width = 800;
        Height = 500;

        // Menu and toolbar
        var menu = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 18, Margin = new Thickness(12, 9, 12, 3) };
        foreach (var item in new[] { "FILE", "EDIT", "VIEW", "BUILD", "DEBUG", "HELP" })
            menu.Children.Add(new PixelText { Text = item, Foreground = Palette.Brush(Palette.Ink) });
        SetDock(menu, Dock.Top);
        Children.Add(menu);

        var build = new PixelButton("BUILD", Palette.StartGreen);
        build.Click += (_, _) => Build();
        var reset = new PixelButton("RESET FILE", Palette.PaperDark, Palette.Ink);
        reset.Click += (_, _) => { _edited.Remove(_file); Open(_file); };
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(9, 6),
            Children = { build, reset },
        };
        SetDock(toolbar, Dock.Top);
        Children.Add(toolbar);

        // Status bar
        var statusBar = new Border
        {
            Padding = new Thickness(9, 6),
            Background = Palette.Brush(Palette.TitleBlue),
            Child = _status,
        };
        SetDock(statusBar, Dock.Bottom);
        Children.Add(statusBar);

        // Error List, Solution Explorer, Designer
        var errorList = Pane("ERROR LIST", new ScrollViewer { Content = _errors });
        var explorer = Pane("SOLUTION EXPLORER", new ScrollViewer { Content = _files });

        // The shell draws hard-edged for pixel art; a designer preview should be smooth.
        RenderOptions.SetEdgeMode(_designer, EdgeMode.Antialias);
        var designer = Pane("DESIGNER", new Border
        {
            Background = Brushes.White,
            ClipToBounds = true,
            Child = new ScrollViewer { Content = _designer },
        });

        // Editor
        _editor = new TextEditor
        {
            ShowLineNumbers = true,
            SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML"),
            FontSize = 12,
            Background = Brushes.White,
            Foreground = Palette.Brush(Palette.Ink),
            LineNumbersForeground = Palette.Brush(Palette.Bevel),
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Padding = new Thickness(6),
        };
        _editor.Options.ConvertTabsToSpaces = true;
        _editor.Options.IndentationSize = 2;
        // The editor draws its own text; keep it smooth, not hard-edged like the pixel chrome.
        RenderOptions.SetEdgeMode(_editor, EdgeMode.Antialias);
        _editor.TextChanged += (_, _) =>
        {
            _edited[_file] = _editor.Text ?? "";
            _typing.Stop();
            _typing.Start();
        };
        var editor = Pane("EDITOR", _editor);

        // Layout: every divider can be dragged
        //   explorer | editor | designer
        //   ----------------------------
        //            error list
        var work = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("168,3,*,3,290"),
            RowDefinitions = new RowDefinitions("*,3,96"),
        };
        work.ColumnDefinitions[0].MinWidth = 90;
        work.ColumnDefinitions[2].MinWidth = 120;
        work.ColumnDefinitions[4].MinWidth = 120;
        work.RowDefinitions[0].MinHeight = 90;
        work.RowDefinitions[2].MinHeight = 45;

        Place(work, explorer, row: 0, column: 0);
        Place(work, Divider(GridResizeDirection.Columns), row: 0, column: 1);
        Place(work, editor, row: 0, column: 2);
        Place(work, Divider(GridResizeDirection.Columns), row: 0, column: 3);
        Place(work, designer, row: 0, column: 4);
        Place(work, Divider(GridResizeDirection.Rows), row: 1, column: 0, columnSpan: 5);
        Place(work, errorList, row: 2, column: 0, columnSpan: 5);
        Children.Add(work);

        _typing.Tick += (_, _) => { _typing.Stop(); Render(); };
        _building.Tick += (_, _) => NextLogLine();

        Open(_file);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _typing.Stop();
        _building.Stop();
    }

    private static void Place(Grid grid, Control control, int row, int column, int columnSpan = 1)
    {
        Grid.SetRow(control, row);
        Grid.SetColumn(control, column);
        Grid.SetColumnSpan(control, columnSpan);
        grid.Children.Add(control);
    }

    /// <summary>
    /// The draggable line between two panes.
    /// </summary>
    private static GridSplitter Divider(GridResizeDirection direction) => new()
    {
        ResizeDirection = direction,
        Background = Palette.Brush(Palette.Bevel),
        Cursor = new Cursor(direction == GridResizeDirection.Columns ? StandardCursorType.SizeWestEast : StandardCursorType.SizeNorthSouth),
    };

    private static Border Pane(string title, Control content)
    {
        var header = new Border
        {
            Padding = new Thickness(9, 5),
            Background = Palette.Brush(Palette.PaperDark),
            Child = new PixelText { Text = title, Foreground = Palette.Brush(Palette.InkSoft) },
        };
        SetDock(header, Dock.Top);
        return new Border
        {
            ClipToBounds = true,
            Child = new DockPanel { Children = { header, content } },
        };
    }

    private void Open(string file)
    {
        _file = file;

        _files.Children.Clear();
        _files.Children.Add(new TextBlock
        {
            Text = "Solution 'Portfolio'",
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(9, 6, 9, 3),
            Foreground = Palette.Brush(Palette.Ink),
        });
        foreach (var (name, _) in s_files)
        {
            var selected = name == file;
            var row = new Border
            {
                Padding = new Thickness(21, 4, 9, 4),
                Background = selected ? Palette.Brush(Palette.TitleBlue) : Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand),
                Child = new TextBlock
                {
                    Text = name,
                    FontSize = 12,
                    Foreground = Palette.Brush(selected ? Palette.White : Palette.Ink),
                },
            };
            row.Tapped += (_, _) => Open(name);
            _files.Children.Add(row);
        }

        var original = Array.Find(s_files, f => f.File == file).Xaml;
        _editor.Text = _edited.TryGetValue(file, out var edited) ? edited : original;
        _typing.Stop();
        Render();
    }

    /// <summary>
    /// Parses the editor's XAML and shows it. On a mistake the last good render stays up.
    /// </summary>
    private void Render()
    {
        try
        {
            if (AvaloniaRuntimeXamlLoader.Load(_editor.Text ?? "") is not Control control)
                throw new InvalidOperationException("The root element must be a control.");

            _designer.Content = control;
            _ok = true;
            _errors.Foreground = Palette.Brush(Palette.InkSoft);
            _errors.Text = "0 errors. 0 warnings that anyone will read.";
            _status.Text = "READY";
        }
        catch (Exception ex)
        {
            _ok = false;
            _errors.Foreground = Palette.Brush(Palette.CloseRed);
            _errors.Text = $"{_file}: {FirstLine(ex)}";
            _status.Text = "1 ERROR";
        }
    }

    // In the browser .NET ships resource keys in place of its XML error texts, to save
    // download size. The common ones are put back into words here.
    private static readonly (string Key, string Text)[] s_xmlErrors =
    {
        ("Xml_TagMismatch", "A tag is not closed, or is closed in the wrong order."),
        ("Xml_InvalidRootData", "There is text outside the root element."),
        ("Xml_UnexpectedEOF", "The file ends before every tag is closed."),
        ("Xml_MissingRoot", "The file has no root element."),
        ("Xml_MultipleRoots", "Only one root element is allowed."),
        ("Xml_BadAttributeChar", "An attribute value contains a character that is not allowed."),
        ("Xml_ExpectingWhiteSpace", "A space is missing between two attributes."),
        ("Xml_BadStartNameChar", "A name starts with a character that is not allowed."),
        ("Xml_DupAttributeName", "The same attribute is set twice."),
    };

    private static string FirstLine(Exception ex)
    {
        var message = (ex.InnerException ?? ex).Message;
        var end = message.IndexOf('\n');
        if (end > 0)
            message = message[..end].Trim();

        if (!message.StartsWith("Xml_", StringComparison.Ordinal))
            return message;

        // "Xml_MessageWithErrorPosition, Xml_TagMismatchEx, 4, 3" ends with the line and column.
        var parts = message.Split(',', StringSplitOptions.TrimEntries);
        var text = "The XML is not well formed.";
        foreach (var (key, friendly) in s_xmlErrors)
        {
            if (message.Contains(key, StringComparison.Ordinal))
                text = friendly;
        }
        return parts.Length >= 3 && int.TryParse(parts[^2], out var line) && int.TryParse(parts[^1], out var column)
            ? $"{text} Line {line}, column {column}."
            : text;
    }

    // Build

    private void Build()
    {
        Render();
        _log.Clear();
        _log.Enqueue("BUILD STARTED...");
        _log.Enqueue("RESTORING PACKAGES...");
        _log.Enqueue("DELETING BIN AND OBJ, JUST IN CASE...");
        if (_ok)
        {
            _log.Enqueue("BUILD SUCCEEDED. 0 ERRORS, 47 WARNINGS");
            Achievements.Unlock("compiles");
        }
        else
        {
            _log.Enqueue("BUILD FAILED. SEE ERROR LIST");
        }
        _building.Start();
        NextLogLine();
    }

    private void NextLogLine()
    {
        if (_log.Count == 0)
        {
            _building.Stop();
            return;
        }
        _status.Text = _log.Dequeue();
    }
}
