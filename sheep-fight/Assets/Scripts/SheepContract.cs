using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Signer;

public class SheepContract : MonoBehaviour
{
    private Account account;
    private Web3 web3;
    private string privateKey = "d18a9a98695fd5976df7e5fceb1c25cccba76a89b0ff72015968350faa5bfac5"; //truffle develop acc 0
    private string from = "0xcbec9a701072198291dd0b78b1163068b8b22dfe";

    public TextAsset contractABI;
    public TextAsset contractAddress;

    void Start()
    {
        var url = "http://localhost:8545";
        var ecKey = EthECKey.GenerateKey();
        // var privateKey = ecKey.GetPrivateKey();
        // var privateKey = "0xF557B67ED7DA128F0B3920072A041C93FC9FB5BCDEA16F73F03D6BB340C3D34A"; //tomo
        account = new Account(privateKey);
        web3 = new Web3(account, url);
        Debug.Log(string.Format("account {0}", account.Address));
        GetContract();
    }

    async void GetContract()
    {
        string abi = contractABI.ToString();
        string address = contractAddress.ToString();
        var contract = web3.Eth.GetContract(abi, address);

        var checkPlaying = contract.GetFunction("isPlaying");
        // var playFunction = contract.GetFunction("play");
        // var submitFunction = contract.GetFunction("submit");
        var ethBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
        Debug.Log(string.Format("ETH balance {0}", Web3.Convert.FromWei(ethBalance.Value)));

        var isPlaying = await checkPlaying.CallAsync<bool>();
        Debug.Log(string.Format("Is Playing: {0}", isPlaying));

        // var gas = await playFunction.EstimateGasAsync();
        // var receipt = await playFunction.SendTransactionAndWaitForReceiptAsync(from);
        // Debug.Log(string.Format("play res: {0}", receipt));

        // var isPlayingAfter = await checkPlaying.CallAsync<bool>();
        // Debug.Log(string.Format("Is Playing After: {0}", isPlayingAfter));
    }
}

[Function("isPlaying", "bool")]
public class IsPlayingFunction : FunctionMessage { }

[Function("play", "bytes32")]
public class PlayFunction : FunctionMessage { }