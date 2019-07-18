using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Signer;

public class ERC20 : MonoBehaviour
{
    private Account account;
    private Web3 web3;

    public TextAsset contractABI;
    public TextAsset contractAddress;

    void Start()
    {
        // var url = "http://localhost:8545";
        var url = "https://testnet.tomochain.com";
        var ecKey = EthECKey.GenerateKey();
        // var privateKey = ecKey.GetPrivateKey();
        var privateKey = "0xF557B67ED7DA128F0B3920072A041C93FC9FB5BCDEA16F73F03D6BB340C3D34A";
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


        var transferFunction = contract.GetFunction("transfer");
        var balanceFunction = contract.GetFunction("balanceOf");
        string from = "0x959eCA49C3f83f20F2873F175bD22F19e971Ff8B";
        var ethBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
        Debug.Log(string.Format("ETH balance {0}", Web3.Convert.FromWei(ethBalance.Value)));
        BigInteger balanceBefore = await balanceFunction.CallAsync<BigInteger>(from);
        Debug.Log(string.Format("Token balance before transfer {0}", balanceBefore.ToString()));
        // string to = "0xa64d6ac040648d74e9be21a51d495a06b5bf57fe";
        // var gas = await transferFunction.EstimateGasAsync(from, null, null, to, 999);
        // var receiptFirstAmountSend = await transferFunction.SendTransactionAndWaitForReceiptAsync(from, gas, null, null, to, 999);
        // BigInteger balanceAfter = await balanceFunction.CallAsync<BigInteger>(from);
        // Debug.Log(string.Format("balance before transfer {0}", balanceAfter.ToString()));
    }

    async void DeployContract()
    {
        var deploymentMessage = new StandardTokenDeployment
        {
            TotalSupply = 100000
        };
        var deploymentHandler = web3.Eth.GetContractDeploymentHandler<StandardTokenDeployment>();
        var transactionReceipt = await deploymentHandler.SendRequestAndWaitForReceiptAsync(deploymentMessage);
        var contractAddress = transactionReceipt.ContractAddress;
        Debug.Log(string.Format("deploy complete, address {0}", contractAddress));
    }
    // void BalanceOf(string address)
    // {
    //     var balanceOfFunctionMessage = new BalanceOfFunction()
    //     {
    //         Owner = account.Address,
    //     };

    //     var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
    //     var balance = await balanceHandler.QueryAsync<BigInteger>(contractAddress, balanceOfFunctionMessage);
    // }
}

public class StandardTokenDeployment : ContractDeploymentMessage
{

    public static string BYTECODE = "";

    public StandardTokenDeployment() : base(BYTECODE) { }

    [Parameter("uint256", "totalSupply")]
    public BigInteger TotalSupply { get; set; }
}

[Function("balanceOf", "uint256")]
public class BalanceOfFunction : FunctionMessage
{
    [Parameter("address", "_owner", 1)]
    public string Owner { get; set; }
}

[Function("transfer", "bool")]
public class TransferFunction : FunctionMessage
{
    [Parameter("address", "_to", 1)]
    public string To { get; set; }

    [Parameter("uint256", "_value", 2)]
    public BigInteger TokenAmount { get; set; }
}

[Event("Transfer")]
public class TransferEventDTO : IEventDTO
{
    [Parameter("address", "_from", 1, true)]
    public string From { get; set; }

    [Parameter("address", "_to", 2, true)]
    public string To { get; set; }

    [Parameter("uint256", "_value", 3, false)]
    public BigInteger Value { get; set; }
}
