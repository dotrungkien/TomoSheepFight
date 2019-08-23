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

    void Start()
    {
        GameManager.Instance.AddListener(EVENT_TYPE.PLAY, this);
        copyButton.onClick.AddListener(CopyAddress);
        AccountSetup();
        GetContract();
    }

    void AccountSetup()
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
        SetBalance();
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

    async void SetBalance()
    {
        ethBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
        gameUI.SetBalance(string.Format("{0:0.00} Tomo", Web3.Convert.FromWei(ethBalance.Value)));
    }

    async void GetContract()
    {
        string abi = contractABI.ToString();
        string address = contractAddress.ToString();
        contract = web3.Eth.GetContract(abi, address);

        bool isPlaying = await CheckPlaying();
        if (isPlaying)
        {
            await ForceEndGame();
        }
        GameManager.Instance.PostNotification(EVENT_TYPE.ACCOUNT_READY);
    }

    public async Task<bool> CheckPlaying()
    {
        var checkPlaying = contract.GetFunction("isPlaying");
        var isPlaying = await checkPlaying.CallAsync<bool>(from);
        return isPlaying;
    }

    public async Task<string> Play(string gameID)
    {
        var playFunction = contract.GetFunction("play");
        var tx = await playFunction.SendTransactionAsync(from, gas, betValue, gameID);
        Debug.Log(string.Format("Play tx: {0}", tx));
        return tx;
    }

    public async Task<string> WinGame()
    {
        var endgameFunction = contract.GetFunction("winGame");
        var tx = await endgameFunction.SendTransactionAsync(from, gas, null);
        Debug.Log(string.Format("WinGame tx: {0}", tx));
        return tx;
    }

    public async Task<string> ForceEndGame()
    {
        var forceEndFunc = contract.GetFunction("forceEndGame");
        var gas = await forceEndFunc.EstimateGasAsync();
        var tx = await forceEndFunc.SendTransactionAsync(from, gas, null);
        Debug.Log(string.Format("ForceEndGame tx: {0}", tx));
        return tx;
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
}
