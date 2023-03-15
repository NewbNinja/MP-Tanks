/*  This file is part of the "Tanks Multiplayer" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Custom powerup implementation for weapon pick ups
    /// </summary>
	public class SG_Powerup_Weapon : Collectible
    {
        /// <summary>
        /// Amount of health points to add per consumption.
        /// </summary>
        public int amount = 5;


        /// <summary>
        /// Overrides the default behavior with a custom implementation.
        /// Check for the current health and adds additional health.
        /// </summary>
        public override bool Apply(Player p)
        {
            if (p == null)
                return false;


            // TODO:
            // VERIFY WITH SERVER IF COLLECTIBLE EXISTS AND IS STILL AVAILABLE TO COLLECT


            // GET PLAYER WEAPONS STORAGE INFO
            // Does the player have enough room to store this weapon pickup (activatable weapon button)


            // PICK UP + STORE WEAPON  or  ALERT PLAYER (Weapons Full)

            // IF PLAYER HAS ROOM:
            // GENERATE RANDOM WEAPON FROM PLAYERS WEAPON POOL 

            // IF PLAYER HAS 20 ITEMS IN THEIR WEAPONS POOL (RANDOMLY CHOOSE ONE) -> ADD TO WEAPONSREADY LIST + ASSIGN BUTTON

            // IF PLAYER DOES NOT HAVE 20 ITEMS IN THEIR WEAPONS POOL:
            // If player has less than 20 items, take number of NFT / crafted weapons they have assigned in their
            // weapons pool, roll random number between 1-20, randomize their weapons among this pool.  If the number
            // chosen belongs to one of their weapons, select it and apply to first available weapons button and add to 
            // players WeaponsReady list so that they can fire it when they choose.


            // REMOVE POWERUP (WEAPON) FROM SCENE / DISABLE AND HIDE IT


            // RETURN TRUE FOR SUCCESSFUL COLLECTION


            int value = p.GetView().GetHealth();

            //don't add health if it is at the maximum already
            if (value == p.maxHealth)
                return false;

            //get current health value and add amount to it
            value += amount;

            //we have to clamp the health to the maximum, so that
            //we don't go over the maximum by accident. Then assign
            //the new health value back to the player
            value = Mathf.Clamp(value, value, p.maxHealth);
            p.GetView().SetHealth(value);

            //return successful collection
            return true;
        }
    }
}
