using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace AvaloniaApplication2
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

    public class Test : Control
    {
        class CustomDrawOp : ICustomDrawOperation
        {
            public CustomDrawOp(Rect bounds)
            {
                Bounds = bounds;

                _eglInterop = new EGLInterop();
            }

            public void Dispose()
            {
                // No-op
            }

            public Rect Bounds { get; }
            public bool HitTest(Point p) => false;
            public bool Equals(ICustomDrawOperation other) => false;
            private EGLInterop _eglInterop;

            public void Render(IDrawingContextImpl context)
            {
                var eglSurface = _eglInterop.CreateEglSurface();
                var grContext = (context as ISkiaDrawingContextImpl).GrContext;

                var desc = new GRBackendTextureDesc
                {
                    TextureHandle = eglSurface,
                    Config = GRPixelConfig.Rgba8888,
                    Height = 600,
                    Width = 800,
                    Origin = GRSurfaceOrigin.TopLeft
                };

                var target =
                    new GRBackendRenderTarget(800, 800, 0, 0,
                        new GRGlFramebufferInfo((uint)fb, GRPixelConfig.Rgba8888.ToGlSizedFormat()));
                
                using (var surface = SKSurface.Create(grContext, desc))
                {
                    var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;

                    canvas.DrawSurface(surface, new SKPoint(10, 10));
                }
            }
        }


        public override void Render(DrawingContext context)
        {
            var noSkia = new FormattedText()
            {
                Text = "Current rendering API is not Skia"
            };
            context.Custom(new CustomDrawOp(new Rect(10, 10, Bounds.Width, Bounds.Height)));
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
}