using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Battle.DiceAttackEffect;
using System.IO;

using System.Reflection;
using System.Xml.Serialization;
using LOR_DiceSystem;
using LOR_XML;
using Mod;
using UI;
using HarmonyLib;
using CustomMapUtility;
using Workshop;

namespace SweetlingOfRosette
{


    public class SweetModInitializer : ModInitializer
    {
        public override void OnInitializeMod()
        {
            base.OnInitializeMod();
            SweetModInitializer.path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
            Harmony harmony = new Harmony("LOR.coollection");            
            MethodInfo method = typeof(SweetModInitializer).GetMethod("BookModel_SetXmlInfo_Post");
            harmony.Patch(typeof(BookModel).GetMethod("SetXmlInfo", AccessTools.all), null, new HarmonyMethod(method), null, null, null);
            Harmony.CreateAndPatchAll(typeof(PatchList), base.GetType().Assembly.GetName().Name);

            SweetModInitializer.PreLoadBufIcons();
            SweetModInitializer.language = GlobalGameManager.Instance.CurrentOption.language;
            SweetModInitializer.Init = true;
            SweetModInitializer.AddLocalize();
        }

        public static void PreLoadBufIcons()
        {
            foreach (var baseGameIcon in Resources.LoadAll<Sprite>("Sprites/BufIconSheet/").Where(x => !BattleUnitBuf._bufIconDictionary.ContainsKey(x.name)))
                BattleUnitBuf._bufIconDictionary.Add(baseGameIcon.name, baseGameIcon);
            string bufIconDirectory = (SweetModInitializer.path + "/ArtWork/BufIcons");
            if (Directory.Exists(bufIconDirectory))
            {
                var path = new DirectoryInfo(bufIconDirectory);
                if (path != null)
                {
                    LoadSpritesIntoDict(path, BufIcons);
                    foreach (var y in BufIcons.Where(x => !BattleUnitBuf._bufIconDictionary.ContainsKey(x.Key)))
                    {
                        BattleUnitBuf._bufIconDictionary.Add(y.Key, y.Value);
                    }
                }
            }
        }
        private static void LoadSpritesIntoDict(DirectoryInfo path, Dictionary<string, Sprite> dict)
        {
            if (path != null && Directory.Exists(path.FullName))
                foreach (var file in path.GetFiles().Where(x => x.Extension == ".png"))
                {
                    if (!dict.ContainsKey(Path.GetFileNameWithoutExtension(file.FullName)))
                    {
                        Texture2D texture2D = new Texture2D(2, 2);
                        texture2D.LoadImage(File.ReadAllBytes(file.FullName));
                        Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
                        dict.Add(Path.GetFileNameWithoutExtension(file.FullName), sprite);
                    }
                }
        }

        public static void AddLocalize()
        {
            
            Dictionary<string, BattleEffectText> dictionary = typeof(BattleEffectTextsXmlList).GetField("_dictionary", AccessTools.all).GetValue(Singleton<BattleEffectTextsXmlList>.Instance) as Dictionary<string, BattleEffectText>;
            System.IO.FileInfo[] files = new DirectoryInfo(SweetModInitializer.path + "/Localize/" + SweetModInitializer.language + "/EffectTexts").GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                using (StringReader stringReader = new StringReader(File.ReadAllText(files[i].FullName)))
                {
                    BattleEffectTextRoot battleEffectTextRoot = (BattleEffectTextRoot)new XmlSerializer(typeof(BattleEffectTextRoot)).Deserialize(stringReader);
                    for (int j = 0; j < battleEffectTextRoot.effectTextList.Count; j++)
                    {
                        BattleEffectText battleEffectText = battleEffectTextRoot.effectTextList[j];
                        dictionary.Add(battleEffectText.ID, battleEffectText);
                    }
                }
            }
        }

        public static void RemoveError()
        {
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            list.Add("0Harmony");
            list.Add("Mono.Cecil");
            list.Add("MonoMod.Common");
            list.Add("MonoMod.RuntimeDetour");
            list.Add("MonoMod.Utils");
            list.Add("Mono.Cecil.Mdb");
            list.Add("Mono.Cecil.Pdb");

            using (List<string>.Enumerator enumerator = Singleton<ModContentManager>.Instance.GetErrorLogs().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string errorLog = enumerator.Current;
                    if (list.Exists((string x) => errorLog.Contains(x)))
                    {
                        list2.Add(errorLog);
                    }
                }
            }
            foreach (string item in list2)
            {
                Singleton<ModContentManager>.Instance.GetErrorLogs().Remove(item);
            }
        }

        public static void BookModel_SetXmlInfo_Post(BookModel __instance, BookXmlInfo ____classInfo, ref List<DiceCardXmlInfo> ____onlyCards)
        {
            if (__instance.BookId.packageId == SweetModInitializer.packageId)
            {
                foreach (int id in ____classInfo.EquipEffect.OnlyCard)
                {
                    DiceCardXmlInfo cardItem = ItemXmlDataList.instance.GetCardItem(new LorId(SweetModInitializer.packageId, id), false);
                    ____onlyCards.Add(cardItem);
                }
            }
        }

        public const string packageId = "SoR4000";
        public static string path;
        public static string language;
        public static bool Init;
        private static Dictionary<string, Sprite> BufIcons = new Dictionary<string, Sprite>();

        private class PatchList
        {

            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(BattleUnitView), "ChangeSkin")]
            public static void BattleUnitView_ChangeSkin(BattleUnitView __instance, string charName)
            {
                WorkshopSkinData workshopBookSkinData = Singleton<CustomizingBookSkinLoader>.Instance.GetWorkshopBookSkinData("SoR4000", charName);
                bool flag = workshopBookSkinData != null;
                if (flag)
                {
                    Dictionary<ActionDetail, ClothCustomizeData> dic = __instance.charAppearance.gameObject.GetComponent<WorkshopSkinDataSetter>().dic;
                    bool flag2 = dic == null || dic.Count == 0;
                    if (flag2)
                    {
                        __instance.charAppearance.gameObject.GetComponent<WorkshopSkinDataSetter>().SetData(workshopBookSkinData);
                    }
                }
            }
            
        }


    }

    public class PassiveAbility_buffetI : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            owner.bufListDetail.AddBuf(new SoR_buffet(1));
        }
    }

    public class PassiveAbility_buffetII : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            owner.bufListDetail.AddBuf(new SoR_buffet(2));
        }
    }

    public class PassiveAbility_buffetIII : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            owner.bufListDetail.AddBuf(new SoR_buffet(3));
        }
    }

    public class PassiveAbility_buffetIV : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            owner.bufListDetail.AddBuf(new SoR_buffet(4));
        }
    }

    public class PassiveAbility_buffetV : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            owner.bufListDetail.AddBuf(new SoR_buffet(5));
        }
    }

    public class PassiveAbility_buffetVII : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            owner.bufListDetail.AddBuf(new SoR_buffet(7));
        }
    }

    public class EnemyTeamStageManager_SoRrosetteFinale : EnemyTeamStageManager
    {
        /// <summary>
        /// in preparation for teleportation behaviours i could need to code and for handling music in combat
        /// </summary>
        /// 
        public override void OnWaveStart()
        {
            CustomMapHandler.InitCustomMap<SoR_mapManager_5>("SE_Stage");
            CustomMapHandler.InitCustomMap<SoR_mapManager_4>("Train");
        }
        public override void OnRoundStart()
        {
            base.OnRoundStart();
            

            CustomMapHandler.EnforceMap(0);

        }
    }

    public class SoR_mapManager_5 : CustomMapManager
    {
        protected internal override string[] CustomBGMs => new string[3]
        {
        "Game_night_loop_1.wav",
        "V2_Loop_2.wav",
        "Game_night_v1_edited.wav"
        };
    }

    public class EnemyTeamStageManager_sallyRealization : EnemyTeamStageManager
    {
        public enum phase
        {
            Promise,
            Queen,
            Grave,
            Sally,
            LoneSprout
        }

        private phase _currentPhase;

        public phase _publicPhase => _currentPhase;

        public int checkpoint = 0;

        //public phase currentPhase => _currentPhase;

        public override bool IsStageFinishable()
        {
            return _currentPhase >= phase.LoneSprout;
        }

        public override void OnWaveStart()
        {
            CustomMapHandler.InitCustomMap<SoR_mapManager_2>("Facility");
            //CustomMapHandler.InitCustomMap<SoR_mapManager_2>("Facility");
            CustomMapHandler.InitCustomMap<SoR_mapManager_3>("SE_Stage");
            CustomMapHandler.InitCustomMap<SoR_mapManager_4>("Train");
            CustomMapHandler.InitCustomMap<SoR_mapManager_4>("Train");
        }

        public override void OnRoundStart()
        {
            //enforcing the rightmap

            base.OnRoundStart();
            int num = 0;
            switch(_currentPhase)
            {
                case phase.Promise:
                    num = 0;
                    break;
                case phase.Queen:
                    num = 0;
                    break;
                case phase.Grave:
                    num = 1;
                    break;
                case phase.Sally:
                    num = 2;
                    break;
                case phase.LoneSprout:
                    num = 3;
                    break;
            }
            
            CustomMapHandler.EnforceMap(num);
        }

        public override void OnRoundEndTheLast()
        {
            CheckPhase();

        }

        private void CheckPhase()
        {
            if (BattleObjectManager.instance.GetAliveList(Faction.Enemy).Count <= 0)//(!BattleObjectManager.get_instance().GetAliveList((Faction)0).Exists((BattleUnitModel x) => x.index == 0))
            {
                SetNewPhase(_currentPhase);
            }
        }

        private void SetNewPhase(phase phaseEnum)
        {
            UnregisterAllUnit();

            switch (phaseEnum)
            {
                case phase.Promise:
                    CreateQueen();
                     _currentPhase = phase.Queen;                    
                    break;
                case phase.Queen:
                    CreateGrave();
                    _currentPhase = phase.Grave;
                    break;
                case phase.Grave:
                    CreateSally();
                   _currentPhase = phase.Sally;
                    break;
                case phase.Sally:
                    CreateLoneSprout();
                    _currentPhase = phase.LoneSprout;
                    break;
                case phase.LoneSprout:
                    break;
            }
            //checkpoint++;

            BattleObjectManager.instance.GetAliveList(Faction.Player).ForEach(delegate (BattleUnitModel x)
            {
                if (x.hp <= 90f)
                {
                    float num = Mathf.Clamp(90f - x.hp, 0f, 10f);
                    x.RecoverHP((int)num);
                }
                int num2 = 0;
                foreach (BattleUnitModel item in BattleObjectManager.instance.GetList())
                {
                    SingletonBehavior<UICharacterRenderer>.Instance.SetCharacter(item.UnitData.unitData, num2++, true, false);
                }
                BattleObjectManager.instance.InitUI();
            });

        }

        private void CreateQueen()
        {        
            Singleton<StageController>.Instance.AddNewUnit(Faction.Enemy, new LorId("SoR4000", 11), 0, -1);
            foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList((Faction)0))
            {
                alive.passiveDetail.OnWaveStart();
            }
        }

        private void CreateGrave()
        {
            //battleUnitModel.passiveDetail.AddPassive(new LorId("SoR4000", 17)); //this line in case you want to add passives in a character
            BattleUnitModel battleUnitModel = Singleton<StageController>.Instance.AddNewUnit(Faction.Enemy, new LorId("SoR4000", 12), 0, -1);           
            foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList((Faction)0))
            {
                alive.passiveDetail.OnWaveStart();
            }
        }

        private void CreateSally()
        {
            Singleton<StageController>.Instance.AddNewUnit(Faction.Enemy, new LorId("SoR4000", 13), 0, -1);
            foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList((Faction)0))
            {
                alive.passiveDetail.OnWaveStart();
            }
        }

        private void CreateLoneSprout()
        {
            Singleton<StageController>.Instance.AddNewUnit(Faction.Enemy, new LorId("SoR4000", 14), 0, -1);
            foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList((Faction)0))
            {
                alive.passiveDetail.OnWaveStart();
            }
        }

        private void UnregisterAllUnit()
        {
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(Faction.Enemy))
            {
                if (!((BattleUnitBaseModel)item).IsDead())
                {
                    item.Die((BattleUnitModel)null, true);
                }
                item.isRegister = false;
            }
        }

    }
    public class PassiveAbility_limeRecycle : PassiveAbilityBase
    {
        public override void OnRoundEndTheLast()
        {
            base.OnRoundEndTheLast();
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetList(Faction.Enemy))
            {
                if (item != owner && item.IsDead())
                {
                    //AddNewUnit(the team, new LorID("packageid of the mod", id of the custom unit), position index of the unit, size)
                    Singleton<StageController>.Instance.AddNewUnit(item.faction, new LorId("SoR4000", 5), item.index, -1);
                }
            }
        }

    }

    public class PassiveAbility_limeRecycleInfused : PassiveAbilityBase
    {
        public override void OnRoundEndTheLast()
        {
            //fetch owner's buff
            SoR_armor numArmor = owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_armor) as SoR_armor;
            SoR_wellfed numWell = owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

            

            base.OnRoundEndTheLast();
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetList(Faction.Enemy))
            {
                if (item != owner && item.IsDead())
                {
                    //AddNewUnit(the team, new LorID("packageid of the mod", id of the custom unit), position index of the unit, size)
                    BattleUnitModel newMinion = Singleton<StageController>.Instance.AddNewUnit(item.faction, new LorId("SoR4000", 23), item.index, -1);
                    SoR_armor numArmorX = newMinion.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_armor) as SoR_armor;
                    SoR_wellfed numWellX = newMinion.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

                    if (numArmorX == null)
                    {
                        item.bufListDetail.AddBuf(new SoR_armor(numArmor.stack));
                    }
                    else
                    {
                        numArmorX.addStacks(numArmor.stack);
                    }

                    if (numWellX == null)
                    {
                        item.bufListDetail.AddBuf(new SoR_wellfed(numWell.stack));
                    }
                    else
                    {
                        numWellX.addStacks(numWell.stack);
                    }

                    //newMinion.passiveDetail.AddPassive(new PassiveAbility_10001());//add speed
                }
            }
        }

    }

    public class PassiveAbility_playDate : PassiveAbilityBase
    {
        
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            CustomMapHandler.InitCustomMap<SoR_mapManager>("SE_Stage");
        }

        public override void OnRoundStart()
        {
            base.OnRoundStart();
            CustomMapHandler.EnforceMap(0);
        }

        //mint's way to end the battle early
        public override void OnRoundEnd()
        {
            //end battle at half hp
            base.OnRoundEnd();

            if (owner.hp <= (owner.MaxHp/2))
            {
                owner.DieFake();
            }
        }
    }

    public class SoR_mapManager : CustomMapManager
    {
        protected internal override string[] CustomBGMs => new string[1]
        {
        "TwoFacedHost.wav"
        };
    }

    public class PassiveAbility_AfterCare : PassiveAbilityBase
    {
        //mint special attack against staggered characters
        public override void OnRoundStart()
        {
            base.OnRoundStart();
            if (owner.IsBreakLifeZero())
            {
                return;
            }
            bool flag = false;
            foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList(Faction.Player))
            {
                if (alive.IsBreakLifeZero())
                {
                    
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 24)); //change for right card?
                battleDiceCardModel.temporary = true;
                battleDiceCardModel.exhaust = true;
                
            }
        }

        public override BattleUnitModel ChangeAttackTarget(BattleDiceCardModel card, int idx)
        {
            //code to retarget to stunned enemies
            //also  170203 to avoid sending all attacks to 1 target
            //idx is the dice being used
            //check if card used is "aftercare" for the code before retargetting use number above for references

            if (card.GetID() == new LorId("SoR4000", 24))//change for good card
            {
                foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList(Faction.Player))
                {
                    if (alive.IsBreakLifeZero())
                    {
                        return alive;
                    }
                }
            }

            return base.ChangeAttackTarget(card, idx);
        }
    }

    public class PassiveAbility_sweetAroma : PassiveAbilityBase
    {

        //afflict aroma when hit
        //will need custom frostbite version later

        /*
        public override void OnTakeDamageByAttack(BattleDiceBehavior atkDice, int dmg)
        {
            base.OnTakeDamageByAttack(atkDice, dmg);
        }*/

        public override void AfterTakeDamage(BattleUnitModel attacker, int dmg)
        {
            base.AfterTakeDamage(attacker, dmg);

            //provide 5 stack of aroma to all enemies
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(attacker.faction, 5))
            {

                item.bufListDetail.AddKeywordBufByCard(KeywordBuf.Alriune_Debuf, 5, base.owner);

            }
        }
    }

    public class PassiveAbility_quick : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            owner.bufListDetail.AddBuf(new SoR_quick(1));
        }
    }

    public class PassiveAbility_SoR_Corrosion_1 : PassiveAbilityBase
    {
        //is in charge of transforming sally and changing the scene as needed
        private bool _progress = false;
        private int _phase = 0;

        public override bool BeforeTakeDamage(BattleUnitModel attacker, int dmg)
        {
            
            if (dmg > owner.hp)
            {
                owner.SetHp(owner.MaxHp);
                Singleton<StageController>.Instance.RoundEndForcely();
                _progress = true;                
            }
            else
            {
                return base.BeforeTakeDamage(attacker, dmg);
            }

            return false; //could be bad?
        }

        
        
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            //CustomMapHandler.InitCustomMap<SoR_mapManager_2>("Facility");
        }

        public override void OnRoundStart()
        {
            
            base.OnRoundStart();
            //CustomMapHandler.EnforceMap();
        }
        
        
        //old, system that does not work
        /*
        public override void OnRoundEndTheLast_ignoreDead()
        {
            base.OnRoundEndTheLast_ignoreDead();

            if (_progress)
            {
                if (!owner.IsDead())
                {
                    owner.Die();
                }
                Singleton<StageController>.Instance.AddNewUnit(owner.faction, new LorId("SoR4000", 11), owner.index, 0);
                
            }

        }
        */

        public void ChangePhase()
        {
            switch(_phase)
            {
                //go to queen
                case 0:
                    return;
                // go to grave of cherry blossom
                case 1:
                    return;
                //go to 
            }
        }


    }

    public class SoR_mapManager_2 : CustomMapManager
    {
        protected internal override string[] CustomBGMs => new string[1]
        {
        "Game_night_loop_1.wav"
        };
    }

    public class SoR_mapManager_3 : CustomMapManager
    {
        protected internal override string[] CustomBGMs => new string[1]
        {
        "V2_Loop_2.wav"
        };
    }

    public class SoR_mapManager_4 : CustomMapManager
    {
        protected internal override string[] CustomBGMs => new string[1]
        {
        "V2_Loop_3.wav"
        };
    }

    public class PassiveAbility_SoR_Corrosion_2 : PassiveAbilityBase
    {
        //is in charge of transforming sally and changing the scene as needed

        public override bool isInvincibleHp => false; //if all fail, use this to make a stagger based battle

        public override bool BeforeTakeDamage(BattleUnitModel attacker, int dmg)
        {

            if (dmg > owner.hp)
            {
                owner.SetHp(1);
                _progress = true;
            }
            else
            {
                return base.BeforeTakeDamage(attacker, dmg);
            }

            return false; //could be bad?
        }

        private bool _progress = false;

        public override void OnWaveStart()
        {
            base.OnWaveStart();
            CustomMapHandler.InitCustomMap<SoR_mapManager_2>("SE_Stage");
        }

        public override void OnRoundStart()
        {

            base.OnRoundStart();
            CustomMapHandler.EnforceMap();
        }




        public override void OnRoundEndTheLast_ignoreDead()
        {
            base.OnRoundEndTheLast_ignoreDead();

            if (_progress)
            {
                if (!owner.IsDead())
                {
                    owner.Die();
                }
                Singleton<StageController>.Instance.AddNewUnit(owner.faction, new LorId("SoR4000", 12), owner.index, 0);

            }

        }
    }
    public class PassiveAbility_Ancientpractice : PassiveAbilityBase
    {
        //behaviour modification

        private int _safety = 1;

        public override int SpeedDiceNumAdder()
        {
            return 5;
        }

        public override void OnRoundStart()
        {
            base.OnRoundStart();

            //
            if (_safety < 4)
            {
                
                BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 28)); 
                battleDiceCardModel.temporary = true;
                battleDiceCardModel.exhaust = true;
            }
            else
            {
                
                BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 29)); 
                battleDiceCardModel.temporary = true;
                battleDiceCardModel.exhaust = true;
            }

            //mass attack
            if (_safety % 3 == 0 && _safety != 0)
            {
                BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 30)); 
                battleDiceCardModel.temporary = true;
                battleDiceCardModel.exhaust = true;
            }
            _safety++;
            // loop to generate proper cards until hand is "full"
            while (owner.allyCardDetail.GetHand().Count < 6)
            {
                int id = UnityEngine.Random.Range(31, 34);

                BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", id));
                battleDiceCardModel.temporary = true;
                battleDiceCardModel.exhaust = true;
            }
        }
        
    }


    public class PassiveAbility_austereheart : PassiveAbilityBase
    {
        //produce the passive to negating ancient practice effect

        //reduce the passive at the end of the turn
    }

    public class PassiveAbility_betrayedheart : PassiveAbilityBase
    {
        //empty, used for a description
    }

    public class PassiveAbility_myWhim : PassiveAbilityBase
    {
        //behaviour modification


        public override int SpeedDiceNumAdder()
        {
            return 5;
        }

        public override void OnRoundStart()
        {
            base.OnRoundStart();

            
            foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList(Faction.Player))
            {
                SoR_disobey numDis = alive.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_disobey) as SoR_disobey;
                //new code that add bonus stacks if not null
                if (numDis != null)
                {
                    if (numDis.stack >= 5)
                    {
                        BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 34));
                        battleDiceCardModel.temporary = true;
                        battleDiceCardModel.exhaust = true;
                    }
                    
                }
                
            }
            
            

            // loop to generate proper cards until hand is "full"
            while (owner.allyCardDetail.GetHand().Count < 6)
            {
                int id = UnityEngine.Random.Range(35, 39);

                BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", id));
                battleDiceCardModel.temporary = true;
                battleDiceCardModel.exhaust = true;
            }
        }

        public override BattleUnitModel ChangeAttackTarget(BattleDiceCardModel card, int idx)
        {
            //code to retarget to stunned enemies
            //also  170203 to avoid sending all attacks to 1 target
            //idx is the dice being used
            //check if card used is the right one for the code before retargetting use number above for references

            if (card.GetID() == new LorId("SoR4000", 34))//change for good card
            {

                foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList_random(Faction.Player, 5))
                {
                    SoR_disobey disobey = alive.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_disobey) as SoR_disobey;

                    if (disobey != null && disobey.stack >= 5)
                    {
                        return alive;
                    }
                }
            }

            return base.ChangeAttackTarget(card, idx);
        }

    }

    public class PassiveAbility_myTarts : PassiveAbilityBase
    {
        //end of round, add disobedience to people with well fed

        public override void OnRoundEnd()
        {
            base.OnRoundEnd();
            foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList_random(Faction.Player, 5))
            {
                SoR_wellfed numWell = alive.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

                if (numWell != null && numWell.stack >= 3)
                {
                    SoR_disobey disobey = alive.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_disobey) as SoR_disobey;

                    if (disobey == null)
                    {
                        alive.bufListDetail.AddBuf(new SoR_disobey(1));

                    }
                    else
                    {
                        disobey.addStacks(1);
                    }
                }
          
            }
        }
    }

    public class PassiveAbility_behead : PassiveAbilityBase
    {
        //empty, used for a description
    }

    public class PassiveAbility_bloom : PassiveAbilityBase
    {
        public override int SpeedDiceNumAdder()
        {
            return 5;
        }

        public override void OnWaveStart()
        {
            base.OnWaveStart();
            owner.SetHp(owner.MaxHp / 2);
        }

        public override void OnRoundStart()
        {
            base.OnRoundStart();
            if (owner.hp >= owner.MaxHp)
            {
                BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 42)); //change for mass attack card
                battleDiceCardModel.temporary = true;
                battleDiceCardModel.exhaust = true;
            }

            // loop to generate proper cards until hand is "full"
            while (owner.allyCardDetail.GetHand().Count < 6)
            {
                int id = UnityEngine.Random.Range(39, 42); //change ids for proper cards

                BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", id));
                battleDiceCardModel.temporary = true;
                battleDiceCardModel.exhaust = true;
            }
        }
    }

    public class PassiveAbility_bonzai : PassiveAbilityBase
    {
        public override void OnRoundEnd()
        {
            base.OnRoundEnd();

            if (owner.IsBreakLifeZero())
            {
                owner.Die();
            }
        }
    }

    public class PassiveAbility_bloodHarvest : PassiveAbilityBase
    {
        public override void OnDieOtherUnit(BattleUnitModel unit)
        {
            base.OnDieOtherUnit(unit);
            //owner.TakeDamage(owner.MaxHp / 2);

            foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList_random(Faction.Player, 5))
            {
                alive.RecoverHP(alive.MaxHp);
                alive.RecoverBreakLife(alive.MaxBreakLife);
            }
        }
    }

    public class PassiveAbility_SoR_seasons : PassiveAbilityBase
    {
        //behaviour modification

        private int _cycle = 0;
        private int massId = 0;

        public override int SpeedDiceNumAdder()
        {
            return 2;
        }

        public override void OnRoundStart()
        {
            base.OnRoundStart();

            switch(_cycle)
            {
                case 0:
                    //spring                    
                    massId = 52;
                    break;
                case 1:
                    //summer                   
                    massId = 51;                    
                    break;
                case 2:
                    //fall                   
                    massId = 48;
                    break;
                case 3:
                    //winter                    
                    massId = 50;
                    break;
                default:
                    //spring                    
                    massId = 49;
                    break;
            }
            // produce cards

            BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", massId));
            battleDiceCardModel.temporary = true;
            battleDiceCardModel.exhaust = true;

            BattleDiceCardModel battleDiceCardModel_2 = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 49));
            battleDiceCardModel_2.temporary = true;
            battleDiceCardModel_2.exhaust = true;

            BattleDiceCardModel battleDiceCardModel_3 = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 49));
            battleDiceCardModel_3.temporary = true;
            battleDiceCardModel_3.exhaust = true;

            if (_cycle >= 3)
            {
                _cycle = 0;
            }
            else
            {
                _cycle++;
            }
        }

    }

    public class PassiveAbility_harshEnviro : PassiveAbilityBase
    {
        //bleed does not work for transfer from one entity to another
        /*
        public override void OnRoundEnd()
        {
            base.OnRoundEnd();
            BattleUnitBuf owner_bleed = owner.bufListDetail.GetActivatedBuf(KeywordBuf.Bleeding);
            BattleUnitBuf future_bleed = owner.bufListDetail.GetReadyBuf(KeywordBuf.Bleeding);

            //fetching these is quite troublesome, as if one return null, other arguments are softlocked, so individual if statement needed
            //fetching status being readyied
            int total_owner_bleed = 0;
            if (future_bleed != null)
            {
                total_owner_bleed += future_bleed.stack;
            }

            if (owner_bleed != null)
            {
                total_owner_bleed += owner_bleed.stack;
            }

            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList())
            {
                BattleUnitBuf item_bleed = item.bufListDetail.GetActivatedBuf(KeywordBuf.Bleeding);
                BattleUnitBuf future_item_bleed = owner.bufListDetail.GetReadyBuf(KeywordBuf.Bleeding);

                if (total_owner_bleed > 0)
                {
                    int total_item_bleed = 0;
                    if (future_item_bleed != null)
                    {
                        total_item_bleed += future_item_bleed.stack;
                    }

                    if (item_bleed != null)
                    {
                        total_item_bleed += item_bleed.stack;
                    }

                    //got to update both current and future bleed
                    if (total_item_bleed < total_owner_bleed)
                    {
                        item_bleed.stack = owner_bleed.stack;
                        future_item_bleed.stack = future_bleed.stack;

                    }

                }
            }
        }*/

        public override void OnRoundStart()
        {
            base.OnRoundStart();

            
            BattleUnitBuf owner_burn = owner.bufListDetail.GetActivatedBuf(KeywordBuf.Burn);
            BattleUnitBuf owner_fairy = owner.bufListDetail.GetActivatedBuf(KeywordBuf.Fairy);
            SoR_frostbite numfrostbite_owner = owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_frostbite) as SoR_frostbite;

            

            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList())
            {
                
                BattleUnitBuf item_burn = item.bufListDetail.GetActivatedBuf(KeywordBuf.Burn);
                BattleUnitBuf item_fairy = item.bufListDetail.GetActivatedBuf(KeywordBuf.Fairy);
                SoR_frostbite numfrostbite_item = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_frostbite) as SoR_frostbite;

                

                //set status effect to be equal to itself if status is lower than itself's
                if (owner_burn != null)
                {
                    if (owner_burn.stack > 0)
                    {
                        if (item_burn == null)
                        {
                            item.bufListDetail.AddKeywordBufThisRoundByCard(KeywordBuf.Burn, owner_burn.stack);
                        }
                        else if (item_burn.stack < owner_burn.stack)
                        {
                            item_burn.stack = owner_burn.stack;
                        }
                    }

                }

                if (owner_fairy != null)
                {
                    if (owner_fairy.stack > 0)
                    {
                        if (item_fairy == null)
                        {
                            item.bufListDetail.AddKeywordBufThisRoundByCard(KeywordBuf.Fairy, owner_fairy.stack);
                        }
                        else if (item_fairy.stack < owner_fairy.stack)
                        {
                            item_fairy.stack = owner_fairy.stack;
                        }
                    }

                }

                if (numfrostbite_owner != null)
                {
                    if (numfrostbite_owner.stack > 0)
                    {
                        if (numfrostbite_item == null)
                        {
                            numfrostbite_item.addStacks(numfrostbite_owner.stack);
                        }
                        else if (numfrostbite_item.stack < numfrostbite_owner.stack)
                        {
                            numfrostbite_item.setStacks(numfrostbite_owner.stack);
                        }
                    }

                }

            }
        }
    }

    public class PassiveAbility_SoR_cookNewUnit : PassiveAbilityBase
    {
         //original attempt at creating a unit on kill
        public override void OnKill(BattleUnitModel target)
        {
            base.OnKill(target);

            List<BattleUnitModel> list = BattleObjectManager.instance.GetList(owner.faction);
            bool hasCook = false;
            foreach (BattleUnitModel item in list)
            {
                if (item.IsDead() && !hasCook)
                {
                    hasCook = true;
                    BattleUnitModel battleUnitModel = Singleton<StageController>.Instance.AddNewUnit(Faction.Enemy, new LorId("SoR4000", 17), item.index);
                }
                
            }
            
            
        }

        /*
        public override void OnDieOtherUnit(BattleUnitModel unit)
        {
            base.OnDieOtherUnit(unit);

            owner.bufListDetail.AddBuf(new SoR_buffet(1)); //debug for trigger

            if (unit.faction == Faction.Player)
            {
                owner.bufListDetail.AddBuf(new SoR_buffet(1)); //debug for activation
                bool hasCook = false;
                foreach (BattleUnitModel item in BattleObjectManager.instance.GetList(Faction.Enemy))
                {
                    if (item.IsDead() && !hasCook)
                    {
                        hasCook = true;
                        BattleUnitModel battleUnitModel = Singleton<StageController>.Instance.AddNewUnit(Faction.Enemy, new LorId("SoR4000", 17), item.index);
                    }

                }
            }
        }

        */
    }

    public class PassiveAbility_SoR_HealStartOfCombat : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(Faction.Enemy))
            {
                item.RecoverHP(100);
            }
        }

    }

    public class PassiveAbility_SoR_RemoveAllDebuffFromTeam : PassiveAbilityBase
    {
        public override void OnRoundEnd()
        {
            base.OnRoundEnd();

            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(Faction.Enemy))
            {
                item.bufListDetail.RemoveBufAll(BufPositiveType.Negative);
            }
        }

    }

    public class PassiveAbility_SoR_buffetUpEndTurn : PassiveAbilityBase
    {
        public override void OnRoundEnd()
        {
            base.OnRoundEnd();

            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(Faction.Enemy))
            {
                SoR_buffet numBuffet = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;

                if (numBuffet == null)
                {
                    item.bufListDetail.AddBuf(new SoR_buffet(1));
                }
                else
                {
                    numBuffet.addStacks(1);
                }

            }
        }

    }

    public class PassiveAbility_massFrostbite : PassiveAbilityBase
    {
        public override void OnRoundStart()
        {
            base.OnRoundStart();
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(Faction.Player))
            {
                SoR_frostbite numFrost = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_frostbite) as SoR_frostbite;

                if (numFrost == null)
                {
                    item.bufListDetail.AddBuf(new SoR_frostbite(1));
                }
                else
                {
                    numFrost.addStacks(1);
                }

            }
        }
    }

    public class PassiveAbility_frosbiteRetaliate : PassiveAbilityBase
    {

        

        public override void AfterTakeDamage(BattleUnitModel attacker, int dmg)
        {
            base.AfterTakeDamage(attacker, dmg);

            //provide 3 stack of frostbite to all enemies
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(attacker.faction, 5))
            {

                SoR_frostbite numFrost = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_frostbite) as SoR_frostbite;

                if (numFrost == null)
                {
                    item.bufListDetail.AddBuf(new SoR_frostbite(1));
                }
                else
                {
                    numFrost.addStacks(1);
                }

            }
        }
    }
  
    public class PassiveAbility_armorOnAllyDeath : PassiveAbilityBase
    {

        public override void OnDieOtherUnit(BattleUnitModel unit)
        {
            base.OnDieOtherUnit(unit);

            SoR_armor numArmor = owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_armor) as SoR_armor;
            if (unit.faction == owner.faction)
            {
                if (numArmor == null)
                {
                    owner.bufListDetail.AddBuf(new SoR_armor(10));
                }
                else
                {
                    numArmor.addStacks(10);
                }
            }
        }

    }

    public class PassiveAbility_wellfedOnAllyDeath : PassiveAbilityBase
    {

        public override void OnDieOtherUnit(BattleUnitModel unit)
        {
            base.OnDieOtherUnit(unit);

            SoR_wellfed numWell = owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;
            if (unit.faction == owner.faction)
            {               
                if (numWell == null)
                {
                    owner.bufListDetail.AddBuf(new SoR_wellfed(3));
                }
                else
                {
                    numWell.addStacks(3);
                }
            }
        }

    }

    public class PassiveAbility_ligtAndCardRegen : PassiveAbilityBase
    {

        public override void OnRoundStart()
        {
            base.OnRoundStart();

            owner.allyCardDetail.DrawCards(1);
            owner.cardSlotDetail.RecoverPlayPoint(1);
        }

    }

    public class PassiveAbility_hostRosette : PassiveAbilityBase
    {
        //add two sap at the start of battle

        public override void OnWaveStart()
        {
            base.OnWaveStart();

            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(Faction.Player, 5))
            {

                SoR_sap numSap = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_sap) as SoR_sap;

                if (numSap == null)
                {
                    item.bufListDetail.AddBuf(new SoR_sap(2));
                }
                else
                {
                    numSap.addStacks(2);
                }

            }
        }

    }

    public class PassiveAbility_bruteRosette : PassiveAbilityBase
    {
        //at end of turn, consume a stack of buffet for 1 stack of ego enhancement.
       

        public override void OnRoundStart()
        {
            base.OnRoundStart();
            SoR_buffet numBuffet = owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;

            if (numBuffet != null)
            {
                SoR_egobuf numBoost = owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_egobuf) as SoR_egobuf;
                //new code that add bonus stacks if not null
                if (numBoost == null)
                {
                    owner.bufListDetail.AddBuf(new SoR_egobuf(1));
                }
                else
                {
                    numBoost.addStacks(1);
                }
                //--stack
                numBuffet.consumeStack();
            }
        }

    }

    public class PassiveAbility_nurturerRosette : PassiveAbilityBase
    {
        //add two sap at the start of battle

        public override void OnRoundStart()
        {
            base.OnRoundStart();

            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(owner.faction, 5))
            {

                SoR_wellfed numWellfed = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

                if (numWellfed == null)
                {
                    item.bufListDetail.AddBuf(new SoR_wellfed(1));
                }
                else
                {
                    numWellfed.addStacks(1);
                }

            }
        }

    }

    public class PassiveAbility_EmergencyFloorSwitch_1 : PassiveAbilityBase
    {
        //teleport if on geb/binah's floor

        public override void OnRoundEnd()
        {
            base.OnRoundEnd();

            SephirahType currentSephirah = Singleton<StageController>.Instance.CurrentFloor;

            if ((currentSephirah == SephirahType.Gebura || currentSephirah == SephirahType.Binah) && owner.UnitData.floorBattleData.param1 <= 0)
            {
                owner.UnitData.floorBattleData.param1 = 1;
                List<StageLibraryFloorModel> availableFloorList = Singleton<StageController>.Instance.GetStageModel().GetAvailableFloorList();
                availableFloorList.RemoveAll((StageLibraryFloorModel x) => x.Sephirah == Singleton<StageController>.Instance.CurrentFloor);
                if (availableFloorList.Count > 0)
                {
                    Singleton<StageController>.Instance.ChangeFloorForcely(availableFloorList[0].Sephirah, owner); // theory, returns Malkuth Or Roland
                }
            }
        }

    }

    public class PassiveAbility_EmergencyFloorSwitch_2 : PassiveAbilityBase
    {
        //teleport at low hp

        public override void OnRoundEnd()
        {
            base.OnRoundEnd();

            if (owner.hp < 40 && owner.UnitData.floorBattleData.param1 <= 2)
            {
                owner.UnitData.floorBattleData.param1 = 3;
                List<StageLibraryFloorModel> availableFloorList = Singleton<StageController>.Instance.GetStageModel().GetAvailableFloorList();
                availableFloorList.RemoveAll((StageLibraryFloorModel x) => x.Sephirah == Singleton<StageController>.Instance.CurrentFloor);
                if (availableFloorList.Count > 0)
                {
                    Singleton<StageController>.Instance.ChangeFloorForcely(availableFloorList[0].Sephirah, owner); // theory, returns Malkuth Or Roland
                }
            }
        }

    }

    public class PassiveAbility_EmergencyFloorSwitch_3 : PassiveAbilityBase
    {
        //teleport if on geb/hod's floor

        public override void OnRoundEnd()
        {
            base.OnRoundEnd();

            SephirahType currentSephirah = Singleton<StageController>.Instance.CurrentFloor;

            if ((currentSephirah == SephirahType.Gebura || currentSephirah == SephirahType.Hod) && owner.UnitData.floorBattleData.param1 <= 1)
            {
                owner.UnitData.floorBattleData.param1 = 2;
                List<StageLibraryFloorModel> availableFloorList = Singleton<StageController>.Instance.GetStageModel().GetAvailableFloorList();
                availableFloorList.RemoveAll((StageLibraryFloorModel x) => x.Sephirah == Singleton<StageController>.Instance.CurrentFloor);
                if (availableFloorList.Count > 0)
                {
                    Singleton<StageController>.Instance.ChangeFloorForcely(availableFloorList[0].Sephirah, owner); // theory, returns Malkuth Or Roland
                }
            }
        }

    }

    public class PassiveAbility_forestOfEverGreen : PassiveAbilityBase
    {
        //sally main passives
        int deadAlly = 0;
        bool egoActive = false;

        public override int SpeedDiceNumAdder()
        {
            return (owner.emotionDetail.EmotionLevel)+ 2;
        }

        public override void OnWaveStart()
        {
            base.OnWaveStart();

            //CustomMapHandler.InitCustomMap<SoR_mapManager_5>("SE_Stage");
            //CustomMapHandler.InitCustomMap<SoR_mapManager_4>("Train");

            owner.allyCardDetail.SetMaxHand(10);
            owner.allyCardDetail.SetMaxDrawHand(10);


            deadAlly = owner.UnitData.floorBattleData.param3;

            if (deadAlly >= 2)
            {
                egoActive = true;
                ChangeDeck();
            }
        }

        public override void OnDieOtherUnit(BattleUnitModel unit)
        {
            base.OnDieOtherUnit(unit);

            if (unit.faction == owner.faction)
            {
                deadAlly++;
                owner.UnitData.floorBattleData.param3++;
            }

            if (deadAlly == 2)
            {
                egoActive = true;

                ChangeDeck();
            }
        }

        public void ChangeDeck()
        {
            //remove previous deck
            owner.allyCardDetail.ExhaustAllCards();

            //produce new deck
            for (int i = 0; i < 3; i++)
            {
                owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 68));
            }
            for (int i = 0; i < 2; i++)
            {
                owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 69));
                owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 70));
            }
            owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 71));
            owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 72));
            owner.allyCardDetail.AddNewCard(new LorId("SoR4000", 73));
            //owner.allyCardDetail.AddNewCard(new LorId("SoR4000", id));

            //change appearance
            base.owner.view.ChangeSkin("evergreen");//try to fetch from main data base and not the mod.//harmony fix battleview_changeskin to fix the fetching issue
            
        }

        public override void OnRoundStart()
        {
            base.OnRoundStart();

            owner.allyCardDetail.DiscardInHand(8);
            owner.allyCardDetail.DrawCards((owner.emotionDetail.EmotionLevel) + 2);
            owner.cardSlotDetail.RecoverPlayPoint(owner.emotionDetail.EmotionLevel);

            if (egoActive)
            {
                //CustomMapHandler.EnforceMap(1);
                giveProtection();

                if (deadAlly >= 4)
                {
                    ProduceMassAttack();
                }

            }
            else
            {
                //CustomMapHandler.EnforceMap(0);
            }

            
        }

        public void giveProtection()
        {
            //give defensive boost to others before self.
            if (deadAlly >= 4)
            {
                //defense to self
                owner.bufListDetail.AddKeywordBufThisRoundByEtc(KeywordBuf.Protection, 5);
            }
            else
            {
                //defense to allies
                foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(owner.faction))
                {
                    if (owner != item)
                    {
                        item.bufListDetail.AddKeywordBufThisRoundByEtc(KeywordBuf.Protection, 5);
                    }
                    else
                    {
                        item.bufListDetail.AddKeywordBufThisRoundByEtc(KeywordBuf.Protection, 2);
                    }
                }
            }
        }

        public void ProduceMassAttack()
        {
            int massId = 0;
            int cycle = UnityEngine.Random.Range(0, 4); ;

            switch (cycle)
            {
                case 0:
                    //spring                    
                    massId = 52;
                    break;
                case 1:
                    //summer                   
                    massId = 51;
                    break;
                case 2:
                    //fall                   
                    massId = 48;
                    break;
                case 3:
                    //winter                    
                    massId = 50;
                    break;
                default:                   
                    massId = 49;
                    break;
            }
            // produce cards

            BattleDiceCardModel battleDiceCardModel = owner.allyCardDetail.AddNewCard(new LorId("SoR4000", massId));
            battleDiceCardModel.temporary = true;
            battleDiceCardModel.exhaust = true;
        }

    }

    public class PassiveAbility_RosetteFloorSwitch : PassiveAbilityBase
    {

        private int numTurn = 0;

        

        public override void OnRoundEnd()
        {
            base.OnRoundEnd();

            SephirahType currentSephirah = Singleton<StageController>.Instance.CurrentFloor;

            if (numTurn >= 2 && owner.UnitData.floorBattleData.param2 < 3)
            {
                owner.UnitData.floorBattleData.param2++;
                List<StageLibraryFloorModel> availableFloorList = Singleton<StageController>.Instance.GetStageModel().GetAvailableFloorList();
                availableFloorList.RemoveAll((StageLibraryFloorModel x) => x.Sephirah == Singleton<StageController>.Instance.CurrentFloor);
                if (availableFloorList.Count > 0)
                {
                    SephirahType sephirah = RandomUtil.SelectOne(availableFloorList).Sephirah;
                    Singleton<StageController>.Instance.ChangeFloorForcely(sephirah, owner);
                }
            }
            numTurn++;
        }

    }

    public class PassiveAbility_RosetteSetDeck : PassiveAbilityBase
    {

        public override void OnWaveStart()
        {
            owner.allyCardDetail.DrawCards(8); //draw full deck for future discard.

            base.OnWaveStart();
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 2));//sweetling
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 6));//sourling
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 7));//delivery boy
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 13));
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 16));
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 19));//butler
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 23));//sherberus
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 27));//Mint
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 47));
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 46));
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 55));//pyretato
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 57));
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 58));//cook
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 60));
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 61));
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 62));//tea
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 65));//lime
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 66));//sourguard
            owner.allyCardDetail.AddNewCardToDiscarded(new LorId("SoR4000", 67));//critic
            
            
        }


    }

    public class PassiveAbility_RosetteSummonAllies : PassiveAbilityBase
    {

        bool mintSum = false;
        bool sallySum = false;

        public override void OnWaveStart()
        {


            for (int i = 1; i < (5 - owner.UnitData.floorBattleData.param2); i++)
            {
                createAlly(i);                
            }

            BattleObjectManager.instance.InitUI();

        }

        public override void OnRoundStart()
        {
            base.OnRoundStart();

            owner.allyCardDetail.DiscardInHand((owner.allyCardDetail.GetHand().Count)-1);
            owner.allyCardDetail.DrawCards(4);
            owner.cardSlotDetail.RecoverPlayPoint(owner.MaxPlayPoint);
        }

        

        public void createAlly(int index)
        {
            
            int unitId = 0;
            int randomCase = UnityEngine.Random.Range(0, 14); ;

            switch (randomCase)
            {
                case 0:                  
                    unitId= 24;
                    break;
                case 1:
                    if (!mintSum)
                    {
                        unitId = 25;
                        mintSum = true;
                    }
                    else
                    {
                        unitId = 23;
                    }
                    break;
                case 2:                 
                    unitId = 26;
                    break;
                case 3:                                       
                    unitId = 27;
                    break;
                case 4:
                    if (!sallySum)
                    {
                        unitId = 28;
                        sallySum = true;
                    }
                    else
                    {
                        unitId = 23;
                    }
                    break;
                case 5:
                    unitId = 23;
                    break;
                case 6:
                    unitId = 22;
                    break;
                case 7:
                    unitId = 21;
                    break;
                case 8:
                    unitId = 20;
                    break;
                case 9:
                    unitId = 19;
                    break;
                case 10:
                    unitId = 20;
                    break;
                case 11:
                    unitId = 17;
                    break;
                case 12:
                    if (owner.UnitData.floorBattleData.param2 < 2)
                    {
                        unitId = 16;
                    }
                    else
                    {
                        unitId = 23;
                    }
                    break;
                case 13:
                    if(owner.UnitData.floorBattleData.param2 < 1)
                    {
                        unitId = 2;
                    }
                    else
                    {
                        unitId = 23;
                    }
                    break;
                default:
                    unitId = 23;
                    break;
            }

            BattleUnitModel battleUnitModel = Singleton<StageController>.Instance.AddNewUnit(Faction.Enemy, new LorId("SoR4000", unitId), index);

        }

        public override void OnRoundEndTheLast_ignoreDead()
        {
            base.OnRoundEndTheLast_ignoreDead();
            if (owner.IsDead())
            {
                foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(owner.faction))
                {
                    item.DieFake();
                }
            }
        }


    }

    public class SoR_buffet : BattleUnitBuf
    {
        protected override string keywordId => "sw_buffet";

        protected override string keywordIconId => "lor_buffet";


        public static string Name = "Buffet";
        public static string Desc = "Allow the use of specific Sweetkin pages without consequences.";

        public override string bufActivatedText
        {
            get
            {
                return "Allow the use of specific Sweetkin pages without consequences.";
            }
        }

        public SoR_buffet(int stack)
        {
            base.stack = stack;
        }

        public void consumeStack()
        {
            if (base.stack > 0)
            {
                base.stack--;
            }
        }

        public void addStacks(int stack)
        {
            for (int i = 0; i < stack; i++)
            {
                base.stack++;
            }
        }

        public override void OnRoundEnd()
        {
            if (base.stack <= 0)
            {
                base._owner.bufListDetail.RemoveBuf((BattleUnitBuf)(object)this);
            }
        }

    }

    public class SoR_quick : BattleUnitBuf
    {
        protected override string keywordId => "sw_quick";

        protected override string keywordIconId => "lor_quick";


        public static string Name = "Quick";
        public static string Desc = "Speed dices have +10 to their rolls";


        public SoR_quick(int stack)
        {
            base.stack = stack;
        }

        public override int GetSpeedDiceAdder(int speedDiceResult)
        {
            if (_owner.IsImmune(bufType))
            {
                return base.GetSpeedDiceAdder(speedDiceResult);
            }
            return 10;
        }

        public override void OnRoundEnd()
        {
            if (base.stack <= 0)
            {
                base._owner.bufListDetail.RemoveBuf((BattleUnitBuf)(object)this);
            }
        }

    }

    public class SoR_egobuf : BattleUnitBuf
    {
        protected override string keywordId => "sw_egobuf";

        protected override string keywordIconId => "lor_egobuf";


        public static string Name = "Ego enhancement";
        public static string Desc = "While the character has this effect, gain 2 strength and 1 haste at the start of the Scene for each stacks";


        public SoR_egobuf(int stack)
        {
            base.stack = stack;
        }

        public override void OnRoundStart()
        {
            base.OnRoundStart();

            
            _owner.bufListDetail.AddKeywordBufThisRoundByEtc(KeywordBuf.Strength, stack * 2, _owner);
            _owner.bufListDetail.AddKeywordBufThisRoundByEtc(KeywordBuf.Quickness, stack, _owner);
            
        }

        public void addStacks(int stack)
        {
            for (int i = 0; i < stack; i++)
            {
                base.stack++;
            }
        }

        public void setStacks(int newStacks)
        {
            base.stack = newStacks;
        }

        public override void OnRoundEnd()
        {
            if (base.stack <= 0)
            {
                base._owner.bufListDetail.RemoveBuf((BattleUnitBuf)(object)this);
            }
        }

    }

    public class SoR_disobey : BattleUnitBuf
    {
        public static string Name = "Disobedience";
        public static string Desc = "At three stacks, become a target for deadly attacks";


        protected override string keywordId => "sw_disobey";

        protected override string keywordIconId => "lor_disobey";

        public SoR_disobey(int stack)
        {
            base.stack = stack;
        }

        public void addStacks(int stack)
        {
            for (int i = 0; i < stack; i++)
            {
                base.stack++;
            }
        }

        public void setStacks(int newStacks)
        {
            base.stack = newStacks;
        }

        public override void OnRoundEnd()
        {
            if (base.stack <= 0)
            {
                base._owner.bufListDetail.RemoveBuf((BattleUnitBuf)(object)this);
            }
        }
    }

    public class SoR_frostbite : BattleUnitBuf
    {
        public static string Name = "Frostbite";
        public static string Desc = "Everytime a dice is rolled, take damage equal to the number of frostbite, then lower frostbite amount by 1.";


        protected override string keywordId => "sw_frostbite";

        protected override string keywordIconId => "lor_frostbite";

        public SoR_frostbite(int stack)
        {
            base.stack = stack;
        }

        public void addStacks(int stack)
        {
            for (int i = 0; i < stack; i++)
            {
                base.stack++;
            }
        }

        public void setStacks(int newStacks)
        {
            base.stack = newStacks;
        }

        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            base.BeforeRollDice(behavior);

            base._owner.TakeDamage(base.stack);
            if (base.stack > 0)
            {
                setStacks(base.stack - 1);
            }
            
        }

        public override void OnRoundEnd()
        {
            if (base.stack <= 0)
            {
                base._owner.bufListDetail.RemoveBuf((BattleUnitBuf)(object)this);
            }
        }
    }


    public class SoR_armor : BattleUnitBuf
    {
        public static string Name = "Armor";
        public static string Desc = "When hp reach 0, set this character's current hp to an amount equal to current armor and loses all stacks of armor.";


        protected override string keywordId => "sw_armor";

        protected override string keywordIconId => "lor_armor";

        public SoR_armor(int stack)
        {
            base.stack = stack;
        }

        public void addStacks(int stack)
        {
            for (int i = 0; i < stack; i++)
            {
                base.stack++;
            }
        }


        public override void OnHpZero()
        {
            base.OnHpZero();
            //if it does not exist, exit function
            if (IsDestroyed())
            {
                return;
            }


            //fetch num of stacks
            int num = 0;
            if (base.stack > num)
            {
                num = base.stack;
            }

            //set current hp to armor amount
            _owner.SetHp(num);

            base.stack = 0;

        }

        public override void OnRoundEnd()
        {
            if (base.stack <= 0)
            {
                base._owner.bufListDetail.RemoveBuf((BattleUnitBuf)(object)this);
            }
        }

    }

    public class SoR_famished : BattleUnitBuf
    {
        public static string Name = "Famished";
        public static string Desc = "This character's deals 1 less damage for each stack of famished";

        protected override string keywordId => "sw_famished";

        protected override string keywordIconId => "lor_famished";

        public SoR_famished(int stack)
        {
            base.stack = stack;
        }


        public override void BeforeGiveDamage(BattleDiceBehavior behavior)
        {
            base.BeforeGiveDamage(behavior);
            behavior.ApplyDiceStatBonus(new DiceStatBonus
            {
                dmg = -base.stack
            });
        }


        public override void OnRoundEnd()
        {
            if (base.stack <= 0)
            {
                base._owner.bufListDetail.RemoveBuf((BattleUnitBuf)(object)this);
            }
        }

        public void addStacks(int stack)
        {
            for (int i = 0; i < stack; i++)
            {
                base.stack++;
            }
        }

    }

    public class SoR_wellfed : BattleUnitBuf
    {
        protected override string keywordId => "sw_wellfed";

        protected override string keywordIconId => "lor_wellfed";

        public static string Name = "Well fed";
        public static string Desc = "This character's deals 1 more damage for each stack of famished";

        public SoR_wellfed(int stack)
        {
            base.stack = stack;
        }

        public override void BeforeGiveDamage(BattleDiceBehavior behavior)
        {
            base.BeforeGiveDamage(behavior);
            behavior.ApplyDiceStatBonus(new DiceStatBonus
            {
                dmg = base.stack
            });
        }


        public override void OnRoundEnd()
        {
            if (base.stack <= 0)
            {
                base._owner.bufListDetail.RemoveBuf((BattleUnitBuf)(object)this);
            }
        }

        public void addStacks(int stack)
        {
            for (int i = 0; i < stack; i++)
            {
                base.stack++;
            }
        }

    }

    public class SoR_sap : BattleUnitBuf
    {
        public SoR_sap(int stack)
        {
            base.stack = stack;
        }
        public static string Name = "Sap";
        public static string Desc = "Reduce the power of offensive dices by 2 for each stacks. At the end of the scene, lose one stack of sap.";

        protected override string keywordId => "sw_sap";

        protected override string keywordIconId => "lor_sap";

        public override void OnRoundEnd()
        {
            //reduce number of stack by one
            if (base.stack > 0)
            {
                base.stack--;
            }

            if (base.stack <= 0)
            {
                base._owner.bufListDetail.RemoveBuf((BattleUnitBuf)(object)this);
            }
        }

        public void addStacks(int stack)
        {
            for (int i = 0; i < stack; i++)
            {
                base.stack++;
            }
        }

        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            base.BeforeRollDice(behavior);

            if (!_owner.IsImmune(bufType) && IsAttackDice(behavior.Detail))
            {
                behavior.ApplyDiceStatBonus(new DiceStatBonus
                {
                    power = -(2 * stack)
                });

            }
        }



    }

    public class DiceCardSelfAbility_SoR_hospitality : DiceCardSelfAbilityBase
    {

        public static string Desc = "[On Use] Target recover 3 HP and gain 1 Sap";

        //SoR_sap sap = new SoR_sap(1);

        public override void OnUseCard()
        {

            BattleUnitModel target = base.card.target;
            target.RecoverHP(3);



            SoR_sap numSap = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_sap) as SoR_sap;
            //new code that add bonus stacks if not null
            if (numSap == null)
            {
                target.bufListDetail.AddBuf(new SoR_sap(1));
            }
            else
            {
                numSap.addStacks(1);
            }

            //original each one is single instance
            //target.bufListDetail.AddBuf(new SoR_sap(1));

        }


    }

    public class DiceCardSelfAbility_SoR_degradedPatron : DiceCardSelfAbilityBase
    {

        public static string Desc = "[On Use] Target gain 3 Sap";


        public override void OnUseCard()
        {

            BattleUnitModel target = base.card.target;


            SoR_sap numSap = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_sap) as SoR_sap;
            //new code that add bonus stacks if not null
            if (numSap == null)
            {
                target.bufListDetail.AddBuf(new SoR_sap(3));
            }
            else
            {
                numSap.addStacks(3);
            }

        }

    }

    public class DiceCardSelfAbility_SoR_begrudgingPatron : DiceCardSelfAbilityBase
    {

        public static string Desc = "[On Use] Target gain 6 Sap";


        public override void OnUseCard()
        {

            BattleUnitModel target = base.card.target;


            SoR_sap numSap = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_sap) as SoR_sap;
            //new code that add bonus stacks if not null
            if (numSap == null)
            {
                target.bufListDetail.AddBuf(new SoR_sap(6));
            }
            else
            {
                numSap.addStacks(6);
            }

        }

    }

    public class DiceCardSelfAbility_SoR_ancientPractice_safe : DiceCardSelfAbilityBase
    {

        public static string Desc = "[On Use] Target has 25% chance to get a haste and strength enhancement for the rest of the act. On a failure, lose the enhancement";


        public override void OnUseCard()
        {

            BattleUnitModel target = base.card.target;


            SoR_egobuf numBoost = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_egobuf) as SoR_egobuf;
            //new code that add bonus stacks if not null
            if (numBoost == null)
            {
                target.bufListDetail.AddBuf(new SoR_egobuf(1));
            }
            else
            {
                numBoost.addStacks(1);
            }

        }

    }

    public class DiceCardSelfAbility_SoR_ancientPractice : DiceCardSelfAbilityBase
    {

        public static string Desc = "[On Use] Target has 25% chance to get a haste and strength enhancement for the rest of the act. On a failure, lose the enhancement";


        public override void OnUseCard()
        {

            BattleUnitModel target = base.card.target;

            int num = UnityEngine.Random.Range(1, 4);

            SoR_egobuf numBoost = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_egobuf) as SoR_egobuf;
            //new code that add bonus stacks if not null
            if (numBoost == null && num == 1)
            {
                target.bufListDetail.AddBuf(new SoR_egobuf(1));
            }
            else
            {
                if (num == 1)
                {
                    numBoost.addStacks(1);
                }
                else
                {
                    numBoost.stack = 0;
                }
               
            }

        }

    }

    public class DiceCardSelfAbility_SoR_summon_sweetling : DiceCardSelfAbilityBase
    {

        public static string Desc = "[On Use] Every friendly characters recover 10 HP";



        public override void OnUseCard()
        {


            //heal first
            foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList(base.owner.faction))
            {
                alive.RecoverHP(10);
            }


        }


    }

    public class DiceCardSelfAbility_SoR_eaten_sweetling : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] a random ally gain 1 wellfed. User spend 1 buffet or takes Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;

            //provide a stack of wellfed to random ally
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(base.owner.faction, 1))
            {
                SoR_wellfed numWell = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

                if (numWell == null)
                {
                    item.bufListDetail.AddBuf(new SoR_wellfed(1));
                }
                else
                {
                    numWell.addStacks(1);
                }
                
            }

            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }


    }

    public class DiceCardSelfAbility_SoR_eaten_cayenne : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] the ally with the most hp gain 5 wellfed. User spend 1 buffet or takes Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;

            BattleUnitModel target = null;
            int target_maxHp = 0;

            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(Faction.Enemy))
            {
                if (item.MaxHp > target_maxHp)
                {
                    target = item;
                    target_maxHp = item.MaxHp;
                }
            }

            if (target == null)
            {
                //provide a stack of wellfed to random ally
                foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(base.owner.faction, 1))
                {
                    SoR_wellfed numWell = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

                    if (numWell == null)
                    {
                        item.bufListDetail.AddBuf(new SoR_wellfed(5));
                    }
                    else
                    {
                        numWell.addStacks(5);
                    }

                }
                
            }
            else
            {
                SoR_wellfed numWell =  target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

                if (numWell == null)
                {
                    target.bufListDetail.AddBuf(new SoR_wellfed(5));
                }
                else
                {
                    numWell.addStacks(5);
                }
            }

            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();
            }

        }


    }

    public class DiceCardSelfAbility_SoR_eaten_fastfood : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] the ally with the most hp gain quick. User spend 1 buffet or takes Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;

            BattleUnitModel target = null;
            int target_maxHp = 0;

            //find a target without quick with highest hp
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(Faction.Enemy))
            {
                //search returns null on targets without the buf
                SoR_quick hasQuick = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_quick) as SoR_quick;

                if (hasQuick == null && item.MaxHp > target_maxHp)
                {
                    target = item;
                    target_maxHp = item.MaxHp;
                }
            }
            // provide quick to target
            if (target != null )            
            {             
                target.bufListDetail.AddBuf(new SoR_quick(1));  
            }

            
            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();
            }

        }


    }

    public class DiceCardSelfAbility_SoR_eaten_crabcake : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] a random ally gain 20 armor. User spend 1 buffet or takes Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;

            //provide a stack of wellfed to random ally
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(base.owner.faction, 1))
            {
                SoR_armor numArmor = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_armor) as SoR_armor;

                if (numArmor == null)
                {
                    item.bufListDetail.AddBuf(new SoR_armor(20));
                }
                else
                {
                    numArmor.addStacks(20);
                }

                
            }

            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }


    }

    public class DiceCardSelfAbility_SoR_eaten_sourling : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Target enemy gain 1 famished. User spend 1 buffet or take Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;


            //provide a stack of famished to random enemy multi version
            /*foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(base.card.target.faction, 1))
            {
                item.bufListDetail.AddBuf(new SoR_famished(1));
            }*/

            //provide a stack of famished to enemy

            BattleUnitModel target = base.card.target;
            SoR_famished numFamished = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_famished) as SoR_famished;
            if (numFamished == null)
            {
                target.bufListDetail.AddBuf(new SoR_famished(1));
            }
            else
            {
                numFamished.addStacks(1);
            }

            



            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }


    }


    public class DiceCardSelfAbility_SoR_eaten_sourGuard : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] All enemies gain 1 famished. User spend 1 buffet or take Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;


            //provide a stack of famished to random enemy multi version
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(base.card.target.faction, 5))
            {
                
  
                SoR_famished numFamished = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_famished) as SoR_famished;
                if (numFamished == null)
                {
                    item.bufListDetail.AddBuf(new SoR_famished(1));
                }
                else
                {
                    numFamished.addStacks(1);
                }

            }


            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }


    }

    public class DiceCardSelfAbility_SoR_eaten_butler : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] All enemies gain 6 Aroma. User spend 1 buffet or take Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;


            //provide a stack of aroma to all enemy 
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(base.card.target.faction, 5))
            {

                item.bufListDetail.AddKeywordBufByCard(KeywordBuf.Alriune_Debuf,6,base.owner);

            }


            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }


    }

    public class DiceCardSelfAbility_SoR_eaten_cerberus : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] All enemies gain 5 Bound. User spend 1 buffet or take Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;


            //provide a stack of famished to random enemy multi version
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(base.card.target.faction, 5))
            {

                item.bufListDetail.AddKeywordBufByCard(KeywordBuf.Binding, 5, base.owner);

            }


            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }


    }

    public class DiceCardSelfAbility_SoR_summon_deliveryboy : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] All ally restore all light.";


        public override void OnUseCard()
        {
            base.OnUseCard();
            foreach (BattleUnitModel alive in BattleObjectManager.instance.GetAliveList(base.owner.faction))
            {
                alive.cardSlotDetail.SetPlayPoint(alive.cardSlotDetail.GetMaxPlayPoint());
            }
        }
    }

    public class DiceCardSelfAbility_SoR_leftovers : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Give a random ally 1 buffet. Draw a page.";

        public override void OnUseCard()
        {
            base.OnUseCard();

            //provide a stack of buffet to random ally
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(base.owner.faction, 1))
            {
                SoR_buffet numBuffet = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;

                if (numBuffet == null)
                {
                    item.bufListDetail.AddBuf(new SoR_buffet(1));
                }
                else
                {
                    numBuffet.addStacks(1);
                }

            }

            owner.allyCardDetail.DrawCards(1);
            
        }

    }

    public class DiceCardSelfAbility_SoR_allYouCanEat : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Gain 3 buffet. Draw a page and recover 1 light.";

        public override void OnUseCard()
        {
            base.OnUseCard();

            
                SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;

                if (numBuffet == null)
                {
                    owner.bufListDetail.AddBuf(new SoR_buffet(3));
                }
                else
                {
                    numBuffet.addStacks(3);
                }


            owner.cardSlotDetail.RecoverPlayPoint(1);
            owner.allyCardDetail.DrawCards(1);
        }

    }

    public class DiceCardSelfAbility_SoR_bonusWellfedDamage : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Do 10 more damage per stack of well fed on the target.";

        

        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            base.BeforeRollDice(behavior);

            BattleUnitModel target = card.target;

            SoR_wellfed numWell = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

            if (numWell != null)
            {
                int stack = numWell.stack;
                behavior?.ApplyDiceStatBonus(new DiceStatBonus
                {
                    dmg = (stack * 10)
                });
            }
            
        }

    }

    public class DiceCardSelfAbility_SoR_improveSelf : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Gain 1 ego enhancement";



        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            base.BeforeRollDice(behavior);

            //add
            SoR_egobuf numBoost = owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_egobuf) as SoR_egobuf;
            //new code that add bonus stacks if not null
            if (numBoost == null)
            {
                owner.bufListDetail.AddBuf(new SoR_egobuf(1));
            }
            else
            {
                numBoost.addStacks(1);
            }

        }

    }

    public class DiceCardSelfAbility_SoR_applyDisobey : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Inflict 1 disobedience to target";

        public override void OnUseCard()
        {
            base.OnUseCard();

            BattleUnitModel target = base.card.target;

            SoR_disobey disobey = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_disobey) as SoR_disobey;

            if (disobey != null)
            {
                disobey.addStacks(1);
            }
            else
            {
                target.bufListDetail.AddBuf(new SoR_disobey(1));
            }
        }

    }

    public class DiceCardSelfAbility_SoR_hurtself : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Take damage equal to 50% of max Hp";

        public override void OnUseCard()
        {
            base.OnUseCard();

            owner.TakeDamage(owner.MaxHp / 2);
        }

    }

    public class DiceCardSelfAbility_SoR_eaten_catering : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Give an ally 5 well fed. User spend 1 buffet or take Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;


            //provide a stack of wellfed to random ally
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(base.owner.faction, 1))
            {
                SoR_wellfed numWell = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

                if (numWell == null)
                {
                    item.bufListDetail.AddBuf(new SoR_wellfed(5));
                }
                else
                {
                    numWell.addStacks(5);
                }

            }

            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }


    }

    public class DiceCardSelfAbility_SoR_eaten_janitor : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] User spend 1 buffet or take Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;
          

            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }

    }

    public class DiceCardSelfAbility_SoR_eaten_security : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Inflict all enemies 2 sap. User spend 1 buffet or take Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;


            //provide a stack of wellfed to random ally
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(Faction.Player, 5))
            {
                SoR_sap numSap = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_sap) as SoR_sap;

                if (numSap == null)
                {
                    item.bufListDetail.AddBuf(new SoR_sap(2));
                }
                else
                {
                    numSap.addStacks(2);
                }

            }

            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }


    }

    public class DiceCardAbility_piercing : DiceCardAbilityBase
    {

        public static string Desc = "Reduce Power of target's current Defensive die by 15";

        public override void BeforeRollDice()
        {
            base.BeforeRollDice();
            if (behavior.TargetDice != null)
            {
                BattleDiceBehavior targetDice = behavior.TargetDice;

                if (IsDefenseDice(targetDice.Detail))
                {
                    targetDice.ApplyDiceStatBonus(new DiceStatBonus
                    {
                        power = -15
                    });
                }
            }
        }
    }

    public class DiceCardAbility_aftercare : DiceCardAbilityBase
    {

        public static string Desc = "[On Hit] Remove all stacks of Aroma and add 3 stacks of well fed.";

        public override void BeforeGiveDamage()
        {
            BattleUnitModel target = base.card.target;

            base.BeforeGiveDamage();
            BattleUnitBuf_Alriune_Debuf aroma = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is BattleUnitBuf_Alriune_Debuf) as BattleUnitBuf_Alriune_Debuf;

            if(aroma != null)
            {
                target.bufListDetail.GetActivatedBuf(KeywordBuf.Alriune_Debuf).Destroy();
            }

            SoR_wellfed numWell = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

            if (numWell == null)
            {
                target.bufListDetail.AddBuf(new SoR_wellfed(3));
            }
            else
            {
                numWell.addStacks(3);
            }
            
        }
    }

    public class DiceCardAbility_teaParty : DiceCardAbilityBase
    {

        public static string Desc = "[On Clash Win] Remove 1 stack of disobidience from the target and add 2 stacks of well fed to both.";

        public override void OnWinParrying()
        {
            base.OnWinParrying();
        
            BattleUnitModel target = base.card.target;
           
            SoR_disobey disobey = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_disobey) as SoR_disobey;

            if (disobey != null)
            {
                disobey.setStacks(disobey.stack - 1);
            }

            SoR_wellfed numWell = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;
            SoR_wellfed party = owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

            if (numWell == null)
            {
                target.bufListDetail.AddBuf(new SoR_wellfed(2));
                
            }
            else
            {
                numWell.addStacks(3);
            }


            if (party == null)
            {
                owner.bufListDetail.AddBuf(new SoR_wellfed(2));

            }
            else
            {
                party.addStacks(3);
            }

        }
    }

    public class DiceCardAbility_yieldVictory : DiceCardAbilityBase
    {

        public static string Desc = "[On Clash Lose] Inflict 1 stack of disobidience to the target.";

        public override void OnLoseParrying()
        {
            base.OnLoseParrying();

            BattleUnitModel target = base.card.target;
          
            SoR_disobey disobey = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_disobey) as SoR_disobey;

            if (disobey != null)
            {
                disobey.addStacks(1);
            }
            else
            {
                target.bufListDetail.AddBuf(new SoR_disobey(1));
            }
        }

    }

    public class DiceCardAbility_instigate : DiceCardAbilityBase
    {

        public static string Desc = "[On Hit] Inflict 1 stack of disobidience to the target.";

        public override void OnSucceedAttack()
        {
            base.OnSucceedAttack();

            BattleUnitModel target = base.card.target;

            SoR_disobey disobey = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_disobey) as SoR_disobey;

            if (disobey != null)
            {
                disobey.addStacks(1);
            }
            else
            {
                target.bufListDetail.AddBuf(new SoR_disobey(1));
            }
        }

    }

    public class DiceCardAbility_cutHead : DiceCardAbilityBase
    {

        public static string Desc = "[On Hit] Kill the target if they have 5 stack of disobidience.";


        public override void OnSucceedAttack()
        {
            base.OnSucceedAttack();

            BattleUnitModel target = base.card.target;

            SoR_disobey disobey = target.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_disobey) as SoR_disobey;

            if (disobey != null && disobey.stack >= 5)
            {
                target.Die();
            }
            
        }
       

    }


    public class DiceCardAbility_fallLocust : DiceCardAbilityBase
    {

        public static string Desc = "[On Hit] Apply 1 fairy to everyone.";

        public override void OnSucceedAttack(BattleUnitModel target)
        {
            base.OnSucceedAttack(target);
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList())
            {
                item.bufListDetail.AddKeywordBufThisRoundByCard(KeywordBuf.Fairy, 1, base.owner);
            }
        }

        /*
        public override void OnSucceedAttack()
        {
            base.OnSucceedAttack();

       
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList())
            {
                item.bufListDetail.AddKeywordBufThisRoundByCard(KeywordBuf.Fairy, 1, base.owner);
            }

        }*/


    }

    public class DiceCardSelfAbility_SoR_summerHeat : DiceCardSelfAbilityBase
    {

        public static string Desc = "[On Use] Apply 10 burn to everyone.";

        public override void OnUseCard()
        {
            base.OnUseCard();

            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList())
            {
                item.bufListDetail.AddKeywordBufThisRoundByCard(KeywordBuf.Burn, 10, base.owner);
            }
        }


    }

    public class DiceCardSelfAbility_SoR_springBleed : DiceCardSelfAbilityBase
    {

        public static string Desc = "[On Use] Apply 10 bleed to everyone next turn.";

        public override void OnUseCard()
        {
            base.OnUseCard();

            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList())
            {
                item.bufListDetail.AddKeywordBufByCard(KeywordBuf.Bleeding, 10, base.owner);
            }
        }


    }

    public class DiceCardSelfAbility_SoR_winterStorm : DiceCardSelfAbilityBase
    {

        public static string Desc = "[On Use] Apply 4 frostbite to everyone.";

        public override void OnUseCard()
        {
            base.OnUseCard();

            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList())
            {

                SoR_frostbite numfrostbite = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_frostbite) as SoR_frostbite;
                if (numfrostbite == null)
                {
                    item.bufListDetail.AddBuf(new SoR_frostbite(4));
                }
                else
                {
                    numfrostbite.addStacks(4);
                }
            }
        }


    }

    public class DiceCardAbility_brutishCharge : DiceCardAbilityBase
    {

        public static string Desc = "Add +20 Power if the attack is one-sided";

        public override void BeforeRollDice()
        {
            base.BeforeRollDice();

            if (!behavior.IsParrying())
            {
                behavior.ApplyDiceStatBonus(new DiceStatBonus
                {
                    power = 20
                });
            }
        }


    }

    public class DiceCardSelfAbility_SoR_eaten_pyretatoe : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Inflict 5 burn to everyone. User spend 1 buffet or take Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;


            //provide a stack of famished to random enemy multi version
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList())
            {

                item.bufListDetail.AddKeywordBufByCard(KeywordBuf.Burn, 5, base.owner);
            }


            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }


    }

    public class DiceCardSelfAbility_SoR_veil : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Discard 2 page, then draw 2 page";


        public override void OnUseCard()
        {
            owner.allyCardDetail.DisCardACardRandom();
            owner.allyCardDetail.DisCardACardRandom();
            owner.allyCardDetail.DrawCards(2);
        }


    }

    public class DiceCardAbility_cookingPrep : DiceCardAbilityBase
    {

        public static string Desc = "[On Hit] Inflict 4 Fragile this scene";

        public override void OnSucceedAttack(BattleUnitModel target)
        {
            base.OnSucceedAttack(target);
            
            target.bufListDetail.AddKeywordBufThisRoundByCard(KeywordBuf.Vulnerable, 4, base.owner);
            
        }


    }

    public class DiceCardSelfAbility_SoR_empoweringCare : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Heal a random ally by 10 and grant them 10 armor. Draw 1 page";


        public override void OnUseCard()
        {
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(owner.faction, 1))
            {
                SoR_armor numArmor = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_armor) as SoR_armor;

                item.RecoverHP(10);
                if (numArmor == null)
                {
                    item.bufListDetail.AddBuf(new SoR_armor(10));
                }
                else
                {
                    numArmor.addStacks(10);
                }

            }

            owner.allyCardDetail.DrawCards(1);
        }


    }

    public class DiceCardSelfAbility_SoR_workshift : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Heal a random ally by 20. Recover 1 light";


        public override void OnUseCard()
        {
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(owner.faction, 1))
            {
                SoR_armor numArmor = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_armor) as SoR_armor;

                item.RecoverHP(20);
                

            }
            owner.cardSlotDetail.RecoverPlayPoint(1);
            
        }


    }

    public class DiceCardSelfAbility_SoR_massArmorAndPower : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] All friendly characters are granted 10 armor and 3 well fed";


        public override void OnUseCard()
        {
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(owner.faction))
            {
                SoR_armor numArmor = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_armor) as SoR_armor;
                SoR_wellfed numWell = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;

                if (numArmor == null)
                {
                    item.bufListDetail.AddBuf(new SoR_armor(10));
                }
                else
                {
                    numArmor.addStacks(10);
                }

                if (numWell == null)
                {
                    item.bufListDetail.AddBuf(new SoR_wellfed(3));
                }
                else
                {
                    numWell.addStacks(3);
                }

            }

            owner.allyCardDetail.DrawCards(1);
        }


    }

    public class DiceCardSelfAbility_SoR_eaten_frostbite : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Inflict 3 Frostbite to all enemies. User spend 1 buffet or take Lethal amount of damage.";


        public override void OnUseCard()
        {
            //check for presence of buffet on the user
            SoR_buffet numBuffet = base.owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_buffet) as SoR_buffet;
            

            //provide a stack of famished to random enemy multi version
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(card.target.faction))
            {
                SoR_frostbite numfrostbite = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_frostbite) as SoR_frostbite;
                if (numfrostbite == null)
                {
                    item.bufListDetail.AddBuf(new SoR_frostbite(3));
                }
                else
                {
                    numfrostbite.addStacks(3);
                }

            }


            if (numBuffet == null)
            {
                owner.TakeDamage(999);
            }

            //buffet removal
            base.OnUseCard();
            if (numBuffet.stack < 1)
            {
                owner.TakeDamage(999);
            }
            else
            {
                numBuffet.consumeStack();

            }
        }


    }

    public class DiceCardSelfAbility_SoR_removeBurn : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Remove all stacks of Burn from combattants. Restore 1 light.";


        public override void OnUseCard()
        {

            //provide a stack of famished to random enemy multi version
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList())
            {
                BattleUnitBuf Buf = owner.bufListDetail.GetActivatedBuf(KeywordBuf.Burn);

                if (Buf != null)
                {
                    Buf.Destroy();
                }
            }

        }


    }

    public class DiceCardSelfAbility_SoR_randomPower: DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Give 1 Strength to three random allies next Scene.";


        public override void OnUseCard()
        {

            //provide a stack of famished to random enemy multi version
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList_random(base.owner.faction, 3))
            {
                item.bufListDetail.AddKeywordBufByCard(KeywordBuf.Strength, 1, base.owner);
            }

        }

    }

    public class DiceCardSelfAbility_SoR_massWellfed : DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Give 2 well fed to all other allies";


        public override void OnUseCard()
        {

            //provide a stack of famished to random enemy multi version
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(owner.faction))
            {
                if (item == owner)
                {
                    continue;
                }
               
                SoR_wellfed numWell = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_wellfed) as SoR_wellfed;               

                if (numWell == null)
                {
                    item.bufListDetail.AddBuf(new SoR_wellfed(2));
                }
                else
                {
                    numWell.addStacks(2);
                }

            }

        }

    }

    public class DiceCardSelfAbility_SoR_massArmor: DiceCardSelfAbilityBase
    {
        public static string Desc = "[On Use] Give 20 Armor to all other allies";


        public override void OnUseCard()
        {

            //provide a stack of famished to random enemy multi version
            foreach (BattleUnitModel item in BattleObjectManager.instance.GetAliveList(owner.faction))
            {
                if (item == owner)
                {
                    continue;
                }

                SoR_armor numArmor = item.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is SoR_armor) as SoR_armor;

                if (numArmor == null)
                {
                    item.bufListDetail.AddBuf(new SoR_armor(20));
                }
                else
                {
                    numArmor.addStacks(20);
                }

            }

        }

    }

    public class DiceCardAbility_recyclePine : DiceCardAbilityBase
    {

        public static string Desc = "[On Hit] Recycle this Dice (Up to 3 times)";

        private int count = 0;

        public override void OnSucceedAttack(BattleUnitModel target)
        {
            base.OnSucceedAttack(target);

            if (count < 3)
            {
                ActivateBonusAttackDice();
                count++;
            }

        }


    }

    public class DiceCardAbility_recoverStaggerTall : DiceCardAbilityBase
    {

        public static string Desc = "[On Clash Win] Recover 9 Stagger Resist";

        private int count = 0;

        public override void OnWinParrying()
        {
            base.OnWinParrying();

            base.owner.breakDetail.RecoverBreak(9);
        }


    }

}


