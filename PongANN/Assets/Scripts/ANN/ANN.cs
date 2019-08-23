using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Main class, doing all of the work
/// </summary>
public class ANN
{
    /// <summary>
    /// Number of inputs coming into the neural network
    /// </summary>
	public int numInputs;

    /// <summary>
    /// Number of output of the neural network 
    /// (a neural network can have more than one output)
    /// </summary>
	public int numOutputs;

    /// <summary>
    /// Number of Hidden Layers <see cref="Layer"/>
    /// </summary>
    /// <remarks>
    /// The layers between input and output layer
    /// </remarks>
	public int numHidden;

    /// <summary>
    /// Number of Neurons Per Hidden Layer
    /// </summary>
	public int numNPerHidden;

    /// <summary>
    /// Value of how fast the neural network is going to learn,
    /// Determines how much any particular training sample that comes in
    /// is going to effect the overall network
    /// </summary>
	public double alpha;

    /// <summary>
    /// List keeping hold of all the layers we have created that contains the neurons
    /// </summary>
	List<Layer> layers = new List<Layer>();

    /// <summary>
    /// Constructor, make sure the value of alpha is from 0 to 1
    /// </summary>
    /// <param name="nInputs"></param>
    /// <param name="nOutputs"></param>
    /// <param name="nHidden"></param>
    /// <param name="nPerHidden"></param>
    /// <param name="_alpha"></param>
    public ANN(int nInputs, int nOutputs, int nHidden, int nPerHidden, double _alpha)
	{
        //Sets all the values
        numInputs = nInputs;
		numOutputs = nOutputs;
		numHidden = nHidden;
		numNPerHidden = nPerHidden;
		alpha = _alpha;

        //In this case the number of neurons in the hidden layer is going to be the same
        if (numHidden > 0)
		{
            //Make the input layer, that takes in the number of neurons and number of inputs
            layers.Add(new Layer(numNPerHidden, numInputs));

			for(int i = 0; i < numHidden-1; i++)
			{
                //Make all the Hidden layer , with number of neurons and
                //number of input that in this case is numNPerHidden
                layers.Add(new Layer(numNPerHidden, numNPerHidden));
			}

			layers.Add(new Layer(numOutputs, numNPerHidden));
		}
        else // a perceptron doesn't have any hidden layer and only one neuron
        {
			layers.Add(new Layer(numOutputs, numInputs));
		}
	}

    /// <summary>
    /// Every time you send an input, you train
    /// </summary>
    /// <param name="inputValues"></param>
    /// <param name="desiredOutput"></param>
    /// <returns></returns>
	public List<double> Train(List<double> inputValues, List<double> desiredOutput)
	{
		List<double> outputValues = new List<double>();
		outputValues = CalcOutput(inputValues, desiredOutput);
		UpdateWeights(outputValues, desiredOutput);
		return outputValues;
	}

    /// <summary>
    /// Gets the Result from the neural network
    /// </summary>
    /// <param name="_inputValues"> Input to the Neural Network </param>
    /// <param name="_desiredValues"> Their Desired Output </param>
    /// <returns>
    /// This will be the output from our neural network
    /// </returns>
    public List<double> CalcOutput(List<double> _inputValues, List<double> _desiredValues)
	{
		List<double> inputs = new List<double>();
		List<double> outputValues = new List<double>();
		int currentInput = 0;

		if(_inputValues.Count != numInputs)
		{
			Debug.Log("ERROR: Number of Inputs must be " + numInputs);
			return outputValues;
		}

        //Put the input values into the list of inputs of the neural network
        inputs = new List<double>(_inputValues);

        //Looping through all the hidden layers
        // input layer -> hidden Layers -> Output Layers
        for (int i = 0; i < numHidden + 1; i++)
		{
            //If not the input layer
            if (i > 0)
			{
				inputs = new List<double>(outputValues);
			}

            //clear the outputs for the current calculation
            outputValues.Clear();

            //Loop through the number of neurons of the current layer
            for (int j = 0; j < layers[i].numNeurons; j++)
			{
                //Holds the value of the dotproduct of the neuron's input and weights
                double N = 0;

                //Clear previous inputs for current calculation
                layers[i].neurons[j].inputs.Clear();

                //Loop through the input list of the neuron
                for (int k = 0; k < layers[i].neurons[j].numInputs; k++)
                {  
                    // Add the inputs of the previous layer into the list of inputs
                    layers[i].neurons[j].inputs.Add(inputs[currentInput]);

                    //Then add the weights * inputs
                    N += layers[i].neurons[j].weights[k] * inputs[currentInput];

					currentInput++;
				}

                //subtract the bias directly  instead of calculating the error and taking it out 
                N -= layers[i].neurons[j].bias;

                //Get the output for the neuron
                if (i == numHidden)
					layers[i].neurons[j].output = ActivationFunctionOutputLayer(N);
				else
					layers[i].neurons[j].output = ActivationFunction(N);


                //save it to the list of output to pass it to the next layer
                outputValues.Add(layers[i].neurons[j].output);
				currentInput = 0;
			}
		}
		return outputValues;
	}

	public string PrintWeights()
	{
		string weightStr = "";
		foreach(Layer l in layers)
		{
			foreach(Neuron n in l.neurons)
			{
				foreach(double w in n.weights)
				{
					weightStr += w + ",";
				}
			}
		}
		return weightStr;
	}

	public void LoadWeights(string weightStr)
	{
		if(weightStr == "") return;
		string[] weightValues = weightStr.Split(',');
		int w = 0;
		foreach(Layer l in layers)
		{
			foreach(Neuron n in l.neurons)
			{
				for(int i = 0; i < n.weights.Count; i++)
				{
					n.weights[i] = System.Convert.ToDouble(weightValues[w]);
					w++;
				}
			}
		}
	}
	
	void UpdateWeights(List<double> outputs, List<double> desiredOutput)
	{
		double error;

        //Go to first layer from last (Back Propagation)
        for (int i = numHidden; i >= 0; i--)
		{
            // go to first layer's first neuron
            for (int j = 0; j < layers[i].numNeurons; j++)
			{
                //if we are at the output layer
                if (i == numHidden)
				{
                    //Get the error
                    error = desiredOutput[j] - outputs[j];

                    //According to the delta rule
                    layers[i].neurons[j].errorGradient = outputs[j] * (1-outputs[j]) * error;
				}
				else
				{
                    //Calculate the errorGradient for the next set of layer
                    layers[i].neurons[j].errorGradient = layers[i].neurons[j].output * (1-layers[i].neurons[j].output);

                    //Also taking a errorGradientSum,
                    //which is going to be the errors in the above this particular layer
                    double errorGradSum = 0;

                    // Now loop through the layer ahead, add all the error gradient sum 
                    for (int p = 0; p < layers[i+1].numNeurons; p++)
					{
						errorGradSum += layers[i+1].neurons[p].errorGradient * layers[i+1].neurons[p].weights[j];
					}

                    //Add it to the current neuron
                    layers[i].neurons[j].errorGradient *= errorGradSum;
				}

                //Now loop through all the inputs of that particular neuron
                for (int k = 0; k < layers[i].neurons[j].numInputs; k++)
				{
                    //if last layer
                    if (i == numHidden)
					{
                        //set error
                        error = desiredOutput[j] - outputs[j];

                        //update the weights of that layer
                        layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * error;
					}
					else
					{
						layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * layers[i].neurons[j].errorGradient;
					}
				}

                //Update bias for evey neuron
                layers[i].neurons[j].bias += alpha * -1 * layers[i].neurons[j].errorGradient;
			}

		}

	}

    /// <summary>
    /// There can be different type of activation functions
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
	double ActivationFunction(double value)
	{
		return TanH(value);
	}

    /// <summary>
    /// Activation function for output layer
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
	double ActivationFunctionOutputLayer(double value)
	{
		return TanH(value);
	}

    /// <summary>
    /// It's a type of activation function called TanH function
    /// </summary>
    /// <param name="value"></param>
    /// <returns>
    /// Gets only -1 to 1
    /// </returns>
	double TanH(double value)
	{
		double k = (double) System.Math.Exp(-2*value);
    	return 2 / (1.0f + k) - 1;
	}

	double Sigmoid(double value) 
	{
    	double k = (double) System.Math.Exp(value);
    	return k / (1.0f + k);
	}
}
