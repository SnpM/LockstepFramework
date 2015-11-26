using UnityEngine;
using System.Collections;
using Lockstep;
public static class BuildManager {
    public static void Visualize () {
        BuildNode buildNode;
        if (InputManager.GetInformationDown ()) {

        }
        if (Interfacing.CachedDidHit)
        {
            if (Interfacing.MousedObject.tag == "BuildTile")
            {/*
                if (InputManager.GetInformationDown())
                {
                    CurrentBuildTile = Interfacing.MousedObject.GetComponent<BuildNode>();
                    if (CurrentBuildTile.CanBuild == false)
                    {
                        CurrentBuildTile = null;
                    } else
                    {
                        if (CurrentBuildTile.Agent.Controllable)
                        {
                            GUIManager.EnvironmentUI.BuildingMenu.Open(CurrentBuildTile);
                        }
                    }
                }*/
            }
        }
    }
}
