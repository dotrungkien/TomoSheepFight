const fs = require('fs');
const path = require('path');
const contracts = path.resolve(__dirname, '../build/contracts/');
const unityAbis = path.resolve(__dirname, '../../sheep-fight/Assets/Contracts/');

module.exports = function(deployer) {
  // let builtContracts = fs.readdirSync(contracts);
  // builtContracts.forEach((contract) => {
  //   if (contract === 'Migrations.json' || contract === 'SafeMath.json') return;
  //   console.log('extracting contract ', contract);
  //   const name = contract.split('.')[0];
  //   let json = JSON.parse(fs.readFileSync(path.resolve(contracts, contract)));
  //   let { abi, networks } = json;
  //   if (!Object.keys(networks).length) return;
  //   fs.writeFileSync(path.resolve(unityAbis, contract), JSON.stringify(json.abi));
  //   // fs.writeFileSync(path.resolve(unityAbis, name + 'Address.txt'), networks['89'].address); // tomo
  //   // fs.writeFileSync(path.resolve(unityAbis, name + 'Address.txt'), networks['4447'].address); // truffle develop
  //   fs.writeFileSync(path.resolve(unityAbis, name + 'Address.txt'), networks['123456'].address); // ganache-cli
  // });
};