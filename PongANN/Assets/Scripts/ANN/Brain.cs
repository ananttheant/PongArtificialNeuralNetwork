using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    public GameObject paddle;
    public GameObject ball;

    //Balls rigidbody, to judge the position and dir and speed
    Rigidbody2D brb;

    //Y velocity, outputed from ANN
    float yvel;

    //boundations for paddle
    float paddleMinY = 8.8f;
    float paddleMaxY = 17.4f;
    float paddleMaxSpeed = 15;

    //To keep track of number of balls saved i.e we hit
    public float numSaved = 0;

    //to Keep track of number of balls we missed
    public float numMissed = 0;


    ///Inputs for the brain/ANN
    /// Ball X pos
    /// Ball Y pos
    /// Ball X vel
    /// Ball Y vel
    /// Paddle X pos
    /// paddle Y pos

    ///Outputs of the ANN
    /// Paddle Vel Y

    //neural network
    ANN ann;

    public bool isTraining = true;

    private void Start()
    {
        ///Why 4 neurons? 
        ///in this case 0 between numInputs
        ann = new ANN(6, 1, 1, 4, 0.11);
        brb = ball.GetComponent<Rigidbody2D>();
    }

    List<double> Run(double bx, double by, double bvx, double bvy, double px, double py, double pv, bool train)
    {
        List<double> inputs = new List<double>();
        List<double> outputs = new List<double>();

        //Populate the inputs
        inputs.Add(bx);
        inputs.Add(by);
        inputs.Add(bvx);
        inputs.Add(bvy);
        inputs.Add(px);
        inputs.Add(py);

        //We put pv in the output is cuz so that we can train it
        outputs.Add(pv);

        if (train)
            return (ann.Train(inputs, outputs));
        else
        {
            return ann.CalcOutput(inputs, outputs);
        }

    }

    private void Update()
    {
        // calculate pos y
        // position where we have to move to paddle to, based on the velocity but also clamps between the boundations 
        float posy = Mathf.Clamp(paddle.transform.position.y + (yvel * Time.deltaTime * paddleMaxSpeed), paddleMinY, paddleMaxY);

        //Update the position of the paddle from the posy, that was calculated in the last frame
        paddle.transform.position = new Vector3(paddle.transform.position.x, posy, paddle.transform.position.z);

        //Container for the output
        List<double> output = new List<double>();

        //picking out the backwall of the court
        //on the 9th physics layer that is the backwall
        int layerMask = 1 << 9;
        //Create a raycast, to tell where exactly the ball is going to hit
        RaycastHit2D hit = Physics2D.Raycast(ball.transform.position, brb.velocity, 1000, layerMask);

        if (hit.collider != null && hit.collider.gameObject.tag == "backwall")
        {
            //change in y,
            //if we hit anything that is backwall, giving us the error of where we should have been
            float dy = (hit.point.y - paddle.transform.position.y);

            output = Run(ball.transform.position.x,
                         ball.transform.position.y,
                         brb.velocity.x, brb.velocity.y,
                         paddle.transform.position.x,
                         paddle.transform.position.y,
                         dy, isTraining
                );

            yvel = (float)output[0];
        }
        else
        {
            yvel = 0;
        }

    }
}
