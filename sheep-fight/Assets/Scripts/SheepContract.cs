using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

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
    // private string privateKey = "0xd18a9a98695fd5976df7e5fceb1c25cccba76a89b0ff72015968350faa5bfac5"; //truffle develop acc 0
    // private string from = "0xcbec9a701072198291dd0b78b1163068b8b22dfe";
    // seed = "exit lens suggest bamboo sniff head sentence focus burger fever prefer benefit";
    private string privateKey = "0x6e42b17dc5d278edfef336250b0e813f9c61f7fdc8de2ef65f05da1b1014f0b9"; //ganache-cli acc 0
    private string from = "0x6f759ba46a8a3337e5bd0bb5e615d5107b723249";
    private Contract contract;

    public TextAsset contractABI;
    public TextAsset contractAddress;
    public GameUI gameUI;

    [HideInInspector]
    public HexBigInteger ethBalance;
    public BigInteger seed;

    void Start()
    {

        // var privateKey = ecKey.GetPrivateKey();
        // var privateKey = "0xF557B67ED7DA128F0B3920072A041C93FC9FB5BCDEA16F73F03D6BB340C3D34A"; //tomo

        GameManager.GetInstance().AddListener(EVENT_TYPE.PLAY, this);
        AccountSetup();
        GetContract();
    }

    async void AccountSetup()
    {
        var url = "http://localhost:8545";
        var ecKey = EthECKey.GenerateKey();
        account = new Account(privateKey);
        web3 = new Web3(account, url);
        Debug.Log(string.Format("account {0}", account.Address));
        ethBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
        Debug.Log(string.Format("ETH balance {0}", Web3.Convert.FromWei(ethBalance.Value)));
        gameUI.SetAccount(account.Address);
        gameUI.SetBalance(string.Format("{0:0.00} ETH", Web3.Convert.FromWei(ethBalance.Value)));
    }

    async void GetContract()
    {
        string abi = contractABI.ToString();
        string address = contractAddress.ToString();
        contract = web3.Eth.GetContract(abi, address);

        bool isPlaying = await CheckPlaying();
        // Debug.Log(string.Format("Is Playing Before: {0}", isPlaying));
        // await EndGame(false);
        // if (!isPlaying)
        // {
        //     await Play();
        // }
        if (isPlaying)
        {
            await EndGame(false);
        }
        // Debug.Log(string.Format("Is Playing After: {0}", await CheckPlaying()));
    }

    public async Task<bool> CheckPlaying()
    {
        var checkPlaying = contract.GetFunction("isPlaying");
        var isPlaying = await checkPlaying.CallAsync<bool>(from);
        return isPlaying;
    }

    public async Task<string> Play()
    {
        var playFunction = contract.GetFunction("play");
        // var gas = await playFunction.EstimateGasAsync(from, new HexBigInteger(900000), new HexBigInteger(Web3.Convert.ToWei(1)));
        var tx = await playFunction.SendTransactionAsync(from, new HexBigInteger(900000), new HexBigInteger(Web3.Convert.ToWei(1)));
        // Debug.Log(string.Format("Play tx: {0}", tx));
        return tx;
    }

    async Task<string> EndGame(bool isWon)
    {
        var endgameFunction = contract.GetFunction("endGame");
        var gas = await endgameFunction.EstimateGasAsync(isWon);
        var tx = await endgameFunction.SendTransactionAsync(from, new HexBigInteger(900000), null, null, isWon);
        // Debug.Log(string.Format("EndGame tx: {0}", tx));
        return tx;
    }

    public async void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.PLAY:
                seed = (new HexBigInteger(await Play())).Value;
                Debug.Log("seed " + seed.ToString());
                break;
            case EVENT_TYPE.BLACK_FINISH:
                break;
            default:
                break;
        }
    }
}
