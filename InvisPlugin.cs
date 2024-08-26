using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using System.Drawing;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API;
using static CounterStrikeSharp.API.Core.Listeners;
using CounterStrikeSharp.API.Modules.Memory;

namespace InvisPlugin;

[MinimumApiVersion(260)]
public class InvisPlugin : BasePlugin
{
    public override string ModuleName => "InvisPlugin";

    public override string ModuleVersion => "0.1.2";
    public override string ModuleAuthor => "Manio";
    public override string ModuleDescription => "Invisibility plugin";

    public List<CCSPlayerController> InvisiblePlayers = [];

    public override void Load(bool hotReload)
    {
        Console.WriteLine(" ");
        Console.WriteLine(" ___   __    _  __   __  ___   _______      _______  ___      __   __  _______  ___   __    _ ");
        Console.WriteLine("|   | |  |  | ||  | |  ||   | |       |    |       ||   |    |  | |  ||       ||   | |  |  | |");
        Console.WriteLine("|   | |   |_| ||  |_|  ||   | |  _____|    |    _  ||   |    |  | |  ||    ___||   | |   |_| |");
        Console.WriteLine("|   | |       ||       ||   | | |_____     |   |_| ||   |    |  |_|  ||   | __ |   | |       |");
        Console.WriteLine("|   | |  _    ||       ||   | |_____  |    |    ___||   |___ |       ||   ||  ||   | |  _    |");
        Console.WriteLine("|   | | | |   | |     | |   |  _____| |    |   |    |       ||       ||   |_| ||   | | | |   |");
        Console.WriteLine("|___| |_|  |__|  |___|  |___| |_______|    |___|    |_______||_______||_______||___| |_|  |__|");
        Console.WriteLine("			     >> Version: 0.1.2");
        Console.WriteLine("		>> GitHub: https://github.com/maniolos/Cs2Invis");
        Console.WriteLine("		>> Fork: https://github.com/johandrevwyk/InvisPlugin");
        Console.WriteLine(" ");

        RegisterListener<OnTick>(OnTick);
        RegisterEventHandler<EventItemPickup>(OnItemPickup);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    }

    [ConsoleCommand("css_invis", "Invisible command")]
    [RequiresPermissions("@css/invis")]
    public void OnInvisCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive) return;

        if (InvisiblePlayers.Contains(player))
        {
            SetPlayerVisible(player);
            InvisiblePlayers.Remove(player);          
            commandInfo.ReplyToCommand("Invisiblity disabled");
        }
        else
        {
            SetPlayerInvisible(player);
            InvisiblePlayers.Add(player);
            commandInfo.ReplyToCommand("Invisiblity enabled");
        }

    }

    private void OnTick()
    {
        foreach (CCSPlayerController player in InvisiblePlayers)
        {
            var pawn = player.PlayerPawn.Value;

            if (pawn != null)
            {
                pawn.EntitySpottedState.Spotted = false;
                Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_entitySpottedState", Schema.GetSchemaOffset("EntitySpottedState_t", "m_bSpotted"));

                Span<uint> spottedByMask = pawn.EntitySpottedState.SpottedByMask;
                for (int i = 0; i < spottedByMask.Length; i++)
                {
                    spottedByMask[i] = 0;
                }

                Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_entitySpottedState", Schema.GetSchemaOffset("EntitySpottedState_t", "m_bSpottedByMask"));
            }
        }
    }


    public static void SetPlayerVisible(CCSPlayerController player)
    {
        var playerPawnValue = player.PlayerPawn.Value;
        if (playerPawnValue == null)
            return;
        playerPawnValue.Render = Color.FromArgb(255, 255, 255, 255);
        Utilities.SetStateChanged(playerPawnValue, "CBaseModelEntity", "m_clrRender");
        var activeWeapon = playerPawnValue!.WeaponServices?.ActiveWeapon.Value;
        if (activeWeapon != null && activeWeapon.IsValid)
        {
            activeWeapon.Render = Color.FromArgb(255, 255, 255, 255);
            activeWeapon.ShadowStrength = 1.0f;
            Utilities.SetStateChanged(activeWeapon, "CBaseModelEntity", "m_clrRender");
        }

        var myWeapons = playerPawnValue.WeaponServices?.MyWeapons;
        if (myWeapons != null)
        {
            foreach (var gun in myWeapons)
            {
                var weapon = gun.Value;
                if (weapon != null)
                {
                    weapon.Render = Color.FromArgb(255, 255, 255, 255);
                    weapon.ShadowStrength = 1.0f;
                    Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");
                }
            }
        }
    }
    public static void SetPlayerInvisible(CCSPlayerController player)
    {
        
        var playerPawnValue = player.PlayerPawn.Value;
        if (playerPawnValue == null || !playerPawnValue.IsValid)
        {
            return;
        }

        if (playerPawnValue != null && playerPawnValue.IsValid)
        {
            playerPawnValue.Render = Color.FromArgb(0, 255, 255, 255);
            Utilities.SetStateChanged(playerPawnValue, "CBaseModelEntity", "m_clrRender");
        }

        var activeWeapon = playerPawnValue!.WeaponServices?.ActiveWeapon.Value;
        if (activeWeapon != null && activeWeapon.IsValid)
        {
            activeWeapon.Render = Color.FromArgb(0, 255, 255, 255);
            activeWeapon.ShadowStrength = 0.0f;
            Utilities.SetStateChanged(activeWeapon, "CBaseModelEntity", "m_clrRender");
        }

        var myWeapons = playerPawnValue.WeaponServices?.MyWeapons;
        if (myWeapons != null)
        {
            foreach (var gun in myWeapons)
            {
                var weapon = gun.Value;
                if (weapon != null)
                {
                    weapon.Render = Color.FromArgb(0, 255, 255, 255);
                    weapon.ShadowStrength = 0.0f;
                    Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");

                    if (weapon.DesignerName == "weapon_c4")
                    {
                        Console.WriteLine($"[Inaccurate] C4 Blinking Status: {weapon.Blinktoggle}");
                        weapon.Blinktoggle = false;
                        Utilities.SetStateChanged(weapon, "CBaseFlex", "m_blinktoggle");
                    }
                }
            }
        }
       
    }

    public HookResult OnItemPickup(EventItemPickup @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid!;
 
        if (player != null && InvisiblePlayers.Contains(player))
        {
            SetPlayerInvisible(player);
        }

        return HookResult.Continue;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid!;

        if (player == null) return HookResult.Continue;
        InvisiblePlayers.Remove(player);

        return HookResult.Continue;
    }
}