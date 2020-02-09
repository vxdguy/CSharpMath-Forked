using System.Drawing;

namespace CSharpMath.Rendering {
  using Displays;
  using FrontEnd;
  public abstract class MathPainter<TCanvas, TColor> : Painter<TCanvas, MathSource, TColor> {
    public MathPainter(float fontSize = DefaultFontSize) : base(fontSize) { }
    protected Displays.IDisplay<Fonts, Glyph> _display;
    protected bool _displayChanged = true;
    public override Displays.IDisplay<Fonts, Glyph> Display => _display;
    public Atoms.MathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }
    protected override RectangleF? MeasureCore(float canvasWidth = float.NaN) =>
      _display?.DisplayBounds(CoordinatesFromBottomLeftInsteadOfTopLeft);
    public RectangleF? Measure {
      get {
        UpdateDisplay();
        return MeasureCore();
      }
    }
    protected override void SetRedisplay() => _displayChanged = true;
    protected void UpdateDisplay() {
      if (_displayChanged && MathList != null) {
        _display = TypesettingContext.Instance.CreateLine(MathList, Fonts, LineStyle);
        _displayChanged = false;
      }
    }
    public override void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var c = WrapCanvas(canvas);
      if (!Source.IsValid) DrawError(c);
      else {
        UpdateDisplay();
        DrawCore(c, _display, IPainterExtensions.GetDisplayPosition(_display.Width, _display.Ascent, _display.Descent, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, c.Width, c.Height, alignment, padding, offsetX, offsetY));
      }
    }
    public void Draw(TCanvas canvas, float x, float y) {
      var c = WrapCanvas(canvas);
      UpdateDisplay();
      DrawCore(c, _display, new PointF(x, CoordinatesFromBottomLeftInsteadOfTopLeft ? y : -y));
    }
    public void Draw(TCanvas canvas, PointF position) {
      var c = WrapCanvas(canvas);
      if (CoordinatesFromBottomLeftInsteadOfTopLeft) position.Y *= -1;
      UpdateDisplay();
      DrawCore(c, _display, position);
    }
  }
}