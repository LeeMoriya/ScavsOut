using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using OptionalUI;
using Partiality;
using Partiality.Modloader;
using UnityEngine;
using RWCustom;

//Remove PublicityStunt requirement
//--------------------------------------------------------------------------------------
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
        public string AssemblyName { get; }
    }
}
//--------------------------------------------------------------------------------------

public class ScavsOut : PartialityMod
{
    public static bool Forbidden = true;
    public static bool PickUp = true;
    public static bool Flee = true;
    public static PartialityMod instance;
    public static List<Scavenger> fleeList = new List<Scavenger>();
    public static string shelter;

    public ScavsOut()
    {
        this.ModID = "ScavsOut!";
        this.Version = "1.0";
        this.author = "LeeMoriya";
    }

    public override void OnEnable()
    {
        base.OnEnable();
        instance = this;
        Hook();
    }

    public static OptionInterface LoadOI()
    {
        return new ScavsOutConfig();
    }

    public static void Hook()
    {
        On.Creature.SuckedIntoShortCut += Creature_SuckedIntoShortCut;
        On.AbstractCreature.Move += AbstractCreature_Move;
        On.Scavenger.Grab += Scavenger_Grab;
        On.Scavenger.Violence += Scavenger_Violence;
        On.Scavenger.Act += Scavenger_Act;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
    }

    private static void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
    {
        orig.Invoke(self, player);
        fleeList = new List<Scavenger>();
        shelter = "";
    }

    //Scavs in the flee list will want to flee the room, and are removed from the list when they do
    private static void Scavenger_Act(On.Scavenger.orig_Act orig, Scavenger self)
    {
        orig.Invoke(self);
        if (self.room.world.game.IsStorySession)
        {
            if (fleeList.Contains(self))
            {
                if (self.abstractCreature.Room.name != shelter)
                {
                    fleeList.Remove(self);
                }
                else
                {
                    self.AI.behavior = ScavengerAI.Behavior.LeaveRoom;
                    self.abstractCreature.abstractAI.SetDestination(self.room.GetWorldCoordinate(self.room.ShortcutLeadingToNode(0).DestTile));
                    if (Vector2.Distance(self.mainBodyChunk.pos, self.room.MiddleOfTile(self.room.ShortcutLeadingToNode(0).StartTile)) < 50f)
                    {
                        for (int i = 0; i < self.room.shortcuts.Length; i++)
                        {
                            if (self.room.shortcuts[i].shortCutType == ShortcutData.Type.RoomExit)
                            {
                                self.SuckedIntoShortCut(self.room.shortcuts[i].StartTile, false);
                            }
                        }
                    }
                }
            }
        }
    }

    //Add scavs to the flee list, changing their behaviour to LeaveRoom while they're in it
    private static void Scavenger_Violence(On.Scavenger.orig_Violence orig, Scavenger self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        if (Flee && self.room != null && self.room.abstractRoom.shelter && self.room.world.game.IsStorySession)
        {
            shelter = self.room.abstractRoom.name;
            fleeList.Add(self);
            self.AI.behavior = ScavengerAI.Behavior.LeaveRoom;
        }
        orig.Invoke(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    //Scavengers cannot pick up objects while in a Shelter
    private static bool Scavenger_Grab(On.Scavenger.orig_Grab orig, Scavenger self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        if (PickUp && self.room != null && self.room.abstractRoom.shelter && self.room.world.game.IsStorySession)
        {
            return false;
        }
        return orig.Invoke(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
    }

    //Abstract Scavengers cannot move to Shelters
    private static void AbstractCreature_Move(On.AbstractCreature.orig_Move orig, AbstractCreature self, WorldCoordinate newCoord)
    {
        if (Forbidden && self.creatureTemplate.type == CreatureTemplate.Type.Scavenger && self.world.game.IsStorySession && self.world != null && self.world.GetAbstractRoom(newCoord).shelter)
        {
            return;
        }
        orig.Invoke(self, newCoord);
    }


    //Realized Scavengers cannot enter a pipe connected to a Shelter
    private static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, RWCustom.IntVector2 entrancePos, bool carriedByOther)
    {
        if (Forbidden && self.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger && self.room.world.game.IsStorySession)
        {
            if (self.room != null && self.room.shortcutData(entrancePos).shortCutType == ShortcutData.Type.RoomExit && self.room.world.GetAbstractRoom(self.room.abstractRoom.connections[self.room.shortcutData(entrancePos).destNode]).shelter)
            {
                (self as Scavenger).AI.pathFinder.destination = new WorldCoordinate(0, 0, 0, 0);
                (self as Scavenger).SpitOutOfShortCut(entrancePos, self.room, false);
                return;
            }
        }
        orig.Invoke(self, entrancePos, carriedByOther);
    }
}



