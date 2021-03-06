﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SC2API_CSharp;
using SC2APIProtocol;
using Action = SC2APIProtocol.Action;
using SharpCV;
using NumSharp;
using Tensorflow;


namespace LaMeBotNS

{
    class LaMeBot : Bot
    {
      //  public ResponseObservation Observation;
        public Race MyRace;
      //  public ResponseGameInfo GameInfo;
        List<Action> actions = new List<Action>();

        public void OnStart(ResponseGameInfo gameInfo, ResponseObservation observation, uint playerId)
        {

          //  Observation = observation;
           // MyRace = GameInfo.PlayerInfo[(int)Observation.Observation.PlayerCommon.PlayerId - 1].RaceActual;
            DebugUtil.WriteLine("MyRace: ");
        }
    
        public IEnumerable<SC2APIProtocol.Action> OnFrame(ResponseGameInfo gameInfo, ResponseData data, ResponseObservation observation, uint playerId)
        {
            //  List<SC2APIProtocol.Action> actions = new List<SC2APIProtocol.Action>();
            Controller.gameInfo = gameInfo;
            Controller.gameData = data;
            Controller.obs = observation;

            Controller.OpenFrame();

            if (Controller.frame == 0)
            {
                Logger.Info("LaMeBot");
                Logger.Info("--------------------------------------");
                Logger.Info("Map: {0}", Controller.gameInfo.MapName);
                Logger.Info("--------------------------------------");
            }

            if (Controller.frame == Controller.SecsToFrames(1))
                Controller.Chat("gl hf");

            var structures = Controller.GetUnits(Units.Structures);
            if (structures.Count == 1)
            {
                //last building                
                if (structures[0].integrity < 0.4) //being attacked or burning down                 
                    if (!Controller.chatLog.Contains("gg"))
                        Controller.Chat("gg");
            }

            var resourceCenters = Controller.GetUnits(Units.ResourceCenters);
            foreach (var rc in resourceCenters)
            {
                if (Controller.CanConstruct(Units.SCV))
                {
                    if (Controller.GetTotalCount(Units.SCV) < 18)
                    {
                        rc.Train(Units.SCV);
                    }
                }
                    
            }


            //keep on buildings depots if supply is tight
            if (Controller.maxSupply - Controller.currentSupply <= 5)
                if (Controller.CanConstruct(Units.SUPPLY_DEPOT))
                    if (Controller.GetPendingCount(Units.SUPPLY_DEPOT) == 0)
                        Controller.Construct(Units.SUPPLY_DEPOT);


            //distribute workers optimally every 10 frames
            if (Controller.frame % 10 == 0)
                Controller.DistributeWorkers();



            //build up to 4 barracks at once
            if (Controller.CanConstruct(Units.BARRACKS))
                if (Controller.GetTotalCount(Units.BARRACKS) < 6)
                    Controller.Construct(Units.BARRACKS);

            //train marine
            foreach (var barracks in Controller.GetUnits(Units.BARRACKS, onlyCompleted: true))
            {
                if (Controller.CanConstruct(Units.MARINE))
                    barracks.Train(Units.MARINE);
            }

            //attack when we have enough units
            var army = Controller.GetUnits(Units.ArmyUnits);
            if (army.Count > 30)
            {
                if (Controller.enemyLocations.Count > 0)
                    Controller.Attack(army, Controller.enemyLocations[0]);
            }

            return Controller.CloseFrame();
        }
        
        public void OnEnd(ResponseGameInfo gameInfo, ResponseObservation observation, uint playerId, Result result)
        { }

        public IEnumerable<SC2APIProtocol.Action> OnFrame(ResponseObservation observation)
        {
            return Controller.CloseFrame();
        }

        public void OnEnd(ResponseObservation observation, Result result)
        {
            //FileUtil.Register("Result: " + result);
            //if (Frame > 0)
            //    FileUtil.Register("Average ms per frame: " + totalExecutionTime / Frame + " Max ms per frame: " + maxExecution
        }


        



        public void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponseObservation observation, uint playerId, string opponentId)
        {


            Controller.gameInfo = gameInfo;
            Controller.gameData = data;
            Controller.obs = observation;
            
            DebugUtil.WriteLine("MyRace: ");
           
        }
    }
}
