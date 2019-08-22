using SkiaSharp;
using SkiaSharp.Views.Forms;
namespace CSharpMath.Forms {
  using Rendering;
  using SkiaSharp;
  public static class MathKeyboardExtensions {
    public static void BindDisplay(this MathKeyboard keyboard,
      SKCanvasView view, MathPainter settings, SKColor caretColor,
      CaretShape caretShape = CaretShape.IBeam, SKStrokeCap cap = SKStrokeCap.Butt) {
      view.EnableTouchEvents = true;
      view.Touch +=
        (sender, e) => {
          if (e.ActionType == SKTouchAction.Pressed)
            keyboard.Tap(new System.Drawing.PointF(e.Location.X, e.Location.Y));
        };
      keyboard.RedrawRequested += (_, __) => view.InvalidateSurface();
      view.PaintSurface +=
        (sender, e) => {
          var c = e.Surface.Canvas;
          c.Clear();
          MathPainter.DrawDisplay(settings, keyboard.Display, c);
          keyboard.DrawCaret(new SkiaCanvas(c, cap, false), caretColor.FromNative(), caretShape);
        };
    }
  }
}
