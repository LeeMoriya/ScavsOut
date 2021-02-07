using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionalUI;
using CompletelyOptional;


public class ScavsOutConfig : OptionInterface
{
    public OpCheckBox shelterAccess;
    public OpCheckBox itemPickup;
    public OpCheckBox fleeShelter;
    public ScavsOutConfig() : base(mod: ScavsOut.instance)
    {

    }

    public override void Initialize()
    {
        //Tabs
        this.Tabs = new OpTab[1];
        this.Tabs[0] = new OpTab("Config");
        //Rect
        OpLabel title = new OpLabel(240f, 475f, "SCAVSOUT!", true);
        OpLabel author = new OpLabel(167f, 455f, "Created by: LeeMoriya | Idea by: ThreeFingerG", false);
        OpRect rect = new OpRect(new UnityEngine.Vector2(140f, 302f), new UnityEngine.Vector2(310f, 140f), 0.3f);
        //Checkboxes
        this.shelterAccess = new OpCheckBox(new UnityEngine.Vector2(160f, 400f), "shelter", true);
        OpLabel shelter = new OpLabel(190f, 402f, "Scavengers are not allowed in shelters", false);
        this.itemPickup = new OpCheckBox(new UnityEngine.Vector2(160f, 360f), "pickup", true);
        OpLabel pickup = new OpLabel(190f, 362f, "Scavengers cannot pick up items in shelters", false);
        this.fleeShelter = new OpCheckBox(new UnityEngine.Vector2(160f, 320f), "flee", true);
        OpLabel flee = new OpLabel(190f, 322f, "Scavengers flee shelters if injured", false);
        this.Tabs[0].AddItems(title, author, rect, this.shelterAccess, this.itemPickup, this.fleeShelter, shelter, pickup, flee);
    }

    public override void ConfigOnChange()
    {
        if (config.ContainsKey("shelter"))
        {
            if (config["shelter"] == "true")
            {
                ScavsOut.Forbidden = true;
            }
            else
            {
                ScavsOut.Forbidden = false;
            }
        }
        if (config.ContainsKey("pickup"))
        {
            if (config["pickup"] == "true")
            {
                ScavsOut.PickUp = true;
            }
            else
            {
                ScavsOut.PickUp = false;
            }
        }
        if (config.ContainsKey("flee"))
        {
            if (config["flee"] == "true")
            {
                ScavsOut.Flee = true;
            }
            else
            {
                ScavsOut.Flee = false;
            }
        }
    }
}
