﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;
using Color = System.Drawing.Color;

namespace Twitch
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, LaneClearMenu, JungleClearMenu, Misc, KillStealMenu, Items;
        public static Item Botrk;
        public static Item Bil;
        public static Item Youmuu;
        public static readonly int[] SDamage = {0, 15, 20, 25, 30, 35};
        public static readonly int[] BDamage = {0, 20, 35, 50, 65, 80};
        public const float YOff = 10;
        public const float XOff = 0;
        public const float Width = 107;
        public const float Thick = 9;
        private static Font thm;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Active E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Twitch")) return;
            Chat.Print("Doctor's Twitch Loaded!", Color.Orange);
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 250, 1550, 275);
            W.AllowedCollisionCount = int.MaxValue;
            E = new Spell.Active(SpellSlot.E, 1200);
            R = new Spell.Active(SpellSlot.R);
            thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 22, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Youmuu = new Item(3142, 10);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Doctor's Twitch", "Twitch");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Spell [Q]"));
            ComboMenu.Add("ComboW", new CheckBox("Spell [W]"));
            ComboMenu.AddGroupLabel("Combo [E] Settings");
            ComboMenu.Add("ComboE", new CheckBox("Spell [E]", false));
            ComboMenu.Add("MinEC", new Slider("Min Stacks Use [E]", 6, 0, 6));
            ComboMenu.AddGroupLabel("Combo [E] On");
            foreach (var target in EntityManager.Heroes.Enemies)
            {
                ComboMenu.Add("combo" + target.ChampionName, new CheckBox("" + target.ChampionName));
            }
            ComboMenu.AddSeparator();
            ComboMenu.Add("ComboR", new CheckBox("Spell [R]"));
            ComboMenu.Add("MinR", new Slider("Min Enemies Use [R]", 3, 0, 5));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassW", new CheckBox("Use [W]", false));
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q]", false));
            HarassMenu.Add("HminQ", new Slider("Min Enemies Use [Q]", 2, 1, 5));
            HarassMenu.AddGroupLabel("Harass [E] Settings");
            HarassMenu.Add("HarassE", new CheckBox("Use [E]", false));
            HarassMenu.Add("HminE", new Slider("Min Stacks Use [E]", 5, 0, 6));
            HarassMenu.AddGroupLabel("Harass [E] On");
            foreach (var target in EntityManager.Heroes.Enemies)
            {
                HarassMenu.Add("haras" + target.ChampionName, new CheckBox("" + target.ChampionName));
            }
            HarassMenu.Add("ManaQ", new Slider("Min Mana For Harass", 40));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("LaneClear Settings");
            LaneClearMenu.AddLabel("[E] Settings");
            LaneClearMenu.Add("E", new CheckBox("Use [E] LaneClear", false));
            LaneClearMenu.Add("ELH", new CheckBox("Only Use [E] If Orbwalker Cant Killable Minion", false));
            LaneClearMenu.Add("Minm", new Slider("Min Minions HasBuff Use [E] LaneClear", 3, 0, 6));
            LaneClearMenu.Add("MinS", new Slider("Min Stacks Use [E] LaneClear", 3, 1, 6));
            LaneClearMenu.AddLabel("[W] Settings");
            LaneClearMenu.Add("W", new CheckBox("Use [W] LaneClear", false));
            LaneClearMenu.Add("minW", new Slider("Min Hit Minions Use [W] LaneClear", 3, 1, 6));
            LaneClearMenu.AddLabel("Mana Settings");
            LaneClearMenu.Add("M", new Slider("Min Mana For LaneClear", 40));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("Q", new CheckBox("Use [Q] JungleClear", false));
            JungleClearMenu.Add("W", new CheckBox("Use [W] JungleClear", false));
            JungleClearMenu.Add("E", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("M", new Slider("Min Mana For JungleClear", 30));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Misc Settings");
            Misc.Add("AntiGap", new CheckBox("Use [W] AntiGapcloser"));
            Misc.Add("FleeQ", new CheckBox("Use [Q] Flee"));
            Misc.Add("FleeW", new CheckBox("Use [W] Flee"));
            Misc.AddGroupLabel("Use [E] Enemy Out Range");
            Misc.Add("E", new CheckBox("Use [E] If Enemy Escape", false));
            Misc.Add("ES", new Slider("Min Stacks Use [E]", 6, 1, 6));
            Misc.AddGroupLabel("Draw Settings");
            Misc.Add("DrawW", new CheckBox("[W] Range"));
            Misc.Add("DrawE", new CheckBox("[E] Range"));
            Misc.Add("DrawT", new CheckBox("Draw [Q] Time"));
            Misc.Add("Damage", new CheckBox("Damage Indicator"));

            Items = Menu.AddSubMenu("Items Settings", "Items");
            Items.AddGroupLabel("Items Settings");
            Items.Add("you", new CheckBox("Use [Youmuu]"));
            Items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));

            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Damage;
            Game.OnUpdate += Game_OnUpdate;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Orbwalker.OnUnkillableMinion += Orbwalker_CantLasthit;
            Orbwalker.OnPostAttack += ResetAttack;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawT"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (Player.Instance.HasBuff("TwitchHideInShadows"))
                {
                    DrawFont(thm, "Q Stealthed : " + QTime(Player.Instance), (float)(ft[0] - 100), (float)(ft[1] + 50), SharpDX.Color.GreenYellow);
                }
            }

            if (Misc["DrawT"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (Player.Instance.HasBuff("TwitchFullAutomatic"))
                {
                    DrawFont(thm, "R Time : " + RTime(Player.Instance), (float)(ft[0] - 70), (float)(ft[1] + 100), SharpDX.Color.Red);
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
			
            KillSteal();
            Item();
            Escape();
        }

        public static float QTime(Obj_AI_Base target)
        {
            if (target.HasBuff("TwitchHideInShadows"))
            {
                return Math.Max(0, target.GetBuff("TwitchHideInShadows").EndTime) - Game.Time;
            }
            return 0;
        }

        public static float RTime(Obj_AI_Base target)
        {
            if (target.HasBuff("TwitchFullAutomatic"))
            {
                return Math.Max(0, target.GetBuff("TwitchFullAutomatic").EndTime) - Game.Time;
            }
            return 0;
        }

        private static bool QCasting
        {
            get { return Player.Instance.HasBuff("TwitchHideInShadows"); }
        }

        public static void Combo()
        {
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            var MinE = ComboMenu["MinEC"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(E.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(W.Range))
                {
                    Q.Cast();
                }

                if (useW && W.IsReady() && !QCasting && target.IsValidTarget(W.Range) && _Player.Distance(target) > Player.Instance.GetAutoAttackRange(target))
                {
                    var pred = W.GetPrediction(target);
                    if (pred.HitChance >= HitChance.Medium)
                    {
                        W.Cast(pred.CastPosition);
                    }
                }

                if (useE && E.IsReady() && E.IsInRange(target) && target.HasBuff("twitchdeadlyvenom"))
                {
                    if (ComboMenu["combo" + target.ChampionName].Cast<CheckBox>().CurrentValue && Stack(target) >= MinE)
                    {
                        E.Cast();
                    }
                }

                if (useR && R.IsReady() && _Player.Position.CountEnemiesInRange(E.Range) >= MinR)
                {
                    R.Cast();
                }
            }
        }

        public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (useW && W.IsReady() && target.IsValidTarget(W.Range) && _Player.Distance(target) < Player.Instance.GetAutoAttackRange(target) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    var Pred = W.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.High)
                    {
                        W.Cast(Pred.CastPosition);
                    }
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["Q"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["W"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["E"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["M"].Cast<Slider>().CurrentValue;
            var monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().FirstOrDefault(a => a.IsValidTarget(E.Range) && (a.BaseSkinName == "SRU_Dragon" || a.BaseSkinName == "SRU_Baron"
                || a.BaseSkinName == "SRU_Blue" || a.BaseSkinName == "SRU_Red" || a.BaseSkinName == "SRU_Dragon_Air" || a.BaseSkinName == "SRU_Dragon_Elder" || a.BaseSkinName == "SRU_Dragon_Earth"
                || a.BaseSkinName == "SRU_Dragon_Fire" || a.BaseSkinName == "SRU_Dragon_Water"));
            if (Player.Instance.ManaPercent < mana)
            {
                return;
		    }

            if (monsters != null)
            {
                if (useW && W.CanCast(monsters) && W.IsInRange(monsters) && Stack(monsters) <= 4)
                {
                    W.Cast(monsters);
                }

                if (useE && E.IsReady() && E.IsInRange(monsters) && monsters.HasBuff("twitchdeadlyvenom") && monsters.TotalShieldHealth() <= EDamage(monsters))
                {
                    E.Cast();
                }

                if (useQ && Q.IsReady() && W.IsInRange(monsters))
                {
                    Q.Cast();
                }
            }
        }

        public static void Item()
        {
            var item = Items["BOTRK"].Cast<CheckBox>().CurrentValue;
            var yous = Items["you"].Cast<CheckBox>().CurrentValue;
            var Minhp = Items["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Items["ihpp"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(900) && !e.IsDead))
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && Bil.IsInRange(target))
                {
                    Bil.Cast(target);
                }
				
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }

                if (yous && Youmuu.IsReady() && Youmuu.IsOwned() && _Player.Distance(target) <= Player.Instance.GetAutoAttackRange() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (_Player.HasBuff("TwitchFullAutomatic"))
                    {
                        Youmuu.Cast();
                    }
                    else
                    {
                        if (_Player.Distance(target) <= 550)
                        {
                            Youmuu.Cast();
                        }
                    }
                }
            }
        }

        private static void Orbwalker_CantLasthit(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            var mana = LaneClearMenu["M"].Cast<Slider>().CurrentValue;
            var useE = LaneClearMenu["ELH"].Cast<CheckBox>().CurrentValue;
            var unit = (useE && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Player.Instance.ManaPercent >= mana);
            if (target == null) return;
            if (unit && E.IsReady() && E.IsInRange(target) && target.HasBuff("twitchdeadlyvenom"))
            {
                if (EDamage(target) >= Prediction.Health.GetPrediction(target, E.CastDelay))
                {
                    E.Cast();
                }
            }
        }

        private static void Flee()
        {
            var useQ = Misc["FleeQ"].Cast<CheckBox>().CurrentValue;
            var useW = Misc["FleeW"].Cast<CheckBox>().CurrentValue;
            if (useQ && Q.IsReady())
            {
                Q.Cast();
            }

            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(W.Range) && !e.IsDead))
            {
                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    var Pred = W.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.High)
                    {
                        W.Cast(Pred.CastPosition);
                    }
                }

            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && e.Sender.Distance(_Player) <= 300)
            {
                W.Cast(e.Sender);
            }
        }

        public static void LaneClear()
        {
            var mana = LaneClearMenu["M"].Cast<Slider>().CurrentValue;
            var useW = LaneClearMenu["W"].Cast<CheckBox>().CurrentValue;
            var MinW = LaneClearMenu["minW"].Cast<Slider>().CurrentValue;
            var useE = LaneClearMenu["E"].Cast<CheckBox>().CurrentValue;
            var minm = LaneClearMenu["Minm"].Cast<Slider>().CurrentValue;
            var MinE2 = LaneClearMenu["MinS"].Cast<Slider>().CurrentValue;
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position, E.Range).ToArray();
            var WCal = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(minions, W.Width, (int)W.Range);
            if (Player.Instance.ManaPercent < mana) return;
            if (minions != null)
            {
                if (useW && W.IsReady() && WCal.HitNumber >= MinW)
                {
                    W.Cast(WCal.CastPosition);
                }

                if (useE && E.IsReady())
                {
                    int ECal = minions.Where(e => e.Distance(_Player.Position) < (E.Range) && Stack(e) >= MinE2).Count(); ;
                    if (ECal >= minm)
                    {
                        E.Cast();
                    }
                }
            }
        }

        public static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var Mana = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var MinQ = HarassMenu["HminQ"].Cast<Slider>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var MinE = HarassMenu["HminE"].Cast<Slider>().CurrentValue;
            if (Player.Instance.ManaPercent <= Mana)
            {
                return;
            }

            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(E.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (_Player.Position.CountEnemiesInRange(900) >= MinQ)
                    {
                        Q.Cast();
                    }
                }

                if (useW && W.IsReady() && !QCasting && target.IsValidTarget(W.Range))
                {
                    var Wpred = W.GetPrediction(target);
                    if (Wpred.HitChance >= HitChance.Medium)
                    {
                        W.Cast(Wpred.CastPosition);
                    }
                }

                if (useE && E.IsReady() && E.IsInRange(target) && target.HasBuff("twitchdeadlyvenom"))
                {
                    if (HarassMenu["haras" + target.ChampionName].Cast<CheckBox>().CurrentValue && Stack(target) >= MinE)
                    {
                        E.Cast();
                    }
                }
            }
        }

        private static void Damage(EventArgs args)
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e.IsValid && e.IsHPBarRendered && e.TotalShieldHealth() > 10))
            {
                var damage = EDamage(enemy);
                if (Misc["Damage"].Cast<CheckBox>().CurrentValue && E.IsReady() && enemy.HasBuff("twitchdeadlyvenom"))
                {
                    var dmgPer = (enemy.TotalShieldHealth() - damage > 0 ? enemy.TotalShieldHealth() - damage : 0) / (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                    var currentHPPer = enemy.TotalShieldHealth() / enemy.TotalShieldMaxHealth();
                    var initPoint = new Vector2((int)(enemy.HPBarPosition.X + XOff + dmgPer * Width), (int)enemy.HPBarPosition.Y + YOff);
                    var endPoint = new Vector2((int)(enemy.HPBarPosition.X + XOff + currentHPPer * Width) + 1, (int)enemy.HPBarPosition.Y + YOff);
                    EloBuddy.SDK.Rendering.Line.DrawLine(System.Drawing.Color.Orange, Thick, initPoint, endPoint);
                }
            }
        }
        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical, SDamage[E.Level] * Stack(target) + (0.25f * _Player.FlatPhysicalDamageMod + 0.2f * _Player.FlatMagicDamageMod + BDamage[E.Level]));
        }


        public static float StackTimeDamage(Obj_AI_Base target)
        {
            float dmg = 0;
            if (!target.HasBuff("twitchdeadlyvenom")) return 0;

            if (Player.Instance.Level < 5)
            {
                dmg = 2;
            }
            if (Player.Instance.Level < 9)
            {
                dmg = 3;
            }
            if (Player.Instance.Level < 13)
            {
                dmg = 4;
            }
            if (Player.Instance.Level < 17)
            {
                dmg = 5;
            }
            if (Player.Instance.Level == 18)
            {
                dmg = 6;
            }
            return dmg * Stack(target) * StackTime(target) - target.HPRegenRate*StackTime(target);
        }

        private static int Stack(Obj_AI_Base target)
        {
            var Ec = 0;
            for (var t = 1; t < 7; t++)
            {
                if (ObjectManager.Get<Obj_GeneralParticleEmitter>().Any(s => s.Position.Distance(target.ServerPosition) <= 175 && s.Name == "twitch_poison_counter_0" + t + ".troy"))
                {
                    Ec = t;
                }
            }
            return Ec;
        }

        public static float StackTime(Obj_AI_Base target)
        {
            if (target.HasBuff("twitchdeadlyvenom"))
            {
                return Math.Max(0, target.GetBuff("twitchdeadlyvenom").EndTime) - Game.Time;
            }
            return 0;
        }

        public static void KillSteal()
        {
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(E.Range) && hero.HasBuff("twitchdeadlyvenom") && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage")))
            {
                if (KsE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (EDamage(target) + StackTimeDamage(target) >= target.TotalShieldHealth() || target.HealthPercent <= 10)
                    {
                        Player.CastSpell(SpellSlot.E);
                    }
                }
            }
        }

        public static void Escape()
        {
            var Eranh = Misc["E"].Cast<CheckBox>().CurrentValue;
            var Eranhs = Misc["ES"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuff("FioraW") && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.HasBuff("SpellShield") && !hero.HasBuff("NocturneShield") && hero.HasBuff("twitchdeadlyvenom") && !hero.IsDead && !hero.IsZombie))
            {
                if (Eranh && E.IsReady())
                {
                    if (Stack(target) >= Eranhs && _Player.Position.Distance(target) >= 1050)
                    {
                        E.Cast();
                    }
                }

                if (E.IsReady() && Player.Instance.HealthPercent <= 15)
                {
                    E.Cast();
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float jx, float jy, ColorBGRA jc)
        {
            vFont.DrawText(null, vText, (int)jx, (int)jy, jc);
        }
    }
}
