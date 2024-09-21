using Ivankarez.NeuralNetworks;
using Ivankarez.NeuralNetworks.Api;

namespace Ivankarez.DriveAI
{
    public static class DnaUtils
    {
        public const int InputSize = 11 + 4;
        public const int OutputSize = 2;

        public static float[] CreateNewDna()
        {
            return CreateNewModel().GetParametersFlat();
        }

        internal static LayeredNetworkModel CreateNeuralNetwork(float[] dna)
        {
            var model = CreateNewModel();
            model.SetParametersFlat(dna);
            return model;
        }

        private static LayeredNetworkModel CreateNewModel()
        {
            return NN.Models.Layered(InputSize,
                NN.Layers.GRU(NN.Size.Of(128)),
                NN.Layers.Dropout(0.2f),
                NN.Layers.GRU(NN.Size.Of(64)),
                NN.Layers.Dropout(0.2f),
                NN.Layers.Dense(64, activation: NN.Activations.Relu()),
                NN.Layers.Dense(32, activation: NN.Activations.Relu()),
                NN.Layers.Dense(OutputSize, activation: NN.Activations.Tanh()));
        }
    }
}
