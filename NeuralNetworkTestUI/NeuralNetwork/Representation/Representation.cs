using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NeuralNetworkTestUI.Utilities;
using OpenTK.Graphics.OpenGL;
using ShNeuralNetwork;

namespace NeuralNetworkTestUI.NeuralNetwork.Representation
{
    public class Representation
    {
        public class NeuronRepresentation
        {
            public PointF Position;
            public readonly ShNeuralNetwork.Neuron Neuron;
            public Color BackGroundColor;
            public Color OutlineColor;

            public NeuronRepresentation(Neuron neuron)
            {
                Neuron = neuron;
                if (neuron is Input)
                {
                    BackGroundColor = Color.Salmon;
                    OutlineColor = Color.DarkOrange;
                }
                else
                {
                    BackGroundColor = Color.Gray;
                    OutlineColor = Color.DarkGray;
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
            public readonly ShNeuralNetwork.Link Link;

            public LinkRepresentation(Link link)
            {
                Link = link;
                Color = Color.DarkOrange;
            }

            public void Draw()
            {
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

        private ShNeuralNetwork.NeuralNetwork _network;
        private readonly List<NeuronRepresentation> _neurons;
        private readonly List<LinkRepresentation> _links;

        public Representation(ShNeuralNetwork.NeuralNetwork network)
        {
            _neurons = new List<NeuronRepresentation>();
            _links = new List<LinkRepresentation>();
            _network = network;

            var horizontalMargin = 200f;
            var verticalMargin = 50f;
            var horizontalOffset = 0f;
            var verticalOffset = 0f;

            var inputLayer = _network.Inputs;
            verticalOffset = -(inputLayer.Count * verticalMargin) / 2f;
            foreach (var input in inputLayer)
            {
                _neurons.Add(new NeuronRepresentation(input)
                {
                    Position = new PointF(horizontalOffset,verticalOffset)
                });
                verticalOffset += verticalMargin;
            }
            horizontalOffset += horizontalMargin;
            foreach (var layer in network.Layers)
            {
                verticalOffset = -(layer.Neurons.Count * verticalMargin) / 2f;
                foreach (var neuron in layer.Neurons)
                {
                    _neurons.Add(new NeuronRepresentation(neuron)
                    {
                        Position = new PointF(horizontalOffset, verticalOffset)
                    });
                    verticalOffset += verticalMargin;
                    foreach (var link in neuron.IncomingLinks)
                    {
                        _links.Add(new LinkRepresentation(link)
                        {
                            Start = _neurons.Find(n=> n.Neuron == link.StartNeuron),
                            End = _neurons.Last()
                        });    
                    }
                }
                horizontalOffset += horizontalMargin;
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
