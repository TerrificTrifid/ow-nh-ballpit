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
                var config = ModHelper.Config;
                UpdateBallpit(config.GetSettingsValue<string>("BallpitType"), config.GetSettingsValue<bool>("HatchCollider"), config.GetSettingsValue<bool>("CockpitCollider"));
            }
        }

        public override void Configure(IModConfig config)
        {
            if (SceneManager.GetActiveScene().name == "SolarSystem") UpdateBallpit(config.GetSettingsValue<string>("BallpitType"), config.GetSettingsValue<bool>("HatchCollider"), config.GetSettingsValue<bool>("CockpitCollider"));
        }

        private bool CheckPhysics()
        {
            return !Locator.GetShipTransform().Find("ShipSector/ShipBallpit/Ernestos/Cockpit/Ernesto"); // path is only valid before physics added, so return true when it becomes null
        }

        private void UpdateBallpit(String typeSetting, bool hatchSetting, bool cockpitSetting)
        {
            ModHelper.Events.Unity.FireOnNextUpdate(() => {
                ModHelper.Events.Unity.RunWhen(CheckPhysics, () => {
                    var ballpits = Locator.GetShipTransform().Find("ShipSector/ShipBallpit");
                    for (int i = 0; i < ballpits.childCount; i++)
                    {
                        var ballpit = ballpits.GetChild(i).gameObject;

                        // TODO: replace with better disabling process after improving hierarchy
                        ballpit.SetActive(ballpit.name.Equals(typeSetting));
                        ballpit.transform.Find("Cockpit").gameObject.SetActive(!cockpitSetting);
                    }
                });
            });
        }
    }
}
