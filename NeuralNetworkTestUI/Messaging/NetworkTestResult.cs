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
        public string Name { get; set; }
        public double[] Values;
    }

    class NetworkTestResult
    {
        public readonly string StatisticName;
        public List<StatisticsRecord> Inputs;
        public List<StatisticsRecord> Values;

        public NetworkTestResult(string statisticName)
        {
            StatisticName = statisticName;
            Inputs = new List<StatisticsRecord>();
            Values = new List<StatisticsRecord>();
        }
    }
}
