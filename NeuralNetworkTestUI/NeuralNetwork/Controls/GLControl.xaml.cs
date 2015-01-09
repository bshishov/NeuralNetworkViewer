using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using NeuralNetworkTestUI.Utilities;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Point = System.Drawing.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace NeuralNetworkTestUI.NeuralNetwork.Controls
{
    /// <summary>
    /// Interaction logic for NeuralNetworkView.xaml
    /// </summary>
    public partial class GLControl: UserControl
    {
        public static readonly DependencyProperty NetworkProperty = DependencyProperty.Register("Network", typeof(ShNeuralNetwork.NeuralNetwork), typeof(GLControl));

        private NeuralNetwork.Representation.Representation _representation;
        public ShNeuralNetwork.NeuralNetwork Network
        {
            get { return (ShNeuralNetwork.NeuralNetwork)GetValue(NetworkProperty); }
            set { SetValue(NetworkProperty, value); }
        }


        private OpenTK.GLControl _glControl;
        private Camera _camera;
        private bool _panning;
        private Point _lastPanningPos;
        private bool _dragging;
        private bool _scaling;

        private void SetupViewport()
        {
            if (_glControl == null)
                return;
            GL.MatrixMode(MatrixMode.Projection);
            GL.Viewport(0, 0, (int)_glControl.Width, (int)_glControl.Height);
        }

        public GLControl()
        {
            InitializeComponent();
        }

        private void WinFormsHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_glControl == null)
                return;
            
            if (!_glControl.Context.IsCurrent)
            {
                _glControl.MakeCurrent();
                SetupViewport();
                _glControl.Context.MakeCurrent(null);
            }
            else
            {
                SetupViewport();
                _glControl.Invalidate();
            }
        }

        private void WinFormsHost_Loaded(object sender, RoutedEventArgs e)
        {
            var prop = DependencyPropertyDescriptor.FromProperty(NetworkProperty, typeof(GLControl));
            prop.AddValueChanged(this, delegate
            {
                _representation = new Representation.Representation(Network);
            }); 
            
            if (_glControl == null)
            {
                _glControl = new OpenTK.GLControl();
                _camera = new Camera();
            }
            if (WindowsFormsHost.Child == null && _glControl != null)
            {
                WindowsFormsHost.Child = _glControl;
                _glControl.Paint += GlPaint;
                _glControl.MouseUp += GlControlMouseUp;
                _glControl.MouseDown += GlControlMouseDown;
                _glControl.MouseMove += GlControlMouseMove;
                _glControl.MouseWheel += OnMouseWheelHandler;
                _glControl.MouseDoubleClick += GlControlDoubleClick;

                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.Normalize);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.ClearColor(Color.FromArgb(40,40,40));
            }
        }

        private void GlControlDoubleClick(object sender, MouseEventArgs e)
        {
            //if (DoubleClick != null)
             //   DoubleClick.Invoke(this, new ClickEventArgs(ToWorld(e.X, e.Y)));
        }

        private void OnMouseWheelHandler(object sender, MouseEventArgs e)
        {
            if (!_glControl.Bounds.Contains(e.X, e.Y))
                return;

            var w = _glControl.Width/2f;
            var h = _glControl.Height/2f;
            var lastZoom = _camera.Zoom;
            _camera.Zoom *= (float) Math.Pow(_camera.ZoomRatio, Math.Sign(e.Delta)); // определяет уменьшать или увеличивать масштаб
            _camera.Zoom = Math.Max(_camera.MinZoom, _camera.Zoom);
            _camera.Zoom = Math.Min(_camera.MaxZoom, _camera.Zoom);

            if (Math.Abs(lastZoom - _camera.Zoom) < 0.001f)
                return;

            var mx = _camera.X + (e.X - w)/_camera.Zoom;
            var my = _camera.Y + (h - (2*h - e.Y))/_camera.Zoom;

            if (e.Delta > 0) // приближение
            {
                _camera.X += (mx - _camera.X)*(_camera.ZoomRatio - 1);
                _camera.Y += (my - _camera.Y)*(_camera.ZoomRatio - 1);
            }
            else // отдаление
            {
                _camera.X -= (mx - _camera.X)*(_camera.ZoomRatio - 1)/_camera.ZoomRatio;
                _camera.Y -= (my - _camera.Y)*(_camera.ZoomRatio - 1)/_camera.ZoomRatio;
            }
            
            GlPaint(this, null);
        }

        private void GlControlMouseMove(object sender, MouseEventArgs e)
        {
            if (_panning)
            {
                _camera.X -= (e.X - _lastPanningPos.X) / _camera.Zoom;
                _camera.Y -= (e.Y - _lastPanningPos.Y) / _camera.Zoom;

                _lastPanningPos = e.Location;
            }
            GlPaint(this, null);
        }

        private void GlControlMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _glControl.Focus();
                _panning = true;
                _lastPanningPos = e.Location;
            }
        }

        private void GlControlMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
                _panning = false;

            if (e.Button == MouseButtons.Left && (_dragging || _scaling))
            {
                _dragging = false;
                _scaling = false;
                return;
            }
        }

        private void GlPaint(object sender, PaintEventArgs e)
        {
            _glControl.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);

            GL.LoadIdentity();

            var w = _glControl.Width / 2f;
            var h = _glControl.Height / 2f;
            var left = _camera.X - w / _camera.Zoom;
            var right = _camera.X + w / _camera.Zoom;
            var top = _camera.Y - h / _camera.Zoom;
            var bottom = _camera.Y + h / _camera.Zoom;

            GL.Ortho(left, right, bottom, top, -1, 1);
            DrawGrid(left, right, bottom, top);
            DrawNetwork();

            GL.Flush();
            GL.Finish();
            _glControl.SwapBuffers();
        }

        private void DrawNetwork()
        {
            if(Network == null)
                return;
            if(_representation == null)
               _representation = new Representation.Representation(Network);
            _representation.Draw();
        }

        private void DrawGrid(float left, float right, float bottom, float top)
        {
            var gridColor = Color.FromArgb(50,50,50);
            int gridSize = 50;
            var l = (int)(left / gridSize);
            var r = (int)(right / gridSize + 1);
            var t = (int)(top / gridSize);
            var b = (int)(bottom / gridSize + 1);
            var axisColor = gridColor;
            var lineColor = Color.FromArgb(
                gridColor.A, 
                gridColor.R + 10,
                gridColor.G + 10,
                gridColor.B + 10);

            for (var i = l; i < r; i++)
                DrawingUtilities.DrawLine(new PointF(i * gridSize, top), new PointF(i * gridSize, bottom), i == 0 ? axisColor : lineColor);

            for (var i = t; i < b; i++)
                DrawingUtilities.DrawLine(new PointF(left, i * gridSize), new PointF(right, i * gridSize), i == 0 ? axisColor : lineColor);
        }
        
    }
}
