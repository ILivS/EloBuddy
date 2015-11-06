using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Drawing.Printing;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EloBuddy.SDK;
using EloBuddy;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;




namespace LivVel_Koz
{
    internal class Program
    {
        public static Menu Menu, Settings, Misc;
        public static Spell.Skillshot Q;
       // public static Spell.Skillshot QSplit;
       // public static Spell.Skillshot QDummy;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static AIHeroClient Player = ObjectManager.Player;
        public static MissileClient QMissile;
       
        public  static Spell.Targeted Ignite;





        private static void Main(string[] args)
        {



            // Game.OnUpdate += Game_OnGameUpdate;
            

            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
           






        }


        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
       }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (_Player.Hero != Champion.Velkoz)
            {
               Chat.Print("Champion not supported!");
               return;
            }
            


            Chat.Print("Vel_Koz By LivS  LOADED  ", System.Drawing.Color.GreenYellow);
            //Create Spells
            Q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, 250, 1300, 50);
            // QSplit = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 2100, 55);
            //  QDummy = new Spell.Skillshot(SpellSlot.Q, (uint) Math.Sqrt(Math.Pow(Q.Range, 2) + Math.Pow(QSplit.Range, 2)),
            // SkillShotType.Linear, 250, int.MaxValue, 50);
         
            W = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Linear, 250, 1700, 85);
           
            E = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Circular, 500, 1500, 100);
            
            R = new Spell.Skillshot(SpellSlot.R, 1550, SkillShotType.Linear, 300, int.MaxValue, 1);
           
            var slot = Player.GetSpellSlotFromName("summonerdot");
            if (slot != SpellSlot.Unknown)
            {
                Ignite = new Spell.Targeted(slot, 600);
            }


            Menu = MainMenu.AddMenu("LivVel_Koz", "LivVel_Koz");
          
            Menu.AddSeparator();
            //ComboMenu
            Settings = Menu.AddSubMenu("Combo", "Combo");
            Settings.Add("QCombo", new CheckBox("Use Q"));
            Settings.Add("WCombo", new CheckBox("Use W"));
            Settings.Add("ECombo", new CheckBox("Use E"));
            Settings.Add("RCombo", new CheckBox("Use R"));
           
            

            Settings.AddSeparator();
            //Harass
            Settings = Menu.AddSubMenu("Harass");
            Settings.Add("QHarass", new CheckBox("Use Q"));
            Settings.Add("WHarass", new CheckBox("Use W" ));
            Settings.Add("EHarass", new CheckBox("Use E"));
            
            Settings.AddSeparator();
            //Farm
            Settings = Menu.AddSubMenu("LaneClear");
            Settings.Add("QFarm", new CheckBox("Use Q"));
            Settings.Add("WFarm", new CheckBox("Use W"));
            Settings.Add("EFarm", new CheckBox("Use E", false));
            Settings.Add("ManaClear", new Slider("Min % Mana", 50, 0, 100));
            Settings.Add("minionw", new Slider("Minions to use E", 3, 1, 20));

            Settings.AddSeparator();
            //jungle Farm
            Settings = Menu.AddSubMenu("JungleClear");
            Settings.Add("Qjg", new CheckBox("Use Q"));
            Settings.Add("Wjg", new CheckBox("Use W)"));
            Settings.Add("Ejg", new CheckBox("Use E"));
            Settings.AddSeparator();
            //Misc
            Misc = Menu.AddSubMenu("Misc");
            Misc.Add("InterruptSpells", new CheckBox("Use interrupt Spells"));

            Misc.Add("Don't Use R on", new CheckBox("NotUseR"));
            //Flee
            Settings = Menu.AddSubMenu("Flee");
            Settings.Add("QFlee", new CheckBox("Use Q", true));
            Settings.Add("WFlee", new CheckBox("Use W", true));
            
            foreach (var hero in EntityManager.Heroes.Enemies)
            {
                // enemies doesnt need to check for team kappa
                Misc.Add("NotuseR" + hero.ChampionName, new CheckBox("NotUseR" + hero.ChampionName, false));
            }
            Misc.AddSeparator();
            //KS
            Settings = Menu.AddSubMenu("KillSteal", "KillSteal");
           Settings.Add("QKs", new CheckBox("Use Q", true));
           Settings.Add("WKs", new CheckBox("Use W", true));
           
            Settings.Add("IKs", new CheckBox("Use Ignite", true));


            // DamageIndicator.Initialize(GetComboDamage);
            // DamageIndicator.DrawingColor = System.Drawing.Color.Aqua;





            //Drawing 
            Settings = Menu.AddSubMenu("Drawing");
            Settings.Add("DrawQ", new CheckBox("DrawQ"));
            Settings.Add("DrawW", new CheckBox("DrawW"));
            Settings.Add("DrawE", new CheckBox("DrawE"));
            Settings.Add("DrawR", new CheckBox("DrawR"));



            Settings.AddSeparator();
           

            Game.OnTick += Game_OnTick;

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Drawing.OnDraw += Drawing_OnDraw;
            Spellbook.OnUpdateChargeableSpell += Spellbook_OnUpdateChargedSpell;

        }

     

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead) { return; }





            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
           // if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
           // {
             //   Harass();
           // }
           if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
           }
           // if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear)
           // {
           //     JungleClear();
           // }
            KillSteal();

        }

       


        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            if (Misc["InterruptSpells"].Cast<CheckBox>().CurrentValue) return;
            E.Cast(sender);

        }


       static  float GetComboDamage(AIHeroClient target)
        {
            var damage = 0d;

            int collisionCount = Q.GetPrediction(target).CollisionObjects.Count();
            if (Q.IsReady() && collisionCount < 1)
                damage += Player.GetSpellDamage(target, SpellSlot.Q);

            if (W.IsReady())
                damage +=  Player.GetSpellDamage(target, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(target, SpellSlot.E);

            if (R.IsReady())
                damage += GetUltDmg((AIHeroClient)target);
            if (Ignite != null && Ignite.IsReady())
            {
                damage += Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite);
            }

            damage = Player.CalculateDamageOnUnit(target, DamageType.Magical, (float)damage) - 20;
            damage += GetUltDmg((AIHeroClient)target);
            if (Ignite != null && Ignite.IsReady())
            {
            damage += Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite);
            }
            damage += GetPassiveDmg();

            return (float)damage;
        }

         static float GetPassiveDmg()
        {
            double stack = 0;
            double dmg = 25 + (10 * Player.Level);

            if (Q.IsReady())
                stack++;

            if (W.IsReady())
                stack += 2;

            if (E.IsReady())
                stack++;

            stack = stack / 3;

            stack = Math.Floor(stack);

            dmg = dmg * stack;

            //Game.PrintChat("Stacks: " + stack);

            return (float)dmg;
        }

        static float GetUltDmg(Obj_AI_Base target)
        {
            double dmg = 0;

            var dist = (Player.ServerPosition.To2D().Distance(target.ServerPosition.To2D()) - 600) / 100;
           var div = Math.Ceiling(10 - dist);

            //Game.PrintChat("ult dmg" + target.BaseSkinName + " " + div);

            if (Player.Distance(target.Position) < 600)
                div = 10;

            if (Player.Distance(target.Position) < 1550)
                if (R.IsReady())
                {
                    double ultDmg = Player.GetSpellDamage(target, SpellSlot.R) / 10;

                    dmg += ultDmg * div;
                }

            if (div >= 3)
                dmg += 25 + (10 * Player.Level);

            if (Settings["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                if (R.IsReady() && dmg > target.Health + 20)
                {
                    var wts = Drawing.WorldToScreen(target.Position);
                    Drawing.DrawText(wts[0], wts[1], System.Drawing.Color.White, ("Killable"));
                }
                else
                {
                    var wts = Drawing.WorldToScreen(target.Position);
                    Drawing.DrawText(wts[0], wts[1], System.Drawing.Color.Red, "No Ult Kill");
                }
            }

            return (float)dmg;
        }
    

        private static void Combo()
        {
            var range = R.IsReady() ? R.Range : Q.Range;
            var target = TargetSelector.GetTarget(range, DamageType.Magical);
         //   var QPred = Prediction.Position.PredictLinearMissile(target, Q.Range, Q.Radius, Q.CastDelay, Q.Speed, int.MaxValue, Player.ServerPosition);

            // var qDummyTarget = TargetSelector.GetTarget(QDummy.Range, DamageType.Magical);
            // var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            //var qDummyTarget = TargetSelector.GetTarget(QDummy.Range, DamageType.Magical);
            // var wTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            //var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            //var rTarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);

            // var useIgnite = Settings["UseIgnite"].Cast<CheckBox>().CurrentValue;
            var useQ = Settings["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = Settings["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = Settings["Ecombo"].Cast<CheckBox>().CurrentValue;
            var useR = Settings["Rcombo"].Cast<CheckBox>().CurrentValue;
            
            if (target == null)
                return;


            //R = (Settings["NotUseR" + target.ChampionName] != null && Settings["NotUseR" + target.ChampionName].Cast<CheckBox>().CurrentValue) && useR;
           
           // float dmg = GetComboDamage(target); 
            if ( W.IsReady() && Player.Distance(target.Position) <= W.Range && useW &&
                W.GetPrediction(target).HitChance >= HitChance.High )
            {
                W.Cast(target);
                return;
            }

            if  ( E.IsReady() && Player.Distance(target.Position) < E.Range && useE &&
                E.GetPrediction(target).HitChance >= HitChance.High )
            {
                E.Cast(target);
                return;
            }
            if ( Q.IsReady() && useQ && target.IsValidTarget(Q.Range)  )
            {
               Q.Cast(target);
                
                
                
                return;
            }
            if ( R.IsReady() && Player.Distance(target.Position) < R.Range  && useR )
            {
                if (GetUltDmg(target) >= target.Health)
                {
                    R.Cast(target.ServerPosition);
                }

            }

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Settings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = System.Drawing.Color.White, BorderWidth = 1, Radius = Q.Range }.Draw(
                    ObjectManager.Player.Position);
            }
            if (Settings["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = System.Drawing.Color.White, BorderWidth = 1, Radius = W.Range }.Draw(
                    ObjectManager.Player.Position);
            }
            if (Settings["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = System.Drawing.Color.White, BorderWidth = 1, Radius = E.Range }.Draw(
                    ObjectManager.Player.Position);
            }
            if (Settings["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = System.Drawing.Color.White, BorderWidth = 1, Radius = R.Radius }.Draw(
                    ObjectManager.Player.Position);
            }

        }
       

    private static void LaneClear()
        
            {
           
            var minionlc = Settings["minionw"].Cast<Slider>().CurrentValue;
            var manalc = Settings["ManaClear"].Cast<Slider>().CurrentValue;

            // var rangedMinionsE = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Minion, EntityManager.UnitTeam.Enemy, Player.Position, E.Range, true).ToList<Obj_AI_Base>();
            var useQ = Settings["QFarm"].Cast<CheckBox>().CurrentValue;

            var useW = Settings["WFarm"].Cast<CheckBox>().CurrentValue;
            var useE = Settings["EFarm"].Cast<CheckBox>().CurrentValue;
            if ( Player.ManaPercent <= manalc) return;
         
            

                if (Q.IsReady() && useQ )
                {
                    var target =
                        EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,Player.ServerPosition,Q.Range).ToArray();
                    var pred = EntityManager.MinionsAndMonsters.GetLineFarmLocation(target, Q.Width, (int) Q.Range);

                    if (pred.HitNumber >= minionlc)
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            


            if ( W.IsReady()  && useW )
            {
                var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition, W.Range + W.Width ).ToArray();
                
               
                    var farmLocation = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minions, W.Width, (int)W.Range);
                    if (farmLocation.HitNumber >= minionlc)
                    {
                        if (W.Cast(farmLocation.CastPosition))
                        {
                            return;
                        }
                    }
                }
            

            if (useE && E.IsReady())
            {
                var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition, E.Range + E.Width).ToArray();

                var ePos = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(minions, E.Width, (int)E.Range);
                if (ePos.HitNumber >= minionlc)
                {
                    if (W.Cast(ePos.CastPosition))
                    {
                        return;
                    }
                }
            }
        }

      



       protected void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            // return if its not a missle
            if (!(obj is MissileClient))
                return;

            var spell = (MissileClient)obj;

            if (Player.Distance(obj.Position) < 1500)
            {
                //Q
                if (spell.IsValid && spell.SData.Name == "VelkozQMissile")
                {
                    //Game.PrintChat("Woot");
                    QMissile = spell;
                }
            }
        }

        static void Spellbook_OnUpdateChargedSpell(Spellbook sender, SpellbookUpdateChargeableSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            {
                return;
                        
            }
        }
        private static void KillSteal()
        {
            var useQ = Settings["QKs"].Cast<CheckBox>().CurrentValue;
            var useI = Settings["IKs"].Cast<CheckBox>().CurrentValue;

            if (useQ && useI && Q.IsReady() && Ignite != null && Ignite.IsReady())
            {
                var enemies =
                     EntityManager.Heroes.Enemies.Where(
                         t =>
                         t.IsValidTarget() && Ignite.IsInRange(t) && Player.GetSpellDamage(t,SpellSlot.Q) + Player.GetSummonerSpellDamage(t,DamageLibrary.SummonerSpells.Ignite) >= t.Health );

                // Resharper op
                foreach (var pred in enemies.Select(t => Q.GetPrediction(t)).Where(pred => pred != null).Where(pred => pred.HitChance >= HitChance.High))
                {
                    Q.Cast(pred.CastPosition);
                    Ignite.Cast(pred.CastPosition);
                }

            }
        }
    }
}






    


       

    





















































