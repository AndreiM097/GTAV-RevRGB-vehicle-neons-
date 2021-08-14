using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Windows.Forms;
using System.Threading;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Runtime.InteropServices;

namespace RevRGB
{
    public class Main : Script
    {
        static int driveMode = 1;
        bool sirenSoundOn;
        bool lockHornOn;
        bool vehicleLocked;
        bool windowsOpen;
        bool selfDrivingActive;
        bool giveMeABreak;
        Ped player;
        float rpmPercent;
        float colorValuefromRPM;
        float decreaseColor;
        float headlightPower;
        float enginePowerMult;




        int GameTimeReference = Game.GameTime + 200;
        public Main()
        {
            Tick += onTick;
            KeyDown += onKeyDown;
        }

        private void onTick(object sender, EventArgs e)
        {
            player = Game.Player.Character;
           
            vehicleDrivingMode();

            keylessEntry();
        
            openDoorDetector();

        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            // How to get the Windows.Gaming or all other Windows references: https://www.youtube.com/watch?v=3N0d6ZMNvOY&ab_channel=AndrewEberle


            //Repair the vehicle and clean player clothes
            if (e.KeyCode == Keys.PageDown)
            {
                try
                {
                    if (player.IsInVehicle())
                    {
                        player.CurrentVehicle.Repair();
                        if (windowsOpen)
                        {
                            player.CurrentVehicle.RollDownWindow(VehicleWindow.FrontLeftWindow);
                            player.CurrentVehicle.RollDownWindow(VehicleWindow.FrontRightWindow);
                        }
                    }
                    player.Sweat = 0;
                    player.ClearBloodDamage();

                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }

            }
            
            //The player drives by himself to the waypoint
            if (e.KeyCode == Keys.Delete)
            {
                try
                {
                    if (player.IsInVehicle() && selfDrivingActive == false)
                    {
                        //World.GetClosestPed(player.Position, 20).Task.AimAt(player.LastVehicle.Position, 5000) ;
                        selfDrivingActive = true;
                        if (GTA.Game.IsWaypointActive)
                        {
                            player.Task.DriveTo(player.LastVehicle, GTA.World.GetWaypointPosition(), 1000, 130, 3);



                            UI.ShowHelpMessage("Self driving active: " + selfDrivingActive, true);
                        }
                    }else{
                        player.Task.DriveTo(player.LastVehicle, player.LastVehicle.Position, 1000, 50, 1);
                        
                        selfDrivingActive = false;
                        UI.ShowHelpMessage("Self driving disabled.", true);
                    }

                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }

            }

            //Turns on cars in the area and modifies player's current car
            if (e.KeyCode == Keys.Insert)
            {
                try
                {
                    
                    Vehicle[] vehicles = World.GetNearbyVehicles(player, 30);
                    UI.Notify("Vehicles: " + vehicles.Length);
                    
                    for (int i = 0; i < vehicles.Length; i++)
                    {
                        

                        if (vehicles[i].EngineRunning && vehicles[i].IsSeatFree(VehicleSeat.Driver) == true)
                        {
                            vehicles[i].EngineRunning = false;
                            vehicles[i].LockStatus = VehicleLockStatus.Locked;
                            vehicles[i].HandbrakeOn = true;
                            vehicles[i].InteriorLightOn = false;
                            vehicles[i].IsSeatFree(VehicleSeat.Driver);
                        }
                        else if ((!vehicles[i].EngineRunning  && vehicles[i].IsSeatFree(VehicleSeat.Driver) == true))
                        {
                            vehicles[i].LockStatus = VehicleLockStatus.Locked;
                            vehicles[i].EngineRunning = true;
                            vehicles[i].InteriorLightOn = true;
                            vehicles[i].LightsOn = true;
                            vehicles[i].HandbrakeOn = false;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("From modding vehicle: " + exception);
                    UI.ShowSubtitle("From Modding vehicle: " + exception, 3000);
                }
            }

            //Locks & unlocks player vehicle
            if (e.KeyCode == Keys.L)
            {
                vehicleLockingSystem();
            }

            //Rolls down the vehicle's windows and if the weather is too bad,refuses to roll down.
            if (e.KeyCode == Keys.End)
            {
                vehicleWindowSystem();
            }

            //Enables or disables police siren or lock horn (when not in a police vehicle)
            if (e.KeyCode == Keys.J)
            {
                try
                {
                    if (player.CurrentVehicle.HasSiren || player.LastVehicle.HasSiren)
                    {
                        if (sirenSoundOn)
                        {
                            sirenSoundOn = false;
                            if (player.IsInVehicle())
                            {
                                player.CurrentVehicle.IsSirenSilent = true;
                            }
                            else
                            {
                                player.LastVehicle.IsSirenSilent = true;
                            }

                            UI.ShowHelpMessage("Siren disabled");

                        }
                        else
                        {
                            sirenSoundOn = true;
                            player.CurrentVehicle.IsSirenSilent = false;
                            UI.ShowHelpMessage("Siren enabled");
                        }
                    }
                    else
                    {
                        if (lockHornOn)
                        {
                            lockHornOn = false;
                            UI.ShowHelpMessage("Lock horn disabled");
                        }
                        else
                        {
                            lockHornOn = true;
                            UI.ShowHelpMessage("Lock horn enabled");
                        }
                    }
                }
                catch (Exception exception)
                {
                    UI.ShowSubtitle("Siren: " + exception, 5000);
                }
            }

            //Selects between different driving modes
            if (e.KeyCode == Keys.N)
            {

                if (driveMode == 2)
                {

                    if (player.IsInVehicle())
                    {

                        player.CurrentVehicle.SetNeonLightsOn(VehicleNeonLight.Left, true);
                        player.CurrentVehicle.SetNeonLightsOn(VehicleNeonLight.Right, true);
                        player.CurrentVehicle.SetNeonLightsOn(VehicleNeonLight.Back, true);
                        player.CurrentVehicle.SetNeonLightsOn(VehicleNeonLight.Front, true);

                    }
                    else
                    {
                        player.LastVehicle.SetNeonLightsOn(VehicleNeonLight.Left, true);
                        player.LastVehicle.SetNeonLightsOn(VehicleNeonLight.Right, true);
                        player.LastVehicle.SetNeonLightsOn(VehicleNeonLight.Back, true);
                        player.LastVehicle.SetNeonLightsOn(VehicleNeonLight.Front, true);


                    }

                    UI.ShowHelpMessage("Drive Mode: " + driveMode + " Corsa", 2000);
                }

                if (driveMode >= 3)
                {
                    try
                    {
                        driveMode = 1;

                        if (player.IsInVehicle())
                        {
                            player.CurrentVehicle.SetNeonLightsOn(VehicleNeonLight.Left, false);
                            player.CurrentVehicle.SetNeonLightsOn(VehicleNeonLight.Right, false);
                            player.CurrentVehicle.SetNeonLightsOn(VehicleNeonLight.Back, false);
                            player.CurrentVehicle.SetNeonLightsOn(VehicleNeonLight.Front, false);

                        }
                        else
                        {
                            player.LastVehicle.SetNeonLightsOn(VehicleNeonLight.Left, false);
                            player.LastVehicle.SetNeonLightsOn(VehicleNeonLight.Right, false);
                            player.LastVehicle.SetNeonLightsOn(VehicleNeonLight.Back, false);
                            player.LastVehicle.SetNeonLightsOn(VehicleNeonLight.Front, false);
                        }

                        UI.ShowHelpMessage("Drive Mode: " + driveMode + " Street ", 2000);

                    }
                    catch (Exception exception)
                    {
                        UI.ShowSubtitle("Exception: " + exception);
                    }
                }
                else
                {
                    driveMode++;
                }
            }

        }

        public void vehicleDrivingMode()
        {

            if (player.IsInVehicle())
            {
                try
                {

                    rpmPercent = player.CurrentVehicle.CurrentRPM / 1 * 100;
                    colorValuefromRPM = rpmPercent / 100 * 255;
                    decreaseColor = rpmPercent / 100 * 255;
                    headlightPower = rpmPercent / 100 * 2;
                    enginePowerMult = rpmPercent / 100 * 4;

                    if (player.CurrentVehicle.Speed <= 0)
                    {
                        player.CurrentVehicle.BrakeLightsOn = true;
                    }
                }
                catch (Exception exception)
                {
                    UI.ShowSubtitle("From vehicleDrivingMode()- first try/catch: " + exception);
                }
            }

            if (player.IsInVehicle() && driveMode == 1)
            {

                try
                {

                    player.CurrentVehicle.LightsMultiplier = (float)headlightPower;
                    player.CurrentVehicle.EnginePowerMultiplier = enginePowerMult;


                }
                catch (Exception exception)
                {
                    UI.ShowSubtitle("From vehicleDrivingMode()- driveMode 0:" + exception);
                }

            }

            if (player.IsInVehicle())
            {
                try
                {
                    if (driveMode == 2)
                    {
                        player.CurrentVehicle.LightsMultiplier = (float)headlightPower;
                        player.CurrentVehicle.EnginePowerMultiplier = (float)enginePowerMult;
                        player.CurrentVehicle.EngineTorqueMultiplier = (float)enginePowerMult;
                    }


                    if (rpmPercent > 80)
                    {
                        player.CurrentVehicle.NeonLightsColor = System.Drawing.Color.FromArgb((int)colorValuefromRPM, ((int)colorValuefromRPM - (int)decreaseColor), ((int)colorValuefromRPM - (int)decreaseColor));
                    }
                    else
                    {
                        if (rpmPercent >= 50)
                        {
                            decreaseColor = rpmPercent / 100 * 128;
                            player.CurrentVehicle.NeonLightsColor = System.Drawing.Color.FromArgb((int)colorValuefromRPM, ((int)colorValuefromRPM - (int)decreaseColor), 0);
                        }
                        else
                        {
                            player.CurrentVehicle.NeonLightsColor = System.Drawing.Color.FromArgb(((int)colorValuefromRPM - (int)decreaseColor), (int)colorValuefromRPM, 0);
                        }
                    }
                }
                catch (Exception exception)
                {
                    //...
                }
            }

        }

        public void checkVehicleDrivingMode()
        {
            
            try
            {

                if (player.IsGettingIntoAVehicle)
                {
                    UI.ShowSubtitle("N: " + driveMode);
                    if (driveMode == 1)
                    {
                        player.GetVehicleIsTryingToEnter().SetNeonLightsOn(VehicleNeonLight.Left, false);
                        player.GetVehicleIsTryingToEnter().SetNeonLightsOn(VehicleNeonLight.Right, false);
                        player.GetVehicleIsTryingToEnter().SetNeonLightsOn(VehicleNeonLight.Back, false);
                        player.GetVehicleIsTryingToEnter().SetNeonLightsOn(VehicleNeonLight.Front, false);
                    }

                    if (driveMode >= 2)
                    {
                        player.GetVehicleIsTryingToEnter().SetNeonLightsOn(VehicleNeonLight.Left, true);
                        player.GetVehicleIsTryingToEnter().SetNeonLightsOn(VehicleNeonLight.Right, true);
                        player.GetVehicleIsTryingToEnter().SetNeonLightsOn(VehicleNeonLight.Back, true);
                        player.GetVehicleIsTryingToEnter().SetNeonLightsOn(VehicleNeonLight.Front, true);
                    }
                }
            }
            catch (Exception exception)
            {
                UI.Notify("Exception at CheckVehicleDrivingMode(): " + exception);
            }
            
        }
            
        public void keylessEntry()
        {

            try
            {
                if (player.IsInVehicle())
                {
                    if (player.CurrentVehicle.HasCollidedWithAnything)
                    {
                        if (selfDrivingActive)
                        {
                            player.CurrentVehicle.Repair();
                        }
                    }
                }

            }
            catch (Exception exception)
            {
                UI.Notify("Exception at KeylessEntry Colliding: " + exception, true);
            }

            if (player.IsInTaxi)
            {
                if(!player.CurrentVehicle.IsSeatFree(VehicleSeat.LeftRear) || !player.CurrentVehicle.IsSeatFree(VehicleSeat.RightRear) || !player.CurrentVehicle.IsSeatFree(VehicleSeat.RightFront))
                {
                    player.CurrentVehicle.TaxiLightOn = true;
                }
                else
                {
                    player.CurrentVehicle.TaxiLightOn = false;
                }
            }

            async Task PutTaskDelay()
            {
                await Task.Delay(2000);
            }

            if (!player.IsInVehicle())
            {
                if (player.LastVehicle.IsInRangeOf(new Vector3(player.Position.X, player.Position.Y, player.Position.Z), 10) == false)
                {
                    player.LastVehicle.LockStatus = VehicleLockStatus.Locked;
                    player.LastVehicle.EngineRunning = false;
                    player.LastVehicle.HandbrakeOn = true;
                    player.LastVehicle.InteriorLightOn = false;
                    
                    openDoorDetector();

                }
                else
                {
                    player.LastVehicle.LockStatus = VehicleLockStatus.Locked;
                    player.LastVehicle.BrakeLightsOn = true;
                    player.LastVehicle.InteriorLightOn = true;
                    player.LastVehicle.LeftIndicatorLightOn = true;
                    player.LastVehicle.RightIndicatorLightOn = true;
                    player.LastVehicle.EngineRunning = true;
                    player.LastVehicle.HandbrakeOn = true;
                    player.LastVehicle.LightsOn = true;
                    player.LastVehicle.LightsMultiplier = 1.5f;
                    player.LastVehicle.HandbrakeOn = false;
                }

                if (player.IsGettingIntoAVehicle || player.IsTryingToEnterALockedVehicle)
                {
                    player.GetVehicleIsTryingToEnter().LockStatus = VehicleLockStatus.Unlocked;
                    player.GetVehicleIsTryingToEnter().HandbrakeOn = false;
                    player.GetVehicleIsTryingToEnter().BrakeLightsOn = true;
                    player.GetVehicleIsTryingToEnter().InteriorLightOn = false;
                    player.GetVehicleIsTryingToEnter().LightsOn = true;
                    player.GetVehicleIsTryingToEnter().LeftIndicatorLightOn = false;
                    player.GetVehicleIsTryingToEnter().RightIndicatorLightOn = false;
                    if (windowsOpen)
                    {
                        player.GetVehicleIsTryingToEnter().RollDownWindow(VehicleWindow.FrontLeftWindow);
                        player.GetVehicleIsTryingToEnter().RollDownWindow(VehicleWindow.FrontRightWindow);
                    }
                    else 
                    {
                        player.GetVehicleIsTryingToEnter().RollUpWindow(VehicleWindow.FrontLeftWindow);
                        player.GetVehicleIsTryingToEnter().RollUpWindow(VehicleWindow.FrontRightWindow);
                    }
                    checkVehicleDrivingMode();
                }
            }
        }

        public void vehicleLockingSystem()
        {
            try
            {

                if (vehicleLocked)
                {
                    if (player.IsInVehicle())
                    {
                        player.CurrentVehicle.LockStatus = VehicleLockStatus.Unlocked;

                        if (lockHornOn)
                        {
                            player.CurrentVehicle.SoundHorn(100);
                            player.CurrentVehicle.LightsOn = true;
                            Wait(100);
                            player.CurrentVehicle.LightsOn = false;
                            Wait(200);
                            player.CurrentVehicle.SoundHorn(100);
                            player.CurrentVehicle.LightsOn = true;
                            Wait(100);
                            player.CurrentVehicle.LightsOn = false;
                        }

                        UI.ShowHelpMessage("Vehicle unlocked", false);
                    }
                    else
                    {
                        player.LastVehicle.LockStatus = VehicleLockStatus.Unlocked;
                        if (lockHornOn)
                        {
                            player.LastVehicle.SoundHorn(100);
                            player.LastVehicle.LightsOn = true;
                            Wait(100);
                            player.LastVehicle.LightsOn = false;
                            Wait(200);
                            player.LastVehicle.SoundHorn(100);
                            player.LastVehicle.LightsOn = true;
                            Wait(100);
                            player.LastVehicle.LightsOn = false;
                        }
                        UI.ShowHelpMessage("Last Vehicle unlocked", false);
                    }
                    vehicleLocked = false;
                }
                else
                {
                    if (player.IsInVehicle())
                    {
                        player.CurrentVehicle.LockStatus = VehicleLockStatus.Locked;
                        if (lockHornOn)
                        {
                            player.CurrentVehicle.SoundHorn(100);
                            player.CurrentVehicle.LightsOn = true;
                            Wait(200);
                            player.CurrentVehicle.LightsOn = false;
                        }
                        UI.ShowHelpMessage("Vehicle locked", false);
                    }
                    else
                    {
                        player.LastVehicle.LockStatus = VehicleLockStatus.Locked;
                        if (lockHornOn)
                        {
                            player.LastVehicle.SoundHorn(100);
                            player.CurrentVehicle.SoundHorn(100);
                            player.LastVehicle.LightsOn = true;
                            Wait(200);
                            player.LastVehicle.LightsOn = false;
                        }
                        UI.ShowHelpMessage("Last Vehicle locked", false);
                    }
                    vehicleLocked = true;
                }
            }
            catch (Exception exception)
            {

                UI.Notify("Lock status: " + exception);

            }
        }

        public void vehicleWindowSystem()
        {
            try
            {
                if (!World.Weather.Equals(Weather.Snowing) || !World.Weather.Equals(Weather.Raining) || !World.Weather.Equals(Weather.ThunderStorm) || !World.Weather.Equals(Weather.Clearing) && player.IsInVehicle())
                {
                    if (windowsOpen == false)
                    {
                        player.CurrentVehicle.RollDownWindow(VehicleWindow.FrontLeftWindow);
                        player.CurrentVehicle.RollDownWindow(VehicleWindow.FrontRightWindow);

                        UI.ShowHelpMessage("Rolled down windows", false);
                        windowsOpen = true;
                    }
                    else
                    {
                        player.CurrentVehicle.RollUpWindow(VehicleWindow.FrontLeftWindow);
                        player.CurrentVehicle.RollUpWindow(VehicleWindow.FrontRightWindow);

                        UI.ShowHelpMessage("Rolled up windows", false);
                        windowsOpen = false;
                    }
                }
                if (World.Weather.Equals(Weather.Raining) || World.Weather.Equals(Weather.ThunderStorm) || World.Weather.Equals(Weather.Clearing) || World.Weather.Equals(Weather.Neutral) && player.IsInVehicle())
                {
                    UI.Notify("Weather: " + GTA.World.Weather);
                    player.CurrentVehicle.RollUpWindow(VehicleWindow.FrontLeftWindow);
                    player.CurrentVehicle.RollUpWindow(VehicleWindow.FrontRightWindow);
                    player.CurrentVehicle.RollUpWindow(VehicleWindow.BackLeftWindow);
                    player.CurrentVehicle.RollUpWindow(VehicleWindow.BackRightWindow);

                    UI.ShowHelpMessage("Bad weather, rolled up windows", false);
                }

            }
            catch (Exception exception)
            {
                UI.Notify("From VehicleWindowSystem(): OpenWindow on key down: " + exception, true);
            }
        }

        public void openDoorDetector()
        {
            if (player.IsInVehicle() && (player.CurrentVehicle.Speed * 3.6f) > 5f)
            {
                if (player.CurrentVehicle.IsDoorOpen(VehicleDoor.FrontLeftDoor))
                {
                    player.CurrentVehicle.CloseDoor(VehicleDoor.FrontLeftDoor, false);
                }

                if (player.CurrentVehicle.IsDoorOpen(VehicleDoor.FrontRightDoor))
                {
                    player.CurrentVehicle.CloseDoor(VehicleDoor.FrontRightDoor, false);
                }

                if (player.CurrentVehicle.IsDoorOpen(VehicleDoor.BackLeftDoor))
                {
                    player.CurrentVehicle.CloseDoor(VehicleDoor.BackLeftDoor, false);
                }

                if (player.CurrentVehicle.IsDoorOpen(VehicleDoor.BackRightDoor))
                {
                    player.CurrentVehicle.CloseDoor(VehicleDoor.BackRightDoor, false);
                }

                if (player.CurrentVehicle.IsDoorOpen(VehicleDoor.Hood))
                {
                    player.CurrentVehicle.CloseDoor(VehicleDoor.Hood, false);
                }

                if (player.CurrentVehicle.IsDoorOpen(VehicleDoor.Trunk))
                {
                    player.CurrentVehicle.CloseDoor(VehicleDoor.Trunk, false);
                }

            }else if (!player.IsInVehicle() && player.LastVehicle.IsInRangeOf(new Vector3(player.Position.X, player.Position.Y, player.Position.Z), 10) == false)
            {
                if (player.LastVehicle.IsDoorOpen(VehicleDoor.FrontLeftDoor))
                {
                    player.LastVehicle.CloseDoor(VehicleDoor.FrontLeftDoor, false);
                }

                if (player.LastVehicle.IsDoorOpen(VehicleDoor.FrontRightDoor))
                {
                    player.LastVehicle.CloseDoor(VehicleDoor.FrontRightDoor, false);
                }

                if (player.LastVehicle.IsDoorOpen(VehicleDoor.BackLeftDoor))
                {
                    player.LastVehicle.CloseDoor(VehicleDoor.BackLeftDoor, false);
                }

                if (player.LastVehicle.IsDoorOpen(VehicleDoor.BackRightDoor))
                {
                    player.LastVehicle.CloseDoor(VehicleDoor.BackRightDoor, false);
                }

                if (player.LastVehicle.IsDoorOpen(VehicleDoor.Hood))
                {
                    player.LastVehicle.CloseDoor(VehicleDoor.Hood, false);
                }

                if (player.LastVehicle.IsDoorOpen(VehicleDoor.Trunk))
                {
                    player.LastVehicle.CloseDoor(VehicleDoor.Trunk, false);
                }
            }
        }

    }   
}