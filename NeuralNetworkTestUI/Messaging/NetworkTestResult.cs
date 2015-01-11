using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;

namespace NeuralNetworkTestUI.Messaging
{
    class StatisticsRecord
    {
        public double[] Inputs;
        public double[] Outputs;
        public double[] Values;
    }

    class NetworkTestResult
    {
        public readonly string StatisticName;
        public List<StatisticsRecord> Records;

        public NetworkTestResult(string statisticName)
        {
            StatisticName = statisticName;
        }
    }
}
