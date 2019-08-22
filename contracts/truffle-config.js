var HDWalletProvider = require('truffle-hdwallet-provider');
var mnemonic = 'glimpse slender shed loan fossil title robot merge swing powder squirrel foot';

module.exports = {
  networks: {
    development: {
      host: '127.0.0.1',
      port: 8545,
      network_id: '123456'
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
