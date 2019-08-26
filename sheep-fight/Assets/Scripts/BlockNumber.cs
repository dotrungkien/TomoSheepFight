using System.Threading.Tasks;
using System.Collections;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.RPC.Eth.Blocks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BlockNumber : MonoBehaviour
{
    public Text blockNumberText;

    void Start()
    {
        StartCoroutine(CheckBlockNumber());
    }

    public IEnumerator CheckBlockNumber()
    {
        var wait = 1;
        while (true)
        {
            yield return new WaitForSeconds(wait);
            var blockNumberRequest = new EthBlockNumberUnityRequest("https://testnet.tomochain.com");
            yield return blockNumberRequest.SendRequest();
            if (blockNumberRequest.Exception == null)
            {
                var blockNumber = blockNumberRequest.Result.Value;
                blockNumberText.text = "Block: " + blockNumber.ToString();
            }
        }
    }
}