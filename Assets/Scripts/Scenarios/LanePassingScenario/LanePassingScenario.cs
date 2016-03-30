﻿using UnityEngine;
using System.Collections;
using MonsterLove.StateMachine;
using UnityStandardAssets.Utility;
using System.Collections.Generic;

namespace VRAVE
{
    public class LanePassingScenario : StateBehaviour
    {

        [SerializeField]
        private GameObject UserCar;
        [SerializeField]
        private GameObject AIVehicle;
        [SerializeField]
        private WaypointCircuit initialTrack;

        private SpawnController manufacturer;
        private HUDController hudController;
        private HUDAudioController audioController;
        private Sensors userCarSensors;
        private CarAIControl userCarAI;
        private CarAIControl AIVehicleAI;
        private CarController userCarController;
        private CarController AIVehicleCarController;

        private LanePassingSensorResponseHandler lanePassingHandler;
        private FollowingSensorResponseHandler followHandler;

        private WaypointCircuit passingTrack;
        private bool userMode;
        private int circuitProgressNum = 0;


        public enum States
        {
            InitState,
            FollowingInstruction,
            Following,
            PassingInstruction,
            Passing,
            PassingWait,
            Mode_Switch
        }

        void Awake()
        {
            Initialize<States>();

            userCarController = UserCar.GetComponent<CarController>();
            userCarController.MaxSpeed = 20f;
            userCarAI = UserCar.GetComponent<CarAIControl>();
            userCarAI.enabled = false;
            UserCar.GetComponent<CarUserControl>().enabled = false;
            userCarSensors = UserCar.GetComponent<Sensors>();


            AIVehicleCarController = AIVehicle.GetComponent<CarController>();
            AIVehicleCarController.MaxSpeed = 10f;
            AIVehicleAI = AIVehicle.GetComponent<CarAIControl>();
            AIVehicleAI.enabled = false;
            (AIVehicle.GetComponent("Halo") as Behaviour).enabled = false;

            manufacturer = GetComponent<SpawnController>();
            hudController = UserCar.GetComponentInChildren<HUDController>();
            audioController = UserCar.GetComponent<HUDAudioController>();

            //passingTrack = Instantiate((WaypointCircuit)(Resources.Load(VRAVEStrings.PassingTrack)));
            switch (AIVehicleCarController.MaxSpeed.ToString())
            {
                case "5":
                    //string track = "PassingTrack" + userCarController.MaxSpeed.ToString();
                    passingTrack = (WaypointCircuit)GameObject.Find(VRAVEStrings.PassingTrack5).GetComponent<WaypointCircuit>();
                    break;
                case "10":
                    passingTrack = (WaypointCircuit)GameObject.Find(VRAVEStrings.PassingTrack10).GetComponent<WaypointCircuit>();
                    break;
                default:
                    passingTrack = (WaypointCircuit)GameObject.Find(VRAVEStrings.PassingTrack10).GetComponent<WaypointCircuit>();
                    break;

            }
            //passingTrack = (WaypointCircuit)GameObject.Find(VRAVEStrings.PassingTrack).GetComponent<WaypointCircuit>();
            //, UserCar.transform.position + UserCar.transform.forward * 2f, UserCar.transform.rotation);
            passingTrack.enabled = false;


            ChangeState(States.InitState);
        }

        // Extend abstract method "ChangeState(uint id)
        //
        // This is used for reacting to "OnTriggerEnter" events, called by WaypointTrigger scripts
        public override void TriggerCb(uint id)
        {
            switch (id)
            {
                case 0:
                    Debug.Log("Case 0!!!");
                    WaypointCircuit wc = GameObject.Find("LeftTurn_Circuit").GetComponent<WaypointCircuit>();
                    userCarAI.switchCircuit(wc, 0);
                    userCarAI.IsCircuit = false;
                    break;
                case 1:
                    Debug.Log("Case 1!!!");
                    if (true)
                    {
                        //ChangeState(States.StoppedAtIntersection);
                    }
                    else {
                        //ChangeState(States.WarnAndRestart);
                    }
                    break;
                case 3: //Beginning of passing track
                    Debug.Log("Inside Case 3");


                    break;
                case 4: //End of passing track
                    Debug.Log("Inside Case 4");
                    //userCarController.MaxSpeed = 30;
                    Debug.Log("Speeding up quickly!");
                    ChangeState(States.Passing);


                    break;
                case 5: //Speed up quickly
                    Debug.Log("Back to normal");
                    //userCarController.MaxSpeed = 20;
                    break;

            }
        }

        /* INIT_STATE */

        // In this state, introduction is given
        // HUD and Audio changes
        public void InitState_Enter()
        {
            userMode = false;
            Debug.Log("Enter: InitState");
            //UseCar and AI Vehicles Created
            //Display Scenario Information on HUD

            //Start when right paddle is pressed
            //ChangeState(States.FollowingInstruction);

        }

        // Wait for the user to press OK
        public void InitState_Update()
        {
            // 	Change to steering wheel paddle
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ChangeState(States.FollowingInstruction);
            }

        }

        /* FOLLOWING_INSTRUCTION */

        public void FollowingInstruction_Enter()
        {
            if(userCarAI.IsUser)
            {
                userCarAI.SetAltResponseHandlerEnable(true);
            }
            else  //This is just to make sure the same functionality is not being given to fully AI non-user cars.
            {
                userCarAI.SetAltResponseHandlerEnable(false);
            }

            Debug.Log("Enter: FollowingInstruction");

            //Play insructions here!!!

            if(userMode)
            {
                (UserCar.GetComponent<CarUserControl>() as CarUserControl).enabled = true;
            }
            else
            {
                //NEEDS TO BE FIXED FOR FOLLOWING HANDLING
            }
            
        }

        public void FollowingInstruction_Update()
        {
            if(userMode)
            {   
                //Once user begins driving, start AI
                if (userCarController.AccelInput >= 0.05f)  //Change to left trigger
                {
                    Debug.Log("Update: FollowingInstruction");
                    ChangeState(States.Following);
                }
            }
            else
            {
                // 	Change to steering wheel paddle
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    ChangeState(States.Following);
                }
            }

        }

        /* FOLLOWING */

        public void Following_Enter()
        {
            Debug.Log("Entered: Following");
            AIVehicleAI.Circuit = initialTrack;
            AIVehicleAI.enabled = true;
            AIVehicleAI.IsCircuit = true;
            (AIVehicle.GetComponent("Halo") as Behaviour).enabled = true;

            if (userMode)
            {
                (UserCar.GetComponent<CarUserControl>() as CarUserControl).enabled = true;
                userCarAI.enabled = false;
            }
            else
            {
                (UserCar.GetComponent<CarUserControl>() as CarUserControl).enabled = false;
                userCarAI.Circuit = initialTrack;
                userCarAI.enabled = true;
            }

        }

        public void Following_Update()
        {
            // 	Change to steering wheel paddle
            if (Input.GetKeyDown(KeyCode.Return))
            {
                userCarAI.SetAltResponseHandlerEnable(true);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                userCarAI.SetAltResponseHandlerEnable(false);
            }
            
        }

        /* PASSING INSTRUCTION */

        public void PassingInstruction_Enter()
        {
            Debug.Log("Entered: PassingInstruction");
            //GameObject[] go = GameObject.FindGameObjectsWithTag("Path");
            //WaypointCircuit wc = GameObject.Find("Figure8_North_3-22").GetComponent<WaypointCircuit>();
            AIVehicleAI.Circuit = initialTrack;
            (AIVehicle.GetComponent("Halo") as Behaviour).enabled = true;
            ChangeState(States.Passing);
        }

        public void PassingInstruction_Update()
        {
            //Move to Passing
            //if (Input.GetKey(KeyCode.Return))
            //{
            //    ChangeState(States.Passing);
            //}
        }

        /* PASSING */

        public void Passing_Enter()
        {
            Debug.Log("Entered: Passing");
            AIVehicleAI.enabled = true;
            AIVehicleAI.IsCircuit = true;


            if (userMode)
            {
                (UserCar.GetComponent<CarUserControl>() as CarUserControl).enabled = true;
                userCarAI.enabled = false;
            }
            else
            {
                (UserCar.GetComponent<CarUserControl>() as CarUserControl).enabled = false;
                circuitProgressNum = Mathf.CeilToInt((userCarAI.ProgressNum - 3) / 4) * 4 + 3; //Rounds the waypoints up to multiples of 3+4n
                //userCarAI.switchCircuit(initialTrack, circuitProgressNum);
                Debug.Log(circuitProgressNum);
                //Debug.Log(initialTrack.ToString());
                userCarAI.IsCircuit = true;
                userCarAI.enabled = true;
            }
        }


        public void Passing_Update()
        {
            Debug.Log("Update: Passing");

            if (Input.GetKey(KeyCode.Return))
            {
                userCarAI.SetSensorResponseHandlerEnable(true);
            }

            if (userCarAI.IsPassing)
            {
                Debug.Log("It's passing and I'm about to go to WAIT!");
                ChangeState(States.PassingWait);
            }
        }


        /* Passing Wait */
        /* Wait while passing other vehicle as to not attempt to pass again until finished. */
        public void PassingWait_Enter()
        {
            //Move passing track to 2 units ahead of the UserCar and set rotation to forward.
            passingTrack.transform.position = (UserCar.transform.position + UserCar.transform.forward * 2f);
            float newAngle = Mathf.RoundToInt(UserCar.transform.eulerAngles.y / 90f) * 90f;
            passingTrack.transform.eulerAngles = new Vector3(0, newAngle, 0);

            Debug.Log("Enter: PassingWait");
            (AIVehicle.GetComponent("Halo") as Behaviour).enabled = false;  //Turn off glow as you pass.
            userCarAI.IsCircuit = false;

            circuitProgressNum = userCarAI.ProgressNum;
            userCarAI.switchCircuit(passingTrack, 0);

            (UserCar.GetComponent<CarUserControl>() as CarUserControl).enabled = false;
            userCarController.MaxSpeed = 20f;
            userCarAI.CautiousMaxAngle = 75f;
            userCarAI.CautiousSpeedFactor = 0.6f;
            userCarAI.enabled = true;
            

        }

        public void PassingWait_Exit()
        {
            Debug.Log("Exit: PassingWait");
            userCarAI.IsPassing = false;
            userCarController.MaxSpeed = 20f;
            userCarAI.CautiousMaxAngle = 25f;
            userCarAI.CautiousSpeedFactor = 0.4f;
            float AISpeed = AIVehicleCarController.CurrentSpeed;
            //userCarController.SetSpeed = new Vector3(0,0,40f);


            if (userMode)
            {
                (UserCar.GetComponent<CarUserControl>() as CarUserControl).enabled = true;
                userCarAI.enabled = false;
            }
            else
            {
                (UserCar.GetComponent<CarUserControl>() as CarUserControl).enabled = false;
                circuitProgressNum = Mathf.CeilToInt((userCarAI.ProgressNum - 3) / 4) * 4 + 3; //Rounds the waypoints up to multiples of 3+4n
                userCarAI.switchCircuit(initialTrack, circuitProgressNum);
                Debug.Log(circuitProgressNum);
                userCarAI.IsCircuit = true;
                //userCarAI.enabled = true;
            }

        }
    }
}
