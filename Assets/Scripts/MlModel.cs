using Ivankarez.NeuralNetworks;
using Ivankarez.NeuralNetworks.Api;
using UnityEngine;


public class MlModel
{
    private LayeredNetworkModel neuralNetwork;

    public MlModel()
    {
        var initializer = NN.Initializers.Uniform();
        neuralNetwork = NN.Models.Layered(32,
            NN.Layers.Dense(64, activation: NN.Activations.Relu(), biasInitializer: initializer, kernelInitializer: initializer),
            NN.Layers.Dense(16, activation: NN.Activations.Relu(), biasInitializer: initializer, kernelInitializer: initializer),
            NN.Layers.Dense(3, activation: NN.Activations.Linear(), biasInitializer: initializer, kernelInitializer: initializer));
    }

    public void SetParameters(float[] parameters)
    {
        neuralNetwork.SetParametersFlat(parameters);
    }

    public float[] GetParameters()
    {
        return neuralNetwork.GetParametersFlat();
    }

    public float[] Feedforward(float[] input)
    {
        var result = neuralNetwork.Feedforward(input);

        result[0] = Mathf.Clamp(result[0], -1f, 1f);
        result[1] = Mathf.Clamp01(result[1]);
        result[2] = Mathf.Clamp01(result[2]);

        return result;
    }
}
