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

public class SheepContract : MonoBehaviour, IListener
{
    private Account account;
    private Web3 web3;
    // seed = "exit lens suggest bamboo sniff head sentence focus burger fever prefer benefit";
    // private string privateKey = "0x6e42b17dc5d278edfef336250b0e813f9c61f7fdc8de2ef65f05da1b1014f0b9"; //ganache-cli acc 0
    // private string from = "0x6f759ba46a8a3337e5bd0bb5e615d5107b723249";
    private string privateKey;
    private string from;
    private Contract contract;
    private Contract faucet;

    public TextAsset contractABI;
    public TextAsset contractAddress;

    public GameUI gameUI;

    public Button copyButton;

    public GameController controller;

    [HideInInspector]
    public HexBigInteger ethBalance;
    public BigInteger seed;

    private HexBigInteger gas = new HexBigInteger(900000);
    private HexBigInteger betValue = new HexBigInteger(Web3.Convert.ToWei(1));

    private Function checkPlaying;
    private Function playFunction;
    private Function forceEndFunction;
    private Function winGameFunction;
    private Function loseGameFunction;

    void Start()
    {
        GameManager.Instance.AddListener(EVENT_TYPE.PLAY, this);
        copyButton.onClick.AddListener(CopyAddress);
        AccountSetup();
        GetContract();
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

    async void AccountSetup()
    {
        // var url = "http://localhost:8545";
        var url = "https://testnet.tomochain.com";

        privateKey = PlayerPrefs.GetString("privateKey");
        if (privateKey == "")
        {
            var ecKey = EthECKey.GenerateKey();
            privateKey = ecKey.GetPrivateKey();
            PlayerPrefs.SetString("privateKey", privateKey);
        }
        account = new Account(privateKey);
        from = account.Address;
        web3 = new Web3(account, url);
        if (PlayerPrefs.GetString("NickName") == "") PlayerPrefs.SetString("NickName", from);
        controller.SetNickName(from);
        gameUI.SetAccount(from);
        // await SetBalance();
        StartCoroutine(BalanceInterval());
    }

    IEnumerator BalanceInterval()
    {
        while (true)
        {
            SetBalance();
            yield return new WaitForSeconds(3f);
        }

    }

    public async Task SetBalance()
    {
        ethBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
        decimal ethBalanceVal = Web3.Convert.FromWei(ethBalance.Value);
        gameUI.SetBalance(string.Format("{0:0.00} Tomo", ethBalanceVal));
        if (ethBalanceVal > 1)
        {
            gameUI.EnablePlay();
        }
        else
        {
            gameUI.InsufficientBalance();
            gameUI.DisablePlay();
        }

    }

    void GetContract()
    {
        string abi = contractABI.ToString();
        string address = contractAddress.ToString();
        contract = web3.Eth.GetContract(abi, address);
        // GameManager.Instance.PostNotification(EVENT_TYPE.ACCOUNT_READY);
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
        gameUI.DisableLoading();
        gameUI.PlayConfirmed();
        // Debug.Log(string.Format("Play tx: {0}", tx));
        return receipt.TransactionHash;
    }

    public async Task<string> WinGame()
    {
        gameUI.EnableLoading();
        var receipt = await winGameFunction.SendTransactionAndWaitForReceiptAsync(from, gas, null);
        gameUI.DisableLoading();
        Debug.Log(string.Format("WinGame tx: {0}", receipt.TransactionHash));
        return receipt.TransactionHash;
    }

    public async Task<string> LoseGame()
    {
        gameUI.EnableLoading();
        var receipt = await loseGameFunction.SendTransactionAndWaitForReceiptAsync(from, gas, null);
        gameUI.DisableLoading();
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
        gameUI.EnableLoading();
        var receipt = await forceEndFunction.SendTransactionAndWaitForReceiptAsync(from, gas, null);
        gameUI.DisableLoading();
        Debug.Log(string.Format("ForceEndGame tx: {0}", receipt.TransactionHash));
        return receipt.TransactionHash;
    }


    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.PLAY:
                // seed = (new HexBigInteger(await Play())).Value;
                // Debug.Log("seed " + seed.ToString());
                break;
            case EVENT_TYPE.BLACK_FINISH:
                break;
            default:
                break;
        }
    }

    private async void OnApplicationQuit()
    {
        await ForceEndGame();
    }
}
