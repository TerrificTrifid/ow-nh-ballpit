using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewHorizonsTemplate
{
    public class ShipBallpit : ModBehaviour
    {
        public INewHorizons NewHorizonsAPI;
        public List<List<GameObject>> Balls;

        private void Start()
        {
            NewHorizonsAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizonsAPI.LoadConfigs(this);
            NewHorizonsAPI.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);
        }

        private void OnStarSystemLoaded(string system)
        {
            if (system == "SolarSystem")
            {
                Balls = new List<List<GameObject>>();
                FindBallRigidbodies();
                var config = ModHelper.Config;
                UpdateBallpit(config.GetSettingsValue<string>("BallpitType"), config.GetSettingsValue<bool>("HatchCollider"), config.GetSettingsValue<bool>("CockpitCollider"), true);
            }
        }

        public override void Configure(IModConfig config)
        {
            if (SceneManager.GetActiveScene().name == "SolarSystem") UpdateBallpit(config.GetSettingsValue<string>("BallpitType"), config.GetSettingsValue<bool>("HatchCollider"), config.GetSettingsValue<bool>("CockpitCollider"));
        }

        private bool CheckPhysics()
        {
            return !Locator.GetShipTransform().Find("ShipSector/ShipBallpit/Balls/Cockpit/Spawn/Ball"); // path is only valid before physics added, so return true when it becomes null
        }

        private void UpdateBallpit(string typeSetting, bool hatchSetting, bool cockpitSetting, bool firstLoad = false)
        {
            ModHelper.Events.Unity.FireOnNextUpdate(() => {
                ModHelper.Events.Unity.RunWhen(CheckPhysics, () => {
                    var ship = Locator.GetShipTransform();

                    ship.Find("ShipSector/HatchCollider").gameObject.SetActive(hatchSetting);
                    ship.Find("ShipSector/CockpitCollider").gameObject.SetActive(cockpitSetting);

                    var switchedFrom = "";
                    var ballpits = ship.Find("ShipSector/ShipBallpit");
                    for (int i = 0; i < ballpits.childCount; i++)
                    {
                        var ballpit = ballpits.GetChild(i).gameObject;

                        ballpit.transform.Find("Cockpit").gameObject.SetActive(!cockpitSetting);

                        if (firstLoad)
                        {
                            if (!ballpit.name.Equals(typeSetting)) ballpit.SetActive(false);
                        }

                        else if (ballpit.name.Equals(typeSetting) && !ballpit.activeSelf)
                        {
                            ballpit.SetActive(true);
                            for (int j = 0; j < Balls[i].Count; j++)
                            {
                                var ballRB = Balls[i][j].GetComponent<OWRigidbody>();
                                ballRB.Unsuspend();
                            }
                        }
                        else if (!ballpit.name.Equals(typeSetting) && ballpit.activeSelf)
                        {
                            switchedFrom = ballpit.name;
                            for (int j = 0; j < Balls[i].Count; j++)
                            {
                                var ball = Balls[i][j];
                                var ballRB = ball.GetComponent<OWRigidbody>();
                                ballRB.Suspend();
                                ball.transform.localPosition = Vector3.zero;
                                ball.transform.localRotation = Quaternion.identity;
                            }

                            ballpit.SetActive(false);
                        }
                    }

                    if (switchedFrom != "") ModHelper.Console.WriteLine("Switched from " + switchedFrom + " to " + typeSetting);
                });
            });
        }

        private void FindBallRigidbodies()
        {
            ModHelper.Events.Unity.FireOnNextUpdate(() => {
                ModHelper.Events.Unity.RunWhen(CheckPhysics, () => {
                    var ballpits = Locator.GetShipTransform().Find("ShipSector/ShipBallpit");
                    for (int i = 0; i < ballpits.childCount; i++)
                    {
                        Balls.Add(new List<GameObject>());
                        var ballpit = ballpits.GetChild(i);
                        for (int j = 0; j < ballpit.childCount; j++)
                        {
                            var group = ballpit.GetChild(j);
                            for (int k = 0; k < group.childCount; k++)
                            {
                                var spawn = group.GetChild(k);
                                if (spawn.childCount != 0)
                                {
                                    var ball = spawn.GetChild(0).gameObject;
                                    Balls[i].Add(ball);
                                }
                            }
                        }
                    }
                });
            });
        }
    }
}
