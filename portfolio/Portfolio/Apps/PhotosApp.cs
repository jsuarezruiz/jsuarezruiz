using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Portfolio.Content;
using Portfolio.Pixel;

namespace Portfolio.Apps;

/// <summary>
/// Conference photos. Each one loads top to bottom, the way images arrived over dial-up.
/// Entries without a file in Assets/Photos get a drawn placeholder.
/// </summary>
public class PhotosApp : DockPanel
{
    private const double ViewW = 420;
    private const double ViewH = 280;

    private readonly Border _reveal = new() { ClipToBounds = true, VerticalAlignment = VerticalAlignment.Top, Height = 0 };
    private readonly PixelText _caption = new() { Foreground = Palette.Brush(Palette.Ink), HorizontalAlignment = HorizontalAlignment.Center };
    private readonly StackPanel _strip = new() { Spacing = 6, Margin = new Thickness(9) };
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(30) };

    public PhotosApp()
    {
        Width = 590;
        Height = 360;

        var left = new Border
        {
            Width = 132,
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(0, 0, 3, 0),
            Child = new ScrollViewer { Content = _strip },
        };
        SetDock(left, Dock.Left);
        Children.Add(left);

        var viewer = new StackPanel
        {
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Border
                {
                    Width = ViewW + 6,
                    Height = ViewH + 6,
                    BorderThickness = new Thickness(3),
                    BorderBrush = Palette.Brush(Palette.Ink),
                    Background = Palette.Brush(Palette.PaperDark),
                    Child = _reveal,
                },
                _caption,
            },
        };
        Children.Add(viewer);

        _timer.Tick += (_, _) =>
        {
            _reveal.Height = Math.Min(ViewH, _reveal.Height + 9);
            if (_reveal.Height >= ViewH)
                _timer.Stop();
        };

        var photos = PortfolioContent.Current.Photos;
        for (var i = 0; i < photos.Count; i++)
        {
            var index = i;
            var thumb = new Border
            {
                Width = 108,
                Height = 72,
                BorderThickness = new Thickness(3),
                BorderBrush = Palette.Brush(Palette.Bevel),
                ClipToBounds = true,
                Cursor = new Cursor(StandardCursorType.Hand),
                Child = Picture(photos[i], i, 108, 72),
            };
            thumb.Tapped += (_, _) => Show(index);
            _strip.Children.Add(thumb);
        }

        if (photos.Count > 0)
            Show(0);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    private void Show(int index)
    {
        var photo = PortfolioContent.Current.Photos[index];
        for (var i = 0; i < _strip.Children.Count; i++)
        {
            if (_strip.Children[i] is Border thumb)
                thumb.BorderBrush = Palette.Brush(i == index ? Palette.TitleBlue : Palette.Bevel);
        }

        _caption.Text = photo.Caption;
        _reveal.Child = new Border { Height = ViewH, VerticalAlignment = VerticalAlignment.Top, Child = Picture(photo, index, ViewW, ViewH) };
        _reveal.Height = 0;
        _timer.Start();
    }

    private static Control Picture(Photo photo, int index, double width, double height)
    {
        var uri = new Uri($"avares://Portfolio/Assets/Photos/{photo.File}");
        if (AssetLoader.Exists(uri))
        {
            using var stream = AssetLoader.Open(uri);
            return new Image { Source = new Bitmap(stream), Stretch = Stretch.UniformToFill, Width = width, Height = height };
        }
        return new PlaceholderPhoto(index) { Width = width, Height = height };
    }

    /// <summary>
    /// A drawn stand-in: a speaker on stage in front of a slide, seen from the audience.
    /// </summary>
    private sealed class PlaceholderPhoto : Control
    {
        private static readonly Color[] s_slides =
        {
            Color.Parse("#7B4FD6"), Color.Parse("#3498DB"), Color.Parse("#1FB5A0"), Color.Parse("#E5484D"),
        };

        private readonly int _index;

        public PlaceholderPhoto(int index) => _index = index;

        public override void Render(DrawingContext context)
        {
            var px = Math.Max(1, Math.Floor(Bounds.Width / 70));
            var cols = Bounds.Width / px;
            var rows = Bounds.Height / px;

            context.FillRectangle(Palette.Brush(Color.Parse("#1A1830")), new Rect(Bounds.Size));

            // The slide on the wall, with a few lines of "text".
            var slide = new Rect(cols * 0.2 * px, rows * 0.1 * px, cols * 0.6 * px, rows * 0.45 * px);
            context.FillRectangle(Palette.Brush(s_slides[_index % s_slides.Length]), slide);
            for (var i = 0; i < 3; i++)
            {
                context.FillRectangle(Palette.Brush(Palette.White),
                    new Rect(slide.X + 4 * px, slide.Y + (4 + i * 4) * px, slide.Width * (i == 0 ? 0.5 : 0.75), px));
            }

            // Stage, speaker and the heads of the front rows.
            var stage = Math.Floor(rows * 0.68) * px;
            context.FillRectangle(Palette.Brush(Color.Parse("#5A3A24")), new Rect(0, stage, Bounds.Width, 3 * px));
            Art.Draw(context, Art.Player, Math.Floor(cols * 0.14) * px, stage - 10 * px, px);

            var heads = Palette.Brush(Color.Parse("#0C0B18"));
            for (var x = 2.0; x < cols; x += 7)
            {
                var jitter = ((int)x * 7 + _index * 3) % 3;
                context.FillRectangle(heads, new Rect(x * px, (rows - 7 - jitter) * px, 5 * px, (7 + jitter) * px));
            }
        }
    }
}
