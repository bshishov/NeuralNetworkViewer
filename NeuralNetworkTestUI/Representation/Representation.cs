using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NeuralNetworkTestUI.Utilities;
using OpenTK.Graphics.OpenGL;
using NeuralNetworkLibBase;

namespace NeuralNetworkTestUI.Representation
{
    public class Representation
    {
        public enum NeuronType
        {
            Input,
            Hidden,
            Output
        }

        public class NeuronRepresentation
        {
            public PointF Position;
            public readonly INode Neuron;
            public Color BackGroundColor;
            public Color OutlineColor;

            public NeuronRepresentation(INode neuron, NeuronType type)
            {
                Neuron = neuron;

                switch (type)
                {
                    case NeuronType.Input:
                        BackGroundColor = Color.Salmon;
                        OutlineColor = Color.DarkOrange;
                        break;
                    case NeuronType.Hidden:
                        BackGroundColor = Color.Gray;
                        OutlineColor = Color.DarkGray;
                        break;
                    case NeuronType.Output:
                        BackGroundColor = Color.Salmon;
                        OutlineColor = Color.DarkOrange;
                        break;
                }
            }

            public void Draw()
            {
                const float radius = 10f;
                DrawingUtilities.DrawCircle(Position, radius, BackGroundColor, PolygonMode.Fill);
                DrawingUtilities.DrawCircle(Position, radius, OutlineColor, PolygonMode.Line);
            }
        }

        public class LinkRepresentation
        {
            public NeuronRepresentation Start;
            public NeuronRepresentation End;
            public Color Color;
            public readonly IConnection Link;

            public LinkRepresentation(IConnection link)
            {
                Link = link;
                Color = Color.DarkOrange;
            }

            public void Draw()
            {
                if(Start != null)
                    DrawingUtilities.DrawLine(Start.Position, End.Position, LinearInterp(Link.Weight));
            }

            public Color LinearInterp(double value)
            {
                if (value < -1)
                    value = -1;
                else if (value > 1)
                    value = 1;
                return Color.FromArgb(
                    255,
                    (int)((value + 1) / 2 * 255), 
                    (int)((value + 1) / 2 * 255), 
                    (int)((-value + 1) / 2 * 255));
            }
        }

        private INeuralNetwork _network;
        private readonly List<NeuronRepresentation> _neurons;
        private readonly List<LinkRepresentation> _links;

        public Representation(INeuralNetwork network)
        {
            _neurons = new List<NeuronRepresentation>();
            _links = new List<LinkRepresentation>();
            _network = network;

            var horizontalMargin = 200f;
            var verticalMargin = 50f;
            var horizontalOffset = 0f;
            var verticalOffset = 0f;


            verticalOffset = -(_network.InputLayer.Nodes.Count() * verticalMargin) / 2f;
            foreach (var neuron in _network.InputLayer.Nodes)
            {
                _neurons.Add(new NeuronRepresentation(neuron, NeuronType.Input)
                {
                    Position = new PointF(horizontalOffset, verticalOffset)
                });
                verticalOffset += verticalMargin;
            }

            horizontalOffset += horizontalMargin;
            foreach (var layer in network.HiddenLayers)
            {
                verticalOffset = -(layer.Nodes.Count() * verticalMargin) / 2f;
                foreach (var neuron in layer.Nodes)
                {
                    _neurons.Add(new NeuronRepresentation(neuron, NeuronType.Hidden)
                    {
                        Position = new PointF(horizontalOffset, verticalOffset)
                    });
                    verticalOffset += verticalMargin;
                    if(neuron.Incoming == null)
                        continue;
                    foreach (var link in neuron.Incoming)
                    {
                        _links.Add(new LinkRepresentation(link)
                        {
                            Start = _neurons.Find(n=> n.Neuron == link.StartNode),
                            End = _neurons.Last()
                        });    
                    }
                }
                horizontalOffset += horizontalMargin;
            }

            verticalOffset = -(_network.OutputLayer.Nodes.Count() * verticalMargin) / 2f;
            foreach (var neuron in _network.OutputLayer.Nodes)
            {
                _neurons.Add(new NeuronRepresentation(neuron, NeuronType.Output)
                {
                    Position = new PointF(horizontalOffset, verticalOffset)
                });
                verticalOffset += verticalMargin;
                if (neuron.Incoming == null)
                    continue;
                foreach (var link in neuron.Incoming)
                {
                    _links.Add(new LinkRepresentation(link)
                    {
                        Start = _neurons.Find(n => n.Neuron == link.StartNode),
                        End = _neurons.Last()
                    });
                }
            }

        }

        public void Draw()
        {
            foreach (var linkRepresentation in _links)
            {
                linkRepresentation.Draw();
            }

            GL.LineWidth(2f);
            foreach (var neuronRepresentation in _neurons)
            {
                neuronRepresentation.Draw();
            }
            GL.LineWidth(1f);
        }
    }
}
