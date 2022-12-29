using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface ICurrency
    {
        /// <summary>
        /// Notifies when the state of the game currency has been updated.
        /// </summary>
        event Action<CBSUpdateCurrencyResult> OnCurrencyUpdated;

        /// <summary>
        /// Get last cached currency data.
        /// </summary>
        Dictionary<string, int> CacheCurrencies { get; }

        /// <summary>
        /// Get data for all currencies.
        /// </summary>
        /// <param name="result"></param>
        void GetCurrencies(Action<CBSGetCurrenciesResult> result = null);

        /// <summary>
        /// Add currency to the current user.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currencyCode"></param>
        /// <param name="result"></param>
        void AddUserCurrency(int amount, string currencyCode, Action<CBSUpdateCurrencyResult> result = null);

        /// <summary>
        /// Reduce the game currency of the current user.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="result"></param>
        void DecreaseUserCurrency(int amount, string currency, Action<CBSUpdateCurrencyResult> result = null);

        /// <summary>
        /// Get all currency packs.
        /// </summary>
        /// <param name="result"></param>
        void GetPacks(Action<CBSGetPacksResult> result);

        /// <summary>
        /// Get all currency packs by specific tag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="result"></param>
        void GetPacksByTag(string tag, Action<CBSGetPacksResult> result);

        /// <summary>
        /// Grant currency pack to current user.
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="result"></param>
        void GrantPack(string packID, Action<CBSGrandPackResult> result);
    }
}
