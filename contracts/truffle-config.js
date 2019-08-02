var HDWalletProvider = require('truffle-hdwallet-provider');
var mnemonic = 'ahihi';

module.exports = {
  networks: {
    development: {
      host: '127.0.0.1',
      port: 8545,
      network_id: '1564718941781'
    },
    tomo: {
      provider: () => new HDWalletProvider(mnemonic, 'https://testnet.tomochain.com', 0, 1, true),
      network_id: '89',
      gas: 3000000,
      gasPrice: 20000000000000,
      gasLimit: 1000000
    }
  }
  // compilers: {
  //   solc: {
  //     version: '0.5.0'
  //   }
  // }
};
