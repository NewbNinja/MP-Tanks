//using Defverse.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

[CreateAssetMenu(fileName = "New Weapon", menuName = "SpaceGuardians Weapon")]
public class SG_Weapon : ScriptableObject
{
    private float damage;
    private bool isCrit;


    //=== ODIN EDITOR SETTINGS ===


    [TitleGroup("Unique Weapon Identifiers", alignment: TitleAlignments.Centered, horizontalLine: true, boldTitle: true, indent: false), Required("Weapon MUST have a Unique Weapon ID")]
    [DetailedInfoBox("NUMBERING CONVENTION EXPLAINED  ->  Click to view detailed information...",
    "NUMBERING CONVENTION EXPLAINED\n\n" +
    "The Weapon ID number is defined using the following structure:\n\n" +
        "<SEASON NUMBER - X> <WEAPON CATEGORY - YY> <UNIQUE SEASONAL WEAPON ID - ZZZ>\n\n" +
        "Example:  Season 5, Weapon Category 4, 12th weapon that season in that category\n" +
        "This would result in the following generated weapon ID:  504012  (XYYZZZ)\n\n" +
        "It is VERY IMPORTANT when creating new weapons that you follow this numbering convention!\n\n" +
        "ANY WEAPON with an ID less than 6 digits long is IN DEVELOPMENT and MUST NOT be pushed LIVE until it has been renumbered!")]

    /// <summary>
    /// Unique weapon ID, should follow the live weapon ID numbering convention X-YY-ZZZ
    /// 
    /// <SEASON NUMBER - X> <WEAPON CATEGORY - YY> <UNIQUE SEASONAL WEAPON ID - ZZZ>
    /// Example:  Season 5, Weapon Category 4, 12th weapon that season in that category
    /// This would result in the following generated weapon ID:  504012  (XYYZZZ).
    /// </summary>
    [SerializeField] private uint _weaponUID;
    
    public string WeaponName; 
    
    /// <summary>
    /// A short unique description for the weapon.
    /// </summary>
    public string Description;

    [Space(10)]

    [Header("Assets")]
    [Tooltip("UI Button Image used to represent this weapon in game")]
    [SerializeField] private Sprite buttonImage;

    /// <summary>
    /// Clip to play when this weapon fires.
    /// </summary>
    public AudioClip shootSound;

    [Space(10)]
    [Header("Projectile Info")]
    [Tooltip("The projectile graphic we want to use, the CHILD element position of our graphic stored in the MasterProjectilePrefab")]
    [SerializeField] public uint projectileID;        // The projectile graphic we want to use - Called in Shooting.cs
                                                     // public GameObject muzzleFlash;
                                                     //public GameObject impactAnimation;   

    [PropertySpace(SpaceBefore = 10, SpaceAfter = 10), PropertyOrder(0)]

    [Tooltip("Select the flash colour to display upon impact"), LabelText("Impact Flash Colour")]
    [SerializeField] private Color color;



    // =====  DAMAGE TAB  =====


    [Title("Damage Attributes", TitleAlignment = TitleAlignments.Centered, HorizontalLine = true, Bold = true)]
    /// <summary>
    /// Weapon category type:
    /// 
    ///  00 = AutoCannon, 01 = Blasters, 02 = FlakCannon, 03 = Lasers, 04 = MicroMissiles,
    ///  05 = PlasmaCannon, 06 = PulseTurret, 07 = Railgun, 08 = Torpedo
    /// </summary>
    /// 
    [TabGroup("Damage"), EnumPaging]
    [DetailedInfoBox("Weapon Category IDs ... Click to view detaild information...",
    "Weapon Category IDs\n\n" +
    "00 = AutoCannon\n" +
    "01 = Blasters\n" +
    "02 = FlakCannon\n" +
    "03 = Lasers\n" +
    "04 = MicroMissiles\n" +
    "05 = PlasmaCannon\n" +
    "06 = PulseTurret\n" +
    "07 = Railgun\n" +
    "08 = Torpedo\n")]

    [SerializeField] private WeaponType WeaponTypeField;      // Used by Odin for Unity Editor

    private enum WeaponType
    {
        AutoCannon,     // 00
        Blasters,       // 01
        FlakCannon,     // 02       -   << Weapon Category ID >>
        Lasers,         // 03
        MicroMissiles,  // 04           Used to partially define a
        PlasmaCannon,   // 05           weapons unique ID in _weaponsUID
        PulseTurret,    // 06
        Railgun,        // 07
        Torpedo         // 08
    }

    [Space(5)]

    [TabGroup("Damage"), EnumPaging]
    [Tooltip("Kinetic = 0, Energy = 1, Explosive = 2")]
    [SerializeField] private AmmoType AmmotypeField;

    private enum AmmoType
    {
        Kinetic,    // 0
        Energy,     // 1
        Explosive   // 2
    }

    [Space(5)]

    [TabGroup("Damage"), EnumPaging]
    [DetailedInfoBox("Damage Type ... Click to view detaild information...",
    "Weapon Category IDs\n\n" +
    "0 = NEUTRAL\n" +
    "1 = FIRE\n" +
    "2 = EARTH\n" +
    "3 = WATER\n" +
    "4 = STORM\n" +
    "5 = ACID\n" +
    "6 = LIGHT\n" +
    "7 = DARK\n" +
    "8 = VOID\n")]
    [Tooltip("0=NEUTRAL, 1=FIRE, 2=EARTH, 3=WATER, 4=STORM, 5=ACID, 6=LIGHT, 7=DARK, 8=VOID")]

    [SerializeField] private DamageType DamageTypeField;      // Used by Odin for Unity Editor
    private enum DamageType
    {
        Neutral,   // 00
        Fire,      // 01
        Earth,     // 02
        Water,     // 03
        Storm,     // 04
        Acid,      // 05
        Light,     // 06
        Dark,      // 07
        Void       // 08
    }

    [Space(10)]

    [Header("DAMAGE")]
    [TabGroup("Damage")]
    [Range(1, 10000)]
    [SerializeField] private float minDamage;

    [TabGroup("Damage")]
    [Range(1, 10000)]
    [SerializeField] private float maxDamage;

    [TabGroup("Damage")]
    [Tooltip("Damage Modifier - BASE modifier - should be 1.0 for 100%")]
    [SerializeField] private float damageModifier = 1f;

    [Space(5)]

    [Header("CRIT")]
    [TabGroup("Damage")]
    [LabelText("Crit Chance %")]
    [Range(0f, 100f)]
    [SerializeField] private float critChance;

    [TabGroup("Damage")]
    [Tooltip("Percentage of EXTRA damage to add if we crit, represented as a float: Example 1.5 (+50%)")]
    [Range(1f, 5f)]
    [SerializeField] private float critModifier;

    [Space(5)]

    [Header("SPEED")]
    [TabGroup("Damage"), Tooltip("Bullet Speed")]
    [Range(5f, 100f)]
    [SerializeField] private float bulletVelocity = 20f;

    [Space(5)]

    [Header("RANGE")]
    [TabGroup("Damage"), Tooltip("Weapon Range:  Default 20")]
    [Range(10f, 100f)]
    [SerializeField] private float projectileRange = 20f;

    [TabGroup("Damage"), Tooltip("Projectile Max Life Time:  Default 1.5")]
    [Range(.5f, 5f)]
    [SerializeField] private float projectileTimeToLive = 1.5f;



    // =====  FIRING PATTERNS TAB  =====


    [Title("Firing Attributes", TitleAlignment = TitleAlignments.Centered, HorizontalLine = true, Bold = true)]
    [TabGroup("Firing Patterns")]
    [EnumPaging]

    [SerializeField] private FiringPattern FiringPatternField;      // Used by Odin for Unity Editor
    private enum FiringPattern
    {
        SingleShot,
        Burst,
        Beam
    }

    [Space(10)]

    [TabGroup("Firing Patterns"), Tooltip("Number of simultaneous projectiles that can be fired from the weapon")]
    [Range(1, 4)]
    [SerializeField] private sbyte numberOfCannons;

    [Space(10)]

    [TabGroup("Firing Patterns")]
    [Range(0.01f, 5f)]
    [SerializeField] private float fireRate;


    [TabGroup("Firing Patterns")]
    [Range(0.01f, 5f)]
    [SerializeField] private float reloadTime;

    [Space(10)]

    [EnumToggleButtons, ShowIf("FiringPatternField", FiringPattern.Beam)]
    [TabGroup("Firing Patterns")]
    [Range(0f, 5f)]
    [SerializeField] private float chargeTime;

    [Space(10)]

    [EnumToggleButtons]
    [ShowIf("FiringPatternField", FiringPattern.Burst)]
    [TabGroup("Firing Patterns")]
    [Range(2, 100)]
    [SerializeField] private int roundsPerBurst;

    [EnumToggleButtons]
    [ShowIf("FiringPatternField", FiringPattern.Burst)]
    [TabGroup("Firing Patterns")]
    [SerializeField] private int numberOfShotsLeftToFire;      // Number of rounds left to fire this burst

    [EnumToggleButtons, ShowIf("FiringPatternField", FiringPattern.Burst)]
    [TabGroup("Firing Patterns"), DisableInPlayMode]
    [SerializeField] private bool burstFirstRound;            // Is this the first round of the burst sequence

    [Space(10)]

    [TabGroup("Firing Patterns"), LabelText("Destroy On Impact?")]
    [SerializeField] private bool destroyProjectileOnImpact = true;



    // =====  OTHER TAB  =====


    [Title("Other Attributes", TitleAlignment = TitleAlignments.Centered, HorizontalLine = true, Bold = true)]
    [TabGroup("Other")]
    [Tooltip("Level of the weapon - bonuses are awarded based upon the level of the weapon")]
    [SerializeField] private int level;

    [TabGroup("Other")]
    [Tooltip("Experience is used to calculate the level of the weapon")]
    [SerializeField] private int experience;

    [Space(10)]

    [TabGroup("Other")]
    [SerializeField] private bool isTradeable;

    [TabGroup("Other")]
    [SerializeField] private bool isCrafted;

    [TabGroup("Other")]
    [SerializeField] private bool isNFT;




    // =====  METHODS  =====


    public uint GetWeaponID
    {
        get { return _weaponUID; }
    }

    private void Awake()
    {
        if (FiringPatternField == FiringPattern.Burst)
        {
            burstFirstRound = true;
            numberOfShotsLeftToFire = roundsPerBurst;
        }
    }

    // Returns damage, range, isCrit, isBeam
    public (int, int, float, float, bool, bool) Fire()
    {
        // Calculate random damage between ranges specified
        damage = UnityEngine.Random.Range(minDamage, maxDamage) * damageModifier;

        // RANDOMIZE CRIT CHANCE
        float a = UnityEngine.Random.Range(0f, 100f);
        {
            if (a <= critChance)
            {
                // WE DID A CRITICAL HIT - ADD ADDITIONAL DAMAGE
                isCrit = true;
                damage *= critModifier;
            }
            else { isCrit = false; }
        }

        return ((int)damage, (int)FiringPatternField, projectileRange, projectileTimeToLive, isCrit, destroyProjectileOnImpact);
    }


    public float GetFireRate()
    {
        return fireRate;
    }


    public (bool, int, float) GetIsBurst()
    {
        bool isBurstWeapon = (FiringPatternField == FiringPattern.Burst) ? true : false;
        return (isBurstWeapon, roundsPerBurst, reloadTime);
    }


    public float GetReloadTime()
    {
        return reloadTime;
    }


    public int GetBurstCountRemaining(){ return numberOfShotsLeftToFire; }

    public void SetBurstCountRemaining(int i)
    {
        // If we have fired the last bullet in the burst, then reset
        if (numberOfShotsLeftToFire <= 0)
        {
            burstFirstRound = true;
            numberOfShotsLeftToFire = roundsPerBurst;
        }
        else
            numberOfShotsLeftToFire--;
    }
}
