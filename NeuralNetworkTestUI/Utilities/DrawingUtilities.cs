using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace NeuralNetworkTestUI.Utilities
{
    class DrawingUtilities
    {
        public static void DrawRect(Rectangle rect, Color color)
        {
            GL.Begin(BeginMode.Quads);
            GL.Color4(color.R, color.G, color.B, color.A);
            GL.Vertex3(rect.Left, rect.Top, 0);
            GL.Vertex3(rect.Right, rect.Top, 0);
            GL.Vertex3(rect.Right, rect.Bottom, 0);
            GL.Vertex3(rect.Left, rect.Bottom, 0);
            GL.End();
        }

        public static void DrawLine(PointF a, PointF b, Color color)
        {
            GL.Begin(BeginMode.Lines);
            GL.Color4(color.R, color.G, color.B, color.A);
            GL.Vertex3(a.X, a.Y, 0);
            GL.Vertex3(b.X, b.Y, 0);
            GL.End();
        }

        public static void DrawRect(Rectangle rect, Color color, PolygonMode mode)
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, mode);
            GL.Begin(BeginMode.Quads);
            GL.Color4(color.R, color.G, color.B, color.A);
            GL.Vertex3(rect.Left, rect.Top, 0);
            GL.Vertex3(rect.Right, rect.Top, 0);
            GL.Vertex3(rect.Right, rect.Bottom, 0);
            GL.Vertex3(rect.Left, rect.Bottom, 0);
            GL.End();
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
        }

        public static void DrawCircle(PointF position, float radius, Color color, PolygonMode mode)
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, mode);
            GL.Begin(BeginMode.Polygon);
            GL.Color4(color.R, color.G, color.B, color.A);
            for (var angle = 0f; angle < 2 * 3.141592f; angle+= 0.2f)
            {
                GL.Vertex3((float)Math.Sin(angle) * radius + position.X, (float)Math.Cos(angle) * radius + position.Y, 0);    
            }

            GL.End();
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
        }
    }
}
