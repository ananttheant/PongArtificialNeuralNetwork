using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A layer contains neurons
/// </summary>
public class Layer
{
    /// <summary>
    /// The number of neurons in the layer
    /// </summary>
    public int numNeurons;

    /// <summary>
    /// List of all the neurons of the layer
    /// </summary>
    public List<Neuron> neurons = new List<Neuron>();

	public Layer(int nNeurons, int numNeuronInputs)
	{
		numNeurons = nNeurons;

		for(int i = 0; i < nNeurons; i++)
		{
			neurons.Add(new Neuron(numNeuronInputs));
		}
	}
}
