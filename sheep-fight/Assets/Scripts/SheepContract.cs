using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Signer;
using Nethereum.Hex.HexTypes;

public class SheepContract : Singleton<SheepContract>
{
    [Header("Deployed contract")]
    public TextAsset contractABI;
    public TextAsset contractAddress;

    private HexBigInteger ethBalance;
    private Web3 web3;
    private Account account;
    private string privateKey;
    private string from;
    private Contract contract;

    private HexBigInteger gas = new HexBigInteger(900000);
    private HexBigInteger betValue = new HexBigInteger(Web3.Convert.ToWei(1));

    private Function checkPlaying;
    private Function playFunction;
    private Function forceEndFunction;
    private Function winGameFunction;
    private Function loseGameFunction;

    void Start()
    {
        AccountSetup();
    }

    void CopyToClipboard(string s)
    {
        TextEditor te = new TextEditor();
        te.text = s;
        te.SelectAll();
        te.Copy();
    }

    public void CopyAddress()
    {
        CopyToClipboard(from);
    }

    public void CopyPrivateKey()
    {
        CopyToClipboard(privateKey);
    }

    public void AccountSetup()
    {
        var url = "https://testnet.tomochain.com";
        // var url = "https://rpc.tomochain.com";
        privateKey = PlayerPrefs.GetString("privateKey");
        if (privateKey == "")
        {
            GameManager.Instance.PostNotification(EVENT_TYPE.NO_PRIVATE_KEY);
            return;
        }
        account = new Account(privateKey);
        from = account.Address;
        PlayerPrefs.SetString("NickName", from);
        GameManager.Instance.PostNotification(EVENT_TYPE.ACCOUNT_READY, this, from);
        web3 = new Web3(account, url);
        StartCoroutine(BalanceInterval());
        GetContract();
    }

    public void SwitchAccount()
    {
        GameManager.Instance.balanceOK = false;
        AccountSetup();
        UpdateBalance();
    }

    IEnumerator BalanceInterval()
    {
        while (true)
        {
            UpdateBalance();
            yield return new WaitForSeconds(1f);
        }

    }

    public async Task UpdateBalance()
    {
        var newBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
        ethBalance = newBalance;
        decimal ethBalanceVal = Web3.Convert.FromWei(ethBalance.Value);
        GameManager.Instance.balanceOK = (ethBalanceVal > 1);
        GameManager.Instance.PostNotification(EVENT_TYPE.BLANCE_UPDATE, this, ethBalanceVal);
    }

    void GetContract()
    {
        string abi = contractABI.ToString();
        string address = contractAddress.ToString();
        contract = web3.Eth.GetContract(abi, address);

        checkPlaying = contract.GetFunction("isPlaying");
        playFunction = contract.GetFunction("play");
        winGameFunction = contract.GetFunction("winGame");
        loseGameFunction = contract.GetFunction("loseGame");
        forceEndFunction = contract.GetFunction("forceEndGame");
    }

    public async Task<bool> CheckPlaying()
    {
        var isPlaying = await checkPlaying.CallAsync<bool>(from);
        return isPlaying;
    }

    public async Task<string> Play(string gameID)
    {
        bool isPlaying = await CheckPlaying();
        if (isPlaying) await ForceEndGame();
        var receipt = await playFunction.SendTransactionAndWaitForReceiptAsync(from, gas, betValue, null, gameID);
        return receipt.TransactionHash;
    }

    public async Task<string> WinGame()
    {
        var receipt = await winGameFunction.SendTransactionAndWaitForReceiptAsync(from, gas, null);
        Debug.Log(string.Format("WinGame tx: {0}", receipt.TransactionHash));
        return receipt.TransactionHash;
    }

    public async Task<string> LoseGame()
    {
        var receipt = await loseGameFunction.SendTransactionAndWaitForReceiptAsync(from, gas, null);
        Debug.Log(string.Format("LoseGame tx: {0}", receipt.TransactionHash));
        return receipt.TransactionHash;
    }

    public async Task<string> ForceEndGame()
    {
        bool isPlaying = await CheckPlaying();
        if (!isPlaying)
        {
            Debug.Log("Not in game, just quit");
            return "";
        }
        var receipt = await forceEndFunction.SendTransactionAndWaitForReceiptAsync(from, gas, null);
        Debug.Log(string.Format("ForceEndGame tx: {0}", receipt.TransactionHash));
        return receipt.TransactionHash;
    }
}
